using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClientMessageHandler.API;
using TextBox = HandyControl.Controls.TextBox;
using Window = System.Windows.Window;

namespace ClientMessageHandler
{
    public partial class DataWindow : Window
    {
        private static DataWindow instance;
        
        private ObservableCollection<string> fileListItems { get; set; }
        
        private List<string> allFileNames;
        
        private int batchSize = 500;

        public static DataWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataWindow();
                }

                return instance;
            }
        }
        
        public DataWindow()
        {
            InitializeComponent();
            fileListItems = new ObservableCollection<string>();
            fileList.ItemsSource = fileListItems;
        }
        
        public async Task PopulateFileListAsync()
        {
            ProgressWindow.Instance.Show();
            
            DateTime startLoad = DateTime.Now;

            if (API.API.FileMessagesDict != null)
            {
                allFileNames = API.API.FileMessagesDict.Keys.ToList();
                
                ProgressWindow.Instance.progressBar.Maximum = allFileNames.Count;
                await LoadNextBatchAsync(ProgressWindow.Instance);
                
                Show();
                
                DateTime finishLoad = DateTime.Now;
                
                Logger.Log($"Load {allFileNames.Count} items takes {finishLoad - startLoad} seconds.");
            }
            else
            {
                Logger.Error("ERROR! FileMessageDict is null or empty !");
            }
        }
        
        private async Task LoadNextBatchAsync(ProgressWindow progressWindow)
        {
            int startIndex = fileListItems.Count;
            int endIndex = startIndex + batchSize;
            if (endIndex > allFileNames.Count)
            {
                endIndex = allFileNames.Count;
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                fileListItems.Add(allFileNames[i]);
                progressWindow.UpdateProgress(i + 1);
            }

            if (endIndex < allFileNames.Count)
            {
                await Task.Delay(50);
                await LoadNextBatchAsync(progressWindow);
            }
            else
            {
                progressWindow.Close();
            }
        }
        
        protected override void OnClosing(CancelEventArgs ev)
        {
            ev.Cancel = true;
            this.Hide();
        }
        
        private void FileList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            messagePanel.Children.Clear();

            if (fileList.SelectedItem is string selectedFile && API.API.FileMessagesDict.TryGetValue(selectedFile, out var messages))
            {
                foreach (var message in messages)
                {
                    var messageTextBox = new TextBox
                    {
                        Text = $"MessageKey: {message.MessageKey}",
                        IsReadOnly = true,
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(0, 0, 0, 5),
                        FontWeight = FontWeights.Bold,
                        Background = Brushes.Transparent,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 15,
                    };

                    var defaultStringTextBox = new TextBox
                    {
                        Text = $"DefaultString: {message.DefaultString}",
                        IsReadOnly = true,
                        BorderThickness = new Thickness(0),
                        Background = Brushes.Transparent,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 15,
                    };

                    messagePanel.Children.Add(messageTextBox);
                    messagePanel.Children.Add(defaultStringTextBox);
                }
            }
        }
        
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.Equals(searchTextBox.Text, "Search...", StringComparison.OrdinalIgnoreCase))
            {
                string searchText = searchTextBox.Text.ToLower();
        
                var filteredFiles = allFileNames.Where(fileName => fileName.ToLower().Contains(searchText)).ToList();
                filteredFiles.AddRange(allFileNames.Where(fileName => API.API.FileMessagesDict[fileName].Any(message => message.MessageKey.ToLower().Contains(searchText) || message.DefaultString.ToLower().Contains(searchText))));
        
                UpdateFileList(filteredFiles);
            }
        }

        private void UpdateFileList(List<string> files)
        {
            fileListItems.Clear();
            foreach (var file in files)
            {
                fileListItems.Add(file);
            }
        }
    }
}