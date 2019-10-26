using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGB.UAService
{
    public class RequestWithdraw
    {
        public long order_id { get; set; }
        public int status { get; set; }
        public string notes { get; set; }
        public string updated_by { get; set; }
        public DateTime operating_time { get; set; }
    }

    public class RequestLock
    {
        public long order_id { get; set; }
        public bool exe_locked { get; set; }
    }

    public class RequestUkey
    {
        public long order_id { get; set; }
        public bool show_ukey { get; set; }
    }
}
