using System;
using ARSoft.Tools.Net.Dns;

namespace TorDNSd.Cache
{
    /// <summary>
    /// Contains a question and the cached records and the UTC-time when the cached query was last hit.
    /// </summary>
    public sealed class CacheEntry
    {
        /// <summary>
        /// Used while hitting.
        /// </summary>
        private readonly object _instanceLock = new object();

        /// <summary>
        /// DNS query question
        /// </summary>
        public readonly DnsQuestion Question;

        /// <summary>
        /// Cached replies.
        /// </summary>
        public readonly DnsRecordBase[] Records;

        /// <summary>
        /// UTC time when entry was created.
        /// </summary>
        public DateTime Since;

        /// <summary>
        /// UTC time when the cache was last hit.
        /// </summary>
        public DateTime LastHit;

        /// <summary>
        /// Amount of times hit.
        /// </summary>
        public int Hits { get; private set; }

        /// <summary>
        /// Create a new cache entry.
        /// </summary>
        public CacheEntry(DnsQuestion question, DnsRecordBase[] records, int ttl)
        {
            Since = DateTime.UtcNow;
            Question = question;

            foreach (var record in records)
            {
                // Override the TTL
                record.TimeToLive = ttl > 0 ? ttl : int.MaxValue;
            }

            Records = records;
        }

        /// <summary>
        /// Hit the cache item, updating the LastHit time and count.
        /// </summary>
        public void Hit()
        {
            lock (_instanceLock)
            {
                LastHit = DateTime.UtcNow;

                Hits++;
            }
        }
    }
}
