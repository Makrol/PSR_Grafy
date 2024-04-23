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
        private int size =0;
        public List<List<int>> matrix = new List<List<int>>();
        public AdjacencyMatrix(int size) {
            for(int i=0;i<size ;i++)
            {
                var tmp = new List<int>();
                for(int j=0;j<size;j++)
                    tmp.Add(0);
                matrix.Add(tmp);
            }
            this.size = size;
        }

        public List<List<int>> generateMatrix(List<Node> nodes)
        {
            for(int i=0;i<nodes.Count;i++)
            {
                var result = findConnectedNodes(nodes[i],i,nodes);
                foreach(int index in result)
                {
                    matrix[i][index] = nodes[i].CalculateDistance(nodes[index]);
                }
            }
            return matrix;
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
