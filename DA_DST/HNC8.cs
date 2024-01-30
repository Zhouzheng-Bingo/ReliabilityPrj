using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CONSTDEFINE;
using DA_DST;
using System.Threading;
using System.Runtime.InteropServices;
using HNCAPI_INTERFACE;



namespace DA_DST
{    
    class HNC8 : NCMachine
    {

        public HNC8(string _machnum, string _localip, int _localport, string _initName, string _ip, int _port, int _itype)
            :base(_machnum, _localip, _localport, _initName, _ip, _port, _itype)
        {

        }
        
        #region 连接函数        
        public override int ConnectToNC(string ip,string username,string password)
        {
            Int32 err = -1;
            HncApi machine = new HncApi();
            // err = machine.HNC_NetInit("10.10.56.66", 9090, "CSharpTest"); //NetInit localip  返回-1，失败；返回0，成功；
            err = machine.HNC_NetInit(localip, (ushort)localport, hncInitName); //NetInit localip  返回-1，失败；返回0，成功；
            if (err == 0) 
            {
                GlobalData.Instance().Machine = machine;
                try
                {
                    Int32 machineNo = -1;
                    machineNo = GlobalData.Instance().Machine.HNC_NetConnect(ip, (ushort)port);//, OnConnect); //返回-1，连接失败；返回0~255(机器号)，连接成功。
                    if (machineNo < 0)
                    {
                        status = CONSTDEFINE.MachStatus.MS_OFFLINE;//离线状态
                        err = -1;
                    }

                    else
                    {
                        status = CONSTDEFINE.MachStatus.MS_ONLINE;//在线状态
                        err = 0;
                    }
                }
                catch (Exception ex)
                {
                    err = -1;
                   
                }
            }
            else
            {
                err = -1;//本地初始化失败        
                Log.WriteLogErrMsg("华中连接函数问题，61行");
            }
            return err;        
        }

        #endregion

        private void OnConnect(String ip, UInt16 port, Boolean result)
        {
            if (result)
            {
                GlobalData.Instance().IsConnect = true;
                //Console.WriteLine("委托返回结果：连接成功！");
            }
            else
            {
                //Console.WriteLine("委托返回结果：连接超时！");
            }
        }

        #region 断开连接函数
        public override int DisConnectToNC()
        {
            int err = -1;
            try
            {
                if (GlobalData.Instance().Machine != null) 
                {
                    GlobalData.Instance().Machine.HNC_NetExit();
                }                  
                err = 0;
                Thread.Sleep(100);//后加的
            }
            catch (Exception ex)
            {
                err = -1;
            }
            return err;
        }
        #endregion

        #region 重连函数
        public override int ReConnectToNC(string ip, string username, string password)
        {
            int err;

            try
            {
                err = DisConnectToNC();
                Thread.Sleep(200);
                err = ConnectToNC(ip, username,password);
             
            }
            catch (Exception ex)
            {
                err = -1;
            }

            return err;
        }
        #endregion

        #region 读机床状态
        public override int UpdateData()
        {
            sendData = 1;
            int ret = -1;
            SChanVals chan_info = new SChanVals();
            if (0 != UpdateComm())
                ret = -1;
            if(ConstDefine.PingIpOrDomainName(this.ip)==true)
            {
                if (status == MachStatus.MS_ONLINE)
                {
                    try
                    {
                        Int32 ch = 0;         //获得当前通道
                        if (0 == GlobalData.Instance().Machine.HNC_SystemGetValue((Int32)HncSystem.HNC_SYS_ACTIVE_CHAN, ref ch))
                        {
                            ;//this.mach_info.activate_chan = ch ;
                        }
                        string str = "";        //获得nc系统版本号 
                        if (0 == GlobalData.Instance().Machine.HNC_SystemGetValue((Int32)HncSystem.HNC_SYS_NC_VER, ref str))
                        {
                            this.mach_info.softversion = str;
                        }
                        int runstatus_estop = -1;
                        if (0 == GlobalData.Instance().Machine.HNC_ChannelGetValue((Int32)HncChannel.HNC_CHAN_IS_ESTOP, 0, 0, ref runstatus_estop))
                        {
                            ;
                        }
                        int runstatus_channel = -1;
                        if (0 == GlobalData.Instance().Machine.HNC_ChannelGetValue((Int32)HncChannel.HNC_CHAN_IS_RUNNING, 0, 0, ref runstatus_channel))
                        {
                            ;
                        }
                        if (0 == GlobalData.Instance().Machine.HNC_ChannelGetMultiValues(0, ref chan_info))
                        {
                            //运行模式
                            //1auto2mdi3jog4teachin 5repos6ref7edit8handle9remoto 10step 11unknown12手轮
                            switch (chan_info.chanMode)
                            {
                                case 1://自动
                                    this.mach_info.disp_mode = 1;
                                    break;
                                case 2://手动
                                    this.mach_info.disp_mode = 8;
                                    break;
                                case 4://手轮
                                    this.mach_info.disp_mode = 12;
                                    break;
                                case 5://回零
                                    this.mach_info.disp_mode = 6;
                                    break;
                                case 7://MDI
                                    this.mach_info.disp_mode = 2;
                                    break;
                                default:
                                    this.mach_info.disp_mode = 11;
                                    break;
                            }
                            //运行状态
                            if (chan_info.isCycle == 1 || chan_info.isRunning == 1)
                            {
                                this.mach_info.interp_state = 1;//运行
                            }
                            else
                            {
                                this.mach_info.interp_state = 2;//其他都是停止
                            }
                            //任务状态 1运行中2急停3保持4回零5mdi6移动7复位8运行中
                            if (chan_info.isCycle == 1) this.mach_info.task_state = 1;
                            else if (chan_info.isEstop == 1) this.mach_info.task_state = 2;
                            else if (chan_info.isHold == 1) this.mach_info.task_state = 3;
                            else if (chan_info.isHoming == 1) this.mach_info.task_state = 4;
                            else if (chan_info.isMdi == 1) this.mach_info.task_state = 5;
                            else if (chan_info.isMoving == 1) this.mach_info.task_state = 6;
                            else if (chan_info.isReseting == 1) this.mach_info.task_state = 7;
                            else if (chan_info.isRunning == 1) this.mach_info.task_state = 8;
                            //轴数
                            this.mach_info.axis_num = chan_info.chanAxisName.Count();
                            //进给实际值
                            this.mach_info.afeed_speed = chan_info.actFeedRate;
                            //进给编程值
                            this.mach_info.feed_speed = chan_info.cmdFeedRate;
                            //进给修调
                            this.mach_info.dfeed_speed = chan_info.feedOverride;
                            //主轴实际值
                            this.mach_info.aspindle_speed = chan_info.actSpdlSpeed[0];
                            //主轴编程值
                            this.mach_info.spindle_speed = chan_info.cmdSpdlSpeed[0];
                            //主轴修调值
                            this.mach_info.dspindle_speed = chan_info.spdlOverride[0];
                            //当前加工工件数量
                            this.mach_info.work_piece = chan_info.partCntr;
                            //当前程序名
                            string str_progname = System.Text.Encoding.ASCII.GetString(chan_info.progName);//测试
                            int end = str_progname.IndexOf("\0");
                            this.mach_info.prog_name = str_progname.Substring(0, end);

                            //this.mach_info.prog_name = System.Text.Encoding.ASCII.GetString(chan_info.progName);
                            //当前程序行号
                            this.mach_info.current_line = chan_info.runRow;//测试                                       

                            //机床运行时间

                            //机床切削时间     

                            int toolNum = 0, toolstartno = 0;    //获取刀具数目
                            GlobalData.Instance().Machine.HNC_ToolGetSysToolNum(ref toolstartno, ref toolNum);
                            this.mach_info.tool_id = toolNum;
                            int Act_time = 0;    //刀具实际切削时间
                            GlobalData.Instance().Machine.HNC_ToolGetToolPara(1, 0, ref Act_time);
                            this.mach_info.run_time = Act_time;
                            int Chan_Tool_use1 = 0;//当前使用刀具号
                            if (0 == GlobalData.Instance().Machine.HNC_ChannelGetValue((Int32)HncChannel.HNC_CHAN_TOOL_USE, 0, 0, ref Chan_Tool_use1))
                                this.mach_info.tool_num = Chan_Tool_use1;
                            double useless = 0; //刀具所属类型
                            if (0 == GlobalData.Instance().Machine.HNC_ToolGetToolPara(1, (Int32)ToolParaIndex.INFTOOL_TYPE, ref useless))
                                this.mach_info.tool_radius = useless;
                            int Use_num1 = 0;//刀具使用次数
                            if (0 == GlobalData.Instance().Machine.HNC_ToolGetToolPara(1, (Int32)ToolParaIndex.MOTOOL_CNT_UNIT1, ref Use_num1))
                                this.mach_info.tool_usenum = Use_num1;

                        }
                        //轴坐标编程值、轴坐标剩余值
                        Double AXIS_CMD_POS = 0;
                        Double AXIS_LEFT_TOGO = 0;
                        for (int i = 0; i < mach_info.axis_num; i++)
                        {
                            if (0 == GlobalData.Instance().Machine.HNC_AxisGetValue((Int32)HncAxis.HNC_AXIS_ACT_POS, i, ref AXIS_CMD_POS))
                            {
                                switch (i)
                                {
                                    case 0: this.mach_info.ahost_value1 = AXIS_CMD_POS; break;
                                    case 1: this.mach_info.ahost_value2 = AXIS_CMD_POS; break;
                                    case 2: this.mach_info.ahost_value3 = AXIS_CMD_POS; break;
                                    case 3: this.mach_info.ahost_value4 = AXIS_CMD_POS; break;
                                    case 4: this.mach_info.ahost_value5 = AXIS_CMD_POS; break;
                                    case 5: this.mach_info.ahost_value6 = AXIS_CMD_POS; break;
                                    default: break;
                                }
                            }
                            if (0 == GlobalData.Instance().Machine.HNC_AxisGetValue((Int32)HncAxis.HNC_AXIS_LEFT_TOGO, i, ref AXIS_LEFT_TOGO))
                            {
                                switch (i)
                                {
                                    case 0: this.mach_info.dist_togo1 = AXIS_LEFT_TOGO; break;
                                    case 1: this.mach_info.dist_togo2 = AXIS_LEFT_TOGO; break;
                                    case 2: this.mach_info.dist_togo3 = AXIS_LEFT_TOGO; break;
                                    case 3: this.mach_info.dist_togo4 = AXIS_LEFT_TOGO; break;
                                    case 4: this.mach_info.dist_togo5 = AXIS_LEFT_TOGO; break;
                                    case 5: this.mach_info.dist_togo6 = AXIS_LEFT_TOGO; break;
                                    default: break;
                                }
                            }
                        }
                        //轴扭矩值
                      
                        if (0 == GlobalData.Instance().Machine.HNC_AxisGetValue((Int32)HncAxis.HNC_AXIS_LOAD_CUR, 5, ref AXIS_LEFT_TOGO))
                        {
                            this.mach_info.spin_torq = AXIS_LEFT_TOGO;
                        }

                        SDataUnion wheel_ride = new SDataUnion(); //砂轮修整量
                        if (0 == GlobalData.Instance().Machine.HNC_MacroVarGetValue(51223, ref wheel_ride))
                            this.mach_info.wheel_ride = Convert.ToDouble(wheel_ride.v.f.ToString());

                        int progOver = 0;
                        this.mach_info.programmed_over = "2"; //程序运行结束信号 程序启动为0，程序结束为1 异常为2
                        if (0 == GlobalData.Instance().Machine.HNC_RegGetValue(2, 2561, ref progOver)) //2代表F寄存器 progOver=F(2561)
                        {
                            string temp = Convert.ToString(progOver, 2);
                            if (temp.Length < 7)
                                this.mach_info.programmed_over = "0";
                            else
                                this.mach_info.programmed_over = Convert.ToString(temp[temp.Length - 7]);
                        }

                        mach_info.error_flag = 0;

                        /// 下边这段故障检测有问题
                        //GlobalData.Instance().Machine.HNC_AlarmSubscribe(false);//订阅

                        //Int32 alarmNum = 0;
                        //Int32 rtn = GlobalData.Instance().Machine.HNC_AlarmGetNum(ref alarmNum);
                        //if (this.Hnc_AlmList != null)
                        //{
                        //    this.Hnc_AlmList.Clear();
                        //}
                        //else { ;}
                        
                        //for (Int16 i = 0; i < alarmNum; i++)
                        //{
                        //    Int32 alarmNo = 0;
                        //    String alarmText = String.Empty;
                        //    ret = GlobalData.Instance().Machine.HNC_AlarmGetData(i, ref alarmNo, ref alarmText);
                        //    if (ret != 0) continue;
                        //    String[] arrStr = { alarmNo.ToString(), alarmText };
                        //    this.Hnc_AlmList.Add(arrStr);
                        //}

                    }
                    catch (Exception ex)
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    ret = 0;
                }
            }
            else
            {
                status = MachStatus.MS_OFFLINE;
                ret = -1;
            }
           
            return ret;
        }
        #endregion
    }
}
