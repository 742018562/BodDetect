using System;
using System.Collections.Generic;
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
using BodDetect.Event;
using BodDetect.UDP;
using MahApps.Metro.Controls.Dialogs;

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

        //readonly FinsClient finsClient;

        DispatcherTimer timer = new DispatcherTimer();

        DispatcherTimer BodTimer = new DispatcherTimer();

        DispatcherTimer WaterSampleTimer = new DispatcherTimer();

        DispatcherTimer RunTimer = new DispatcherTimer();


        private BodHelper bodHelper;
        public BodData bodData = new BodData();

        public ProgressDialogController progressDialog;

        private CancellationTokenSource StopCts = new CancellationTokenSource();

        MainWindow_Model mainWindow_Model = new MainWindow_Model();

        ConfigData configData = new ConfigData();
        private bool disposedValue;

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


            init();

            this.DataContext = mainWindow_Model;

        }

        public void init()
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    SysStatusList.Items.Add(new SysStatusMsg(i, "test0", "test1", "test2"));
                }

                for (int i = 0; i < 10; i++)
                {
                    AlarmList.Items.Add(new AlarmData(i, "test0", i + 10, "test1", "test2", true));
                }

                for (int i = 0; i < 10; i++)
                {
                    HisAlarmList.Items.Add(new AlarmData(i, "test0", i + 10, "test1", "test2", true));
                }

                logList.Items.Add("test");

                string ip = IP_textbox.Text;

                string[] value = ip.Split('.');
                if (value.Length < 4)
                {
                    this.ShowMessageAsync("Error","异常ip!");
                }
                int port = Convert.ToInt32(Port_TextBox.Text,10);

                bodHelper = new BodHelper(ip, port);
                bodHelper.Init();

                bodHelper.refreshProcess = new BodHelper.RefreshUI(RefeshProcess);
                bodHelper.refreshStaus = new BodHelper.RefreshStaus(RefreshStatus);

                bodHelper.refreshData = new BodHelper.RefreshData(RefreshData);
                bodHelper.refreshProcessStatus = new BodHelper.RefreshProcessStatus(RefreshProcessStatus);

                bodHelper.mainWindow = this;
            }
            catch (Exception)
            {
                this.ShowMessageAsync("Error", "连接PLC异常!");
            }
        }

        private void MetroButton_Click(object sender, RoutedEventArgs e)
        {

            float[] DoDota = bodHelper.GetDoData();
            uint[] TurbidityData = bodHelper.GetTurbidityData();
            float[] PHData = bodHelper.GetPHData();
            ushort[] CODData = bodHelper.GetCodData();

            bodData.TemperatureData = DoDota[0];
            bodData.DoData = DoDota[1];
            bodData.TurbidityData = (float)TurbidityData[0] / 1000;
            bodData.PHData = PHData[1];
            bodData.CodData = (float)CODData[0] / 100;
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

        private void MetroButton_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                string ip = IP_textbox.Text;

                string[] value = ip.Split('.');
                if (value.Length < 4)
                {
                     this.ShowMessageAsync("Error", "异常ip!");
                }

                int port = Convert.ToInt32(Port_TextBox.Text);
                bool success = bodHelper.ConnectPlc();

                if (success) 
                {
                     this.ShowMessageAsync("与PLC通讯", "连接成功！",MessageDialogStyle.Affirmative);
                }               
            }
            catch (Exception)
            {
                this.ShowMessageAsync("Error", "连接PLC异常!");
            }

        }

        private void RefreshData(BodData data)
        {
            mainWindow_Model.BodData = data.Bod;
            mainWindow_Model.CodData = data.CodData;
            mainWindow_Model.DoData = data.DoData;
            mainWindow_Model.PHData = data.PHData;
            mainWindow_Model.TemperatureData = data.TemperatureData;
            mainWindow_Model.TurbidityData = data.TurbidityData;
            mainWindow_Model.Uv254Data = data.Uv254Data;
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

            //Dictionary<UInt16, UInt16> dic = new Dictionary<ushort, ushort>();

            //dic.Add(32300, 3);
            //dic.Add(32301, 4);
            //dic.Add(32302, 4);
            //dic.Add(32303, 1);
            //dic.Add(32304, 2);

            //byte bitAdress = 0X00;

            //ushort[] value = { 3, 4, 4, 1, 2 };
            //finsClient.WriteData(32300, bitAdress, value, Dr);

            //bitAdress = 0X08;
            //byte[] wValue = { 0X01 };
            //finsClient.WriteBitData(0, bitAdress, wValue, Wr);

            //bitAdress = 0X00;
            //float[] floatData = finsClient.ReadBigFloatData(596, bitAdress, 2, Dr);
        }

        //private void Sampling_Click_3(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("sssss");
        //    return;

        //    Thread BodDectThread = new Thread(new ThreadStart(bodHelper.StartBodDetect));
        //    BodDectThread.IsBackground = true;
        //    BodDectThread.Start();
        //}

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
            catch (Exception)
            {

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

            SysStaus.Content = text;
        }

        #endregion

        //private void StoreValve_Checked(object sender, RoutedEventArgs e)
        //{
        //    MetroSwitch a = (MetroSwitch)sender;
        //    a.IsChecked = false;

        //    return;

        //    try
        //    {

        //        if (bodHelper.IsSampling == true)
        //        {
        //            MessageBox.Show(" 现在正在采样过程中,禁止相关操作.", "提示", MessageBoxButton.OK);
        //            return;
        //        }

        //        if (StoreValve.IsChecked == true)
        //        {
        //            bool hasChecked = ValveDic.Any(t => t.Value.IsChecked == true && t.Key != PLCConfig.DepositValveBit);

        //            if (hasChecked)
        //            {
        //                if (MessageBox.Show("有其他的阀门打开,是否关闭其他阀门", "提示", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
        //                {
        //                    foreach (var item in ValveDic)
        //                    {
        //                        if (item.Key != PLCConfig.DepositValveBit)
        //                        {
        //                            item.Value.IsChecked = false;
        //                        }
        //                    }

        //                    byte[] data = { PLCConfig.DepositValveBit };
        //                    bodHelper.ValveControl(PLCConfig.Valve2Address, data);
        //                }
        //            }

        //        }
        //    }
        //    catch (Exception)
        //    {
        //        StoreValve.IsChecked = false;
        //        MessageBox.Show(" 沉淀池阀门打开失败.", "提示", MessageBoxButton.OK);
        //    }

        //}

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
            catch (Exception)
            {
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
            catch (Exception)
            {
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
            catch (Exception)
            {
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
            catch (Exception)
            {

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
            catch (Exception)
            {

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
            catch (Exception)
            {

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
            catch (Exception)
            {

                throw;
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
            catch (Exception)
            {

                throw;
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
            catch (Exception)
            {

                throw;
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
            catch (Exception)
            {


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

            if (!success)
            {
                MessageBox.Show(" 阀门打开失败.", "提示", MessageBoxButton.OK);
            }

            success = bodHelper.PunpAbsorb(punpCapType);

            if (!success)
            {
                MessageBox.Show(" 注射泵抽水失败.", "提示", MessageBoxButton.OK);
            }

            Thread.Sleep(5000);

            success = bodHelper.ValveControl(address[1], data[1]);
            if (!success)
            {
                MessageBox.Show(" 阀门打开失败.", "提示", MessageBoxButton.OK);
            }
            success = bodHelper.PumpDrain();

            if (!success)
            {
                MessageBox.Show(" 注射泵放水失败.", "提示", MessageBoxButton.OK);
            }

            Thread.Sleep(5000);
        }

        private void PumpCache_ButtonClick(object sender, EventArgs e)
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

                byte[] StandValve = { PLCConfig.bufferValveBit };

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
            catch (Exception)
            {


            }
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


                byte[] StandValve = { PLCConfig.WaterValveBit };

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

                byte[] data1 = { 0 };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data1);
            }
            catch (Exception)
            {


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
            catch (Exception)
            {


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
            catch (Exception)
            {


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
            catch (Exception)
            {


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
            catch (Exception)
            {


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
            catch (Exception)
            {


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

        private void StandDilution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cap = StandAll.Text;
                string zoom = Dilution.Text;

                if (string.IsNullOrEmpty(cap))
                {
                    MessageBox.Show(" 请输入抽取容量.", "提示", MessageBoxButton.OK);
                }

                int capData = Convert.ToInt32(cap);
                int zoomdata = Convert.ToInt32(zoom);

                int waterData = capData * (zoomdata - 1);

                int times = capData / 5;

                int extraTimes = capData % 5;

                byte[] StandValve = { PLCConfig.StandardValveBit };

                byte[] StandBodValve = { PLCConfig.NormalValveBit };

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
            catch (Exception)
            {


            }
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
            catch (Exception)
            {


            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Tool.ShowInputPanel();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Tool.HideInputPanel();
        }

        private void PreButton_Click(object sender, RoutedEventArgs e)
        {
            if (bodHelper == null || bodHelper.finsClient == null || bodHelper.finsClient == null)
            {
                MessageBox.Show(" PLC未连接请连接.", "提示", MessageBoxButton.OK);
            }

            bodHelper.PreInit();
        }

        private void PreButton2ml_Click(object sender, RoutedEventArgs e)
        {
            if (bodHelper == null || bodHelper.finsClient == null || bodHelper.finsClient == null)
            {
                MessageBox.Show(" PLC未连接请连接.", "提示", MessageBoxButton.OK);
            }

            byte[] StandBodValve = { PLCConfig.NormalValveBit };

            byte[] waterValve = { PLCConfig.WaterValveBit };

            byte[] AirValve = { PLCConfig.AirValveBit };

            List<byte[]> data = new List<byte[]>();
            List<ushort> address = new List<ushort>();
            data.Add(waterValve);
            data.Add(StandBodValve);

            address.Add(PLCConfig.Valve2Address);
            address.Add(PLCConfig.Valve2Address);

            PumpProcess(data, address, PunpCapType.Point2ml);

            data[0] = AirValve;

            PumpProcess(data, address, PunpCapType.fiveml);

        }

        private void Pre2ml_Click(object sender, RoutedEventArgs e)
        {
            bodHelper.PunpAbsorb(PunpCapType.Point2ml);
        }

        private void Pumpdrain2ml_Click(object sender, RoutedEventArgs e)
        {
            bodHelper.PumpDrain();
        }

        private void PrePumpAri_Click(object sender, RoutedEventArgs e)
        {
            //bodHelper.ChangePunpValve(PumpValveType.pre);
            //bodHelper.PrePumpCtrl(PrePumpWork.preAbsorb);


            byte[] data = { PLCConfig.AirValveBit };

            bodHelper.ValveControl(PLCConfig.Valve2Address, data);
            data[0] = 0X01;

            bodHelper.ChangePunpValve(PumpValveType.pre);
            bodHelper.finsClient.WriteBitData(1, 0, data, PLCConfig.Wr);

            //bodHelper.ValveControl(PLCConfig.Valve1Address, PLCConfig.PrePumpValveAir, data);
            bodHelper.PrePumpCtrl(PrePumpWork.preAbsorb);
            Thread.Sleep(5000);
            bodHelper.PumpDrain();

        }

        private void PrePumpwater_Click(object sender, RoutedEventArgs e)
        {
            //bodHelper.ChangePunpValve(PumpValveType.pre);
            //bodHelper.PrePumpCtrl(PrePumpWork.preDrain);

            byte[] data = { PLCConfig.WaterValveBit };

            bodHelper.ValveControl(PLCConfig.Valve2Address, data);
            data[0] = 0X01;

            bodHelper.ChangePunpValve(PumpValveType.pre);

            bodHelper.finsClient.WriteBitData(0, 15, data, PLCConfig.Wr);

            //bodHelper.ValveControl(PLCConfig.Valve1Address, PLCConfig.PrePumpValveAir, data);
            bodHelper.PrePumpCtrl(PrePumpWork.preAbsorb);

            Thread.Sleep(5000);
            bodHelper.PumpDrain();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //this.WindowState = WindowState.Maximized;

            //this.WindowStyle = WindowStyle.None;

            ////this.ResizeMode = ResizeMode.NoResize;


            //this.Topmost = true;

            this.Background = new SolidColorBrush( Color.FromRgb((byte)255, (byte)255, (byte)255));
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if ((bool)toggleButton.IsChecked)
            {
                StartBod(sender, e);
            }
            else
            {

                AbortBod(sender, e);
            }

        }

        public async void BodRun(object sender, EventArgs e)
        {
            //try
            //{

            //    if (BodDetectRun != null && BodDetectRun.IsAlive)
            //    {
            //        BodDetectRun.Abort();
            //    }

            //}
            //catch (Exception)
            //{

            //}
            await Task.Factory.StartNew(() => bodHelper.StartBodDetect(StopCts.Token), StopCts.Token);
            


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
                    RunTimer.Tick += BodRun;
                    RunTimer.Interval = new TimeSpan(0, SpaceHour, 0);
                    RunTimer.Start();

                    BodRun(sender, e);

                    _loading.Visibility = Visibility.Visible;
                }
                else
                {
                    bodHelper.manualevent.Set();
                    _loading.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {

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


            configData.SampDil = Convert.ToInt32(SampDil.Text);
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
            progressDialog = await this.ShowProgressAsync("Please wait...", "正在重置运行流程,请稍等...",true);

            await Task.Run(StopBod);
        }

        public async void StopBod() 
        {
            Thread.Sleep(10000);
            while (true) 
            {
                if (!bodHelper.IsSampling) 
                {
                    break;
                }
            }
            
            progressDialog.SetMessage("运行流程重置完成...");
            Thread.Sleep(2000);
            await  progressDialog.CloseAsync();
        }

        private void DrainEmpty_Click(object sender, RoutedEventArgs e)
        {
            //byte[] Valves = { PLCConfig.NormalValveBit, PLCConfig.SampleValveBit };

            //List<byte[]> data = new List<byte[]>();
            //byte[] tem = { PLCConfig.SampleValveBit };
            //byte[] tem1 = { PLCConfig.AirValveBit };
            //data.Add(tem);
            //data.Add(tem1);

            //List<ushort> Address = new List<ushort>();
            //Address.Add(PLCConfig.Valve2Address);
            //Address.Add(PLCConfig.Valve2Address);

            //foreach (var item in Valves)
            //{
            //    data[0][0] = item;
            //    for (int i = 0; i < 10; i++)
            //    {
            //        PumpProcess(data, Address, PunpCapType.fiveml);
            //    }
            //}

            byte[] data1 = { PLCConfig.CisternValveBit };
            bodHelper.ValveControl(PLCConfig.Valve1Address, data1);
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
            catch (Exception)
            {

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
            catch (Exception)
            {


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
            catch (Exception)
            {


            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:指定 IFormatProvider", Justification = "<挂起>")]
        private void GetBodData_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter streamWriter = File.CreateText("D:\\test1111.txt");
            try
            {
                byte[] data = { 0 };



                bodHelper.ValveControl(PLCConfig.Valve1Address, data);

                //bodHelper.serialPortHelp.OpenPort();
                streamWriter.WriteLine("开始读数据");

                bodHelper.serialPortHelp.ReadBodFun(RegStatus.CurrentBod, 3);

                data = bodHelper.serialPortHelp.ReadData();
                streamWriter.WriteLine("读取数据完成");

                if (data == null)
                {
                    streamWriter.WriteLine("data == null");

                    return;
                }

                int iLength = data.Length;
                foreach (var item in data)
                {
                    streamWriter.WriteLine(item.ToString());
                }

                if (iLength != 11)
                {

                    streamWriter.WriteLine("iLength != 11");
                    return;
                }

                int iCount = Convert.ToInt32(data[2]);

                byte[] BodValue = { data[3], data[4], data[5], data[6] };
                float Bod = Tool.ToInt32(BodValue)[0];


            //    float Bod = bodHelper.serialPortHelp.BodCurrentData();



                streamWriter.WriteLine(Bod.ToString());
                streamWriter.Close();

                //bodHelper.serialPortHelp.ClosePort();
            }
            catch (Exception ex)
            {
                streamWriter.WriteLine(ex.Message);
                streamWriter.WriteLine(ex.StackTrace);

                streamWriter.Close();
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:指定 IFormatProvider", Justification = "<挂起>")]
        private void UpdataData_Click(object sender, RoutedEventArgs e)
        {

            byte[] Temp = { PLCConfig.SensorPower };
            bool success = bodHelper.ValveControl(100, Temp);
            if (!success)
            {
                return;
            }
            //int warmUpTime = Convert.ToInt32(WarmUpTime.Text);
            //Thread.Sleep(warmUpTime * 1000);
            float[] DoDota = bodHelper.GetDoData();
            uint[] TurbidityData = bodHelper.GetTurbidityData();
            float[] PHData = bodHelper.GetPHData();
            ushort[] CODData = bodHelper.GetCodData();

            mainWindow_Model.TemperatureData = DoDota[0];
            mainWindow_Model.DoData = DoDota[1];
            mainWindow_Model.TurbidityData = (float)TurbidityData[0] / 1000;
            mainWindow_Model.PHData = PHData[1];
            mainWindow_Model.CodData = (float)CODData[0] / 100;
            mainWindow_Model.Uv254Data = bodData.Uv254Data;

        }

        private async void FetchWater_Click(object sender, RoutedEventArgs e)
        {
             //LoginDialogSettings loginDialogSettings = new LoginDialogSettings();
             //await this.ShowLoginAsync();
             
            bodHelper.WashCistern(configData.WashTimes);

            byte[] data = { PLCConfig.CisternPumpBit };
            bool success = bodHelper.ValveControl(PLCConfig.Valve1Address, data);
            if (!success)
            {

                await this.ShowMessageAsync("This is the title", "PLC 通讯失败。");
            }
        }


        private async void MeasureBod_Click(object sender, RoutedEventArgs e) 
        {
            await Task.Factory.StartNew(()=> bodHelper.StartBodDetect(StopCts.Token), StopCts.Token);
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
                BodTimer.Stop();
                WaterSampleTimer.Stop();
                RunTimer.Stop();
                _loading.animationTimer.Stop();

                this.Dispose();
                System.Environment.Exit(0);

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
                    // TODO: 释放托管状态(托管对象)
                    bodHelper.Dispose();
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
    }
}
