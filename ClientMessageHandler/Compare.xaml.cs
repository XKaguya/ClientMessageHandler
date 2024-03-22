using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using ClientMessageHandler.API;

namespace ClientMessageHandler;

public partial class Compare : Window
{
    private static Compare _instance;

    private List<string> filePaths = new();
    
    public static Compare Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Compare();
            }

            return _instance;
        }
    }
    
    public Compare()
    {
        InitializeComponent();
    }
    
    protected override void OnClosing(CancelEventArgs ev)
    {
        ev.Cancel = true;
        this.Hide();
    }
    
    private void SelectFile1(object sender, RoutedEventArgs e)
    {
        Logger.Log($"Selecting Original File...");

        var temp = API.API.LoadXmlFileAlter();
        if (temp != null && !filePaths.Contains(temp))
        {
            Content1.Text = temp;
            
            filePaths.Add(temp); 
        }
    }
    
    private void SelectFile2(object sender, RoutedEventArgs e)
    {
        Logger.Log($"Selecting File2...");

        var temp = API.API.LoadXmlFileAlter();
        if (temp != null && !filePaths.Contains(temp))
        {
            Content2.Text = temp;
            
            filePaths.Add(temp); 
        }
    }
    
    private void SelectFile3(object sender, RoutedEventArgs e)
    {
        Logger.Log($"Selecting File2...");

        var temp = API.API.LoadXmlFileAlter();
        if (temp != null && !filePaths.Contains(temp))
        {
            Content3.Text = temp;
            
            filePaths.Add(temp); 
        }
    }

    private async void CallDif(object sender, RoutedEventArgs e)
    {
        Logger.Log($"CallDif executed.");
        
        await API.API.GetDifferenceAsync(filePaths);

        await DataWindow.Instance.PopulateFileListAsync();
        
        filePaths.Clear();
    }
}