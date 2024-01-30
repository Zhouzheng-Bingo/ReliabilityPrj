using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DA_DST
{
    public class MachineModel
    {
        public MachineModel() 
        { 
        }

        /// <summary>
        /// 机床类型
        /// </summary>
        private string machineType;

        public string MachineType
        {
            get { return machineType; }
            set { machineType = value; }
        }

        /// <summary>
        /// 机床编号
        /// </summary>
        private string machineNumber;

        public string MachineNumber
        {
            get { return machineNumber; }
            set { machineNumber = value; }
        }

        /// <summary>
        /// 运行模式
        /// </summary>
        private string disp_mode;

        public string Disp_Mode
        {
            get { return disp_mode; }
            set { disp_mode = value; }
        }

        /// <summary>
        /// 机床状态
        /// </summary>
        private string interp_state;

        public string Interp_State
        {
            get { return interp_state; }
            set { interp_state = value; }
        }


    }
}
