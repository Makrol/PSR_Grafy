using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafy_serwer
{
    public class ResultRecord
    {
        public string ClientName { get; set; }
        public string RecieveDate { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public ReturnObject result { get; set; }
    }
}
