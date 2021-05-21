using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLToolApp.Util
{
    public class DatabaseConfigInfo
    {
        public string ReferencePrimaryTable { get; set; }
        public string ReferencePrimaryKey { get; set; }
        public string ReferenceForeignTable { get; set; }
        public string ReferenceForeignKey { get; set; }
    }
}
