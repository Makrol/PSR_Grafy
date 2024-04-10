using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafy_serwer
{
    public class CalculationResult
    {
        public int startNode { get; set; }
        public List<int> nodeDistances { get; set; } = new List<int>();
        public List<List<int>> paths { get; set; } = new List<List<int>>();
        public int sum { get; set; } = 0;
        public DateTime beginCalculation { get; set; }
        public DateTime endCalculations { get; set; }

        public string PathToString()
        {
            string result = "";
            foreach (List<int> path in paths)
            {
                for(int i=0;i<path.Count;i++)
                {
                    result += path[i];
                    if(i< path.Count-1)
                        result += "->";
                }
                if (path.Count == 0)
                    result += "Brak\n";
                else
                    result += "\n";
            }
            return result;
        }
    }
}
