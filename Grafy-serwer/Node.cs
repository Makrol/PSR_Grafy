using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Grafy_serwer
{
    public class Node
    {
        //elpisa rysowana na ekranie
        public Ellipse ellipse { get; set; }
        //lista lini połączonych z punktem
        public List<EdgeEndpoint> edgeEndpoints = new List<EdgeEndpoint>();
    }
}
