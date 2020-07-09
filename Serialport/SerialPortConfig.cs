using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect
{
    public enum RegStatus 
    {
        Nomale = 4096,
        Samling,
        electric,
        BodStand,
        LastBod = 4102,
        CurrentBod = 4105,
        Dev = 4108
    }

    public enum RegCtrl 
    {
        Stand_Deep = 4128,
        Start_Stand,
        Sample_Dil,
        Start_Sample,
        Stop_All,
        Clear,
        SysReset
    }

    public static class SerialPortConfig
    {
        /// <summary>
        /// 串口地址
        /// </summary>
        public const byte Address = 0X01;
        
        /// <summary>
        /// 读取状态功能码
        /// </summary>
        public const byte Fun_Red_Code = 0X04;

        /// <summary>
        /// 控制Bod的功能码
        /// </summary>
        public const byte Fun_Ctrl_Code = 0X06;

        public const byte RegHighAddress = 0X10;

        ///// <summary>
        ///// 寄存器状态地址
        ///// </summary>
        //public const ushort Reg_Status = 4097;

        ///// <summary>
        ///// 寄存器状态地址
        ///// </summary>
        //public const ushort Reg_Sampling_Status = 4097;

    }
}
