using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ARSoft.Tools.Net.Dns;
using TorDNSd.Logging;
using TorDNSd.Utils;

namespace TorDNSd
{
    public sealed class TorDNS
    {
        private DnsServer _dnsServer;
        private DnsSocksClient _dnsSocksClient;
        private DnsClient _dnsClient;

        public readonly ConfigCollection Configuration = new ConfigCollection();

        #region Configuration Wrappers
        public IPAddress ServerBindIP
        {
            get { return Configuration.GetValue("server-bindip", "127.0.0.1").GetValue(IPAddress.Parse("127.0.0.1")); }
        }

        public bool ServerEnabled
        {
            get { return Configuration.GetValue("server-enabled", "false").GetValue(false); }
        }

        public bool SocksEnabled
        {
            get { return Configuration.GetValue("socks-enabled", "false").GetValue(true); }
        }

        public IPAddress SocksIP
        {
            get { return Configuration.GetValue("server-bindip", "127.0.0.1").GetValue(IPAddress.Parse("127.0.0.1")); }
        }

        public short SocksPort
        {
            get { return Configuration.GetValue("socks-port", "9050").GetValue((short)9050); }
        }

        public int DnsProxyTimeout
        {
            get { return Configuration.GetValue("dns-proxy-timeout", "10000").GetValue(10000); }
        }

        public int DnsDirectTimeout
        {
            get { return Configuration.GetValue("dns-direct-timeout", "10000").GetValue(10000); }
        }

        public List<IPAddress> DnsProxy
        {
            get
            {
                var result =
                    Configuration.GetValues("dns-proxy").Where(p => p.GetValue((IPAddress)null) != null).Select(
                        p => p.GetValue((IPAddress)null)).ToList();

                if (result.Count == 0)
                {
                    // Add defaults
                    result.Add(IPAddress.Parse("8.8.8.8")); // Primary Public Google DNS
                    result.Add(IPAddress.Parse("8.8.4.4")); // Secondary Public Google DNS
                }

                return result;
            }
        }

        public List<IPAddress> DnsDirect
        {
            get
            {
                var result =
                    Configuration.GetValues("dns-direct").Where(p => p.GetValue((IPAddress)null) != null).Select(
                        p => p.GetValue((IPAddress)null)).ToList();

                if (result.Count == 0)
                {
                    // Add defaults
                    result.Add(IPAddress.Parse("8.8.8.8")); // Primary Public Google DNS
                    result.Add(IPAddress.Parse("8.8.4.4")); // Secondary Public Google DNS
                }

                return result;
            }
        }
        #endregion


        public bool IsRunning { get; private set; }

        public bool Start()
        {
            if (_dnsServer != null)
            {
                throw new Exception("The server is already running.");
            }

            if (!ServerEnabled)
            {
                // Server not enabled
                return false;
            }

            _dnsServer = new DnsServer(ServerBindIP, 32, 32, OnQuery);
            Logger.Log(LogSeverity.Info, "Starting the TorDNS server. Bind IP: {0}", ServerBindIP.ToString());

            Refresh(true);

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

        public void Refresh(bool skipServerCheck)
        {
            _dnsClient = new DnsClient(DnsDirect, DnsDirectTimeout);
            _dnsSocksClient = !SocksEnabled ? null : new DnsSocksClient(SocksIP, SocksPort, DnsProxy, DnsProxyTimeout);

            if (!skipServerCheck)
            {
                if (ServerEnabled && !IsRunning)
                {
                    Start();
                }

                if (!ServerEnabled && IsRunning)
                {
                    Stop();
                }
            }
        }

        public void Refresh()
        {
            Refresh(false);
        }

        private DnsMessageBase OnQuery(DnsMessageBase message, IPAddress clientaddress, ProtocolType protocoltype)
        {
            message.IsQuery = false;

            if (!ServerEnabled)
            {
                Logger.Log(LogSeverity.Warning, "Received a DNS request while the server is not enabled.");

                message.ReturnCode = ReturnCode.ServerFailure;
                return message;
            }

            DnsMessage query = message as DnsMessage;

            if (query != null && query.Questions.Count == 1)
            {
                // Get the question
                DnsQuestion question = query.Questions[0];

                // Apply all filters on the question
                FilterResult filterResult = ApplyFilters(question);

                Logger.Log(LogSeverity.Debug, "QUERY: {0} CLASS: {1} TYPE: {2} FILTER: {3}", question.Name, question.RecordClass.ToString(), question.RecordType.ToString(), filterResult.ToString());
                
                DnsMessage answer = null;

                switch (filterResult)
                {
                    case FilterResult.Proxy:
                        if (_dnsSocksClient == null)
                        {
                            // Socks not enabled
                            message.ReturnCode = ReturnCode.ServerFailure;
                            return message;
                        }

                        answer = _dnsSocksClient.Resolve(question.Name, question.RecordType, question.RecordClass);
                        break;

                    case FilterResult.SkipProxy:
                        if (_dnsClient == null)
                        {
                            // Socks not enabled
                            message.ReturnCode = ReturnCode.ServerFailure;
                            return message;
                        }

                        answer = _dnsClient.Resolve(question.Name, question.RecordType, question.RecordClass);

                        break;

                    case FilterResult.Reject:
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
                    return query;
                }
            }

            // Not a valid query or upstream server did not answer correct
            message.ReturnCode = ReturnCode.ServerFailure;
            return message;
        }

        private FilterResult ApplyFilters(DnsQuestion question)
        {
            // Default : Proxy
            FilterResult result = FilterResult.Proxy;
            
            foreach (var entry in Configuration.Where(c => c.Key.StartsWith("filter-", StringComparison.InvariantCultureIgnoreCase)))
            {
                switch(entry.Key.ToLower())
                {
                    case "filter-proxy":
                        if (Glob.Match(question.Name, entry.Value ))
                        {
                            result = FilterResult.Proxy;
                        }
                        break;

                    case "filter-reject":
                        if (Glob.Match(question.Name, entry.Value))
                        {
                            result = FilterResult.Reject;
                        }
                        break;

                    case "filter-skip-proxy":
                        if (Glob.Match(question.Name, entry.Value))
                        {
                            result = FilterResult.SkipProxy;
                        }
                        break;
                }
            }

            return result;
        }

        private enum FilterResult
        {
            Reject,
            Proxy,
            SkipProxy
        }
    }
}
