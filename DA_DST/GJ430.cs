using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CONSTDEFINE;
using System.Threading;
using System.Runtime.InteropServices;
using GOLDING;

namespace DA_DST
{

    class GJ430 : NCMachine
    {
        int iNCIndex = -1;
        public GJ430(string _machnum, string _ip, int _port, int _itype)
            : base(_machnum, _ip, _port, _itype)
        {

        }

        #region 连接函数
        public override int ConnectToNC(string ip, string username, string password)
        {
            int ret = -1;
            try
            {
                Thread.Sleep(40000);
                if (ip.Equals("192.168.2.3"))
                {
                    Thread.Sleep(10000);
                }
                else if (ip.Equals("192.168.2.17"))
                {
                    Thread.Sleep(20000);
                }
                ret = Golding.connectToNC(ip, (ushort)port, ref iNCIndex);
                if (ret == 0)
                {
                    status = CONSTDEFINE.MachStatus.MS_ONLINE;
                    ret = 0;
                }
                else
                {
                    status = CONSTDEFINE.MachStatus.MS_OFFLINE;
                    ret = -1;
                }

            }
            catch (Exception ex)
            {
                status = CONSTDEFINE.MachStatus.MS_OFFLINE;
                Log.WriteLogErrMsg("高精连接函数问题，45行");
                ret = -1;
            }
            return ret;
        }

        #endregion

        #region 断开连接函数
        public override int DisConnectToNC()
        {
            int err = -1;
            try
            {
                err = Golding.disconnectToNC(iNCIndex);
                Thread.Sleep(100);//后加的
            }
            catch (Exception ex)
            {
                Console.WriteLine("高精关闭函数问题，64行");
                Log.WriteLogErrMsg("高精关闭函数问题，64行");
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
                err = Golding.disconnectToNC(iNCIndex);
                Thread.Sleep(200);
                err = Golding.connectToNC(ip, (ushort)port, ref iNCIndex);    //连接机器
                //connectToNC( string IPAddr,System.UInt16 Port,ref int iNCIndex );
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
            string retValueString = "";
            int retValueInt = -1;
            double retValueDouble = 0.0;
            double[] retValueArrayDouble = new double[16];
            StringBuilder buf = new StringBuilder(256);
            int ret = -1;
            int index = -1;
            GOLDING.Golding.DNC_TOOL_LIFE_TABLE toolLifeTable = new GOLDING.Golding.DNC_TOOL_LIFE_TABLE();
            GOLDING.Golding.DNC_TOOLOFFSET_STRUCT tooloffset = new GOLDING.Golding.DNC_TOOLOFFSET_STRUCT();
            if (0 != UpdateComm())
                ret = -1;
            if (status == MachStatus.MS_ONLINE)
            {
                try
                {

                    index = 2001;   //配置轴数

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusIntVal(iNCIndex, 0, index, ref retValueInt))
                        {
                            this.mach_info.axis_num = retValueInt;
                            ret = 0;
                        }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                   
                    index = 1004;//运行状态

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusIntVal(iNCIndex, 0, index, ref retValueInt))//0：就绪1：运行2：暂停3：保持
                        {
                            ret = 0;    
                        switch ((int)retValueInt)
                            {
                                case 0:
                                    this.mach_info.interp_state = 0;
                                    break;
                                case 1:
                                    this.mach_info.interp_state = 1;
                                    break;
                                case 2:
                                    this.mach_info.interp_state = 2;
                                    break;
                                case 3:
                                    this.mach_info.interp_state = 3;
                                    break;
                                default: this.mach_info.interp_state = 11; break;
                            }
                        }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }

                    index = 1003; //运行模式0：自动模式1：单步模式2：MDI 模式3：手动模式4：手轮模式5：回零模式

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusIntVal(iNCIndex, 0, index, ref retValueInt))
                        {
                            ret = 0;    
                        switch ((int)retValueInt)
                            {
                                case 0:
                                    this.mach_info.disp_mode = 0;
                                    break;
                                case 1:
                                    this.mach_info.disp_mode = 1;
                                    break;
                                case 2:
                                    this.mach_info.disp_mode = 2;
                                    break;
                                case 3:
                                    this.mach_info.disp_mode = 3;
                                    break;
                                case 4:
                                    this.mach_info.disp_mode = 4;
                                    break;
                                case 5:
                                    this.mach_info.disp_mode = 5;
                                    break;
                                default:
                                    this.mach_info.disp_mode = 6;
                                    break;
                            }
                        }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 8004;//主轴实际速度 --接口有问题

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusAxisVal(iNCIndex, 0, index, retValueArrayDouble))
                    //Console.WriteLine(retValueArrayDouble[0]);
                    {
                        this.mach_info.aspindle_speed = retValueArrayDouble[0];
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 8003;//主轴编程速度 --接口有问题

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusAxisVal(iNCIndex, 0, index, retValueArrayDouble))
                    {
                        this.mach_info.spindle_speed = retValueArrayDouble[0];
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    ////index = 8008;//主轴扭矩 -直接跳过不采集
                    ////if (0 == Golding.getStatusAxisVal(iNCIndex, 0, index, retValueArrayDouble))
                    ////    this.mach_info.spin_torq = retValueArrayDouble[0];
                    index = 8005;//主轴倍率

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusDoubleVal(iNCIndex, 0, index, ref retValueDouble))
                    {
                        this.mach_info.dspindle_speed = retValueDouble;
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 5002;//进给实际速度

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusDoubleVal(iNCIndex, 0, index, ref retValueDouble))
                    {
                        this.mach_info.afeed_speed = retValueDouble;
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 5001;//进给编程速度

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusDoubleVal(iNCIndex, 0, index, ref retValueDouble))
                    {
                        this.mach_info.feed_speed = retValueDouble;
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 5009;//进给倍率

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusDoubleVal(iNCIndex, 0, index, ref retValueDouble))
                    {
                        this.mach_info.dfeed_speed = retValueDouble;
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 7006;//实际值 -- 这里其他设备获取的都是编程值，高精这个获取不了编程值，只有实际值，所以在这我把实际值给到了编程值，需要的时候再改变

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusAxisVal(iNCIndex, 0, index, retValueArrayDouble))
                        {
                            this.mach_info.host_value1 = retValueArrayDouble[0];
                            this.mach_info.host_value2 = retValueArrayDouble[1];
                            this.mach_info.host_value3 = retValueArrayDouble[2];
                            this.mach_info.host_value4 = retValueArrayDouble[3];
                            if (double.IsNaN(this.mach_info.host_value4)) 
                            {
                                this.mach_info.host_value4 = 0;
                            }
                            this.mach_info.host_value5 = retValueArrayDouble[4];
                            this.mach_info.host_value6 = retValueArrayDouble[5];
                            this.mach_info.host_value7 = retValueArrayDouble[6];
                            this.mach_info.host_value8 = retValueArrayDouble[7];
                            this.mach_info.host_value9 = retValueArrayDouble[8];
                            ret = 0;    
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 7010;//剩余量

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusAxisVal(iNCIndex, 0, index, retValueArrayDouble))
                        {
                            this.mach_info.dist_togo1 = retValueArrayDouble[0];
                            this.mach_info.dist_togo2 = retValueArrayDouble[1];
                            this.mach_info.dist_togo3 = retValueArrayDouble[2];
                            this.mach_info.dist_togo4 = retValueArrayDouble[3];
                            this.mach_info.dist_togo5 = retValueArrayDouble[4];
                            this.mach_info.dist_togo6 = retValueArrayDouble[5];
                            this.mach_info.dist_togo7 = retValueArrayDouble[6];
                            this.mach_info.dist_togo8 = retValueArrayDouble[7];
                            this.mach_info.dist_togo9 = retValueArrayDouble[8];
                            ret = 0;    
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    //// 读取刀具列表

                    ////for (int p = 0; p < 10; p++)
                    ////{
                    ////    if (0 == Golding.getToolOffsetTable(iNCIndex, 0, p, ref tooloffset))
                    ////    {
                    ////        this.mach_info.tool_all[p] = tooloffset.chanToolTable[p].type;
                    ////        Console.WriteLine(tooloffset.chanToolTable[p].type);
                    ////    }
                    ////}


                    index = 4001;//当前主轴刀具号

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusIntVal(iNCIndex, 0, index, ref retValueInt))
                    {
                        this.mach_info.tool_num = retValueInt;
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 4003;//当前主轴刀半径

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusDoubleVal(iNCIndex, 0, index, ref retValueDouble))
                    {
                        this.mach_info.tool_radius = retValueDouble;
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 2003;//当前程序名
                    StringBuilder buf1 = new StringBuilder(256);

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusStrVal(iNCIndex, 0, index, buf1))
                    {
                        this.mach_info.prog_name = buf1.ToString();
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 2006;//当前行号
                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusIntVal(iNCIndex, 0, index, ref retValueInt))
                    {
                        this.mach_info.current_line = retValueInt;
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    index = 3002;//运行时间
                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusDoubleVal(iNCIndex, 0, index, ref retValueDouble))
                    {
                        this.mach_info.run_time = (int)retValueDouble;
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    StringBuilder buf2 = new StringBuilder(256);
                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getErrorVal(iNCIndex, 0, buf2))    //手动清除类故障           
                    {
                        this.mach_info.error_handle = buf2.ToString();
                        if (this.mach_info.error_handle.Equals(""))
                        {
                            this.mach_info.error_flag = 0;
                        }
                        else
                        {
                            this.mach_info.error_flag = 1;
                        }
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    StringBuilder buf3 = new StringBuilder(256);
                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getAutoMotionErrorVal(iNCIndex, 0, buf3))//自动清除类故障
                    {
                        this.mach_info.error_motion = buf3.ToString();
                        if (this.mach_info.error_handle.Equals(""))
                        {
                            //this.mach_info.error_flag = 0;
                        }
                        else
                        {
                            this.mach_info.error_flag = 1;
                        }
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    StringBuilder buf5 = new StringBuilder(256);
                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getAutoPlcErrorVal(iNCIndex, 0, buf5))//PLC清除类故障，此部分采集的值仍然为空消息，但已经添加了msg
                    {
                        this.mach_info.error_plc = buf5.ToString();
                        if (this.mach_info.error_handle.Equals(""))
                        {
                            //this.mach_info.error_flag = 0;
                        }
                        else
                        {
                            this.mach_info.error_flag = 1;
                        }
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }

                    //if (this.mach_info.error_handle.Equals("") || this.mach_info.error_plc.Equals("") || this.mach_info.error_motion.Equals(""))
                    //{
                    //    this.mach_info.error_flag = 0;
                    //}
                    //else
                    //{
                    //    this.mach_info.error_flag = 1;
                    //}

                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getToolLifeTable(iNCIndex, 0, 2, ref  toolLifeTable))//刀具管理
                    {
                        this.mach_info.tool_id = toolLifeTable.id;
                        this.mach_info.tool_usenum = toolLifeTable.used_num;
                        this.mach_info.tool_life = toolLifeTable.life_num;
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    //读取程序结束标识位
                    index = 2009;
                    if (status == MachStatus.MS_ONLINE && ConstDefine.PingIpOrDomainName(this.ip) == true && 0 == Golding.getStatusIntVal(iNCIndex, 0, index, ref retValueInt))
                    {
                        this.mach_info.programmed_over = retValueInt.ToString();
                        ret = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Log.WriteLogErrMsg("高精采集函数问题，314行");
                    ret = -1;
                }
            }
            //Thread.Sleep(1000);
            return ret;
        }
        #endregion
    }
}
