using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace GOLDING
{
    class Golding
    {
        public struct PmCartesia
        {
            public double x, y, z;

        } ;

        public struct CncPose
        {
            PmCartesia tran;
            double a, b, c;
            double u, v, w;
        }
        public struct CncCmd
        {
            public System.Int32 mode;//模式
            public System.Int32 type;//类型
            public System.Int32 enable;//使能
            public System.Int32 axis;//轴号
            public System.Int32 rate;//倍率
            public System.Int32 index;//序号
            public System.Int32 line;//行号
            public System.Int32 int_value;
            public System.UInt32 long_value;//长整值
            public System.Double d_value;
            public System.Double vel;//速度
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public char[] keywords;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] filename;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] command;
            public CncPose origin;
            public System.Int32 tool;
            public System.Double length;
            public System.Double wear;
            public System.Double length2;
            public System.Double wear2;
            public System.Double diameter;
            public System.Double dia_wear;
            public System.Int32 orientation;
            public System.Double tool_coord_x;
            public System.Double tool_coord_y;
            public System.Double tool_coord_z;
        } ;
        public struct DNC_TOOL_LIFE_TABLE //刀具寿命管理
        {
            public int id;//序号
            public int used_num;//使用次数
            public int alarm_num;//预警次数
            public int life_num;//寿命
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public int[] alternate;//替补
            public int surplus;
        } ;
        public struct DNC_CANON_TOOL_TABLE
        {
            public int type;  //0:车刀   1:铣刀  2: 磨刀  3:其他//MIN_CUTTER_OFFSET_VAR 5001-5099
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            float[] toolOffset; //MIN_CUTTER_OFFSET_VAR+300   5101-5199   5201 -5299  5301-5399
            float radiusOffset; //半径   //MIN_CUTTER_OFFSET_VAR+400   5401-5499   
            int orientation; //方向码//MIN_CUTTER_OFFSET_VAR+500   5501-5599     
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            float[] toolWear;  //MIN_CUTTER_OFFSET_VAR+800   5601-5699   5701 -5799  5801-5899
            float radiusWear; //半径磨耗//MIN_CUTTER_OFFSET_VAR+900   5901-5999
        };

        public struct DNC_TOOLOFFSET_STRUCT
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 99)]
            public DNC_CANON_TOOL_TABLE[] chanToolTable; // BASE_CUTTER_OFFSET  5001-6000	
        };
        [DllImport("NCConnectDll.dll", EntryPoint = "connectToNC")]
        public  static extern int connectToNC(string IPAddr, System.UInt16 Port, ref int iNCIndex);

        [DllImport("NCConnectDll.dll", EntryPoint = "disconnectToNC")]
        public  static extern int disconnectToNC(int iNCIndex);

        [DllImport("NCConnectDll.dll", EntryPoint = "getStatusIntVal")]
        public  static extern int getStatusIntVal(int iNCIndex, int chanNo, int index, ref int retValue);
        [DllImport("NCConnectDll.dll", EntryPoint = "getStatusIntArrayVal")]
        public  static extern int getStatusIntArrayVal(int iNCIndex, int chanNo, int index, ref int[] retArray);

        [DllImport("NCConnectDll.dll", EntryPoint = "getStatusDoubleVal")]
        public  static extern int getStatusDoubleVal(int iNCIndex, int chanNo, int index, ref double retDoubleValue);
        [DllImport("NCConnectDll.dll", EntryPoint = "getStatusDoubleArrayVal")]
        public  static extern int getStatusDoubleArrayVal(int iNCIndex, int chanNo, int index, ref double[] retArray);
        [DllImport("NCConnectDll.dll", EntryPoint = "getStatusStrVal")]
        public  static extern int getStatusStrVal(int iNCIndex, int chanNo, int index, StringBuilder retString);
        [DllImport("NCConnectDll.dll", EntryPoint = "getIOVal")]
        public  static extern int getIOVal(int iNCIndex, int index, int signal_index, StringBuilder retString);
        [DllImport("NCConnectDll.dll", EntryPoint = "getStatusAxisVal")]
        public static extern int getStatusAxisVal(int iNCIndex, int chanNo, int index, double[] retArray);

        [DllImport("NCConnectDll.dll", EntryPoint = "getErrorVal")]
        public  static extern int getErrorVal(int iNCIndex, int chanNo, StringBuilder retString);

        [DllImport("NCConnectDll.dll", EntryPoint = "getAutoMotionErrorVal")]
        public  static extern int getAutoMotionErrorVal(int iNCIndex, int chanNo, StringBuilder retString);

        [DllImport("NCConnectDll.dll", EntryPoint = "getAutoPlcErrorVal")]
        public  static extern int getAutoPlcErrorVal(int iNCIndex, int chanNo, StringBuilder retString);
        [DllImport("NCConnectDll.dll", EntryPoint = "getToolLifeTable")]
        public  static extern int getToolLifeTable(int iNCIndex, int chanNo, int index, ref DNC_TOOL_LIFE_TABLE toolLifeTable);
        [DllImport("NCConnectDll.dll", EntryPoint = "getToolOffsetTable")]
        public  static extern int getToolOffsetTable(int iNCIndex, int chanNo, int index, ref DNC_TOOLOFFSET_STRUCT chantooloffset);

        [DllImport("NCConnectDll.dll", EntryPoint = "getStatusPeek")]
        public  static extern int getStatusPeek(int iNCIndex);

    }
}
