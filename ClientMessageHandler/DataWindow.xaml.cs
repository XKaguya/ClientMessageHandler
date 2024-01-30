using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClientMessageHandler.API;
using ClientMessageHandler.Entry;

namespace ClientMessageHandler;

public partial class DataWindow : Window
{
    private static DataWindow instance;

    private ObservableCollection<MessageEntry> dataItems;

    private List<MessageEntry> originalDataItems;

    public static DataWindow Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DataWindow();
                instance.InitializeComponent();
                instance.searchTextBox.TextChanged += instance.SearchHandler;

                instance.InitMethod();
            }

            return instance;
        }
    }

    private void InitMethod()
    {
        dataItems = new ObservableCollection<MessageEntry>();
        originalDataItems = new List<MessageEntry>();
        listView.ItemsSource = dataItems;
    }

    private void ListView_Drop(object sender, DragEventArgs ev)
    {
        if (ev.Data.GetDataPresent(typeof(MessageEntry)))
        {
            MessageEntry droppedData = ev.Data.GetData(typeof(MessageEntry)) as MessageEntry;
            if (droppedData != null)
            {
                dataItems.Add(droppedData);
            }
        }
    }

    public void SetData(List<MessageEntry> msg)
    {
        Logger.Log($"Received {msg.Count} items in SetData method.");

        originalDataItems.Clear();
        originalDataItems.AddRange(msg);

        RestoreData();
    }

    private void SearchHandler(object sender, TextChangedEventArgs e)
    {
        string searchText = searchTextBox.Text;

        if (!string.Equals(searchText, "Search...", StringComparison.OrdinalIgnoreCase))
        {
            List<MessageEntry> filteredList = originalDataItems
                .Where(entry =>
                    entry.FileName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    entry.MessageKey.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    entry.DefaultString.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            UpdateListView(filteredList);
        }
        else
        {
            RestoreData();
        }
    }

    private void RestoreData()
    {
        if (dataItems != null)
        {
            dataItems.Clear();
            dataItems.AddRange(originalDataItems);
        }
    }

    private void UpdateListView(List<MessageEntry> itemList)
    {
        dataItems.Clear();
        dataItems.AddRange(itemList);
    }
}

public static class ObservableCollectionExtensions
{
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
