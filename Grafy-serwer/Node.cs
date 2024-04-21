using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Grafy_serwer
{
    public class Node
    {
        //pozycja rysowanej elipsy
        public Point position { get; set; }
        //elpisa rysowana na ekranie
        public Ellipse ellipse { get; set; }
        //lista lini połączonych z punktem
        public List<EdgeEndpoint> edgeEndpoints = new List<EdgeEndpoint>();
        public List<int> forignNodeIndexes = new List<int>();
        public int CalculateDistance(Node otherNode)
        {
            if (ellipse == null || otherNode.ellipse == null)
                throw new ArgumentNullException("Ellipse is not initialized");

            double thisX = Canvas.GetLeft(ellipse) + ellipse.Width / 2;
            double thisY = Canvas.GetTop(ellipse) + ellipse.Height / 2;

            double otherX = Canvas.GetLeft(otherNode.ellipse) + otherNode.ellipse.Width / 2;
            double otherY = Canvas.GetTop(otherNode.ellipse) + otherNode.ellipse.Height / 2;

            double distance = Math.Sqrt(Math.Pow(thisX - otherX, 2) + Math.Pow(thisY - otherY, 2));

            return (int)distance;
        }
    }
}
