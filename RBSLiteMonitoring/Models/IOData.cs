using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBSLiteMonitoring.Models
{
    public class IOData
    {
        public long NetIn { get; set; }
        public long NetOut { get; set; }
        public long DiskRead { get; set; }
        public long DiskWrite { get; set; }
    }
}
