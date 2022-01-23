using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBSLiteMonitoring.Models
{
    public class RamData
    {
        public long Total { get; set; }
        public long Used { get; set; }
        public long Cache { get; set; }
        public long Swap { get; set; }
        public long Boot { get; set; }
    }
}
