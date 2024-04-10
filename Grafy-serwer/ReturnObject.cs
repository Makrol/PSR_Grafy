using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafy_serwer
{
    public class ReturnObject
    {
        public string client {  get; set; }
        public DateTime receiveTime { get; set; }
        public DateTime beginTime { get; set; }
        public DateTime endTime { get; set; }
        public List<CalculationResult> results { get; set; } = new List<CalculationResult>();
    }
}
