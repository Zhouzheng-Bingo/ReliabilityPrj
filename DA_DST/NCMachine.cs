using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CONSTDEFINE;
using Opc.Ua;
using System.Threading;
using System.Net;
using System.IO;

namespace DA_DST
{
    
    class NCMachine
    {
        #region 类属性
        public string ip,localip = null;
	    public string machnum = null;
        public int port, itype, localport;
        public string hncInitName = null;

        public MachStatus status;//机器的连接状态  MS_INIT=-1,MS_OFFLINE=0,MS_ONLINE=1,MS_STOP=2
        public MachStatus status_last;//机器的连接状态  MS_INIT=-1,MS_OFFLINE=0,MS_ONLINE=1,MS_STOP=2
        public ConnectStatus connectstatus;   //连接状态 0：已连上，1连接中
        public machine_info_t mach_info = new machine_info_t();
        public IsAcquire isacquire;
        /// <summary>
        /// 时间信息
        /// </summary>
        public long today_poweron_time;
        public long today_idle_time;
        public long today_error_time;
        public long today_run_time;
        public long today_run_time_meet_setting;
        public long today_offline_time;
        public long today_cut_time;
        public long day_poweron_time;
        public long day_run_time;
        public long day_cut_time;
        /// <summary>
        /// 高低速报警
        /// </summary>
        public int Slarge;
        public int Ssmall;
        public int Flarge;
        public int Fsmall;
        public double Llarge;
        public double Lsmall;
        public int runstatus;  //1:符合运行条件；0：不符合运行条件；2：停止
        public long alarm_fstartime;
        public long alarm_fendtime;
        public long alarm_flastime;
        public long alarm_sstartime;
        public long alarm_sendtime;
        public long alarm_slastime;
        /// <summary>
        /// 有效工时
        /// </summary>
        public string s_userid_last, s_userid_now;
        public bool zero_flag;
        /// <summary>
        /// 更新时间
        /// </summary>
        public long delta_time;
        public long now_time;
        public long temp_delta_time;
        /// <summary>
        /// 界面故障flag
        /// </summary>
        public int flag_error_state;
        /// <summary>
        /// 界面运行设置flag
        /// </summary>
        public int set_flag;//1:符合运行条件，0：不符合运行条件
        /// <summary>
        /// 甘特图
        /// </summary>
        public int i_machstatus; //0停止1运行3开机-1离线
        public long machstatus_starttime;//某状态的开始时间
        public long machstatus_endtime;//某状态的结束时间
        public short machstatus_flag;//某状态的标识 0开始1结束
        
        /// <summary>
        /// Golding数控系统
        /// </summary>
        public string error_handle_last;
        public string error_motion_last;
        public string error_plc_last;
        public int error_type;//故障类型  1:handle 2:motion 3:plc 0无故障
        public int ErrorStartTime_handle;
        public int ErrorEndTime_handle;
        public int ErrorStartTime_motion;
        public int ErrorEndTime_motion;
        public int ErrorStartTime_plc;
        public int ErrorEndTime_plc;

        /// <summary>
        /// Rexroth、秦川数控系统,共用变量,两者根据不同的实例进行区分，Rexroth暂未给error接口
        /// </summary>
        public int error_last;
        public int ErrorStartTime;
        public int ErrorEndTime;

        /// <summary>
        /// HNC数控系统
        /// </summary>
        public List<HncFaultVector> HncnowFaultList = new List<HncFaultVector>();
        public List<HncFaultVector> HnclastFaultList = new List<HncFaultVector>();
        public List<HncFaultVector> HncuendFaultList = new List<HncFaultVector>();
        public List<HncFaultVector> HnctempFaultList = new List<HncFaultVector>();
        public List<HncFaultVector> HncendFaultList = new List<HncFaultVector>();
        public List<HncFaultVector> HncnewFaultList = new List<HncFaultVector>();
        public List<string[]> Hnc_AlmList;

        //订阅成功标识
        public Subscri m_sub = new Subscri();

        public ushort Flibhndl;
        //多线程的相关变量
        public ManualResetEvent mre = new ManualResetEvent(false);
        //设备连接状态变化标识 0没变化1有变化 在界面的系统信息输出变化
        public int flag_statuschange;

        /// <summary>
        /// 程序日志
        /// </summary>
        public int progflag;
        public string prognameLast;
        public int interpLast;
        public int currentlineLast;
        public long proglog_starttime;
        public long proglog_endtime;
        public int workpiceclast;
        /// <summary>
        /// webservice所用字典
        /// </summary>
        public Dictionary<string, string> webdic = new Dictionary<string, string>();

        public int sendData = 0;

        #endregion

        public NCMachine()
        {}

        public NCMachine(string _machnum,string _ip,int _port,int _itype)
        {
            machnum = _machnum;
            ip = _ip;
            port = _port;
            itype = _itype;
            status = MachStatus.MS_INIT;
            connectstatus = ConnectStatus.MACH_CONNECTED;
            ClearData();
            today_poweron_time = 0;
            today_run_time = 0;
            today_error_time = 0;
            today_cut_time = 0;
            today_run_time_meet_setting = 0;
            today_idle_time = 0;
            today_offline_time = 0;
            Slarge = 0;
            Ssmall = 0;
            Flarge = 0;
            Fsmall = 0;
            Llarge = 0;
            Lsmall = 0;
            runstatus = -1;
            alarm_fstartime = 0;
            alarm_fendtime = 0;
            alarm_flastime = 0;
            alarm_sstartime = 0;
            alarm_sendtime = 0;
            alarm_slastime = 0;
            zero_flag = false;
            now_time = 0;
            delta_time = 0;
            i_machstatus = -1;
            machstatus_starttime = -1;
            machstatus_endtime = -1;
            machstatus_flag = -1;
            set_flag = -1;
            error_last = -1;
            ErrorStartTime = -1;
            ErrorEndTime = -1;
            isacquire = IsAcquire.No;
            temp_delta_time = 0;
            error_plc_last = "";
            error_motion_last = "";
            error_handle_last = "";
            //error_type = 0;
            ErrorStartTime_handle = -1;
            ErrorStartTime_motion = -1;
            ErrorStartTime_plc = -1;
            ErrorEndTime_handle = -1;
            ErrorEndTime_motion = -1;
            ErrorEndTime_plc = -1;
            flag_statuschange = 0;
            status_last = MachStatus.MS_INIT;

            interpLast = -1;
            currentlineLast = -1;
            progflag = -1;
            prognameLast = "";
            proglog_starttime = 0;
            proglog_endtime = 0;
            workpiceclast = -1;
            sendData = 0;
            InitialWebDic();
        }

        public NCMachine(string _machnum, string _localip, int _localport, string _hncInitName, string _ip, int _port, int _itype)
        {
            machnum = _machnum;
            localip = _localip;
            ip = _ip;
            port = _port;
            localport = _localport;
            hncInitName = _hncInitName;
            itype = _itype;
            status = MachStatus.MS_INIT;
            connectstatus = ConnectStatus.MACH_CONNECTED;
            ClearData();
            today_poweron_time = 0;
            today_run_time = 0;
            today_error_time = 0;
            today_cut_time = 0;
            today_run_time_meet_setting = 0;
            today_idle_time = 0;
            today_offline_time = 0;
            Slarge = 0;
            Ssmall = 0;
            Flarge = 0;
            Fsmall = 0;
            Llarge = 0;
            Lsmall = 0;
            runstatus = -1;
            alarm_fstartime = 0;
            alarm_fendtime = 0;
            alarm_flastime = 0;
            alarm_sstartime = 0;
            alarm_sendtime = 0;
            alarm_slastime = 0;
            zero_flag = false;
            now_time = 0;
            delta_time = 0;
            i_machstatus = -1;
            machstatus_starttime = -1;
            machstatus_endtime = -1;
            machstatus_flag = -1;
            set_flag = -1;
            error_last = -1;
            ErrorStartTime = -1;
            ErrorEndTime = -1;
            isacquire = IsAcquire.No;
            temp_delta_time = 0;
            error_plc_last = "";
            error_motion_last = "";
            error_handle_last = "";
            //error_type = 0;
            ErrorStartTime_handle = -1;
            ErrorStartTime_motion = -1;
            ErrorStartTime_plc = -1;
            ErrorEndTime_handle = -1;
            ErrorEndTime_motion = -1;
            ErrorEndTime_plc = -1;
            flag_statuschange = 0;
            status_last = MachStatus.MS_INIT;

            interpLast = -1;
            currentlineLast = -1;
            progflag = -1;
            prognameLast = "";
            proglog_starttime = 0;
            proglog_endtime = 0;
            workpiceclast = -1;
            sendData = 0;
            InitialWebDic();
        }

        public void InitialWebDic()
        {
            webdic.Add("now_time", "");
            webdic.Add("delta_time", "");
            webdic.Add("INTERP_STATE", "");
            webdic.Add("DISP_MODE", "");
            webdic.Add("HOST_VALUE1", "");
            webdic.Add("HOST_VALUE2", "");
            webdic.Add("HOST_VALUE3", "");
            webdic.Add("HOST_VALUE4", "");
            webdic.Add("HOST_VALUE5", "");
            webdic.Add("HOST_VALUE6", "");
            webdic.Add("HOST_VALUE7", "");
            webdic.Add("HOST_VALUE8", "");
            webdic.Add("HOST_VALUE9", "");
            webdic.Add("AHOST_VALUE1", "");
            webdic.Add("AHOST_VALUE2", "");
            webdic.Add("AHOST_VALUE3", "");
            webdic.Add("AHOST_VALUE4", "");
            webdic.Add("AHOST_VALUE5", "");
            webdic.Add("AHOST_VALUE6", "");
            webdic.Add("AHOST_VALUE7", "");
            webdic.Add("AHOST_VALUE8", "");
            webdic.Add("AHOST_VALUE9", "");
            webdic.Add("DIST_TOGO1", "");
            webdic.Add("DIST_TOGO2", "");
            webdic.Add("DIST_TOGO3", "");
            webdic.Add("DIST_TOGO4", "");
            webdic.Add("DIST_TOGO5", "");
            webdic.Add("DIST_TOGO6", "");
            webdic.Add("ACT_AXIS_VELOCITY1", "");
            webdic.Add("ACT_AXIS_VELOCITY2", "");
            webdic.Add("ACT_AXIS_VELOCITY3", "");
            webdic.Add("ACT_AXIS_VELOCITY4", "");
            webdic.Add("ACT_AXIS_VELOCITY5", "");
            webdic.Add("ACT_AXIS_VELOCITY6", "");
            webdic.Add("ACT_AXIS_VELOCITY7", "");
            webdic.Add("ACT_AXIS_VELOCITY8", "");
            webdic.Add("ACT_AXIS_VELOCITY9", "");
            webdic.Add("FEED_SPEED", "");
            webdic.Add("AFEED_SPEED", "");
            webdic.Add("DFEED_SPEED", "");
            webdic.Add("SPINDLE_SPEED", "");
            webdic.Add("ASPINDLE_SPEED", "");
            webdic.Add("DSPINDLE_SPEED", "");
            webdic.Add("SPIN_TORQ", "");
            webdic.Add("PROG_NAME", "");
            webdic.Add("CURRENT_LINE", "");
            webdic.Add("WORK_PIECE", "");
            webdic.Add("AXIS_NUM", "");
            webdic.Add("ERROR_ID", "");
            webdic.Add("total_poweron_time", "");
            webdic.Add("total_run_time", "");
            webdic.Add("total_cut_time", "");
            webdic.Add("day_poweron_time", "");
            webdic.Add("day_run_time", "");
            webdic.Add("day_cut_time", "");
            webdic.Add("SOFTVERSION", "");
            webdic.Add("RUN_TIME", "");
            webdic.Add("MACHINE_ONLINE", "");        
        }
        #region 清空结构体函数
        public void ClearData()
        {
            mach_info.act_axis_velocity1 = 0;
            mach_info.act_axis_velocity2 = 0;
            mach_info.act_axis_velocity3 = 0;
            mach_info.act_axis_velocity4 = 0;
            mach_info.act_axis_velocity5 = 0;
            mach_info.act_axis_velocity6 = 0;
            mach_info.act_axis_velocity7 = 0;
            mach_info.act_axis_velocity8 = 0;
            mach_info.act_axis_velocity9 = 0;
            mach_info.afeed_speed = 0;
            mach_info.ahost_value1 = 0;
            mach_info.ahost_value2 = 0;
            mach_info.ahost_value3 = 0;
            mach_info.ahost_value4 = 0;
            mach_info.ahost_value5 = 0;
            mach_info.ahost_value6 = 0;
            mach_info.ahost_value7 = 0;
            mach_info.ahost_value8 = 0;
            mach_info.ahost_value9 = 0;
            mach_info.alarmText = "";
            mach_info.aspindle_speed = 0;
            mach_info.axis_num = 0;
            mach_info.avail_axis = "";
            mach_info.current_line = 0;
            mach_info.cut_time = 0;
            mach_info.dfeed_speed = 0;
            mach_info.disp_mode = -1;
            mach_info.dist_togo1 = 0;
            mach_info.dist_togo2 = 0;
            mach_info.dist_togo3 = 0;
            mach_info.dist_togo4 = 0;
            mach_info.dist_togo5 = 0;
            mach_info.dist_togo6 = 0;
            mach_info.dist_togo7 = 0;
            mach_info.dist_togo8 = 0;
            mach_info.dist_togo9 = 0;
            mach_info.dspindle_speed = 0;
            mach_info.equip_ip = "";
            mach_info.error_axisnum = 0;
            mach_info.error_flag = 0;
            mach_info.error_handle = "";
            mach_info.error_id = 0;
            mach_info.error_motion = "";
            mach_info.error_plc = "";
            mach_info.feed_speed = 0;
            mach_info.host_value1 = 0;
            mach_info.host_value2 = 0;
            mach_info.host_value3 = 0;
            mach_info.host_value4 = 0;
            mach_info.host_value5 = 0;
            mach_info.host_value6 = 0;
            mach_info.host_value7 = 0;
            mach_info.host_value8 = 0;
            mach_info.host_value9 = 0;
            mach_info.interp_state = -1;
            mach_info.mainfile_name = "";
            mach_info.mainFileTotalLine = 0;
            mach_info.Machine_type = "";
            mach_info.Machine_toolProbe = "";
            mach_info.poweron_time = 0;
            mach_info.plc_warningNumber = 0;
            mach_info.programmed_over = "";
            mach_info.prog_name = "";
            mach_info.run_time = 0;
            mach_info.softversion = "";
            mach_info.spin_torq = 0;
            mach_info.spindle_speed = 0;
            mach_info.subfile_name = "";
            mach_info.subFileTotalLine = 0;
            mach_info.sublevel = 0;
            mach_info.task_state = 0;
            mach_info.tool_id = 0;
            mach_info.tool_length = 0;
            mach_info.tool_life = 0;
            mach_info.tool_num = 0;
            mach_info.tool_radius = 0;
            mach_info.tool_usenum = 0;
            mach_info.today_cut_time = 0;
            mach_info.today_poweron_time = 0;
            mach_info.today_run_time = 0;
            mach_info.total_cut_time = 0;
            mach_info.total_poweron_time = 0;
            mach_info.total_run_time = 0;
            mach_info.work_piece = 0;
            mach_info.wheel_remaintime = 0;
            mach_info.wheel_ride = 0; 
        }
        #endregion

        #region 开启监听
        public virtual void Monitor(string _ip, int _port) 
        { 
        }
        #endregion

        #region 连接函数
        public virtual int ConnectToNC(string ip,string username,string password)
        {
            return -1;
        }
        #endregion

        #region 读机床状态
        public virtual int UpdateData()
        {
            return -1;
        }
        #endregion

        #region 断开连接函数
        public virtual int DisConnectToNC()
        {
            return -1;
        }
        #endregion

        #region 重新连接函数
        public virtual int ReConnectToNC(string ip, string username, string password)
        {
            return -1;
        }
        #endregion

        #region 判断连接状态并做相应处理函数
        public int UpdateComm()
        {
            int ret = -1;
            long temp_time = 0;
            temp_time = GetCurrentTimeUnix();
            if(now_time == 0)//解决初始化时出现delta_time为当前时间戳的情况出现
            {
                delta_time = 0;
            }
            else
            {
                delta_time = temp_time - now_time;
            }
            
            temp_delta_time = temp_time - now_time;
            if (temp_delta_time > 0)
            {
                now_time = temp_time;
                temp_delta_time = 0;
            }           
            if(status == MachStatus.MS_INIT)
            {
                bool rtn = ConstDefine.PingIpOrDomainName(ip);
                if(rtn == true)
                {
                    ret = this.ConnectToNC(ip, "opcua", "opcua");
                    if (ret == 0)
                    {
                        this.status = MachStatus.MS_ONLINE; 
                    }
                    else
                    {
                        if (this.itype == (int)MachineType.HNC8)
                        {
                            this.HnclastFaultList.Clear();
                            this.HncnewFaultList.Clear();
                            this.HncnowFaultList.Clear();
                            this.HncendFaultList.Clear();
                            this.HncuendFaultList.Clear();
                        }
                        this.status = MachStatus.MS_OFFLINE;
                    }
                }
                else
                {
                    if (this.itype == (int)MachineType.HNC8)
                    {
                        this.HnclastFaultList.Clear();
                        this.HncnewFaultList.Clear();
                        this.HncnowFaultList.Clear();
                        this.HncendFaultList.Clear();
                        this.HncuendFaultList.Clear();
                    }
                    this.status = MachStatus.MS_OFFLINE;
                }                                   
            }
            else if(status == MachStatus.MS_ONLINE)
            {
                ret = 0;
            }
            else if(status == MachStatus.MS_OFFLINE)
            {
                bool rtn = ConstDefine.PingIpOrDomainName(ip);
                if (rtn == true)
                {
                    ret = this.ConnectToNC(ip, "opcua", "opcua");
                    if (ret == 0)
                    {
                        this.status = MachStatus.MS_ONLINE;
                    }
                    else
                    {
                        if (this.itype == (int)MachineType.HNC8)
                        {
                            this.HnclastFaultList.Clear();
                            this.HncnewFaultList.Clear();
                            this.HncnowFaultList.Clear();
                            this.HncendFaultList.Clear();
                            this.HncuendFaultList.Clear();
                        }
                        this.status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                }
                else
                {
                    /*
                     * 
                     此段张哥给的山东华中采集程序貌似忘了添加,NCMachine.cs中459-466
                     */
                    if (this.itype == (int)MachineType.HNC8)
                    {
                        this.HnclastFaultList.Clear();
                        this.HncnewFaultList.Clear();
                        this.HncnowFaultList.Clear();
                        this.HncendFaultList.Clear();
                        this.HncuendFaultList.Clear();
                    }
                    this.status = MachStatus.MS_OFFLINE;
                    ret = -1;
                }                                   
               
            }
            return ret;   
        }
        #endregion

        #region 获取时间戳函数
        public long GetCurrentTimeUnix()
        {
            TimeSpan cha = (System.DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
            long t = (long)cha.TotalSeconds;
            return t;
        }
        #endregion

        #region 延迟连接
        public void DelayConnect()
        {
            int ret = 0;
                      
            if(status == MachStatus.MS_OFFLINE)
            {         
                if (itype > (int)MachineType.MOXA1242 && itype < 4000)//moxa不重新初始化socket
                {
                    ret = this.ReConnectToNC(this.ip,"opcua","opcua");                  
                }
                else
                {
                    ret = this.DisConnectToNC();
                    Thread.Sleep(500);
                    if (this.itype == (int)MachineType.HNC8)
                    {
                        this.HnclastFaultList.Clear();
                        this.HncnewFaultList.Clear();
                        this.HncnowFaultList.Clear();
                        this.HncendFaultList.Clear();
                        this.HncuendFaultList.Clear();
                    }
                }
                DisConnectToNC();
                System.Threading.Thread.Sleep(100);
                ret = ConnectToNC(ip,"opcua","opcua");
                if(ret == 0)
                {
                    status = MachStatus.MS_ONLINE;
                }
                if (ret == 0)//出现多次反复连接后再也无法连接的情况，需要重启动设备，处理方法为反复的断开连接断开连接成功再连接
                {
                    ret = this.ConnectToNC(this.ip,"opcua","opcua");
                    status = MachStatus.MS_ONLINE;
                }
                else if (this.itype >= (int)MachineType.FANUC_MD && this.itype <= (int)MachineType.FANUC_MD_MATE)
                {
                    ret = this.ConnectToNC(this.ip, "opcua", "opcua");
                    status = MachStatus.MS_ONLINE;
                }
                else if (this.itype == (int)MachineType.HNC8)
                {
                    ret = this.ConnectToNC(this.ip, "opcua", "opcua");
                    status = MachStatus.MS_ONLINE;
                }
                else if (this.itype == (int)MachineType.QCMTT) 
                {
                    ret = this.ConnectToNC(this.ip, "opcua", "opcua");
                    status = MachStatus.MS_ONLINE;
                }
                else if (this.itype == (int)MachineType.REXROTH)
                {
                    ret = this.ConnectToNC(this.ip, "opcua", "opcua");
                    status = MachStatus.MS_ONLINE;
                }
            }

        }
        #endregion

    }

}
