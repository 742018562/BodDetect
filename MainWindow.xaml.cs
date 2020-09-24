using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Arthas.Controls;
using Arthas.Utility.Media;
using BodDetect.BodDataManage;
using BodDetect.DataBaseInteractive.Sqlite;
using BodDetect.Event;
using BodDetect.PagerDataModels;
using BodDetect.UDP;
using MahApps.Metro.Controls.Dialogs;
using System.Configuration;

namespace BodDetect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow, IDisposable
    {
        private Dictionary<byte, MetroProgressBar> metroProgressBars = new Dictionary<byte, MetroProgressBar>();
        private Dictionary<byte, EventHandler> ProcessHandlers = new Dictionary<byte, EventHandler>();
        private Dictionary<byte, MetroSwitch> ValveDic = new Dictionary<byte, MetroSwitch>();
        private Dictionary<byte, MetroSwitch> MultiValveDic = new Dictionary<byte, MetroSwitch>();

        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer UpdataStatusTimer = new DispatcherTimer();
        DispatcherTimer StandWaterTimer = new DispatcherTimer();
        DispatcherTimer RunTimer = new DispatcherTimer();
        DispatcherTimer CodTimer = new DispatcherTimer();

        public Process kbpr;
        private BodHelper bodHelper;
        public BodData bodData = new BodData();

        public ProgressDialogController progressDialog;

        private CancellationTokenSource StopCts = new CancellationTokenSource();

        MainWindow_Model mainWindow_Model = new MainWindow_Model();

        ConfigData configData = new ConfigData();
        private bool disposedValue;

        public bool DataGridIsEdit = false;

        public Task BodCurrentRunTask;

        public MainWindow()
        {
            InitializeComponent();

            metroProgressBars.Add(PLCConfig.WaterValveBit, WaterView);
            metroProgressBars.Add(PLCConfig.bufferValveBit, CacheView);
            metroProgressBars.Add(PLCConfig.StandardValveBit, StandardView);
            metroProgressBars.Add(Convert.ToByte((ushort)100), PumpView);
            metroProgressBars.Add(PLCConfig.AirValveBit, AirView);
            metroProgressBars.Add(PLCConfig.NormalValveBit, NormalView);
            metroProgressBars.Add(PLCConfig.SampleValveBit, SampleView);
            metroProgressBars.Add(PLCConfig.DepositValveBit, StoreWaterView);
            metroProgressBars.Add(Convert.ToByte((ushort)101), WaterSampleView);

            ProcessHandlers.Add(PLCConfig.WaterValveBit, RefeshWaterProcessEvent);
            ProcessHandlers.Add(PLCConfig.bufferValveBit, RefeshCachProcessEvent);
            ProcessHandlers.Add(PLCConfig.StandardValveBit, RefeshStandProcessEvent);
            ProcessHandlers.Add(Convert.ToByte((ushort)100), RefeshPumpProcessEvent);
            ProcessHandlers.Add(PLCConfig.AirValveBit, RefeshAirProcessEvent);
            ProcessHandlers.Add(PLCConfig.NormalValveBit, RefeshNormalProcessEvent);
            ProcessHandlers.Add(PLCConfig.SampleValveBit, RefeshSampleProcessEvent);
            ProcessHandlers.Add(PLCConfig.DepositValveBit, RefeshStoreWaterProcessEvent);
            ProcessHandlers.Add(Convert.ToByte((ushort)101), RefeshWaterSampleProcessEvent);

            ValveDic.Add(PLCConfig.WaterValveBit, WaterValve);
            ValveDic.Add(PLCConfig.bufferValveBit, CacheValve);
            ValveDic.Add(PLCConfig.StandardValveBit, StandValve);
            ValveDic.Add(PLCConfig.AirValveBit, AirValve);
            ValveDic.Add(PLCConfig.NormalValveBit, NormalValve);
            ValveDic.Add(PLCConfig.SampleValveBit, Valve);
            ValveDic.Add(PLCConfig.DepositValveBit, StoreValve);


            //ValveDic.Add(PLCConfig.CisternValveBit, RowValve);
            //ValveDic.Add(PLCConfig.WashValveBit, WashValve);
            //ValveDic.Add(PLCConfig.BodDrainValveBit, BodRowValve);

            initAsync();

            this.DataContext = mainWindow_Model;

        }

        public async void initAsync()
        {
            try
            {

                string PLCip = ConfigurationManager.AppSettings["PLCip"];
                string PLCport = ConfigurationManager.AppSettings["PLCport"];

                LogUtil.Log(PLCip + PLCport);


                string[] value = PLCip.Split('.');
                if (value.Length < 4)
                {
                    await this.ShowMessageAsync("Error", "异常ip!", MessageDialogStyle.Affirmative);
                }
                int port = Convert.ToInt32(PLCport, 10);

                bodHelper = new BodHelper(PLCip, port);
                bodHelper.Init();

                bodHelper.refreshProcess = new BodHelper.RefreshUI(RefeshProcess);
                bodHelper.refreshStaus = new BodHelper.RefreshStaus(RefreshStatus);
                bodHelper.refreshData = new BodHelper.RefreshData(RefreshData);
                bodHelper.refreshProcessStatus = new BodHelper.RefreshProcessStatus(RefreshProcessStatus);
                bodHelper.addAlramInfo = new BodHelper.AddAlramInfo(AddAlarmInfo);

                bodHelper.mainWindow = this;

                initConfig();

                if (!bodHelper.IsConnectPlc)
                {
                    LogUtil.LogError("连接PLC异常");
                    await this.ShowMessageAsync("Error", "连接PLC异常!");
                    return;
                }
                if (!bodHelper.ConnectSeri)
                {
                    LogUtil.LogError("连接串口异常");

                    await this.ShowMessageAsync("Error", "连接串口异常!");
                    return;

                }

                await this.Dispatcher.InvokeAsync(() => Start());

            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "开机启动initAsync");
                await this.ShowMessageAsync("Error", "连接PLC异常!");
            }
        }

        private async void Start()
        {
            try
            {
                UpdataStatusTimer.Tick += UpdateDevStatus;
                int times = Convert.ToInt32(UpdataStatus.Text);
                UpdataStatusTimer.Interval = new TimeSpan(0, times, 0);

                int SpaceHour = Convert.ToInt32(sampleSpac.Text);

                start.IsChecked = true;
                if (!bodHelper.IsSampling)
                {
                    initConfig();

                    Task initTask = await Task.Factory.StartNew(() => bodHelper.PreInitAsync());

                    if (!System.IO.File.Exists(XmlHelp.Xmlpath))
                    {
                        XmlHelp.createXml(XmlHelp.Xmlpath);
                    }

                    Task.WaitAll(initTask);

                    StandWaterTimer.Tick += StartStandWaterAsync;
                    StandWaterTimer.Interval = new TimeSpan(0, 1, 0, 0);

                    StandWaterTimer.Start();
                    //Task StandTask = Task.Factory.StartNew(() => bodHelper.StartBodStandWater());

                    //TimeSpan timeSpan = new TimeSpan(SpaceHour, 0, 0);
                    //Task.WaitAll(StandTask);

                    if (RunTimer == null || !RunTimer.IsEnabled)
                    {
                        RunTimer.Tick += BodRun;
                        RunTimer.Interval = new TimeSpan(SpaceHour, 0, 0);
                        RunTimer.Start();
                    }

                    BodCurrentRunTask = Task.Factory.StartNew(() => bodHelper.StartBodDetect(StopCts.Token), StopCts.Token);
                    _loading.Visibility = Visibility.Visible;
                }
                else
                {
                    bodHelper.manualevent.Set();
                    _loading.Visibility = Visibility.Collapsed;
                }

            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "开机启动异步线程Start");
            }
        }

        private void MetroButton_Click(object sender, RoutedEventArgs e)
        {

            float[] DoDota = bodHelper.GetDoData();
            Thread.Sleep(3000);

            uint[] TurbidityData = bodHelper.GetTurbidityData();
            Thread.Sleep(3000);

            float[] PHData = bodHelper.GetPHData();
            Thread.Sleep(3000);
            bodData.TemperatureData = DoDota[0];
            bodData.DoData = DoDota[1];
            bodData.TurbidityData = (float)TurbidityData[0] / 1000;
            bodData.PHData = PHData[1];
            //bodData.CodData = (float)CODData[0] / 100;
            //      if (finsClient == null)
            //      {
            //          MessageBox.Show("PLC 未连接");
            //          return;
            //      }


            //      ushort Address = Convert.ToUInt16(address_TextBox.Text);

            //      byte bitAddress = Convert.ToByte(Bit_TextBox.Text);

            //      ushort Count = Convert.ToUInt16(DataCount_TextBox.Text);

            ////      string area = MemoryAreaCode_combo.SelectedItem.ToString();


            //      byte AreaCode = 0X82;

            //      ushort[] data = finsClient.ReadData(Address, bitAddress, Count, AreaCode);
            //      foreach (var item in data)
            //      {
            //          string value = Convert.ToString(item) + "\r\n";
            //          Data_TextBox.AppendText(value);
            //      }

        }

        //private void MetroButton_Click_1(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        string ip = IP_textbox.Text;

        //        string[] value = ip.Split('.');
        //        if (value.Length < 4)
        //        {
        //            this.ShowMessageAsync("Error", "异常ip!");
        //        }

        //        int port = Convert.ToInt32(Port_TextBox.Text);
        //        bool success = bodHelper.ConnectPlc();

        //        if (success)
        //        {
        //            this.ShowMessageAsync("与PLC通讯", "连接成功！", MessageDialogStyle.Affirmative);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogUtil.LogError(ex);
        //        this.ShowMessageAsync("Error", "连接PLC异常!");
        //    }

        //}

        //private void ResetIp_Click(object sender, RoutedEventArgs e)
        //{
        //    IP_textbox.Text = "192.168.0.174";
        //    Port_TextBox.Text = "9600";
        //}

        private async void RefreshData(BodData data)
        {
            mainWindow_Model.BodData = data.Bod;
            mainWindow_Model.CodData = data.CodData;
            mainWindow_Model.DoData = data.DoData;
            mainWindow_Model.PHData = data.PHData;
            mainWindow_Model.TemperatureData = data.TemperatureData;
            mainWindow_Model.TurbidityData = data.TurbidityData;
            mainWindow_Model.Uv254Data = data.Uv254Data;
            mainWindow_Model.HumidityDataData = data.HumidityData;
            mainWindow_Model.AirTemperatureData = data.AirTemperatureData;

            if (mainWindow_Model.BodData < 0 || mainWindow_Model.BodData > 1000)
            {
                //BOD.Foreground = new SolidColorBrush( Color.FromRgb(255,0,0));
                BOD_.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }

            HisDatabase hisDatabase = new HisDatabase();
            hisDatabase.DoData = mainWindow_Model.DoData;
            hisDatabase.DoDataUnit = "mg/L";
            hisDatabase.PHData = mainWindow_Model.PHData;
            hisDatabase.TemperatureData = mainWindow_Model.TemperatureData;
            hisDatabase.TemperatureUnit = "C";
            hisDatabase.TurbidityData = mainWindow_Model.TurbidityData;
            hisDatabase.TurbidityUnit = "mg/L";
            hisDatabase.Bod = mainWindow_Model.BodData;
            hisDatabase.CodData = mainWindow_Model.CodData;
            hisDatabase.Uv254Data = mainWindow_Model.Uv254Data;
            hisDatabase.BodElePot = data.BodElePot;
            hisDatabase.BodElePotDrop = data.BodElePotDrop;

            hisDatabase.CreateDate = data.CreateDate;
            hisDatabase.CreateTime = data.CreateTime;

            mainWindow_Model.HisParamData.AddData(hisDatabase);

            HisDataBaseModel hisDataBaseModel = new HisDataBaseModel();
            hisDatabase.CopyToHisDataBaseModel(hisDataBaseModel);
            await Task.Factory.StartNew(() => BodSqliteHelp.InsertHisBodData(hisDataBaseModel));

            mainWindow_Model.UpdateSensorStatus();
        }

        private void MetroButton_Click_2(object sender, RoutedEventArgs e)
        {
            float[] floatData = bodHelper.GetDoData();

            floatData = bodHelper.GetPHData();

            uint[] value = bodHelper.GetTurbidityData();

            byte[] IOCmd = { PLCConfig.WashValveBit, PLCConfig.BodDrainValveBit };

            bodHelper.ValveControl(PLCConfig.Valve2Address, IOCmd);

            byte[] IOCmd2 = { PLCConfig.DepositValveBit, PLCConfig.StandardValveBit };
            bodHelper.ValveControl(PLCConfig.Valve1Address, IOCmd2);
        }


        #region 委托处理
        public void RefeshProcess(DelegateParam param)
        {
            try
            {
                if (metroProgressBars.Count < 0)
                    return;
                switch (param.State)
                {
                    case ProcessState.ShowData:
                        metroProgressBars[param.Uid].Value = (double)param.Data;
                        break;

                    case ProcessState.AutoAdd:
                        ProcessAutoAdd(param);
                        break;
                    case ProcessState.AutoRed:
                        break;
                    case ProcessState.Hidden:
                        break;
                    case ProcessState.Show:
                        break;
                }

            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                return;
            }
        }

        /// <summary>
        /// 多通阀进度条控制
        /// </summary>
        /// <param name="param"></param>
        public void ProcessAutoAdd(DelegateParam param)
        {
            while (true)
            {
                if (!timer.IsEnabled)
                {
                    timer.Tick += ProcessHandlers[param.Uid];
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                    timer.Start();
                    return;
                }
            }
        }

        ///// <summary>
        ///// Bod部分的进度条委托
        ///// </summary>
        ///// <param name="param"></param>
        //public void BodProcessCtrl(DelegateParam param)
        //{


        //}

        public void RefeshWaterProcessEvent(object sender, EventArgs e)
        {
            if (WaterView.Value >= WaterView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshWaterProcessEvent;
            }

            WaterView.Value++;
        }

        public void RefeshCachProcessEvent(object sender, EventArgs e)
        {
            if (CacheView.Value >= CacheView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshCachProcessEvent;
            }

            CacheView.Value++;
        }

        public void RefeshStandProcessEvent(object sender, EventArgs e)
        {
            if (StandardView.Value >= StandardView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshStandProcessEvent;
            }

            StandardView.Value++;
        }

        public void RefeshPumpProcessEvent(object sender, EventArgs e)
        {
            if (PumpView.Value >= PumpView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshPumpProcessEvent;
            }

            PumpView.Value++;
        }
        public void RefeshAirProcessEvent(object sender, EventArgs e)
        {
            if (AirView.Value >= AirView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshAirProcessEvent;
            }

            AirView.Value++;
        }

        public void RefeshNormalProcessEvent(object sender, EventArgs e)
        {
            if (NormalView.Value >= NormalView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshNormalProcessEvent;
            }

            NormalView.Value++;
        }

        public void RefeshSampleProcessEvent(object sender, EventArgs e)
        {
            if (SampleView.Value >= SampleView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshSampleProcessEvent;
            }

            SampleView.Value++;
        }

        public void RefeshStoreWaterProcessEvent(object sender, EventArgs e)
        {
            if (StoreWaterView.Value >= StoreWaterView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshStoreWaterProcessEvent;
            }

            StoreWaterView.Value++;
        }

        public void RefeshWaterSampleProcessEvent(object sender, EventArgs e)
        {
            if (WaterSampleView.Value >= WaterSampleView.Maximum)
            {
                timer.Stop();
                timer.Tick -= RefeshWaterSampleProcessEvent;
            }

            WaterSampleView.Value++;
        }

        public void RefreshStatus(SysStatus sysStatus)
        {
            switch (sysStatus)
            {
                case SysStatus.Sampling:
                    start.IsChecked = true;
                    break;
                case SysStatus.Pause:
                    start.IsChecked = false;
                    break;
                case SysStatus.Complete:
                    start.IsChecked = false;
                    break;
                default:
                    break;
            }

        }

        public void RefreshProcessStatus(ProcessType processType)
        {
            string status = Tool.GetProcessTypeToString(processType);

            SysStatusData sysStatusData = new SysStatusData();
            sysStatusData.Status = status;
            sysStatusData.id = mainWindow_Model.SysStatusDataModel.AllSysStatusData.Count + 1;
            sysStatusData.num = 0;
            DateTime dateTime = DateTime.Now;
            sysStatusData.CreateDate = dateTime.ToLongDateString();
            sysStatusData.CreateTime = dateTime.ToLongTimeString();
            SysStatusInfoModel sysStatusInfoModel = new SysStatusInfoModel();
            sysStatusData.CopyToSysStatusInfoModel(sysStatusInfoModel);
            Task refeshTask = Task.Factory.StartNew(() => BodSqliteHelp.InsertSysStatusData(sysStatusInfoModel));

            //   refeshTask.Dispose();
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                mainWindow_Model.SysStatusDataModel.AddData(sysStatusData);

                SysStaus.Content = status;

                LogUtil.Log("状态更新成功:" + status);
            }));

        }

        public void AddAlarmInfo(AlarmData alarmData)
        {
            alarmData.id = mainWindow_Model.AlramPagerModels.AllAlarmData.Count + 1;

            mainWindow_Model.AlramPagerModels.AddData(alarmData);

            AlramInfoModel alramInfoModel = new AlramInfoModel();
            alarmData.CopyToAlramInfoModel(alramInfoModel);
            BodSqliteHelp.InsertAlramInfo(alramInfoModel);

            HisAlarmList.UpdateLayout();
        }

        #endregion


        private void Valve_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Valve.IsChecked == true)
                {
                    byte[] data = { PLCConfig.SampleValveBit };
                    bodHelper.ValveControl(PLCConfig.Valve2Address, data);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                Valve.IsChecked = false;
                this.ShowMessageAsync("Error", "送样(样液)阀门打开失败.");
            }
        }

        private void NormalValve_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NormalValve.IsChecked == true)
                {
                    byte[] data = { PLCConfig.NormalValveBit };
                    bodHelper.ValveControl(PLCConfig.Valve2Address, data);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                NormalValve.IsChecked = false;
                this.ShowMessageAsync("Error", "送样(标液)阀门打开失败.");
            }
        }

        private void Valves_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MetroSwitch valve = (MetroSwitch)sender;

                if (bodHelper.IsSampling == true)
                {
                    this.ShowMessageAsync("Error", "现在正在采样过程中,禁止相关操作.");

                    valve.IsChecked = false;
                    return;
                }

                if (valve.IsChecked == true)
                {

                    foreach (var item in ValveDic)
                    {
                        if (item.Value != valve)
                        {
                            item.Value.IsChecked = false;
                        }
                    }

                    var key = ValveDic.FirstOrDefault(t => t.Value == valve).Key;

                    byte[] data = { key };
                    bodHelper.ValveControl(PLCConfig.Valve2Address, data);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

                StoreValve.IsChecked = false;
                this.ShowMessageAsync("Error", "阀门打开失败.");
            }
        }

        private void MetroButton_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bodHelper.IsSampling == true)
                {
                    this.ShowMessageAsync("Error", "现在正在采样过程中,禁止相关操作..");

                    return;
                }

                var CheckedVavle = ValveDic.Where(t => t.Value.IsChecked == true).ToList();

                if (CheckedVavle == null)
                {
                    this.ShowMessageAsync("Error", "只能打开一个阀门,请关闭阀门.");

                    return;
                }

                if (CheckedVavle.Count > 2)
                {
                    this.ShowMessageAsync("Error", "只能打开一个阀门,请关闭阀门.");
                    return;
                }

                var key = CheckedVavle[0].Key;

                timer.Tick += ProcessHandlers[key];

                bodHelper.PunpAbsorb(PunpCapType.fiveml);
                timer.Start();

            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }
        }

        private void PumpDrain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (bodHelper.IsSampling == true)
                {
                    this.ShowMessageAsync("Error", "现在正在采样过程中,禁止相关操作.");

                    return;
                }

                var CheckedVavle = ValveDic.Where(t => t.Value.IsChecked == true).ToList();

                if (CheckedVavle == null)
                {
                    this.ShowMessageAsync("Error", "请打开任意一个阀门.");
                    return;
                }

                if (CheckedVavle.Count > 2)
                {
                    this.ShowMessageAsync("Error", "只能打开一个阀门,请关闭阀门.");
                    return;
                }

                bodHelper.PumpDrain();

            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }
        }

        private void PumpDrain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bodHelper.IsSampling == true)
                {
                    this.ShowMessageAsync("Error", "现在正在采样过程中,禁止相关操作.");

                    return;
                }

                var CheckedVavle = ValveDic.Where(t => t.Value.IsChecked == true).ToList();

                if (CheckedVavle == null)
                {
                    this.ShowMessageAsync("Error", "请打开任意一个阀门.");

                    return;
                }

                if (CheckedVavle.Count > 2)
                {
                    this.ShowMessageAsync("Error", "只能打开一个阀门,请关闭阀门.");

                    return;
                }

                bodHelper.PumpDrain();

            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }
        }

        private void PumpWaterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (bodHelper.IsSampling == true)
                {
                    this.ShowMessageAsync("Error", "现在正在采样过程中,禁止相关操作.");

                    return;
                }

                PumpWaterButton.Visibility = Visibility.Collapsed;
                PumpStopButton.Visibility = Visibility.Visible;
                byte[] data = { PLCConfig.CisternPumpBit };

                bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }

        }

        private void PumpStopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (bodHelper.IsSampling == true)
                {
                    this.ShowMessageAsync("Error", "现在正在采样过程中,禁止相关操作.");

                    return;
                }

                PumpWaterButton.Visibility = Visibility.Visible;
                PumpStopButton.Visibility = Visibility.Collapsed;
                byte[] data = { 0 };

                bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }

        }

        private void RowValve_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bodHelper.IsSampling == true)
                {
                    this.ShowMessageAsync("Error", "现在正在采样过程中,禁止相关操作.");

                    return;
                }

                byte[] data = { PLCConfig.DepositValveBit };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        private void StandCap_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void MetroComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PunpStand_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = PunpStand.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    this.ShowMessageAsync("Tips", "请输入抽取容量.");

                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.StandardValveBit };

                byte[] StandBodValve = { PLCConfig.NormalValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);


                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception ex)
            {

                LogUtil.LogError(ex);

            }
        }

        private void PumpProcess(List<byte[]> data, List<ushort> address, PunpCapType punpCapType)
        {
            if (data == null || data.Count < 2 || address == null || address.Count < 2)
            {
                return;
            }

            bool success = false;

            success = bodHelper.ValveControl(address[0], data[0]);

            Thread.Sleep(1000);

            if (!success)
            {
                MessageBox.Show(" 阀门打开失败.", "提示", MessageBoxButton.OK);
            }

            success = bodHelper.PunpAbsorb(punpCapType);

            if (!success)
            {
                MessageBox.Show(" 注射泵抽水失败.", "提示", MessageBoxButton.OK);
            }

            Thread.Sleep(7000);

            success = bodHelper.ValveControl(address[1], data[1]);

            Thread.Sleep(1000);

            if (!success)
            {
                MessageBox.Show(" 阀门打开失败.", "提示", MessageBoxButton.OK);
            }
            success = bodHelper.PumpDrain();

            if (!success)
            {
                MessageBox.Show(" 注射泵放水失败.", "提示", MessageBoxButton.OK);
            }

            Thread.Sleep(7000);
        }

        private async void PumpCache_ButtonClick(object sender, EventArgs e)
        {

            await this.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    string cap = PumpCache.Text;
                    if (string.IsNullOrEmpty(cap))
                    {
                        MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                    }

                    int capData = Convert.ToInt32(cap);

                    int times = capData / 5;

                    int extraTimes = capData % 5;

                    byte[] StandValve = { PLCConfig.WaterValveBit };

                    byte[] StandBodValve = { PLCConfig.NormalValveBit };

                    List<byte[]> data = new List<byte[]>();
                    List<ushort> address = new List<ushort>();
                    data.Add(StandValve);
                    data.Add(StandBodValve);

                    address.Add(PLCConfig.Valve2Address);
                    address.Add(PLCConfig.Valve2Address);


                    for (int i = 0; i < times; i++)
                    {
                        PumpProcess(data, address, PunpCapType.fiveml);
                    }

                    for (int i = 0; i < extraTimes; i++)
                    {
                        PumpProcess(data, address, PunpCapType.oneml);

                    }

                    byte[] data1 = { 0 };
                    bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
                }
                catch (Exception ex)
                {

                    LogUtil.LogError(ex);

                }
            });
        }

        private void DrainCahce_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = DrainCahce.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData;


                byte[] StandValve = { PLCConfig.NormalValveBit };

                byte[] StandBodValve = { PLCConfig.AirValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);


                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);


            }
        }

        private void PunpSample_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = PunpSample.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.DepositValveBit };

                byte[] StandBodValve = { PLCConfig.SampleValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        private void PumpWater_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = PumpWater.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.WaterValveBit };

                byte[] StandBodValve = { PLCConfig.SampleValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }
        }

        private void PumpCache1_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = PumpCache1.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.bufferValveBit };

                byte[] StandBodValve = { PLCConfig.SampleValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);


            }
        }

        private void DrainSample_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = DrainSample.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData;


                byte[] StandValve = { PLCConfig.SampleValveBit };

                byte[] StandBodValve = { PLCConfig.AirValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }


                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        private void wash_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                string cap = wash.Text;
                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.WaterValveBit };

                byte[] StandBodValve = { PLCConfig.AirValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }
        }

        private void sample_start_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = { PLCConfig.CisternPumpBit };

            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            if (!success)
            {
                MessageBox.Show(" 抽水样泵打开失败.", "提示", MessageBoxButton.OK);
            }
        }

        private void sample_end_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = { 0 };

            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            if (!success)
            {
                MessageBox.Show(" 抽水样泵关闭失败.", "提示", MessageBoxButton.OK);
            }
        }

        private void drain_start_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = { PLCConfig.CisternValveBit };

            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            if (!success)
            {
                MessageBox.Show(" 沉淀池排水阀打开失败.", "提示", MessageBoxButton.OK);
            }
        }

        private void drain_end_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = { 0 };

            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            if (!success)
            {
                MessageBox.Show(" 沉淀池排水阀关闭失败.", "提示", MessageBoxButton.OK);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string value = textBox.Text;

            int a = SysStatusList.Items.Count;
            SysStatusList.Items.Add(new SysStatusMsg(a + 1, "2", value, value));
        }

        private async void StandDilution_Click(object sender, RoutedEventArgs e)
        {

            await Task.Factory.StartNew(() => bodHelper.DiluteStandWater());
            return;

            //try
            //{
            //    string cap = StandAll.Text;
            //    string zoom = Dilution.Text;

            //    if (string.IsNullOrEmpty(cap))
            //    {
            //        MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
            //    }

            //    int capData = Convert.ToInt32(cap);
            //    int zoomdata = Convert.ToInt32(zoom);

            //    int waterData = capData * (zoomdata - 1);

            //    int times = capData / 5;

            //    int extraTimes = capData % 5;

            //    byte[] StandValve = { PLCConfig.StandardValveBit };

            //    byte[] StandBodValve = { PLCConfig.NormalValveBit };

            //    byte[] waterValve = { PLCConfig.WaterValveBit };

            //    List<byte[]> data = new List<byte[]>();
            //    List<ushort> address = new List<ushort>();
            //    data.Add(StandValve);
            //    data.Add(StandBodValve);

            //    address.Add(PLCConfig.Valve2Address);
            //    address.Add(PLCConfig.Valve2Address);

            //    while (times > 0)
            //    {
            //        PumpProcess(data, address, PunpCapType.fiveml);
            //        times--;
            //    }

            //    while (extraTimes > 0)
            //    {
            //        PumpProcess(data, address, PunpCapType.oneml);
            //        extraTimes--;
            //    }


            //    times = waterData / 5;
            //    extraTimes = waterData % 5;
            //    data[0] = waterValve;
            //    data[1] = StandBodValve;

            //    while (times > 0)
            //    {
            //        PumpProcess(data, address, PunpCapType.fiveml);
            //        times--;
            //    }

            //    while (extraTimes > 0)
            //    {
            //        PumpProcess(data, address, PunpCapType.oneml);
            //        extraTimes--;
            //    }


            //    byte[] data1 = { 0 };
            //    bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            //}
            //catch (Exception)
            //{


            //}
        }

        private void SampleDilution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cap = SampleAll.Text;
                string zoom = DilutionSamp.Text;

                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);
                int zoomdata = Convert.ToInt32(zoom);

                int waterData = capData * (zoomdata - 1);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.DepositValveBit };

                byte[] StandBodValve = { PLCConfig.SampleValveBit };

                byte[] waterValve = { PLCConfig.WaterValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(StandValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }

                times = waterData / 5;
                extraTimes = waterData % 5;
                data[0] = waterValve;
                data[1] = StandBodValve;

                while (times > 0)
                {
                    PumpProcess(data, address, PunpCapType.fiveml);
                    times--;
                }

                while (extraTimes > 0)
                {
                    PumpProcess(data, address, PunpCapType.oneml);
                    extraTimes--;
                }


                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            kbpr = Tool.ShowInputPanel(kbpr);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Tool.HideInputPanel(kbpr);
        }

        private async void PreButton_Click(object sender, RoutedEventArgs e)
        {
            if (bodHelper == null || bodHelper.finsClient == null || bodHelper.finsClient == null)
            {
                MessageBox.Show(" PLC未连接请连接.", "提示", MessageBoxButton.OK);
            }


            await Task.Factory.StartNew(() => bodHelper.PreInitAsync());
        }

        private async void PreButton2ml_ClickAsync(object sender, RoutedEventArgs e)
        {
            await Task.Factory.StartNew(() =>
            {
                if (bodHelper == null || bodHelper.finsClient == null || bodHelper.finsClient == null)
                {
                    MessageBox.Show(" PLC未连接请连接.", "提示", MessageBoxButton.OK);
                }

                byte[] StandBodValve = { PLCConfig.NormalValveBit };

                byte[] waterValve = { PLCConfig.StandardValveBit };

                byte[] AirValve = { PLCConfig.AirValveBit };

                List<byte[]> data = new List<byte[]>();
                List<ushort> address = new List<ushort>();
                data.Add(waterValve);
                data.Add(StandBodValve);

                address.Add(PLCConfig.Valve2Address);
                address.Add(PLCConfig.Valve2Address);

                bool success = bodHelper.ValveControl(address[0], data[0]);

                Thread.Sleep(300);

                if (!success)
                {
                    MessageBox.Show(" 阀门打开失败.", "提示", MessageBoxButton.OK);
                }

                success = bodHelper.PunpAbsorb(PunpCapType.Point2ml);

                //PumpProcess(data, address, PunpCapType.Point2ml);
            });
            //data[0] = AirValve;

            //PumpProcess(data, address, PunpCapType.fiveml);

        }

        private void Pre2ml_Click(object sender, RoutedEventArgs e)
        {
            bodHelper.PunpAbsorb(PunpCapType.Point2ml);
        }

        private void Pumpdrain2ml_Click(object sender, RoutedEventArgs e)
        {
            bodHelper.PumpDrain();
        }

        private async void PrePumpAri_Click(object sender, RoutedEventArgs e)
        {
            //bodHelper.ChangePunpValve(PumpValveType.pre);
            //bodHelper.PrePumpCtrl(PrePumpWork.preAbsorb);
            await Task.Factory.StartNew(() =>
            {
                byte[] data = { PLCConfig.AirValveBit };

                bodHelper.ValveControl(PLCConfig.Valve2Address, data);
                data[0] = 0X01;

                bodHelper.ChangePunpValve(PumpValveType.pre);
                bodHelper.finsClient.WriteBitData(1, 0, data, PLCConfig.Wr);

                //bodHelper.ValveControl(PLCConfig.Valve1Address, PLCConfig.PrePumpValveAir, data);
                bodHelper.PrePumpCtrl(PrePumpWork.preAbsorb);
                Thread.Sleep(5000);
                bodHelper.PumpDrain();
            });
        }

        private async void PrePumpwater_Click(object sender, RoutedEventArgs e)
        {
            //bodHelper.ChangePunpValve(PumpValveType.pre);
            //bodHelper.PrePumpCtrl(PrePumpWork.preDrain);

            await Task.Factory.StartNew(() =>
            {
                byte[] data = { PLCConfig.WaterValveBit };

                bodHelper.ValveControl(PLCConfig.Valve2Address, data);
                data[0] = 0X01;

                bodHelper.ChangePunpValve(PumpValveType.pre);

                bodHelper.finsClient.WriteBitData(0, 15, data, PLCConfig.Wr);

                //bodHelper.ValveControl(PLCConfig.Valve1Address, PLCConfig.PrePumpValveAir, data);
                bodHelper.PrePumpCtrl(PrePumpWork.preAbsorb);

                Thread.Sleep(5000);
                bodHelper.PumpDrain();
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //this.WindowState = WindowState.Maximized;

            //this.WindowStyle = WindowStyle.None;

            ////this.ResizeMode = ResizeMode.NoResize;

            //this.Topmost = true;

            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.Topmost = true;

            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.WindowState = System.Windows.WindowState.Maximized;

        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton toggleButton = sender as ToggleButton;
                if ((bool)toggleButton.IsChecked)
                {
                    if (bodHelper.IsSampling)
                    {
                        this.ShowMessageAsync("Warning。。。。", "BOD流程给已启动，请勿重复启动流程。", MessageDialogStyle.Affirmative);
                        return;
                    }
                    bodHelper.NeedStop = false;

                    StartBod(sender, e);
                }
            }
            catch (Exception ex)
            {

                LogUtil.LogError(ex);
            }

            //else
            //{
            //    AbortBod(sender, e);
            //}
        }

        public void BodRun(object sender, EventArgs e)
        {
            //try
            //{

            //    if (BodDetectRun != null && BodDetectRun.IsAlive)
            //    

            //        BodDetectRun.Abort();
            //    }

            //}
            //catch (Exception)
            //{

            //}
            LogUtil.Log("启动线程执行BOD流程");

            if (BodCurrentRunTask == null)
            {
                StopCts = new CancellationTokenSource();
                BodCurrentRunTask = Task.Factory.StartNew(() => bodHelper.StartBodDetect(StopCts.Token), StopCts.Token);
                return;
            }

            if (bodHelper.IsSampling || !BodCurrentRunTask.IsCompleted)
            {
                bool success = BodCurrentRunTask.Wait(1000 * 30);

                if (!success)
                {
                    return;
                }

            }

            StopCts = new CancellationTokenSource();
            BodCurrentRunTask = Task.Factory.StartNew(() => bodHelper.StartBodDetect(StopCts.Token), StopCts.Token);
            LogUtil.Log("执行BOD流程成功");


            //BodDetectRun = new Thread(new ThreadStart(bodHelper.StartBodDetect));
            //BodDetectRun.IsBackground = true;
            //BodDetectRun.Start();

        }

        public void AbortBod(object sender, RoutedEventArgs e)
        {
            bodHelper.manualevent.Reset();
        }

        public void StartBod(object sender, RoutedEventArgs e)
        {
            try
            {
                int SpaceHour = Convert.ToInt32(sampleSpac.Text);

                if (!bodHelper.IsSampling)
                {
                    initConfig();

                    if (RunTimer == null || !RunTimer.IsEnabled)
                    {
                        RunTimer.Tick += BodRun;
                        RunTimer.Interval = new TimeSpan(SpaceHour, 0, 0);
                        RunTimer.Start();

                    }

                    BodRun(sender, e);
                    _loading.Visibility = Visibility.Visible;
                }
                else
                {
                    //  bodHelper.manualevent.Set();
                    _loading.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }

        }

        public void StartStandWaterAsync(object sender, EventArgs e)
        {
            try
            {
                if (!System.IO.File.Exists(XmlHelp.Xmlpath))
                {
                    XmlHelp.createXml(XmlHelp.Xmlpath);

                    if (BodCurrentRunTask != null && !BodCurrentRunTask.IsCompleted)
                    {

                        LogUtil.Log("等待线程task的完成,taskID = " + BodCurrentRunTask.Id);
                        bool success = BodCurrentRunTask.Wait(1000 * 10);

                        if (!success)
                        {
                            return;
                        }

                    }
                    BodCurrentRunTask = Task.Factory.StartNew(() => bodHelper.StartBodStandWater());
                    LogUtil.Log("等待线程task的完成,taskID = " + BodCurrentRunTask.Id);

                    return;
                }

                string time = XmlHelp.readtext(XmlHelp.Xmlpath);

                DateTime dateTime = Convert.ToDateTime(time);

                TimeSpan timeSpan = DateTime.Now - dateTime;
                if (timeSpan.Days > configData.BodStandFreq)
                {
                    if (BodCurrentRunTask != null && !BodCurrentRunTask.IsCompleted)
                    {

                        LogUtil.Log("等待线程task的完成,taskID = " + BodCurrentRunTask.Id);
                        bool success = BodCurrentRunTask.Wait(1000 * 10);

                        if (!success)
                        {
                            return;
                        }

                        // Task.WaitAll(BodCurrentRunTask);
                    }
                    BodCurrentRunTask = Task.Factory.StartNew(() => bodHelper.StartBodStandWater());
                    LogUtil.Log("等待线程task的完成,taskID = " + BodCurrentRunTask.Id);

                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }

        }

        public void initConfig()
        {
            if (string.IsNullOrEmpty(sampleSpac.Text) ||
                string.IsNullOrEmpty(InietTime.Text) ||
                string.IsNullOrEmpty(PrecipitateTime.Text) ||
                string.IsNullOrEmpty(EmptyTime.Text) ||
                string.IsNullOrEmpty(WarmUpTime.Text) ||
                string.IsNullOrEmpty(WashTimes.Text) ||
                string.IsNullOrEmpty(SampleScale.Text))
            {
                MessageBox.Show(" 流程配置有参数未设置,请设置后再启动.", "提示", MessageBoxButton.OK);
            }

            //configData.SampDil = Convert.ToInt32(SampDil.Text);
            configData.SampVol = Convert.ToInt32(SampVol.Text);
            configData.StandDil = Convert.ToInt32(StandDil.Text);
            configData.StandVol = Convert.ToInt32(StandVol.Text);
            configData.EmptyTime = Convert.ToInt32(EmptyTime.Text);
            configData.InietTime = Convert.ToInt32(InietTime.Text);
            configData.PrecipitateTime = Convert.ToInt32(PrecipitateTime.Text);
            configData.SpaceHour = Convert.ToInt32(sampleSpac.Text);
            configData.WarmUpTime = Convert.ToInt32(WarmUpTime.Text);
            configData.WashTimes = Convert.ToInt32(WashTimes.Text);
            configData.SampleScale = Convert.ToInt32(SampleScale.Text);
            configData.UpdateStatusInter = Convert.ToInt32(UpdataStatus.Text);
            configData.BodStandFreq = Convert.ToInt32(StandDilFreq.Text);

            bodHelper.configData = configData;
        }

        private void start_Checked(object sender, RoutedEventArgs e)
        {
            string path = @"pack://application:,,,/Resources/zanting.png";
            BitmapImage image = new BitmapImage(new Uri(path, UriKind.Absolute));
            BodRunImg.Source = image;
            _loading.Visibility = Visibility.Collapsed;
            RunStatuLab.Content = "暂停运行";
        }

        private void start_Unchecked(object sender, RoutedEventArgs e)
        {
            string path = @"pack://application:,,,/Resources/icon_player.png";
            BitmapImage image = new BitmapImage(new Uri(path, UriKind.Absolute));
            BodRunImg.Source = image;
            _loading.Visibility = Visibility.Visible;

            RunStatuLab.Content = "开始运行";
        }

        private async void restart_Click(object sender, RoutedEventArgs e)
        {
            _loading.Visibility = Visibility.Visible;
            StopCts.Cancel();

            bodHelper.NeedStop = true;
            LogUtil.Log("正在停止当前流程.");
            RunTimer.Tick -= BodRun;
            RunTimer.Stop();
            BodRunImg.IsEnabled = true;
            start.IsChecked = false;
            await this.ShowMessageAsync("Please wait...", "正在停止运行流程,请稍等...", MessageDialogStyle.Affirmative);

            // await Task.Run(StopBod);
        }

        public async void StopBod()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (!bodHelper.IsSampling)
                {
                    break;
                }
            }

            RunTimer.Tick -= BodRun;
            RunTimer.Stop();
            // progressDialog.SetMessage("运行流程重置完成...");
            Thread.Sleep(2000);
            await progressDialog.CloseAsync();
        }

        private async void DrainEmpty_Click(object sender, RoutedEventArgs e)
        {

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    byte[] Valves = { PLCConfig.NormalValveBit, PLCConfig.SampleValveBit };

                    List<byte[]> data = new List<byte[]>();
                    byte[] tem = { PLCConfig.SampleValveBit };
                    byte[] tem1 = { PLCConfig.AirValveBit };
                    data.Add(tem);
                    data.Add(tem1);

                    List<ushort> Address = new List<ushort>();
                    Address.Add(PLCConfig.Valve2Address);
                    Address.Add(PLCConfig.Valve2Address);

                    foreach (var item in Valves)
                    {
                        data[0][0] = item;
                        for (int i = 0; i < 10; i++)
                        {
                            PumpProcess(data, Address, PunpCapType.fiveml);
                        }
                    }

                    byte[] data1 = { PLCConfig.CisternValveBit };
                    bodHelper.ValveControl(PLCConfig.Valve1Address, data1);
                }
                catch (Exception ex)
                {

                    LogUtil.LogError(ex);
                }

            });

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (SingleProcessTab == null)
            {
                return;
            }
            // mainWindow_Model.DebugMode = true;

            SingleProcessTab.Visibility = Visibility.Visible;
            SingleProcessTab.IsSelected = false;
            DevStaus.IsSelected = true;
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (SingleProcessTab == null)
            {
                return;
            }

            // mainWindow_Model.DebugMode = false;

            SingleProcessTab.Visibility = Visibility.Collapsed;
            SingleProcessTab.IsSelected = false;
            DevStaus.IsSelected = true;

        }

        private void WaterInModel_Checked(object sender, RoutedEventArgs e)
        {
            if (configData != null)
            {
                configData.sampleDilType = SampleDilType.WarerIn;
            }
        }

        private void WaterOutModel_Checked(object sender, RoutedEventArgs e)
        {
            if (configData != null)
            {
                configData.sampleDilType = SampleDilType.WaterOut;
            }
        }

        private void WashValve_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = { 0 };
            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);

        }

        private void BodSample_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] data = { PLCConfig.WashValveBit, PLCConfig.SelectValveBit };
                bodHelper.ValveControl(PLCConfig.Valve1Address, data);

                SerialPortHelp serialPortHelp = new SerialPortHelp();
                serialPortHelp.OpenPort();
                serialPortHelp.StartSampleMes();
                serialPortHelp.ClosePort();
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }

        }

        private void BodStand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] data = { PLCConfig.WashValveBit };
                bodHelper.ValveControl(PLCConfig.Valve1Address, data);
                SerialPortHelp serialPortHelp = new SerialPortHelp();
                serialPortHelp.OpenPort();
                serialPortHelp.StartStandMeas();
                serialPortHelp.ClosePort();
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }


        }

        private void BodWash_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                byte[] data = { 0 };

                bodHelper.ValveControl(PLCConfig.Valve1Address, data);

                SerialPortHelp serialPortHelp = new SerialPortHelp();
                serialPortHelp.OpenPort();
                serialPortHelp.StartWash();
                serialPortHelp.ClosePort();
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }

        }

        private async void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                initConfig();

                double hours = RunTimer.Interval.TotalHours;
                if (hours != configData.SpaceHour)
                {
                    RunTimer.Interval = new TimeSpan(configData.SpaceHour, 0, 0);
                }

                double min = UpdataStatusTimer.Interval.TotalMinutes;
                if (min != configData.UpdateStatusInter)
                {
                    UpdataStatusTimer.Interval = new TimeSpan(0, configData.UpdateStatusInter, 0);
                }

                await this.ShowMessageAsync("Tips.", "配置成功。", MessageDialogStyle.Affirmative);
            }
            catch (Exception ex)
            {

                LogUtil.LogError(ex);
            }


        }

        private void ReadConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SampDil.Text = configData.SampDil.ToString();
                SampVol.Text = configData.SampVol.ToString();
                StandDil.Text = configData.StandDil.ToString();
                StandVol.Text = configData.StandVol.ToString();
                EmptyTime.Text = configData.EmptyTime.ToString();
                InietTime.Text = configData.InietTime.ToString();
                PrecipitateTime.Text = configData.PrecipitateTime.ToString();
                sampleSpac.Text = configData.SpaceHour.ToString();
                WarmUpTime.Text = configData.WarmUpTime.ToString();
                WashTimes.Text = configData.WashTimes.ToString();
                SampleScale.Text = configData.SampleScale.ToString();
                UpdataStatus.Text = configData.UpdateStatusInter.ToString();
                StandDilFreq.Text = configData.BodStandFreq.ToString();
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }

        }

        private void ResetConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                configData.SampVol = 50;
                configData.StandDil = 250;
                configData.StandVol = 50;
                configData.EmptyTime = 300;
                configData.InietTime = 60;
                configData.PrecipitateTime = 10;
                configData.SpaceHour = 2;
                configData.WarmUpTime = 60;
                configData.WashTimes = 5;
                configData.SampleScale = 1;
                configData.UpdateStatusInter = 30;
                configData.BodStandFreq = 3;

                ReadConfig_Click(sender, e);

                DebugModel.IsChecked = true;

                OutModel.IsChecked = true;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:指定 IFormatProvider", Justification = "<挂起>")]
        private void GetBodData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] data = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve1Address, data);

                //bodHelper.serialPortHelp.OpenPort();

                float[] BodData = bodHelper.serialPortHelp.BodCurrentData();

                if (BodData == null)
                {

                    return;
                }

                int iLength = data.Length;


                if (iLength != 11)
                {

                    return;
                }

                int iCount = Convert.ToInt32(data[2]);

                byte[] BodValue = { data[3], data[4], data[5], data[6] };
                float Bod = Tool.ToInt32(BodValue)[0];


                //    float Bod = bodHelper.serialPortHelp.BodCurrentData();

                //bodHelper.serialPortHelp.ClosePort();
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:指定 IFormatProvider", Justification = "<挂起>")]
        private async void UpdataData_Click(object sender, RoutedEventArgs e)
        {

            //  bodHelper.StartUv254();
            await this.Dispatcher.InvokeAsync(() => GetData());

            await this.ShowMessageAsync("正在获取传感器数据....", "请2分钟后到参数页面查看.", MessageDialogStyle.Affirmative);

        }

        private async void GetData()
        {
            try
            {
                ushort[] data = bodHelper.finsClient.ReadData(100, 0, 1, PLCConfig.IO);
                ushort io = 0;
                if (data != null && data.Length > 0)
                {
                    io = data[0];
                    LogUtil.Log(io.ToString());
                }

                var n = io | 32;
                io = Convert.ToUInt16(n);
                ushort[] Temp = { io };

                bool success = bodHelper.finsClient.WriteData(100, 0, Temp, PLCConfig.IO);

                if (!success)
                {
                    return;
                }
                int warmUpTime = Convert.ToInt32(WarmUpTime.Text);
                await Task.Delay(warmUpTime * 1000);

                float[] DoDota = await Task.Factory.StartNew(() => bodHelper.GetDoData());

                // float[] DoDota =  bodHelper.GetDoData();
                await Task.Delay(3 * 1000);

                uint[] TurbidityData = await Task.Factory.StartNew(() => bodHelper.GetTurbidityData());
                await Task.Delay(3 * 1000);

                float[] PHData = await Task.Factory.StartNew(() => bodHelper.GetPHData());
                await Task.Delay(3 * 1000);
                ushort[] Uv254Data = await Task.Factory.StartNew(() => bodHelper.GetUv254Data());
                await Task.Delay(3 * 1000);
                ushort[] TempAndHumDada = await Task.Factory.StartNew(() => bodHelper.GetTempAndHumData());
                await Task.Delay(3 * 1000);

                data = bodHelper.finsClient.ReadData(100, 0, 1, PLCConfig.IO);
                if (data != null && data.Length > 0)
                {
                    io = data[0];
                    LogUtil.Log(io.ToString());
                }

                n = io ^ 32;
                io = Convert.ToUInt16(n);
                Temp[0] = io ;

                success = bodHelper.finsClient.WriteData(100, 0, Temp, PLCConfig.IO);

                mainWindow_Model.TemperatureData = DoDota[0];
                mainWindow_Model.DoData = DoDota[1];
                mainWindow_Model.TurbidityData = (float)TurbidityData[0] / 1000;
                mainWindow_Model.PHData = PHData[1];
                mainWindow_Model.Uv254Data = (float)Uv254Data[0] / 100;
                mainWindow_Model.HumidityDataData = (float)TempAndHumDada[0] / 10;
                mainWindow_Model.AirTemperatureData = (float)TempAndHumDada[1] / 10;


                HisDatabase hisDatabase = new HisDatabase();
                hisDatabase.DoData = mainWindow_Model.DoData;
                hisDatabase.DoDataUnit = "mg/L";
                hisDatabase.PHData = mainWindow_Model.PHData;
                hisDatabase.TemperatureData = mainWindow_Model.TemperatureData;
                hisDatabase.TemperatureUnit = "C";
                hisDatabase.TurbidityData = mainWindow_Model.TurbidityData;
                hisDatabase.TurbidityUnit = "mg/L";
                DateTime dateTime = DateTime.Now;
                hisDatabase.CreateDate = dateTime.ToLongDateString();
                hisDatabase.CreateTime = dateTime.ToLongTimeString();
                hisDatabase.Bod = mainWindow_Model.BodData;
                hisDatabase.Uv254Data = mainWindow_Model.Uv254Data;
                hisDatabase.CodData = mainWindow_Model.CodData;

                mainWindow_Model.HisParamData.AddData(hisDatabase);

                HisDataBaseModel model = new HisDataBaseModel();

                hisDatabase.CopyToHisDataBaseModel(model);
                //model.AirTemperature = hisDatabase.AirTemperatureData;
                //model.Bod = hisDatabase.Bod;
                //model.Cod = hisDatabase.CodData;
                //model.CreateDate = hisDatabase.CreateDate;
                //model.CreateTime = hisDatabase.CreateTime;
                //model.DO = hisDatabase.DoData;
                //model.Humidity = hisDatabase.HumidityData;
                //model.id = hisDatabase.Id;
                //model.PH = hisDatabase.PHData;
                //model.RunNum = 0;
                //model.Temperature = hisDatabase.TemperatureData;
                //model.Turbidity = hisDatabase.TurbidityData;
                //model.Uv254 = hisDatabase.Uv254Data;

                //BodSqliteHelp bodSqliteHelp = new BodSqliteHelp();

                await Task.Factory.StartNew(() => BodSqliteHelp.InsertHisBodData(model));

                mainWindow_Model.UpdateSensorStatus();
                UpdataBodStatus();
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }

        }

        private async void FetchWater_Click(object sender, RoutedEventArgs e)
        {
            //LoginDialogSettings loginDialogSettings = new LoginDialogSettings();
            //await this.ShowLoginAsync();
            try
            {
                if (bodHelper.IsSampling)
                {
                    await this.ShowMessageAsync("Tips。", "系统BOD流程正在运行,请稍后操作。", MessageDialogStyle.Affirmative);
                    return;
                }

                bodHelper.WashCistern(configData.WashTimes);

                byte[] data = { PLCConfig.CisternPumpBit };
                bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
                if (!success)
                {

                    await this.ShowMessageAsync("This is the title", "PLC 通讯失败。");
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }


        }

        private async void MeasureBod_Click(object sender, RoutedEventArgs e)
        {
            await Task.Factory.StartNew(() => bodHelper.StartBodDetect(StopCts.Token), StopCts.Token);
        }

        private async void MeasureCod_Click(object sender, RoutedEventArgs e)
        {
            await Task.Factory.StartNew(() => bodHelper.StartCod());

            CodTimer.Interval = new TimeSpan(0, 5, 0);
            CodTimer.Tick += GetCODdata;
            CodTimer.Start();
        }

        private void GetCODdata(object sender, EventArgs e)
        {
            try
            {
                byte bitAdress = 0X00;
                ushort[] Data = bodHelper.finsClient.ReadData(450, bitAdress, 1, PLCConfig.Wr);
                if (Data != null && Data.Length > 0 && Data[0] == 4)
                {
                    CodTimer.Tick -= GetCODdata;
                    CodTimer.Stop();
                    float[] value = bodHelper.finsClient.ReadBigFloatData(432, bitAdress, 2, PLCConfig.Dr);
                    mainWindow_Model.CodData = value[0];
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        private void UpdateDevStatus(object sender, EventArgs e)
        {
            try
            {
                bool IsConnect = bodHelper.finsClient.IsConnect();
                mainWindow_Model.UpdatePlcStatus(IsConnect, true);
                UpdataBodStatus();
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        public void UpdataBodStatus()
        {
            string text = GetBodStatus();
            string AlramInfo = GetAlramInfo();
            string SampleText = GetBodSampleStatus();

            DateTime dateTime = DateTime.Now;

            if (string.IsNullOrEmpty(text))
            {
                mainWindow_Model.DevStatusModel.BOD_Connect_Status = "异常";
                mainWindow_Model.DevStatusModel.BOD_Connect_ImgSource = DevStatusModels.Redpath;

                mainWindow_Model.DevStatusModel.BOD_Run_Status = "异常";
                mainWindow_Model.DevStatusModel.BOD_Run_ImgSource = DevStatusModels.Redpath;

                AlarmData alarmData = new AlarmData();
                alarmData.id = mainWindow_Model.AlramPagerModels.AllAlarmData.Count + 1;
                alarmData.DeviceInfo = 1;
                alarmData.ErrorCode = 1;
                alarmData.ErrorDes = "Bod通讯断开";
                alarmData.CreateDate = dateTime.ToLongDateString();
                alarmData.CreateTime = dateTime.ToLongTimeString();

                mainWindow_Model.AlramPagerModels.AddData(alarmData);

                AlramInfoModel alramInfoModel = new AlramInfoModel();
                alarmData.CopyToAlramInfoModel(alramInfoModel);
                BodSqliteHelp.InsertAlramInfo(alramInfoModel);
            }
            else
            {
                mainWindow_Model.DevStatusModel.BOD_Connect_Status = "正常";
                mainWindow_Model.DevStatusModel.BOD_Connect_ImgSource = DevStatusModels.Greenpath;

                mainWindow_Model.DevStatusModel.BOD_Run_Status = text;
                mainWindow_Model.DevStatusModel.BOD_Run_ImgSource = DevStatusModels.Greenpath;

                mainWindow_Model.DevStatusModel.BOD_Alram_Status = AlramInfo;
                if (AlramInfo != "无告警")
                {
                    mainWindow_Model.DevStatusModel.BOD_Alram_ImgSource = DevStatusModels.Redpath;
                    AlarmData alarmData = new AlarmData();
                    alarmData.id = mainWindow_Model.AlramPagerModels.AllAlarmData.Count + 1;
                    alarmData.DeviceInfo = 1;
                    alarmData.ErrorCode = 2;
                    alarmData.ErrorDes = AlramInfo;
                    alarmData.CreateDate = dateTime.ToLongDateString();
                    alarmData.CreateTime = dateTime.ToLongTimeString();

                    mainWindow_Model.AlramPagerModels.AddData(alarmData);

                    AlramInfoModel alramInfoModel = new AlramInfoModel();
                    alarmData.CopyToAlramInfoModel(alramInfoModel);
                    BodSqliteHelp.InsertAlramInfo(alramInfoModel);
                }
                else
                {
                    mainWindow_Model.DevStatusModel.BOD_Alram_ImgSource = DevStatusModels.Greenpath;
                }

                mainWindow_Model.DevStatusModel.BOD_Sample_Status = SampleText;
            }
        }

        public void HisDataExport_click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isSuccess = false;
                string text = "导出成功.";
                using (SQLiteConnection conn = new SQLiteConnection(BodSqliteHelp.connStr))
                {
                    conn.Open();
                    string sql = "SELECT * FROM HisDataBase";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);

                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();


                    DataTable dt = ds.Tables[0];

                    isSuccess = Tool.DataToExcel(dt, "hisData");
                }

                if (!isSuccess)
                {
                    text = "导出失败.";
                }

                this.ShowMessageAsync("Tips", text, MessageDialogStyle.Affirmative);
            }
            catch (Exception ex)
            {

                LogUtil.LogError(ex);
            }



        }


        #region 程序关闭处理

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                bodHelper.refreshProcess -= new BodHelper.RefreshUI(RefeshProcess);
                bodHelper.refreshStaus -= new BodHelper.RefreshStaus(RefreshStatus);

                bodHelper.refreshData -= new BodHelper.RefreshData(RefreshData);
                bodHelper.refreshProcessStatus -= new BodHelper.RefreshProcessStatus(RefreshProcessStatus);

                timer.Stop();
                UpdataStatusTimer.Stop();
                StandWaterTimer.Stop();
                RunTimer.Stop();
                _loading.animationTimer.Stop();

                this.Dispose();
                System.Environment.Exit(0);

            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    bodHelper.Dispose();
                    StopCts.Dispose();

                    if (kbpr != null)
                    {
                        kbpr.Kill();
                        kbpr.Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~MainWindow()
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

        #endregion

        private async void HisDataTimeSelect(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                await this.Dispatcher.InvokeAsync(() => mainWindow_Model.HisParamData.UpdateDataByDate(HisDataStartPicker.SelectedDate, HisDataEndPicker.SelectedDate));
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);

            }
        }

        private async void AlramDataTimeSelect(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                await this.Dispatcher.InvokeAsync(() => mainWindow_Model.AlramPagerModels.UpdateDataByDate(AlarmDataStartPicker.SelectedDate, AlarmDataEndPicker.SelectedDate));
            }
            catch (Exception ex)
            {

                LogUtil.LogError(ex);
            }
        }


        private void dataGrid1_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (DataGridIsEdit)
            {
                e.Row.IsEnabled = true;
                e.Row.Background = new SolidColorBrush(Color.FromRgb(255, 180, 180));
            }
            else
            {
                e.Row.IsEnabled = false;
            }
        }

        public void AddRow_Click(object sender, RoutedEventArgs e)
        {
            DataGridIsEdit = true;
            dataGrid1.CanUserAddRows = true;

        }

        public void GridSave_Click(object sender, RoutedEventArgs e)
        {
            dataGrid1.UpdateDefaultStyle();
        }


        private void dataGrid1_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            e.Row.Background = new SolidColorBrush(Color.FromRgb(0, 200, 200));
            Tool.HideInputPanel(kbpr);
        }

        private void dataGrid1_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            kbpr = Tool.ShowInputPanel(kbpr);
        }

        private void test11_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                ushort[] data = bodHelper.finsClient.ReadData(100, 0, 1, PLCConfig.IO);

                if (data != null && data.Length > 0)
                {
                    ushort io = data[0];

                    test11.Text = io.ToString();

                }
            }
            catch (Exception ex)
            {

                LogUtil.LogError(ex);
            }

        }
    }
}
