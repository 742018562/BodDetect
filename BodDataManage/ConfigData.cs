using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public class ConfigData
    {
        public int StandVol { get; set; }
        public int StandDil { get; set; }
        public int SampVol { get; set; }
        public int SampDil { get; set; }

        public int InietTime { get; set; }
        public int EmptyTime { get; set; }
        public int PrecipitateTime { get; set; }

        public ConfigData() { }
    }
}
