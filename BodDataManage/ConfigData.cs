using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public class ConfigData
    {
        /// <summary>
        /// 标液体积
        /// </summary>
        public int StandVol { get; set; }

        /// <summary>
        /// 标液稀释倍数
        /// </summary>
        public int StandDil { get; set; }

        /// <summary>
        /// 样液体积
        /// </summary>
        public int SampVol { get; set; }

        /// <summary>
        /// 样液稀释倍数
        /// </summary>
        public int SampDil { get; set; }

        /// <summary>
        /// 进水超时
        /// </summary>
        public int InietTime { get; set; }

        /// <summary>
        /// 排空超时
        /// </summary>
        public int EmptyTime { get; set; }

        /// <summary>
        /// 沉淀时间
        /// </summary>
        public int PrecipitateTime { get; set; }

        /// <summary>
        /// 预热时间
        /// </summary>
        public int WarmUpTime { get; set; }

        /// <summary>
        /// 润洗次数
        /// </summary>
        public int WashTimes { get; set; }

        /// <summary>
        /// 五餐传感器和BOD采集比例
        /// </summary>
        public int SampleScale { get; set; }


        public int SpaceHour { get; set; }

        public ConfigData() { }
    }
}
