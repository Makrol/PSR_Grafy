using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafy_serwer
{
    public class Dijkstra
    {
        private Dijkstra() { }

        static public CalculationResult determineSolution(List<List<int>> graph, int sourceNodeIndex, int size)
        {
            DateTime begin = DateTime.Now;
            int[] distance = new int[size];
            int[] parent = new int[size]; 

            bool[] shortestPathSet = new bool[size]; 

            for (int i = 0; i < size; i++)
            {
                distance[i] = int.MaxValue; 
                parent[i] = -1; 
                shortestPathSet[i] = false; 
            }

            distance[sourceNodeIndex] = 0;

            for (int count = 0; count < size - 1; count++)
            {
                int u = MinDistance(distance, shortestPathSet, size);

                shortestPathSet[u] = true;

                for (int v = 0; v < size; v++)
                {
                    if (!shortestPathSet[v] && graph[u][v] != 0 && distance[u] != int.MaxValue && distance[u] + graph[u][v] < distance[v])
                    {
                        distance[v] = distance[u] + graph[u][v];
                        parent[v] = u;
                    }
                }
            }
            DateTime end = DateTime.Now;

            PrintSolution(distance, parent, size, sourceNodeIndex);
            CalculationResult returnObject = CreateReturnObject(distance, parent, size, sourceNodeIndex);
            returnObject.beginCalculation = begin;
            returnObject.endCalculations = end;
            return returnObject;
        }
        private static int MinDistance(int[] distance, bool[] shortestPathSet,int size)
        {
            int min = int.MaxValue, minIndex = -1;

            for (int v = 0; v < size; v++)
            {
                if (shortestPathSet[v] == false && distance[v] <= min)
                {
                    min = distance[v];
                    minIndex = v;
                }
            }

            return minIndex;
        }
        private static CalculationResult CreateReturnObject(int[] distance, int[] parent, int size, int sourceNodeIndex)
        {
            CalculationResult returnObject = new CalculationResult();
            int sum = 0;
            for (int i = 0; i < size; i++)
            {
                List<int> pathForNode = new List<int>();
                CreatePath(parent, i,pathForNode);
                returnObject.paths.Add(pathForNode);
                returnObject.nodeDistances.Add(distance[i]);
                sum += distance[i];
            }
            returnObject.sum = sum;
            return returnObject;

        }
        private static void PrintSolution(int[] distance, int[] parent, int size, int sourceNodeIndex)
        {
            int sum = 0;
            Debug.WriteLine("Wierzchołek \t\t Odległość od źródła");
            for (int i = 0; i < size; i++)
            {
                Debug.WriteLine(i + " \t\t " + distance[i]);
                sum += distance[i];
            }

            Debug.WriteLine("Suma długości: " + sum);

            Debug.WriteLine("\nNajkrótsze ścieżki:");
            for (int i = 1; i < size; i++)
            {
                Debug.Write("Źródło -> " + i + " : "+ sourceNodeIndex);
                PrintPath(parent, i);
                Debug.WriteLine("");
            }
        }

        private static void PrintPath(int[] parent, int j)
        {
            if (parent[j] == -1)
                return;

            PrintPath(parent, parent[j]);
            Debug.Write(" -> " + j);
        }
        private static void CreatePath(int[] parent, int j,List<int> pathForNode)
        {
            if (parent[j] == -1)
                return;

            CreatePath(parent, parent[j], pathForNode);
            pathForNode.Add(j);
        }
    }
}
