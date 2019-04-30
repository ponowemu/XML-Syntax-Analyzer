using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLAnalyzer
{
    class Tag
    {
        public int TagId {get;set;}
        public string TagName { get; set; }
        public bool HasAttribute { get; set; }
        public int ChildLevel { get; set; }
        public Attribute Attribute { get; set; }
        public Tag Parent { get; set; }
        public bool TagType { get; set; }
        public Text Text { get; set; }
    }
}
