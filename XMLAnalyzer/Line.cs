﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLAnalyzer
{
    class Line
    {
        public int LineNumber { get; set; }
        public bool HasError { get; set; }
        public string Content { get; set; }
    }
}