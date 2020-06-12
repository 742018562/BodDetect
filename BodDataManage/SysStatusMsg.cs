using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public class SysStatusMsg
    {

        public SysStatusMsg(int id, string DeviceInfo, string StatusInfo, string dateTime)
        {
            this.id = id;
            this.DeviceInfo = DeviceInfo;
            this.StatusInfo = StatusInfo;
            this.dateTime = dateTime;
        }

        public int id { get; set; }
        public string DeviceInfo { get; set; }
        public string StatusInfo { get; set; }
        public string dateTime { get; set; }
    }
}
