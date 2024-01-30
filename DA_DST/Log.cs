using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DA_DST
{
    class Log
    {
        //加入文件读写锁
        static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
        public static void WriteLog(Exception ex)
        {         
            string log_folder = AppDomain.CurrentDomain.BaseDirectory + "Log";
            string date_name = DateTime.Now.ToShortDateString().Replace("/", "");
            string filePath = log_folder + "\\" + date_name + ".log";
            if (!System.IO.Directory.Exists(log_folder))
            {
                System.IO.Directory.CreateDirectory(log_folder);
            }
            if (!System.IO.File.Exists(filePath))
            {
                System.IO.File.Create(filePath).Close();
            }
            LogWriteLock.EnterWriteLock();
            System.IO.StreamWriter sw = System.IO.File.AppendText(filePath);
            sw.WriteLine("-----------------------------------------------");
            sw.WriteLine("Date:" + DateTime.Now.ToShortDateString() + " Time" + DateTime.Now.ToShortTimeString());
            sw.WriteLine(ex.Message);
            sw.WriteLine(ex.StackTrace);
            sw.Close();
            LogWriteLock.ExitWriteLock();
        }
        public static void WriteLogErrMsg(string strErrMsg)
        {
            string log_folder = AppDomain.CurrentDomain.BaseDirectory + "Log";
            string date_name = DateTime.Now.ToShortDateString().Replace("/","");
            string filePath = log_folder + "\\" + date_name + ".log";
            if (!System.IO.Directory.Exists(log_folder))
            {
                System.IO.Directory.CreateDirectory(log_folder);
            }
            if (!System.IO.File.Exists(filePath))
            {
                System.IO.File.Create(filePath).Close();
            }
            LogWriteLock.EnterWriteLock();
            System.IO.StreamWriter sw = System.IO.File.AppendText(filePath);
            sw.WriteLine("-----------------------------------------------");
            sw.WriteLine("Date:" + DateTime.Now.ToShortDateString() + " Time" + DateTime.Now.ToShortTimeString());
            sw.WriteLine(strErrMsg);
            //sw.WriteLine(ex.StackTrace);
            sw.Close();
            LogWriteLock.ExitWriteLock();
        }
    }
}
