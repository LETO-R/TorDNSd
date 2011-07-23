using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using ARSoft.Tools.Net.Dns;
using TorDNSd.Cache;
using TorDNSd.Cache.Extensions;
using TorDNSd.Logging;
using TorDNSd.Utils;

namespace TorDNSd
{
    /// <summary>
    /// TorDNS core.
    /// </summary>
    public sealed class TorDNS
    {
        private DnsServer _dnsServer;
        private DnsSocksClient _dnsSocksClient;
        private DnsClient _dnsClient;
        private List<CacheEntry> _dnsCache = new List<CacheEntry>();

        /// <summary>
        /// Current active configuration.
        /// </summary>
        public TorDNSConfiguration Configuration { get; private set; }

        /// <summary>
        /// Next configuration. Used when the configuration is reloaded (using Reload())
        /// </summary>
        public TorDNSConfiguration NextConfiguration { get; private set; }
        
        /// <summary>
        /// Is TorDNS running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Construct a new TorDNS instance.
        /// </summary>
        public TorDNS()
        {
            Configuration = new TorDNSConfiguration();
            NextConfiguration = new TorDNSConfiguration();
        }

        /// <summary>
        /// Attempt to start the server.
        /// </summary>
        /// <remarks>Will only start when 'start-enabled' is true.</remarks>
        /// <returns>True on success, false if the server is not enabled.</returns>
        public bool Start()
        {
            if (_dnsServer != null)
            {
                throw new Exception("The server is already running.");
            }

            if (!Configuration.ServerEnabled)
            {
                // Server not enabled
                return false;
            }

            _dnsServer = new DnsServer(Configuration.ServerBindIP, 32, 32, OnQuery);
            Logger.Log(LogSeverity.Info, "Starting the TorDNS server. Bind IP: {0}", Configuration.ServerBindIP.ToString());

            RefreshConfiguration(true);

            _dnsServer.Start();

            IsRunning = true;
            return true;
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            try
            {
                Logger.Log(LogSeverity.Info, "Stopping the TorDNS server.");
                _dnsServer.Stop();
            }
            finally
            {
                _dnsServer = null;
                IsRunning = false;
            }
        }

        public void RefreshConfiguration(bool skipServerCheck)
        {
            _dnsClient = new DnsClient(Configuration.DnsDirect.ToList(), Configuration.DnsDirectTimeout);
            _dnsSocksClient = !Configuration.SocksEnabled ? null : new DnsSocksClient(Configuration.SocksIP, Configuration.SocksPort, Configuration.DnsProxy.ToList(), Configuration.DnsProxyTimeout);

            if (!skipServerCheck)
            {
                if (Configuration.ServerEnabled && !IsRunning)
                {
                    Start();
                }

                if (!Configuration.ServerEnabled && IsRunning)
                {
                    Stop();
                }
            }
        }

        public void RefreshConfiguration()
        {
            RefreshConfiguration(false);
        }

        private DnsMessageBase OnQuery(DnsMessageBase message, IPAddress clientaddress, ProtocolType protocoltype)
        {
            message.IsQuery = false;

            if (!Configuration.ServerEnabled)
            {
                Logger.Log(LogSeverity.Warning, "Received a DNS request while the server is not enabled.");

                message.ReturnCode = ReturnCode.ServerFailure;
                return message;
            }

            var query = message as DnsMessage;

            if (query != null && query.Questions.Count == 1)
            {
                // Get the question
                DnsQuestion question = query.Questions[0];

                // Apply all filters on the question
                FilterAction filterResult = ApplyFilters(question);

                // Only check the cache when we do not need to auto-reject
                if (filterResult != FilterAction.Reject)
                {
                    DnsRecordBase[] remappedRecords = ApplyRemaps(question);

                    if (remappedRecords.Length > 0)
                    {
                        query.AnswerRecords.AddRange(remappedRecords);
                        query.ReturnCode = ReturnCode.NoError;

                        return query;
                    }

                    if (Configuration.DnsCacheEnabled)
                    {
                        lock (_dnsCache)
                        {
                            // Check the cache
                            CacheEntry cachedEntry = _dnsCache.FirstOrDefault(c => c.Question.IsEqualTo(question));

                            if (cachedEntry != null)
                            { 
                                // Cache hit!
                                if (Configuration.DnsCacheTtl > 0 && DateTime.UtcNow - cachedEntry.LastHit > new TimeSpan(0, 0, 0, Configuration.DnsCacheTtl))
                                {
                                    // Hit expired
                                    _dnsCache.Remove(cachedEntry);
                                }
                                else
                                { 
                                    // Hit did not expire, use it.
                                    cachedEntry.Hit();

                                    query.AnswerRecords.AddRange(cachedEntry.Records);
                                    query.ReturnCode = ReturnCode.NoError;

                                    return query;
                                }
                            }
                        }
                    }
                }

                Logger.Log(LogSeverity.Debug, "QUERY: {0} CLASS: {1} TYPE: {2} FILTER: {3}", question.Name, question.RecordClass.ToString(), question.RecordType.ToString(), filterResult.ToString());
                
                DnsMessage answer = null;

                switch (filterResult)
                {
                    case FilterAction.Proxy:
                        if (_dnsSocksClient == null)
                        {
                            // Socks not enabled
                            message.ReturnCode = ReturnCode.ServerFailure;
                            return message;
                        }

                        answer = _dnsSocksClient.Resolve(question.Name, question.RecordType, question.RecordClass);
                        break;

                    case FilterAction.SkipProxy:
                        if (_dnsClient == null)
                        {
                            // Socks not enabled
                            message.ReturnCode = ReturnCode.ServerFailure;
                            return message;
                        }

                        answer = _dnsClient.Resolve(question.Name, question.RecordType, question.RecordClass);

                        break;

                    case FilterAction.Reject:
                        message.ReturnCode = ReturnCode.ServerFailure;
                        return message;
                }

                // if got an answer, copy it to the message sent to the client
                if (answer != null)
                {
                    foreach (DnsRecordBase record in (answer.AnswerRecords))
                    {
                        query.AnswerRecords.Add(record);
                    }
                    foreach (DnsRecordBase record in (answer.AdditionalRecords))
                    {
                        query.AnswerRecords.Add(record);
                    }

                    query.ReturnCode = ReturnCode.NoError;

                    if (Configuration.DnsCacheEnabled)
                    {
                        lock (_dnsCache)
                        {
                            if (!_dnsCache.Any(c => c.Question.IsEqualTo(question)))
                            {
                                var cacheEntry = new CacheEntry(question, query.AnswerRecords.ToArray(), Configuration.DnsCacheTtl);
                                cacheEntry.Hit();
                                
                                _dnsCache.Add(cacheEntry);

                                // Check if the cache is full
                                if (Configuration.DnsCacheSize > 0 && _dnsCache.Count > Configuration.DnsCacheSize)
                                {
                                    // Remove the oldest entry
                                    _dnsCache.Remove(_dnsCache.OrderBy(c => c.LastHit).First());
                                }

                                Logger.Log(LogSeverity.Debug, "DNS reply cached.");
                            }
                        }
                    }

                    return query;
                }
            }

            // Not a valid query or upstream server did not answer correct
            message.ReturnCode = ReturnCode.ServerFailure;
            return message;
        }

        private DnsRecordBase[] ApplyRemaps(DnsQuestion question)
        {
            var records = new List<DnsRecordBase>();

            foreach (var entry in Configuration.Where(c => c.Key.Equals("remap", StringComparison.InvariantCultureIgnoreCase)))
            {
                // FORMAT: <filter> <class> <type> <value>
                string[] parts = entry.Value.Split(new[] {' '}, 4);

                if (parts.Length != 4)
                {
                    // Invalid rule
                    continue;
                }

                string remapFilter = parts[0];
                string remapClass = parts[1];
                string remapType = parts[2];
                string remapValue = parts[3];

                // See if we got a match
                if (Glob.Match(question.Name, remapFilter)
                    && (question.RecordClass == RecordClass.Any || (question.RecordClass.ToString().Equals(remapClass, StringComparison.InvariantCultureIgnoreCase)))
                    && (question.RecordType == RecordType.Any || (question.RecordType.ToString().Equals(remapType, StringComparison.InvariantCultureIgnoreCase))))
                {
                    IPAddress ipaddress = null;

                    switch (remapType.ToUpper())
                    {
                        case "A":
                            if (!IPAddress.TryParse(remapValue, out ipaddress))
                            {
                                // Invalid argument
                                continue;
                            }

                            records.Add(new ARecord(question.Name, Configuration.RemapTtl, ipaddress));
                            break;
                        case "MX":
                            ushort priority = 0;

                            if (remapValue.Contains(" "))
                            {
                                ushort.TryParse(remapValue.Split(' ')[0], out priority);
                            }

                            records.Add(new MxRecord(question.Name, Configuration.RemapTtl, priority, remapValue.Split(' ')[0]));
                            break;
                        case "NS":
                            records.Add(new NsRecord(question.Name, Configuration.RemapTtl, remapValue.Trim()));
                            break;
                    }
                }
            }

            return records.ToArray();
        }

        private FilterAction ApplyFilters(DnsQuestion question)
        {
            // Default : Proxy
            FilterAction result = FilterAction.Proxy;
            
            foreach (var entry in Configuration.Where(c => c.Key.StartsWith("filter-", StringComparison.InvariantCultureIgnoreCase)))
            {
                switch(entry.Key.ToLower())
                {
                    case "filter-proxy":
                        if (Glob.Match(question.Name, entry.Value ))
                        {
                            result = FilterAction.Proxy;
                        }
                        break;

                    case "filter-reject":
                        if (Glob.Match(question.Name, entry.Value))
                        {
                            result = FilterAction.Reject;
                        }
                        break;

                    case "filter-skip-proxy":
                        if (Glob.Match(question.Name, entry.Value))
                        {
                            result = FilterAction.SkipProxy;
                        }
                        break;
                }
            }

            return result;
        }

        private enum FilterAction
        {
            Reject,
            Proxy,
            SkipProxy
        }

        /// <summary>
        /// Clear the DNS cache.
        /// </summary>
        public void ClearCache()
        {
            lock (_dnsCache)
            {
                _dnsCache.Clear();
            }
        }
    }
}
