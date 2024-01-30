using System.ComponentModel;

namespace ClientMessageHandler.Entry
{
    public class MessageEntry : INotifyPropertyChanged
    {
        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        private string _messageKey;
        public string MessageKey
        {
            get { return _messageKey; }
            set
            {
                if (_messageKey != value)
                {
                    _messageKey = value;
                    OnPropertyChanged(nameof(MessageKey));
                }
            }
        }

        private string _defaultString;
        public string DefaultString
        {
            get { return _defaultString; }
            set
            {
                if (_defaultString != value)
                {
                    _defaultString = value;
                    OnPropertyChanged(nameof(DefaultString));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}