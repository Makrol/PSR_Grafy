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
        public Ellipse ellipse { get; set; }
        public List<EdgeEndpoint> edgeEndpoints = new List<EdgeEndpoint>();
    }
}
