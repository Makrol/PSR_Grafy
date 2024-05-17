using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafy_serwer
{
    class BarabásiAlbert
    {
        static private List<List<int>> adjacencyList;
        private BarabásiAlbert() { }
        static public void init(int vertices)
        {
            adjacencyList = new List<List<int>>();
            for (int i = 0; i < vertices; i++)
            {
                adjacencyList.Add(new List<int>());
            }
        }

        static public void AddEdge(int source, int destination)
        {
            adjacencyList[source].Add(destination);
            adjacencyList[destination].Add(source);
        }
        static public List<Tuple<int, int>> createEdges(int m0, int m, int vertices)
        {
            init(vertices);
            // Creating a fully connected initial graph with m0 vertices
            for (int i = 0; i < m0; i++)
            {
                for (int j = i + 1; j < m0; j++)
                {
                    AddEdge(i, j);
                }
            }

            // Generating additional vertices
            for (int i = m0; i < vertices; i++)
            {
                var degrees = new Dictionary<int, int>(); // Store degrees of existing vertices

                // Calculate the degree of each existing vertex
                for (int j = 0; j < i; j++)
                {
                    degrees[j] = adjacencyList[j].Count;
                }

                // Create m edges for the new vertex
                for (int j = 0; j < m; j++)
                {
                    var selectedVertex = RouletteWheelSelection(degrees);
                    AddEdge(i, selectedVertex);
                    degrees[selectedVertex]++;
                }
            }
            var returnList = new List<Tuple<int, int>>();
            for (int i = 0; i < adjacencyList.Count; i++)
            {
                Console.Write("Vertex " + i + " is connected to: ");
                foreach (var neighbor in adjacencyList[i])
                {
                    Console.Write(neighbor + " ");
                    returnList.Add(Tuple.Create(i, neighbor));
                }
                Console.WriteLine();
            }
            return returnList;
        }
        static public List<Tuple<int, int>> createEdgesWithPercents(int m0, int m, int vertices, double connectionPercentage)
        {
            init(vertices);
            // Creating a fully connected initial graph with m0 vertices
            for (int i = 0; i < m0; i++)
            {
                for (int j = i + 1; j < m0; j++)
                {
                    AddEdge(i, j);
                }
            }

            // Generating additional vertices
            for (int i = m0; i < vertices; i++)
            {
                var degrees = new Dictionary<int, int>(); // Store degrees of existing vertices

                // Calculate the degree of each existing vertex
                for (int j = 0; j < i; j++)
                {
                    degrees[j] = adjacencyList[j].Count;
                }

                // Create m edges for the new vertex
                int totalPossibleConnections = (i * (i - 1)) / 2; // Total possible connections in a fully connected graph
                int connectionsToMake = (int)Math.Round(connectionPercentage * totalPossibleConnections);

                for (int j = 0; j < connectionsToMake; j++)
                {
                    var selectedVertex = RouletteWheelSelection(degrees);
                    AddEdge(i, selectedVertex);
                    degrees[selectedVertex]++;
                }
            }
            var returnList = new List<Tuple<int, int>>();
            for (int i = 0; i < adjacencyList.Count; i++)
            {
                Console.Write("Vertex " + i + " is connected to: ");
                foreach (var neighbor in adjacencyList[i])
                {
                    Console.Write(neighbor + " ");
                    returnList.Add(Tuple.Create(i, neighbor));
                }
                Console.WriteLine();
            }
            return returnList;
        }

        static private int RouletteWheelSelection(Dictionary<int, int> degrees)
        {
            // Roulette wheel selection based on vertex degrees
            int totalDegree = 0;
            foreach (var degree in degrees.Values)
            {
                totalDegree += degree;
            }

            int randomNumber = new Random().Next(totalDegree);
            int sum = 0;
            foreach (var kvp in degrees)
            {
                sum += kvp.Value;
                if (randomNumber < sum)
                {
                    return kvp.Key;
                }
            }
            return degrees.Count - 1;
        }
    }
}