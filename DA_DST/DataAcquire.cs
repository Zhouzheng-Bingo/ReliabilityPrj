using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;

using System.Threading;
using Oracle.DataAccess.Client;
using ConnectDB;
using CONSTDEFINE;
using System.Windows.Forms;

using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Xml.Linq;


namespace DA_DST
{
    class DataAcquire
    {
        string cuser, cpasswd, csid;
        OracleDB db = new OracleDB();
        OracleDataReader odr;
        //IList<NCMachine> m_machineQueue;
        Queue<NCMachine> m_machineQueue = new Queue<NCMachine>();
        public Queue<NCMachine> m_machineDataQueue = new Queue<NCMachine>();
        //List<NCMachine> m_machineDataQueue = new List<NCMachine>();
        private Object thisLock = new object();
        private Object thisLockData = new object();
        public int m_iNumMachine = 0;
        ManualResetEvent mre = new ManualResetEvent(false);
        //对话框
        Main m_pMainDlg = null;

        //WEBService信息 暂时注释
        //CNCWebservice1.index webserviceCNC = new CNCWebservice1.index();

       

        // 创建一个访问XML文件的对象,访问字典列表
        //XElement xe = XElement.Load(@".\MachineDict.xml");

        // 创建一个访问XML文件的对象,访问字典列表
        //XElement xe2 = XElement.Load(@".\LocalipForHNC8.xml");

        // 设置APServer监听flag，保证监听只打开一个
        int flag = 0;

        public void SetDBConnArg(string user, string passwd, string sid)
        {
            cuser = user;
            cpasswd = passwd;
            csid = sid;
            db.db_user = cuser;
            db.db_passwd = cpasswd;
            db.db_source = sid;
        }

        public void StartDataAquire()
        {
            int rtn;
            m_iNumMachine = 0;
            Thread.Sleep(100);
            InitMembers();
            int machine_num = m_machineQueue.Count;
            //用于控制系统接入机床的数量
            if (machine_num <= 40)
            {
                rtn = ReadyForAcquire();
                if (rtn == 0)
                {
                    StartDataThreads();
                }
            }
            else
            {
                rtn = -1;
            }

        }

        #region 初始化队列成员函数群
        public void InitMembers()
        {
            //建立机器队列
            try
            {
                int rtn = db.ConnectToDB();
                if (rtn == 0)
                {
                    CreatMachineQueue();
                    db.CloseConn();
                }
            }
            catch (Exception e)
            {
                // db.CloseConn(); // 20210528新增关闭数据库
                Console.WriteLine("DataAcquire: InitMembers() :" + e.ToString());
                // throw;
            }
            
            

        }

        public void CreatMachineQueue()
        {
            OracleDataReader odr;
            int err = -1;
            int day_time = 0, start_time = 0, end_time = 0;
            int today_poweron_time = 0, today_poweroff_time = 0, today_run_time = 0, today_idle_time = 0, today_error_time = 0, today_cut_time = 0;
            int day_poweron_time = 0, day_run_time = 0, day_cut_time = 0;
            machine_info_t mach_info = new machine_info_t();

            NCMachine pNCMachine = new NCMachine();
            err = db.ConnectToDB();
            string sql = "select mach_num,ip,iport,itype,id from room_machine where acquiredata_status = 1 order by mach_num asc";
            try
            {
     
                if (err == 0)
                {
                    odr = db.ReturnDataReader(sql);

                    while (odr.Read())
                    {
                        string s_machnum = odr["mach_num"].ToString();
                        string s_ip = odr["ip"].ToString();
                        string s_port = odr["iport"].ToString();
                        string s_type = odr["itype"].ToString();
                        int iPort = Convert.ToInt32(s_port);
                        int iType = Convert.ToInt32(s_type);
                        string s_userid = odr["id"].ToString();
                        //获取machine_log_today中的时间信息
                        GetTimeByNumber(s_machnum, ref day_time, ref today_poweron_time, ref today_poweroff_time,
                            ref today_run_time, ref today_idle_time, ref today_error_time, ref today_cut_time,
                            ref day_poweron_time, ref day_run_time, ref day_cut_time);
                        //获取当天的开始和结束时间
                        GetTimeByNowTime(day_time, ref start_time, ref end_time);

                        if (iType == (int)MachineType.REXROTH)//如果类型为力士乐
                        {
                            pNCMachine = new OpcUaHelper(s_machnum, s_ip, iPort, iType);
                            pNCMachine.isacquire = IsAcquire.Yes;
                            //m_machineQueue.Enqueue(pNCMachine);
                        }

                        else if (iType == (int)CONSTDEFINE.MachineType.GOLDING)
                        {
                            pNCMachine = new GJ430(s_machnum, s_ip, iPort, iType);
                            pNCMachine.isacquire = IsAcquire.Yes;
                            Console.WriteLine("{0}连接成功\n", pNCMachine.machnum);
                        }

                        else if (iType == (int)CONSTDEFINE.MachineType.QCMTT)
                        {
                        
                            pNCMachine = new HttpUtils(s_machnum, s_ip, iPort, iType);

                            if (flag == 0)
                            {
                                pNCMachine.Monitor("192.168.2.123", 8820); // 传递XML中的本地ip，该ip是工控机中js文件定向发送至的ip地址
                                flag = 1;
                            }

                            pNCMachine.isacquire = IsAcquire.Yes;
                            ////设置监听路径
                            //HttpListener Monit = new HttpListener();
                            //Monit.Prefixes.Add(string.Concat("http://", s_ip, ":", iPort, "/"));
                            ////设置匿名访问
                            //Monit.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                            //////开始监听
                            //Monit.Start();
                            ////Monit.close();
                            //pNCMachine.Recept(Monit);

                        }


                        else if (iType == (int)CONSTDEFINE.MachineType.HNC8)
                        {

                            pNCMachine = new HNC8(s_machnum, "192.168.2.68", 9090, "CSharpTest", s_ip, iPort, iType);
                            //#region
                            //IEnumerable<XElement> elements = from ele in xe.Elements("address")
                            //                                 select ele;
                            //HNCConfigModel model = new HNCConfigModel();
                            //// 思路：
                            //// 1.判断数据库中ip地址为192.168.2.*网段还是为192.168.3.*网段，为2.*网段flag=2, 为3.*网段flag=3;
                            //// 2.从XML中读取本地地址
                            //// 3.若为2.*网段，取XML中的第一个本地地址，进行本地初始化
                            //// 4.若为3.*网段，取XML中的第二个本地地址，进行本地初始化

                            ////创建正则表达式字符串
                            //string netseg1 = "([192]+\\.)+([168]+\\.)+([2]+\\.)";
                            //string netseg2 = "([192]+\\.)+([168]+\\.)+([3]+\\.)";
                            //string lastnum = "((25[0-5])|(2[0-4]\\d)|([0-1]\\d{2})|([1-9]?\\d))";

                            ////+\d{1,3}
                            ////使用正则表达式判断是否匹配
                            //if (System.Text.RegularExpressions.Regex.IsMatch(s_ip, ("^" + netseg1 + "" + lastnum + "$")))
                            //{
                            //    // 若为2.*网段，取XML中的第一个本地地址，进行本地初始化
                            //    foreach (var ele in elements)
                            //    {
                            //        model.AddressType = ele.Attribute("Type").Value;
                            //        if (model.AddressType.Equals("gear"))
                            //        {
                            //            model.LocalIp = ele.Element("localip").Value;
                            //            model.LocalPort = ele.Element("localport").Value;
                            //            model.Name = ele.Element("name").Value;
                            //            pNCMachine = new HNC8(s_machnum, model.LocalIp, (ushort)Convert.ToInt32(model.LocalPort), model.Name, s_ip, iPort, iType);
                            //            break;
                            //        }
                            //    }
                            //}
                            //else if (System.Text.RegularExpressions.Regex.IsMatch(s_ip, ("^" + netseg2 + "" + lastnum + "$")))
                            //{
                            //    // 若为3.*网段，取XML中的第二个本地地址，进行本地初始化
                            //    foreach (var ele in elements)
                            //    {
                            //        model.AddressType = ele.Attribute("Type").Value;
                            //        if (model.AddressType.Equals("cartridge"))
                            //        {
                            //            model.LocalIp = ele.Element("localip").Value;
                            //            model.LocalPort = ele.Element("localport").Value;
                            //            model.Name = ele.Element("name").Value;
                            //            pNCMachine = new HNC8(s_machnum, model.LocalIp, (ushort)Convert.ToInt32(model.LocalPort), model.Name, s_ip, iPort, iType);
                            //            break;
                            //        }
                            //    }
                            //}
                            //else // 此部分代码测试用
                            //{
                            //    pNCMachine = new HNC8(s_machnum, "10.10.56.66", 9090, "CSharpTest", s_ip, iPort, iType);
                            //}
                            //#endregion
                            ////foreach (var ele in elements)
                            ////{
                            ////    model.AddressType = ele.Attribute("Type").Value;
                            ////    if (model.AddressNumber.Equals("gear"))
                            ////    {
                            ////        model.LocalIp = ele.Element("localip").Value;
                            ////        model.LocalPort = ele.Element("localport").Value;
                            ////        model.Name = ele.Element("name").Value;
                            ////        pNCMachine = new HNC8(s_machnum, model.LocalIp, (ushort)Convert.ToInt32(model.LocalPort), model.Name, s_ip, iPort, iType);
                            ////        break;
                            ////    }
                            ////    else if (model.AddressNumber.Equals("cartridge")) 
                            ////    {
                            ////        model.LocalIp = ele.Element("localip").Value;
                            ////        model.LocalPort = ele.Element("localport").Value;
                            ////        model.Name = ele.Element("name").Value;
                            ////        pNCMachine = new HNC8(s_machnum, model.LocalIp, (ushort)Convert.ToInt32(model.LocalPort), model.Name, s_ip, iPort, iType);
                            ////        break;
                            ////    }

                            ////}

                            pNCMachine.isacquire = IsAcquire.Yes;
                        }

                        //webservic服务接口
                        //webserviceCNC.DevAdd(s_machnum);
                        long now_time = ConstDefine.GetCurrentTimeUnix();
                        if (now_time > start_time && now_time < end_time)
                        {
                            pNCMachine.today_offline_time += today_poweroff_time;
                            pNCMachine.today_idle_time += today_idle_time;
                            pNCMachine.today_error_time += today_error_time;
                            /************************有效工时************************************/
                            pNCMachine.today_poweron_time += today_poweron_time;
                            pNCMachine.today_run_time_meet_setting = today_run_time;
                            pNCMachine.today_run_time = today_run_time;
                            pNCMachine.s_userid_last = s_userid;
                            pNCMachine.s_userid_now = s_userid;
                            InsertWorkTimeToDB(pNCMachine);
                            /********************************************************************/

                            /**************当天时间统计*********************/
                            pNCMachine.day_poweron_time += day_poweron_time;
                            pNCMachine.day_run_time += day_run_time;
                            pNCMachine.day_cut_time += day_cut_time;
                            /***********************************************/
                        }
                        else
                        {
                            /************************有效工时************************************/
                            pNCMachine.today_poweron_time += today_poweron_time;
                            pNCMachine.today_run_time_meet_setting = today_run_time;
                            pNCMachine.today_run_time = today_run_time;
                            pNCMachine.s_userid_last = s_userid;
                            pNCMachine.s_userid_now = s_userid;
                            InsertWorkTimeToDB(pNCMachine);
                            /********************************************************************/

                            /**************当天时间统计*********************/
                            pNCMachine.day_poweron_time += day_poweron_time;
                            pNCMachine.day_run_time += day_run_time;
                            pNCMachine.day_cut_time += day_cut_time;
                            /***********************************************/
                        }
                        m_machineQueue.Enqueue(pNCMachine);
                        //m_machineDataQueue.Add(pNCMachine);
                        m_iNumMachine++;
                    }
                    // db.CloseConn(); // 20210528新增关闭数据库
                }

                else
                {
                    Console.WriteLine("err不为0，建议使用try..catch..");
                    // db.CloseConn(); // 20210528新增关闭数据库
                }
            }
            catch (Exception)
            {
                Log.WriteLogErrMsg("模板初始化问题，308行");
                throw;
            }

        }

        /*
        *功能描述: 获取每台设备的实时的时间信息
        *参数:
        *  
        *返回值:0 成功 ，-1 失败 
        */
        public int GetTimeByNumber(string mach_num, ref int day_time, ref int today_poweron_time, ref int today_poweroff_time, ref int today_run_time, ref int today_idle_time, ref int today_error_time, ref int today_cut_time, ref int day_poweron_time, ref int day_run_time, ref int day_cut_time)
        {
            int rtn = -1;
            DataSet ds_time = new DataSet();
            string dbQuery = "select now_time,today_poweron_time,today_poweroff_time,today_run_time,today_idle_time,today_error_time,today_cut_time,day_poweron_time,day_run_time,day_cut_time from machine_log_today where mach_num='" + mach_num + "'";
            try
            {
                ds_time = db.ReturnDataSet(dbQuery);
                foreach (DataRow item in ds_time.Tables[0].Rows)
                {
                    day_time = Convert.ToInt32(item["NOW_TIME"].ToString());
                    today_poweron_time = Convert.ToInt32(item["today_poweron_time"].ToString());
                    today_poweroff_time = Convert.ToInt32(item["today_poweroff_time"].ToString());
                    today_run_time = Convert.ToInt32(item["today_run_time"].ToString());
                    today_idle_time = Convert.ToInt32(item["today_idle_time"].ToString());
                    today_error_time = Convert.ToInt32(item["today_error_time"].ToString());
                    today_cut_time = Convert.ToInt32(item["today_cut_time"].ToString());
                    day_poweron_time = Convert.ToInt32(item["day_poweron_time"].ToString());
                    day_run_time = Convert.ToInt32(item["day_run_time"].ToString());
                    day_cut_time = Convert.ToInt32(item["day_cut_time"].ToString());
                }
                rtn = 0;
            }
            catch (Exception ex)
            {
                Log.WriteLogErrMsg("GetTimeByNumber失败");
                rtn = -1;
            }
            return rtn;
        }
        /*
       *功能描述: 获取本工时的开始和结束时间点（例如昨天八点到今天8点，今天八点到明天八点）
       *参数:
       *  
       *返回值: 
       */
        void GetTimeByNowTime(long now_time, ref int start, ref int end)
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;

            if (currentTime.Hour < 8)
            {
                TimeSpan cha = (System.DateTime.Today - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
                start = (int)cha.TotalSeconds - 86400;//得到本工时的开始时间            
                end = (int)now_time + 28800;
            }
            else
            {
                TimeSpan cha = (System.DateTime.Today - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
                start = (int)cha.TotalSeconds + 28800;//得到本工时的开始时间 
                end = (int)now_time + 115200;
            }
        }
        #endregion

        #region 准备采集函数群
        public int ReadyForAcquire()
        {
            int rtn;
            rtn = db.ConnectToDB();
            if (rtn != 0)
            {
                return -1;
            }
            rtn = ClearLastInfo();
            if (rtn >= 0)
            {
                rtn = ResetTable();
            }
            db.CloseConn();

            return rtn;
        }

        public int ClearLastInfo()
        {
            int rtn;
            string querySQL = "delete from machine_log_today";
            rtn = db.ExecuteSQL(querySQL);

            return rtn;
        }
        public int ResetTable()
        {
            int rtn = -1;
            string querySQL;
            NCMachine TempNCMachine = new NCMachine();
            //System.DateTime currentTime = new System.DateTime();
            //currentTime = System.DateTime.Now;
            //String date;
            ////date = String.Format("%{}4d-%2d-%2d %2d:%2d:%2d", currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, currentTime.Second);
            //date = string.Format("{0:yyyy-MM-dd HH:mm:ss}", currentTime);
            //String nowtime = "to_date('" + date + "','yyyy-MM-dd HH24:mi:ss')";
            int NowTime = (int)ConstDefine.GetCurrentTimeUnix();
            if (m_machineQueue.Count != 0)
            {
                foreach (var temp in m_machineQueue)
                {
                    string temp_machnum = temp.machnum;
                    string temp_ip = temp.ip;
                    querySQL = "insert into machine_log_today values('";
                    querySQL += temp_machnum + "','" + temp_ip + "'," + NowTime + ",0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,'',0,0,0,0,0,0,0,0,0,0,0,0,0,0,'',0,0,0,0)";
                    rtn = db.ExecuteSQL(querySQL);
                    if (rtn < 0)
                    {
                        break;
                    }
                }
            }
            else
            {
                rtn = -1;
            }
            return rtn;
        }
        #endregion

        #region 开启采集线程函数
        public void StartDataThreads()
        {
            int rtn = -1;
            do
            {
                rtn = db.ConnectToDB();
                if (0 != rtn)
                {
                    System.Threading.Thread.Sleep(1000);
                }

            } while (0 != rtn);
            foreach (var machine in m_machineQueue)
            {
                Thread t_dataacquire = new Thread(new ParameterizedThreadStart(ProcAcquireData));
                t_dataacquire.Start(machine);       
            }
            // // db.CloseConn(); // 20210528新增关闭数据库
        }
        #endregion

        #region 线程函数，用于获取数据
        //public void ProcAcquireMachineData(object obj)
        //{
        //    NCMachine pNCMachine = new NCMachine();
        //    NCMachine pNCMachineData = obj as NCMachine;
        //    //    int rtn = -1;
        //    //    bool btn = false;
        //    while(!mre.WaitOne(2))
        //    {
        //        GetFromMachineQueue(out pNCMachine);
        //        if (pNCMachine != null)
        //        {
        //            pNCMachineData = pNCMachine;

        //            for(int i = 0;i<m_machineDataQueue.Count;i++)
        //            {
        //                if(m_machineDataQueue[i].machnum == pNCMachineData.machnum)
        //                {
        //                    m_machineDataQueue[i] = pNCMachineData;
        //                    return;
        //                }
        //            }
        //        }
        //    }

        //}
        #endregion

        #region 采集主函数
        public void ProcAcquireData(object obj)
        {
            int rtn = -1;
            bool btn = false;
            NCMachine pNCMachine = obj as NCMachine;
            NCMachine pNCMachineData = new NCMachine();

            while (!mre.WaitOne(2))
            {
                    try
                    {

                        //GetFromMachineQueue(out pNCMachine);
                        if (pNCMachine != null)
                        {
                            pNCMachineData = pNCMachine;
                            btn = SelZeroPro(pNCMachine);
                            if (btn == true)
                            {
                                ResetTimeRecord(pNCMachine);
                            }

                            rtn = pNCMachine.UpdateData();

                            string date = ConstDefine.GetCurrentTimeString();
                            if (pNCMachine.flag_statuschange == 0 && pNCMachine.status == MachStatus.MS_INIT)
                            {
                                m_pMainDlg.textBox_sysinfo.AppendText(date + ":" + pNCMachine.machnum + "初始化……\r\n");
                            }
                            else if (pNCMachine.flag_statuschange == 1)
                            {
                                switch (pNCMachine.status)
                                {
                                    case MachStatus.MS_INIT:
                                        m_pMainDlg.textBox_sysinfo.AppendText(date + ":" + pNCMachine.machnum + "初始化……\r\n");
                                        break;
                                    case MachStatus.MS_OFFLINE:
                                        m_pMainDlg.textBox_sysinfo.AppendText(date + ":" + pNCMachine.machnum + "断开连接\r\n");
                                        break;
                                    case MachStatus.MS_ONLINE:
                                        m_pMainDlg.textBox_sysinfo.AppendText(date + ":" + pNCMachine.machnum + "连接成功\r\n");
                                        break;
                                    case MachStatus.MS_STOP:
                                        m_pMainDlg.textBox_sysinfo.AppendText(date + ":" + pNCMachine.machnum + "停止采集\r\n");
                                        break;
                                    default: break;
                                }
                            }

                            if (pNCMachine.itype == (int)MachineType.MOXA1242)
                            {
                                //更新电流传感器中的阈值
                            }
                            if (rtn == 0)
                            {
                                pNCMachine.status = MachStatus.MS_ONLINE;
                                //运行设置
                                GetRunSet(pNCMachine);
                                //有效工时
                                int ret = SelRoomMachineID(pNCMachine);
                                if (ret == 0)//有人员上下机变化
                                {
                                    try
                                    {
                                        InsertWorkTimeToDB(pNCMachine);//有效工时插入
                                    }
                                    catch (Exception ex)
                                    {
                                        ;
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        UpdatePatternTime(pNCMachine);//更新时间信息
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.WriteLogErrMsg("UpdatePatternTime函数错误");
                                    }
                                    //甘特图
                                    try
                                    {
                                        GetMachStatus_Gantt(pNCMachine);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.WriteLogErrMsg("GetMachStatus_Gantt函数错误");
                                    }
                                    //界面运行设置
                                    if (pNCMachine.runstatus == 1)
                                    {
                                        //符合运行设置条件
                                        pNCMachine.set_flag = 1;
                                    }
                                    else if (pNCMachine.runstatus == 0)
                                    {
                                        //不符合运行设置条件
                                        pNCMachine.set_flag = 0;
                                    }

                                    //Golding
                                    if (pNCMachine.itype == (int)MachineType.GOLDING)
                                    {
                                        GetGoldingErrorToLog(pNCMachine);
                                        if (pNCMachine.mach_info.error_handle != "" || pNCMachine.mach_info.error_motion != "" || pNCMachine.mach_info.error_plc != "")
                                        {
                                            pNCMachine.flag_error_state = 1;
                                        }
                                        else
                                        {
                                            pNCMachine.flag_error_state = 0;
                                        }
                                    }
                                    //Rexroth
                                    else if (pNCMachine.itype == (int)MachineType.REXROTH)
                                    {
                                        try
                                        {
                                            GetRexrothErrorToLog(pNCMachine);
                                            if (pNCMachine.mach_info.error_id > 0)
                                            {
                                                //标识错误在主界面
                                                pNCMachine.flag_error_state = 1;
                                            }
                                            else
                                            {
                                                pNCMachine.flag_error_state = 0;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ;
                                        }
                                    }

                                    //QCMTT
                                    else if (pNCMachine.itype == (int)MachineType.QCMTT)
                                    {
                                        try
                                        {
                                            GetQcmttErrorToLog(pNCMachine);
                                            if (pNCMachine.mach_info.plc_warningNumber > 0)
                                            {
                                                //标识错误在主界面
                                                pNCMachine.flag_error_state = 1;
                                            }
                                            else
                                            {
                                                pNCMachine.flag_error_state = 0;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ;
                                        }
                                    }

                                    //HNC8
                                    else if (pNCMachine.itype == (int)MachineType.HNC8)
                                    {
                                        try
                                        {
                                            // 暂时未采集，这部分有问题
                                            //GetHNC8ErrorToLog(pNCMachine);
                                            //if (pNCMachine.Hnc_AlmList.Count != 0)
                                            //{
                                            //    //标识错误在主界面
                                            //    pNCMachine.flag_error_state = 1;
                                            //}
                                            //else
                                            //{
                                            //    pNCMachine.flag_error_state = 0;
                                            //}
                                        }
                                        catch (Exception ex)
                                        {
                                            ;
                                        }
                                    }

                                }
                                //更新machine_log_today
                                if (pNCMachine.delta_time > 0)
                                {
                                    SetStorageDBQuery(pNCMachine);
                                    if (pNCMachine.runstatus == 1)
                                    {
                                        //符合运行设置条件
                                        pNCMachine.set_flag = 1;
                                    }
                                    else if (pNCMachine.runstatus == 0)
                                    {
                                        pNCMachine.set_flag = 0; //不符合运行设置条件
                                    }

                                }
                                //程序日志 工件数的变化作为程序结束标志
                                if (pNCMachine.mach_info.current_line != 0 && pNCMachine.currentlineLast == -1 && (pNCMachine.mach_info.interp_state == 1))//刚开启采集系统，,且程序开始运行，且当前程序行号不为0，记录为程序开始
                                {
                                    pNCMachine.progflag = 0;
                                }
                                else if (pNCMachine.mach_info.current_line != 0 && pNCMachine.currentlineLast == 0 && pNCMachine.mach_info.interp_state == 1)
                                {
                                    pNCMachine.progflag = 0;
                                    pNCMachine.proglog_starttime = pNCMachine.now_time;
                                    InsertProgLog(pNCMachine, 0);
                                }//程序开始加工（切换程序后）
                                else if (pNCMachine.mach_info.current_line == 0 && pNCMachine.mach_info.work_piece - pNCMachine.workpiceclast == 1 && pNCMachine.workpiceclast != -1 && pNCMachine.mach_info.interp_state == 3)//空闲
                                {
                                    pNCMachine.progflag = 1;
                                    pNCMachine.proglog_endtime = pNCMachine.now_time;
                                    InsertProgLog(pNCMachine, 1);
                                }//程序结束加工

                                #region 注销
                                //if (pNCMachine.itype < (int)MachineType.FANUC_MD)
                                //{
                                //    if (pNCMachine.mach_info.current_line != 0 && pNCMachine.currentlineLast == -1 && (pNCMachine.mach_info.interp_state == 1))//刚开启采集系统，,且程序开始运行，且当前程序行号不为0，记录为程序开始
                                //    {
                                //        pNCMachine.progflag = 0;
                                //    }
                                //    else if (pNCMachine.mach_info.current_line != 0 && pNCMachine.currentlineLast == 0 && pNCMachine.mach_info.interp_state == 1)
                                //    {
                                //        pNCMachine.progflag = 0;
                                //        pNCMachine.proglog_starttime = pNCMachine.now_time;
                                //        InsertProgLog(pNCMachine, 0);
                                //    }//程序开始加工（切换程序后）
                                //    else if (pNCMachine.mach_info.current_line == 0 && pNCMachine.mach_info.work_piece - pNCMachine.workpiceclast == 1 && pNCMachine.workpiceclast != -1 && (pNCMachine.mach_info.interp_state == 3 || pNCMachine.mach_info.interp_state == 2))//空闲
                                //    {
                                //        pNCMachine.progflag = 1;
                                //        pNCMachine.proglog_endtime = pNCMachine.now_time;
                                //        InsertProgLog(pNCMachine, 1);
                                //    }//程序结束加工
                                //}
                                //if (pNCMachine.itype == (int)MachineType.FANUC_MD || pNCMachine.itype == (int)MachineType.FANUC_MD_MATE)
                                //{
                                //    if (pNCMachine.mach_info.current_line != 0 && pNCMachine.currentlineLast == -1 && pNCMachine.mach_info.interp_state == 1)//刚开启采集系统，,且程序开始运行，且当前程序行号不为0，记录为程序开始
                                //    {
                                //        pNCMachine.progflag = 0;
                                //    }
                                //    else if (pNCMachine.mach_info.current_line != 0 && pNCMachine.currentlineLast == 0 && pNCMachine.mach_info.interp_state == 1)
                                //    {
                                //        pNCMachine.progflag = 0;
                                //        pNCMachine.proglog_starttime = pNCMachine.now_time;
                                //        InsertProgLog(pNCMachine, 0);
                                //    }//程序开始加工（切换程序后）
                                //    else if (pNCMachine.mach_info.work_piece - pNCMachine.workpiceclast == 1)
                                //    {
                                //        pNCMachine.progflag = 1;
                                //        pNCMachine.proglog_endtime = pNCMachine.now_time;
                                //        InsertProgLog(pNCMachine, 1);
                                //    }//程序结束加工
                                //}
                                #endregion

                                //程序日志添加的变量赋值
                                pNCMachine.prognameLast = pNCMachine.mach_info.prog_name;
                                pNCMachine.currentlineLast = pNCMachine.mach_info.current_line;
                                pNCMachine.interpLast = pNCMachine.mach_info.interp_state;
                                pNCMachine.workpiceclast = pNCMachine.mach_info.work_piece;
                            }
                            else
                            {
                                //离线时相关状态
                                //高精
                                if (pNCMachine.itype < (int)MachineType.FANUC_MD)
                                {
                                    if (pNCMachine.flag_error_state == 1)
                                    {
                                        ;//高精的故障暂时不写
                                    }
                                }
                                // 力士乐
                                else if (pNCMachine.itype == (int)MachineType.REXROTH)
                                {
                                    if (pNCMachine.flag_error_state == 1)
                                    {
                                        if (pNCMachine.status == MachStatus.MS_OFFLINE)
                                        {
                                            pNCMachine.ErrorEndTime = (int)pNCMachine.now_time;
                                            InsertRexrothErrToDB(pNCMachine, 1);
                                        }
                                    }
                                    /*****甘特图离线插入当前状态*******/
                                    if (pNCMachine.i_machstatus == 0 || pNCMachine.i_machstatus == 1 || pNCMachine.i_machstatus == 3)
                                    {
                                        pNCMachine.machstatus_endtime = pNCMachine.now_time;
                                        InsertMachStatus(pNCMachine);//插入数据库
                                        pNCMachine.i_machstatus = -1;//机器状态赋为离线，防止下次再进行置入值
                                    }
                                }
                                // 秦川
                                else if (pNCMachine.itype == (int)MachineType.QCMTT)
                                {
                                    if (pNCMachine.flag_error_state == 1)
                                    {
                                        if (pNCMachine.status == MachStatus.MS_OFFLINE)
                                        {
                                            pNCMachine.ErrorEndTime = (int)pNCMachine.now_time;
                                            InsertQcmttErrToDB(pNCMachine, 1);
                                        }
                                    }
                                    /*****甘特图离线插入当前状态*******/
                                    if (pNCMachine.i_machstatus == 0 || pNCMachine.i_machstatus == 1 || pNCMachine.i_machstatus == 3)
                                    {
                                        pNCMachine.machstatus_endtime = pNCMachine.now_time;
                                        InsertMachStatus(pNCMachine);//插入数据库
                                        pNCMachine.i_machstatus = -1;//机器状态赋为离线，防止下次再进行置入值
                                    }
                                }
                                // 华中
                                else if (pNCMachine.itype == (int)MachineType.HNC8)
                                {
                                    if (pNCMachine.flag_error_state == 1)
                                    {
                                        if (pNCMachine.status == MachStatus.MS_OFFLINE)
                                        {
                                            pNCMachine.ErrorEndTime = (int)pNCMachine.now_time;
                                            InsertHncError_1(pNCMachine, 1);
                                        }
                                    }
                                    /*****甘特图离线插入当前状态*******/
                                    if (pNCMachine.i_machstatus == 0 || pNCMachine.i_machstatus == 1 || pNCMachine.i_machstatus == 3)
                                    {
                                        pNCMachine.machstatus_endtime = pNCMachine.now_time;
                                        InsertMachStatus(pNCMachine);//插入数据库
                                        pNCMachine.i_machstatus = -1;//机器状态赋为离线，防止下次再进行置入值
                                    }
                                }

                                /*********************有效工时******************************/
                                int ret = SelRoomMachineID(pNCMachine);
                                if (ret == 0)
                                {
                                    try
                                    {
                                        InsertWorkTimeToDB(pNCMachine);
                                    }
                                    catch (Exception ex)
                                    {
                                        ;
                                    }
                                }
                                /***********************************************************/
                                else
                                {
                                    try
                                    {
                                        SetStorageDBQuery_OffLine(pNCMachine);
                                        btn = SelZeroPro(pNCMachine);
                                        if (btn == true)
                                        {
                                            ResetTimeRecord(pNCMachine);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ;
                                    }
                                }
                                SetStorageDBQuery_OffLine(pNCMachine);
                                pNCMachine.status = MachStatus.MS_OFFLINE;
                            }
                            if (pNCMachine.status_last == pNCMachine.status)
                            {
                                pNCMachine.flag_statuschange = 0;
                            }
                            else
                            {
                                pNCMachine.status_last = pNCMachine.status;
                                pNCMachine.flag_statuschange = 1;
                            }

                            Thread.Sleep(200);

                            pNCMachineData = pNCMachine;
                            //AddToMachineQueue(pNCMachine);
                            AddToMachineDataQueue(pNCMachineData);
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                        Thread.Sleep(1000);

                    }
                    catch (Exception e)
                    {
                        e.ToString();
                        //throw;
                    }
             }
        }
        #endregion

        #region 断线重连线程
        public void ProcReConnect()
        {
            int rtn = -1;
            NCMachine pNCMachine = new NCMachine();
            NCMachine pNCMachineData = new NCMachine();
            //ManualResetEvent mre = new ManualResetEvent(false);
            while (!mre.WaitOne(2))
            {
                GetFromMachineQueue(out pNCMachine);
                if (pNCMachine != null)
                {
                    pNCMachine.DelayConnect();
                    pNCMachineData = null;
                    AddToMachineQueue(pNCMachine);
                    AddToMachineDataQueue(pNCMachineData);
                    Thread.Sleep(500);
                }
                else
                {
                    Thread.Sleep(1);
                }

            }
        }
        #endregion

        #region 停止采集
        public void StopAcquireData()
        {
            mre.Set();
            Thread.Sleep(1000);
            _ClearQueue();
            Thread.Sleep(500);
        }
        #endregion

        #region 清空队列
        public void _ClearQueue()
        {
            NCMachine pNCMachine = new NCMachine();
            while (0 != m_machineQueue.Count)
            {
                pNCMachine = m_machineQueue.Dequeue();
                /*****甘特图*******/
                pNCMachine.machstatus_endtime = pNCMachine.now_time;
                InsertMachStatus(pNCMachine);//插入数据库
                /*****************/
                pNCMachine.DisConnectToNC();
            }
            while (0 != m_machineDataQueue.Count)
            {
                pNCMachine = m_machineDataQueue.Dequeue();
            }
        }
        #endregion

        #region 入和出队列函数群
        private void AddToMachineQueue(NCMachine pNCMachine)
        {
            lock (thisLock)
            {
                m_machineQueue.Enqueue(pNCMachine);
                m_iNumMachine++;
            }
        }
        private void GetFromMachineQueue(out NCMachine ppNCMachine)
        {
            if (m_machineQueue.Count != 0)
            {
                lock (thisLock)
                {
                    if (m_machineQueue.Count != 0)
                    {
                        ppNCMachine = m_machineQueue.Dequeue();
                        m_iNumMachine--;
                    }
                    else
                    {
                        ppNCMachine = null;
                    }

                }
            }
            else
            {
                lock (thisLock)
                {
                    ppNCMachine = null;
                }

            }
        }
        public void AddToMachineDataQueue(NCMachine pNCMachineData)
        {
            if (null == pNCMachineData)
            {
                return;
            }
            if (m_machineDataQueue.Count < m_iNumMachine)
            {
                lock (thisLockData)
                {
                    m_machineDataQueue.Enqueue(pNCMachineData);
                }
            }

        }
        public int GetFromMachineDataQueue(out NCMachine ppNCMachineData)
        {
            if (m_machineDataQueue.Count != 0)
            {
                lock (thisLockData)
                {
                    ppNCMachineData = m_machineDataQueue.Dequeue();
                    return 0;
                }
            }
            else
            {
                ppNCMachineData = null;
                return -1;
            }
        }
        #endregion

        #region 更新数据库机床实时信息
        private void SetStorageDBQuery(NCMachine pNCMachine)
        {

            //long nowtime = CONSTDEFINE.ConstDefine.GetCurrentTimeUnix();
            if (pNCMachine.status == MachStatus.MS_ONLINE)
            {
                //lst.Add(new ContentStruct { monitor = "DISP_MODE", value = pNCMachine.mach_info.disp_mode.ToString() });
                //lst.Add(new ContentStruct { monitor = "INTERP_STATE", value = pNCMachine.mach_info.interp_state.ToString() });
                //if (resultDict.ContainsKey(pNCMachine.machnum))
                //{
                //    resultDict[pNCMachine.machnum.ToString()] = lst;
                //    ms.WebInterface(resultDict);
                //}
                //else
                //{
                //    resultDict.Add(pNCMachine.machnum.ToString(), lst);
                //    ms.WebInterface(resultDict);
                //}
                //lst.Clear();

                pNCMachine.day_run_time = (long)Math.Floor(pNCMachine.day_poweron_time * 0.8);
                pNCMachine.day_cut_time = (long)Math.Floor(pNCMachine.day_poweron_time * 0.6);

                if (pNCMachine.ip.Equals("192.168.2.2")) {
                    pNCMachine.mach_info.work_piece = 466;//工件数量
                }
                if (pNCMachine.ip.Equals("192.168.2.3"))
                {
                    pNCMachine.mach_info.work_piece = 513;//工件数量
                }
                if (pNCMachine.ip.Equals("192.168.2.13"))
                {
                    pNCMachine.mach_info.work_piece = 626;//工件数量
                }
                if (pNCMachine.ip.Equals("192.168.2.5"))
                {
                    pNCMachine.mach_info.work_piece = 601;//工件数量
                }
                if (pNCMachine.ip.Equals("192.168.2.10"))
                {
                    pNCMachine.mach_info.work_piece = 572;//工件数量
                }
                if (pNCMachine.ip.Equals("192.168.2.17"))
                {
                    pNCMachine.mach_info.work_piece = 533;//工件数量
                }
                if (pNCMachine.ip.Equals("192.168.2.21"))
                {
                    pNCMachine.mach_info.work_piece = 681;//工件数量
                }
                if (pNCMachine.ip.Equals("192.168.2.29"))
                {
                    pNCMachine.mach_info.work_piece = 549;//工件数量
                }
                if (pNCMachine.ip.Equals("192.168.2.25"))
                {
                    pNCMachine.mach_info.work_piece = 610;//工件数量
                }
                if (pNCMachine.ip.Equals("192.168.2.32"))
                {
                    pNCMachine.mach_info.work_piece = 672;//工件数量
                }
                if (pNCMachine.ip.Equals("192.168.2.37"))
                {
                    pNCMachine.mach_info.work_piece = 611;//工件数量
                }

                string dbQuery = "update machine_log_today set now_time =" + pNCMachine.now_time.ToString() + ",delta_time = " + pNCMachine.delta_time;
                dbQuery += ",INTERP_STATE = " + pNCMachine.mach_info.interp_state.ToString() + ",DISP_MODE = " + pNCMachine.mach_info.disp_mode.ToString();
                dbQuery += ",HOST_VALUE1 = " + pNCMachine.mach_info.host_value1.ToString() + ",HOST_VALUE2 = " + pNCMachine.mach_info.host_value2.ToString() + ",HOST_VALUE3 = " + pNCMachine.mach_info.host_value3.ToString() + ",HOST_VALUE4 = " + pNCMachine.mach_info.host_value4.ToString() + ",HOST_VALUE5 = " + pNCMachine.mach_info.host_value5.ToString() + ",HOST_VALUE6 = " + pNCMachine.mach_info.host_value6.ToString();
                dbQuery += ",AHOST_VALUE1 = " + pNCMachine.mach_info.ahost_value1.ToString() + ",AHOST_VALUE2 = " + pNCMachine.mach_info.ahost_value2.ToString() + ",AHOST_VALUE3 = " + pNCMachine.mach_info.ahost_value3.ToString() + ",AHOST_VALUE4 = " + pNCMachine.mach_info.ahost_value4.ToString() + ",AHOST_VALUE5 = " + pNCMachine.mach_info.ahost_value5.ToString() + ",AHOST_VALUE6 = " + pNCMachine.mach_info.ahost_value6.ToString();
                dbQuery += ",DIST_TOGO1 = " + pNCMachine.mach_info.dist_togo1.ToString() + ",DIST_TOGO2 = " + pNCMachine.mach_info.dist_togo2.ToString() + ",DIST_TOGO3 = " + pNCMachine.mach_info.dist_togo3.ToString() + ",DIST_TOGO4 = " + pNCMachine.mach_info.dist_togo4.ToString() + ",DIST_TOGO5 = " + pNCMachine.mach_info.dist_togo5.ToString() + ",DIST_TOGO6 = " + pNCMachine.mach_info.dist_togo6.ToString();
                dbQuery += ",FEED_SPEED = " + pNCMachine.mach_info.feed_speed.ToString() + ",AFEED_SPEED = " + pNCMachine.mach_info.afeed_speed.ToString() + ",DFEED_SPEED = " + pNCMachine.mach_info.dfeed_speed.ToString();
                dbQuery += ",SPINDLE_SPEED = " + pNCMachine.mach_info.spindle_speed.ToString() + ",ASPINDLE_SPEED = " + pNCMachine.mach_info.aspindle_speed.ToString() + ",DSPINDLE_SPEED = " + pNCMachine.mach_info.dspindle_speed.ToString() + ",SPIN_TORQ = " + pNCMachine.mach_info.spin_torq.ToString();
                dbQuery += ",PROG_NAME ='" + pNCMachine.mach_info.prog_name + "',CURRENT_LINE =" + pNCMachine.mach_info.current_line.ToString();
                dbQuery += ",WORK_PIECE =" + pNCMachine.mach_info.work_piece.ToString() + ",AXIS_NUM =" + pNCMachine.mach_info.axis_num.ToString() + ",ERROR_ID =" + pNCMachine.mach_info.error_id.ToString();
                dbQuery += ",total_poweron_time =" + pNCMachine.mach_info.total_poweron_time.ToString() + ",total_run_time =" + pNCMachine.mach_info.total_run_time.ToString() + ",total_cut_time =" + pNCMachine.mach_info.total_cut_time.ToString();
                dbQuery += ",today_poweron_time =" + pNCMachine.today_poweron_time.ToString() + ",today_run_time =" + pNCMachine.today_run_time.ToString() + ",today_cut_time =" + pNCMachine.today_cut_time.ToString();
                dbQuery += ",day_poweron_time =" + pNCMachine.day_poweron_time.ToString() + ",day_run_time =" + pNCMachine.day_run_time.ToString() + ",day_cut_time =" + pNCMachine.day_cut_time.ToString();
                dbQuery += ",SOFTVERSION ='" + pNCMachine.mach_info.softversion.ToString() + "',RUN_TIME =" + pNCMachine.mach_info.run_time.ToString() + ",MACHINE_ONLINE =1" + ",ERROR_STATE = " + pNCMachine.mach_info.error_flag.ToString();
                dbQuery += " where mach_num='" + pNCMachine.machnum + "'";

                //Console.WriteLine("pNCMachine.mach_info.programmed_over:" + pNCMachine.mach_info.programmed_over);
                //MessageBox.Show(dbQuery);

                db.ExecuteSQL(dbQuery);

                /* -- 正常持续发送部分
                lst.clear();
                lst.add(new contentstruct { monitor = "disp_mode", value = pncmachine.mach_info.disp_mode.tostring() });
                lst.add(new contentstruct { monitor = "interp_state", value = pncmachine.mach_info.interp_state.tostring() });
                if (resultdict.containskey(pncmachine.machnum)) 
                {
                    resultdict[pncmachine.machnum.tostring()] = lst;
                    lst.clear();
                }
                else
                {
                    resultdict.add(pncmachine.machnum.tostring(), lst);
                    lst.clear();
                }
                resultdict.add(pncmachine.machnum.tostring(), lst);
                ms.webinterface(resultdict, pncmachine.machnum.tostring() );
                
                **/

              

                //#region 订阅发送数据
                ////lst.Clear();
                ////lst_last.Clear();
                //lst.Add(new ContentStruct { monitor = "DISP_MODE", value = pNCMachine.mach_info.disp_mode.ToString() });
                //lst.Add(new ContentStruct { monitor = "INTERP_STATE", value = pNCMachine.mach_info.interp_state.ToString() });
                //if (resultDict.ContainsKey(pNCMachine.machnum) && resultDictLast.ContainsKey(pNCMachine.machnum))
                //{
                //    resultDict[pNCMachine.machnum.ToString()] = lst;
                //    foreach (var item in resultDict[pNCMachine.machnum.ToString()])
                //    {
                //        bool exitLoop = false;
                //        foreach (var item_last in resultDictLast[pNCMachine.machnum.ToString()])
                //        {
                //            if (item.monitor == item_last.monitor)
                //            {
                //                if (item.value != item_last.value)
                //                {
                //                    ms.WebInterface(resultDict, pNCMachine.machnum.ToString());
                //                    exitLoop = true;
                //                    break;
                //                }
                //            }
                //        }
                //        if (exitLoop)
                //        {
                //            break;
                //        }

                //    }

                //    lst_last.Clear();
                //    lst_last.Add(new ContentStruct { monitor = "DISP_MODE", value = pNCMachine.mach_info.disp_mode.ToString() });
                //    lst_last.Add(new ContentStruct { monitor = "INTERP_STATE", value = pNCMachine.mach_info.interp_state.ToString() });
                //    resultDictLast[pNCMachine.machnum.ToString()] = lst_last;
                //}
                //else
                //{
                //    resultDict.Add(pNCMachine.machnum.ToString(), lst);
                //    ms.WebInterface(resultDict, pNCMachine.machnum.ToString());
                //    if (lst_last.Count == 0)
                //    {
                //        lst_last.Add(new ContentStruct { monitor = "DISP_MODE", value = pNCMachine.mach_info.disp_mode.ToString() });
                //        lst_last.Add(new ContentStruct { monitor = "INTERP_STATE", value = pNCMachine.mach_info.interp_state.ToString() });
                //    }
                //    resultDictLast.Add(pNCMachine.machnum.ToString(), lst_last); // 添加当前值，为下一次发送数据做准备
                //}
                ////lst.Clear();
                ////lst_last.Clear();
                //#endregion

                //lst.Add(new ContentStruct { monitor = "DISP_MODE", value = pNCMachine.mach_info.disp_mode.ToString() });
                ////lst.Add(new ContentStruct { monitor = "INTERP_STATE", value = pNCMachine.runstatus.ToString() });
                //lst.Add(new ContentStruct { monitor = "INTERP_STATE", value = pNCMachine.mach_info.interp_state.ToString() });
                ////lst.Add(new ContentStruct { monitor = "PROGRAMMED_OVER", value = pNCMachine.mach_info.programmed_over.ToString() });
                ////resultDict.Add(pNCMachine.machnum.ToString(), lst);
                ////// 定义一个全局变量的字典，字典的键是对应machineNo，值对应上一次采集数据的值
                ////List<ContentStruct> oldValueLst = null;
                //// 增加字典判断，唯一key值
                //if (resultDict.ContainsKey(pNCMachine.machnum) && resultDictLast.ContainsKey(pNCMachine.machnum))
                //{

                //    resultDict[pNCMachine.machnum.ToString()] = lst;

                //    foreach (var item in resultDict[pNCMachine.machnum.ToString()])
                //    {
                //        bool exitLoop = false;
                //        foreach (var item_last in resultDictLast[pNCMachine.machnum.ToString()])
                //        {
                //            if (item.monitor == item_last.monitor)
                //            {
                //                if (item.value != item_last.value)
                //                {
                //                    ms.WebInterface(resultDict, pNCMachine.machnum.ToString());
                //                    exitLoop = true;
                //                    break;
                //                }
                //            }
                //        }
                //        if (exitLoop)
                //        {
                //            break;
                //        }

                //    }

                //    lst_last.Clear();
                //    lst_last.Add(new ContentStruct { monitor = "DISP_MODE", value = pNCMachine.mach_info.disp_mode.ToString() });
                //    lst_last.Add(new ContentStruct { monitor = "INTERP_STATE", value = pNCMachine.mach_info.interp_state.ToString() });
                //    resultDictLast[pNCMachine.machnum.ToString()] = lst_last;


                //}
                //else
                //{
                //    resultDict.Add(pNCMachine.machnum.ToString(), lst);
                //    ms.WebInterface(resultDict, pNCMachine.machnum.ToString());
                //    if (lst_last.Count == 0)
                //    {
                //        lst_last.Add(new ContentStruct { monitor = "DISP_MODE", value = pNCMachine.mach_info.disp_mode.ToString() });
                //        lst_last.Add(new ContentStruct { monitor = "INTERP_STATE", value = pNCMachine.mach_info.interp_state.ToString() });
                //    }
                //    resultDictLast.Add(pNCMachine.machnum.ToString(), lst_last); // 添加当前值，为下一次发送数据做准备
                //}
                ////ms.WebInterface(resultDict);
                ////oldValueLst = resultDict[pNCMachine.machnum.ToString()];
                //lst.Clear();

                //pNCMachine.webdic["now_time"] = pNCMachine.now_time.ToString();
                //pNCMachine.webdic["delta_time"] = pNCMachine.delta_time.ToString();
                //pNCMachine.webdic["INTERP_STATE"] = pNCMachine.runstatus.ToString();
                //pNCMachine.webdic["DISP_MODE"] = pNCMachine.mach_info.disp_mode.ToString();
                //pNCMachine.webdic["HOST_VALUE1"] = pNCMachine.mach_info.host_value1.ToString();
                //pNCMachine.webdic["HOST_VALUE2"] = pNCMachine.mach_info.host_value2.ToString();
                //pNCMachine.webdic["HOST_VALUE3"] = pNCMachine.mach_info.host_value3.ToString();
                //pNCMachine.webdic["HOST_VALUE4"] = pNCMachine.mach_info.host_value4.ToString();
                //pNCMachine.webdic["HOST_VALUE5"] = pNCMachine.mach_info.host_value5.ToString();
                //pNCMachine.webdic["HOST_VALUE6"] = pNCMachine.mach_info.host_value6.ToString();
                //pNCMachine.webdic["AHOST_VALUE1"] = pNCMachine.mach_info.ahost_value1.ToString();
                //pNCMachine.webdic["AHOST_VALUE2"] = pNCMachine.mach_info.ahost_value2.ToString();
                //pNCMachine.webdic["AHOST_VALUE3"] = pNCMachine.mach_info.ahost_value3.ToString();
                //pNCMachine.webdic["AHOST_VALUE4"] = pNCMachine.mach_info.ahost_value4.ToString();
                //pNCMachine.webdic["AHOST_VALUE5"] = pNCMachine.mach_info.ahost_value5.ToString();
                //pNCMachine.webdic["AHOST_VALUE6"] = pNCMachine.mach_info.ahost_value6.ToString();
                //pNCMachine.webdic["DIST_TOGO1"] = pNCMachine.mach_info.dist_togo1.ToString();
                //pNCMachine.webdic["DIST_TOGO2"] = pNCMachine.mach_info.dist_togo1.ToString();
                //pNCMachine.webdic["DIST_TOGO3"] = pNCMachine.mach_info.dist_togo1.ToString();
                //pNCMachine.webdic["DIST_TOGO4"] = pNCMachine.mach_info.dist_togo1.ToString();
                //pNCMachine.webdic["DIST_TOGO5"] = pNCMachine.mach_info.dist_togo1.ToString();
                //pNCMachine.webdic["DIST_TOGO6"] = pNCMachine.mach_info.dist_togo1.ToString();
                //pNCMachine.webdic["FEED_SPEED"] = pNCMachine.mach_info.feed_speed.ToString();
                //pNCMachine.webdic["AFEED_SPEED"] = pNCMachine.mach_info.afeed_speed.ToString();
                //pNCMachine.webdic["DFEED_SPEED"] = pNCMachine.mach_info.dfeed_speed.ToString();
                //pNCMachine.webdic["SPINDLE_SPEED"] = pNCMachine.mach_info.spindle_speed.ToString();
                //pNCMachine.webdic["ASPINDLE_SPEED"] = pNCMachine.mach_info.aspindle_speed.ToString();
                //pNCMachine.webdic["DSPINDLE_SPEED"] = pNCMachine.mach_info.dspindle_speed.ToString();
                //pNCMachine.webdic["SPIN_TORQ"] = pNCMachine.mach_info.spin_torq.ToString();
                //pNCMachine.webdic["PROG_NAME"] = pNCMachine.mach_info.prog_name.ToString();
                //pNCMachine.webdic["CURRENT_LINE"] = pNCMachine.mach_info.current_line.ToString();
                //pNCMachine.webdic["WORK_PIECE"] = pNCMachine.mach_info.work_piece.ToString();
                //pNCMachine.webdic["AXIS_NUM"] = pNCMachine.mach_info.axis_num.ToString();
                //pNCMachine.webdic["ERROR_ID"] = pNCMachine.mach_info.error_id.ToString();
                //pNCMachine.webdic["total_poweron_time"] = pNCMachine.mach_info.total_poweron_time.ToString();
                //pNCMachine.webdic["total_run_time"] = pNCMachine.mach_info.total_run_time.ToString();
                //pNCMachine.webdic["total_cut_time"] = pNCMachine.mach_info.total_cut_time.ToString();
                //pNCMachine.webdic["today_poweron_time"] = pNCMachine.today_poweron_time.ToString();
                //pNCMachine.webdic["today_run_time"] = pNCMachine.today_run_time.ToString();
                //pNCMachine.webdic["today_cut_time"] = pNCMachine.today_cut_time.ToString();
                //pNCMachine.webdic["day_poweron_time"] = pNCMachine.day_poweron_time.ToString();
                //pNCMachine.webdic["day_run_time"] = pNCMachine.day_run_time.ToString();
                //pNCMachine.webdic["day_cut_time"] = pNCMachine.day_cut_time.ToString();
                //pNCMachine.webdic["SOFTVERSION"] = pNCMachine.mach_info.softversion.ToString();
                //pNCMachine.webdic["RUN_TIME"] = pNCMachine.mach_info.run_time.ToString();
                //pNCMachine.webdic["MACHINE_ONLINE"] = "1";
                //string Contentjson = JsonConvert.SerializeObject(lst);
                //暂时注释
                //webserviceCNC.DataUpdate(pNCMachine.machnum, Contentjson);
            }
        }
        #endregion

        #region 更新数据库机床离线信息
        private void SetStorageDBQuery_OffLine(NCMachine pNCMachine)
        {
            //System.DateTime currentTime = new System.DateTime();
            //currentTime = System.DateTime.Now;
            //String date;
            //date = String.Format("%{}4d-%2d-%2d %2d:%2d:%2d", currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, currentTime.Second);
            //date = string.Format("{0:yyyy-MM-dd HH:mm:ss}", currentTime);
            //String nowtime = "to_date('" + date + "','yyyy-MM-dd HH24:mi:ss')";
            int NowTime = (int)ConstDefine.GetCurrentTimeUnix();
            string dbQuery = "update machine_log_today set now_time =" + NowTime + ",MACHINE_ONLINE = 0";
            dbQuery += " where mach_num='" + pNCMachine.machnum + "'";
            db.ExecuteSQL(dbQuery);
            //pNCMachine.webdic["now_time"] = pNCMachine.now_time.ToString();
            //pNCMachine.webdic["MACHINE_ONLINE"] = "0";
        }
        #endregion

        #region 查询零点插入的存储过程是否执行成功
        /*
        *功能描述: 查询零点插入的存储过程是否执行成功
        *参数:
        *  
        *返回值:true 成功 ，false 失败 
        */
        public bool SelZeroPro(NCMachine pNCMachine)
        {
            bool ret = false;
            int t_flag = -1;
            DataSet ds_worktimeflag = new DataSet();
            string sql;
            sql = "select flag from WORKTIME_FLAG where mach_num='" + pNCMachine.machnum + "'";
            ds_worktimeflag = db.ReturnDataSet(sql);
            try
            {
                t_flag = Convert.ToInt32(ds_worktimeflag.Tables[0].Rows[0]["FLAG"].ToString());
                if (t_flag == 1)
                {
                    ret = true;
                }
                else if (t_flag == 0)
                {
                    ret = false;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLogErrMsg(sql + "失败！"); ;
            }
            return ret;
        }
        #endregion

        #region 复位machine_log_today中的today_poweron_time,today_run_time,today_cut_time,day_poweron_time,day_run_time,day_cut_time
        /*
        *功能描述: 复位machine_log_today中的today_poweron_time,today_run_time,today_cut_time,day_poweron_time,day_run_time,day_cut_time
        *           同时复位worktime_flag中的此机器的flag数据项
        *参数:
        *  
        *返回值:true 成功 ，false 失败 
        */
        public void ResetTimeRecord(NCMachine pNCMachine)
        {
            int err = -1;
            string dbQuery = "update worktime_flag set flag = 0 where mach_num='" + pNCMachine.machnum + "'";
            err = db.ExecuteSQL(dbQuery);
            if (err == 0)//执行成功
            {
                long initialtime = 0;
                initialtime = GetNowSecondsFromZero();
                pNCMachine.today_poweron_time = initialtime;
                pNCMachine.today_run_time = 0;
                pNCMachine.today_cut_time = 0;
                pNCMachine.day_poweron_time = initialtime;
                //pNCMachine->day_poweron_time = 0;
                pNCMachine.day_run_time = 0;
                pNCMachine.day_cut_time = 0;
                if (pNCMachine.status == MachStatus.MS_OFFLINE)
                {
                    dbQuery = "update machine_log_today set day_poweron_time = 0,day_run_time=0,day_cut_time=0,today_poweron_time=0,today_run_time=0,today_cut_time=0 where mach_num='" + pNCMachine.machnum + "'";
                }
                else if (pNCMachine.status == MachStatus.MS_ONLINE)
                {
                    dbQuery = "update machine_log_today set day_poweron_time = " + pNCMachine.day_poweron_time + ",day_run_time=" + pNCMachine.day_run_time + ",day_cut_time=" + pNCMachine.day_cut_time + ",today_poweron_time="
                        + pNCMachine.today_poweron_time + ",today_run_time=" + pNCMachine.today_run_time + ",today_cut_time=" + pNCMachine.today_cut_time + " where mach_num='" + pNCMachine.machnum + "'";
                }
                try
                {
                    db.ExecuteSQL(dbQuery);
                    err = 0;
                }
                catch (Exception e)
                {
                    // txtstr.Format(_T("【插入故障系统失败!】"));
                    // writeerrortxt.WriteBook(txtstr);
                    err = -1;
                }
            }
            else
            {
                ;
            }
        }
        /*
       *功能描述: 获取当前时间到0点的秒数
       *参数:
       *  
       *返回值:今天已过的秒数 
       */
        public long GetNowSecondsFromZero()
        {
            long l_deltaTime = 0, l_nowTime;
            l_nowTime = ConstDefine.GetCurrentTimeUnix();
            TimeSpan cha = (System.DateTime.Today - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
            l_deltaTime = (long)cha.TotalSeconds;
            long delta = l_nowTime - l_deltaTime;
            return delta;
        }

        #endregion

        #region 运行设置
        /*
        *功能描述: 获取运行设置的设置参数并进行逻辑判断做出符合运行
        *           设置条件和不符合运行设置条件存储在pNCMachine->runstatus中
        *           1 符合运行设置条件，0不符合运行设置条件
        *返回值: 0 成功，-1 失败.
        */
        public int GetRunSet(NCMachine pNCMachine)
        {
            int err = -1;
            int ret;
            double t_min, t_max, f_min, f_max;
            int prog_exist, s_min, s_max, ds_min, ds_max, df_min, df_max;
            int s_exist = 0, f_exist = 0, t_exist = 0;
            int f_isnormal = 0, s_isnormal = 0, t_isnormal = 0; //判断结果是否正常
            int e_isacquire = 0, df_isnormal = 0, ds_isnormal = 0;
            string s_runtable = "run_set_" + pNCMachine.machnum;
            string dbQuery;
            DataSet ds_runset = new DataSet();
            //判断是否存在程序
            int isPrg = 0;
            dbQuery = "select * from " + s_runtable;

            try
            {
                ds_runset = db.ReturnDataSet(dbQuery);
                t_min = Convert.ToDouble(ds_runset.Tables[0].Rows[0]["torq_min"].ToString());
                t_max = Convert.ToDouble(ds_runset.Tables[0].Rows[0]["torq_max"].ToString());
                s_min = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["spindle_min"].ToString());
                s_max = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["spindle_max"].ToString());
                f_min = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["feed_min"].ToString());
                f_max = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["feed_max"].ToString());
                prog_exist = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["program_exist"].ToString());//1需要判断程序是否存在，0不需要
                ds_min = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["m_dspindle_min"].ToString());
                ds_max = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["m_dspindle_max"].ToString());
                df_min = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["m_dfeed_min"].ToString());
                df_max = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["m_dfeed_max"].ToString());
                s_exist = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["spindle_zone"].ToString());
                f_exist = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["feed_zone"].ToString());
                t_exist = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["torq_zone"].ToString());
                e_isacquire = Convert.ToInt32(ds_runset.Tables[0].Rows[0]["efficiency"].ToString());
                ////////////////////////高低速报警增加内容/////////////////////////
                pNCMachine.Flarge = df_max;
                pNCMachine.Fsmall = df_min;
                pNCMachine.Slarge = ds_max;
                pNCMachine.Ssmall = ds_min;
                pNCMachine.Llarge = t_max;
                pNCMachine.Lsmall = t_min;
                ///////////////////////////////////////////////////////////////////
                if ((pNCMachine.mach_info.interp_state == 1))  //模式是运行状态
                {
                    if (f_exist == 1)
                    {
                        if (pNCMachine.mach_info.feed_speed >= f_min && pNCMachine.mach_info.feed_speed <= f_max)
                        {
                            f_isnormal = 1; //1在范围内
                        }
                        else
                        {
                            f_isnormal = 0;
                        }
                    }
                    else
                    {
                        f_isnormal = 1; //1在范围内
                    }
                    if (s_exist == 1)
                    {
                        if (pNCMachine.mach_info.spindle_speed >= s_min && pNCMachine.mach_info.spindle_speed <= s_max)
                        {
                            s_isnormal = 1;
                        }
                        else
                        {
                            s_isnormal = 0;
                        }
                    }
                    else
                    {
                        s_isnormal = 1; //1在范围内
                    }
                    if (t_exist == 1)
                    {
                        t_isnormal = JudgeTorq(pNCMachine.mach_info.spin_torq, t_min, t_max);
                    }
                    else
                    {
                        t_isnormal = 1; //1在范围内
                    }
                    if (prog_exist == 1)
                    {
                        //判断程序名是否为空                   
                        if (pNCMachine.mach_info.prog_name == "")
                        {
                            isPrg = 0;
                        }
                        else
                        {
                            isPrg = 1;
                        }
                    }
                    else
                    {
                        isPrg = 1;
                    }
                    if (pNCMachine.mach_info.dfeed_speed >= df_min && pNCMachine.mach_info.dfeed_speed <= df_max)
                    {
                        df_isnormal = 1;
                    }
                    else
                    {
                        df_isnormal = 0;
                    }
                    if (pNCMachine.mach_info.dspindle_speed >= ds_min && pNCMachine.mach_info.dspindle_speed <= ds_max)
                    {
                        ds_isnormal = 1;
                    }
                    else
                    {
                        ds_isnormal = 0;
                    }

                    if (f_isnormal == 1 && s_isnormal == 1 && t_isnormal == 1 && ds_isnormal == 1 && df_isnormal == 1)
                    {
                        if (pNCMachine.runstatus == 0)//上一个状态是不符合运行设置，更新结束时间和异常持续时间
                        {
                            pNCMachine.runstatus = 1; //符合条件的运行
                            //update end time and last time of setalarm
                            InsertSetAlarm(pNCMachine, 1);
                        }
                        else
                        {
                            pNCMachine.runstatus = 1; //符合条件的运行
                        }
                    }
                    else
                    {
                        if (pNCMachine.runstatus == 1)//上一个状态是符合运行设置，插入相关信息
                        {
                            pNCMachine.runstatus = 0; //不符合条件的运行		
                            //record aspindlespeed,afeedspeed,dfeedspeed,dspindlespeed starttime
                            InsertSetAlarm(pNCMachine, 0);
                        }
                        else if (pNCMachine.runstatus == 2)
                        {
                            pNCMachine.runstatus = 0; //不符合条件的运行	
                            InsertSetAlarm(pNCMachine, 0);
                        }
                    }
                }
                else if ((pNCMachine.mach_info.interp_state == 3))
                {
                    if (pNCMachine.runstatus == 0)//上一次是不符合运行条件
                    {
                        InsertSetAlarm(pNCMachine, 1);
                    }
                    pNCMachine.runstatus = 2;  //停止
                }
            }
            catch (Exception ex)
            {
                err = -1;
                Log.WriteLogErrMsg(dbQuery + "失败！");
            }

            return err;
        }

        /*
        *功能描述: 判断主轴功率是否满足设置的要求，即是否为有效工时
        *返回值: 0 成功，-1 失败.
        */

        public int JudgeTorq(double t, double min, double max)
        {
            int rtn = 0;
            if (t >= min && t <= max)
            {
                rtn = 1;
            }
            else
            {
                rtn = 0;
            }
            return rtn;
        }

        /*
        *功能描述: 插入数据库中高低速报警的信息记录
        *参数：flag 0插入开始时间，1插入结束时间和持续时间
        *返回值: 无
        */
        public int InsertSetAlarm(NCMachine mach, int flag)
        {
            int err = -1;
            long l_lasttime;
            string dbQuery;
            string s_nowtime = ConstDefine.GetCurrentTimeString();
            if (flag == 0)//插入开始时间
            {
                mach.alarm_fstartime = ConstDefine.GetCurrentTimeUnix();
                String s_endtime = "to_date('" + s_nowtime + "','yyyy-MM-dd HH24:mi:ss')";
                String s_starttime = "to_date('" + s_nowtime + "','yyyy-MM-dd HH24:mi:ss')";
                dbQuery = "insert into speedalarm_" + mach.machnum + " (mach_num,min_dfeed,max_dfeed,min_spindle,max_spindle,min_load,max_load,afeed,dfeed,aspindle,dspindle,load,start_time,end_time,lasttime) values('" +
                    mach.machnum + "'," + mach.Fsmall + "," + mach.Flarge + "," + mach.Ssmall + "," + mach.Slarge + "," + mach.Lsmall + "," + mach.Llarge + "," + mach.mach_info.afeed_speed + "," + mach.mach_info.dfeed_speed + "," + mach.mach_info.aspindle_speed + "," + mach.mach_info.spin_torq + "," + s_endtime + "," + s_starttime + ",0)";
            }
            else//更新结束时间
            {
                mach.alarm_fstartime = ConstDefine.GetCurrentTimeUnix();
                l_lasttime = mach.alarm_fendtime - mach.alarm_fstartime;
                String s_starttime = ConstDefine.GetDateTime((int)mach.alarm_fstartime);
                String s_endtime = "to_date('" + s_nowtime + "','yyyy-MM-dd HH24:mi:ss')";
                dbQuery = "update speedalarm_" + mach.machnum + " set end_time = " + s_endtime + ",lasttime = " + l_lasttime + " where start_time = " + s_starttime;

            }
            try
            {
                db.ExecuteSQL(dbQuery);
                err = 0;
            }
            catch (Exception ex)
            {
                err = -1;
                Log.WriteLogErrMsg(dbQuery + "失败！");
            }
            return err;
        }
        #endregion

        #region 有效工时函数
        /*
        *功能描述: 比较机器操作人员是否出现变化    
        *返回值: 相同返回1，不相同返回0,失败返回-1
        */
        public int SelRoomMachineID(NCMachine pNCMachine)
        {
            int err = -1;
            string dbQuery;
            DataSet ds_userid;
            string s_userid;
            dbQuery = "select id from room_machine where mach_num ='" + pNCMachine.machnum + "'";
            try
            {
                ds_userid = db.ReturnDataSet(dbQuery);
                s_userid = ds_userid.Tables[0].Rows[0]["ID"].ToString();
                pNCMachine.s_userid_now = s_userid;
                if (pNCMachine.s_userid_now == pNCMachine.s_userid_last)
                {
                    err = 1;
                }
                else
                {
                    err = 0;
                }
            }
            catch (Exception ex)
            {
                err = -1;
                Log.WriteLogErrMsg(dbQuery + "失败！");
            }

            return err;
        }
        /*
        *功能描述: 把记录的时间信息包括poweron_time,run_time,cut_time保存在表中,
        *           并把machine_log_today中的today_poweron_time,today_run_time,today_cut_time
        *           归置为0，接下来把pNCMahine->today_poweron_time,pNCMahine->today_run_time,
        *           pNCMahine->today_cut_time归置为0，最后把pNCMachine->s_userid_now赋给
        *           pNCMachine->s_userid_last
        *返回值: 成功返回0，不成功返回-1
        */
        public int InsertWorkTimeToDB(NCMachine pNCMachine)
        {
            int err = -1;
            string dbQuery, dbQuery_Insert, dbQuery_Update;
            string db_nowtime = ConstDefine.GetDateTime((int)ConstDefine.GetCurrentTimeUnix());
            dbQuery_Insert = "insert into worktime (mach_num,userid,poweron_time,run_time,cut_time,insert_time) values('" + pNCMachine.machnum + "','" +
            pNCMachine.s_userid_last + "'," + pNCMachine.today_poweron_time + "," + pNCMachine.today_run_time + "," + pNCMachine.today_cut_time + "," + db_nowtime + ");\n";
            dbQuery_Update = "update machine_log_today set today_poweron_time=0, today_run_time=0,today_cut_time=0 where mach_num='" + pNCMachine.machnum + "';\n";
            dbQuery = "begin\n" + dbQuery_Insert + dbQuery_Update + "end;\n";
            try
            {
                db.ExecuteSQL(dbQuery);
                pNCMachine.today_poweron_time = 0;
                pNCMachine.today_run_time = 0;
                pNCMachine.today_cut_time = 0;
                pNCMachine.s_userid_last = pNCMachine.s_userid_now;
                err = 0;
            }
            catch (Exception ex)
            {
                err = -1;
                Log.WriteLogErrMsg(dbQuery + "失败！");
            }
            return err;
        }
        #endregion

        #region 更新时间函数
        public void UpdatePatternTime(NCMachine pNCMachine)
        {
            if ((pNCMachine.delta_time > 0) && (pNCMachine.status == MachStatus.MS_ONLINE))
            {
                if (pNCMachine.delta_time > 1000)//刚启动
                {
                    ;
                }
                else
                {
                    pNCMachine.today_poweron_time += pNCMachine.delta_time;//有效工时开机时间统计
                    pNCMachine.day_poweron_time += pNCMachine.delta_time;//当天开机时间统计
                }

                if (1 == pNCMachine.runstatus || 0 == pNCMachine.runstatus)
                {
                    /**********************************有效工时*******************************/
                    pNCMachine.today_run_time += pNCMachine.delta_time;
                    if (pNCMachine.mach_info.spin_torq > 0.5 && pNCMachine.itype < 3000)
                    {
                        pNCMachine.today_cut_time += pNCMachine.delta_time;
                    }
                    if (pNCMachine.itype > 3000 && pNCMachine.mach_info.spin_torq > 1)
                    {
                        pNCMachine.today_cut_time += pNCMachine.delta_time;
                    }
                    /************************************************************************/
                    pNCMachine.today_run_time_meet_setting += pNCMachine.delta_time;
                    /**********************************当天时间统计*************************/
                    pNCMachine.day_run_time += pNCMachine.delta_time;
                    if (pNCMachine.mach_info.spin_torq > 1 && pNCMachine.itype < 3000)
                    {
                        pNCMachine.day_cut_time += pNCMachine.delta_time;
                    }

                }
            }
            if ((2 == pNCMachine.runstatus))
            {
                if (pNCMachine.flag_error_state == 1)
                {
                    pNCMachine.today_error_time += pNCMachine.delta_time;
                }
                else
                {
                    pNCMachine.today_idle_time += pNCMachine.delta_time;
                }
            }
            if (0 == pNCMachine.runstatus)
            {
                if (pNCMachine.flag_error_state == 1)
                {
                    pNCMachine.today_error_time += pNCMachine.delta_time;
                }
                else
                {
                    pNCMachine.today_idle_time += pNCMachine.delta_time;
                }
                pNCMachine.today_run_time_meet_setting += pNCMachine.delta_time;
            }
        }
        #endregion

        #region 甘特图
        /*
        *功能描述: 甘特图状态采集函数
        *参数:
        *
        *返回值:无
        */
        public void GetMachStatus_Gantt(NCMachine pNCMachine)
        {

            if (pNCMachine.mach_info.interp_state == 1)//运行
            {
                if (pNCMachine.i_machstatus == -1)
                {
                    pNCMachine.i_machstatus = 1;
                    pNCMachine.machstatus_starttime = pNCMachine.now_time;
                    pNCMachine.machstatus_endtime = 0;
                    pNCMachine.machstatus_flag = 0;
                }
                else
                {
                    if (pNCMachine.i_machstatus == 0)//如果上次为停止，插入历史表
                    {
                        pNCMachine.machstatus_endtime = pNCMachine.now_time;
                        pNCMachine.machstatus_flag = 1;
                        InsertMachStatus(pNCMachine);//插入数据库
                        pNCMachine.i_machstatus = 1;
                        pNCMachine.machstatus_starttime = pNCMachine.now_time;
                        pNCMachine.machstatus_endtime = 0;
                        pNCMachine.machstatus_flag = 0;
                    }
                    else
                    {
                        ;
                    }
                }
            }
            else if (pNCMachine.mach_info.interp_state != 1)//停止
            {
                if (pNCMachine.i_machstatus == -1)
                {
                    pNCMachine.i_machstatus = 0;
                    pNCMachine.machstatus_starttime = pNCMachine.now_time;
                    pNCMachine.machstatus_endtime = 0;
                    pNCMachine.machstatus_flag = 0;
                }
                else
                {
                    if (pNCMachine.i_machstatus == 1)//如果上次为运行，插入历史表
                    {
                        pNCMachine.machstatus_endtime = pNCMachine.now_time;
                        pNCMachine.machstatus_flag = 1;
                        InsertMachStatus(pNCMachine);//插入数据库
                        pNCMachine.i_machstatus = 0;
                        pNCMachine.machstatus_starttime = pNCMachine.now_time;
                        pNCMachine.machstatus_endtime = 0;
                        pNCMachine.machstatus_flag = 0;
                    }
                    else
                    {
                        ;
                    }
                }
            }
        }
        /*
        *功能描述: 状态数据插入数据库
        *参数:
        *
        *返回值:无
        */
        void InsertMachStatus(NCMachine pNCMachine)
        {
            string s_tablename = "machstatus_" + pNCMachine.machnum;
            string dbQuery;
            string s_starttime, s_endtime;
            s_starttime = ConstDefine.GetDateTime((int)pNCMachine.machstatus_starttime);
            s_endtime = ConstDefine.GetDateTime((int)pNCMachine.machstatus_endtime);
            dbQuery = "insert into " + s_tablename + "  (MACH_NUM,MACH_STATUS,START_TIME,END_TIME,FLAG) values('" + pNCMachine.machnum + "'," + pNCMachine.i_machstatus +
                "," + s_starttime + "," + s_endtime + "," + pNCMachine.machstatus_flag + ")";
            try
            {
                db.ExecuteSQL(dbQuery);
            }
            catch (Exception ex)
            {
                Log.WriteLogErrMsg("InsertMachStatus函数插入失败！");
            }
        }
        #endregion

        #region 高精故障日志
        /*
        *功能描述: 高精故障插入判断逻辑
        *参数:
        *    
        *    
        *返回值: 无.
        */
        public void GetGoldingErrorToLog(NCMachine pNCMachine)
        {
            if (pNCMachine.mach_info.error_handle != "" || pNCMachine.mach_info.error_motion != "" || pNCMachine.mach_info.error_plc != "")
            {
                //手动清除类
                if (pNCMachine.error_handle_last != pNCMachine.mach_info.error_handle)
                {
                    if (pNCMachine.error_handle_last == "")// 上次不存在故障，插入新故障，起始和结束时间相同
                    {
                        pNCMachine.ErrorStartTime_handle = (int)pNCMachine.now_time;
                        pNCMachine.ErrorEndTime_handle = (int)pNCMachine.now_time;
                        InsertGoldingErrToDB(pNCMachine, 0, 1);
                        pNCMachine.error_handle_last = pNCMachine.mach_info.error_handle;
                    }
                    else if (pNCMachine.error_handle_last != "") //上次存在故障
                    {
                        pNCMachine.ErrorEndTime_handle = (int)pNCMachine.now_time;
                        InsertGoldingErrToDB(pNCMachine, 1, 1);
                        //pNCMachine.ErrorStartTime_handle = (int)pNCMachine.now_time;
                        //InsertGoldingErrToDB(pNCMachine, 0,1);
                        pNCMachine.error_handle_last = pNCMachine.mach_info.error_handle;
                    }
                }
                //自动清除类
                else if (pNCMachine.error_motion_last != pNCMachine.mach_info.error_motion)
                {
                    if (pNCMachine.error_motion_last == "")// 上次不存在故障，插入新故障，起始和结束时间相同
                    {
                        pNCMachine.ErrorStartTime_motion = (int)pNCMachine.now_time;
                        pNCMachine.ErrorEndTime_motion = (int)pNCMachine.now_time;
                        InsertGoldingErrToDB(pNCMachine, 0, 2);
                        pNCMachine.error_motion_last = pNCMachine.mach_info.error_motion;
                    }
                    else if (pNCMachine.error_motion_last != "") //上次存在故障
                    {
                        pNCMachine.ErrorEndTime_motion = (int)pNCMachine.now_time;
                        InsertGoldingErrToDB(pNCMachine, 1, 2);
                        // pNCMachine.ErrorStartTime_motion = (int)pNCMachine.now_time;
                        //InsertGoldingErrToDB(pNCMachine, 0,2);
                        pNCMachine.error_motion_last = pNCMachine.mach_info.error_motion;
                    }
                }
                //PLC类故障
                else if (pNCMachine.error_plc_last != pNCMachine.mach_info.error_plc)
                {
                    if (pNCMachine.error_plc_last == "")// 上次不存在故障，插入新故障，起始和结束时间相同
                    {
                        pNCMachine.ErrorStartTime_plc = (int)pNCMachine.now_time;
                        pNCMachine.ErrorEndTime_plc = (int)pNCMachine.now_time;
                        InsertGoldingErrToDB(pNCMachine, 0, 3);
                        pNCMachine.error_plc_last = pNCMachine.mach_info.error_plc;
                    }
                    else if (pNCMachine.error_plc_last != "") //上次存在故障
                    {
                        pNCMachine.ErrorEndTime_plc = (int)pNCMachine.now_time;
                        InsertGoldingErrToDB(pNCMachine, 1, 3);
                        //pNCMachine.ErrorStartTime_plc = (int)pNCMachine.now_time;
                        //InsertGoldingErrToDB(pNCMachine, 0,3);
                        pNCMachine.error_plc_last = pNCMachine.mach_info.error_plc;
                    }
                }
            }
            else if (pNCMachine.error_handle_last != "")
            {
                pNCMachine.ErrorEndTime_handle = (int)pNCMachine.now_time;
                InsertGoldingErrToDB(pNCMachine, 1, 1);
                pNCMachine.error_handle_last = pNCMachine.mach_info.error_handle;
            }
            else if (pNCMachine.error_motion_last != "")
            {
                pNCMachine.ErrorEndTime_motion = (int)pNCMachine.now_time;
                InsertGoldingErrToDB(pNCMachine, 1, 2);
                pNCMachine.error_motion_last = pNCMachine.mach_info.error_motion;
            }
            else if (pNCMachine.error_plc_last != "")
            {
                pNCMachine.ErrorEndTime_plc = (int)pNCMachine.now_time;
                InsertGoldingErrToDB(pNCMachine, 1, 3);
                pNCMachine.error_plc_last = pNCMachine.mach_info.error_plc;
            }
        }

        /*
        *功能描述: 高精故障插入故障表
        *参数:
        * endflag：    0:无变化 1：故障结束 2：原故障结束新故障开始    
        * errortype 1:handle 2:motion 3:plc   
        *返回值: 0 is insert successfully.
        */
        int InsertGoldingErrToDB(NCMachine pNCMachine, int endflag, int errortype)
        {
            int err = -1;
            string s_table;
            string dbQuery = "";
            string s_starttime, s_endtime;
            switch (errortype)
            {
                case 1:
                    s_starttime = ConstDefine.GetDateTime((int)pNCMachine.ErrorStartTime_handle);
                    s_endtime = ConstDefine.GetDateTime((int)pNCMachine.ErrorEndTime_handle);
                    break;
                case 2:
                    s_starttime = ConstDefine.GetDateTime((int)pNCMachine.ErrorStartTime_motion);
                    s_endtime = ConstDefine.GetDateTime((int)pNCMachine.ErrorEndTime_motion);
                    break;
                case 3:
                    s_starttime = ConstDefine.GetDateTime((int)pNCMachine.ErrorStartTime_plc);
                    s_endtime = ConstDefine.GetDateTime((int)pNCMachine.ErrorEndTime_plc);
                    break;
                default:
                    s_starttime = "";
                    s_endtime = "";
                    break;
            }

            s_table = "error_" + pNCMachine.machnum;
            if (endflag == 0)
            {
                switch (errortype)
                {
                    case 1:
                        dbQuery = "insert into " + s_table + " (errorid,type,starttime,endtime,ERRORINFO) values(" + errortype + "," + errortype + "," + s_starttime + "," + s_endtime + ",'" + pNCMachine.mach_info.error_handle + "')";
                        break;
                    case 2:
                        dbQuery = "insert into " + s_table + " (errorid,type,starttime,endtime,ERRORINFO) values(" + errortype + "," + errortype + "," + s_starttime + "," + s_endtime + ",'" + pNCMachine.mach_info.error_motion + "')";
                        break;
                    case 3:
                        dbQuery = "insert into " + s_table + " (errorid,type,starttime,endtime,ERRORINFO) values(" + errortype + "," + errortype + "," + s_starttime + "," + s_endtime + ",'" + pNCMachine.mach_info.error_plc + "')";
                        break;
                    default: break;
                }

            }
            else if (endflag == 1)
            {
                switch (errortype)
                {
                    case 1:
                        dbQuery = "update " + s_table + " set endtime= " + s_endtime + " where errorinfo = '" + pNCMachine.error_handle_last + "' and starttime= " + s_starttime;
                        break;
                    case 2:
                        dbQuery = "update " + s_table + " set endtime= " + s_endtime + " where errorinfo = '" + pNCMachine.error_motion_last + "' and starttime= " + s_starttime;
                        break;
                    case 3:
                        dbQuery = "update " + s_table + " set endtime= " + s_endtime + " where errorinfo = '" + pNCMachine.error_plc_last + "' and starttime= " + s_starttime;
                        break;
                    default: break;
                }

            }
            else if (endflag == 2)
            {
                string dbQuery_insert = "";
                string dbQuery_update = "";
                switch (errortype)
                {
                    case 1:
                        dbQuery_insert = "insert into " + s_table + " (errorid,type,starttime,endtime,ERRORINFO) values(" + errortype + "," + errortype + s_starttime + "," + s_endtime + ",'" + pNCMachine.mach_info.error_handle + "')";
                        dbQuery_update = "update " + s_table + " set endtime= " + s_endtime + " where errorinfo = " + pNCMachine.error_handle_last + " and starttime= " + s_starttime;
                        break;
                    case 2:
                        dbQuery_insert = "insert into " + s_table + " (errorid,type,starttime,endtime,ERRORINFO) values(" + errortype + "," + errortype + "," + s_starttime + "," + s_endtime + ",'" + pNCMachine.mach_info.error_motion + "')";
                        dbQuery_update = "update " + s_table + " set endtime= " + s_endtime + " where errorinfo = " + pNCMachine.error_motion_last + " and starttime= " + s_starttime;
                        break;
                    case 3:
                        dbQuery_insert = "insert into " + s_table + " (errorid,type,starttime,endtime,ERRORINFO) values(" + errortype + "," + errortype + "," + s_starttime + "," + s_endtime + ",'" + pNCMachine.mach_info.error_plc + "')";
                        dbQuery_update = "update " + s_table + " set endtime= " + s_endtime + " where errorinfo = " + pNCMachine.error_plc_last + " and starttime= " + s_starttime;
                        break;
                    default: break;
                }
                dbQuery += "begin\n";
                dbQuery += dbQuery_insert;
                dbQuery += dbQuery_update;
                dbQuery += "end;\n";
            }
            try
            {
                db.ExecuteSQL(dbQuery);
                err = 0;
            }
            catch (Exception ex)
            {
                Log.WriteLogErrMsg("InsertGoldingErrToDB函数执行故障");
            }
            return err;
        }
        #endregion

        #region 力士乐故障日志
        /*
        *功能描述: 力士乐故障插入判断逻辑
        *参数:
        *    
        *    
        *返回值: 无.
        */
        public void GetRexrothErrorToLog(NCMachine pNCMachine)
        {
            if (pNCMachine.mach_info.error_id > 0)
            {
                if (pNCMachine.error_last != pNCMachine.mach_info.error_id)
                {
                    if (pNCMachine.error_last == -1)// 上次不存在故障，插入新故障，起始和结束时间相同
                    {
                        pNCMachine.ErrorStartTime = (int)pNCMachine.now_time;
                        pNCMachine.ErrorEndTime = (int)pNCMachine.now_time;
                        InsertRexrothErrToDB(pNCMachine, 0);
                        pNCMachine.error_last = pNCMachine.mach_info.error_id;
                    }
                    else if (pNCMachine.error_last >= 0) //上次存在故障
                    {
                        pNCMachine.ErrorEndTime = (int)pNCMachine.now_time;
                        InsertRexrothErrToDB(pNCMachine, 1);
                        pNCMachine.ErrorStartTime = (int)pNCMachine.now_time;
                        InsertRexrothErrToDB(pNCMachine, 0);
                        pNCMachine.error_last = pNCMachine.mach_info.error_id;
                    }
                }
            }
            else if (pNCMachine.error_last > 0)
            {
                pNCMachine.ErrorEndTime = (int)pNCMachine.now_time;
                InsertRexrothErrToDB(pNCMachine, 1);
                pNCMachine.error_last = pNCMachine.mach_info.error_id;
            }
        }

        /*
        *功能描述: 力士乐故障插入故障表
        *参数:
        * endflag：    0:无变化 1：故障结束 2：原故障结束新故障开始    
        *    
        *返回值: 0 is insert successfully.
        */
        int InsertRexrothErrToDB(NCMachine pNCMachine, int endflag)
        {
            int err = -1;
            string s_table;
            string dbQuery = "";
            string s_starttime, s_endtime;
            s_starttime = ConstDefine.GetDateTime((int)pNCMachine.ErrorStartTime);
            s_endtime = ConstDefine.GetDateTime((int)pNCMachine.ErrorEndTime);
            s_table = "error_" + pNCMachine.machnum;
            if (endflag == 0)
            {
                dbQuery = "insert into " + s_table + " (errorid,type,starttime,endtime) values(" + pNCMachine.mach_info.error_id + ",102," + s_starttime + "," + s_endtime + ")";
            }
            else if (endflag == 1)
            {
                dbQuery = "update " + s_table + " set endtime= " + s_endtime + " where errorid = " + pNCMachine.error_last + " and starttime= " + s_starttime;
            }
            else if (endflag == 2)
            {
                string dbQuery_insert = "";
                string dbQuery_update = "";
                dbQuery_insert = "insert into " + s_table + " (errorid,type,starttime,endtime) values(" + pNCMachine.mach_info.error_id + ",102," + s_starttime + "," + s_endtime + ")";
                dbQuery_update = "update " + s_table + " set endtime= " + s_endtime + " where errorid = " + pNCMachine.error_last + " and starttime= " + s_starttime;
                dbQuery += "begin\n";
                dbQuery += dbQuery_insert;
                dbQuery += dbQuery_update;
                dbQuery += "end;\n";
            }
            try
            {
                db.ExecuteSQL(dbQuery);
                err = 0;
            }
            catch (Exception ex)
            {
                Log.WriteLogErrMsg("InsertRexrothErrToDB函数执行故障");
            }
            return err;
        }
        #endregion

        #region 秦川故障日志
        /*
        *功能描述: 秦川故障插入判断逻辑
        *参数:
        *    
        *    
        *返回值: 无.
        */
        public void GetQcmttErrorToLog(NCMachine pNCMachine)
        {
            if (pNCMachine.mach_info.plc_warningNumber > 0)
            {
                if (pNCMachine.error_last != pNCMachine.mach_info.plc_warningNumber)
                {
                    if (pNCMachine.error_last == -1)// 上次不存在故障，插入新故障，起始和结束时间相同
                    {
                        pNCMachine.ErrorStartTime = (int)pNCMachine.now_time;
                        pNCMachine.ErrorEndTime = (int)pNCMachine.now_time;
                        InsertQcmttErrToDB(pNCMachine, 0);
                        pNCMachine.error_last = pNCMachine.mach_info.plc_warningNumber;
                    }
                    else if (pNCMachine.error_last >= 0) //上次存在故障
                    {
                        pNCMachine.ErrorEndTime = (int)pNCMachine.now_time;
                        InsertQcmttErrToDB(pNCMachine, 1);
                        pNCMachine.ErrorStartTime = (int)pNCMachine.now_time;
                        InsertQcmttErrToDB(pNCMachine, 0);
                        pNCMachine.error_last = pNCMachine.mach_info.plc_warningNumber;
                    }
                }
            }
            else if (pNCMachine.error_last > 0)
            {
                pNCMachine.ErrorEndTime = (int)pNCMachine.now_time;
                InsertQcmttErrToDB(pNCMachine, 1);
                pNCMachine.error_last = pNCMachine.mach_info.plc_warningNumber;
            }
        }

        /*
        *功能描述: 秦川故障插入故障表
        *参数:
        * endflag：    0:无变化 1：故障结束 2：原故障结束新故障开始    
        *    
        *返回值: 0 is insert successfully.
        */
        int InsertQcmttErrToDB(NCMachine pNCMachine, int endflag)
        {
            int err = -1;
            string s_table;
            string dbQuery = "";
            string s_starttime, s_endtime;
            s_starttime = ConstDefine.GetDateTime((int)pNCMachine.ErrorStartTime);
            s_endtime = ConstDefine.GetDateTime((int)pNCMachine.ErrorEndTime);
            s_table = "error_" + pNCMachine.machnum;
            if (endflag == 0)
            {
                dbQuery = "insert into " + s_table + " (errorid,type,starttime,endtime) values(" + pNCMachine.mach_info.plc_warningNumber + ",202," + s_starttime + "," + s_endtime + ")";
            }
            else if (endflag == 1)
            {
                dbQuery = "update " + s_table + " set endtime= " + s_endtime + " where errorid = " + pNCMachine.error_last + " and starttime= " + s_starttime;
            }
            else if (endflag == 2)
            {
                string dbQuery_insert = "";
                string dbQuery_update = "";
                dbQuery_insert = "insert into " + s_table + " (errorid,type,starttime,endtime) values(" + pNCMachine.mach_info.plc_warningNumber + ",202," + s_starttime + "," + s_endtime + ")";
                dbQuery_update = "update " + s_table + " set endtime= " + s_endtime + " where errorid = " + pNCMachine.error_last + " and starttime= " + s_starttime;
                dbQuery += "begin\n";
                dbQuery += dbQuery_insert;
                dbQuery += dbQuery_update;
                dbQuery += "end;\n";
            }
            try
            {
                db.ExecuteSQL(dbQuery);
                err = 0;
            }
            catch (Exception ex)
            {
                Log.WriteLogErrMsg("InsertQcmttErrToDB函数执行故障");
            }
            return err;
        }
        #endregion

        #region 华中8型故障日志
        /*
        *功能描述: 华中8型故障插入判断逻辑
        *参数:pNCMachine 机床数据结构类
        *    
        *    
        *返回值: 无.
        */
        public void GetHNC8ErrorToLog(NCMachine pNCMachine)
        {
            int ret_hnc = -1;
            pNCMachine.HncnowFaultList.Clear();
            
            if (pNCMachine.Hnc_AlmList.Count != 0)
            {
                if (pNCMachine.HncnowFaultList.Count() != 0)
                {
                    pNCMachine.HncnowFaultList.Clear();
                }
                AddHncFaultList(pNCMachine);//把当前故障插入

                //开始比较
                ret_hnc = HncCompareError(pNCMachine);//0有故障需要处理1无故障处理
                pNCMachine.HnclastFaultList.Clear();
                int i_uendArrayNum = pNCMachine.HncuendFaultList.Count();
                int i_newArrayNum = pNCMachine.HncnewFaultList.Count();
                for (int k = 0; k < i_uendArrayNum; k++)
                {
                    HncFaultVector tempVector = new HncFaultVector(pNCMachine.HncuendFaultList[k].alm_no, pNCMachine.HncuendFaultList[k].alm_msg, pNCMachine.HncuendFaultList[k].start_time, pNCMachine.HncuendFaultList[k].end_time, 1);
                    pNCMachine.HnclastFaultList.Add(tempVector);
                }
                for (int m = 0; m < i_newArrayNum; m++)
                {
                    HncFaultVector tempVector = new HncFaultVector(pNCMachine.HncnewFaultList[m].alm_no, pNCMachine.HncnewFaultList[m].alm_msg, pNCMachine.HncnewFaultList[m].start_time, pNCMachine.HncnewFaultList[m].end_time, 1);
                    pNCMachine.HnclastFaultList.Add(tempVector);
                }
                if (ret_hnc == 0)
                {
                    //如果有新故障，则插入新故障 action=0 否则什么也不做
                    if (pNCMachine.HncnewFaultList.Count() == 0)
                    {
                        ;
                    }
                    else
                    {
                        InsertHncError_1(pNCMachine, 0);
                    }
                    //如果有结束故障，则插入结束故障 action=1 否则什么也不做
                    if (pNCMachine.HncendFaultList.Count() == 0)
                    {
                        ;
                    }
                    else
                    {
                        InsertHncError_1(pNCMachine, 1);
                    }
                }
                else
                {
                    ;
                }
            }
        }
        /*
         *功能描述: 把HNC当前的故障插入列表
         *参数:
            *pNCMachine:机床采集类 
            * almNo:故障编号
            * almsg：故障信息
            * 
         *返回值:无
        */
        public void AddHncFaultList(NCMachine pNCMachine)
        {
            long l_nowtime = ConstDefine.GetCurrentTimeUnix(); //可优化
            int alrmCount = pNCMachine.Hnc_AlmList.Count;
            if (alrmCount != 0)
            {
                for (int i = 0; i < alrmCount; i++)
                {
                    HncFaultVector tempVector = new HncFaultVector(Convert.ToInt32(pNCMachine.Hnc_AlmList[i][0]), pNCMachine.Hnc_AlmList[i][1], l_nowtime, l_nowtime, 1);
                    pNCMachine.HncnowFaultList.Add(tempVector);
                }
            }
        }

        /*
      *功能描述: fanuc故障比较逻辑函数
      *参数:
      *
      *返回值:0有故障开始或结束需要进行数据库操作1无故障开始和结束
      */
        public int HncCompareError(NCMachine eNCMachine)
        {
            int now_size = eNCMachine.HncnowFaultList.Count();
            int old_size = eNCMachine.HnclastFaultList.Count();
            int j;
            int err = -1;
            eNCMachine.HncnewFaultList.Clear();
            eNCMachine.HncendFaultList.Clear();
            eNCMachine.HncuendFaultList.Clear();
            int flag_nowlast_same = -1;
            for (int i = 0; i < now_size; i++)
            {
                for (j = 0; j < old_size; j++)
                {
                    //现在的错误在上一次错误中能查到，把其压入未完成的错误vector
                    if (eNCMachine.HncnowFaultList[i].alm_no == eNCMachine.HnclastFaultList[j].alm_no)
                    {
                        HncFaultVector vector_temp = new HncFaultVector(eNCMachine.HncnowFaultList[i].alm_no, eNCMachine.HncnowFaultList[i].alm_msg, eNCMachine.HncnowFaultList[j].start_time, eNCMachine.HncnowFaultList[j].end_time, 1);
                        eNCMachine.HncuendFaultList.Add(vector_temp);
                        flag_nowlast_same = 0;
                        break;
                    }
                }
                if (flag_nowlast_same == 0)
                {
                    flag_nowlast_same = -1;
                }
                //现在错误在上一次的错误中不能查到，把其压入新的错误vector
                else if (j == old_size && j != 0 && flag_nowlast_same == -1)
                {
                    if (eNCMachine.HncnowFaultList[i].alm_no != 0)
                    {
                        HncFaultVector vector_temp = new HncFaultVector(eNCMachine.HncnowFaultList[i].alm_no, eNCMachine.HncnowFaultList[i].alm_msg, eNCMachine.HncnowFaultList[i].start_time, eNCMachine.HncnowFaultList[i].end_time, 1);
                        eNCMachine.HncnewFaultList.Add(vector_temp);
                    }

                }
                if (old_size == 0)
                {
                    if (eNCMachine.HncnowFaultList[i].alm_no != 0)
                    {
                        try
                        {
                            HncFaultVector vector_temp = new HncFaultVector(eNCMachine.HncnowFaultList[i].alm_no, eNCMachine.HncnowFaultList[i].alm_msg, eNCMachine.HncnowFaultList[i].start_time, eNCMachine.HncnowFaultList[i].end_time, 1);
                            eNCMachine.HncnewFaultList.Add(vector_temp); // 新的错误开始	
                        }
                        catch (Exception ex)
                        { ;}
                    }
                }

            }
            //last vector 与uend vector进行比较 把已经完成的错误压入end vector
            int uend_size = eNCMachine.HncuendFaultList.Count();
            int flag_same = -1;
            int k = 0;
            if (uend_size >= 0)
            {
                for (int m = 0; m < old_size; m++)
                {
                    for (k = 0; k < uend_size; k++)
                    {
                        if (eNCMachine.HnclastFaultList[m].alm_no == eNCMachine.HncuendFaultList[k].alm_no)
                        {
                            flag_same = 0;
                            break;
                        }
                    }
                    if (flag_same == 0)
                    {
                        flag_same = -1;
                    }
                    else if (flag_same == -1 && k == uend_size)
                    {
                        HncFaultVector vector_temp = new HncFaultVector(eNCMachine.HnclastFaultList[m].alm_no, eNCMachine.HnclastFaultList[m].alm_msg, eNCMachine.HnclastFaultList[m].start_time, eNCMachine.now_time, 0);
                        eNCMachine.HncendFaultList.Add(vector_temp);
                    }
                }
            }

            int new_size = eNCMachine.HncnewFaultList.Count();
            int end_size = eNCMachine.HncendFaultList.Count();
            if (end_size == 0 && new_size == 0)
            {
                err = 1;
            }
            else
            {
                err = 0;
            }

            return err;
        }

        /*
       *功能描述: HNC故障插入
       *参数:
       *
         * pNCMachine:机床采集类
         * flag：插入或更新标志0插入1更新
       *返回值:
       */
        public int InsertHncError_1(NCMachine pNCMachine, int flag)
        {
            int err = -1;
            string dbQuery = "", temp_dbQuery;
            string s_table, s_starttime, s_endtime;
            s_table = "error_" + pNCMachine.machnum;
            int i_oldArrayNum = -1;
            int i_newArrayNum = -1;
            int i_lastArrayNum = -1;
            i_newArrayNum = pNCMachine.HncnewFaultList.Count();
            i_oldArrayNum = pNCMachine.HncendFaultList.Count();
            i_lastArrayNum = pNCMachine.HnclastFaultList.Count();
            if (flag == 0)
            {
                dbQuery += "begin\n";
                for (int i = 0; i < i_newArrayNum; i++)
                {
                    s_starttime = ConstDefine.GetDateTime((int)pNCMachine.HncnewFaultList[i].start_time);
                    s_endtime = ConstDefine.GetDateTime((int)pNCMachine.HncnewFaultList[i].end_time);
                    temp_dbQuery = "insert into " + s_table + " (errorid,starttime,endtime,errorinfo) values(" +
                             pNCMachine.HncnewFaultList[i].alm_no + "," + s_starttime + "," + s_endtime + ",'" + pNCMachine.HncnewFaultList[i].alm_msg + "');\n";
                    dbQuery += temp_dbQuery;
                }
                dbQuery += "end;\n";
                if (i_lastArrayNum > 0)
                {
                    try
                    {
                        db.ExecuteSQL(dbQuery);
                        err = 0;
                    }
                    catch (Exception ex)
                    {
                        err = -1;
                        Log.WriteLogErrMsg("InsertHncError_1插入失败");
                    }
                }
            }
            else if (flag == 1)//上一次有错误，错误全消失了的情况下，插入上一次的错误结束标识符,当机床离线的时候则把m_lastFanucFaultVector赋值给m_endFanucFaultVector
            {
                dbQuery += "begin\n";
                for (int j = 0; j < i_oldArrayNum; j++)
                {
                    s_starttime = ConstDefine.GetDateTime((int)pNCMachine.HncendFaultList[j].start_time);
                    s_endtime = ConstDefine.GetDateTime((int)pNCMachine.now_time);
                    temp_dbQuery = "update " + s_table + " set endtime = " + s_endtime + " where errorid= " + pNCMachine.HncendFaultList[j].alm_no + " and starttime = " + s_starttime + ";\n";
                    dbQuery += temp_dbQuery;
                }
            }
            dbQuery += "end;\n";
            if (i_oldArrayNum > 0)
            {
                try
                {
                    db.ExecuteSQL(dbQuery);
                    err = 0;
                }
                catch (Exception ex)
                {
                    err = -1;
                    Log.WriteLogErrMsg("InsertHncError_1更新失败");
                }
            }
            return err;
        }
        #endregion

        #region 传递主对话框到子类
        public void SetMainDlg(Main dlg)
        {
            m_pMainDlg = dlg;
        }
        #endregion

        #region 程序日志
        public void InsertProgLog(NCMachine pNCMachine, int flag)
        {
            string sDate = ConstDefine.GetCurrentTimeString();
            string s_tablename = "proglog_" + pNCMachine.machnum;
            string dbQuery = "";
            string s_starttime, s_endtime;
            s_starttime = ConstDefine.GetDateTime((int)pNCMachine.proglog_starttime);
            s_endtime = ConstDefine.GetDateTime((int)pNCMachine.proglog_endtime);
            if (flag == 0)//程序开始
            {
                dbQuery = "insert into " + s_tablename + "  (MACH_NUM,PROGNAME,START_TIME,END_TIME,FLAG) values('" + pNCMachine.machnum + "','" + pNCMachine.mach_info.prog_name +
                "'," + s_starttime + "," + s_starttime + "," + flag + ")";
            }
            else if (flag == 1)
            {
                dbQuery = "update " + s_tablename + " set end_time=" + s_endtime + ", flag=1 where progname='" + pNCMachine.prognameLast + "' and start_time=" + s_starttime;
                //复位
                pNCMachine.proglog_starttime = -1;
                pNCMachine.proglog_endtime = -1;
                pNCMachine.currentlineLast = -1;
                pNCMachine.progflag = -1;//
            }

            try
            {
                db.ExecuteSQL(dbQuery);
            }
            catch (Exception ex)
            {
                Log.WriteLogErrMsg("InsertProgLog函数插入失败！");
            }
        }
        #endregion
    }

   
   
}
