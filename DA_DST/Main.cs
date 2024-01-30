using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ConnectDB;
using Oracle.DataAccess.Client;
using CONSTDEFINE;
using System.Diagnostics;
using System.Threading;

namespace DA_DST
{
    public partial class Main : Form
    {
        //重载Listvew函数解决闪烁问题
        public class DoubleBufferListView : ListView
        {
            public DoubleBufferListView()
            {
                SetStyle(ControlStyles.DoubleBuffer |
                 ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.AllPaintingInWmPaint, true);
                UpdateStyles();
            }
        }
        //定义双缓冲列表
        DoubleBufferListView MachListView = new DoubleBufferListView();
        //实例化数据库
        OracleDB db = new OracleDB();
        //设备数量
        int m_iNumMachine;
        Dictionary<string, string> dic_MachArea = new Dictionary<string, string>();
        List<string> list_area = new List<string>();

        DataAcquire dataacquire = new DataAcquire();
        OracleDataReader odr;
        int VerifyFlag = -1;

        /// <summary>
        /// http协议发送内容
        /// </summary>
        // 创建一个把数据发送到http的类的实例对象
        MoniterStatus ms = new MoniterStatus();
        // 创建一个传入MoniterStatus类的WebIInterface方法的字典
        Dictionary<string, List<ContentStruct>> resultDict = new Dictionary<string, List<ContentStruct>>();


        // 创建上一次采集数据的备份
        Dictionary<string, List<ContentStruct>> resultDictLast = new Dictionary<string, List<ContentStruct>>();
        List<ContentStruct> lst_last = new List<ContentStruct>();
        //计时标识
        int timer_count = 0;
        int machine_NOself = 0;
        int isSendFlag = 0;

        #region 自适应
        AutoSizeFormClass asc = new AutoSizeFormClass();
        private void Main_Load(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);
        }
        private void Main_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }
        #endregion

        #region 主函数
        public Main()
        {
            CheckForIllegalCrossThreadCalls = false;
            m_iNumMachine = 0;
            InitialList();

            InitialHttpStruct();

            InitializeComponent();
            InitialTransport();
            InitialTimer();
            InitialTreeView();
        }
        #endregion

        public void InitialTransport()
        {
            OracleDataReader odr;
            string sql = "select mach_num,ip,iport,itype from room_machine where acquiredata_status = 1 order by mach_num asc";
            OracleDB dbTransport = new OracleDB();
            try
            {
                dbTransport.ConnectToDB();

                odr = dbTransport.ReturnDataReader(sql);
                // db.CloseConn(); // 20210602新增关闭数据库
                while (odr.Read())
                {
                    string s_machnum = odr["mach_num"].ToString();
                    string s_ip = odr["ip"].ToString();
                    string s_port = odr["iport"].ToString();
                    string s_type = odr["itype"].ToString();
                    int i_type = Convert.ToInt16(s_type);
                    //if (this.IsHandleCreated)
                    //{
                    //防止在窗口句柄初始化之前就走到下面的代码 
                    IntPtr i = this.Handle;
                    Invoke(new EventHandler(delegate { InsertMachineToList(s_machnum, s_ip, i_type); }));// 异步执行主界面的内容修改    
                    //}
                    //InsertMachineToList(s_machnum, s_ip, i_type);
                }
            }
            catch (Exception e)
            {
                e.ToString();
                //throw;
            }
            finally
            {
                dbTransport.CloseConn();
            }
            /*
             以上20210602日改，需验证是否能解决数据库关闭问题
             * 
             */
        }

        public void InitialTimer()
        {
            dataacquire.SetMainDlg(this);
            dataacquire.SetDBConnArg(db.db_user, db.db_passwd, db.db_source);
            dataacquire.StartDataAquire();

            string date = GetDateTime();
            //textBox_sysinfo.AppendText(date + ":" + "开启采集……\r\n");
            //记录日志
            Log.WriteLogErrMsg(date + ":" + "开启采集……\r\n");
            //开启定时器
            timer_update.Interval = 100; // 0.1*20=2
            //timer_update.Interval = 250; // 0.25*20=5
            timer_update.Enabled = true;
            timer_update.Start();
        }

        #region 获取字符串时间函数
        private string GetDateTime()
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            String nowDate;

            //date = String.Format("%{}4d-%2d-%2d %2d:%2d:%2d", currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, currentTime.Second);
            nowDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", currentTime);
            return nowDate;
        }
        #endregion

        #region 写入数据库参数
        public void DataBaseSetting(string s_user, string s_passwd, string s_source)
        {
            db.db_passwd = s_passwd;
            db.db_source = s_source;
            db.db_user = s_user;
        }
        #endregion

        #region 列表初始化函数
        public void InitialList()
        {
            //给重绘列表赋属性
            MachListView.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            MachListView.FullRowSelect = true;
            MachListView.HideSelection = false;
            MachListView.Location = new System.Drawing.Point(298, 129);
            MachListView.Name = "MachListView";
            MachListView.Size = new System.Drawing.Size(1115, 355);
            MachListView.TabIndex = 2;
            MachListView.UseCompatibleStateImageBehavior = false;
            MachListView.View = System.Windows.Forms.View.Details;
            MachListView.GridLines = true;

            //将其添加到form窗体里面
            this.Controls.Add(this.MachListView);
            MachListView.Columns.Add("机床编号", 120, HorizontalAlignment.Left); //一步添加 
            MachListView.Columns.Add("机床IP", 150, HorizontalAlignment.Center);
            MachListView.Columns.Add("机床状态", 80, HorizontalAlignment.Center);
            MachListView.Columns.Add("操作模式", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("主轴速度", 80, HorizontalAlignment.Center);
            MachListView.Columns.Add("主轴倍率", 80, HorizontalAlignment.Center);
            MachListView.Columns.Add("进给轴速度", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("进给轴倍率", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("当前程序", 290, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴1编程值", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴2编程值", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴3编程值", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴4编程值", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴5编程值", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴6编程值", 100, HorizontalAlignment.Center);

            MachListView.Columns.Add("轴1速度", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴2速度", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴3速度", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴4速度", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴5速度", 100, HorizontalAlignment.Center);
            MachListView.Columns.Add("轴6速度", 100, HorizontalAlignment.Center);
        }
        public void ClearLsit()
        {
            MachListView.Items.Clear();
        }
        public void InsertMachineToList(string name, string ip, int type)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = name;
            lvi.SubItems.Add(ip);
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");

            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");

            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            MachListView.Items.Add(lvi);
        }
        #endregion

        #region 树形图初始化
        public void InitialTreeView()
        {
            treeView_Machine.Nodes.Clear();
            //查询工区信息-->查询工区内的设备-->建立哈希表
            CreateAreaMach();
            TreeNode node1 = new TreeNode();
            node1.Text = "设备树";
            node1.ImageIndex = 0;
            node1.SelectedImageIndex = 0;
            try
            {
                foreach (var item_2 in list_area)//添加二级节点
                {
                    TreeNode node2 = new TreeNode();
                    node2.Text = item_2;
                    node2.ImageIndex = 1;
                    node2.SelectedImageIndex = 1;
                    foreach (var item_3 in dic_MachArea)//添加三级节点
                    {
                        if (dic_MachArea[item_3.Key] == item_2)
                        {
                            TreeNode node3 = new TreeNode();
                            node3.Text = item_3.Key;
                            node3.ImageIndex = 2;
                            node3.SelectedImageIndex = 2;
                            node2.Nodes.Add(node3);
                        }
                    }
                    node1.Nodes.Add(node2);
                }
                treeView_Machine.Nodes.Add(node1);
                //定位根节点
                treeView_Machine.SelectedNode = treeView_Machine.Nodes[0];
                //展开组件中的所有节点
                treeView_Machine.SelectedNode.ExpandAll();
            }
            catch (Exception e)
            {
                MessageBox.Show("区域设备哈希表故障！");
            }
        }

        //查询工区信息-->查询工区内的设备-->建立哈希表
        public int CreateAreaMach()
        {
            int err = -1;
            string sql = "";
            list_area.Clear();
            dic_MachArea.Clear();
            err = db.ConnectToDB();
            if (err == 0)
            {
                sql = "select distinct(room_id) from room_machine order by room_id asc";
                DataSet ds_area = new DataSet();
                DataSet ds_mach = new DataSet();
                ds_area = db.ReturnDataSet(sql);
                foreach (DataRow item in ds_area.Tables[0].Rows)//遍历一表多行一列
                {
                    sql = "select mach_num from room_machine where room_id = '" + item["ROOM_ID"].ToString() + "' order by mach_num asc";
                    ds_mach = db.ReturnDataSet(sql);
                    list_area.Add(item["ROOM_ID"].ToString());
                    if (ds_mach.Tables[0].Rows.Count == 0)
                    {
                        ;
                    }
                    else
                    {
                        foreach (DataRow mDr in ds_mach.Tables[0].Rows)//遍历一表多行多列
                        {
                            dic_MachArea.Add(mDr["MACH_NUM"].ToString(), item["ROOM_ID"].ToString());
                        }
                    }
                }
                db.CloseConn();
            }
            else
            {
                MessageBox.Show("数据库连接错误！");
            }
            return err;
        }
        #endregion

        #region http数据结构初始化
        void InitialHttpStruct()
        {
            List<ContentStruct> lst = new List<ContentStruct>();
            resultDict.Clear();
            lst.Clear();
            lst.Add(new ContentStruct { monitor = "M0503002", value = "" }); // DISP_MODE
            lst.Add(new ContentStruct { monitor = "M0503001", value = "" }); // INTERP_STATE
            lst.Add(new ContentStruct { monitor = "M0503003", value = "" }); // DISP_MODE
            lst.Add(new ContentStruct { monitor = "M0503004", value = "" }); // DISP_MODE
            lst.Add(new ContentStruct { monitor = "M0503005", value = "" }); // DISP_MODE
            lst.Add(new ContentStruct { monitor = "M0503006", value = "" }); // DISP_MODE
            lst.Add(new ContentStruct { monitor = "M0503007", value = "" }); // DISP_MODE
            lst.Add(new ContentStruct { monitor = "M0503008", value = "" }); // DISP_MODE
            //加入程序完成信号
            lst.Add(new ContentStruct { monitor = "M0503009", value = "" }); // PROGRAM_OVER
            lst.Add(new ContentStruct { monitor = "M0503010", value = "" }); // run_time
            lst.Add(new ContentStruct { monitor = "M0503011", value = "" }); // run_time
            resultDict.Add("5271004", lst);
            resultDict.Add("5271008", lst);
            resultDict.Add("5271007", lst);
            resultDict.Add("5271002", lst);
            resultDict.Add("5271006", lst);
            resultDict.Add("5271003", lst);
            resultDict.Add("5271010", lst);
            resultDict.Add("5271011", lst);
            resultDict.Add("5271005", lst);
            resultDict.Add("5271009", lst);
            resultDict.Add("5271001", lst);
        }
        #endregion

        private void timer_update_Tick(object sender, EventArgs e)
        {
            int err = -1;
            NCMachine sNCMachine = new NCMachine();
            NCMachine S = new NCMachine();
            Dictionary<string, int> dic_MachOnline = new Dictionary<string, int>();
            while (0 == (err = dataacquire.GetFromMachineDataQueue(out sNCMachine)))
            {
                S = sNCMachine;
                List<ContentStruct> lst = new List<ContentStruct>();
                MachListView.BeginUpdate();
                UpdateListView(sNCMachine);
                if (sNCMachine.itype == (int)MachineType.QCMTT)//解决秦川数据跳变
                {
                    //if (sNCMachine.sendData == 1) {
                    if (sNCMachine.delta_time > 0 && sNCMachine.status == MachStatus.MS_ONLINE)
                    {
                        if (sNCMachine.mach_info.disp_mode != -1 && sNCMachine.mach_info.interp_state != -1)
                        {
                            lst.Clear();
                            lst.Add(new ContentStruct { monitor = "M0503002", value = sNCMachine.mach_info.disp_mode.ToString() }); // DISP_MODE
                            lst.Add(new ContentStruct { monitor = "M0503001", value = sNCMachine.mach_info.interp_state.ToString() }); // INTERP_STATE
                            lst.Add(new ContentStruct { monitor = "M0503003", value = sNCMachine.mach_info.ready.ToString() }); // DISP_MODE
                            lst.Add(new ContentStruct { monitor = "M0503004", value = sNCMachine.mach_info.tool_id.ToString() }); // DISP_MODE
                            lst.Add(new ContentStruct { monitor = "M0503005", value = sNCMachine.mach_info.tool_num.ToString() }); // DISP_MODE
                            lst.Add(new ContentStruct { monitor = "M0503006", value = sNCMachine.mach_info.tool_radius.ToString() }); // DISP_MODE
                            //lst.Add(new ContentStruct { monitor = "M0503007", value = sNCMachine.mach_info.run_time.ToString() }); // DISP_MODE
                            lst.Add(new ContentStruct { monitor = "M0503007", value = "7" }); // DISP_MODE
                            lst.Add(new ContentStruct { monitor = "M0503008", value = sNCMachine.mach_info.tool_usenum.ToString() }); // DISP_MODE
                            //加入程序完成信号
                            lst.Add(new ContentStruct { monitor = "M0503009", value = sNCMachine.mach_info.programmed_over.ToString() }); // PROGRAM_OVER
                            //lst.Add(new ContentStruct { monitor = "M0503010", value = sNCMachine.mach_info.run_time.ToString() }); // run_time
                            lst.Add(new ContentStruct { monitor = "M0503010", value = "10" }); // run_time
                            lst.Add(new ContentStruct { monitor = "M0503011", value = sNCMachine.sendData.ToString() }); // 做判断是否发送
                            if (resultDict.ContainsKey(sNCMachine.machnum))
                            {
                                resultDict[sNCMachine.machnum.ToString()] = lst;
                                //lst.Clear();
                            }
                            else
                            {
                                resultDict.Add(sNCMachine.machnum.ToString(), lst);
                                //lst.Clear();
                            }
                        }
                    }
                    //}                             
                }
                else
                {
                    //if (sNCMachine.sendData == 1) {

                    lst.Clear();
                    lst.Add(new ContentStruct { monitor = "M0503002", value = sNCMachine.mach_info.disp_mode.ToString() }); // DISP_MODE
                    lst.Add(new ContentStruct { monitor = "M0503001", value = sNCMachine.mach_info.interp_state.ToString() }); // INTERP_STATE
                    lst.Add(new ContentStruct { monitor = "M0503003", value = sNCMachine.mach_info.ready.ToString() }); // DISP_MODE
                    lst.Add(new ContentStruct { monitor = "M0503004", value = sNCMachine.mach_info.tool_id.ToString() }); // DISP_MODE
                    lst.Add(new ContentStruct { monitor = "M0503005", value = sNCMachine.mach_info.tool_num.ToString() }); // DISP_MODE
                    lst.Add(new ContentStruct { monitor = "M0503006", value = sNCMachine.mach_info.tool_radius.ToString() }); // DISP_MODE
                    //lst.Add(new ContentStruct { monitor = "M0503007", value = sNCMachine.mach_info.run_time.ToString() }); // DISP_MODE
                    lst.Add(new ContentStruct { monitor = "M0503007", value = "7" }); // DISP_MODE
                    lst.Add(new ContentStruct { monitor = "M0503008", value = sNCMachine.mach_info.tool_usenum.ToString() }); // DISP_MODE
                    //加入程序完成信号
                    lst.Add(new ContentStruct { monitor = "M0503009", value = sNCMachine.mach_info.programmed_over.ToString() }); // PROGRAM_OVER
                    //lst.Add(new ContentStruct { monitor = "M0503010", value = sNCMachine.mach_info.run_time.ToString() }); // run_time
                    lst.Add(new ContentStruct { monitor = "M0503010", value = "10" }); // run_time
                    lst.Add(new ContentStruct { monitor = "M0503011", value = sNCMachine.sendData.ToString() }); // 做判断是否发送
                    if (resultDict.ContainsKey(sNCMachine.machnum))
                    {
                        resultDict[sNCMachine.machnum.ToString()] = lst;
                        //lst.Clear();
                    }
                    else
                    {
                        resultDict.Add(sNCMachine.machnum.ToString(), lst);
                        //lst.Clear();
                    }
                    //}
                }

                MachListView.EndUpdate();
            }
            OracleDataReader odr;
            string strSQL = "SELECT MACH_NUM,MACHINE_ONLINE FROM MACHINE_LOG_TODAY";
            OracleDB dbOnline = new OracleDB();
            try
            {
                err = dbOnline.ConnectToDB();
                if (err == 0)
                {
                    dic_MachOnline.Clear();
                    odr = dbOnline.ReturnDataReader(strSQL);
                    while (odr.Read())
                    {
                        string s_machnum = odr["MACH_NUM"].ToString();
                        string s_MachineOnline = odr["MACHINE_ONLINE"].ToString();
                        //修改
                        if (s_machnum.Equals("5272001") || s_machnum.Equals("5272002") || s_machnum.Equals("5272003") || s_machnum.Equals("5272004") || s_machnum.Equals("5272005"))
                        {

                        }
                        else
                        {
                            int iMachineOnline = Convert.ToInt32(s_MachineOnline);
                            dic_MachOnline.Add(s_machnum, iMachineOnline);
                        }
                    }
                    // db.CloseConn(); // 20210602新增关闭数据库
                }
                else
                {
                    textBox_sysinfo.AppendText("数据库连接失败！");
                    // db.CloseConn(); // 20210528新增关闭数据库
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                //throw;
            }
            finally
            {
                dbOnline.CloseConn();
            }
            /*
             以上20210602日改，需验证是否能解决数据库关闭问题
             * 
             */
            int online_num = 0;

            foreach (var item in dic_MachOnline)
            {
                if (dic_MachOnline[item.Key] == 1)
                {
                    online_num++;
                }
            }
            // 1.若机床全部关闭，online_num=0，left_count=10,timer_count++后能到10
            // 2.若机床全部开启，online_num=11,
            if (online_num != 0)
            {
                // 若机床全部离线,下一轮，在线才执行下面代码
                int left_count = 20 - online_num;
                if (timer_count == left_count)//如果5秒计时到，执行http的post操作,timer_count复位
                {
                    foreach (var item in dic_MachOnline)
                    {
                        if (dic_MachOnline[item.Key] == 1)
                        {
                            SendHttpData(item.Key);
                            //Thread.Sleep(150);
                        }
                    }
                    timer_count = 0;
                }
                else
                {
                    timer_count++;
                    //if (timer_count == 12) 
                    //{
                    //    timer_count = 0;
                    //}
                    if (timer_count > 20)
                    {
                        timer_count = 0;
                    }

                }
                //resultDict.Add(pNCMachine.machnum.ToString(), lst);
                //ms.WebInterface(resultDict, sNCMachine.machnum.ToString());
            }

        }
        public void SendHttpData(string mach_num)
        {
            int flag = 0;
            try
            {
                if (ConstDefine.PingIpOrDomainName("192.168.2.68"))
                //if (ConstDefine.PingIpOrDomainName("127.0.0.1"))
                //if (ConstDefine.PingIpOrDomainName("192.168.2.127"))
                {
                    foreach (var item in resultDict[mach_num.ToString()])
                    {
                        if (item.monitor.Equals("M0503011") && item.value.Equals("1"))
                        {
                            flag = 1;
                        }
                    }
                    if (flag == 1)
                    {
                        ms.WebInterface(resultDict, mach_num);
                    }
                }
                else
                {
                    //textBox_sysinfo.AppendText("127.0.0.1:8877连接失败！");
                    textBox_sysinfo.AppendText("192.168.2.68:8877连接失败！");
                }
            }
            catch (Exception ex)
            {
                textBox_sysinfo.AppendText(ex.ToString());
            }
        }

        #region 更新列表数据函数群
        private void UpdateListView(NCMachine pNCMachine)
        {
            int item = GetMachineItem(pNCMachine);
            if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_ONLINE)
            {
                //if (this.MachListView.SelectedItems.Count > 0)
                //{
                if (pNCMachine.flag_error_state == 1)
                {
                    MachListView.Items[item].UseItemStyleForSubItems = false;
                    MachListView.Items[item].SubItems[2].Text = "故障";
                    MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Red;
                }
                else
                {
                    switch (pNCMachine.mach_info.interp_state)
                    {
                        case 1:
                            MachListView.Items[item].UseItemStyleForSubItems = false;
                            MachListView.Items[item].SubItems[2].Text = "运行";
                            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Green;
                            break;
                        case 2:
                            MachListView.Items[item].SubItems[2].Text = "等待";
                            MachListView.Items[item].UseItemStyleForSubItems = false;
                            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
                            break;
                        case 3:
                            MachListView.Items[item].SubItems[2].Text = "停止";
                            MachListView.Items[item].UseItemStyleForSubItems = false;
                            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
                            break;
                        case 4:
                            MachListView.Items[item].SubItems[2].Text = "结束";
                            MachListView.Items[item].UseItemStyleForSubItems = false;
                            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
                            break;
                        case 5:
                            MachListView.Items[item].SubItems[2].Text = "中断";
                            MachListView.Items[item].UseItemStyleForSubItems = false;
                            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
                            break;
                        case 6:
                            MachListView.Items[item].SubItems[2].Text = "复位";
                            MachListView.Items[item].UseItemStyleForSubItems = false;
                            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
                            break;
                        case 7:
                            MachListView.Items[item].SubItems[2].Text = "start";
                            MachListView.Items[item].UseItemStyleForSubItems = false;
                            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
                            break;
                        default:
                            MachListView.Items[item].SubItems[2].Text = "unknown";
                            MachListView.Items[item].UseItemStyleForSubItems = false;
                            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
                            break;
                    }
                }
                switch (pNCMachine.mach_info.disp_mode)//运行模式 //1auto2mdi3jog4teachin5repos6ref7edit8handle9remoto  11unknown
                {
                    case 1:
                        MachListView.Items[item].SubItems[3].Text = "自动";
                        MachListView.Items[item].UseItemStyleForSubItems = false;
                        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Blue;
                        break;
                    case 2:
                        MachListView.Items[item].SubItems[3].Text = "MDI";
                        MachListView.Items[item].UseItemStyleForSubItems = false;
                        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Green;
                        break;
                    case 3:
                        MachListView.Items[item].SubItems[3].Text = "JOG";
                        MachListView.Items[item].UseItemStyleForSubItems = false;
                        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkBlue;
                        break;
                    case 4:
                        MachListView.Items[item].SubItems[3].Text = "TEACHIN";
                        MachListView.Items[item].UseItemStyleForSubItems = false;
                        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkGreen;
                        break;
                    case 5:
                        MachListView.Items[item].SubItems[3].Text = "REPOS";
                        MachListView.Items[item].UseItemStyleForSubItems = false;
                        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOliveGreen;
                        break;
                    case 6:
                        MachListView.Items[item].SubItems[3].Text = "REF";
                        MachListView.Items[item].UseItemStyleForSubItems = false;
                        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOrange;
                        break;
                    case 7:
                        MachListView.Items[item].SubItems[3].Text = "EDIT";
                        MachListView.Items[item].UseItemStyleForSubItems = false;
                        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOrange;
                        break;
                    case 8:
                        MachListView.Items[item].SubItems[3].Text = "手轮";
                        MachListView.Items[item].UseItemStyleForSubItems = false;
                        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOrange;
                        break;
                    case 9:
                        MachListView.Items[item].SubItems[3].Text = "远程";
                        MachListView.Items[item].UseItemStyleForSubItems = false;
                        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOrange;
                        break;
                    default:
                        MachListView.Items[item].SubItems[3].Text = "未知模式";
                        MachListView.Items[item].UseItemStyleForSubItems = false;
                        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOliveGreen;
                        break;
                }
                MachListView.Items[item].SubItems[4].Text = pNCMachine.mach_info.aspindle_speed.ToString();
                MachListView.Items[item].SubItems[5].Text = pNCMachine.mach_info.dspindle_speed.ToString() + "%";
                MachListView.Items[item].SubItems[6].Text = pNCMachine.mach_info.feed_speed.ToString();
                MachListView.Items[item].SubItems[7].Text = pNCMachine.mach_info.dfeed_speed.ToString() + "%";
                MachListView.Items[item].SubItems[8].Text = pNCMachine.mach_info.prog_name;
                MachListView.Items[item].SubItems[9].Text = pNCMachine.mach_info.host_value1.ToString();
                MachListView.Items[item].SubItems[10].Text = pNCMachine.mach_info.host_value2.ToString();
                MachListView.Items[item].SubItems[11].Text = pNCMachine.mach_info.host_value3.ToString();
                MachListView.Items[item].SubItems[12].Text = pNCMachine.mach_info.host_value4.ToString();
                MachListView.Items[item].SubItems[13].Text = pNCMachine.mach_info.host_value5.ToString();
                MachListView.Items[item].SubItems[14].Text = pNCMachine.mach_info.host_value6.ToString();

                MachListView.Items[item].SubItems[15].Text = pNCMachine.mach_info.ahost_value1.ToString();
                MachListView.Items[item].SubItems[16].Text = pNCMachine.mach_info.ahost_value2.ToString();
                MachListView.Items[item].SubItems[17].Text = pNCMachine.mach_info.ahost_value3.ToString();
                MachListView.Items[item].SubItems[18].Text = pNCMachine.mach_info.ahost_value4.ToString();
                MachListView.Items[item].SubItems[19].Text = pNCMachine.mach_info.ahost_value5.ToString();
                MachListView.Items[item].SubItems[20].Text = pNCMachine.mach_info.ahost_value6.ToString();

                MachListView.Items[item].SubItems[21].Text = pNCMachine.mach_info.act_axis_velocity1.ToString();
                MachListView.Items[item].SubItems[22].Text = pNCMachine.mach_info.act_axis_velocity2.ToString();
                MachListView.Items[item].SubItems[23].Text = pNCMachine.mach_info.act_axis_velocity3.ToString();
                MachListView.Items[item].SubItems[24].Text = pNCMachine.mach_info.act_axis_velocity4.ToString();
                MachListView.Items[item].SubItems[25].Text = pNCMachine.mach_info.act_axis_velocity5.ToString();
                MachListView.Items[item].SubItems[26].Text = pNCMachine.mach_info.act_axis_velocity6.ToString();
                //}

            }
            else if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_OFFLINE)
            {
                //if (this.MachListView.SelectedItems.Count > 0)
                //{
                //MachListView.Items[item].UseItemStyleForSubItems = false;
                MachListView.Items[item].SubItems[2].Text = "离线";
                MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Red;
                MachListView.Items[item].SubItems[3].Text = "--";
                MachListView.Items[item].SubItems[4].Text = "--";
                MachListView.Items[item].SubItems[5].Text = "--";
                MachListView.Items[item].SubItems[6].Text = "--";
                MachListView.Items[item].SubItems[7].Text = "--";
                MachListView.Items[item].SubItems[8].Text = "--";
                MachListView.Items[item].SubItems[9].Text = "--";
                MachListView.Items[item].SubItems[10].Text = "--";
                MachListView.Items[item].SubItems[11].Text = "--";
                MachListView.Items[item].SubItems[12].Text = "--";
                MachListView.Items[item].SubItems[13].Text = "--";
                MachListView.Items[item].SubItems[14].Text = "--";

                MachListView.Items[item].SubItems[15].Text = "--";
                MachListView.Items[item].SubItems[16].Text = "--";
                MachListView.Items[item].SubItems[17].Text = "--";
                MachListView.Items[item].SubItems[18].Text = "--";
                MachListView.Items[item].SubItems[19].Text = "--";
                MachListView.Items[item].SubItems[20].Text = "--";

                MachListView.Items[item].SubItems[21].Text = "--";
                MachListView.Items[item].SubItems[22].Text = "--";
                MachListView.Items[item].SubItems[23].Text = "--";
                MachListView.Items[item].SubItems[24].Text = "--";
                MachListView.Items[item].SubItems[25].Text = "--";
                //}

                //MachListView.Items[item].SubItems[26].Text = "--";
            }
            else if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_INIT)
            {
                //if (this.MachListView.SelectedItems.Count > 0)
                //{
                MachListView.Items[item].SubItems[2].Text = "连接……";
                //}

            }

            #region 注销
            ////if (pNCMachine.itype == (int)MachineType.SIEMENS840DSL_SW47 || pNCMachine.itype == (int)MachineType.SIEMENS840D)
            ////{
            ////    if(pNCMachine.status == CONSTDEFINE.MachStatus.MS_ONLINE)
            ////    {
            ////        if (pNCMachine.mach_info.interp_state == 0)
            ////        {
            ////            MachListView.Items[item].UseItemStyleForSubItems = false;
            ////            MachListView.Items[item].SubItems[2].Text = "运行";
            ////            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Green;
            ////        }
            ////        else
            ////        {
            ////            MachListView.Items[item].SubItems[2].Text = "停止";
            ////            MachListView.Items[item].UseItemStyleForSubItems = false;
            ////            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            ////        }
            ////        switch (pNCMachine.mach_info.disp_mode)
            ////        {
            ////            case 0:
            ////                MachListView.Items[item].SubItems[3].Text = "JOG";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkGreen;
            ////                break;
            ////            case 1:
            ////                MachListView.Items[item].SubItems[3].Text = "MDI";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Yellow;
            ////                break;
            ////            default:
            ////                MachListView.Items[item].SubItems[3].Text = "AUTO";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Green;
            ////                break;
            ////        }
            ////        MachListView.Items[item].SubItems[4].Text = pNCMachine.mach_info.aspindle_speed.ToString();
            ////        MachListView.Items[item].SubItems[5].Text = pNCMachine.mach_info.dspindle_speed.ToString() + "%";
            ////        MachListView.Items[item].SubItems[6].Text = pNCMachine.mach_info.feed_speed.ToString();
            ////        MachListView.Items[item].SubItems[7].Text = pNCMachine.mach_info.dfeed_speed.ToString() + "%";
            ////        MachListView.Items[item].SubItems[8].Text = pNCMachine.mach_info.prog_name;
            ////    }
            ////    else if(pNCMachine.status == CONSTDEFINE.MachStatus.MS_OFFLINE)
            ////    {
            ////        MachListView.Items[item].UseItemStyleForSubItems = false;
            ////        MachListView.Items[item].SubItems[2].Text = "离线";
            ////        MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Red;
            ////        MachListView.Items[item].SubItems[3].Text = "--";
            ////        MachListView.Items[item].SubItems[4].Text = "--";
            ////        MachListView.Items[item].SubItems[5].Text = "--";
            ////        MachListView.Items[item].SubItems[6].Text = "--";
            ////        MachListView.Items[item].SubItems[7].Text = "--";
            ////        MachListView.Items[item].SubItems[8].Text = "--";
            ////    }
            ////    else if(pNCMachine.status == CONSTDEFINE.MachStatus.MS_INIT)
            ////    {
            ////        MachListView.Items[item].SubItems[2].Text = "连接……";
            ////    }

            ////}
            ////else//1run2wait3stop4break5end6reset7start11unknown
            ////{
            //     if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_ONLINE)
            //     {
            //         if (pNCMachine.flag_error_state == 1)
            //         {
            //             MachListView.Items[item].UseItemStyleForSubItems = false;
            //             MachListView.Items[item].SubItems[2].Text = "故障";
            //             MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Red;
            //         }
            //         else
            //         {
            //             switch(pNCMachine.mach_info.interp_state)
            //             {
            //                 case 1:
            //                     MachListView.Items[item].UseItemStyleForSubItems = false;
            //                     MachListView.Items[item].SubItems[2].Text = "运行";
            //                     MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Green;
            //                     break;
            //                 case 2:
            //                     MachListView.Items[item].SubItems[2].Text = "等待";
            //                     MachListView.Items[item].UseItemStyleForSubItems = false;
            //                     MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //                     break;
            //                 case 3:
            //                     MachListView.Items[item].SubItems[2].Text = "停止";
            //                     MachListView.Items[item].UseItemStyleForSubItems = false;
            //                     MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //                     break;
            //                 case 4:
            //                     MachListView.Items[item].SubItems[2].Text = "结束";
            //                     MachListView.Items[item].UseItemStyleForSubItems = false;
            //                     MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //                     break;
            //                 case 5:
            //                      MachListView.Items[item].SubItems[2].Text = "中断";
            //                      MachListView.Items[item].UseItemStyleForSubItems = false;
            //                      MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //                     break;
            //                 case 6:
            //                     MachListView.Items[item].SubItems[2].Text = "复位";
            //                     MachListView.Items[item].UseItemStyleForSubItems = false;
            //                     MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //                     break;
            //                 case 7:
            //                     MachListView.Items[item].SubItems[2].Text = "start";
            //                     MachListView.Items[item].UseItemStyleForSubItems = false;
            //                     MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //                     break;
            //                 default:
            //                      MachListView.Items[item].SubItems[2].Text = "unknown";
            //                      MachListView.Items[item].UseItemStyleForSubItems = false;
            //                      MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //                      break;
            //             }
            //             //if (pNCMachine.mach_info.interp_state == 1)
            //             //{
            //             //    MachListView.Items[item].UseItemStyleForSubItems = false;
            //             //    MachListView.Items[item].SubItems[2].Text = "运行";
            //             //    MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Green;
            //             //}
            //             //else if (pNCMachine.mach_info.interp_state == 2)
            //             //{
            //             //    MachListView.Items[item].SubItems[2].Text = "等待";
            //             //    MachListView.Items[item].UseItemStyleForSubItems = false;
            //             //    MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //             //}
            //             //else if (pNCMachine.mach_info.interp_state == 3)
            //             //{
            //             //    MachListView.Items[item].SubItems[2].Text = "停止";
            //             //    MachListView.Items[item].UseItemStyleForSubItems = false;
            //             //    MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //             //}
            //             //else if (pNCMachine.mach_info.interp_state == 4)
            //             //{
            //             //    MachListView.Items[item].SubItems[2].Text = "结束";
            //             //    MachListView.Items[item].UseItemStyleForSubItems = false;
            //             //    MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //             //}
            //             //else if (pNCMachine.mach_info.interp_state == 5)
            //             //{
            //             //    MachListView.Items[item].SubItems[2].Text = "中断";
            //             //    MachListView.Items[item].UseItemStyleForSubItems = false;
            //             //    MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //             //}
            //             //else if (pNCMachine.mach_info.interp_state == 6)
            //             //{
            //             //    MachListView.Items[item].SubItems[2].Text = "复位";
            //             //    MachListView.Items[item].UseItemStyleForSubItems = false;
            //             //    MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //             //}
            //             //else if (pNCMachine.mach_info.interp_state == 7)
            //             //{
            //             //    MachListView.Items[item].SubItems[2].Text = "start";
            //             //    MachListView.Items[item].UseItemStyleForSubItems = false;
            //             //    MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //             //}
            //             //else
            //             //{
            //             //    MachListView.Items[item].SubItems[2].Text = "unknown";
            //             //    MachListView.Items[item].UseItemStyleForSubItems = false;
            //             //    MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            //             //}
            //         }
            //         switch (pNCMachine.mach_info.disp_mode)//运行模式 //1auto2mdi3jog4teachin5repos6ref7edit8handle9remoto  11unknown
            //         {
            //             case 1:
            //                 MachListView.Items[item].SubItems[3].Text = "自动";
            //                 MachListView.Items[item].UseItemStyleForSubItems = false;
            //                 MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Blue;
            //                 break;
            //             case 2:
            //                 MachListView.Items[item].SubItems[3].Text = "MDI";
            //                 MachListView.Items[item].UseItemStyleForSubItems = false;
            //                 MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Green;
            //                 break;
            //             case 3:
            //                 MachListView.Items[item].SubItems[3].Text = "JOG";
            //                 MachListView.Items[item].UseItemStyleForSubItems = false;
            //                 MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkBlue;
            //                 break;
            //             case 4:
            //                 MachListView.Items[item].SubItems[3].Text = "TEACHIN";
            //                 MachListView.Items[item].UseItemStyleForSubItems = false;
            //                 MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkGreen;
            //                 break;
            //             case 5:
            //                 MachListView.Items[item].SubItems[3].Text = "REPOS";
            //                 MachListView.Items[item].UseItemStyleForSubItems = false;
            //                 MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOliveGreen;
            //                 break;
            //             case 6:
            //                 MachListView.Items[item].SubItems[3].Text = "REF";
            //                 MachListView.Items[item].UseItemStyleForSubItems = false;
            //                 MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOrange;
            //                 break;
            //             case 7:
            //                 MachListView.Items[item].SubItems[3].Text = "EDIT";
            //                 MachListView.Items[item].UseItemStyleForSubItems = false;
            //                 MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOrange;
            //                 break;
            //             case 8:
            //                 MachListView.Items[item].SubItems[3].Text = "手轮";
            //                 MachListView.Items[item].UseItemStyleForSubItems = false;
            //                 MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOrange;
            //                 break;
            //             case 9:
            //                 MachListView.Items[item].SubItems[3].Text = "远程";
            //                 MachListView.Items[item].UseItemStyleForSubItems = false;
            //                 MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOrange;
            //                 break;      
            //             default:
            //                 MachListView.Items[item].SubItems[3].Text = "未知模式";
            //                 MachListView.Items[item].UseItemStyleForSubItems = false;
            //                 MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOliveGreen;
            //                 break;
            //         }
            //         MachListView.Items[item].SubItems[4].Text = pNCMachine.mach_info.aspindle_speed.ToString();
            //         MachListView.Items[item].SubItems[5].Text = pNCMachine.mach_info.dspindle_speed.ToString() + "%";
            //         MachListView.Items[item].SubItems[6].Text = pNCMachine.mach_info.feed_speed.ToString();
            //         MachListView.Items[item].SubItems[7].Text = pNCMachine.mach_info.dfeed_speed.ToString() + "%";
            //         MachListView.Items[item].SubItems[8].Text = pNCMachine.mach_info.prog_name;
            //     }
            //     else if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_OFFLINE)
            //     {
            //         MachListView.Items[item].UseItemStyleForSubItems = false;
            //         MachListView.Items[item].SubItems[2].Text = "离线";
            //         MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Red;
            //         MachListView.Items[item].SubItems[3].Text = "--";
            //         MachListView.Items[item].SubItems[4].Text = "--";
            //         MachListView.Items[item].SubItems[5].Text = "--";
            //         MachListView.Items[item].SubItems[6].Text = "--";
            //         MachListView.Items[item].SubItems[7].Text = "--";
            //         MachListView.Items[item].SubItems[8].Text = "--";
            //     }
            //     else if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_INIT)
            //     {
            //         MachListView.Items[item].SubItems[2].Text = "连接……";
            //     }
            ////}
            ////else if (pNCMachine.itype == (int)CONSTDEFINE.MachineType.FANUC_MD || pNCMachine.itype == (int)CONSTDEFINE.MachineType.FANUC_MD_MATE)
            ////{
            ////    if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_ONLINE)
            ////    {
            ////        if(pNCMachine.flag_error_state == 1)
            ////        {
            ////            MachListView.Items[item].UseItemStyleForSubItems = false;
            ////            MachListView.Items[item].SubItems[2].Text = "故障";
            ////            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Red;
            ////        }
            ////        else
            ////        {
            ////            if (pNCMachine.mach_info.interp_state == 3)
            ////            {
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[2].Text = "运行";
            ////                MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Green;
            ////            }
            ////            else
            ////            {
            ////                MachListView.Items[item].SubItems[2].Text = "停止";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            ////            }
            ////        }

            ////        switch (pNCMachine.mach_info.disp_mode)//系统运行方式 0MDI 1自动3EDIT4HANDLE5JOG9REF10REMOTO
            ////        {
            ////            case 0:
            ////                MachListView.Items[item].SubItems[3].Text = "MDI";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Blue;
            ////                break;
            ////            case 1:
            ////                MachListView.Items[item].SubItems[3].Text = "自动";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Green;
            ////                break;
            ////            case 3:
            ////                MachListView.Items[item].SubItems[3].Text = "EDIT";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkBlue;
            ////                break;
            ////            case 4:
            ////                MachListView.Items[item].SubItems[3].Text = "HANDLE";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkGreen;
            ////                break;
            ////            case 5:
            ////                MachListView.Items[item].SubItems[3].Text = "JOG";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOliveGreen;
            ////                break;
            ////            case 9:
            ////                MachListView.Items[item].SubItems[3].Text = "REF";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOrange;
            ////                break;
            ////            case 10:
            ////                MachListView.Items[item].SubItems[3].Text = "REMOTE";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkSeaGreen;
            ////                break;
            ////            default:
            ////                MachListView.Items[item].SubItems[3].Text = "未知模式";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOliveGreen;
            ////                break;
            ////        }
            ////        MachListView.Items[item].SubItems[4].Text = pNCMachine.mach_info.aspindle_speed.ToString();
            ////        MachListView.Items[item].SubItems[5].Text = pNCMachine.mach_info.dspindle_speed.ToString() + "%";
            ////        MachListView.Items[item].SubItems[6].Text = pNCMachine.mach_info.feed_speed.ToString();
            ////        MachListView.Items[item].SubItems[7].Text = pNCMachine.mach_info.dfeed_speed.ToString() + "%";
            ////        MachListView.Items[item].SubItems[8].Text = pNCMachine.mach_info.prog_name;
            ////    }
            ////    else if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_OFFLINE)
            ////    {
            ////        MachListView.Items[item].UseItemStyleForSubItems = false;
            ////        MachListView.Items[item].SubItems[2].Text = "离线";
            ////        MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Red;
            ////        MachListView.Items[item].SubItems[3].Text = "--";
            ////        MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.White;
            ////        MachListView.Items[item].SubItems[4].Text = "--";
            ////        MachListView.Items[item].SubItems[4].BackColor = System.Drawing.Color.White;
            ////        MachListView.Items[item].SubItems[5].Text = "--";
            ////        MachListView.Items[item].SubItems[5].BackColor = System.Drawing.Color.White;
            ////        MachListView.Items[item].SubItems[6].Text = "--";
            ////        MachListView.Items[item].SubItems[6].BackColor = System.Drawing.Color.White;
            ////        MachListView.Items[item].SubItems[7].Text = "--";
            ////        MachListView.Items[item].SubItems[7].BackColor = System.Drawing.Color.White;
            ////        MachListView.Items[item].SubItems[8].Text = "--";
            ////        MachListView.Items[item].SubItems[8].BackColor = System.Drawing.Color.White;
            ////    }
            ////    else if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_INIT)
            ////    {
            ////        MachListView.Items[item].SubItems[2].Text = "连接……";
            ////    }
            ////}
            ////else if (pNCMachine.itype == (int)CONSTDEFINE.MachineType.GOLDING)
            ////{
            ////    if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_ONLINE)
            ////    {
            ////        if (pNCMachine.flag_error_state == 1)
            ////        {
            ////            MachListView.Items[item].UseItemStyleForSubItems = false;
            ////            MachListView.Items[item].SubItems[2].Text = "故障";
            ////            MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Red;
            ////        }
            ////        else
            ////        {
            ////            if (pNCMachine.mach_info.interp_state == 2 || pNCMachine.mach_info.interp_state == 4)
            ////            {
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[2].Text = "运行";
            ////                MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Green;
            ////            }
            ////            else
            ////            {
            ////                MachListView.Items[item].SubItems[2].Text = "停止";
            ////                MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.DarkGreen;
            ////            }
            ////            switch (pNCMachine.mach_info.disp_mode)//系统运行方式 0MDI 1自动3EDIT4HANDLE5JOG9REF10REMOTO
            ////            {
            ////                case 0:
            ////                    MachListView.Items[item].SubItems[3].Text = "MDI";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Blue;
            ////                    break;
            ////                case 1:
            ////                    MachListView.Items[item].SubItems[3].Text = "自动";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Green;
            ////                    break;
            ////                case 2:
            ////                    MachListView.Items[item].SubItems[3].Text = "AUTO";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Green;
            ////                    break;
            ////                case 3:
            ////                    MachListView.Items[item].SubItems[3].Text = "STEP";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.Chocolate;
            ////                    break;
            ////                case 4:
            ////                    MachListView.Items[item].SubItems[3].Text = "MANUJC";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkGreen;
            ////                    break;
            ////                case 5:
            ////                    MachListView.Items[item].SubItems[3].Text = "MANJS";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOliveGreen;
            ////                    break;
            ////                case 6:
            ////                    MachListView.Items[item].SubItems[3].Text = "PROF";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOliveGreen;
            ////                    break;
            ////                case 7:
            ////                    MachListView.Items[item].SubItems[3].Text = "HOME";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkSeaGreen;
            ////                    break;
            ////                case 8:
            ////                    MachListView.Items[item].SubItems[3].Text = "RESET";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkSeaGreen;
            ////                    break;
            ////                case 9:
            ////                    MachListView.Items[item].SubItems[3].Text = "HANDWHEEL";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOrange;
            ////                    break;
            ////                default:
            ////                    MachListView.Items[item].SubItems[3].Text = "未知模式";
            ////                    MachListView.Items[item].UseItemStyleForSubItems = false;
            ////                    MachListView.Items[item].SubItems[3].BackColor = System.Drawing.Color.DarkOliveGreen;
            ////                    break;
            ////            }
            ////            MachListView.Items[item].SubItems[4].Text = pNCMachine.mach_info.aspindle_speed.ToString();
            ////            MachListView.Items[item].SubItems[5].Text = pNCMachine.mach_info.dspindle_speed.ToString() + "%";
            ////            MachListView.Items[item].SubItems[6].Text = pNCMachine.mach_info.feed_speed.ToString();
            ////            MachListView.Items[item].SubItems[7].Text = pNCMachine.mach_info.dfeed_speed.ToString() + "%";
            ////            MachListView.Items[item].SubItems[8].Text = pNCMachine.mach_info.prog_name;
            ////        }

            ////    }
            ////    else if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_OFFLINE)
            ////    {
            ////        MachListView.Items[item].UseItemStyleForSubItems = false;
            ////        MachListView.Items[item].SubItems[2].Text = "离线";
            ////        MachListView.Items[item].SubItems[2].BackColor = System.Drawing.Color.Red;
            ////        MachListView.Items[item].SubItems[3].Text = "--";
            ////        MachListView.Items[item].SubItems[4].Text = "--";
            ////        MachListView.Items[item].SubItems[5].Text = "--";
            ////        MachListView.Items[item].SubItems[6].Text = "--";
            ////        MachListView.Items[item].SubItems[7].Text = "--";
            ////        MachListView.Items[item].SubItems[8].Text = "--";
            ////    }
            ////    else if (pNCMachine.status == CONSTDEFINE.MachStatus.MS_INIT)
            ////    {
            ////        MachListView.Items[item].SubItems[2].Text = "连接……";
            ////    }
            ////}
            #endregion
        }

        private int GetMachineItem(NCMachine pNCMachine)
        {
            int item = MachListView.Items.Count;
            // ListViewItem foundItem = listView_Machines.FindItemWithText(pNCMachine.machnum,true,0);
            int i = -1;
            for (i = 0; i < item; i++)
            {
                ListViewItem lvi = MachListView.Items[i];
                if (lvi.SubItems[0].Text == pNCMachine.machnum)
                {
                    break;
                }
            }
            return i;
        }
        #endregion

        #region 按钮操作

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            dataacquire.StopAcquireData();
            Process.GetCurrentProcess().Kill();
            System.Environment.Exit(0);
        }
        private void button_login_Click(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            loginForm.StartPosition = FormStartPosition.CenterParent;
            loginForm.ShowDialog();
            VerifyFlag = loginForm.VerifyFlag;
            if (VerifyFlag == 0)
            {
                button_start.Enabled = true;
                button_exit.Enabled = true;
                DataBaseSetting(loginForm.s_dbname, loginForm.s_dbpasswd, loginForm.s_dbsource);
                statusStrip_Status.Text = "当前登录用户：" + loginForm.s_userid;
                string date = GetDateTime();
                textBox_sysinfo.AppendText(date + ":" + loginForm.s_userid + "用户登录成功\r\n");
                Log.WriteLogErrMsg(date + ":" + "用户登录成功\r\n");
                InitialTreeView();
            }
            else
            {
                string date = GetDateTime();
                textBox_sysinfo.AppendText(date + ":" + "登录失败\r\n");
                Log.WriteLogErrMsg(date + ":" + "登录失败\r\n");
            }
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            OracleDataReader odr;
            db.ConnectToDB();
            string sql = "select mach_num,ip,iport,itype from room_machine where acquiredata_status = 1 order by mach_num asc";
            odr = db.ReturnDataReader(sql);
            while (odr.Read())
            {
                string s_machnum = odr["mach_num"].ToString();
                string s_ip = odr["ip"].ToString();
                string s_port = odr["iport"].ToString();
                string s_type = odr["itype"].ToString();
                int i_type = Convert.ToInt16(s_type);
                Invoke(new EventHandler(delegate { InsertMachineToList(s_machnum, s_ip, i_type); }));// 异步执行主界面的内容修改    
            }
            dataacquire.SetMainDlg(this);
            dataacquire.SetDBConnArg(db.db_user, db.db_passwd, db.db_source);
            dataacquire.StartDataAquire();

            string date = GetDateTime();
            textBox_sysinfo.AppendText(date + ":" + "开启采集……\r\n");
            //记录日志
            Log.WriteLogErrMsg(date + ":" + "开启采集……\r\n");
            // db.CloseConn(); // 20210528新增关闭数据库
            //开启定时器
            timer_update.Interval = 500;
            timer_update.Enabled = true;
            timer_update.Start();
            button_start.Enabled = false;
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            dataacquire.StopAcquireData();
            // System.Threading.Thread.Sleep(500);
            try
            {
                Process.GetCurrentProcess().Kill();
                System.Environment.Exit(0);
            }
            catch (Exception)
            {
                // 如果遇到win32Exception异常，直接忽略,这是由于开启窗口过多引起的
                // throw;
            }

        }
        #endregion



    }
}
