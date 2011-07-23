using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TorDNSd.Utils;

namespace TorDNSd
{
    /// <summary>
    /// TorDNS Configuration Collection
    /// </summary>
    public class TorDNSConfiguration : ConfigCollection
    {
        public TorDNSConfiguration()
        {

        }

        public TorDNSConfiguration(IList<ConfigEntry> list) 
            : base(list)
        {

        }

        public TorDNSConfiguration(IEnumerable<ConfigEntry> list) 
            : base(list)
        {

        }

        public TorDNSConfiguration(params string[] entries) 
            : base(entries)
        {

        }   
        
        #region Configuration Wrappers (return default value if not set)

        /// <summary>
        /// 'server-bindip' option.
        /// </summary>
        /// <remarks>Default: 127.0.0.1</remarks>
        public IPAddress ServerBindIP
        {
            get
            {
                return GetValue("server-bindip", "127.0.0.1").GetValue(IPAddress.Parse("127.0.0.1"));
            }
        }

        /// <summary>
        /// 'server-enabled' option.
        /// </summary>
        /// <remarks>Default: false</remarks>
        public bool ServerEnabled
        {
            get
            {
                return GetValue("server-enabled", "false").GetValue(false);
            }
        }

        /// <summary>
        /// 'socks-enabled' option.
        /// </summary>
        /// <remarks>Default: true</remarks>
        public bool SocksEnabled
        {
            get
            {
                return GetValue("socks-enabled", "false").GetValue(true);
            }
        }


        /// <summary>
        /// 'socks-ip' option.
        /// </summary>
        /// <remarks>Default: 127.0.0.1</remarks>
        public IPAddress SocksIP
        {
            get
            {
                return GetValue("server-bindip", "127.0.0.1").GetValue(IPAddress.Parse("127.0.0.1"));
            }
        }

        /// <summary>
        /// 'socks-port' option.
        /// </summary>
        /// <remarks>Default: 9050</remarks>
        public short SocksPort
        {
            get
            {
                return GetValue("socks-port", "9050").GetValue((short)9050);
            }
        }

        /// <summary>
        /// 'dns-proxy-timeout' option.
        /// </summary>
        /// <remarks>Default: 10000</remarks>
        public int DnsProxyTimeout
        {
            get
            {
                return GetValue("dns-proxy-timeout", "10000").GetValue(10000);
            }
        }

        /// <summary>
        /// 'dns-direct-timeout' option.
        /// </summary>
        /// <remarks>Default: 10000</remarks>
        public int DnsDirectTimeout
        {
            get
            {
                return GetValue("dns-direct-timeout", "10000").GetValue(10000);
            }
        }

        /// <summary>
        /// 'dns-proxy' option.
        /// </summary>
        /// <remarks>Default: 8.8.8.8 and 8.8.4.4</remarks>
        public IPAddress[] DnsProxy
        {
            get
            {
                var result = GetValues("dns-proxy").Where(p => p.GetValue((IPAddress)null) != null).Select(
                        p => p.GetValue((IPAddress)null)).ToList();

                if (result.Count == 0)
                {
                    // Add defaults
                    result.Add(IPAddress.Parse("8.8.8.8")); // Primary Public Google DNS
                    result.Add(IPAddress.Parse("8.8.4.4")); // Secondary Public Google DNS
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// 'dns-direct' option.
        /// </summary>
        /// <remarks>Default: 8.8.8.8 and 8.8.4.4</remarks>
        public IPAddress[] DnsDirect
        {
            get
            {
                var result = GetValues("dns-direct").Where(p => p.GetValue((IPAddress)null) != null).Select(
                        p => p.GetValue((IPAddress)null)).ToList();

                if (result.Count == 0)
                {
                    // Add defaults
                    result.Add(IPAddress.Parse("8.8.8.8")); // Primary Public Google DNS
                    result.Add(IPAddress.Parse("8.8.4.4")); // Secondary Public Google DNS
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// 'dns-cache-enabled' option.
        /// </summary>
        /// <remarks>Default: true</remarks>
        public bool DnsCacheEnabled
        {
            get
            {
                return GetValue("dns-cache-enabled", "true").GetValue(true);
            }
        }

        /// <summary>
        /// 'dns-cache-ttl' option.
        /// </summary>
        /// <remarks>Default: 3600</remarks>
        public int DnsCacheTtl
        {
            get
            {
                return GetValue("dns-cache-ttl", "3600").GetValue(3600);
            }
        }

        /// <summary>
        /// 'dns-cache-size' option.
        /// </summary>
        /// <remarks>Default: 1000</remarks>
        public int DnsCacheSize
        {
            get
            {
                return GetValue("dns-cache-ttl", "1000").GetValue(1000);
            }
        }

        /// <summary>
        /// 'filter-proxy' option.
        /// </summary>
        /// <remarks>Default: *</remarks>
        public string[] FilterProxy
        {
            get
            {
                var result = GetValues("filter-proxy").Where(p => p.GetValue((string)null) != null).Select(p => p.GetValue((string)null)).ToList();

                if (result.Count == 0)
                {
                    result.Add("*");
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// 'filter-direct' option.
        /// </summary>
        public string[] FilterDirect
        {
            get
            {
                return GetValues("filter-direct").Where(p => p.GetValue((string)null) != null).Select(p => p.GetValue((string)null)).ToArray();
            }
        }

        /// <summary>
        /// 'filter-reject' option.
        /// </summary>
        /// <remarks>Default: *.lan_dn, *.onion and localhost</remarks>
        public string[] FilterReject
        {
            get
            {
                var result = GetValues("filter-reject").Where(p => p.GetValue((string)null) != null).Select(p => p.GetValue((string)null)).ToList();

                if (result.Count == 0)
                {
                    result.Add("*.lan_dn");
                    result.Add("*.onion");
                    result.Add("localhost");
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// 'remap-ttl' option.
        /// </summary>
        /// <remarks>Default: 3600</remarks>
        public int RemapTtl
        {
            get
            {
                return GetValue("remap-ttl", "3600").GetValue(3600);
            }
        }
        #endregion

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>New instance containing all items of the original instance.</returns>
        public TorDNSConfiguration Clone()
        {
            return new TorDNSConfiguration(Items);
        }

        /// <summary>
        /// Optimize the configuration collection.
        /// </summary>
        public void Optimize()
        {
            // Optimize the single options, just leaving the last-set option.
            foreach (string singleOption in Options.Single.Select(s => s.Key))
            {
                ConfigEntry lastEntry = Items.LastOrDefault(i => i.Key.Equals(singleOption, StringComparison.InvariantCultureIgnoreCase));

                // Items contains at least one entry for this option, make sure it is the only one
                if (lastEntry != null)
                {
                    // Remove older duplicates
                    foreach (ConfigEntry duplicateEntry in Items.Where(i => i.Key.Equals(singleOption, StringComparison.InvariantCultureIgnoreCase) && i != lastEntry ))
                    {
                        Items.Remove(duplicateEntry);
                    }
                }
            }
        }
    }
}
