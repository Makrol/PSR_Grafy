using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafy_serwer.RecordsTemplates
{
    public class ResultPathRecord
    {
        public string Clients { get; set; }
        public string RecieveDate { get; set; }
        public string BegineDate { get; set; }
        public string EndDate { get; set; }
        public string Path { get; set; }
        public string Distance { get; set; }
        public string StartNode { get; set; }
        public List<List<int>> IndexPaths {  get; set; }
    }
}
