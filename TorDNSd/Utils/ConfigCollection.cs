using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace TorDNSd.Utils
{
    /// <summary>
    /// Collection of ConfigEntry entries, able to append / load additional entries.
    /// </summary>
    public class ConfigCollection : Collection<ConfigEntry>
    {
        /// <summary>
        /// Appends of key/value pair strings that are prefixed with this will be ignored.
        /// </summary>
        public const string COMMENT_PREFIX = "#";

        public ConfigCollection()
        {

        }

        public ConfigCollection(IList<ConfigEntry> list)
            : base(list)
        {

        }

        public ConfigCollection(IEnumerable<ConfigEntry> list)
            : base(list.ToList())
        {

        }
        public ConfigCollection(params string[] entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            Array.ForEach(entries.ToArray(), i => Append(i));
        }

        /// <summary>
        /// Attempt to read the list of entries from the specified file. Throws on failure to read the file.
        /// </summary>
        /// <param name="file">Path to the configuration file. All lines </param>
        /// <returns>ConfigCollection instance containing the key/value pairs read from the file.</returns>
        public static ConfigCollection FromFile(string file)
        {
            string[] lines = File.ReadAllLines(file);

            if (lines.Length == 0)
            {
                return new ConfigCollection();
            }

            return new ConfigCollection(lines);
        }

        /// <summary>
        /// Append the specified entries to this list.
        /// </summary>
        /// <param name="entries">The list of ConfigEntry instances. Can be null or empty.</param>
        /// <returns>The appended entries.</returns>
        public ConfigEntry[] Append(params ConfigEntry[] entries)
        {
            if (entries == null || entries.Length == 0)
            {
                // Nothing to add
                return new ConfigEntry[0];
            }

            // Get the list of valid entries (not null)
            ConfigEntry[] validEntries = entries.Where(i => i != null).ToArray();

            // Add every entry to this collection
            Array.ForEach(validEntries, Add);

            // Return the list of valid entries
            return validEntries;
        }

        /// <summary>
        /// Append the items of the specified collection to this one.
        /// </summary>
        /// <param name="collection">Collection to add from. Cannot be null. Null-entries will be ignored.</param>
        /// <returns>The append</returns>
        public ConfigEntry[] Append(Collection<ConfigEntry> collection)
        {
            // Collection cannot be null
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            // Get the list of valid entries (not null)
            ConfigEntry[] validEntries = collection.Where(i => i != null).ToArray();

            // Add every entry to this collection
            Array.ForEach(validEntries, Add);

            // Return the list of valid entries
            return validEntries;
        }

        /// <summary>
        /// Parses the value in a key/value pair and adds the entry if it is a valid entry.
        /// </summary>
        /// <param name="value">The key/value pair to parse.</param>
        /// <returns>The added ConfigEntry instance on success, null otherwise.</returns>
        public ConfigEntry Append(string value)
        {
            // Empty value or value starting with COMMENT_PREFIX
            if (string.IsNullOrEmpty(value) || (value = value.TrimStart()).StartsWith(COMMENT_PREFIX))
            {
                return null;
            }

            ConfigEntry entry = ConfigEntry.Parse(value);

            if (entry == null)
            {
                return null;
            }

            Add(entry);

            return entry;
        }

        public ConfigEntry[] GetValues(string key)
        {
            return GetValues(key, false);
        }


        public ConfigEntry[] GetValues(string key, bool caseSensitive)
        {
            return
                   Items.Where(i => i.Key.Equals(key, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)).ToArray();        
        }


        public ConfigEntry GetValue(string key)
        {
            return GetValue(key, false);
        }

        public ConfigEntry GetValue(string key, string defaultValue)
        {
            return GetValue(key, false) ?? new ConfigEntry(key, defaultValue);
        }

        public ConfigEntry GetValue(string key, bool caseSensitive)
        {
            return GetValues(key, caseSensitive).LastOrDefault();
        }

        public ConfigEntry[] this[string key, bool caseSensitive]
        {
            get { return GetValues(key, caseSensitive); }
        }

        public ConfigEntry[] this[string key]
        {
            get { return GetValues(key, false); }
        }
    }
}
