using System.Windows;

namespace Grafy_serwer
{
    public class GraphStructure
    {
        public List<Point> nodePositions { get; set; } = new List<Point>();
        public List<Tuple<int, int>> edgeEndpoints { get; set;} = new List<Tuple<int, int>>();
        public bool IsEdgeUsed(int startNode, int endNode)
        {
            foreach (var edge in edgeEndpoints)
            {
                if ((edge.Item1 == startNode && edge.Item2 == endNode) ||
                    (edge.Item1 == endNode && edge.Item2 == startNode))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
