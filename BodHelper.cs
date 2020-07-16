using BodDetect.UDP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using BodDetect.BodDataManage;
using System.Threading;
using System.Windows;
using System.IO;
using System.Windows.Automation;
using System.Threading.Tasks;

namespace BodDetect
{
    public class BodHelper: IDisposable
    {
        public delegate void RefreshUI(DelegateParam delegateParam);

        public delegate void RefreshStaus(SysStatus sysStatus);

        public delegate void RefreshData(BodData bodata);

        public delegate void RefreshProcessStatus(ProcessType processType);

        public RefreshStaus refreshStaus;

        public RefreshUI refreshProcess;

        public RefreshData refreshData;

        public RefreshProcessStatus refreshProcessStatus;

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

        BodData bodData = new BodData();

        public SerialPortHelp serialPortHelp = new SerialPortHelp();

        public ConfigData configData;

        public ManualResetEvent manualevent = new ManualResetEvent(true);


        private AutoResetEvent resetEvent = new AutoResetEvent(true);

        bool ConnectSeri = false;
        bool IsConnectPlc = false;

        Timer timer;
        private bool disposedValue;

        public BodHelper(string plcIP, int port)
        {
            PLC_IP = plcIP;
            PLCAddress = IPAddress.Parse(plcIP);

            PLC_Port = port;
            PLCEndPoint = new IPEndPoint(PLCAddress, port);

            resetEvent.Reset();
        }

        public void Init() 
        {

            ConnectSeri = serialPortHelp.OpenPort();
            IsConnectPlc = ConnectPlc();
        }

        public bool ConnectPlc()
        {
            try
            {
                finsClient = new FinsClient(PLCEndPoint, PLC_IP);

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
                case PunpCapType.Point2ml:
                    address = PLCConfig.PumpAbsorbAddress_02ml;
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
            manualevent.WaitOne();
            if (pLCParams.Count < 2)
            {
                return false;
            }

            bool success = ValveControl(pLCParams[0].address, pLCParams[0].data.ToArray());
            if (!success)
                return false;
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
            if (!success)
                return false;

            id = pLCParams[0].data[0];
            param = new DelegateParam(id, 0, ProcessState.ShowData, UiType.ProcessBar);
            mainWindow.Dispatcher.Invoke(refreshProcess, param);


            Thread.Sleep(1000);

            success = ValveControl(pLCParams[1].address, pLCParams[1].data.ToArray());
            if (!success)
                return false;
            success = PumpDrain();

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

            manualevent.WaitOne();
            return success;
        }

        public bool ChangePunpValve(PumpValveType pumpValveType)
        {
            bool success = false;
            try
            {
                byte bitAdress = 0X00;
                switch (pumpValveType)
                {
                    case PumpValveType.work:
                        bitAdress = 11;
                        break;
                    case PumpValveType.pre:
                        bitAdress = 12;
                        break;
                    default:
                        break;
                }

                byte[] wValue = { 0X01 };
                success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);
            }
            catch (Exception )
            {

                return false;
            }

            return success;
        }

        public bool PrePumpCtrl(PrePumpWork prePumpWork)
        {
            bool success = false;
            try
            {
                byte bitAdress = 0X00;
                switch (prePumpWork)
                {
                    case PrePumpWork.preAbsorb:
                        bitAdress = 13;
                        break;
                    case PrePumpWork.preDrain:
                        bitAdress = 14;
                        break;
                    default:
                        break;
                }

                byte[] wValue = { 0X01 };
                success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);
            }
            catch (Exception)
            {

                return false;
            }

            return success;
        }

        public bool PreInit()
        {
            //【1.用清水填满储液环】
            //【2.排出储液环5ml清水】
            //【3.将清水,样液,标液，缓冲液管道填满相应溶液】
            try
            {
                //【1.用清水填满储液环】
                int counts = 3;
                byte[] data = { PLCConfig.AirValveBit };

                ValveControl(PLCConfig.Valve2Address, data);
                data[0] = 0X01;

                bool sucess = false;


                //【1.用清水填满储液环】
                for (int i = 0; i < counts; i++)
                {
                    ChangePunpValve(PumpValveType.pre);
                    //ValveControl(PLCConfig.Valve1Address,PLCConfig.PrePumpValveAir, data);
                    sucess = finsClient.WriteBitData(0, 15, data, PLCConfig.Wr);
                    Thread.Sleep(1000);
                    sucess = PrePumpCtrl(PrePumpWork.preAbsorb);
                    Thread.Sleep(6000);
                    sucess = PumpDrain();

                    //byte[] data = { PLCConfig.WaterValveBit };

                    //bodHelper.ValveControl(PLCConfig.Valve2Address, data);
                    //data[0] = 0X01;

                    //bodHelper.ChangePunpValve(PumpValveType.pre);

                    //bodHelper.finsClient.WriteBitData(0, 15, data, PLCConfig.Wr);

                    ////bodHelper.ValveControl(PLCConfig.Valve1Address, PLCConfig.PrePumpValveAir, data);
                    //bodHelper.PrePumpCtrl(PrePumpWork.preAbsorb);

                    //Thread.Sleep(5000);
                    //bodHelper.PumpDrain();

                }
                Thread.Sleep(6000);

                //【2.排出储液环5ml清水，在用预备阀吸取5ml清水】
                data[0] = 0X01;
                sucess = ChangePunpValve(PumpValveType.pre);
                if (!sucess)
                {
                    return false;
                }
                Thread.Sleep(1000);

                finsClient.WriteBitData(1, 0, data, PLCConfig.Wr);
                Thread.Sleep(1000);

                // ValveControl(PLCConfig.Valve1Address, PLCConfig.PrePumpValveAir, data);
                PrePumpCtrl(PrePumpWork.preAbsorb);
                Thread.Sleep(6000);

                PumpDrain();
                Thread.Sleep(6000);

                ChangePunpValve(PumpValveType.pre);
                Thread.Sleep(1000);

                sucess = finsClient.WriteBitData(0, 15, data, PLCConfig.Wr);
                Thread.Sleep(1000);
                sucess = PrePumpCtrl(PrePumpWork.preAbsorb);
                Thread.Sleep(6000);
                PumpDrain();


                Thread.Sleep(10000);

                //【3.将清水,样液,标液，缓冲液管道填满相应溶液】
                data[0] = 0X01;
                //   byte[] valve = { PLCConfig.WaterValveBit, PLCConfig.StandardValveBit, PLCConfig.DepositValveBit, PLCConfig.bufferValveBit };
                List<byte> Valve = new List<byte>();
                Valve.Add(PLCConfig.WaterValveBit);
                Valve.Add(PLCConfig.StandardValveBit);
                Valve.Add(PLCConfig.DepositValveBit);
                Valve.Add(PLCConfig.bufferValveBit);

                byte[] tempdata = { PLCConfig.AirValveBit };
                foreach (byte item in Valve)
                {
                    data[0] = item;
                    for (int i = 0; i < 2; i++)
                    {
                        ValveControl(PLCConfig.Valve2Address, data);
                        PunpAbsorb(PunpCapType.fiveml);
                        Thread.Sleep(6000);
                        tempdata[0] = PLCConfig.AirValveBit;
                        ValveControl(PLCConfig.Valve2Address, tempdata);
                        PumpDrain();
                    }

                    tempdata[0] = PLCConfig.WaterValveBit;
                    ValveControl(PLCConfig.Valve2Address, tempdata);
                    PunpAbsorb(PunpCapType.fiveml);
                    Thread.Sleep(6000);
                    tempdata[0] = PLCConfig.AirValveBit;
                    ValveControl(PLCConfig.Valve2Address, tempdata);
                    PumpDrain();
                }

            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }

        public async Task  StartBodDetect(CancellationToken token)
        {
            //[ 1. 通过泵抽取水样到储水池  ]
            //[ 2. 调用BOD接口使用缓冲液清洗 ]
            //[ 3. 抽取标定液然后进行稀释放入到标液池,抽取清水清洗注射泵]
            //[ 4. 获取COD的值判断是否需要稀释]
            //[ 5. 抽取样液进行稀释后放入样液池]
            //[ 6. 调用BOD使用标液池的标液]
            //[ 7. 调用BOD使用样液池的样液]
            //[ 8. 通过BOD获取最终计算值]
            //[ 9. 通过判断BOD的值排空标液,样液和沉淀池里的水样]
            try
            {

                if (finsClient == null)
                {
                    return;
                }

                if (token.IsCancellationRequested) 
                {
                    return;
                }

                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.init);

                //if (!ConnectSeri)
                //{
                //    MessageBox.Show(" 串口打开失败,请检查串口设置..", "提示", MessageBoxButton.OK);
                //    return;
                //}

                IsSampling = true;

                bool success = false;

                //[1. 通过泵抽取水样到储水池  ]
                if (token.IsCancellationRequested)
                {
                    return;
                }
                success = await FetchWater();
                if (!success)
                {
                    return ;
                }
                //[ 2. 调用BOD接口使用缓冲液清洗 ]

                //[ 3. 抽取标定液然后进行稀释放入到标液池,抽取清水清洗注射泵]
                if (token.IsCancellationRequested)
                {
                    return;
                }
                success = DiluteStandWater();
                if (!success)
                {
                    return ;
                }


                //[ 4. 获取COD的值判断是否需要稀释]
                if (token.IsCancellationRequested)
                {
                    return;
                }
                manualevent.WaitOne();
                byte[] Temp = { PLCConfig.SensorPower };
                success = ValveControl(100, Temp);
                if (!success)
                {
                    return;
                }

                await Task.Delay(configData.WarmUpTime * 1000);
                float[] DoDota = GetDoData();
                uint[] TurbidityData = GetTurbidityData();
                float[] PHData = GetPHData();
                ushort[] CODData = GetCodData();

                bodData.TemperatureData = DoDota[0];
                bodData.DoData = DoDota[1];
                bodData.TurbidityData = (float)TurbidityData[0] / 1000;
                bodData.PHData = PHData[1];
                bodData.CodData = (float)CODData[0] / 100;

                StreamWriter streamWriter = File.CreateText("D:\\test.txt");
                streamWriter.WriteLine(bodData.TemperatureData.ToString());
                streamWriter.WriteLine(bodData.DoData.ToString());
                streamWriter.WriteLine(bodData.TurbidityData.ToString());
                streamWriter.WriteLine(bodData.PHData.ToString());
                streamWriter.WriteLine(bodData.CodData.ToString());
                streamWriter.Close();

                mainWindow.Dispatcher.Invoke(refreshData, bodData);

                //[ 5. 抽取样液进行稀释后放入样液池]
                if (token.IsCancellationRequested)
                {
                    return;
                }
                success = DiluteStandWater();
                if (!success)
                {
                    return;
                }

                //[ 6. 调用BOD使用标液池的标液]
                if (token.IsCancellationRequested)
                {
                    return;
                }
                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.BodStand);
                List<byte> TempData = new List<byte>();
                TempData.Add(PLCConfig.WashValveBit);
                ValveControl(PLCConfig.Valve1Address, TempData.ToArray());

                if (ConnectSeri)
                {
                    serialPortHelp.SetStandDeep(20);
                    serialPortHelp.StartStandMeas();
                    Thread.Sleep(10 * 60 * 1000);
                }

                //[ 7. 调用BOD使用样液池的样液]
                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.BodSample);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                TempData.Add(PLCConfig.SelectValveBit);
                ValveControl(PLCConfig.Valve1Address, TempData.ToArray());

                if (ConnectSeri)
                {
                    serialPortHelp.SetSampleDil((ushort)configData.SampDil);
                    serialPortHelp.StartSampleMes();
                    Thread.Sleep(10 * 60 * 1000);
                }
                //[ 8. 通过BOD获取最终计算值]

                if (ConnectSeri)
                {
                    bodData.Bod = serialPortHelp.BodCurrentData();
                    streamWriter.WriteLine(bodData.Bod.ToString());
                    streamWriter.Close();
                    mainWindow.Dispatcher.Invoke(refreshData, bodData);
                }

                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.DrainEmpty);

                //[ 9. 通过判断BOD的值排空标液,样液和沉淀池里的水样]
                byte[] Valves = { PLCConfig.NormalValveBit, PLCConfig.SampleValveBit };

                //serialPortHelp.SetSampleDil((ushort)configData.SampDil);
                if (ConnectSeri)
                {
                    serialPortHelp.StartWash();
                }

               // resetEvent.WaitOne();
                DrianEmpty(Valves);
                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.Waitding);
            }
            catch (Exception ex)
            {
                StreamWriter streamWriter = File.CreateText("D:\\test.txt");
                string message = ex.Message;
                streamWriter.WriteLine(message);
                streamWriter.WriteLine(ex.StackTrace);
                streamWriter.Close();
            }
            finally
            {
                mainWindow.Dispatcher.Invoke(refreshStaus, SysStatus.Complete);
                IsSampling = false;
            }
        }

        /// <summary>
        /// 取水
        /// </summary>
        /// <returns></returns>
        public async Task<bool> FetchWater() 
        {
            mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.SampleWater);

            WashCistern(configData.WashTimes);

            byte[] data = { PLCConfig.CisternPumpBit };
            bool success = ValveControl(PLCConfig.Valve1Address, data);
            if (!success)
            {
                return false;
            }

            timer = new Timer(new System.Threading.TimerCallback(PrecipitateIsTimeOut), null, configData.InietTime * 1000, configData.InietTime * 1000);
            //  timer.Change(0, configData.InietTime * 1000);

            //沉淀池沉淀
            await Task.Delay(configData.PrecipitateTime * 1000);

            return true;
        }

        /// <summary>
        /// 稀释水样流程
        /// </summary>
        /// <returns></returns>
        public bool DiluteWater()
        {
            List<PLCParam> pLCParams = new List<PLCParam>();

            PLCParam pLCParam1 = new PLCParam
            {
                address = PLCConfig.Valve2Address
            };
            pLCParam1.data.Add(PLCConfig.bufferValveBit);
            pLCParams.Add(pLCParam1);

            PLCParam pLCParam2 = new PLCParam
            {
                address = PLCConfig.Valve2Address
            };
            pLCParam2.data.Add(PLCConfig.NormalValveBit);
            pLCParams.Add(pLCParam2);

            mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.SampleDil);

            int waterVol = configData.SampVol - (configData.SampVol / configData.SampDil);
            int Times1 = configData.SampVol / configData.SampDil / 5;
            int Times2 = configData.SampVol / configData.SampDil % 5;
            pLCParams[0].data[0] = PLCConfig.DepositValveBit;
            pLCParams[1].data[0] = PLCConfig.SampleValveBit;

            bool success = false;

            for (int i = 0; i < Times1; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return false;
                }
            }

            for (int i = 0; i < Times2; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.oneml);
                if (!success)
                {
                    return false;
                }
            }

            pLCParams[0].data[0] = PLCConfig.AirValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
            //[ 固定0.2ml的缓冲液]
            pLCParams[0].data[0] = PLCConfig.bufferValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.Point2ml);

            Times1 = waterVol / 5;
            Times2 = waterVol % 5;
            pLCParams[0].data[0] = PLCConfig.WaterValveBit;

            for (int i = 0; i < Times1; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return false;
                }
            }

            for (int i = 0; i < Times2; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.oneml);
                if (!success)
                {
                    return false;
                }
            }

            pLCParams[0].data[0] = PLCConfig.AirValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            pLCParams[0].data[0] = PLCConfig.WaterValveBit;
            pLCParams[1].data[0] = PLCConfig.AirValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            return success;
        }

        public bool DiluteStandWater() 
        {
            mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.StandDil);
            List<PLCParam> pLCParams = new List<PLCParam>();

            PLCParam pLCParam1 = new PLCParam
            {
                address = PLCConfig.Valve2Address
            };
            pLCParam1.data.Add(PLCConfig.bufferValveBit);
            pLCParams.Add(pLCParam1);

            PLCParam pLCParam2 = new PLCParam
            {
                address = PLCConfig.Valve2Address
            };
            pLCParam2.data.Add(PLCConfig.NormalValveBit);
            pLCParams.Add(pLCParam2);

            bool success = false;

            int waterVol = configData.StandVol - (configData.StandVol / configData.StandDil);
            int Times1 = configData.StandVol / configData.StandDil / 5;
            int Times2 = configData.StandVol / configData.StandDil % 5;

            for (int i = 0; i < Times1; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return false;
                }
            }

            for (int i = 0; i < Times2; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.oneml);
                if (!success)
                {
                    return false;
                }
            }

            pLCParams[0].data[0] = PLCConfig.AirValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            //[ 固定0.2的标定液]
            pLCParams[0].data[0] = PLCConfig.StandardValveBit;
            pLCParams[1].data[0] = PLCConfig.NormalValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.Point2ml);
            if (!success)
            {
                return false;
            }

            Times1 = waterVol / 5;
            Times2 = waterVol % 5;
            pLCParams[0].data[0] = PLCConfig.WaterValveBit;
            pLCParams[1].data[0] = PLCConfig.NormalValveBit;

            for (int i = 0; i < Times1; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return false;
                }
            }

            for (int i = 0; i < Times2; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.oneml);
                if (!success)
                {
                    return false;
                }
            }

            pLCParams[0].data[0] = PLCConfig.AirValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
            if (!success)
            {
                return false;
            }

            pLCParams[0].data[0] = PLCConfig.WaterValveBit;
            pLCParams[1].data[0] = PLCConfig.AirValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            return success;
        }



        public bool CisternIsFull() 
        {
            ushort[] Data = finsClient.ReadData(PLCConfig.Valve1Address, 0, 1, PLCConfig.IO);

            int value = Data[0] & 16;
            if (value == 0) 
            {
                return true;
            }

            return false;
        }

        public bool CisternIsEmpty() 
        {
            ushort[] Data = finsClient.ReadData(PLCConfig.Valve1Address, 0, 1, PLCConfig.IO);

            int value = Data[0] & 1;
            if (value == 0)
            {
                return true;
            }

            return false;
        }

        public bool WashCistern(int Times) 
        {
            bool success = false;
            byte[] data = { PLCConfig.CisternPumpBit };


            for (int i = 0; i < Times; i++)
            {
                success = ValveControl(PLCConfig.Valve1Address, data);                
                if (!success)
                {
                    return false;
                }

                //timer = new Timer(new System.Threading.TimerCallback(CheckCisternIsFull),null, Timeout.Infinite, 1000);
                //timer.Change(0, 1000);
                //resetEvent.WaitOne();
                Thread.Sleep(2 * 60 * 1000);


                byte[] Temdata = { PLCConfig.CisternValveBit };
                ValveControl(PLCConfig.Valve1Address, Temdata);
                Thread.Sleep(1 * 60 * 1000);

                //timer = new Timer(new System.Threading.TimerCallback(CheckCisternIsEmpty), null, Timeout.Infinite, 1000);
                //timer.Change(0, 1000);
                //resetEvent.WaitOne();
            }

            return true;
        }

        public void CheckCisternIsFull(object time) 
        {
            if (CisternIsFull()) 
            {
                timer.Change(Timeout.Infinite, 1000);
                resetEvent.Set();
            }
        }

        public void CheckCisternIsEmpty(object time) 
        {
            if (CisternIsEmpty()) 
            {
                timer.Change(Timeout.Infinite, 1000);
                resetEvent.Set();
            }
        }

        public bool DrianEmpty(byte[] Valves)
        {
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
            pLCParam2.data.Add(PLCConfig.AirValveBit);
            pLCParams.Add(pLCParam2);

            bool success = false;

            foreach (var item in Valves)
            {
                pLCParams[0].data[0] = item;
                for (int i = 0; i < 10; i++)
                {
                    success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

                    if (!success)
                    {
                        return false;
                    }
                }
            }

            byte[] data = { PLCConfig.CisternValveBit };
            ValveControl(PLCConfig.Valve1Address, data);

            return success;
        }

        public void PrecipitateIsTimeOut(object time)
        {           
            if (CisternIsFull())
            {
                timer.Change(Timeout.Infinite, configData.InietTime * 1000);
            }
            else
            {
                timer.Change(Timeout.Infinite, configData.InietTime * 1000);
                byte[] data = { 0 };
                ValveControl(PLCConfig.Valve1Address, data);
                MessageBox.Show(" 进水超时,水泵已关.", "提示", MessageBoxButton.OK);
            }
        }

        public bool ClearSolution()
        {
            bool success = false;
            List<PLCParam> pLCParams = new List<PLCParam>();

            PLCParam pLCParam1 = new PLCParam
            {
                address = PLCConfig.Valve2Address
            };
            pLCParam1.data.Add(PLCConfig.SampleValveBit);
            pLCParams.Add(pLCParam1);

            PLCParam pLCParam2 = new PLCParam
            {
                address = PLCConfig.Valve2Address
            };
            pLCParam2.data.Add(PLCConfig.AirValveBit);
            pLCParams.Add(pLCParam2);

            //抽空样液
            for (int i = 0; i < 10; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return success;
                }
            }

            pLCParams[0].data[0] = PLCConfig.NormalValveBit;

            for (int i = 0; i < 10; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                if (!success)
                {
                    return success;
                }
            }

            pLCParams[0].data[0] = PLCConfig.WaterValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            return success;
        }


        public void ClosePLC() 
        {
            try
            {
                if (finsClient != null)
                {
                    finsClient.Close();
                    finsClient.Dispose();
                }

            }
            catch (Exception)
            {

            }

        }

        public void CloseTimer() 
        {
            try
            {
                //timer.Dispose();
            }
            catch (Exception)
            {

            }

        }

        public void CloseEvent() 
        {
            try
            {
                manualevent.Dispose();
                resetEvent.Dispose();
            }
            catch (Exception)
            {

            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                    if (IsConnectPlc) 
                    {
                        ClosePLC();

                    }

                    CloseTimer();

                    if (serialPortHelp != null)
                    {
                        serialPortHelp.Dispose();
                    }

                    CloseEvent();

                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~BodHelper()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
