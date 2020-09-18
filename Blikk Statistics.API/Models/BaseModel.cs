using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blikk_Statistics.API.Models
{
    public class BaseModel
    {
        public int Count { get; set; }
        public int BlikkCount { get; set; }
        public int ResellerCount { get; set; }
        public int VismaCount { get; set; }
        public int VismaCountNew { get; set; }
        public int VismaCountLost { get; set; }

        public int DocumentsTotal { get; set; }
        public int EasyTotal { get; set; }
        public int BusinessTotal { get; set; }
        public int ProTotal { get; set; }
        public int VismaSmartTotal { get; set; }
        public int VismaProTotal { get; set; }

        public int DocumentsStandard { get; set; }
        public int EasyStandard { get; set; }
        public int BusinessStandard { get; set; }
        public int ProStandard { get; set; }
        public int VismaSmartStandard { get; set; }
        public int VismaProStandard { get; set; }

        public int DocumentsBasic { get; set; }
        public int EasyBasic { get; set; }
        public int BusinessBasic { get; set; }
        public int ProBasic { get; set; }
        public int VismaProBasic { get; set; }
        public int VismaSmartBasic{ get; set; }

        public int DocumentsLight { get; set; }
        public int EasyLight { get; set; }
        public int BusinessLight { get; set; }
        public int ProLight { get; set; }
        public int VismaSmartLight { get; set; }
        public int VismaProLight { get; set; }

        public List<Customer> CustomerList { get; set; }
        public GraphStatistics GraphStatistics { get; set; }
    }
}