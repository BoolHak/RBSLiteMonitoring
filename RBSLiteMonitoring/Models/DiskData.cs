using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBSLiteMonitoring.Models
{
    public class DiskData
    {
        public string FileSystem { get; set; }
        public string Size { get; set; }
        public string Used { get; set; }
        public string Available { get; set; }
        public string Use { get; set; }
        public string MontedOn { get; set; }
    }
}
