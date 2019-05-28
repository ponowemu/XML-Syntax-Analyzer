using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLAnalyzer
{
    class Xml
    {
        public int LineNumber { get; set; }
        public int StartingTagsNumber { get; set; }
        public int EndingTagsNumber { get; set; }

        public string Content { get; set; }
        public bool HasError { get; set;}
        public bool HasRoot { get; set; }
        public bool HasXmlDec { get; set; }

        public List<Tag> AllTags { get; set; }
        public List<Tag> StartingTagsList { get; set; }
        public List<Tag> EndingTagsList { get; set; }
    }
}
