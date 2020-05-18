using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Arthas.Controls;
using BodDetect.BodDataManage;
using BodDetect.Event;
using BodDetect.UDP;

namespace BodDetect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        BodHelper bodHelper;
        public Dictionary<byte, MetroProgressBar> metroProgressBars = new Dictionary<byte, MetroProgressBar>();

        public Dictionary<byte, EventHandler> ProcessHandlers = new Dictionary<byte, EventHandler>();
        //readonly FinsClient finsClient;

        DispatcherTimer timer = new DispatcherTimer();


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


            ProcessHandlers.Add(PLCConfig.WaterValveBit,RefeshWaterProcessEvent);
            ProcessHandlers.Add(PLCConfig.bufferValveBit, RefeshCachProcessEvent);
            ProcessHandlers.Add(PLCConfig.StandardValveBit, RefeshStandProcessEvent);
            ProcessHandlers.Add(Convert.ToByte((ushort)100), RefeshPumpProcessEvent);
            ProcessHandlers.Add(PLCConfig.AirValveBit, RefeshAirProcessEvent);
            ProcessHandlers.Add(PLCConfig.NormalValveBit, RefeshNormalProcessEvent);
            ProcessHandlers.Add(PLCConfig.SampleValveBit, RefeshSampleProcessEvent);
            ProcessHandlers.Add(PLCConfig.DepositValveBit, RefeshStoreWaterProcessEvent);
            ProcessHandlers.Add(Convert.ToByte((ushort)101), RefeshWaterSampleProcessEvent);
        }

        private void MetroButton_Click(object sender, RoutedEventArgs e)
        {
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
                int port = Convert.ToInt32(Port_TextBox.Text);

                bodHelper = new BodHelper(ip, port);
                bool success = bodHelper.ConnectPlc();

                bodHelper.refreshProcess = new BodHelper.RefreshUI(RefeshProcess);
                bodHelper.mainWindow = this;

            }
            catch (Exception)
            {
                MessageBox.Show("连接PLC异常!");
            }

        }

        private void MetroButton_Click_2(object sender, RoutedEventArgs e)
        {

            float[] floatData = bodHelper.GetDoData();

            floatData = bodHelper.GetPHData();

            uint[] value = bodHelper.GetTurbidityData();

            byte[] IOCmd = { PLCConfig.WashValveBit, PLCConfig.BodDrainValveBit};

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


        private void Sampling_Click_3(object sender, RoutedEventArgs e)
        {
            Thread BodDectThread = new Thread(new ThreadStart(bodHelper.StartBodDetect));
            BodDectThread.IsBackground = true;
            BodDectThread.Start();
        }


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

        private void StoreValve_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] data = { PLCConfig.DepositValveBit };
                bodHelper.ValveControl(PLCConfig.Valve2Address, data);
            }
            catch (Exception)
            {
                StoreValve.IsChecked = false;
                MessageBox.Show(" 沉淀池阀门打开失败.");
            }

        }
    }
}
