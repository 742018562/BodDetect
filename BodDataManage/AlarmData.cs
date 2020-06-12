using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public class AlarmData
    {

        public AlarmData(int id, string DeviceInfo, int AlramNum, string AlramInfo, string dateTime, bool HasHandle)
        {
            this.id = id;
            this.DeviceInfo = DeviceInfo;
            this.AlramNum = AlramNum;
            this.AlramInfo = AlramInfo;
            this.dateTime = dateTime;
            this.HasHandle = HasHandle;
        }

        public int id { get; set; }
        public string DeviceInfo { get; set; }

        public int AlramNum { get; set; }
        public string AlramInfo { set; get; }

        public string dateTime { get; set; }
        public bool HasHandle { get; set; }
    }
}
