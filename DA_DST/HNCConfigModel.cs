using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DA_DST
{
    class HNCConfigModel
    {
        public HNCConfigModel()
        { 
        }

        /// <summary>
        /// 地址所在区域
        /// </summary>
        private string addressType;

        public string AddressType
        {
            get { return addressType; }
            set { addressType = value; }
        }

        /// <summary>
        /// 机床号,HNC本地地址配置中，不需要每台机床具体的机床编号
        /// </summary>
        /*
        private string addressNumber;

        public string AddressNumber
        {
            get { return addressNumber; }
            set { addressNumber = value; }
        }
        **/

        /// <summary>
        /// 本地地址
        /// </summary>
        private string localip;

        public string LocalIp
        {
            get { return localip; }
            set { localip = value; }
        }

        /// <summary>
        /// 本地端口
        /// </summary>
        private string localport;

        public string LocalPort
        {
            get { return localport; }
            set { localport = value; }
        }

        /// <summary>
        /// 作者
        /// </summary>
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
