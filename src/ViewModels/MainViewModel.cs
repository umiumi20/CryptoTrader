using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace CryptoTrader.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _currentDateTime = "Current Date and Time (UTC): 2024-12-28 21:34:42";
        private string _currentUser = "Current User's Login: umiumi20";
        private readonly DispatcherTimer _timer;

        public string CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                if (_currentDateTime != value)
                {
                    _currentDateTime = $"Current Date and Time (UTC): {value}";
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = $"Current User's Login: {value}";
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            CurrentUser = "umiumi20";
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => UpdateDateTime();
            _timer.Start();
        }

        private void UpdateDateTime()
        {
            CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
