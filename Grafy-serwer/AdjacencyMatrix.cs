using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Grafy_serwer
{
    public class AdjacencyMatrix
    {
        private int[,] matrix;
        public AdjacencyMatrix(int size) {
            matrix = new int[size, size];
        }

        public void generateMatrix(List<Node> nodes)
        {
            for(int i=0;i<nodes.Count;i++)
            {
               
                    Debug.WriteLine(findConnectedNodes(nodes[i],i,nodes));

            }
        }
        private List<int> findConnectedNodes(Node currentNode,int currentNodeIndex, List<Node> nodes)
        {
            List<int> result = new List<int>();
            for (int j = 0; j < nodes.Count; j++)
            {
                foreach (EdgeEndpoint edge in nodes[j].edgeEndpoints)
                {
                    foreach(EdgeEndpoint currentNodeEdge in currentNode.edgeEndpoints)
                    {
                        if(currentNodeEdge.line == edge.line)
                        {
                            if(currentNodeIndex!=j)
                                result.Add(j);
                        }
                    }
                }
            }
            return result;
        }
    }
}
