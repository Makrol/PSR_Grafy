using System.Windows;

namespace Grafy_serwer
{
    public class GraphStructure
    {
        public List<Point> nodePositions { get; set; } = new List<Point>();
        public List<Tuple<int, int>> edgeEndpoints { get; set;} = new List<Tuple<int, int>>();
    }
}
