using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

            await Task.Factory.StartNew(() => SetBodStand());
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


            await Task.Factory.StartNew(() => SetBodSampleDil());


        }

        public async void StartBodStand_click(object sender, RoutedEventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await Task.Factory.StartNew(() => StartBodStand());

        }

        public async void StartBodStand() 
        {
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
            await Task.Factory.StartNew(() => StartBodSample());
        }

        public async void StartBodSample() 
        {
            bool success = bodHelper.serialPortHelp.StartSampleMes();
            if (success)
            {
                await this.ShowMessageAsync("样品测量成功。", "可以获取BOD的测量数据。", MessageDialogStyle.Affirmative);
            }
            else
            {
                await this.ShowMessageAsync("样品测量失败。", "请重新测量.", MessageDialogStyle.Affirmative);
            }
        }

        public async void StopBodAndWash(object sender, RoutedEventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await Task.Factory.StartNew(() => StopBodAndWash());
        }

        public async void StopBodAndWash() 
        {
            bool success = bodHelper.serialPortHelp.StartWash();
            if (success)
            {
                await this.ShowMessageAsync("停止测量成功开始清洗状态。", "请等待BOD清洗。", MessageDialogStyle.Affirmative);
            }
            else
            {
                await this.ShowMessageAsync("停止测量失败。", "请查看BOD状态是否有报警信息.", MessageDialogStyle.Affirmative);
            }
        }

        public async void ClearAlram_click(object sender, RoutedEventArgs e) 
        {
            if (!serialPortIsAlive())
            {
                return;
            }
            await Task.Factory.StartNew(() => ClearAlram());
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
            await Task.Factory.StartNew(() => ResetBod());
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


        public bool serialPortIsAlive() 
        {
            if (bodHelper.serialPortHelp == null || !bodHelper.serialPortHelp.IsAlive())
            {
                this.ShowMessageAsync("BOD串口未打开。", "请打开串口后重新设置。", MessageDialogStyle.Affirmative);
                return false;
            }
            return true;
        }

    }
}
