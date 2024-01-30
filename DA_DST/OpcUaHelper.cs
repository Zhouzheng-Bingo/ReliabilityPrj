using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using CONSTDEFINE;
using OpcUaHelper;
using Opc.Ua.Client;
using Opc.Ua;

namespace DA_DST
{
    /*
     * 秦川力士乐系统
     * **/
    class OpcUaHelper : NCMachine
    {
        OpcUaClient m_OpcUaClient = new OpcUaClient();
        public string username = null;
        public string password = null;
        public string s_ip;
        public OpcUaHelper(string _machnum, string _ip, int _port, int _itype)
            : base(_machnum, _ip, _port, _itype)
        {
            //Thread.Sleep(60000);
            m_sub = Subscri.FAILED;
        }
        Form form1 = new Form();
        //用于存放标签和数据
        Dictionary<string, string> dic_i = new Dictionary<string, string>();

        #region 连接函数
        public override int ConnectToNC(string ip, string username, string password)
        {
            //m_Server.CertificateEvent += new certificateValidation(m_Server_CertificateEvent);
            // Connect to  Machine
            Thread.Sleep(120000);
            int err = -1;
            dic_i.Clear();
            bool ret = ConstDefine.PingIpOrDomainName(this.ip);
            if (ret == true)
            {
                //Thread.Sleep(60000);
                string ip_s = "opc.tcp://" + ip + ":" + this.port.ToString();
                try
                {
                    m_OpcUaClient.ConnectServer(ip_s);
                    if (m_OpcUaClient.Connected == true)
                    {
                        err = 0;
                        m_OpcUaClient.RemoveAllSubscription();
                        err = this.CreateSubscription();
                        if (err == -1)
                        {
                            m_sub = Subscri.FAILED; //建立订阅失败
                        }
                        else
                        {
                            m_sub = Subscri.SUCCESS; //建立订阅成功
                            status = MachStatus.MS_ONLINE;
                        }

                        return err;
                    }
                    else
                    {
                        err = -1;
                        status = MachStatus.MS_OFFLINE;
                        return err;
                    }
                }
                catch (Exception e)
                {
                    e.ToString();
                    Console.WriteLine("连接这里有问题");
                    return -1;
                }
            }
            else
            {
                return -1;
                status = MachStatus.MS_OFFLINE;
                Log.WriteLogErrMsg("力士乐连接函数问题，83行");
            }
        }
        #endregion

        int axisNum = 0;

        #region 读取函数
        public override int UpdateData()
        {
            int ret = -1;
            if (0 != UpdateComm())
            { return -1; }
            if (status == MachStatus.MS_ONLINE)
            {
                bool rtn = ConstDefine.PingIpOrDomainName(ip);
                if (rtn == true)
                {
                    GetSubscriptionValue();
                    ret = 0;
                }
                else
                {
                    ret = -1;
                    status = MachStatus.MS_OFFLINE;
                }
            }
            else if (status != MachStatus.MS_ONLINE)
            {
                ret = -1;
                ret = UpdateComm();
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
                m_OpcUaClient.RemoveAllSubscription();
                m_OpcUaClient.Disconnect();
            }
            catch (Exception e)
            { }
            return err;
        }
        #endregion

        #region 重新连接函数
        public override int ReConnectToNC(string ip, string username, string password)
        {
            int err = 0;
            err = ConnectToNC(ip, username, password);
            if (err == 0)
            {
                this.status = MachStatus.MS_ONLINE;

                err = this.CreateSubscription();
                if (err == -1)
                {
                    m_sub = Subscri.FAILED; //建立订阅失败
                }
                else
                {
                    m_sub = Subscri.SUCCESS; //建立订阅成功
                }
            }
            return err;
        }
        #endregion

        #region 订阅相关的函数群
        public int CreateSubscription()
        {
            int err = -1;
            GetSubscriptionItem(ref dic_i);
            int count_nodeid = -1;
            int i = 0;
            count_nodeid = this.dic_i.Count();
            String[] keyArr = dic_i.Keys.ToArray<String>();  //防止数据改变无法进行遍历
            Dictionary<string, string> dic = new Dictionary<string, string>();
            for (int j = 0; j < keyArr.Length; j++)
            {
                string s_itmeID = keyArr[j];
                NodeId node = new NodeId(s_itmeID, 27);
                keyArr[j] = node.ToString();
                dic.Add(s_itmeID, "--");
            }
            try
            {
                m_OpcUaClient.AddSubscription(this.machnum, keyArr, CallBack);
                err = 0;
                return err;
            }
            catch
            {
                err = -1;
                return err;
            }
        }

        void CallBack(string item, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args)
        {
            if (form1.InvokeRequired)
            {
                form1.Invoke(new Action<string, MonitoredItem, MonitoredItemNotificationEventArgs>(CallBack), item, monitoredItem, args);
                return;
            }

            Opc.Ua.MonitoredItemNotification notification = args.NotificationValue as Opc.Ua.MonitoredItemNotification;

            if (notification != null)
            {
                string it = monitoredItem.DisplayName;
                try
                {
                    foreach (var itemID in dic_i)
                    {
                        string s_temp = "ns=27;s=" + itemID.Key;
                        if (it == "")
                        {
                            continue;
                        }
                        else if ((s_temp == it) && (notification.Value.WrappedValue.TypeInfo!=null))
                        {

                            if (notification.Value.WrappedValue.TypeInfo.BuiltInType == Opc.Ua.BuiltInType.Int32)
                            {
                                if (notification.Value.WrappedValue.TypeInfo.ValueRank == -1)
                                {
                                    int temp = (int)notification.Value.WrappedValue.Value;               // 最终值
                                    if (s_temp.Equals("ns=27;s=NC.Chan.OperationMode,01,Mode"))
                                    {
                                        dic_i[itemID.Key] = temp.ToString();
                                        // Console.WriteLine("dic_i[itemID.Key]:kkkkkkkkkkk" + dic_i[itemID.Key]);
                                    }

                                    if (s_temp.Equals("ns=27;s=NC.Chan.OperationState,01,ChanState"))
                                    {
                                        dic_i[itemID.Key] = temp.ToString();
                                        // Console.WriteLine("dic_i[itemID.Key]:kkkkkkkkkkk" + dic_i[itemID.Key]);
                                    }

                                    if (s_temp.Equals("ns=27;s=NC.CplPermVariable,@REMAIN_TIMES"))
                                    {
                                        dic_i[itemID.Key] = temp.ToString();
                                        // Console.WriteLine("dic_i[itemID.Key]:kkkkkkkkkkk" + dic_i[itemID.Key]);
                                    }

                                    if (s_temp.Equals("ns=27;s=NC.CplPermVariable,@NC_GO"))
                                    {
                                        dic_i[itemID.Key] = temp.ToString();
                                        // Console.WriteLine("dic_i[itemID.Key]:kkkkkkkkkkk" + dic_i[itemID.Key]);
                                    }
                                }
                            }
                            if (notification.Value.WrappedValue.TypeInfo.BuiltInType == Opc.Ua.BuiltInType.Double)
                            {
                                if (notification.Value.WrappedValue.TypeInfo.ValueRank == -1) // 判断double类型是否为数值
                                {
                                    double double_temp_value = (double)notification.Value.WrappedValue.Value;               // 最终值

                                }
                                else if (notification.Value.WrappedValue.TypeInfo.ValueRank == 1) // 判断double类型是否为一维数组
                                {
                                    double[] double_temp_linear_array = (double[])notification.Value.WrappedValue.Value;           // 最终值
                                    if (s_temp.Equals("ns=27;s=NC.Chan.ProgSpindleSpeed,01,Speed"))
                                    {
                                        if (double_temp_linear_array != null && double_temp_linear_array.Length != 0)
                                        {
                                            dic_i[itemID.Key] = double_temp_linear_array[0].ToString();
                                        }
                                        //Console.WriteLine("dic_i[itemID.Key]mmmmmmmmmmmmmmm:" + dic_i[itemID.Key]);
                                    }

                                    if (s_temp.Equals("ns=27;s=NC.Chan.ActFeedOverride,01"))
                                    {
                                        if (double_temp_linear_array != null && double_temp_linear_array.Length != 0)
                                        {
                                            dic_i[itemID.Key] = double_temp_linear_array[0].ToString();
                                        }
                                    }

                                    if (s_temp.Equals("ns=27;s=NC.Chan.ProgrammedFeedrate,01"))
                                    {
                                        if (double_temp_linear_array != null && double_temp_linear_array.Length != 0)
                                        {
                                            dic_i[itemID.Key] = double_temp_linear_array[0].ToString();
                                        }
                                    }

                                    if (s_temp.Equals("ns=27;s=NC.Chan.ActFeedrate,01"))
                                    {
                                        if (double_temp_linear_array != null && double_temp_linear_array.Length != 0)
                                        {
                                            dic_i[itemID.Key] = double_temp_linear_array[0].ToString();
                                        }
                                    }

                                    if (s_temp.Equals("ns=27;s=NC.Chan.ActSpindleOverride,01,ActiveValue"))
                                    {
                                        if (double_temp_linear_array != null && double_temp_linear_array.Length != 0)
                                        {
                                            dic_i[itemID.Key] = double_temp_linear_array[0].ToString();
                                        }
                                    }

                                    if (s_temp.Equals("ns=27;s=NC.Chan.AxisPosAcs,01,1"))
                                    {
                                        int i = 1;
                                        //String axisProgramedValue = "NC.Chan.AxisPosAcs,01,1," + i;
                                        for (i = 1; i <= double_temp_linear_array.Length; ++i)
                                        {
                                            String axisProgramedValue = "NC.Chan.AxisPosAcs,01,1," + i;
                                            dic_i[axisProgramedValue] = double_temp_linear_array[i - 1].ToString();
                                        }
                                    }

                                    if (s_temp.Equals("ns=27;s=NC.Chan.ActAxisVelocity,01,Value"))
                                    {
                                        int i = 1;
                                        //String axisActVelocity = "NC.Chan.ActAxisVelocity,01,Value," + i;
                                        for (i = 1; i <= double_temp_linear_array.Length; ++i)
                                        {
                                            String axisActVelocity = "NC.Chan.ActAxisVelocity,01,Value," + i;
                                            dic_i[axisActVelocity] = double_temp_linear_array[i - 1].ToString();
                                        }
                                    }
                                }
                            }
                            if (notification.Value.WrappedValue.TypeInfo.BuiltInType == Opc.Ua.BuiltInType.String)
                            {

                                if (notification.Value.WrappedValue.TypeInfo.ValueRank == -1) // 判断string类型是否为数值
                                {
                                    string temp = (string)notification.Value.WrappedValue.Value;               // 最终值
                                    if (s_temp.Equals("ns=27;s=NC.Chan.ActMainProgram,01"))
                                    {
                                        if (temp != null && temp.Length != 0)
                                        {
                                            dic_i[itemID.Key] = temp.ToString();
                                        }
                                        //Console.WriteLine("dic_i[itemID.Key]:" + dic_i[itemID.Key]);
                                    }

                                    //if (s_temp.Equals("ns=10;i=6009"))
                                    //{
                                    //    if (temp != null && temp.Length != 0)
                                    //    {
                                    //        dic_i[itemID.Key] = temp.ToString();
                                    //    }
                                    //    //Console.WriteLine("dic_i[itemID.Key]:" + dic_i[itemID.Key]);
                                    //}
                                }
                                else if (notification.Value.WrappedValue.TypeInfo.ValueRank == 1) // 判断string类型是否为一维数组
                                {
                                    string[] string_temp_linear_array = (string[])notification.Value.WrappedValue.Value;           // 最终值
                                    if (s_temp.Equals("ns=27;s=NC.Chan.AxisScale,01,Name"))
                                    {
                                        if (string_temp_linear_array != null && string_temp_linear_array.Length != 0)
                                        {
                                            dic_i[itemID.Key] = string_temp_linear_array.Length.ToString();
                                        }
                                    }
                                }

                            }
                            if (notification.Value.WrappedValue.TypeInfo.BuiltInType == Opc.Ua.BuiltInType.Float)
                            {
                                float temp = (float)notification.Value.WrappedValue.Value;

                                if (s_temp.Equals("ns=27;s=NC.CplPermVariable,@F,720"))
                                {
                                    dic_i[itemID.Key] = temp.ToString();
                                    // Console.WriteLine("dic_i[itemID.Key]:kkkkkkkkkkk" + dic_i[itemID.Key]);
                                }

                                if (s_temp.Equals("ns=27;s=NC.CplPermVariable,@ALLFEED"))
                                {
                                    dic_i[itemID.Key] = temp.ToString();
                                    //Console.WriteLine("dic_i[itemID.Key]mmmmmmmmmmmmmmm:" + dic_i[itemID.Key]);
                                }

                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLogErrMsg("订阅返回数据失败！");
                    ex.ToString();
                    Console.WriteLine("创建订阅CallBack有问题");
                }
            }
        }

        //建立订阅数据项的字典
        public void GetSubscriptionItem(ref Dictionary<string, string> dic)
        {
            //dic.Add("ns=10;i=6009", ""); // 软件版本号
            dic.Add("NC.Chan.AxisScale,01,Name", ""); // 添加轴数
            dic.Add("NC.Chan.ProgSpindleSpeed,01,Speed", ""); // 主轴编程速度
            dic.Add("NC.Chan.ActSpindleOverride,01,ActiveValue", ""); // 主轴倍率
            dic.Add("NC.Chan.ActFeedrate,01", ""); // 进给真实值
            dic.Add("NC.Chan.ProgrammedFeedrate,01", ""); // 进给编程速度

            dic.Add("NC.Chan.ActFeedOverride,01", ""); // 进给倍率
            dic.Add("NC.Chan.ActMainProgram,01", ""); // 程序名
            dic.Add("NC.Chan.OperationMode,01,Mode", ""); // 机床运行状态
            dic.Add("NC.Chan.OperationState,01,ChanState", ""); // 界面模式

            //// 添加轴编程坐标值
            dic.Add("NC.Chan.AxisPosAcs,01,1", "");
            dic.Add("NC.Chan.AxisPosAcs,01,1,1", "");
            dic.Add("NC.Chan.AxisPosAcs,01,1,2", "");
            dic.Add("NC.Chan.AxisPosAcs,01,1,3", "");

            dic.Add("NC.Chan.AxisPosAcs,01,1,4", "");
            dic.Add("NC.Chan.AxisPosAcs,01,1,5", "");
            dic.Add("NC.Chan.AxisPosAcs,01,1,6", "");

            dic.Add("NC.Chan.AxisPosAcs,01,1,7", "");
            dic.Add("NC.Chan.AxisPosAcs,01,1,8", "");
            dic.Add("NC.Chan.AxisPosAcs,01,1,9", "");

            dic.Add("NC.Chan.ActAxisVelocity,01,Value", "");
            dic.Add("NC.Chan.ActAxisVelocity,01,Value,1", "");
            dic.Add("NC.Chan.ActAxisVelocity,01,Value,2", "");
            dic.Add("NC.Chan.ActAxisVelocity,01,Value,3", "");

            dic.Add("NC.Chan.ActAxisVelocity,01,Value,4", "");
            dic.Add("NC.Chan.ActAxisVelocity,01,Value,5", "");
            dic.Add("NC.Chan.ActAxisVelocity,01,Value,6", "");

            dic.Add("NC.Chan.ActAxisVelocity,01,Value,7", "");
            dic.Add("NC.Chan.ActAxisVelocity,01,Value,8", "");
            dic.Add("NC.Chan.ActAxisVelocity,01,Value,9", "");

            dic.Add("NC.CplPermVariable,@F,720", "");

            dic.Add("NC.CplPermVariable,@ALLFEED", "");
            dic.Add("NC.CplPermVariable,@REMAIN_TIMES", "");
            dic.Add("NC.CplPermVariable,@NC_GO", "");
        }
        #endregion

        // 获取值存入结构体中
        public void GetSubscriptionValue()
        {
            sendData = 1;
            try
            {
                if (!String.IsNullOrEmpty(dic_i["NC.Chan.AxisScale,01,Name"]))
                {
                    axisNum = Convert.ToInt32(dic_i["NC.Chan.AxisScale,01,Name"].ToString());
                  
                    if (dic_i["NC.Chan.AxisScale,01,Name"] != "")
                    {
                        mach_info.axis_num = Convert.ToInt32(dic_i["NC.Chan.AxisScale,01,Name"].ToString());
                    }
                    else
                    {
                        mach_info.axis_num = 0;
                    }

                    if (dic_i["NC.Chan.ProgSpindleSpeed,01,Speed"] != "")
                    {
                        mach_info.spindle_speed = Convert.ToDouble(dic_i["NC.Chan.ProgSpindleSpeed,01,Speed"].ToString());
                    }
                    else
                    {
                        mach_info.spindle_speed = 0;
                    }

                    if (dic_i["NC.Chan.ActSpindleOverride,01,ActiveValue"] != "")
                    {
                        mach_info.dspindle_speed = Convert.ToDouble(dic_i["NC.Chan.ActSpindleOverride,01,ActiveValue"].ToString());
                    }
                    else
                    {
                        mach_info.dspindle_speed = 0;
                    }

                    if (dic_i["NC.Chan.ActFeedrate,01"] != "")
                    {
                        mach_info.afeed_speed = Convert.ToDouble(dic_i["NC.Chan.ActFeedrate,01"].ToString());
                    }
                    else
                    {
                        mach_info.afeed_speed = 0;
                    }

                    if (dic_i["NC.Chan.ProgrammedFeedrate,01"] != "")
                    {
                        mach_info.feed_speed = Convert.ToDouble(dic_i["NC.Chan.ProgrammedFeedrate,01"].ToString());
                    }
                    else
                    {
                        mach_info.feed_speed = 0;
                    }

                    if (dic_i["NC.Chan.ActFeedOverride,01"] != "")
                    {
                        mach_info.dfeed_speed = Convert.ToDouble(dic_i["NC.Chan.ActFeedOverride,01"].ToString());
                    }
                    else
                    {
                        mach_info.dfeed_speed = 0;
                    }

                    if (dic_i["NC.Chan.ActMainProgram,01"] != "")
                    {
                        mach_info.prog_name = Convert.ToString(dic_i["NC.Chan.ActMainProgram,01"].ToString());
                    }
                    else
                    {
                        mach_info.prog_name = "";
                    }

                    if (dic_i["NC.Chan.OperationMode,01,Mode"] != "")
                    {
                        //add by zhang 
                        //此处的运行模式未按照统一标准进行转换
                        //1auto2mdi3jog4teachin 5repos6ref7edit8handle9remoto 10step 11unknown12手轮
                        mach_info.disp_mode = Convert.ToInt32(dic_i["NC.Chan.OperationMode,01,Mode"].ToString());
                    }
                    else
                    {
                        mach_info.disp_mode = 0;
                    }

                    if (dic_i["NC.Chan.OperationState,01,ChanState"] != "")
                    {
                        mach_info.interp_state = Convert.ToInt32(dic_i["NC.Chan.OperationState,01,ChanState"].ToString());
                    }
                    else
                    {
                        mach_info.interp_state = 0;
                    }

                    if (dic_i["NC.CplPermVariable,@F,720"] != "")
                    {
                        mach_info.tool_num = Convert.ToInt32(dic_i["NC.CplPermVariable,@F,720"].ToString());
                    }
                    else
                    {
                        mach_info.tool_num = 0;
                    }

                    int i = 1;
                    for (i = 1; i <= axisNum; ++i)
                    {
                        String axisProgramedValue = "NC.Chan.AxisPosAcs,01,1," + i;
                        // 轴编程值
                        if (dic_i[axisProgramedValue] != "")
                        {
                            switch (i)
                            {
                                case 1: mach_info.host_value1 = Convert.ToDouble(dic_i[axisProgramedValue].ToString()); break;
                                case 2: mach_info.host_value2 = Convert.ToDouble(dic_i[axisProgramedValue].ToString()); break;
                                case 3: mach_info.host_value3 = Convert.ToDouble(dic_i[axisProgramedValue].ToString()); break;
                                case 4: mach_info.host_value4 = Convert.ToDouble(dic_i[axisProgramedValue].ToString()); break;
                                case 5: mach_info.host_value5 = Convert.ToDouble(dic_i[axisProgramedValue].ToString()); break;
                                case 6: mach_info.host_value6 = Convert.ToDouble(dic_i[axisProgramedValue].ToString()); break;
                                case 7: mach_info.host_value7 = Convert.ToDouble(dic_i[axisProgramedValue].ToString()); break;
                                case 8: mach_info.host_value8 = Convert.ToDouble(dic_i[axisProgramedValue].ToString()); break;
                                case 9: mach_info.host_value9 = Convert.ToDouble(dic_i[axisProgramedValue].ToString()); break;
                            }
                        }

                        String axisActVelocity = "NC.Chan.ActAxisVelocity,01,Value," + i;
                        // 轴真实速度
                        if (dic_i[axisActVelocity] != "")
                        {
                            switch (i)
                            {
                                case 1: mach_info.act_axis_velocity1 = Convert.ToDouble(dic_i[axisActVelocity].ToString()); break;
                                case 2: mach_info.act_axis_velocity2 = Convert.ToDouble(dic_i[axisActVelocity].ToString()); break;
                                case 3: mach_info.act_axis_velocity3 = Convert.ToDouble(dic_i[axisActVelocity].ToString()); break;
                                case 4: mach_info.act_axis_velocity4 = Convert.ToDouble(dic_i[axisActVelocity].ToString()); break;
                                case 5: mach_info.act_axis_velocity5 = Convert.ToDouble(dic_i[axisActVelocity].ToString()); break;
                                case 6: mach_info.act_axis_velocity6 = Convert.ToDouble(dic_i[axisActVelocity].ToString()); break;
                                case 7: mach_info.act_axis_velocity7 = Convert.ToDouble(dic_i[axisActVelocity].ToString()); break;
                                case 8: mach_info.act_axis_velocity8 = Convert.ToDouble(dic_i[axisActVelocity].ToString()); break;
                                case 9: mach_info.act_axis_velocity9 = Convert.ToDouble(dic_i[axisActVelocity].ToString()); break;
                            }
                        }
                    }

                    // 砂轮
                    if (dic_i["NC.CplPermVariable,@ALLFEED"] != "")
                    {
                        mach_info.wheel_ride = Convert.ToDouble(dic_i["NC.CplPermVariable,@ALLFEED"].ToString());
                    }
                    else
                    {
                        mach_info.wheel_ride = 0.0;
                    }

                    if (dic_i["NC.CplPermVariable,@REMAIN_TIMES"] != "")
                    {
                        mach_info.wheel_remaintime = Convert.ToInt32(dic_i["NC.CplPermVariable,@REMAIN_TIMES"].ToString());
                    }
                    else
                    {
                        mach_info.wheel_remaintime = 0;
                    }

                    // 采集到，则是显示的值，采集不到则显示-1
                    if (dic_i["NC.CplPermVariable,@NC_GO"] != "")
                    {
                        // mach_info.programmed_over = Convert.ToString(dic_i["NC.CplPermVariable,@F,101"].ToString());
                        mach_info.programmed_over = Convert.ToString(dic_i["NC.CplPermVariable,@NC_GO"].ToString());
                    }
                    else
                    {
                        mach_info.programmed_over = "-1";
                    }

                    mach_info.error_flag = 0;

                    /* temp*/
                    if (ip.Equals("192.168.2.37")) {
                        mach_info.current_line = 13;
                        mach_info.work_piece = 516;
                    }
                    else if(ip.Equals("192.168.2.10"))
                    {
                        mach_info.current_line = 26;
                        mach_info.work_piece = 662;
                    }
                    else
                    {
                        mach_info.current_line = 7;
                        mach_info.work_piece = 703;
                    }
                    

                    // mach_info.programmed_over = mach_info.interp_state.ToString();
                }


                // 判断初始值不为空字符串

            }
            catch (Exception ex)
            {
                Log.WriteLogErrMsg("GetSubscriptionValue()故障");
                ex.ToString();
                Console.WriteLine("GetSubscriptionValue有问题");
            }
        }
    }
}
