using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Schema;


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
        Sensor,
        CodCollection,
        SampleWater,
        StandDil,
        SampleDil,
        BodStand,
        BodSample,
        BodSampleComplete,
        BodWash,
        DrainEmpty,
        Waitding
    }

    public class Tool
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
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                return null;
            }
        }

        public static void HideInputPanel(Process kbpr)
        {
            try
            {
                if (kbpr != null && !kbpr.HasExited)
                {
                    kbpr.CloseMainWindow();

                    kbpr.Kill();
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

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
                case ProcessType.Sensor:
                    text += "正在采集传感器数据...";
                    break;
                case ProcessType.CodCollection:
                    text += "正在采集Cod数据...";
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
                case ProcessType.BodSampleComplete:
                    text += "测量BOD完成....";
                    break;
                case ProcessType.BodWash:
                    text += "正在清洗BOD...";
                    break;
                case ProcessType.DrainEmpty:
                    text += "正在排空溶液...";
                    break;
                case ProcessType.Waitding:
                    text += "系统空闲...";
                    break;
                default:
                    break;
            }

            return text;
        }


        public static string GetBodStatusToString(int status)
        {
            string text = "";

            switch (status)
            {
                case 0:
                    text = "空闲";
                    break;
                case 1:
                    text = "清洗未完成";
                    break;
                case 2:
                    text = "正在做标准";
                    break;
                case 3:
                    text = "正在做样品";
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
                LogUtil.LogError(ex);

                return ex.Message;
            }
        }
        #endregion

        #region 数据库导出成excel
        public class Export
        {
            public string Encoding = "UTF-8";

            public void EcportExcel(DataTable dt, string fileName)
            {

                if (dt != null)
                {
                    StringWriter sw = new StringWriter();
                    CreateStringWriter(dt, ref sw);
                    sw.Close();
                }

            }

            private void CreateStringWriter(DataTable dt, ref StringWriter sw)
            {
                string sheetName = "sheetName";

                sw.WriteLine("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
                sw.WriteLine("<head>");
                sw.WriteLine("<!--[if gte mso 9]>");
                sw.WriteLine("<xml>");
                sw.WriteLine(" <x:ExcelWorkbook>");
                sw.WriteLine(" <x:ExcelWorksheets>");
                sw.WriteLine(" <x:ExcelWorksheet>");
                sw.WriteLine(" <x:Name>" + sheetName + "</x:Name>");
                sw.WriteLine(" <x:WorksheetOptions>");
                sw.WriteLine(" <x:Print>");
                sw.WriteLine(" <x:ValidPrinterInfo />");
                sw.WriteLine(" </x:Print>");
                sw.WriteLine(" </x:WorksheetOptions>");
                sw.WriteLine(" </x:ExcelWorksheet>");
                sw.WriteLine(" </x:ExcelWorksheets>");
                sw.WriteLine("</x:ExcelWorkbook>");
                sw.WriteLine("</xml>");
                sw.WriteLine("<![endif]-->");
                sw.WriteLine("</head>");
                sw.WriteLine("<body>");
                sw.WriteLine("<table>");
                sw.WriteLine(" <tr>");
                string[] columnArr = new string[dt.Columns.Count];
                int i = 0;
                foreach (DataColumn columns in dt.Columns)
                {

                    sw.WriteLine(" <td>" + columns.ColumnName + "</td>");
                    columnArr[i] = columns.ColumnName;
                    i++;
                }
                sw.WriteLine(" </tr>");

                foreach (DataRow dr in dt.Rows)
                {
                    sw.WriteLine(" <tr>");
                    foreach (string columnName in columnArr)
                    {
                        sw.WriteLine(" <td style='vnd.ms-excel.numberformat:@'>" + dr[columnName] + "</td>");
                    }
                    sw.WriteLine(" </tr>");
                }
                sw.WriteLine("</table>");
                sw.WriteLine("</body>");
                sw.WriteLine("</html>");
            }
        }

        /// <summary>
        /// 把datagridView保存为excel
        /// </summary>
        /// <param name="m_DataTable">DataGridView绑定的DataTable为参数</param>
        public static bool DataToExcel(DataTable m_DataTable, string FileName)
        {
            try
            {



                //SaveFileDialog kk = new SaveFileDialog();
                //kk.Title = "保存EXECL文件";
                //kk.Filter = "EXECL文件(*.xls) |*.xls |所有文件(*.*) |*.*";
                //kk.FilterIndex = 1;
                //if (kk.ShowDialog() == DialogResult.OK)
                //{
                DateTime time = DateTime.Now;
                FileName = "D:\\" + FileName + "_";
                FileName += time.ToLongDateString() + ".csv";
                if (File.Exists(FileName))
                    File.Delete(FileName);
                //object missing = System.Reflection.Missing.Value;
                //Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.ApplicationClass();
                //app.Application.Workbooks.Add(true);
                //Microsoft.Office.Interop.Excel.Workbook book = (Microsoft.Office.Interop.Excel.Workbook)app.ActiveWorkbook;
                //Microsoft.Office.Interop.Excel.Worksheet sheet = (Microsoft.Office.Interop.Excel.Worksheet)book.ActiveSheet;
                //sheet.Cells[1, 1] = "源数据站点名称记录";
                //sheet.Cells[1, 2] = "匹配总数";
                //for (int i = 0; i < m_DataTable.Columns.Count; i++)
                //{
                //    sheet.Cells[1, i + 1] = m_DataTable.Columns[i].Caption.ToString() + Convert.ToChar(9);
                //}

                ////将DataTable赋值给excel
                //for (int k = 0; k < m_DataTable.Rows.Count; k++)
                //{
                //    sheet.Cells[k + 2, 1] = m_DataTable.Rows[k][0];
                //    sheet.Cells[k + 2, 2] = m_DataTable.Rows[k][1];
                //}
                ////保存excel文件
                //book.SaveCopyAs(FileName);
                ////关闭文件
                //book.Close(false, missing, missing);
                ////退出excel
                //app.Quit();

                FileStream objFileStream;
                StreamWriter objStreamWriter;
                string strLine = "";
                objFileStream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write);
                objStreamWriter = new StreamWriter(objFileStream, System.Text.Encoding.Unicode);
                for (int i = 0; i < m_DataTable.Columns.Count; i++)
                {
                    strLine = strLine + m_DataTable.Columns[i].Caption.ToString() + Convert.ToChar(9);
                }
                objStreamWriter.WriteLine(strLine);
                strLine = "";

                for (int i = 0; i < m_DataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < m_DataTable.Columns.Count; j++)
                    {
                        if (m_DataTable.Rows[i].ItemArray[j] == null)
                            strLine = strLine + " " + Convert.ToChar(9);
                        else
                        {
                            string rowstr = "";
                            rowstr = m_DataTable.Rows[i].ItemArray[j].ToString();
                            if (rowstr.IndexOf("\r\n") > 0)
                                rowstr = rowstr.Replace("\r\n", " ");
                            if (rowstr.IndexOf("\t") > 0)
                                rowstr = rowstr.Replace("\t", " ");
                            strLine = strLine + rowstr + Convert.ToChar(9);
                        }
                    }
                    objStreamWriter.WriteLine(strLine);
                    strLine = "";
                }
                objStreamWriter.Close();
                objFileStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                return false;
            }
            //}
        }
        /// <summary>
        /// 把datagridView保存为excel
        /// </summary>
        /// <param name="m_DataView">DataGridView显示的内容</param>
        public void DataToExcel2(DataGridView m_DataView)
        {
            SaveFileDialog kk = new SaveFileDialog();
            kk.Title = "保存EXECL文件";
            kk.Filter = "EXECL文件(*.xls) |*.xls |所有文件(*.*) |*.*";
            kk.FilterIndex = 1;
            if (kk.ShowDialog() == DialogResult.OK)
            {
                string FileName = kk.FileName;
                if (File.Exists(FileName))
                    File.Delete(FileName);
                FileStream objFileStream;
                StreamWriter objStreamWriter;
                string strLine = "";
                objFileStream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write);
                objStreamWriter = new StreamWriter(objFileStream, System.Text.Encoding.Unicode);
                for (int i = 0; i < m_DataView.Columns.Count; i++)
                {
                    if (m_DataView.Columns[i].Visible == true)
                    {
                        strLine = strLine + m_DataView.Columns[i].HeaderText.ToString() + Convert.ToChar(9);
                    }
                }
                objStreamWriter.WriteLine(strLine);
                strLine = "";

                for (int i = 0; i < m_DataView.Rows.Count; i++)
                {
                    if (m_DataView.Columns[0].Visible == true)
                    {
                        if (m_DataView.Rows[i].Cells[0].Value == null)
                            strLine = strLine + " " + Convert.ToChar(9);
                        else
                            strLine = strLine + m_DataView.Rows[i].Cells[0].Value.ToString() + Convert.ToChar(9);
                    }
                    for (int j = 1; j < m_DataView.Columns.Count; j++)
                    {
                        if (m_DataView.Columns[j].Visible == true)
                        {
                            if (m_DataView.Rows[i].Cells[j].Value == null)
                                strLine = strLine + " " + Convert.ToChar(9);
                            else
                            {
                                string rowstr = "";
                                rowstr = m_DataView.Rows[i].Cells[j].Value.ToString();
                                if (rowstr.IndexOf("\r\n") > 0)
                                    rowstr = rowstr.Replace("\r\n", " ");
                                if (rowstr.IndexOf("\t") > 0)
                                    rowstr = rowstr.Replace("\t", " ");
                                strLine = strLine + rowstr + Convert.ToChar(9);
                            }
                        }
                    }
                    objStreamWriter.WriteLine(strLine);
                    strLine = "";
                }
                objStreamWriter.Close();
                objFileStream.Close();
                MessageBox.Show("保存EXCEL成功");
            }
        }

        #endregion

    }
}
