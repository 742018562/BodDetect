using BodDetect.UDP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using BodDetect.BodDataManage;
using System.Threading;

namespace BodDetect
{
    public class BodHelper
    {
        public delegate void RefreshUI(DelegateParam delegateParam);

        public RefreshUI refreshProcess;

        public FinsClient finsClient { get; set; }

        public MainWindow mainWindow { get; set; }

        public string PLC_IP { get; set; }
        public int PLC_Port { get; set; }

        private bool isSampling;

        IPAddress PLCAddress { get; set; }

        IPEndPoint PLCEndPoint { get; set; }
        public bool IsSampling { get => isSampling; set => isSampling = value; }

        Dictionary<byte, int> Valve1Pairs = new Dictionary<byte, int>();
        Dictionary<byte, int> Valve2Pairs = new Dictionary<byte, int>();

        BodData bodData;

        public BodHelper(string plcIP, int port)
        {
            PLC_IP = plcIP;
            PLCAddress = IPAddress.Parse(plcIP);

            PLC_Port = port;
            PLCEndPoint = new IPEndPoint(PLCAddress, port);

        }

        public bool ConnectPlc()
        {
            try
            {
                finsClient = new FinsClient(PLCEndPoint);
            }
            catch (Exception)
            {

                return false;
            }

            return true;
        }


        public uint[] GetTurbidityData()
        {
            if (finsClient == null)
            {
                return null;
            }

            byte bitAdress = 0X00;

            ushort[] value = { 3, 4, 4, 1, 2 };
            finsClient.WriteData(32300, bitAdress, value, PLCConfig.Dr);

            bitAdress = 0X08;
            byte[] wValue = { 0X01 };
            bool success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);

            if (!success)
            {
                return null;
            }

            bitAdress = 0X00;
            uint[] Data = finsClient.ReadIntData(PLCConfig.Turbidity_StartAddress, bitAdress, PLCConfig.Turbidity_Count, PLCConfig.Dr);

            return Data;
        }


        public float[] GetDoData()
        {
            if (finsClient == null)
            {
                return null;
            }
            byte bitAdress = 0X00;

            ushort[] value = { 7, 3, 4, 0, 4 };
            finsClient.WriteData(32300, bitAdress, value, PLCConfig.Dr);

            bitAdress = 0X08;
            byte[] wValue = { 0X01 };
            bool success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);

            if (!success)
            {
                return null;
            }

            bitAdress = 0X00;
            float[] Data = finsClient.ReadDOFloatData(PLCConfig.DO_StartAddress, bitAdress, PLCConfig.DO_Count, PLCConfig.Dr);

            return Data;
        }

        public float[] GetPHData()
        {
            if (finsClient == null)
            {
                return null;
            }
            byte bitAdress = 0X00;

            ushort[] value = { 1, 3, 4, 0, 4 };
            finsClient.WriteData(32300, bitAdress, value, PLCConfig.Dr);

            bitAdress = 0X08;
            byte[] wValue = { 0X01 };
            bool success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);

            if (!success)
            {
                return null;
            }

            bitAdress = 0X00;
            float[] Data = finsClient.ReadDOFloatData(PLCConfig.PH_StartAddress, bitAdress, PLCConfig.PH_Count, PLCConfig.Dr);

            return Data;

        }

        public ushort[] GetCodData()
        {
            if (finsClient == null)
            {
                return null;
            }
            byte bitAdress = 0X00;

            ushort[] value = { 21, 4, 4, 8, 1 };
            finsClient.WriteData(32300, bitAdress, value, PLCConfig.Dr);

            bitAdress = 0X08;
            byte[] wValue = { 0X01 };
            bool success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);

            if (!success)
            {
                return null;
            }

            bitAdress = 0X00;
            ushort[] Data = finsClient.ReadData(PLCConfig.COD_StartAddress, bitAdress, PLCConfig.COD_Count, PLCConfig.Dr);

            return Data;
        }

        /// <summary>
        /// 抽水
        /// </summary>
        /// <param name="bout"></param>
        /// <param name="punpCapType"></param>
        /// <returns></returns>
        public bool PunpAbsorb(PunpCapType punpCapType)
        {
            if (finsClient == null)
            {
                return false;
            }

            ushort address = 0;
            switch (punpCapType)
            {
                case PunpCapType.oneml:
                    address = PLCConfig.PumpAbsorbAddress_1ml;
                    break;
                case PunpCapType.fiveml:
                    address = PLCConfig.PumpAbsorb5mlAddress_5ml;
                    break;
                default:
                    break;
            }

            byte bitAdress = 0X00;
            ushort[] value = { 1 };
            bool success = finsClient.WriteData(address, bitAdress, value, PLCConfig.Dr);


            return success;
        }

        /// <summary>
        /// 注水
        /// </summary>
        /// <returns></returns>
        public bool PumpDrain()
        {
            if (finsClient == null)
            {
                return false;
            }

            byte bitAdress = 0X00;
            ushort[] value = { 1 };

            bool success = finsClient.WriteData(PLCConfig.PumpDrainAddress, bitAdress, value, PLCConfig.Dr);
            if (!success)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 打开单个阀门开关
        /// </summary>
        /// <param name="address"></param>
        /// <param name="bitAddress"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool ValveControl(ushort address, byte bitAddress, byte[] data)
        {
            if (finsClient == null)
            {
                return false;
            }
            bool success = finsClient.WriteBitData(address, bitAddress, data, PLCConfig.IO);

            return success;
        }

        /// <summary>
        /// 多个阀门开关需要打开
        /// </summary>
        /// <param name="address"> IO 控制的地址</param>
        /// <param name="data">需要打开的开关地址</param>
        /// <returns></returns>
        public bool ValveControl(ushort address, byte[] data)
        {

            byte bitAdress = 0X00;
            if (finsClient == null)
            {
                return false;
            }

            ushort[] valve = { 0 };

            valve[0] = MultipleIOCmdValue(data);

            bool success = finsClient.WriteData(address, bitAdress, valve, PLCConfig.IO);

            return success;
        }

        public ushort MultipleIOCmdValue(byte[] data)
        {
            ushort valve = 0;
            ushort temp;
            for (int i = 0; i < data.Length; i++)
            {
                ushort index = Convert.ToUInt16(data[i]);
                temp = Convert.ToUInt16(1 << index);
                valve = Convert.ToUInt16(valve | temp);
            }

            return valve;
        }

        /// <summary>
        /// 注射泵一次来回的流程
        /// </summary>
        /// <param name="pLCParams"></param>
        /// <param name="punpCapType"></param>
        /// <returns></returns>
        public bool PumpOnceProcess(List<PLCParam> pLCParams, PunpCapType punpCapType)
        {
            if (pLCParams.Count < 2)
            {
                return false;
            }

            bool success = ValveControl(pLCParams[0].address, pLCParams[0].data.ToArray());
            //if (!success)
            //    return false;
            success = PunpAbsorb(punpCapType);

            byte id = pLCParams[0].data[0];
            DelegateParam param = new DelegateParam(id, 0, ProcessState.AutoAdd, UiType.ProcessBar);
            refreshProcess(param);

            ushort uid = 100;
            id = Convert.ToByte(uid);
            param = new DelegateParam(id, 0, ProcessState.AutoAdd, UiType.ProcessBar);
            refreshProcess(param);
            //IAsyncResult asyncResult = refreshProcess.BeginInvoke(param, null, null);

            Thread.Sleep(5000);
            //if (!success)
            //    return false;

            id = pLCParams[0].data[0];
            param = new DelegateParam(id, 0, ProcessState.ShowData, UiType.ProcessBar);
            mainWindow.Dispatcher.Invoke(refreshProcess, param);


            Thread.Sleep(1000);

            success = ValveControl(pLCParams[1].address, pLCParams[1].data.ToArray());
            //if (!success)
            //    return false;
            success = PumpDrain();


            //while (!asyncResult.IsCompleted) 
            //{
            //    id = Convert.ToByte(100);
            //    param = new DelegateParam(id, 0, ProcessState.AutoAdd, UiType.ProcessBar);
            //    asyncResult = refreshProcess.BeginInvoke(param, null, null);
            //    break;
            //}
            //id = pLCParams[1].data[0];
            //param = new DelegateParam(id, 0, ProcessState.ShowData, UiType.ProcessBar);
            //mainWindow.Dispatcher.Invoke(refreshProcess, param);
            //asyncResult = refreshProcess.BeginInvoke(param, null, null);

            id = pLCParams[1].data[0];
            param = new DelegateParam(id, 0, ProcessState.AutoAdd, UiType.ProcessBar);
            refreshProcess(param);

            Thread.Sleep(4000);

            id = Convert.ToByte(uid);
            param = new DelegateParam(id, 0, ProcessState.ShowData, UiType.ProcessBar);
            mainWindow.Dispatcher.Invoke(refreshProcess, param);

            Thread.Sleep(1000);

            id = pLCParams[1].data[0];
            param = new DelegateParam(id, 0, ProcessState.ShowData, UiType.ProcessBar);
            mainWindow.Dispatcher.Invoke(refreshProcess, param);

            Thread.Sleep(1000);






            //while (!asyncResult.IsCompleted)
            //{
            //    id = pLCParams[1].data[0];
            //    param = new DelegateParam(id, 0, ProcessState.ShowData, UiType.ProcessBar);
            //    asyncResult = refreshProcess.BeginInvoke(param, null, null);
            //    break;
            //}
            return success;
        }


        public void StartBodDetect()
        {
            //[ 1. 通过泵抽取水样到储水池  ]
            //[ 2. 调用BOD接口使用缓冲液清洗 ]
            //[ 3. 抽取标定液然后进行稀释放入到标液池,抽取清水清洗注射泵]
            //[ 4. 获取COD的值判断是否需要稀释]
            //[ 5. 抽取样液进行稀释后放入样液池]
            //[ 6. 调用BOD使用标液池的标液]

            //[ 7. 调用BOD使用样液池的样液]
            //[ 08. 通过BOD获取最终计算值]


            try
            {
                if (finsClient == null)
                {
                    return;
                }
                IsSampling = true;

                bool success = false;
                //    byte[] data = { PLCConfig.CisternPumpBit };
                //    //[1. 通过泵抽取水样到储水池  ]
                //    success = ValveControl(PLCConfig.Valve1Address, data);
                //    if (!success)
                //    {
                //        return;
                //    }

                //[ 2. 调用BOD接口使用缓冲液清洗 ]

                //[ 3. 抽取标定液然后进行稀释放入到标液池,抽取清水清洗注射泵]
                List<PLCParam> pLCParams = new List<PLCParam>();

                PLCParam pLCParam1 = new PLCParam
                {
                    address = PLCConfig.Valve2Address
                };
                pLCParam1.data.Add(PLCConfig.StandardValveBit);
                pLCParams.Add(pLCParam1);

                PLCParam pLCParam2 = new PLCParam
                {
                    address = PLCConfig.Valve2Address
                };
                pLCParam2.data.Add(PLCConfig.NormalValveBit);
                pLCParams.Add(pLCParam2);

                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return;
                }

                pLCParams[0].data[0] = PLCConfig.WaterValveBit;
                pLCParams[1].data[0] = PLCConfig.AirValveBit;

                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return;
                }

                //[ 固定5ml的缓冲液]
                pLCParams[0].data[0] = PLCConfig.bufferValveBit;
                pLCParams[1].data[0] = PLCConfig.NormalValveBit;
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return;
                }

                pLCParams[0].data[0] = PLCConfig.WaterValveBit;
                pLCParams[1].data[0] = PLCConfig.AirValveBit;
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return;
                }

                //[ 4. 获取COD的值判断是否需要稀释]
                float[] DoDota = GetDoData();
                uint[] TurbidityData = GetTurbidityData();
                float[] PHData = GetPHData();
                ushort[] CODData = GetCodData();

                bodData.TemperatureData = DoDota[0];
                bodData.DoData = DoDota[1];
                bodData.TurbidityData = (float)TurbidityData[0] / 1000;
                bodData.PHData = PHData[1];
                bodData.CodData = (float)CODData[0] / 100;

                //[ 5. 抽取样液进行稀释后放入样液池]
                pLCParams[0].data[0] = PLCConfig.DepositValveBit;
                pLCParams[1].data[0] = PLCConfig.SampleValveBit;
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

                pLCParams[0].data[0] = PLCConfig.WaterValveBit;
                pLCParams[1].data[0] = PLCConfig.AirValveBit;
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return;
                }

                //[ 固定5ml的缓冲液]
                pLCParams[0].data[0] = PLCConfig.bufferValveBit;
                pLCParams[1].data[0] = PLCConfig.SampleValveBit;
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

                pLCParams[0].data[0] = PLCConfig.WaterValveBit;
                pLCParams[1].data[0] = PLCConfig.AirValveBit;
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

                if (!success)
                {
                    return;
                }

                //[ 6. 调用BOD使用标液池的标液]
                List<byte> TempData = new List<byte>();
                TempData.Add(PLCConfig.WashValveBit);
                ValveControl(PLCConfig.Valve1Address, TempData.ToArray());

                //[ 7. 调用BOD使用样液池的样液]
                TempData.Add(PLCConfig.SelectValveBit);
                ValveControl(PLCConfig.Valve1Address, TempData.ToArray());
            }
            catch (Exception ex)
            {
                string message = ex.Message;

            }
            finally 
            {
                IsSampling = false;
            }

        }
    }

}
