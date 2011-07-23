using System.Collections.Generic;

namespace TorDNSd
{
    /// <summary>
    /// Static class with the list of valid options + description.
    /// </summary>
    public static class Options
    {
        /// <summary>
        /// Single-value options.
        /// </summary>
        public static Dictionary<string, string> Single;

        /// <summary>
        /// Multi-value options.
        /// </summary>
        public static Dictionary<string, string> Multi;

        /// <summary>
        /// Initialize the static class.
        /// </summary>
        static Options()
        {
            Single = new Dictionary<string, string>();
            Multi = new Dictionary<string, string>();

            // Single-value options
            Single.Add("server-bind", "Local IP address to bind on.");
            Single.Add("server-enabled", "Enable (true) or disable (false) the dns server.");
            Single.Add("socks-ip", "IP address of the SOCKS4 proxy server to forward DNS requests through.");
            Single.Add("socks-port", "Port of the SOCKS proxy server.");
            Single.Add("socks-enabled", "Enable (true) or disable (false) forwarding DNS requests through the SOCKS proxy.");
            Single.Add("dns-direct-timeout", "Timeout for DNS requests that are forwarded directly (in milliseconds).");
            Single.Add("dns-proxy-timeout", "Timeout for DNS requests that are forwarded through the SOCKS proxy (in milliseconds).");
            Single.Add("dns-cache-ttl", "Amount of time in seconds to cache DNS query results.");
            Single.Add("dns-cache-size", "Maximum amount of entries to cache. Oldest entries will be removed if the cache is full.");
            Single.Add("dns-cache-enabled", "Enable (true) or disable (false) caching of DNS query results.");
            Single.Add("remap-ttl", "The TTL in seconds to set for all remaps.");

            // Multi-value options
            Multi.Add("dns-direct", "DNS server IP used to forward queries directly.");
            Multi.Add("dns-proxy", "DNS server IP used to forward queries through the SOCKS proxy.");
            Multi.Add("filter-proxy", "Matching queries will be forwarded through the SOCKS proxy.");
            Multi.Add("filter-direct", "Matching queries will be forwarded directly.");
            Multi.Add("filter-reject", "Matching queries will be rejected.");
            Multi.Add("remap", "Remap queries so that they return a preconfigured reply.");
        }
    }
}
