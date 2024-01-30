using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DST
{
    class HncFaultVector
    {
        public HncFaultVector(int alm_no, string alm_msg, long start_time, long end_time, int flag)
        {
            this.alm_no = alm_no;
            this.alm_msg = alm_msg;
            this.start_time = start_time;
            this.end_time = end_time;
            this.flag = flag;
        }
        
        public int alm_no;
        public string alm_msg;
        public long start_time;
        public long end_time;
        int flag;
    }
}
