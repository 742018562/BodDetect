using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public enum PunpCapType
    {
        oneml,
        fiveml,
        Point2ml
    }

    public enum FunType
    {
        Valve,
        Pump,
    }

    public enum UiType 
    {
        ProcessBar,
        Image,
        Button
    }


    public enum ProcessState 
    {
        ShowData,
        AutoAdd,
        AutoRed,
        Hidden,
        Show
    }



    public static class PLCConfig
    {
        public const byte Dr = 0X82;
        public const byte Wr = 0X31;
        public const byte IO = 0X80;
        public const byte AR = 0X33;


        public const ushort Turbidity_StartAddress = 596;
        public const ushort Turbidity_Count = 2;

        public const ushort DO_StartAddress = 610;
        public const ushort DO_Count = 4;

        public const ushort PH_StartAddress = 600;
        public const ushort PH_Count = 4;

        public const ushort COD_StartAddress = 630;
        public const ushort COD_Count = 1;

        public const ushort Data_Status = 640;

        public const ushort PumpAbsorbAddress_02ml = 234;
        public const ushort PumpAbsorbAddress_1ml = 233;
        public const ushort PumpAbsorb5mlAddress_5ml = 231;
        public const ushort PumpDrainAddress = 232;

        public const ushort Valve1Address = 100;
        public const ushort Valve2Address = 101;

        #region 100管道IO输出
        /// <summary>
        /// 储水池到排口的阀 1表示阀开排水,0表示阀关
        /// </summary>
        public const byte CisternValveBit = 0X00;

        /// <summary>
        /// 清洗液阀（三通）,0表示清洗液开,1表示标液或样液开
        /// </summary>
        public const byte WashValveBit = 0X01;

        /// <summary>
        /// BOD到排口阀（三通）,0表示接排水口，1变送接清洗液循环
        /// </summary>
        public const byte BodDrainValveBit = 0X02;

        /// <summary>
        /// 标液与样液选择阀（三通），1表示样液开，0表示标液开
        /// </summary>
        public const byte SelectValveBit = 0X03;

        /// <summary>
        /// 水样到储水池的泵 这个置1之后判断I0.03是否达到水位，达到水位之后需要置0了
        /// </summary>
        public const byte CisternPumpBit = 0X04;

        /// <summary>
        /// 传感器电源开关,0表示关闭电源,1表示打开电源
        /// </summary>
        public const byte SensorPower = 0X05;

        /// <summary>
        /// 初始化阶段注射泵另外一个阀门对应的三通阀 0表示阀门常闭在空气口，1表示阀门打到清水口
        /// </summary>
        public const byte PrePumpValveAir = 0X07;

        #endregion

        #region 101管道IO输出

        /// <summary>
        /// 沉淀池阀
        /// </summary>
        public const byte DepositValveBit = 0X00;

        /// <summary>
        /// 标定液阀
        /// </summary>
        public const byte StandardValveBit = 0X01;

        /// <summary>
        /// 缓冲液阀
        /// </summary>
        public const byte bufferValveBit = 0X02;

        /// <summary>
        /// 清水阀
        /// </summary>
        public const byte WaterValveBit = 0X03;

        /// <summary>
        /// 废液/空气阀
        /// </summary>
        public const byte AirValveBit = 0X04;


        /// <summary>
        /// 送样池阀（标液）
        /// </summary>
        public const byte NormalValveBit = 0X05;

        /// <summary>
        /// 送样池阀（样液）
        /// </summary>
        public const byte SampleValveBit = 0X06;
        #endregion

    }
}
