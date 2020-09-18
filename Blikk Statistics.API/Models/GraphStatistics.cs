using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blikk_Statistics.API.Models
{
    public class GraphStatistics
    {
        public List<GraphData> Data;
    }

    public class GraphData
    {
        public string Key { get; set; }
        public int Value { get; set; }
    }
}
