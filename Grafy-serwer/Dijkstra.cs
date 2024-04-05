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

        static public void determineSolution(int[,] graph, int sourceNodeIndex, int size)
        {
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
                    if (!shortestPathSet[v] && graph[u, v] != 0 && distance[u] != int.MaxValue && distance[u] + graph[u, v] < distance[v])
                    {
                        distance[v] = distance[u] + graph[u, v];
                        parent[v] = u;
                    }
                }
            }

            PrintSolution(distance, parent, size, sourceNodeIndex);
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
    }
}
