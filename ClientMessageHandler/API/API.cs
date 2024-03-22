using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using ClientMessageHandler.Entry;
using Microsoft.Win32;

namespace ClientMessageHandler.API
{
    public static class API
    {
        public static Dictionary<string, List<MessageEntry>> FileMessagesDict { get; set; } = new Dictionary<string, List<MessageEntry>>();

        public static void Serialize(string xmlString)
        {
            try
            {
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
                            if (!FileMessagesDict.ContainsKey(fileName))
                            {
                                FileMessagesDict[fileName] = new List<MessageEntry>();
                            }
                            
                            MessageEntry entry = new MessageEntry
                            {
                                MessageKey = messageElement.SelectSingleNode("ns:MessageKey", nsmgr)?.Value,
                                DefaultString = messageElement.SelectSingleNode("ns:DefaultString", nsmgr)?.Value ?? messageElement.SelectSingleNode("ns:Defaultstring", nsmgr)?.Value
                            };
                            
                            FileMessagesDict[fileName].Add(entry);
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                Logger.Error($"Error parsing XML: {ex.Message}");
            }

            if (FileMessagesDict.Count == 0)
            {
                Logger.Error($"No messages found in the XML.");
            }
        }
        
        private static async Task SerializeAsync(string xmlString)
        {
            try
            {
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
                            if (!FileMessagesDict.ContainsKey(fileName))
                            {
                                FileMessagesDict[fileName] = new List<MessageEntry>();
                            }
                    
                            MessageEntry entry = new MessageEntry
                            {
                                MessageKey = messageElement.SelectSingleNode("ns:MessageKey", nsmgr)?.Value,
                                DefaultString = messageElement.SelectSingleNode("ns:DefaultString", nsmgr)?.Value ?? messageElement.SelectSingleNode("ns:Defaultstring", nsmgr)?.Value
                            };
                    
                            FileMessagesDict[fileName].Add(entry);
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                Logger.Error($"Error parsing XML: {ex.Message}");
            }

            if (FileMessagesDict.Count == 0)
            {
                Logger.Error($"No messages found in the XML.");
            }
        }
        
        private static async Task<Dictionary<string, List<MessageEntry>>> SerializeAsync(string xmlString, bool ifReturn)
        {
            try
            {
                Dictionary<string, List<MessageEntry>> tempDictionary = new();
                
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
                            if (!tempDictionary.ContainsKey(fileName))
                            {
                                tempDictionary[fileName] = new List<MessageEntry>();
                            }
                    
                            MessageEntry entry = new MessageEntry
                            {
                                MessageKey = messageElement.SelectSingleNode("ns:MessageKey", nsmgr)?.Value,
                                DefaultString = messageElement.SelectSingleNode("ns:DefaultString", nsmgr)?.Value ?? messageElement.SelectSingleNode("ns:Defaultstring", nsmgr)?.Value
                            };
                    
                            tempDictionary[fileName].Add(entry);
                        }
                    }
                }

                return tempDictionary;
            }
            catch (XmlException ex)
            {
                Logger.Error($"Error parsing XML: {ex.Message}");
            }

            if (FileMessagesDict.Count == 0)
            {
                Logger.Error($"No messages found in the XML.");
            }

            return null;
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

        // Compare method should not use the LoadXmlFile method. It should use a method that only returns a file path one.
        // Which is LoadXmlFileAlter()
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
            Dictionary<string, List<MessageEntry>> tempDictionary0 = new();
            Dictionary<string, List<MessageEntry>> tempDictionary1 = new();
            Dictionary<string, List<MessageEntry>> tempDictionary2 = new();
            
            Logger.Log($"Selected {filePaths.Count} to compare.");

            DateTime load = DateTime.Now;

            Dictionary<string, string> files = new();

            foreach (string file in filePaths)
            {
                try
                {
                    string fileContent = await File.ReadAllTextAsync(file);
                    files[file] = fileContent;

                    if (tempDictionary0.Count == 0)
                    {
                        tempDictionary0 = await SerializeAsync(files[file], true);
                    }
                    else if (tempDictionary1.Count == 0)
                    {
                        tempDictionary1 = await SerializeAsync(files[file], true);
                    }
                    else if (filePaths.Count == 3 && tempDictionary2.Count == 0)
                    {
                        tempDictionary2 = await SerializeAsync(files[file], true);
                    }
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
            Logger.Log($"GetDifference method takes {loaded - load} second to load {files.Count} files.");
            Logger.Log("Now proceeding difference part.");

            var differentKeys = await GetDifferentKeysAsync(tempDictionary0, tempDictionary1, tempDictionary2);
            
            FileMessagesDict.Clear();
            FileMessagesDict = differentKeys;

            if (differentKeys == FileMessagesDict)
            {
                Logger.Log($"Size: {differentKeys.Count}");
            }
        }
        
        private static async Task<Dictionary<string, List<MessageEntry>>> GetDifferentKeysAsync(
            Dictionary<string, List<MessageEntry>> dict0,
            Dictionary<string, List<MessageEntry>> dict1,
            Dictionary<string, List<MessageEntry>> dict2)
        {
            var resultDict = new Dictionary<string, List<MessageEntry>>();

            var allKeys = dict0.Keys.Union(dict1.Keys);
            if (dict2 != null)
                allKeys = allKeys.Union(dict2.Keys);

            foreach (var key in allKeys)
            {
                var entryList0 = dict0.ContainsKey(key) ? dict0[key] : new List<MessageEntry>();
                var entryList1 = dict1.ContainsKey(key) ? dict1[key] : new List<MessageEntry>();
                var entryList2 = dict2 != null && dict2.ContainsKey(key) ? dict2[key] : new List<MessageEntry>();

                var allEntries = entryList0.Union(entryList1).Union(entryList2);

                var differentEntries = await Task.Run(() =>
                    allEntries.GroupBy(entry => entry.DefaultString)
                        .Where(group => group.Count() == 1)
                        .SelectMany(group => group)
                        .ToList());

                if (differentEntries.Any())
                {
                    foreach (var entry in differentEntries)
                    {
                        if ((dict0.ContainsKey(key) && !dict1.ContainsKey(key) && !dict2.ContainsKey(key)) ||
                            (!dict0.ContainsKey(key) && dict1.ContainsKey(key) && !dict2.ContainsKey(key)) ||
                            (!dict0.ContainsKey(key) && !dict1.ContainsKey(key) && dict2.ContainsKey(key)))
                        {
                            entry.MessageKey += " (NEW)";
                        }
                        else
                        {
                            entry.MessageKey += " (MODIFIED)";
                        }
                    }

                    resultDict.Add(key, differentEntries.ToList());
                }
            }

            return resultDict;
        }
    }
}