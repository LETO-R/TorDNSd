using ARSoft.Tools.Net.Dns;

namespace TorDNSd.Cache.Extensions
{
    /// <summary>
    /// DnsQuestion extension methods.
    /// </summary>
    public static class DnsQuestionEx
    {
        /// <summary>
        /// Compares source with target to see if it matches.
        /// </summary>
        public static bool IsEqualTo(this DnsQuestion source, DnsQuestion target)
        {
            if (source == target)
            {
                return true;
            }

            if (source == null || target == null)
            {
                return false;
            }

            return source.Name == target.Name && source.RecordClass == target.RecordClass &&
                   source.RecordType == target.RecordType;
        }
    }
}
