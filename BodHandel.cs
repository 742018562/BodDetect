using BodDetect.BodDataManage;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BodDetect
{
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow, IDisposable
    {
        public async void SetBodStand()
        {
            ushort standData = 20;
            if (!string.IsNullOrEmpty(SetStandData.Text))
            {
                standData = Convert.ToUInt16(SetStandData.Text);
            }

            bool success = bodHelper.serialPortHelp.SetStandDeep(standData);

            if (success)
            {
                string msg = "浓度值为" + standData.ToString();
                await this.ShowMessageAsync("标准浓度设置成功。", msg, MessageDialogStyle.Affirmative);
            }
            else 
            {
                await this.ShowMessageAsync("标准浓度设置失败。", "请查看BOD状态是否有报警信息.", MessageDialogStyle.Affirmative);
            }
        }


        public async void SetBodStand_click(object sender, EventArgs e) 
        {

            if (!serialPortIsAlive())
            {
                return;
            }

            await this.Dispatcher.InvokeAsync(() => SetBodStand());
        }

        public async void SetBodSampleDil() 
        {
            ushort standData = 1;
            if (!string.IsNullOrEmpty(SetSampleDli.Text))
            {
                standData = Convert.ToUInt16(SetSampleDli.Text);
            }

            bool success = bodHelper.serialPortHelp.SetStandDeep(standData);

            if (success)
            {
                string msg = "样液稀释倍数为:" + standData.ToString();
                await this.ShowMessageAsync("样液稀释倍数设置成功。", msg, MessageDialogStyle.Affirmative);
            }
            else
            {
                await this.ShowMessageAsync("样液稀释倍数设置失败。", "请查看BOD状态是否有报警信息.", MessageDialogStyle.Affirmative);
            }
        }

        public async void SetBodSampleDil_click(object sender, EventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.ShowMessageAsync("开始对BOD进行标定。", "请稍等10分钟后查看标定状态.", MessageDialogStyle.Affirmative);


            await this.Dispatcher.InvokeAsync(() => SetBodSampleDil());


        }

        public async void StartBodStand_click(object sender, RoutedEventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            if (bodHelper.IsSampling)
            {
                await this.ShowMessageAsync("Tips。", "系统BOD流程正在运行,请稍后操作。", MessageDialogStyle.Affirmative);
                return;
            }

            await this.Dispatcher.InvokeAsync(() => StartBodStand());

        }

        public async void StartBodStand() 
        {
            List<byte> TempData = new List<byte>();
            TempData.Add(PLCConfig.WashValveBit);
            bodHelper.ValveControl(PLCConfig.Valve1Address, TempData.ToArray());
            bool success = bodHelper.serialPortHelp.StartStandMeas();
            if (success)
            {
                await this.ShowMessageAsync("BOD标定成功。", "可以进行后续步骤。", MessageDialogStyle.Affirmative);
            }
            else
            {
                await this.ShowMessageAsync("BOD标定失败。", "请重新标定.", MessageDialogStyle.Affirmative);
            }
        }

        public async void StartBodSample_click(object sender, RoutedEventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.Dispatcher.InvokeAsync(() => StartBodSample());
        }

        public async void StartBodSample() 
        {
            for (int i = 0; i < 10; i++)
            {
                bodHelper.serialPortHelp.SetSampleDil(1);
                List<byte> TempData = new List<byte>();
                TempData.Add(PLCConfig.WashValveBit);
                TempData.Add(PLCConfig.SelectValveBit);
                bodHelper.ValveControl(PLCConfig.Valve1Address, TempData.ToArray());
                bool success = bodHelper.serialPortHelp.StartSampleMes();

                await Task.Delay(8 * 60 * 1000 + 10000);

                StopBodAndWash();

                await Task.Delay(30 * 60 * 1000);
            }



            

            //if (success)
            //{
            //    await this.ShowMessageAsync("样品测量成功。", "可以获取BOD的测量数据。", MessageDialogStyle.Affirmative);
            //}
            //else
            //{
            //    await this.ShowMessageAsync("样品测量失败。", "请重新测量.", MessageDialogStyle.Affirmative);
            //}
        }

        public async void StopBodAndWash(object sender, RoutedEventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.Dispatcher.InvokeAsync(() => StopBodAndWash());
        }

        public async void StopBodAndWash() 
        {
            byte[] tempvalue = { 0 };
            bodHelper.ValveControl(PLCConfig.Valve1Address, tempvalue);
            await Task.Delay(200); 

            bool success = bodHelper.serialPortHelp.StartWash();
            //if (success)
            //{
            //    await this.ShowMessageAsync("停止测量成功开始清洗状态。", "请等待BOD清洗。", MessageDialogStyle.Affirmative);
            //}
            //else
            //{
            //    await this.ShowMessageAsync("停止测量失败。", "请查看BOD状态是否有报警信息.", MessageDialogStyle.Affirmative);
            //}
        }

        public async void ClearAlram_click(object sender, RoutedEventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.Dispatcher.InvokeAsync(() => ClearAlram());
        }

        public async void ClearAlram() 
        {
            ushort data = 1;
            bool success = bodHelper.serialPortHelp.ClearAlram(data);
            if (success)
            {
                await this.ShowMessageAsync("清除警告完成。", "", MessageDialogStyle.Affirmative);
            }
            else
            {
                await this.ShowMessageAsync("清除警告失败。", "请查看BOD有报警信息.", MessageDialogStyle.Affirmative);
            }
        }

        public async void ResetBod_click(object sender, RoutedEventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.Dispatcher.InvokeAsync(() => ResetBod());
        }

        public async void ResetBod() 
        {
            bool success = bodHelper.serialPortHelp.SysReset();
            if (success)
            {
                await this.ShowMessageAsync("系统复位完成。", "", MessageDialogStyle.Affirmative);
            }
            else
            {
                await this.ShowMessageAsync("系统复位失败。", "请查看BOD有报警信息.", MessageDialogStyle.Affirmative);
            }
        }


        public async void GetBodStatus_click(object sender, EventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.Dispatcher.InvokeAsync(() => GetBodStatus());
        }

        public string GetBodStatus() 
        {
            int status = bodHelper.serialPortHelp.GetBodStatus();
            string text = Tool.GetBodStatusToString(status);

            BodStatus_text.Text = text;
            return text;
        }

        public async void GetBodSampleStatus_click(object sender, EventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.Dispatcher.InvokeAsync(() => GetBodSampleStatus());
        }

        public string GetBodSampleStatus() 
        {
            int status = bodHelper.serialPortHelp.GetSamplingStatus();
            string text = "";
            switch (status)
            {
                case 0:
                    text = "未完成";
                    break;
                case 1:
                    text = "做样完成";
                    break;
                case 2:
                    text = "做标完成";
                    break;
                case 3:
                    text = "做样完成,做标完成";
                    break;
                default:
                    break;
            }

            SampleStatus_box.Text = text;
            return text;
        }

        public async void GetElepot_click(object sender, EventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.Dispatcher.InvokeAsync(() => GetElepot());
        }

        public void GetElepot() 
        {
            float data = bodHelper.serialPortHelp.GetElePot();

            ElePot_textbox.Text = data.ToString("F2");

        }

        public async void GetAlramInfo_click(object sender, EventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.Dispatcher.InvokeAsync(() => GetAlramInfo());
        }

        public string GetAlramInfo() 
        {
            int status = bodHelper.serialPortHelp.GetAlarmStatus();

            int temp = status & 1;
            string text = "无告警";
            if (temp > 0) 
            {
                text += " 高温报警 ";
            }

            temp = status & 2;
            if (temp > 0) 
            {
                text += " 第一电极报警 ";
            }

            temp = status & 4;
            if (temp > 0) 
            {
                text += " 第二电极报警 ";
            }

            AlramInfo_textbox.Text = text;
            return text;
        }

        public async void GetBodStandData_click(object sender, RoutedEventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.Dispatcher.InvokeAsync(() => GetBodStandData());
        }

        public void GetBodStandData() 
        {
            float[] data = bodHelper.serialPortHelp.GetBodStandData();

            BodStandData.Content = data[0].ToString("F2");

            StandElePotDrop_lab.Content = data[1].ToString("F2");

        }

        public async void GetBodCurrentData_click(object sender, RoutedEventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await this.Dispatcher.InvokeAsync(() => GetBodCurrentData());
        }

        public void GetBodCurrentData() 
        {
            try
            {
                float[] data = bodHelper.serialPortHelp.BodCurrentData();

                CurrentData_lab.Content = data[0].ToString("F2");

                CurrentElePotDrop_lab.Content = data[1].ToString("F2");
            }
            catch (Exception)
            {

            }

        }

        public bool serialPortIsAlive() 
        {
            if (bodHelper.serialPortHelp == null || !bodHelper.serialPortHelp.IsAlive())
            {
                this.ShowMessageAsync("BOD串口未打开。", "请打开串口后重新设置。", MessageDialogStyle.Affirmative);
                return false;
            }
            return true;
        }

        public async void StartBodStandWater_Click(object sender, RoutedEventArgs e) 
        {
            if (bodHelper.IsSampling)
            {
                await this.ShowMessageAsync("Tips。", "系统BOD流程正在运行,请稍后操作。", MessageDialogStyle.Affirmative);
                return;
            }

            await Task.Factory.StartNew(() => bodHelper.StartBodStandWater());
        }

        public async void StartBodSampleWater_Click(object sender, RoutedEventArgs e) 
        {
            if (bodHelper.IsSampling) 
            {
                await this.ShowMessageAsync("Tips。", "系统BOD流程正在运行,请稍后操作。", MessageDialogStyle.Affirmative);
                return ;
            }

            await Task.Factory.StartNew(() => bodHelper.StartBodSample());

        }

    }
}
