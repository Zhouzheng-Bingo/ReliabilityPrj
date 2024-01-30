/***************************类说明*********************************/
//用于Oracle方法和属性
//未来可加入针对MYSQL、SQLServer等类
/*****************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Collections;
using System.Reflection;

namespace ConnectDB
{
    class OracleDB
    {
        public string db_user;
        public string db_passwd;
        public string db_source;
        static string connectionString;
        OracleConnection myConnection;
        #region 打开数据库
        //打开数据库
        public int ConnectToDB() //连接数据库函数
        {
            int ret = -1;
            //connectionString = "Data Source=" + db_source + ";User ID=" + db_user + ";PassWord=" + db_passwd;
            connectionString = "Data Source=orcl;User ID=DA04;PassWord=DA04";
            myConnection = new OracleConnection(connectionString);
            try
            {
                myConnection.Open();
                ret = 0;
            }
            catch (Exception a)
            {
                ret = -1;
                //MessageBox.Show(a.ToString());
            }
            return ret;
        }
        #endregion

        #region 关闭数据库
        //关闭数据库
        public void CloseConn()
        {
            if (myConnection.State == ConnectionState.Open)
                myConnection.Close();
        }
        #endregion


        #region 执行SQL语句，返回 DataReader,用之前一定要先.read()打开,然后才能读到数据
        /// <summary>  
        /// 执行SQL语句，返回 DataReader,用之前一定要先.read()打开,然后才能读到数据  
        /// </summary>  
        /// <param name="sql">sql语句</param>  
        /// <returns>返回一个OracleDataReader</returns>  
        public OracleDataReader ReturnDataReader(String sql)
        {
            OracleCommand command = new OracleCommand(sql, myConnection);
            return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
        #endregion

        #region 读取函数
        public DataSet ReturnDataSet(string sql)
        {
            DataSet dataSet = new DataSet();
            OracleDataAdapter OraDA = new OracleDataAdapter(sql, myConnection);
            OraDA.Fill(dataSet);
            return dataSet;
        }
        #endregion

        #region 执行SQL语句，返回记录总数数
        /// <summary>  
        /// 执行SQL语句，返回记录总数数  
        /// </summary>  
        /// <param name="sql">sql语句</param>  
        /// <returns>返回记录总条数</returns>  
        public int GetRecordCount(string sql)
        {
            int recordCount = 0;
            OracleCommand command = new OracleCommand(sql, myConnection);
            OracleDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                recordCount++;
            }
            dataReader.Close();
            //    CloseConn();  
            return recordCount;
        }
        #endregion

        //插入数据库
        #region 执行SQL语句,返回所影响的行数
        /// <summary>  
        /// 执行SQL语句,返回所影响的行数  
        /// </summary>  
        /// <param name="sql"></param>  
        /// <returns></returns>  
        public int ExecuteSQL(string sql)
        {
            int Cmd = 0;

            OracleCommand command = new OracleCommand(sql, myConnection);
            try
            {
                Cmd = command.ExecuteNonQuery();
                return 0;
            }
            catch(Exception e)
            {
                Console.WriteLine("error:" + e);
                Cmd = -1;
            }
            finally
            {
                //     CloseConn();  
            }

            return Cmd;
        }
        #endregion

        #region 创建不带参数的存储过程
        /// <summary>  
        /// 创建不带参数的存储过程  
        /// </summary>  
        /// <param name="sql"></param>  
        /// <returns></returns>  
        public int ExecuteNoParaProc(string sql)
        {
            int Cmd = 0;
            OracleCommand orm = myConnection.CreateCommand();
            orm.CommandType = CommandType.StoredProcedure;
            orm.CommandText = sql;
            try
            {
                Cmd = orm.ExecuteNonQuery();
                Cmd = 0;
            }
            catch
            {
                Cmd = -1;
            }
            return Cmd;
        }
        #endregion

        #region 创建带参数的存储过程
        /// <summary>  
        /// 创建不带参数的存储过程  
        /// </summary>  
        /// <param name="sql"></param>  
        /// <returns></returns>  
        public int ExecuteParaProc(string sql)
        {
            int ret = -1;
            OracleCommand om = myConnection.CreateCommand();
            om.CommandType = CommandType.StoredProcedure;
            om.CommandText = sql;
            om.Parameters.Add("v_id", OracleDbType.Int32).Direction = ParameterDirection.Input;
            om.Parameters["v_id"].Value = 2;
            om.Parameters.Add("v_name", OracleDbType.Varchar2).Direction = ParameterDirection.Input;
            om.Parameters["v_name"].Value = "aa";
            om.ExecuteNonQuery();
            return ret;
        }
        #endregion



    }

}