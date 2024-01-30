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
        public static List<MessageEntry> Messages { get; set; } = new List<MessageEntry>();

        public static List<MessageEntry> Serialize(string xmlString)
        {
            List<MessageEntry> messages = new List<MessageEntry>();

            try
            {
                using (StringReader stringReader = new StringReader(xmlString))
                using (XmlReader reader = XmlReader.Create(stringReader))
                {
                    XPathDocument xPathDoc = new XPathDocument(reader);
                    XPathNavigator navigator = xPathDoc.CreateNavigator();

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(navigator.NameTable);
                    nsmgr.AddNamespace("ns", "http://datacontract.gib.me/startrekonline");

                    int messagesCount = navigator.Select("//ns:Message", nsmgr).Count;

                    messages = new List<MessageEntry>(messagesCount);

                    foreach (XPathNavigator messageElement in navigator.Select("//ns:Message", nsmgr))
                    {
                        MessageEntry entry = new MessageEntry
                        {
                            MessageKey = messageElement.SelectSingleNode("ns:MessageKey", nsmgr)?.Value,
                            FileName = messageElement.SelectSingleNode("ns:FileName", nsmgr)?.Value,
                            DefaultString = messageElement.SelectSingleNode("ns:DefaultString", nsmgr)?.Value
                        };

                        messages.Add(entry);
                    }
                }
            }
            catch (XmlException ex)
            {
                Logger.Error($"Error parsing XML: {ex.Message}");
            }

            if (messages.Count == 0)
            {
                Logger.Error($"No messages found in the XML.");
            }

            return messages;
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
                Messages = LoadXmlFromFile(filePath);
                DateTime loaded = DateTime.Now;
                Logger.Log($"LoadXmlFromFile method takes {loaded - load} second.");

                return filePath;
            }

            return null;
        }

        private static List<MessageEntry> LoadXmlFromFile(string filePath)
        {
            try
            {
                string xmlString = File.ReadAllText(filePath);
                return Serialize(xmlString);
            }
            catch (IOException ex)
            {
                Logger.Error($"{nameof(IOException)}: {ex.Message}");
                return new List<MessageEntry>();
            }
        }
    }
}
