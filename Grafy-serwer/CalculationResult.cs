using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafy_serwer
{
    public class CalculationResult
    {
        public List<int> nodeDistances { get; set; } = new List<int>();
        public List<List<int>> paths { get; set; } = new List<List<int>>();
        public int sum { get; set; } = 0;
        public DateTime beginCalculation { get; set; }
        public DateTime endCalculations { get; set; }
    }
}
