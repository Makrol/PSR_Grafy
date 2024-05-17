using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafy_serwer
{
    public class ConectedCLientInfo
    {
        private string _ipAddress;
        private string _port;
        private string _status;

        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public string Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public ConectedCLientInfo(string ipAddress, string port, string status)
        {
            _ipAddress = ipAddress;
            _port = port;
            _status = status;
        }
    }
}
