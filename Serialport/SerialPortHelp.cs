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
    public  class SerialPortHelp : IDisposable
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
            throw new NotImplementedException();
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
                flag = true;
            }
            catch (Exception)
            {

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
            catch (Exception)
            {

            }

            return flag;
        }

        private void ConfigurePort()
        {
            serialPort.PortName = "COM1";
            serialPort.BaudRate = 9600;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
        }

        public void ReadBodFun(RegStatus regStatus, Int32 iRegCount) 
        {
            List<byte> Message = new List<byte>();

            byte Address = SerialPortConfig.Address;
            byte FunCode = SerialPortConfig.Fun_Red_Code;
            ushort temp = (ushort)regStatus;
            byte RegHighAddress = (byte)(temp >> 8);
            byte RegLowAddress = (byte)temp;
            byte RegCountHighAddress =(byte)(iRegCount >> 8);
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

            SerialPortWrite(Message.ToArray());
        }

        public void StartStandMeas() 
        {
            List<byte> Message = new List<byte>();

            byte Address = SerialPortConfig.Address;
            byte FunCode = SerialPortConfig.Fun_Ctrl_Code;
            byte RegHighAddress = (int)RegCtrl.Start_Stand >> 8;

            ushort temp = (ushort)RegCtrl.Start_Stand;
            byte RegLowAddress = (byte)temp;
            byte DataHighAddress = 0X00;
            byte DataLowAddress = 0x01;

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
        }


        public void StartSampleMes() 
        {
            List<byte> Message = new List<byte>();

            byte Address = SerialPortConfig.Address;
            byte FunCode = SerialPortConfig.Fun_Ctrl_Code;
            byte RegHighAddress = (int)RegCtrl.Start_Sample >> 8;
            ushort temp = (ushort)RegCtrl.Start_Sample;
            byte RegLowAddress = (byte)temp;
            byte DataHighAddress = 0X00;
            byte DataLowAddress = 0x01;

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
        }

        public bool SetSampleDil(ushort Dil) 
        {
           return SetData(Dil, RegCtrl.Sample_Dil);
        }


        public void SetStandDeep(ushort data) 
        {
            SetData(data, RegCtrl.Stand_Deep);
        }

        public void StartWash() 
        {
            List<byte> Message = new List<byte>();

            byte Address = SerialPortConfig.Address;
            byte FunCode = SerialPortConfig.Fun_Ctrl_Code;
            byte RegHighAddress = (int)RegCtrl.Stop_All >> 8;
            ushort temp = (ushort)RegCtrl.Stop_All;
            byte RegLowAddress = (byte)temp;
            byte DataHighAddress = 0X00;
            byte DataLowAddress = 0x01;

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

                return Tool.CompareBytes(revData, Message.ToArray());

            }
            catch (Exception)
            {
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
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public byte[] ReadData()
        {
            int bytesToRead = serialPort.BytesToRead;
            byte[] tempBuffer = new byte[bytesToRead];
            try
            {
                int count = serialPort.Read(tempBuffer, 0, bytesToRead);

                if (count == 0) 
                {
                    return null;
                }

                return tempBuffer;
            }
            catch (Exception)
            {
                return null;
            }

        }


        public float BodCurrentData()
        {
            ReadBodFun(RegStatus.CurrentBod, 3);

            byte[] data = ReadData();

            if (data == null) 
            {
                return 0;
            }

            int iLength = data.Length;

            if (iLength != 11) 
            {
                return 0;
            }

            int iCount = Convert.ToInt32(data[2]);

            byte[] BodValue = { data[3],data[4],data[5],data[6]};
            float Value =  Tool.ToInt32(BodValue)[0] ;

            return Value / 1000;
        }

        public void Dispose()
        {
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