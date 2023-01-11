using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 监控数据.Models
{
    public class SqlQueryModel
    {
        public string fname { get; set; }
        public string initialization { get; set; }
        public string query { get; set; }
        public string remark { get; set; }
        public string useflag { get; set; }
        public string WEB_HOOK { get; set; }
        public string secret { get; set; }
    }
}
