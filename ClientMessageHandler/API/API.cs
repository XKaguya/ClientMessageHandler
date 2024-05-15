using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using ClientMessageHandler.Entry;
using Microsoft.Win32;

namespace ClientMessageHandler.Generic
{
    public static class API
    {
        public static Dictionary<string, List<MessageEntry>> FileMessagesDict { get; set; } = new Dictionary<string, List<MessageEntry>>();

        public static void Serialize(string xmlString)
        {
            try
            {
                var messages = DeserializeXml(xmlString);
                foreach (var kvp in messages)
                {
                    if (!FileMessagesDict.ContainsKey(kvp.Key))
                    {
                        FileMessagesDict[kvp.Key] = new List<MessageEntry>();
                    }
                    FileMessagesDict[kvp.Key].AddRange(kvp.Value);
                }
            }
            catch (XmlException ex)
            {
                Logger.Error($"Error parsing XML: {ex.Message}");
            }

            if (FileMessagesDict.Count == 0)
            {
                Logger.Error("No messages found in the XML.");
            }
        }

        private static Dictionary<string, List<MessageEntry>> DeserializeXml(string xmlString)
        {
            var messages = new Dictionary<string, List<MessageEntry>>();

            using (StringReader stringReader = new StringReader(xmlString))
            using (XmlReader reader = XmlReader.Create(stringReader))
            {
                XPathDocument xPathDoc = new XPathDocument(reader);
                XPathNavigator navigator = xPathDoc.CreateNavigator();

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(navigator.NameTable);
                nsmgr.AddNamespace("ns", "http://datacontract.gib.me/startrekonline");

                foreach (XPathNavigator messageElement in navigator.Select("//ns:Message", nsmgr))
                {
                    string fileName = messageElement.SelectSingleNode("ns:FileName", nsmgr)?.Value;

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if (!messages.ContainsKey(fileName))
                        {
                            messages[fileName] = new List<MessageEntry>();
                        }

                        MessageEntry entry = new MessageEntry
                        {
                            MessageKey = messageElement.SelectSingleNode("ns:MessageKey", nsmgr)?.Value,
                            DefaultString = messageElement.SelectSingleNode("ns:DefaultString", nsmgr)?.Value ?? messageElement.SelectSingleNode("ns:Defaultstring", nsmgr)?.Value
                        };

                        messages[fileName].Add(entry);
                    }
                }
            }

            return messages;
        }

        private static async Task<Dictionary<string, List<MessageEntry>>> DeserializeXmlAsync(string xmlString)
        {
            return await Task.Run(() => DeserializeXml(xmlString)).ConfigureAwait(false);
        }

        public static async Task<Dictionary<string, List<MessageEntry>>> SerializeAsync(string xmlString, bool ifReturn = false)
        {
            try
            {
                var messages = await DeserializeXmlAsync(xmlString).ConfigureAwait(false);
                return messages;
            }
            catch (XmlException ex)
            {
                Logger.Error($"Error parsing XML: {ex.Message}");
                return new Dictionary<string, List<MessageEntry>>();
            }
        }

        public static string? LoadXmlFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files|*.xml|All Files|*.*",
                Title = "Select XML File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                DateTime load = DateTime.Now;
                string filePath = openFileDialog.FileName;

                try
                {
                    string xmlString = File.ReadAllText(filePath);
                    Serialize(xmlString);
                }
                catch (IOException ex)
                {
                    Logger.Error($"{nameof(IOException)}: {ex.Message}");
                }

                DateTime loaded = DateTime.Now;
                Logger.Log($"LoadXmlFromFile method takes {loaded - load} second. Loaded {FileMessagesDict.Count} items.");

                return filePath;
            }

            return null;
        }

        public static string? LoadXmlFileAlter()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files|*.xml|All Files|*.*",
                Title = "Select XML File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        public static async Task GetDifferenceAsync(List<string> filePaths)
        {
            var tempDictionaries = new List<Dictionary<string, List<MessageEntry>>>();

            Logger.Log($"Selected {filePaths.Count} to compare.");

            DateTime load = DateTime.Now;

            foreach (string file in filePaths)
            {
                try
                {
                    string fileContent = await File.ReadAllTextAsync(file).ConfigureAwait(false);
                    var tempDict = await SerializeAsync(fileContent, true).ConfigureAwait(false);
                    tempDictionaries.Add(tempDict);
                }
                catch (IOException ex)
                {
                    Logger.Error($"{nameof(IOException)}: {ex.Message}");
                }
                finally
                {
                    Logger.Log($"Completed serialize of file: {file}");
                }
            }

            DateTime loaded = DateTime.Now;
            Logger.Log($"GetDifference method takes {loaded - load} second to load {filePaths.Count} files.");
            Logger.Log("Now proceeding difference part.");

            var differentKeys = await GetDifferentKeysAsync(tempDictionaries).ConfigureAwait(false);

            FileMessagesDict = differentKeys;

            Logger.Log($"Size: {differentKeys.Count}");
        }

        private static async Task<Dictionary<string, List<MessageEntry>>> GetDifferentKeysAsync(
            List<Dictionary<string, List<MessageEntry>>> dictionaries)
        {
            var resultDict = new Dictionary<string, List<MessageEntry>>();
            var allKeys = dictionaries.SelectMany(dict => dict.Keys).Distinct().ToList();

            var keyToEntriesMap = new Dictionary<string, List<(int index, MessageEntry entry)>>();

            for (int i = 0; i < dictionaries.Count; i++)
            {
                foreach (var kvp in dictionaries[i])
                {
                    if (!keyToEntriesMap.ContainsKey(kvp.Key))
                    {
                        keyToEntriesMap[kvp.Key] = new List<(int index, MessageEntry entry)>();
                    }
                    keyToEntriesMap[kvp.Key].AddRange(kvp.Value.Select(entry => (i, entry)));
                }
            }

            foreach (var key in allKeys)
            {
                var entryGroups = keyToEntriesMap.ContainsKey(key) ? keyToEntriesMap[key] : new List<(int index, MessageEntry entry)>();
                var groupedEntries = entryGroups.GroupBy(e => e.entry.DefaultString).ToList();
                
                var differentEntries = await Task.Run(() =>
                    groupedEntries
                        .Where(g => g.Count() == 1)
                        .SelectMany(g => g)
                        .ToList()).ConfigureAwait(false);

                if (differentEntries.Any())
                {
                    foreach (var entryGroup in differentEntries)
                    {
                        var (index, entry) = entryGroup;
                        if (entryGroups.Count == 1)
                        {
                            entry.MessageKey += index == 0 ? " (NEW)" : " (OLD)";
                        }
                        else
                        {
                            var isModified = entryGroups.Select(e => e.entry.DefaultString).Distinct().Count() > 1;
                            if (isModified)
                            {
                                entry.MessageKey += index == dictionaries.Count - 1 ? " (MODIFIED) (New)" : " (MODIFIED) (Old)";
                            }
                        }
                    }
                    resultDict[key] = differentEntries.Select(de => de.entry).ToList();
                }
                else
                {
                    if (entryGroups.Count < dictionaries.Count)
                    {
                        foreach (var entryGroup in entryGroups)
                        {
                            var (index, entry) = entryGroup;
                            if (index < dictionaries.Count - 1)
                            {
                                entry.MessageKey += " (REMOVED)";
                                if (!resultDict.ContainsKey(key))
                                {
                                    resultDict[key] = new List<MessageEntry>();
                                }
                                resultDict[key].Add(entry);
                            }
                        }
                    }
                }
            }

            return resultDict;
        }
    }
}
