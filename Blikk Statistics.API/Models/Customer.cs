using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blikk_Statistics.API.Models
{
    public class Customer
    {
        public string Date { get; set; }
        public string Name { get; set; }
        public bool IsChurn { get; set; }
        public string BelongsTo { get; set; }
        public string Edition { get; set; }
    }
}
