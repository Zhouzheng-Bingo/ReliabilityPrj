using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HNCAPI_INTERFACE;

namespace DA_DST
{
    class GlobalData
    {
        private static GlobalData m_glbData;
        private HncApi m_machine;
        
        public GlobalData()
        {
            m_machine = null;
        }
        public static GlobalData Instance()
        {
            if (m_glbData == null)
            {
                m_glbData = new GlobalData();
            }

            return m_glbData;
        }

      
        public HNCAPI_INTERFACE.HncApi Machine
        {
            get { return m_machine; }
            set { m_machine = value; }
        }

        private Boolean m_isConnect;
        public System.Boolean IsConnect
        {
            get { return m_isConnect; }
            set { m_isConnect = value; }
        }
    }
}
