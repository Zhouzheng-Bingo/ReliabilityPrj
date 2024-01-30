using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using CONSTDEFINE;

namespace DA_DST
{
    /*
     * 秦川APServer系统
     * **/
    class HttpUtils : NCMachine
    {
        //public HttpListener Monit = null;//;= new HttpListener();
        //String body = "";

        static HttpListener sSocket = null;
        static string body = "";

        //machine_info_t mach_info = new machine_info_t();
        public HttpUtils(string _machnum, string _ip, int _port, int _itype)
            : base(_machnum, _ip, _port, _itype)
        {

        }

        public override void Monitor(string _ip, int _port) 
        {
            try
            {
                sSocket = new HttpListener();
                sSocket.Prefixes.Add(string.Concat("http://", _ip, ":", _port, "/"));
                sSocket.Start();
                sSocket.BeginGetContext(new AsyncCallback(GetContextCallBack), sSocket);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        static void GetContextCallBack(IAsyncResult ar)
        {
            try
            {
                sSocket = ar.AsyncState as HttpListener;
                HttpListenerContext context = sSocket.EndGetContext(ar);
                sSocket.BeginGetContext(new AsyncCallback(GetContextCallBack), sSocket);
                Stream stream = context.Request.InputStream;
                System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.UTF8);
                body = reader.ReadToEnd();
                //Console.WriteLine(body);
                //其它处理code
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        public override int ConnectToNC(string ip, string username, string password)
        {
            int err = -1;
            bool ret = ConstDefine.PingIpOrDomainName(this.ip);
            if (ret == true)
            {
                try
                {
                    //modify by zhang
                    if(sSocket.IsListening == true)
                    {
                        status = MachStatus.MS_ONLINE;
                        err = 0;
                    }
                    else
                    {
                        status = MachStatus.MS_OFFLINE;
                        err = -1;                    
                    }
                    //if (body.Equals(""))
                    //{
                    //    status = MachStatus.MS_OFFLINE;
                    //    err = -1;
                    //}
                    //else
                    //{
                    //    status = MachStatus.MS_ONLINE;
                    //    err = 0;
                    //}
                }
                catch (Exception e)
                {
                    e.ToString();
                    Console.WriteLine("连接这里有问题");
                    return -1;
                }
                return err;
            }
            else
            {
                status = MachStatus.MS_OFFLINE;
                Log.WriteLogErrMsg("秦川连接函数问题，109行");
                return -1;
            }
        }

        public override int UpdateData()    //HttpListener Monit)
        {
            int ret = -1;
            if (0 != UpdateComm())
            {
                ret = -1;
            }

            if (status == MachStatus.MS_ONLINE)
            {
                try
                {
                    if (ConstDefine.PingIpOrDomainName(this.ip) == true) 
                    {
                        string data = "";
                        data = HttpUtility.ParseQueryString(body).Get("data");
                        Console.WriteLine("收到POST数据:" + HttpUtility.UrlDecode(body));
                        sendData = 1;
                        if (!string.IsNullOrEmpty(data))
                        {
                            if (data.Equals(""))
                            {

                            }
                            else
                            {
                                JObject jObject;
                                try
                                {
                                    //解析json字符串
                                    jObject = JObject.Parse(data);
                                    string json_equip_ip = (string)jObject["equip_ip"];
                                    if (json_equip_ip.Equals("192.168.2.25"))
                                    {
                                        Console.WriteLine("here!!!");
                                    }
                                    if (json_equip_ip.Equals(ip))
                                    {                                        
                                        #region 赋值给结构体
                                        mach_info.equip_ip = (string)jObject["equip_ip"];//设备IP

                                        if (mach_info.equip_ip.Equals("192.168.2.21")) {
                                            Console.WriteLine("hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh");
                                        }

                                        mach_info.Machine_type = (string)jObject["nc_type"];//系统类型
                                        mach_info.softversion = (string)jObject["cnc_software"];//版本号
                                        mach_info.Machine_toolProbe = (string)jObject["machine_toolProbe"]; //机床是否有测头
                                        switch ((string)jObject["nc_status"]) //机床运行状态
                                        {
                                            case "M02": mach_info.interp_state = 0; break; // MO2
                                            case "run": mach_info.interp_state = 1; break;    //正在运行
                                            case "stop": mach_info.interp_state = 2; break;   //急停暂停
                                            default: mach_info.interp_state = 3; break;
                                        }
                                        if ((string)jObject["run_mode"] == "auto")//机床运行模式
                                            mach_info.disp_mode = 0;
                                        else if ((string)jObject["run_mode"] == "single") mach_info.disp_mode = 1;
                                        else if ((string)jObject["run_mode"] == "mdi") mach_info.disp_mode = 2;
                                        else if ((string)jObject["run_mode"] == "dryrun") mach_info.disp_mode = 3;
                                        else if ((string)jObject["run_mode"] == "search") mach_info.disp_mode = 4;
                                        else if ((string)jObject["run_mode"] == "test") mach_info.disp_mode = 5;
                                        else if ((string)jObject["run_mode"] == "manual") mach_info.disp_mode = 6;
                                        else if ((string)jObject["run_mode"] == "home") mach_info.disp_mode = 7;
                                        else if ((string)jObject["run_mode"] == "null") mach_info.disp_mode = 8;
                                        else mach_info.disp_mode = 9;

                                        mach_info.aspindle_speed = Convert.ToDouble((string)jObject["spl_speed"]);//主轴实际速率spI到底是L还是1 还是I或l
                                        mach_info.dspindle_speed = Convert.ToDouble((string)jObject["spl_override"]); //主轴倍率
                                        mach_info.spindle_speed = Convert.ToDouble((string)jObject["spl_programmed"]);//主轴编程速率

                                        mach_info.avail_axis = (string)jObject["avail_axis"];//配置轴
                                        mach_info.axis_num = count(mach_info.avail_axis); //配置的轴数目
                                        mach_info.host_value1 = (Convert.ToDouble((string)jObject["axis_position"][0])) / 10000;  //X轴编程坐标值
                                        mach_info.host_value2 = (Convert.ToDouble((string)jObject["axis_position"][1])) / 10000;  //Y轴编程坐标值
                                        mach_info.host_value3 = (Convert.ToDouble((string)jObject["axis_position"][2])) / 10000;  //Z轴编程坐标值
                                        mach_info.host_value4 = (Convert.ToDouble((string)jObject["axis_position"][3])) / 10000;  //Z轴编程坐标值
                                        mach_info.host_value5 = (Convert.ToDouble((string)jObject["axis_position"][4])) / 10000;  //Z轴编程坐标值
                                        mach_info.host_value6 = (Convert.ToDouble((string)jObject["axis_position"][5])) / 10000;  //Z轴编程坐标值
                                        mach_info.host_value7 = (Convert.ToDouble((string)jObject["axis_position"][6])) / 10000;  //A轴编程坐标值
                                        mach_info.host_value8 = (Convert.ToDouble((string)jObject["axis_position"][7])) / 10000;  //C轴编程坐标值
                                        mach_info.host_value9 = (Convert.ToDouble((string)jObject["axis_position"][8])) / 10000;  //Z轴编程坐标值

                                        mach_info.dist_togo1 = (Convert.ToDouble((string)jObject["axis_delta"][0])) / 1000; //X轴剩余值
                                        mach_info.dist_togo2 = (Convert.ToDouble((string)jObject["axis_delta"][1])) / 1000; //Y轴剩余值
                                        mach_info.dist_togo3 = (Convert.ToDouble((string)jObject["axis_delta"][2])) / 1000; //Z轴剩余值
                                        mach_info.dist_togo4 = (Convert.ToDouble((string)jObject["axis_delta"][3])) / 1000; //A轴剩余值
                                        mach_info.dist_togo5 = (Convert.ToDouble((string)jObject["axis_delta"][4])) / 1000; //C轴剩余值
                                        mach_info.dist_togo6 = (Convert.ToDouble((string)jObject["axis_delta"][5])) / 1000; //X轴剩余值
                                        mach_info.dist_togo7 = (Convert.ToDouble((string)jObject["axis_delta"][6])) / 1000; //Y轴剩余值
                                        mach_info.dist_togo8 = (Convert.ToDouble((string)jObject["axis_delta"][7])) / 1000; //Z轴剩余值
                                        mach_info.dist_togo9 = (Convert.ToDouble((string)jObject["axis_delta"][8])) / 1000; //A轴剩余值

                                        mach_info.prog_name = (string)jObject["run_programmed"].ToString(); //执行程序名

                                        string run_line = (string)jObject["run_line"];
                                        run_line = run_line.Substring(1);
                                        mach_info.current_line = Convert.ToInt32(run_line);//当前执行程序行号

                                        //if (mach_info.equip_ip.Equals("192.168.2.5"))
                                        //{
                                        //    string run_line = (string)jObject["run_line"];
                                        //    run_line = run_line.Substring(1);                                            
                                        //    mach_info.current_line = Convert.ToInt32(run_line);//当前执行程序行号
                                            
                                        //    mach_info.work_piece = 455;//工件数量
                                        //}                                        
                                        //if (mach_info.equip_ip.Equals("192.168.2.13"))
                                        //{
                                        //    string run_line = (string)jObject["run_line"];
                                        //    string[] run_line_array = run_line.Split('N');
                                        //    run_line = run_line_array[1];
                                        //    mach_info.current_line = Convert.ToInt32(run_line);//当前执行程序行号

                                        //    mach_info.work_piece = 638;//工件数量
                                        //}
                                        //if (mach_info.equip_ip.Equals("192.168.2.21"))
                                        //{
                                        //    string run_line = (string)jObject["run_line"];
                                        //    string[] run_line_array = run_line.Split('`');
                                        //    run_line = run_line_array[1];
                                        //    mach_info.current_line = Convert.ToInt32(run_line);//当前执行程序行号

                                        //    mach_info.work_piece = 579;//工件数量
                                        //}
                                        //if (mach_info.equip_ip.Equals("192.168.2.25"))
                                        //{
                                        //    string run_line = (string)jObject["run_line"];
                                        //    string[] run_line_array = run_line.Split('N');
                                        //    run_line = run_line_array[1];
                                        //    mach_info.current_line = Convert.ToInt32(run_line);//当前执行程序行号

                                        //    mach_info.work_piece = 593;//工件数量
                                        //}
                                        //if (mach_info.equip_ip.Equals("192.168.2.32"))
                                        //{
                                        //    string run_line = (string)jObject["run_line"];
                                        //    string[] run_line_array = run_line.Split('s');
                                        //    run_line = run_line_array[1];
                                        //    mach_info.current_line = Convert.ToInt32(run_line);//当前执行程序行号

                                        //    mach_info.work_piece = 476;//工件数量
                                        //}

                                        mach_info.dfeed_speed = Convert.ToDouble((string)jObject["feed_override"]); //进给倍率
                                        mach_info.feed_speed = Convert.ToDouble((string)jObject["feed_programmed"]);//进给编程值
                                        mach_info.afeed_speed = Convert.ToDouble((string)jObject["feed_speed"]);    //进给实际值
                                        mach_info.ready = Convert.ToInt32((string)jObject["ready"]);    //ready信号
                                        mach_info.plc_warningNumber = Convert.ToInt32((string)jObject["plc_warningNumber"]);     //plc报警代码
                                        if (mach_info.plc_warningNumber.ToString().Equals("") || mach_info.plc_warningNumber.ToString().Equals("0"))
                                        {
                                            mach_info.error_flag = 0;
                                        }
                                        else
                                        {
                                            mach_info.error_flag = 1;
                                        }
                                        mach_info.programmed_over = (string)jObject["programmed_over"]; //程序执行完毕信号
                                        if (mach_info.programmed_over.Equals("True"))
                                        {
                                            mach_info.programmed_over = "1";
                                        }
                                        else if (mach_info.programmed_over.Equals("False"))
                                        {
                                            mach_info.programmed_over = "0";
                                        }
                                        else if (mach_info.programmed_over.Equals("1"))
                                        {
                                            mach_info.programmed_over = "1";
                                        }
                                        else if (mach_info.programmed_over.Equals("0"))
                                        {
                                            mach_info.programmed_over = "0";
                                        }
                                        else
                                        {
                                            //Console.WriteLine("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa:" + mach_info.programmed_over);
                                            mach_info.programmed_over = "error";
                                        }

                                        //if (mach_info.nc_status != "") //m02:运行结束 run:正在执行 stop:暂停 MS_INIT = -1,MS_OFFLINE = 0, MS_ONLINE = 1, MS_STOP = 2
                                        //    return 0;
                                        //return -1;
                                        //switch ((string)jObject["nc_status"])
                                        //{
                                        //    case "M02":
                                        //    case "run": status = MachStatus.MS_ONLINE; return 0;
                                        //    case "stop": status = MachStatus.MS_OFFLINE; return 2;
                                        //    default: break;
                                        //}
                                        ret = 0;
                                        #endregion
                                    }
                                    else
                                    { ret = 0; }
                                }
                                catch (Exception e)
                                {
                                    
                                    e.ToString();
                                }
                                
                                
                            }
                        }
                        ret = 0;
                    }
                    else
                    {
                        //status = MachStatus.MS_OFFLINE;
                        ret = -1;
                    }
                    
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Log.WriteLogErrMsg("APServer连接函数问题，216行");
                    ret = -1;
                }
            }
            //else if (status == MachStatus.MS_OFFLINE)
            //{
            //    ret = -1;
            //}
            //if (ret == -1) {
            //    Console.WriteLine("stop!!!!");
            //}
            return ret;
        }

        public int count(string s)
        {
            int num = s.Length;
            int temp = 0;
            char[] strArry = s.ToArray();
            for (int i = 0; i < num; i++)
            {
                if (strArry[i] == ',')
                    temp++;
            }
            return (num - temp);
        }
    }
}
