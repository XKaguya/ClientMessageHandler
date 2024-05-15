using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClientMessageHandler.Generic;
using TextBox = HandyControl.Controls.TextBox;
using Window = System.Windows.Window;

namespace ClientMessageHandler
{
    public partial class DataWindow : Window
    {
        private static DataWindow instance;
        
        private ObservableCollection<string> fileListItems { get; set; } = new ObservableCollection<string>();
        
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
            fileList.ItemsSource = fileListItems;
        }

        public async Task PopulateFileListAsync()
        {
            ProgressWindow.Instance.Show();
            
            DateTime startLoad = DateTime.Now;

            if (API.FileMessagesDict != null)
            {
                allFileNames = API.FileMessagesDict.Keys.ToList();
                
                ProgressWindow.Instance.progressBar.Maximum = allFileNames.Count;
                await LoadFilesInBatchesAsync(ProgressWindow.Instance);
                
                Show();
                
                DateTime finishLoad = DateTime.Now;
                
                Logger.Log($"Load {allFileNames.Count} items takes {finishLoad - startLoad} seconds.");
            }
            else
            {
                Logger.Error("ERROR! FileMessageDict is null or empty!");
            }
        }

        private async Task LoadFilesInBatchesAsync(ProgressWindow progressWindow)
        {
            for (int i = 0; i < allFileNames.Count; i += batchSize)
            {
                var batch = allFileNames.Skip(i).Take(batchSize).ToList();

                await Dispatcher.InvokeAsync(() =>
                {
                    foreach (var fileName in batch)
                    {
                        fileListItems.Add(fileName);
                        progressWindow.UpdateProgress(fileListItems.Count);
                    }
                });

                await Task.Delay(1);
            }

            progressWindow.Hide();
        }

        protected override void OnClosing(CancelEventArgs ev)
        {
            ev.Cancel = true;
            Hide();
        }
        
        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                messagePanel.Children.Clear();

                if (fileList.SelectedItem is string selectedFile &&
                    API.FileMessagesDict.TryGetValue(selectedFile, out var messages))
                {
                    foreach (var message in messages)
                    {
                        var horizontalPanel = new StackPanel
                            { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 10) };

                        var messageTextBox = CreateTextBox($"MessageKey: {message.MessageKey}", FontWeights.Bold);
                        var defaultStringTextBox = CreateTextBox($"DefaultString: {message.DefaultString}");

                        var button = new Button
                        {
                            Content = "Toggle Details",
                            Margin = new Thickness(5)
                        };

                        var webBrowser = new WebBrowser
                        {
                            Visibility = Visibility.Collapsed
                        };

                        button.Click += (s, args) =>
                        {
                            if (webBrowser.Visibility == Visibility.Collapsed)
                            {
                                string htmlContent = $@"
                            <html>
                            <body>
                            <p><strong>MessageKey:</strong> {message.MessageKey}</p>
                            <p><strong>DefaultString:</strong> {message.DefaultString}</p>
                            </body>
                            </html>";

                                webBrowser.NavigateToString(htmlContent);
                                webBrowser.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                webBrowser.Visibility = Visibility.Collapsed;
                            }
                        };

                        horizontalPanel.Children.Add(messageTextBox);
                        horizontalPanel.Children.Add(button);

                        var verticalPanel = new StackPanel { Orientation = Orientation.Vertical };
                        verticalPanel.Children.Add(horizontalPanel);
                        verticalPanel.Children.Add(defaultStringTextBox);
                        verticalPanel.Children.Add(webBrowser);

                        messagePanel.Children.Add(verticalPanel);

                        webBrowser.Width = double.NaN;
                        webBrowser.LoadCompleted += (s, e) =>
                        {
                            dynamic doc = webBrowser.Document;
                            if (doc != null && doc!.body != null)
                            {
                                webBrowser.Height = double.NaN;
                                webBrowser.Height = doc!.body.scrollHeight;
                                
                                string script = "document.body.style.overflow ='hidden'";
                                webBrowser.InvokeScript("execScript", new Object[] { script, "JavaScript" });
                            }
                        };
                        webBrowser.SetValue(Grid.RowSpanProperty, 2);
                        webBrowser.SetValue(Grid.ColumnSpanProperty, 2);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message + exception.StackTrace);
            }
        }

        private TextBox CreateTextBox(string text, FontWeight fontWeight = default)
        {
            return new TextBox
            {
                Text = text,
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = fontWeight == default ? FontWeights.Normal : fontWeight,
                Background = Brushes.Transparent,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 15,
            };
        }
        
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.Equals(searchTextBox.Text, "Search...", StringComparison.OrdinalIgnoreCase))
            {
                string searchText = searchTextBox.Text.ToLower();

                var filteredFiles = allFileNames
                    .Where(fileName => fileName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Concat(allFileNames.Where(fileName => API.FileMessagesDict[fileName]
                        .Any(message => message.MessageKey.Contains(searchText, StringComparison.OrdinalIgnoreCase) 
                                     || message.DefaultString.Contains(searchText, StringComparison.OrdinalIgnoreCase))))
                    .Distinct()
                    .ToList();

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
