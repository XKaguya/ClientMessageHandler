using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ClientMessageHandler
{
    public class DataWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> fileList;

        public ObservableCollection<string> FileList
        {
            get { return fileList; }
            set
            {
                fileList = value;
                OnPropertyChanged(nameof(FileList));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

