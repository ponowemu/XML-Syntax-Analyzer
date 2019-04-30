using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLAnalyzer
{
    class Error
    {
        public int LineNumber { get; set; }
        public string ErrorName { get; set; }
        public string ErrorValue { get; set; }
        public bool Warning { get; set; }
    }
}
