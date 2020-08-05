﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BodDetect
{
    public enum SysStatus
    {
        Sampling = 1,
        Pause,
        Complete
    }

    public enum RecDataType
    {
        error = -1,
        _uint16,
        _uint32,
        Little_float,
        Big_float,
        DO_float
    }


    public enum MemoryAreaCode
    {
        WR = 0X31,
        DR = 0X80,
        IO = 0X81
    }


    public enum PumpValveType
    {
        work = 1,
        pre = 2
    }

    public enum PrePumpWork
    {
        preAbsorb = 1,
        preDrain
    }

    public enum ProcessType
    {
        init,
        SampleWater,
        StandDil,
        SampleDil,
        BodStand,
        BodSample,
        DrainEmpty,
        Waitding
    }

    class Tool
    {

        private const Int32 WM_SYSCOMMAND = 274;
        private const UInt32 SC_CLOSE = 61536;
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegisterWindowMessage(string lpString);


        #region 虚拟键盘控制

        public static Process ShowInputPanel(Process kbpr)
        {

            try
            {
                if (kbpr == null || kbpr.HasExited)
                {
                    kbpr = System.Diagnostics.Process.Start("osk.exe");
                }

                return kbpr; // 打开系统键盘


                //C:\\Windows\\System32\\osk.exe  C:\\Program Files\\Common Files\\microsoft shared\\ink\\TabTip.exe
                //string file = "C:\\Windows\\System32\\osk.exe";

                //if (System.IO.File.Exists(file))
                //{
                //    return -1;
                //}
                ////Process.Start(@"C:\Windows\System32\osk.exe");


                //ProcessStartInfo procStartInfo = new ProcessStartInfo()
                //{
                //    RedirectStandardError = true,
                //    RedirectStandardOutput = true,
                //    UseShellExecute = false,
                //    CreateNoWindow = true,
                //    Verb = "runas",
                //    FileName = file,
                //    Arguments = "/user:Administrator cmd /K "
                //};

                //using (Process proc = new Process())
                //{
                //    proc.StartInfo = procStartInfo;
                //    proc.Start();
                //    string output = proc.StandardOutput.ReadToEnd();
                //    if (string.IsNullOrEmpty(output))
                //        output = proc.StandardError.ReadToEnd();

                //}
                ////    Process.Start(file);
                ////return SetUnDock(); //不知SetUnDock()是什么，所以直接注释返回1
                //return 1;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void HideInputPanel(Process kbpr)
        {

            if (kbpr != null && !kbpr.HasExited)
            {
                kbpr.CloseMainWindow();

                kbpr.Kill();
            }
        }

        #endregion

        #region 不同数据类型字节
        public static string StringtoHex(byte[] myBytes)
        {
            string result = null;
            foreach (byte b in myBytes)
            {
                string val = b.ToString("X");
                result += (val.Length == 2 ? val : "0" + val);
            }
            return result;
        }

        public static ushort[] ToShorts(byte[] data)
        {
            ushort[] r = new ushort[data.Length / 2];
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = data[i * 2 + 1];
                r[i] = (ushort)(r[i] | data[i * 2] << 8);
            }
            return r;
        }

        public static float[] BigToFloats(byte[] data)
        {
            float[] r = new float[data.Length / 4];
            for (int i = 0; i < r.Length; i++)
            {
                byte[] floats = new byte[4];
                floats[0] = data[i * 4];
                floats[1] = data[i * 4 + 1];
                floats[2] = data[i * 4 + 2];
                floats[3] = data[i * 4 + 3];
                Array.Reverse(floats);   //反转数组转成大端。
                r[i] = BitConverter.ToSingle(floats, 0);
                //r[i] = data[i * 2 + 1]; 
                //r[i] = (ushort)(r[i] | data[i * 2] << 8);
            }
            return r;
        }

        public static float[] LitToFloats(byte[] data)
        {
            float[] r = new float[data.Length / 4];
            for (int i = 0; i < r.Length; i++)
            {
                byte[] floats = new byte[4];
                floats[3] = data[i * 4];
                floats[2] = data[i * 4 + 1];
                floats[1] = data[i * 4 + 2];
                floats[0] = data[i * 4 + 3];

                r[i] = BitConverter.ToSingle(floats, 0);
            }
            return r;
        }

        public static float[] DoFloats(byte[] data)
        {
            float[] r = new float[data.Length / 4];
            for (int i = 0; i < r.Length; i++)
            {
                byte[] floats = new byte[4];
                floats[0] = data[i * 4 + 1];
                floats[1] = data[i * 4];
                floats[2] = data[i * 4 + 3];
                floats[3] = data[i * 4 + 2];

                r[i] = BitConverter.ToSingle(floats, 0);

            }
            return r;
        }

        public static uint[] ToInt32(byte[] data)
        {
            uint[] r = new uint[data.Length / 4];
            for (int i = 0; i < r.Length; i++)
            {
                byte[] int32s = new byte[4];
                int32s[3] = data[i * 4];
                int32s[2] = data[i * 4 + 1];
                int32s[1] = data[i * 4 + 2];
                int32s[0] = data[i * 4 + 3];
                r[i] = BitConverter.ToUInt32(int32s, 0);
            }
            return r;

        }

        public static byte[] ToBytes(ushort[] shorts)
        {
            byte[] r = new byte[shorts.Length * 2];
            for (int i = 0; i < shorts.Length; i++)
            {
                r[i * 2] = (byte)(shorts[i] >> 8);
                r[i * 2 + 1] = (byte)shorts[i];
            }

            return r;
        }

        public static byte[] ToBytes(int shorts)
        {
            byte[] r = new byte[2];

            r[0] = (byte)(shorts >> 8);
            r[1] = (byte)shorts;

            return r;
        }

        public static bool IsSameBytes(byte[] LeftData, byte[] RightData)
        {
            if (LeftData == null || RightData == null)
            {
                return false;
            }

            int leftCount = LeftData.Length;
            int RightCount = RightData.Length;

            if (leftCount != RightCount)
            {
                return false;
            }

            for (int i = 0; i < leftCount; i++)
            {
                if (LeftData[i] != RightData[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckCRC16(byte[] data)
        {
            int len = data.Length;

            if (len - 2 <= 0)
            {
                return false;
            }

            byte[] CRCData = { 0, 0 };


            ushort crc = 0xFFFF;

            for (int i = 0; i < len - 2; i++)
            {
                crc = (ushort)(crc ^ (data[i]));
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                }
            }
            CRCData[0] = (byte)((crc & 0xFF00) >> 8);  //高位置
            CRCData[1] = (byte)(crc & 0x00FF);         //低位置

            if (CRCData[0] != data[len - 2] || CRCData[1] != data[len - 1]) 
            {
                return false;
            }

            return true;

        }

        #endregion

        public static string GetProcessTypeToString(ProcessType processType) 
        {
            string text = "系统运转状态：";
            switch (processType)
            {
                case ProcessType.init:
                    text += "正在初始化...";
                    break;
                case ProcessType.SampleWater:
                    text += "正在取水样...";
                    break;
                case ProcessType.StandDil:
                    text += "正在稀释标定液...";
                    break;
                case ProcessType.SampleDil:
                    text += "正在稀释样液...";
                    break;
                case ProcessType.BodStand:
                    text += "正在标定BOD...";
                    break;
                case ProcessType.BodSample:
                    text += "正在测量BOD...";
                    break;
                case ProcessType.DrainEmpty:
                    text += "测量完成,正在排空溶液...";
                    break;
                case ProcessType.Waitding:
                    text += "系统空闲...";
                    break;
                default:
                    break;
            }

            return text;
        }


        #region 本地网络数据
        /// <summary>
        /// 取本机主机ip
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            try
            {

                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        string ip = "";
                        ip = IpEntry.AddressList[i].ToString();
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

    }
}
