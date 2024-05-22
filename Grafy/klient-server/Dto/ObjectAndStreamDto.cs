using Grafy_serwer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace klient_server.Dto
{
    public class ObjectAndStreamDto
    {
        public SendObject obj {  get; set; }
        public NetworkStream stream { get; set; }

        public ObjectAndStreamDto(SendObject obj, NetworkStream stream)
        {
            this.obj = obj;
            this.stream = stream;
        }
    }
}
