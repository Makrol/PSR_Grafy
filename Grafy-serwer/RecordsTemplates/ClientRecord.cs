using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafy_serwer
{
    public class CliendRecord : INotifyPropertyChanged
    {
        public long timeSum = 0;
        private int _count;
        public int Count
        {
            get { return _count; }
            set
            {
                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }
        private long _AvarageTime;
        public long AvarageTime
        {
            get { return _AvarageTime; }
            set
            {
                if (_AvarageTime != value)
                {
                    _AvarageTime = value;
                    OnPropertyChanged(nameof(AvarageTime));
                }
            }
        }

        public string IPAddress { get; set; }
        public int Port { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
