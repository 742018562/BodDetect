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
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;
using System.Data.Entity.Core.Mapping;

namespace BodDetect
{
    public class BodHelper : IDisposable
    {
        public delegate void RefreshUI(DelegateParam delegateParam);

        public delegate void RefreshStaus(SysStatus sysStatus);

        public delegate void RefreshData(BodData bodata);

        public delegate void RefreshProcessStatus(ProcessType processType);

        public delegate void AddAlramInfo(AlarmData alarmData);

        public RefreshStaus refreshStaus;

        public RefreshUI refreshProcess;

        public RefreshData refreshData;

        public RefreshProcessStatus refreshProcessStatus;

        public AddAlramInfo addAlramInfo;

        public FinsClient finsClient { get; set; }

        public MainWindow mainWindow { get; set; }

        public string PLC_IP { get; set; }
        public int PLC_Port { get; set; }

        private bool isSampling;
        public bool NeedStop = false;

        private static AsyncSemaphore s_asyncSemaphore = new AsyncSemaphore();

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

        object m_lock = new object();

        public bool ConnectSeri = false;
        public bool IsConnectPlc = false;

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
                byte[] data = { PLCConfig.SampleValveBit };

                bool success = ValveControl(PLCConfig.Valve2Address, data);
                return success;

            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                return false;
            }

        }

        public uint[] GetTurbidityData()
        {
            if (finsClient == null)
            {
                return null;
            }

            LogUtil.Log("开始采集浊度数据");

            byte bitAdress = 0X00;

            ushort[] value = { 3, 4, 4, 1, 2 };
            finsClient.WriteData(32300, bitAdress, value, PLCConfig.Dr);
            Thread.Sleep(1000);

            bitAdress = 0X08;
            byte[] wValue = { 0X01 };
            bool success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);

            if (!success)
            {
                return null;
            }

            int i = 0;
            while (i < 5)
            {
                i++;
                Thread.Sleep(10000);

                ushort[] temp = finsClient.ReadData(32352, 0, 1, PLCConfig.Dr);
                if (temp == null || temp.Length < 1 || temp[0] != 0)
                {
                    continue;
                }


                bitAdress = 0X00;
                uint[] Data = finsClient.ReadIntData(PLCConfig.Turbidity_StartAddress, bitAdress, PLCConfig.Turbidity_Count, PLCConfig.Dr);
                if (Data != null && Data.Length > 0)
                {
                    LogUtil.Log("采集浊度成功：" + i + "次" + "数据--" + Data[0]);

                    return Data;
                }
            }
            return new uint[] { 0 };

        }

        public float[] GetDoData()
        {
            if (finsClient == null)
            {
                return new float[] { 0, 0 };
            }

            LogUtil.Log("开始采集DO数据");

            byte bitAdress = 0X00;

            ushort[] value = { 7, 3, 4, 0, 4 };
            finsClient.WriteData(32300, bitAdress, value, PLCConfig.Dr);
            Thread.Sleep(1000);

            bitAdress = 0X08;
            byte[] wValue = { 0X01 };
            bool success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);

            if (!success)
            {
                return new float[] { 0, 0 };
            }

            int i = 0;
            while (i < 5)
            {
                i++;
                Thread.Sleep(10000);

                ushort[] temp = finsClient.ReadData(32352, 0, 1, PLCConfig.Dr);
                if (temp == null || temp.Length < 1 || temp[0] != 0)
                {
                    continue;
                }

                bitAdress = 0X00;
                float[] Data = finsClient.ReadDOFloatData(PLCConfig.DO_StartAddress, bitAdress, PLCConfig.DO_Count, PLCConfig.Dr);
                if (Data != null && Data.Length > 0)
                {
                    LogUtil.Log("采集DO成功：" +i+"次"+ "数据--"+ Data[0]);

                    return Data;
                }
            }

            return new float[] { 0, 0 };
        }

        public float[] GetPHData()
        {
            if (finsClient == null)
            {
                return new float[] { 0, 0 };
            }

            LogUtil.Log("开始采集PH数据");


            byte bitAdress = 0X00;

            ushort[] value = { 6, 3, 4, 0, 4 };
            finsClient.WriteData(32300, bitAdress, value, PLCConfig.Dr);
            Thread.Sleep(1000);

            bitAdress = 0X08;
            byte[] wValue = { 0X01 };
            bool success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);

            if (!success)
            {
                return new float[] { 0, 0 };
            }

            int i = 0;
            while (i < 5)
            {
                i++;
                Thread.Sleep(10000);

                ushort[] temp = finsClient.ReadData(32352, 0, 1, PLCConfig.Dr);
                if (temp == null || temp.Length < 1 || temp[0] != 0)
                {
                    continue;
                }
                bitAdress = 0X00;
                float[] Data = finsClient.ReadDOFloatData(PLCConfig.PH_StartAddress, bitAdress, PLCConfig.PH_Count, PLCConfig.Dr);
                if (Data != null && Data.Length > 0)
                {
                    LogUtil.Log("采集PH成功：" + i + "次" + "数据--" + Data[0]);

                    return Data;
                }
            }
            return new float[] { 0, 0 };

        }

        public ushort[] GetUv254Data()
        {
            if (finsClient == null)
            {
                return new ushort[] { 0, 0 };
            }

            LogUtil.Log("开始采集UV254数据");

            int i = 0;
            ushort[] Data = { 0, 0 };

            i++;
            byte bitAdress = 0X00;

            ushort[] value = { 21, 4, 4, 8, 1 };
            finsClient.WriteData(32300, bitAdress, value, PLCConfig.Dr);
            Thread.Sleep(1000);

            bitAdress = 0X08;
            byte[] wValue = { 0X01 };
            bool success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);

            if (!success)
            {
                return new ushort[] { 0, 0 };
            }

            while (i < 5)
            {
                Thread.Sleep(8000);
                ushort[] temp = finsClient.ReadData(32352, 0, 1, PLCConfig.Dr);
                if (temp == null || temp.Length < 1 || temp[0] != 0)
                {
                    continue;
                }
                bitAdress = 0X00;
                ushort[] Data1 = finsClient.ReadData(PLCConfig.COD_StartAddress, bitAdress, PLCConfig.COD_Count, PLCConfig.Dr);
                if (Data1 != null && Data1.Length > 0)
                {
                    LogUtil.Log("采集UV24成功：" + i + "次" + "数据--" + Data[0]);

                    return Data1;
                }
            }
            return new ushort[] { 0, 0 };

        }

        public ushort[] GetTempAndHumData()
        {
            if (finsClient == null)
            {
                return new ushort[] { 0, 0 };
            }
            byte bitAdress = 0X00;

            ushort[] value = { 13, 3, 4, 0, 2 };
            finsClient.WriteData(32300, bitAdress, value, PLCConfig.Dr);
            Thread.Sleep(1000);

            bitAdress = 0X08;
            byte[] wValue = { 0X01 };
            bool success = finsClient.WriteBitData(0, bitAdress, wValue, PLCConfig.Wr);

            if (!success)
            {
                return new ushort[] { 0, 0 };
            }

            int i = 0;
            while (i < 5)
            {
                i++;
                Thread.Sleep(10000);

                ushort[] temp = finsClient.ReadData(32352, 0, 1, PLCConfig.Dr);
                if (temp == null || temp.Length < 1 || temp[0] != 0)
                {
                    continue;
                }
                bitAdress = 0X00;
                ushort[] Data = finsClient.ReadData(PLCConfig.Data_TempAndHun, bitAdress, PLCConfig.TempAndHun_Count, PLCConfig.Dr);
                if (Data != null && Data.Length > 0)
                {
                    return Data;
                }
            }
            return new ushort[] { 0, 0 };
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
            //manualevent.WaitOne();
            if (pLCParams.Count < 2)
            {
                return false;
            }

            bool success = ValveControl(pLCParams[0].address, pLCParams[0].data.ToArray());
            if (!success)
                return false;

            Thread.Sleep(1000);

            success = PunpAbsorb(punpCapType);

            byte id = pLCParams[0].data[0];
            DelegateParam param = new DelegateParam(id, 0, ProcessState.AutoAdd, UiType.ProcessBar);
            refreshProcess(param);

            ushort uid = 100;
            id = Convert.ToByte(uid);
            param = new DelegateParam(id, 0, ProcessState.AutoAdd, UiType.ProcessBar);
            refreshProcess(param);
            //IAsyncResult asyncResult = refreshProcess.BeginInvoke(param, null, null);

            Thread.Sleep(10000);
            if (!success)
                return false;

            id = pLCParams[0].data[0];
            param = new DelegateParam(id, 0, ProcessState.ShowData, UiType.ProcessBar);
            mainWindow.Dispatcher.Invoke(refreshProcess, param);


            Thread.Sleep(1000);

            success = ValveControl(pLCParams[1].address, pLCParams[1].data.ToArray());
            if (!success)
                return false;
            Thread.Sleep(1000);

            success = PumpDrain();

            id = pLCParams[1].data[0];
            param = new DelegateParam(id, 0, ProcessState.AutoAdd, UiType.ProcessBar);
            refreshProcess(param);

            Thread.Sleep(10000);

            id = Convert.ToByte(uid);
            param = new DelegateParam(id, 0, ProcessState.ShowData, UiType.ProcessBar);
            mainWindow.Dispatcher.Invoke(refreshProcess, param);

            Thread.Sleep(1000);

            id = pLCParams[1].data[0];
            param = new DelegateParam(id, 0, ProcessState.ShowData, UiType.ProcessBar);
            mainWindow.Dispatcher.Invoke(refreshProcess, param);

            Thread.Sleep(1000);

            // manualevent.WaitOne();
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
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

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
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                return false;
            }

            return success;
        }

        public async Task<bool> PreInitAsync()
        {
            //【1.用空气排空储液环】
            //【2.用备用阀吸取储液环10ml清水】
            //【3.将清水,样液,标液，缓冲液管道填满相应溶液】
            using (await s_asyncSemaphore.WaitAsync())
            {

                try
                {
                    mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.init);

                    //【1.用清水填满储液环】
                    int counts = 4;
                    byte[] data = { PLCConfig.AirValveBit };

                    ValveControl(PLCConfig.Valve2Address, data);
                    data[0] = 0X01;

                    bool sucess = false;


                    //【1.用空气排空储液环】
                    for (int i = 0; i < counts; i++)
                    {
                        ChangePunpValve(PumpValveType.pre);
                        //ValveControl(PLCConfig.Valve1Address,PLCConfig.PrePumpValveAir, data);
                        sucess = finsClient.WriteBitData(1, 0, data, PLCConfig.Wr);
                        Thread.Sleep(2000);
                        sucess = PrePumpCtrl(PrePumpWork.preAbsorb);
                        Thread.Sleep(10000);
                        sucess = PumpDrain();
                        Thread.Sleep(10000);

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

                    data[0] = PLCConfig.AirValveBit;

                    ValveControl(PLCConfig.Valve2Address, data);
                    //【2.排出储液环5ml清水，在用预备阀吸取5ml清水】
                    for (int i = 0; i < 2; i++)
                    {
                        data[0] = 0X01;
                        sucess = ChangePunpValve(PumpValveType.pre);
                        if (!sucess)
                        {
                            return false;
                        }
                        Thread.Sleep(2000);

                        finsClient.WriteBitData(0, 15, data, PLCConfig.Wr);
                        Thread.Sleep(2000);

                        // ValveControl(PLCConfig.Valve1Address, PLCConfig.PrePumpValveAir, data);
                        PrePumpCtrl(PrePumpWork.preAbsorb);
                        Thread.Sleep(10000);

                        PumpDrain();
                        Thread.Sleep(10000);
                    }

                    ////【3.将清水,样液,标液，缓冲液管道填满相应溶液】
                    //data[0] = 0X01;
                    ////   byte[] valve = { PLCConfig.WaterValveBit, PLCConfig.StandardValveBit, PLCConfig.DepositValveBit, PLCConfig.bufferValveBit };
                    //List<byte> Valve = new List<byte>();
                    //Valve.Add(PLCConfig.WaterValveBit);
                    //Valve.Add(PLCConfig.StandardValveBit);
                    //Valve.Add(PLCConfig.DepositValveBit);
                    //Valve.Add(PLCConfig.bufferValveBit);

                    //byte[] tempdata = { PLCConfig.AirValveBit };
                    //foreach (byte item in Valve)
                    //{
                    //    data[0] = item;
                    //    for (int i = 0; i < 2; i++)
                    //    {
                    //        ValveControl(PLCConfig.Valve2Address, data);
                    //        PunpAbsorb(PunpCapType.fiveml);
                    //        Thread.Sleep(6000);
                    //        tempdata[0] = PLCConfig.AirValveBit;
                    //        ValveControl(PLCConfig.Valve2Address, tempdata);
                    //        PumpDrain();
                    //    }

                    //    tempdata[0] = PLCConfig.WaterValveBit;
                    //    ValveControl(PLCConfig.Valve2Address, tempdata);
                    //    PunpAbsorb(PunpCapType.fiveml);
                    //    Thread.Sleep(6000);
                    //    tempdata[0] = PLCConfig.AirValveBit;
                    //    ValveControl(PLCConfig.Valve2Address, tempdata);
                    //    PumpDrain();
                    //}

                    byte[] Valves = { PLCConfig.NormalValveBit, PLCConfig.SampleValveBit };
                    DrianEmpty(Valves);
                    mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.Waitding);

                }
                catch (Exception ex)
                {
                    LogUtil.LogError(ex , "初始化线程PreInitAsync");

                    return false;
                }
                return true;
            }
        }

        public async Task StartBodDetect(CancellationToken token)
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

            //using (await s_asyncSemaphore.WaitAsync())
            //{
            try
            {

                if (finsClient == null)
                {
                    return;
                }
                NeedStop = false;
                if (token.IsCancellationRequested)
                {
                    return;
                }
                LogUtil.Log("开始测量BOD");

                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.init);

                int status = serialPortHelp.GetBodStatus();

                if (status == 2 || status == 3)
                {
                    byte[] tempvalue = { PLCConfig.BodDrainValveBit };
                    ValveControl(PLCConfig.Valve1Address, tempvalue);

                    serialPortHelp.StartWash();
                    await Task.Delay(28 * 60 * 1000);

                    tempvalue[0] = 0;
                    ValveControl(PLCConfig.Valve1Address, tempvalue);
                }
                //[ BOD的清洗液进行循环]
                byte[] tempvalue1 = { 0 };
                ValveControl(PLCConfig.Valve1Address, tempvalue1);

                //[ 清除做样完成标志]
                serialPortHelp.ClearAlram(2);

                IsSampling = true;

                bool success = false;

                //[1. 通过泵抽取水样到储水池  ]
                if (token.IsCancellationRequested)
                {
                    return;
                }

                DateTime dateTime = DateTime.Now;
                bodData.CreateTime = dateTime.ToLongTimeString();
                bodData.CreateDate = dateTime.ToLongDateString();

                success = await FetchWater();
                if (!success)
                {
                    return;
                }

                byte[] SensorPower = { PLCConfig.SensorPower };
                success = ValveControl(100, SensorPower);
                if (!success)
                {
                    return;
                }

                //[ 2.采集传感器参数]
                await Task.Delay(configData.WarmUpTime * 1000);
                for (int i = 0; i < configData.SampleScale - 1; i++)
                {
                    byte[] data1 = { PLCConfig.CisternPumpBit };
                    success = ValveControl(PLCConfig.Valve1Address, data1);
                    if (!success)
                    {
                        return;
                    }

                    await Task.Delay(2 * 60 * 1000);
                    ReadSensorData();
                    await mainWindow.Dispatcher.BeginInvoke(refreshData, bodData);


                    byte[] Temdata = { PLCConfig.CisternValveBit };
                    ValveControl(PLCConfig.Valve1Address, Temdata);
                    await Task.Delay(1 * 60 * 1000);
                }

                ReadSensorData();


                //[ 启动COD开始采样]
                StartCod();
                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.CodCollection);

                Task<float> CodDataResult = await Task.Factory.StartNew(() => ReadCodData());

                //[ 3. 抽取标定液然后进行稀释放入到标液池,抽取清水清洗注射泵]
                //success = DiluteStandWater();
                //if (!success)
                //{
                //    return;
                //}
                //mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.BodStand);


                if (token.IsCancellationRequested)
                {
                    return;
                }
                List<byte> TempData = new List<byte>();
                TempData.Add(PLCConfig.WashValveBit);
                TempData.Add(PLCConfig.BodDrainValveBit);
#if DEBUG

#else
                    ValveControl(PLCConfig.Valve1Address, TempData.ToArray());
                    if (ConnectSeri)
                    {
                        serialPortHelp.SetStandDeep(20);
                        serialPortHelp.StartStandMeas();
                        await Task.Delay(8 * 60 * 1000);
                        byte[] tempvalue = { PLCConfig.BodDrainValveBit };
                        ValveControl(PLCConfig.Valve1Address, tempvalue);
                        await Task.Delay(28 * 60 * 1000);
                        byte[] temp = { 0 };
                        ValveControl(PLCConfig.Valve1Address, temp);
                    }
#endif


                //[ 4. 获取COD的值判断是否需要稀释]
                if (token.IsCancellationRequested)
                {
                    return;
                }
                //manualevent.WaitOne();
                Task.WaitAll(CodDataResult);
                bodData.CodData = CodDataResult.Result;
                if (bodData.CodData == -1)
                {
                    return;
                }

                LogUtil.Log("COD采集数据：" + CodDataResult.Result.ToString());

                int SampDil = GetSampleDilByCod(bodData.CodData);

                //[ 5. 抽取样液进行稀释后放入样液池]
                if (token.IsCancellationRequested)
                {
                    return;
                }
                success = DiluteWater(SampDil);
                if (!success)
                {
                    LogUtil.Log("样品稀释失败,稀释倍数为:"+ SampDil);

                    return;
                }

                //[ 6. 调用BOD使用标液池的标液]
                if (token.IsCancellationRequested)
                {
                    return;
                }

                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.BodSample);

                if (ConnectSeri)
                {
                    bodData.BodElePot = serialPortHelp.GetElePot();
                    for (int i = 0; i < 6; i++)
                    {
                        if (i == 5)
                        {
                            DisPlayAlarmInfo(1, 18, "BOD 启动测量失败.");

                            return;
                        }

                        success = serialPortHelp.SetSampleDil((ushort)SampDil);
                        if (!success)
                        {
                            continue;
                        }
                        success = serialPortHelp.StartSampleMes();
                        if (success)
                        {
                            TempData.Add(PLCConfig.SelectValveBit);
                            ValveControl(PLCConfig.Valve1Address, TempData.ToArray());

                            break;
                        }
                    }

                    await Task.Delay(8 * 60 * 1000);

                    //清洗
                    byte[] tempvalue = { PLCConfig.BodDrainValveBit };
                    ValveControl(PLCConfig.Valve1Address, tempvalue);
                }

                //[ 8. 通过BOD获取最终计算值]
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (ConnectSeri)
                {
                    float[] recvData = { 0, 0 };
                    int index = 0;

                    while (index < 10)
                    {
                        await Task.Delay(2 * 60 * 1000);
                        index++;

                        int bod_status = serialPortHelp.GetSamplingStatus();
                        if (bod_status != 0)
                        {
                            int i = 0;
                            while (i < 5)
                            {
                                i++;
                                recvData = serialPortHelp.BodCurrentData();

                                if (recvData != null && recvData.Length > 1 && recvData[0] != 0)
                                {
                                    await mainWindow.Dispatcher.BeginInvoke(refreshProcessStatus, ProcessType.BodSampleComplete);
                                    bodData.Bod = recvData[0];
                                    bodData.BodElePotDrop = recvData[1];
                                    if (bodData.Bod < 0 || bodData.Bod > 50)
                                    {
                                        DisPlayAlarmInfo(1, 14, "BOD测量值超出量程");
                                        bodData.CodData = CodDataResult.Result;
                                        await mainWindow.Dispatcher.BeginInvoke(refreshData, bodData);
                                        return;
                                    }

                                    await mainWindow.Dispatcher.BeginInvoke(refreshProcessStatus, ProcessType.BodWash);

                                    await Task.Delay(28 * 60 * 1000);

                                    LogUtil.Log("BOD值：" + bodData.Bod.ToString() + ", BOD电位差：" + bodData.BodElePotDrop.ToString());

                                    break;
                                }
                                else
                                {
                                    bodData.Bod = 0;
                                    bodData.BodElePotDrop = 0;
                                }

                                await Task.Delay(60 * 1000);
                            }

                            if (bodData.Bod == 0 && bodData.BodElePotDrop == 0) 
                            {
                                DisPlayAlarmInfo(1, 15, "BOD取值异常");
                            }
                            break;
                        }
                    }

                    //[ BOD的清洗液进行循环]
                    byte[] tempvalue = { 0 };
                    ValveControl(PLCConfig.Valve1Address, tempvalue);
                }

                bodData.CodData = CodDataResult.Result;
                await mainWindow.Dispatcher.BeginInvoke(refreshData, bodData);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "测量BOD流程异常");
            }
            finally
            {
                //[ 9. 通过判断BOD的值排空标液,样液和沉淀池里的水样]
                byte[] Valves = { PLCConfig.NormalValveBit, PLCConfig.SampleValveBit };
                DrianEmpty(Valves);
                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.Waitding);

                byte[] tempvalue = { 0 };
                ValveControl(PLCConfig.Valve1Address, tempvalue);
                IsSampling = false;
                mainWindow.Dispatcher.Invoke(refreshStaus, SysStatus.Complete);
            }
            //}

        }

        /// <summary>
        /// 取水
        /// </summary>
        /// <returns></returns>
        public async Task<bool> FetchWater()
        {
            try
            {
                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.SampleWater);

                if (!WashCistern(configData.WashTimes))
                {
                    return false;
                }

                byte[] data = { PLCConfig.CisternPumpBit };
                bool success = ValveControl(PLCConfig.Valve1Address, data);
                if (!success)
                {
                    return false;
                }

                timer = new Timer(new System.Threading.TimerCallback(PrecipitateIsTimeOut), null, configData.InietTime * 1000, configData.InietTime * 1000);
                //  timer.Change(0, configData.InietTime * 1000);

                Thread.Sleep(1 * 60 * 1000);

                //沉淀池沉淀
                await Task.Delay(configData.PrecipitateTime * 1000);

                return true;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "采水流程");
                return false;
            }

        }

        /// <summary>
        /// 标定流程
        /// </summary>
        /// <returns></returns>
        public async void StartBodStandWater()
        {
            //using (await s_asyncSemaphore.WaitAsync())
            //{
            try
            {
                while (true)
                {
                    if (!isSampling)
                    {
                        break;
                    }

                    await Task.Delay(60 * 1000);
                }

                //[ 清除做标完成标志]
                serialPortHelp.ClearAlram(4);

                bool Success = DiluteStandWater();
                if (!Success)
                {
                    return;
                }

                if (ConnectSeri)
                {
                    mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.BodStand);
                    serialPortHelp.SetStandDeep(20);
                    List<byte> TempData = new List<byte>();
                    TempData.Add(PLCConfig.WashValveBit);
                    TempData.Add(PLCConfig.BodDrainValveBit);
                    ValveControl(PLCConfig.Valve1Address, TempData.ToArray());

                    serialPortHelp.StartStandMeas();
                    await Task.Delay(8 * 60 * 1000);
                    byte[] tempvalue = { PLCConfig.BodDrainValveBit };
                    ValveControl(PLCConfig.Valve1Address, tempvalue);
                    mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.BodWash);

                    await Task.Delay(28 * 60 * 1000);
                    byte[] temp = { 0 };
                    ValveControl(PLCConfig.Valve1Address, temp);

                    mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.DrainEmpty);

                    List<PLCParam> pLCParams = new List<PLCParam>();

                    bool success = false;
                    PLCParam pLCParam1 = new PLCParam
                    {
                        address = PLCConfig.Valve2Address
                    };
                    pLCParam1.data.Add(PLCConfig.NormalValveBit);
                    pLCParams.Add(pLCParam1);

                    PLCParam pLCParam2 = new PLCParam
                    {
                        address = PLCConfig.Valve2Address
                    };
                    pLCParam2.data.Add(PLCConfig.AirValveBit);
                    pLCParams.Add(pLCParam2);
                    int index = 0;
                    while (index < 9)
                    {
                        success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                        index++;
                    }

                    mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.Waitding);

                    XmlHelp.updatexml(XmlHelp.Xmlpath);

                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "标定BOD流程");
            }
            //}
        }

        public async void StartBodSample()
        {
            using (await s_asyncSemaphore.WaitAsync())
            {
                try
                {
                    bool Success = DiluteWater(1);
                    if (!Success)
                    {
                        return;
                    }

                    if (ConnectSeri)
                    {
                        mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.BodSample);

                        List<byte> TempData = new List<byte>();
                        TempData.Add(PLCConfig.WashValveBit);
                        TempData.Add(PLCConfig.BodDrainValveBit);
                        TempData.Add(PLCConfig.SelectValveBit);
                        ValveControl(PLCConfig.Valve1Address, TempData.ToArray());

                        serialPortHelp.SetSampleDil(1);
                        bodData.BodElePot = serialPortHelp.GetElePot();
                        serialPortHelp.StartSampleMes();
                        await Task.Delay(8 * 60 * 1000);

                        //清洗
                        byte[] tempvalue = { PLCConfig.BodDrainValveBit };
                        ValveControl(PLCConfig.Valve1Address, tempvalue);

                        await Task.Delay(60 * 1000);

                        float[] recvData = { 0, 0 };

                        int index = 0;
                        while (index < 5)
                        {
                            recvData = serialPortHelp.BodCurrentData();

                            if (recvData != null && recvData.Length > 1 && recvData[0] < 65535)
                            {
                                break;
                            }

                            await Task.Delay(20 * 1000);
                            index++;
                        }

                        bodData.Bod = recvData[0];
                        bodData.BodElePotDrop = recvData[1];

                        if (bodData.Bod < 0 || bodData.Bod > 50)
                        {
                            DisPlayAlarmInfo(1, 14, "COD测量值超出量程");
                            return;
                        }
                        StreamWriter streamWriter = File.CreateText("D:\\test.txt");
                        streamWriter.WriteLine(bodData.Bod.ToString());
                        streamWriter.Close();

                        await mainWindow.Dispatcher.BeginInvoke(refreshData, bodData);
                        await Task.Delay(28 * 60 * 1000);
                        byte[] temp = { 0 };
                        ValveControl(PLCConfig.Valve1Address, temp);
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.LogError(ex, "单独手动测量BOD流程");
                }

                return;
            }


        }


        /// <summary>
        /// 稀释水样流程
        /// </summary>
        /// <returns></returns>
        public bool DiluteWater(int dil)
        {
            List<PLCParam> pLCParams = new List<PLCParam>();
            bool success = false;

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
            pLCParam2.data.Add(PLCConfig.SampleValveBit);
            pLCParams.Add(pLCParam2);

            mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.SampleDil);

            //预备操作
            pLCParams[0].data[0] = PLCConfig.DepositValveBit;
            pLCParams[1].data[0] = PLCConfig.AirValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            pLCParams[0].data[0] = PLCConfig.bufferValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            pLCParams[0].data[0] = PLCConfig.StandardValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            pLCParams[0].data[0] = PLCConfig.WaterValveBit;
            for (int i = 0; i < 2; i++)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
            }

            //预备排空一次管内残留液
            pLCParams[0].data[0] = PLCConfig.SampleValveBit;
            pLCParams[1].data[0] = PLCConfig.AirValveBit;
            int TempTimes = 2;
            while (TempTimes > 0)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                TempTimes--;
            }

            if (dil < 1)
            {
                dil = 1;
            }

            int waterVol = configData.SampVol - (configData.SampVol / dil);
            int Times1 = configData.SampVol / dil / 5;
            int Times2 = configData.SampVol / dil % 5;
            pLCParams[0].data[0] = PLCConfig.DepositValveBit;
            pLCParams[1].data[0] = PLCConfig.SampleValveBit;


            for (int i = 0; i < Times1 - 1; i++)
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


            pLCParams[0].data[0] = PLCConfig.bufferValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            Times1 = waterVol / 5;
            Times2 = waterVol % 5;
            pLCParams[0].data[0] = PLCConfig.WaterValveBit;

            for (int i = 0; i < Times1 - 1; i++)
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

            bool success = false;
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
            int Times1 = 0;
            int Times2 = 0;

            //预备排空一次管内残留液
            pLCParams[0].data[0] = PLCConfig.NormalValveBit;
            pLCParams[1].data[0] = PLCConfig.AirValveBit;
            int TempTimes = 2;
            while (TempTimes > 0)
            {
                success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
                TempTimes--;
            }

            pLCParams[0].data[0] = PLCConfig.StandardValveBit;
            pLCParams[1].data[0] = PLCConfig.AirValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
            if (!success)
            {
                return false;
            }

            pLCParams[0].data[0] = PLCConfig.WaterValveBit;
            pLCParams[1].data[0] = PLCConfig.AirValveBit;

            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            if (!success)
            {
                return false;
            }

            //固定5ml缓冲溶液
            pLCParams[0].data[0] = PLCConfig.bufferValveBit;
            pLCParams[1].data[0] = PLCConfig.NormalValveBit;
            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);
            if (!success)
            {
                return false;
            }


            //pLCParams[0].data[0] = PLCConfig.AirValveBit;
            //success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

            //[ 固定0.2的标定液]
            pLCParams[0].data[0] = PLCConfig.StandardValveBit;
            pLCParams[1].data[0] = PLCConfig.NormalValveBit;
            //success = PumpOnceProcess(pLCParams, PunpCapType.Point2ml);
            success = ValveControl(pLCParams[0].address, pLCParams[0].data.ToArray());
            if (!success)
                return false;

            Thread.Sleep(1000);

            success = PunpAbsorb(PunpCapType.Point2ml);
            Thread.Sleep(5000);

            pLCParams[0].data[0] = PLCConfig.WaterValveBit;
            pLCParams[1].data[0] = PLCConfig.NormalValveBit;

            success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);


            if (!success)
            {
                return false;
            }

            //int waterVol = configData.StandVol - (configData.StandVol / configData.StandDil);

            //Times1 = waterVol / 5;
            //Times2 = waterVol % 5;
            pLCParams[0].data[0] = PLCConfig.WaterValveBit;
            pLCParams[1].data[0] = PLCConfig.NormalValveBit;

            Times1 = 8;

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

            //pLCParams[0].data[0] = PLCConfig.WaterValveBit;
            //pLCParams[1].data[0] = PLCConfig.AirValveBit;
            //success = PumpOnceProcess(pLCParams, PunpCapType.fiveml);

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
                Thread.Sleep(configData.InietTime * 1000);

                if (!CisternIsFull())
                {
                    byte[] temp = { 0 };
                    ValveControl(PLCConfig.Valve1Address, temp);
                    DisPlayAlarmInfo(7, 10, "抽水泵进水超时");
                    return false;
                }

                byte[] Temdata = { PLCConfig.CisternValveBit };
                ValveControl(PLCConfig.Valve1Address, Temdata);
                Thread.Sleep(configData.EmptyTime * 100);

                //ushort[] value = finsClient.ReadData(PLCConfig.Valve1Address, 0, 1, PLCConfig.IO);
                //if (value == null || value.Length < 1 || (value[0] & 1) != 0)
                //{
                //    DisPlayAlarmInfo(7, 11, "抽水泵排水超时");
                //}

                //timer = new Timer(new System.Threading.TimerCallback(CheckCisternIsEmpty), null, Timeout.Infinite, 1000);
                //timer.Change(0, 1000);
                //resetEvent.WaitOne();
            }

            return true;
        }

        public void ReadSensorData()
        {
            try
            {
                mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.Sensor);

                float[] DoDota = GetDoData();
                Thread.Sleep(5 * 1000);

                uint[] TurbidityData = GetTurbidityData();
                Thread.Sleep(5 * 1000);

                float[] PHData = GetPHData();
                Thread.Sleep(5 * 1000);

                ushort[] Uv254Data = GetUv254Data();
                Thread.Sleep(5 * 1000);

                ushort[] TempAndHumData = GetTempAndHumData();
                Thread.Sleep(5 * 1000);

                bodData.TemperatureData = DoDota[0];
                bodData.DoData = DoDota[1];
                bodData.TurbidityData = (float)TurbidityData[0] / 1000;
                bodData.PHData = PHData[1];
                bodData.Uv254Data = (float)Uv254Data[0] / 100;
                bodData.HumidityData = TempAndHumData[0] / 10;
                bodData.AirTemperatureData = TempAndHumData[1] / 10;

                string msg = "传感器数据:" + "DoDota = " + bodData.DoData + ",TurbidityData = " + bodData.TurbidityData + ",PHData = " + bodData.PHData + ",Uv254Data = " + bodData.Uv254Data;

                LogUtil.Log(msg);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }

        }

        public void StartCod()
        {
            try
            {
                ushort[] Data = finsClient.ReadData(450, 0, 1, PLCConfig.Dr);

                //cod已经在运行
                if (Data != null && Data[0] == 2)
                {
                    LogUtil.Log("COD正在运行,不用重复启动。");
                    return;
                }
                byte bitAdress = 0X08;

                byte[] wValue = { 0X01 };
                bool success = finsClient.WriteBitData(15, bitAdress, wValue, PLCConfig.Wr);

                if (!success)
                {
                    LogUtil.Log("COD启动失败，PLC通讯异常。");
                }

                bitAdress = 0X00;
                success = finsClient.WriteBitData(511, bitAdress, wValue, PLCConfig.Wr);
                if (!success)
                {
                    LogUtil.Log("COD启动失败，PLC通讯异常。");

                }
            }
            catch (Exception ex)
            {

                LogUtil.LogError(ex);
            }


        }

        public async Task<float> ReadCodData()
        {
            //Task<bool> CompletetResult =  await Task.Factory.StartNew ( ()=>IsCODComplete());

            //Task.WaitAll(CompletetResult);
            float value = 0;
            byte bitAdress = 0X00;
            while (true)
            {
                await Task.Delay(1 * 60 * 1000);

                ushort[] Data = finsClient.ReadData(453, bitAdress, 1, PLCConfig.Dr);

                if (Data != null && Data.Length > 0) 
                {
                    if(Data[0] != 0) 
                    {
                        string msg = string.Empty;

                        switch (Data[0])
                        {
                            case 1:
                                msg = "系统故障";
                                break;
                            case 2:
                                msg = "采集原水故障";
                                break;
                            case 3:
                                msg = "缺试剂";
                                break;
                            case 4:
                                msg = "缺蒸馏水";
                                break;
                            case 5:
                                msg = "加热故障";
                                break;
                            case 6:
                                msg = "排残液故障";
                                break;
                            case 7:
                                msg = "测量值超量程异常";
                                break;
                            case 8:
                                msg = "其他故障";
                                break;
                            case 15:
                                msg = "通信失败";
                                break;
                            default:
                                break;
                        }

                        LogUtil.Log("COD异常信息：" + msg);
                        DisPlayAlarmInfo(8, 800+ Data[0], msg);

                        return -1;
                    }
                }


                 Data = finsClient.ReadData(450, bitAdress, 1, PLCConfig.Dr);


                if (Data != null && Data.Length > 0)
                {
                    if (Data[0] == 255)
                    {
                        return -1;
                    }

                    if (Data[0] == 4)
                    {
                        float[] Data1 = finsClient.ReadBigFloatData(432, bitAdress, 2, PLCConfig.Dr);
                        if (Data1 == null || Data1.Length < 1)
                        {
                            DisPlayAlarmInfo(8, 813, "COD测量值取值异常");
                            return -1;
                        }
                        value = Data1[0];

                        if (bodData.CodData < 0 || bodData.CodData > 999)
                        {
                            DisPlayAlarmInfo(8, 13, "COD测量值超出量程");
                            return -1;
                        }

                        return value;
                    }
                }

            }

            //float value = 0;
            //if (CompletetResult.Result)
            //{
            //    byte bitAdress = 0X00;
            //    float[] Data = finsClient.ReadBigFloatData(432, bitAdress, 2, PLCConfig.Dr);
            //    value = Data[0];
            //}
        }

        public async Task<bool> IsCODComplete()
        {
            try
            {
                while (true)
                {
                    await Task.Delay(5 * 60 * 1000);

                    byte bitAdress = 0X00;
                    ushort[] Data = finsClient.ReadData(450, bitAdress, 1, PLCConfig.Wr);
                    if (Data[0] == 4)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
                return false;
            }
        }

        public int GetSampleDilByCod(float CodDdata)
        {
            double CodDil = 0;

            switch (configData.sampleDilType)
            {
                case SampleDilType.WarerIn:
                    CodDil = 0.5;
                    break;
                case SampleDilType.WaterOut:
                    CodDil = 0.3;
                    break;
                default:
                    break;
            }

            if (CodDil == 0)
            {
                return 0;
            }

            double Dil = CodDdata * CodDil / 20;

            return Convert.ToInt32(Dil);

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
            mainWindow.Dispatcher.Invoke(refreshProcessStatus, ProcessType.DrainEmpty);
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

            Thread.Sleep(configData.EmptyTime * 100);

            //ushort[] value = finsClient.ReadData(PLCConfig.Valve1Address, 0, 1, PLCConfig.IO);
            //if (value == null || value.Length < 1 || (value[0] & 1) != 0) 
            //{
            //    DisPlayAlarmInfo(7, 11, "抽水泵排水超时");
            //}

            return success;
        }

        public void PrecipitateIsTimeOut(object time)
        {
            try
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
                    DisPlayAlarmInfo(7, 10, "抽水泵进水超时");
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }

        }

        public void DisPlayAlarmInfo(int deviceInfo, int errorCode, string errorDes)
        {
            DateTime dateTime = DateTime.Now;
            AlarmData alarmData = new AlarmData();
            alarmData.DeviceInfo = deviceInfo;
            alarmData.ErrorCode = errorCode;
            alarmData.ErrorDes = errorDes;
            alarmData.CreateDate = dateTime.ToLongDateString();
            alarmData.CreateTime = dateTime.ToLongTimeString();

            mainWindow.Dispatcher.BeginInvoke(addAlramInfo, alarmData);
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
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }

        }

        public void CloseTimer()
        {
            try
            {

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

                   // CloseTimer();

                    if (serialPortHelp != null)
                    {
                        serialPortHelp.Dispose();
                    }

                    manualevent.Dispose();
                    resetEvent.Dispose();
                    s_asyncSemaphore.Dispose();

                    try
                    {
                        if (timer != null)
                        {
                            timer.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogError(ex);
                    }

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
