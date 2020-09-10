using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BodDetect
{
    public class SerialPortHelp : IDisposable
    {
        /// <summary>
        /// SerialPort对象
        /// </summary>
        private SerialPort serialPort = new SerialPort();

        // 需要一个定时器用来，用来保证即使缓冲区没满也能够及时将数据处理掉，防止一直没有到达
        // 阈值，而导致数据在缓冲区中一直得不到合适的处理。
        private DispatcherTimer checkTimer = new DispatcherTimer();

        //private void InitSerialPort()
        //{
        //    serialPort.DataReceived += SerialPort_DataReceived;
        //    InitCheckTimer();
        //}

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = ReadData();
        }

        public bool OpenPort()
        {
            bool flag = false;
            ConfigurePort();

            try
            {
                serialPort.Open();
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
                //serialPort.DataReceived += SerialPort_DataReceived;
                flag = true;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }

            return flag;
        }

        public bool ClosePort()
        {
            bool flag = false;

            try
            {
                serialPort.Close();
                flag = true;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }

            return flag;
        }

        public bool IsAlive()
        {
            if (serialPort == null)
            {
                return false;
            }

            return serialPort.IsOpen;
        }

        private void ConfigurePort()
        {
            serialPort.PortName = "COM2";
            serialPort.BaudRate = 9600;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
        }

        private void ReadBodFun(RegStatus regStatus, Int32 iRegCount)
        {
            List<byte> Message = new List<byte>();

            byte Address = SerialPortConfig.Address;
            byte FunCode = SerialPortConfig.Fun_Red_Code;
            ushort temp = (ushort)regStatus;
            byte RegHighAddress = (byte)(temp >> 8);
            byte RegLowAddress = (byte)temp;
            byte RegCountHighAddress = (byte)(iRegCount >> 8);
            byte RegCountLowAddress = (byte)iRegCount;

            Message.Add(Address);
            Message.Add(FunCode);
            Message.Add(RegHighAddress);
            Message.Add(RegLowAddress);
            Message.Add(RegCountHighAddress);
            Message.Add(RegCountLowAddress);

            byte[] Crc16 = CRC.CRC16(Message.ToArray());

            Message.Add(Crc16[1]);
            Message.Add(Crc16[0]);
            LogUtil.Log("发送读取BOD命令报文，功能类型：" + regStatus + ",具体发送报文：" + Convert.ToBase64String(Message.ToArray()));

            SerialPortWrite(Message.ToArray());
        }

        private byte[] BodCtrlFun(RegCtrl regCtrl, Int32 Data)
        {
            List<byte> Message = new List<byte>();

            byte Address = SerialPortConfig.Address;
            byte FunCode = SerialPortConfig.Fun_Ctrl_Code;

            ushort temp = (ushort)regCtrl;
            byte RegHighAddress = (byte)(temp >> 8);
            byte RegLowAddress = (byte)temp;

            byte DataHighAddress = (byte)(Data >> 8);
            byte DataLowAddress = (byte)Data;

            Message.Add(Address);
            Message.Add(FunCode);
            Message.Add(RegHighAddress);
            Message.Add(RegLowAddress);
            Message.Add(DataHighAddress);
            Message.Add(DataLowAddress);

            byte[] Crc16 = CRC.CRC16(Message.ToArray());

            Message.Add(Crc16[1]);
            Message.Add(Crc16[0]);

            SerialPortWrite(Message.ToArray());

            LogUtil.Log("发送BOD控制命令报文，功能类型：" + regCtrl + ",具体发送报文：" + Convert.ToBase64String(Message.ToArray()));

            return Message.ToArray();
        }

        public bool StartStandMeas()
        {

            byte[] sendMsg = BodCtrlFun(RegCtrl.Start_Stand, 1);

            byte[] recvMsg = ReadData();

            return Tool.IsSameBytes(sendMsg, recvMsg);

        }

        public bool StartSampleMes()
        {
            byte[] sendMsg = BodCtrlFun(RegCtrl.Start_Sample, 1);
            byte[] recvMsg = ReadData();

            return Tool.IsSameBytes(sendMsg, recvMsg);

        }

        public bool SetSampleDil(ushort Dil)
        {
            byte[] sendMsg = BodCtrlFun(RegCtrl.Sample_Dil, Dil);

            byte[] recvMsg = ReadData();

            return Tool.IsSameBytes(sendMsg, recvMsg);
            //return Tool.CompareBytes(revData, Message.ToArray());
            //return SetData(Dil, RegCtrl.Sample_Dil);
        }

        public bool SetStandDeep(ushort data)
        {

            byte[] sendMsg = BodCtrlFun(RegCtrl.Stand_Deep, data);
            byte[] recvMsg = ReadData();

            return Tool.IsSameBytes(sendMsg, recvMsg);

            //SetData(data, RegCtrl.Stand_Deep);
        }

        public bool StartWash()
        {
            byte[] sendMsg = BodCtrlFun(RegCtrl.Stop_All, 7);

            byte[] recvMsg = ReadData();

            return Tool.IsSameBytes(sendMsg, recvMsg);

        }

        public bool ClearAlram(ushort data)
        {
            byte[] sendMsg = BodCtrlFun(RegCtrl.Clear, data);
            byte[] recvMsg = ReadData();

            return Tool.IsSameBytes(sendMsg, recvMsg);
        }

        public bool SysReset()
        {
            byte[] sendMsg = BodCtrlFun(RegCtrl.SysReset, 1);
            byte[] recvMsg = ReadData();

            return Tool.IsSameBytes(sendMsg, recvMsg);
        }

        public bool SetData(ushort Data, RegCtrl regCtrl)
        {
            try
            {
                List<byte> Message = new List<byte>();
                byte[] RegCtrl = Tool.ToBytes(Convert.ToInt32(regCtrl));

                byte Address = SerialPortConfig.Address;
                byte FunCode = SerialPortConfig.Fun_Ctrl_Code;
                byte RegHighAddress = RegCtrl[0];
                byte RegLowAddress = RegCtrl[1];
                byte DataHighAddress = (byte)(Data >> 8);
                byte DataLowAddress = (byte)Data;

                Message.Add(Address);
                Message.Add(FunCode);
                Message.Add(RegHighAddress);
                Message.Add(RegLowAddress);
                Message.Add(DataHighAddress);
                Message.Add(DataLowAddress);

                byte[] Crc16 = CRC.CRC16(Message.ToArray());

                Message.Add(Crc16[1]);
                Message.Add(Crc16[0]);

                SerialPortWrite(Message.ToArray());

                byte[] revData = ReadData();

                if (revData == null)
                {
                    return false;
                }

                return Tool.IsSameBytes(revData, Message.ToArray());

            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                return false;

            }


        }

        private bool SerialPortWrite(byte[] textData)
        {
            if (serialPort == null)
            {
                return false;
            }

            if (serialPort.IsOpen == false)
            {
                return false;
            }

            try
            {
                serialPort.Write(textData, 0, textData.Length);

            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                return false;
            }

            return true;
        }

        public byte[] ReadData()
        {
            Thread.Sleep(500);
            int bytesToRead = serialPort.BytesToRead;
            byte[] tempBuffer = new byte[bytesToRead];
            try
            {
                int count = serialPort.Read(tempBuffer, 0, bytesToRead);

                if (count == 0)
                {
                    return null;
                }

                LogUtil.Log("串口读取数据：" + Convert.ToBase64String(tempBuffer));
                return tempBuffer;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                return null;
            }

        }

        public float[] BodCurrentData()
        {
            ReadBodFun(RegStatus.CurrentBod, 3);

            byte[] data = ReadData();

            if (data == null)
            {
                return new float[] { 0 };
            }

            int iLength = data.Length;

            if (BodRecvDataBaseCheck(data, SerialPortConfig.Fun_Red_Code) ||
                data[2] != 0x06 ||
                iLength < 11)
            {
                return new float[] { 0 };
            }

            int iCount = Convert.ToInt32(data[2]);

            byte[] BodValue = { data[6], data[5], data[4], data[3] };
            float Value = BitConverter.ToInt32(BodValue,0);

            byte[] tempbe = { data[8], data[7] };
            float ElePotDrop = BitConverter.ToInt16(tempbe,0);

            return new float[] { Value / 100, ElePotDrop / 100 };
        }

        public float[] GetBodStandData()
        {
            ReadBodFun(RegStatus.BodStand, 3);

            byte[] data = ReadData();

            if (data == null)
            {
                return new float[] { -1 };
            }

            int iLength = data.Length;

            if (BodRecvDataBaseCheck(data, SerialPortConfig.Fun_Red_Code) ||
                data[2] != 0x06 ||
                iLength != 11)
            {
                return new float[] { -1 };
            }

            int iCount = Convert.ToInt32(data[2]);

            byte[] BodValue = { data[6], data[5], data[4], data[3] };
            float Value = BitConverter.ToInt32(BodValue,0);

            byte[] tempbe = { data[8], data[7] };
            float ElePotDrop = BitConverter.ToInt16(tempbe,0);

            return new float[] { Value / 100, ElePotDrop / 100 };
        }

        public int GetBodStatus()
        {
            ReadBodFun(RegStatus.Nomale, 1);
            byte[] data = ReadData();

            if (BodRecvDataBaseCheck(data, SerialPortConfig.Fun_Red_Code) ||
                data[2] != 0x02)
            {
                return -1;
            }

            byte[] tempbe = { data[4], data[3] };

            return BitConverter.ToInt16(tempbe,0);
        }

        public int GetSamplingStatus()
        {
            ReadBodFun(RegStatus.Samling, 1);
            byte[] data = ReadData();

            if (BodRecvDataBaseCheck(data, SerialPortConfig.Fun_Red_Code) ||
                data[2] != 0x02)
            {
                return -1;
            }

            byte[] tempbe = { data[4], data[3] };

            return BitConverter.ToInt16(tempbe,0);
        }

        public float GetElePot()
        {
            ReadBodFun(RegStatus.electric, 1);
            byte[] data = ReadData();

            if (BodRecvDataBaseCheck(data, SerialPortConfig.Fun_Red_Code) ||
                data[2] != 0x02)
            {
                return -1;
            }

            byte[] tempbe = { data[4], data[3] };

            float temp = BitConverter.ToInt16(tempbe,0);

            return temp / 100;
        }

        public int GetAlarmStatus()
        {
            ReadBodFun(RegStatus.Dev, 1);
            byte[] data = ReadData();

            if (BodRecvDataBaseCheck(data, SerialPortConfig.Fun_Red_Code) ||
                data[2] != 0x02)
            {
                return -1;
            }

            byte[] tempbe = { data[4], data[3] };

            return BitConverter.ToInt16(tempbe, 0);
        }

        private bool BodRecvDataBaseCheck(byte[] data, byte funCode)
        {
            if (data == null ||
                data.Length < 3 ||
                data[0] != 0x01 ||
                data[1] != funCode)
                //Tool.CheckCRC16(data))
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            ClosePort();
            serialPort.Dispose();
        }

        #region 定时器
        ///// <summary>
        ///// 超时时间为50ms
        ///// </summary>
        //private const int TIMEOUT = 50;
        //private void InitCheckTimer()
        //{
        //    // 如果缓冲区中有数据，并且定时时间达到前依然没有得到处理，将会自动触发处理函数。
        //    checkTimer.Interval = new TimeSpan(0, 0, 0, 0, TIMEOUT);
        //    checkTimer.IsEnabled = false;
        //    checkTimer.Tick += CheckTimer_Tick;
        //}

        //private void StartCheckTimer()
        //{
        //    checkTimer.IsEnabled = true;
        //    checkTimer.Start();
        //}

        //private void StopCheckTimer()
        //{
        //    checkTimer.IsEnabled = false;
        //    checkTimer.Stop();
        //}
        #endregion
    }
}