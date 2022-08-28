using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLToolApp.Util
{
    public class RouteInfo
    {
        public string Domain { get; set; }
        public string Method { get; set; }
        public string URI { get; set; }
        public string Middleware { get; set; }
        public string Name { get; set; }
        public string Action { get; set; }
    }
}
