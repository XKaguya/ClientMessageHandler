using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClientMessageHandler.Entry
{
    public class MessageEntry : INotifyPropertyChanged
    {
        private string? _messageKey;
        public string? MessageKey
        {
            get => _messageKey;
            set => SetProperty(ref _messageKey, value);
        }

        private string? _defaultString;
        public string? DefaultString
        {
            get => _defaultString;
            set => SetProperty(ref _defaultString, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        
        public override string ToString()
        {
            return $"MessageKey: {MessageKey}, DefaultString: {DefaultString}";
        }
    }
}