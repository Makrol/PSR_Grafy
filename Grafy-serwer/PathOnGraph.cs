using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Grafy_serwer
{
    public class PathOnGraph
    {
        public Ellipse destinationNode {  get; set; }
        public List<Line> pathParts { get; set; } = new List<Line>();
    }
}
