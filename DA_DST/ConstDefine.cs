using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace CONSTDEFINE
{
    
    struct machine_info_t
    {
        //public double host_value1, host_value2, host_value3, host_value4, host_value5, host_value6;
        //public double ahost_value1, ahost_value2, ahost_value3, ahost_value4, ahost_value5, ahost_value6;
        //public double dist_togo1, dist_togo2, dist_togo3, dist_togo4, dist_togo5, dist_togo6;
        //public double feed_speed, afeed_speed, dfeed_speed;
        //public double spindle_speed, aspindle_speed, dspindle_speed;
        //public double spin_torq;
        //public int current_line, work_piece, tool_num, axis_num;
        //public double tool_radius, tool_length;
        //public int disp_mode, task_state, interp_state;
        //public long poweron_time, run_time, cut_time;
        //public double total_poweron_time, total_run_time, total_cut_time;
        //public double today_poweron_time, today_run_time, today_cut_time;
        //public int error_id, error_axisnum;
        //public string prog_name;
        //public string softversion;
        //public double mainFileTotalLine, subFileTotalLine;
        //public double error_flag;
        //public double sublevel;
        //public string mainfile_name;
        //public string subfile_name;
        //public string error_handle, error_motion, error_plc;

        public double host_value1, host_value2, host_value3, host_value4, host_value5, host_value6, host_value7, host_value8, host_value9;//X\Y\Z\A\B\C编程值
        public double ahost_value1, ahost_value2, ahost_value3, ahost_value4, ahost_value5, ahost_value6, ahost_value7, ahost_value8, ahost_value9;//X\Y\Z\A\B\C实际值
        public double dist_togo1, dist_togo2, dist_togo3, dist_togo4, dist_togo5, dist_togo6, dist_togo7, dist_togo8, dist_togo9;//X\Y\Z\A\B\C剩余值
        public double act_axis_velocity1, act_axis_velocity2, act_axis_velocity3, act_axis_velocity4, act_axis_velocity5, act_axis_velocity6, act_axis_velocity7, act_axis_velocity8, act_axis_velocity9;//X\Y\Z\A\B\C轴速度
        public double feed_speed, afeed_speed, dfeed_speed;//进给编程值、进给实际值、进给倍率
        public int ready;
        public double spindle_speed, aspindle_speed, dspindle_speed;//主轴编程值、主轴实际值、主轴倍率
        public double spin_torq;//主轴扭矩
        public int current_line, work_piece, tool_num, axis_num;//当前行、工件数量、刀具号、配置轴数量
        public double tool_radius, tool_length;//刀半径、刀长
        public int disp_mode, task_state, interp_state;//显示模式、任务状态、机床状态
        public long poweron_time, run_time, cut_time;
        public double total_poweron_time, total_run_time, total_cut_time;
        public double today_poweron_time, today_run_time, today_cut_time;
        public int error_id, error_axisnum; // error_id 给力士乐使用
        public string prog_name;//当前程序名
        public string softversion;//软件版本
        public double mainFileTotalLine, subFileTotalLine;//主程序行号和子程序行号
        public double error_flag;//故障标志位
        public double sublevel;//程序层次数
        public string mainfile_name;//主程序
        public string subfile_name;//子程序
        public string error_handle, error_motion, error_plc;//手动清除类故障、自动清除类故障、PLC类故障,给高精使用
        public int plc_warningNumber; // 故障变量,给秦川使用
        public int tool_id;//刀具id
        public int tool_usenum;//刀具使用次数
        public int tool_life;//刀具寿命
        //public int[] tool_all; // 所有刀具
        public double wheel_ride; // 砂轮修整量
        public int wheel_remaintime; // 砂轮预计寿命

        public string equip_ip;//机床ip
        public string Machine_type;   //系统类型
        public string Machine_toolProbe; //机床是否有测头
        public string avail_axis;        //可用轴
        public string alarmText;     //报警文本
        public string programmed_over;//程序执行完毕
    }

    public struct testStruct
    {
        public double powerontime;
        public double cycletime;
        public ushort acprog;
        public ushort acstate;
        public double actime;
        public double acparts;
        public ushort jsqstatus;
        public double actspeed;
        public double cmdspeed;
        public double driveload;
        public ushort chanstatus;
        public double feedrateipoovr;
        public double cmdfeedrateipo;
        public double actfeedrateipo;
        public byte totalbyte;
        public byte bytenum;

        //[MarshalAs(UnmanagedType.BStr, SizeConst = 32)]
        //public byte[] program;
        public byte program1;
        public byte program2;
        public byte program3;
        public byte program4;
        public byte program5;
        public byte program6;
        public byte program7;
        public byte program8;
        public byte program9;
        public byte program10;
        public byte program11;
        public byte program12;
        public byte program13;
        public byte program14;
        public byte program15;
        public byte program16;
        public byte program17;
        public byte program18;
        public byte program19;
        public byte program20;
        public byte program21;
        public byte program22;
        public byte program23;
        public byte program24;
        public byte program25;
        public byte program26;
        public byte program27;
        public byte program28;
        public byte program29;
        public byte program30;
        public byte program31;
        public byte program32;

        public int actlinenumber;
        public int textindex;
        public ushort numgeoaxes;
        public double actprogpos_x;
        public double actprogpos_y;
        public double actprogpos_z;
        public double prodisttogo_x;
        public double prodisttogo_y;
        public double prodisttogo_z;
        public double speedovr;
    }
    enum MachStatus
    {
        MS_INIT = -1, MS_OFFLINE = 0, MS_ONLINE = 1, MS_STOP = 2
    };
    enum ConnectStatus
    {
        MACH_CONNECTED = 0, MACH_CONNECTING = 1
    };
    enum MachineType
    {
        LY08 = 52,
        GOLDING = 10,
        GOLDING_NEW = 11,
        GOLDING_680 = 12,
        GOLDING_320A = 13,
        GOLDING_320B = 14,
        FANUC_MD = 1001,
        FANUC_MD_MATE = 1002,
        SIEMENS840DSL_SW47 = 2002,
        SIEMENS840D = 2001,
        S7 = 2004,
        MOXA1242 = 3001,
        REXROTH = 6001,
        QCMTT = 6002,
        QCMTT_CHAMFERING = 6004, // 倒角机
        QCMTT_HOBBING = 6005, // 滚齿机
        QCMTT_MOLDING = 6006, // 成型磨
        QCMTT_CYLINDRICAL = 6007, // 外圆磨
        QCMTT_PLANE = 6008, // 平磨
        HNC8 = 6003
    };
    enum IsAcquire
    {
        Yes = 1,
        No = 0
    }
    enum Subscri
    {
        SUCCESS = 0, FAILED = -1
    };
    class ConstDefine
    {
        //ConstDefine FANUCMD 1001;
        #region 检查网络是否畅通
        public static bool PingIpOrDomainName(string strIpOrDName)
        {
            try
            {
                Ping objPingSender = new Ping();
                PingOptions objPinOptions = new PingOptions();
                objPinOptions.DontFragment = true;
                string data = "";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int intTimeout = 2000;
                PingReply objPinReply = objPingSender.Send(strIpOrDName, intTimeout, buffer, objPinOptions);
                string strInfo = objPinReply.Status.ToString();
                if (strInfo == "Success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region 获取时间戳函数
        public static long GetCurrentTimeUnix()
        {
            TimeSpan cha = (System.DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
            long t = (long)cha.TotalSeconds;
            return t;
        }
        #endregion

        #region 获取时间字符串函数
        public static string GetCurrentTimeString()
        {
            /*************获取当前时间******************/
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            String nowDate;
            nowDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", currentTime);
            /******************************************/
            return nowDate;
        }
        #endregion

        #region 获取时间数据库格式字符串
        public static string GetDateTime(long timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = ((long)timeStamp * 10000000);
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime targetDt = dtStart.Add(toNow);
            string s_time = string.Format("{0:yyyy-MM-dd HH:mm:ss}", targetDt);
            s_time = "to_date('" + s_time + "','yyyy-MM-dd HH24:mi:ss')";
            return s_time;
        }
        #endregion
    }
    public class ContentStruct
    {
        public string monitor;
        public string value;
    }

    public class TransferStruct
    {
        public string equipmentId;
        public string equipmentCode;
        public string sendTime;
        public string messageType;
        public List<ContentStruct> messageContent;
    }

    public class MoniterStatus
    {
        Dictionary<string, List<ContentStruct>> resultDict = new Dictionary<string, List<ContentStruct>>();
        Dictionary<string, List<ContentStruct>> resultDict2 = new Dictionary<string, List<ContentStruct>>();
        #region 创建http发送post类型json
        /// <summary>
        /// 发送http post请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="parameters">查询参数集合</param>
        /// <returns></returns>
        public HttpWebResponse CreatePostHttpResponse(string url, string parameters, Encoding encode, string contentType = "application/json;charset=utf8")
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);//创建请求对象
            request.Method = "POST";//请求方式
            request.ContentType = contentType;//链接类型

            //构造查询字符串
            if (!(parameters == null || parameters.Length == 0))
            {
                byte[] data = encode.GetBytes(parameters);
                try
                {
                    Stream stream = request.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                }
                catch (Exception e)
                {
                    e.ToString();
                    //throw;
                }

                //写入请求流
            }
            return request.GetResponse() as HttpWebResponse;
        }

        // 以下方法为机器控制提供web服务，输出机床状态
        public void WebInterface(Dictionary<string, List<ContentStruct>> resultDict, string machine_num)
        {

            //url不同计算机需要更换IP
            // string url = "http://127.0.0.1:8889/api/webapi/Get_Data";
            //string url = "http://127.0.0.1:8877/api/webapi/Get_Data";
            //string url = "http://192.168.2.127:8877/api/webapi/Get_Data";
            string url = "http://192.168.2.68:8877/api/webapi/Get_Data";
            string str="";

            resultDict2 = resultDict;
            // 若修改项，需改该索引
            resultDict2[machine_num.ToString()].RemoveAt(10);

            foreach (var item in resultDict2.ToArray())
            {
                if(item.Key == machine_num)
                {
                    string equId = "";
                    if (machine_num.Equals("5271001")) {
                        equId = "8a58c9e670e762120170e808c5320220";
                    }
                    else if (machine_num.Equals("5271002")) {
                        equId = "8a58c9e670e762120170e808c5320221";
                    }
                    else if (machine_num.Equals("5271003"))
                    {
                        equId = "8a58c9e670e762120170e808c5320231";
                    }
                    else if (machine_num.Equals("5271004"))
                    {
                        equId = "8a58c9e670e762120170e808c5320228";
                    }
                    else if (machine_num.Equals("5271005"))
                    {
                        equId = "8a58c9e670e762120170e808c5320229";
                    }
                    else if (machine_num.Equals("5271006"))
                    {
                        equId = "8a58c9e670e762120170e808c5320225";
                    }
                    else if (machine_num.Equals("5271007"))
                    {
                        equId = "8a58c9e670e762120170e808c5320222";
                    }
                    else if (machine_num.Equals("5271008"))
                    {
                        equId = "8a58c9e670e762120170e808c5320224"; // 外圆磨
                    }
                    else if (machine_num.Equals("5271009"))
                    {
                        equId = "8a58c9e670e762120170e808c5320223"; // 内圆磨2
                    }
                    else if (machine_num.Equals("5271010"))
                    {
                        equId = "8a58c9e670e762120170e808c5320226";
                    }
                    else if (machine_num.Equals("5271011"))
                    {
                        equId = "8a58c9e670e762120170e808c5320227";
                    }
                    else if (machine_num.Equals("5272001"))
                    {
                        equId = "5272001";
                    }
                    else if (machine_num.Equals("5272002"))
                    {
                        equId = "5272002";
                    }
                    else if (machine_num.Equals("5272003"))
                    {
                        equId = "5272003";
                    }
                    else if (machine_num.Equals("5272004"))
                    {
                        equId = "5272004";
                    }
                    else if (machine_num.Equals("5272005"))
                    {
                        equId = "5272005";
                    }
                    else {
                        equId = item.Key.ToString();
                    }
                    
                    //foreach (var item1 in resultDict[machine_num.ToString()])
                    //{
                    //    item1.
                    //    if (item1.monitor.Equals("M0503011") && item1.value.Equals("1"))
                    //    {
                    //        flag = 1;
                    //    }
                    //}
                    var messageContent = item.Value as List<ContentStruct>;

                    var result = JsonConvert.SerializeObject(new
                    {
                        equipmentId = equId,
                        equipmentCode = "",
                        sendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        messageType = "5",
                        messageContent = JsonConvert.SerializeObject(messageContent.ToArray())
                    });
                    str = result;
                    break;
                }
               
                //str += result;
            }
    
            // 发送post数据
            CreatePostHttpResponse(url, str, Encoding.UTF8);
        }
        #endregion
    }
}
