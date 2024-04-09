using System;
using System.Collections.Generic;

namespace Grafy_serwer
{
    public class SendObject
    {
        public List<List<int>> matrix { get; set; } = new List<List<int>>();
        public List<int> nodeIndexes { get; set; } = new List<int>();
    }
}
