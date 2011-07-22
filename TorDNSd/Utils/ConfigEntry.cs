using System;
using System.Net;

namespace TorDNSd.Utils
{
    /// <summary>
    /// Represents a single configuration entry.
    /// </summary>
    public class ConfigEntry
    {
        /// <summary>
        /// The key/value seperator.
        /// </summary>
        public const string KEY_VALUE_SEPERATOR = "=";

        /// <summary>
        /// Configuration key
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// Configuration value
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// Create a new config 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public ConfigEntry(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || key.Trim() == string.Empty)
            {
                throw new ArgumentException("Key cannot be null or empty after trim.", "key");
            }

            Key = key;
            Value = value ?? string.Empty;
        }

        public ConfigEntry(string key)
            : this(key, string.Empty)
        {

        }

        /// <summary>
        /// Attempts to parse the specified value into a ConfigEntry instance.
        /// <para><remarks>Format: key=value</remarks></para>
        /// <para>The key and value will both be trimmed.</para>
        /// </summary>
        /// <returns>A ConfigEntry instance on success, null otherwise.</returns>
        public static ConfigEntry Parse(string value)
        {
            return Parse(value, true);
        }

        /// <summary>
        /// Attempts to parse the specified value into a ConfigEntry instance.
        /// <para><remarks>Format: key=value</remarks></para>
        /// <para>The key will be trimmed. The value will be trimmed when trimValue is true.</para>
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="trimValue">When true, the value will be trimmed.</param>
        /// <returns>A ConfigEntry instance on success, null otherwise.</returns>
        public static ConfigEntry Parse(string value, bool trimValue)
        {
            if (string.IsNullOrEmpty(value) || !value.Contains(KEY_VALUE_SEPERATOR))
            {
                return null;
            }

            string[] parts = value.Split(new string[] { KEY_VALUE_SEPERATOR }, 2, StringSplitOptions.None);

            // The key cannot be empty
            if (parts[0] == string.Empty)
            {
                return null;
            }

            // When only 1 part no value was specified, create an entry with no value set
            return parts.Length == 1 ? new ConfigEntry(parts[0]) : new ConfigEntry(parts[0], trimValue ? parts[1].Trim() : parts[1]);
        }

        public int GetValue(int defaultValue)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return defaultValue;
            }

            int.TryParse(Value, out defaultValue);

            return defaultValue;
        }

        public string GetValue(string defaultValue)
        {
            return string.IsNullOrEmpty(Value) ? defaultValue : Value;
        }

        public long GetValue(long defaultValue)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return defaultValue;
            }

            long.TryParse(Value, out defaultValue);

            return defaultValue;
        }

        public bool GetValue(bool defaultValue)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return defaultValue;
            }

            bool.TryParse(Value, out defaultValue);

            return defaultValue;
        }

        public IPAddress GetValue(IPAddress defaultValue)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return defaultValue;
            }

            IPAddress.TryParse(Value, out defaultValue);

            return defaultValue;
        }

        public short GetValue(short defaultValue)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return defaultValue;
            }

            short.TryParse(Value, out defaultValue);

            return defaultValue;
        }
    }
}
