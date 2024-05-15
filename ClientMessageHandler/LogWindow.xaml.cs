using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ClientMessageHandler.Generic;

namespace ClientMessageHandler
{
    public partial class LogWindow : Window
    {
        private static LogWindow instance;
        private RichTextBox logRichTextBox;

        public static LogWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogWindow();
                    instance.InitializeComponent();
                    instance.logRichTextBox = instance.LogRichTextBox;
                    Logger.SetLogTarget(instance.logRichTextBox);
                }

                return instance;
            }
        }
        
        protected override void OnClosing(CancelEventArgs ev)
        {
            ev.Cancel = true;
            this.Hide();
        }

        private LogWindow()
        {
            InitializeComponent();
            logRichTextBox = LogRichTextBox;
        }
    }
}