using System;
using System.Collections.Generic;
using System.IO;
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

        public static string LoadXmlFile()
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
    }
}