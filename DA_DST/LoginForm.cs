using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Xml;
using Oracle.DataAccess.Client;
using ConnectDB;

namespace DA_DST
{
    public partial class LoginForm : Form
    {
        public string s_userid, s_userpasswd, s_dbname, s_dbpasswd, s_dbsource;
        OracleDB db = new OracleDB();
        public int VerifyFlag;
        public LoginForm()
        {
            InitializeComponent();
            InitialLoadXML("./Config/Login.xml");
            VerifyFlag = -1;//0登录成功-1登录失败
        }

        public void InitialLoadXML(string xml_address)
        {          
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xml_address);
                //得到根节点
                XmlNode xn = xmlDoc.SelectSingleNode("LoginInfo");
                //得到所有1级子节点
                XmlElement xe = (XmlElement)xn;
                s_userid = xe.GetElementsByTagName("userid").Item(0).InnerText;
                s_userpasswd = xe.GetElementsByTagName("userpasswd").Item(0).InnerText;
                s_dbname = xe.GetElementsByTagName("dbname").Item(0).InnerText;
                s_dbpasswd = xe.GetElementsByTagName("dbpasswd").Item(0).InnerText;
                s_dbsource = xe.GetElementsByTagName("dbsource").Item(0).InnerText;
               
                textBox_username.Text = s_userid;
                textBox_passwd.Text = s_userpasswd;
                textBox_dbname.Text = s_dbname;
                textBox_dbpasswd.Text = s_dbpasswd;
                textBox_sourcename.Text = s_dbsource;                
            }
            catch (Exception e)
            {
                ;
            }
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            //配置信息写入xml文档
            WriteXMLFile("./Config/Login.xml");
            //数据库配置信息写入DataBaseSetting
            db.db_passwd = s_dbpasswd;
            db.db_source = s_dbsource;
            db.db_user = s_dbname;
            //登录程序
            bool ret = false;
            if (textBox_username == null || textBox_passwd == null || textBox_username.Text == "" || textBox_passwd.Text == "" || textBox_dbname == null || textBox_dbpasswd == null || textBox_sourcename == null || textBox_dbname.Text == "" || textBox_dbpasswd.Text == "" || textBox_sourcename.Text == "")
            {
                MessageBox.Show("请完整的填写信息！");
            }
            else
            {
                ret = VerificationUser();
                if(ret == false)
                {
                    VerifyFlag = -1;
                    MessageBox.Show("登录失败！请检查用户名和密码或数据库信息是否正确");
                }
                else
                {
                    VerifyFlag = 0;
                    MessageBox.Show("欢迎" + textBox_username.Text + "用户登录系统！");
                    this.Close();
                }
            }
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region XML文档写入
        public void WriteXMLFile(string xml_address)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                s_userid = textBox_username.Text;
                s_userpasswd = textBox_passwd.Text;
                s_dbname = textBox_dbname.Text;
                s_dbpasswd = textBox_dbpasswd.Text;
                s_dbsource = textBox_sourcename.Text;
                xmlDoc.Load(xml_address);
                //得到跟几点的Device
                XmlNode xn = xmlDoc.SelectSingleNode("LoginInfo");
                //得到所有1级子节点
                XmlElement xe = (XmlElement)xn;
                xe.GetElementsByTagName("userid").Item(0).InnerText = s_userid;
                xe.GetElementsByTagName("userpasswd").Item(0).InnerText = s_userpasswd;
                xe.GetElementsByTagName("dbname").Item(0).InnerText = s_dbname;
                xe.GetElementsByTagName("dbpasswd").Item(0).InnerText = s_dbpasswd;
                xe.GetElementsByTagName("dbsource").Item(0).InnerText = s_dbsource;
                xmlDoc.Save(xml_address);
            }
            catch (Exception e)
            {
                ;
            }
        }
        #endregion

        #region 验证用户名和密码
        public bool VerificationUser()
        {
            bool ret = false;
            int err = db.ConnectToDB();
            DataSet dataset = new DataSet();
            string sql = "";
            if (err == 0)
            {
                sql = "select authority from dncuser where username = '" + s_userid + "' and passwd = '" + s_userpasswd + "'";
                dataset = db.ReturnDataSet(sql);
                if (dataset.Tables[0].Rows.Count == 0)
                {
                    ret = false;
                }
                else
                {
                    int i_author = Convert.ToInt32(dataset.Tables[0].Rows[0]["AUTHORITY"].ToString());
                    //if (i_author == 1)
                    //{
                    //    authorFlag = 1;
                    //}
                    //else
                    //{
                    //    authorFlag = 0;
                    //}
                    ret = true;

                }
            }
            else
            {
                MessageBox.Show("数据库连接失败，请检查数据库！");
            }
            // db.CloseConn(); // 20210528新增关闭数据库
            return ret;
        }
        #endregion

        
    }
}
