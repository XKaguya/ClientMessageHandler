using System.Windows;

namespace ClientMessageHandler
{
    public partial class ProgressWindow : Window
    {
        private static ProgressWindow instance { get; set; }
        
        public ProgressWindow()
        {
            InitializeComponent();
        }
        
        public static ProgressWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProgressWindow();
                }

                return instance;
            }
        }

        public void UpdateProgress(double value)
        {
            progressBar.Value = value;
        }
    }
}