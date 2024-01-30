using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ClientMessageHandler.API
{
    public class Logger
    {
        private static RichTextBox logRichTextBox;

        private static int MAX_LOG_COUNT { get; set; } = 180;

        public static string LogLevel { get; set; } = "Info";

        static Logger()
        {
            logRichTextBox = new RichTextBox();
        }

        public static void SetLogLevel(string level)
        {
            if (level == "Debug" || level == "DEBUG")
            {
                LogLevel = level;
            }
            else if (level == "Info" || level == "INFO")
            {
                LogLevel = level;
            }
        }
        
        public static void SetLogTarget(RichTextBox richTextBox)
        {
            logRichTextBox = richTextBox;
        }

        public static void SetLogLineLimit(int num)
        {
            MAX_LOG_COUNT = num;
        }
        
        public static void SetLogBackgroundColor(SolidColorBrush color)
        {
            logRichTextBox.Background = color;
        }

        public static void Log(string message)
        {
            if (logRichTextBox != null)
            {
                if (logRichTextBox.Document.Blocks.Count > MAX_LOG_COUNT)
                {
                    ClearLog();
                }
        
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [INFO]: {message}";
                LogAddLine(logMessage, Brushes.CornflowerBlue);
            }
        }


        public static void Error(string message)
        {
            if (logRichTextBox != null)
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR]: {message}";
                LogAddLine(logMessage, Brushes.Red);
            }
        }
        
        public static void Debug(string message)
        {
            if (logRichTextBox != null)
            {
                if (LogLevel == "Debug" || LogLevel == "DEBUG")
                {
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [DEBUG]: {message}";
                    LogAddLine(logMessage, Brushes.Chocolate);
                }
            }
        }

        private static void LogAddLine(string message, SolidColorBrush color)
        {
            Paragraph paragraph = new Paragraph(new Run(message));
            paragraph.Foreground = color;

            logRichTextBox.Document.Blocks.Add(paragraph);
        }
        
        public static void ClearLog()
        {
            logRichTextBox.Document.Blocks.Clear();
        }
    }
}