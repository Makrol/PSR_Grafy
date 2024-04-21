using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Grafy_serwer
{
    public class EdgeEndpoint
    {
        public Line line { get; set; }
        public bool isFirstPoint = false;
    }
}
