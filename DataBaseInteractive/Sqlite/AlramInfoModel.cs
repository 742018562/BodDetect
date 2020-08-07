using BodDetect.BodDataManage;
using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.DataBaseInteractive.Sqlite
{
    public class AlramInfoModel
    {
        public int id { get; set; }
        public int DeviceInfo { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorDes { get; set; }
        public Boolean HasHandle { get; set; }
        public string CreateDate { get; set; }
        public string CreateTime { get; set; }

        public object[] GetObjectList()
        {
            object[] paramList = new object[7];

            paramList[0] = id;
            paramList[1] = DeviceInfo;
            paramList[2] = ErrorCode;
            paramList[3] = ErrorDes;
            paramList[4] = HasHandle;
            paramList[5] = CreateDate;
            paramList[6] = CreateTime;
            return paramList;
        }

        public void CopyToSysStatusData(AlarmData alarmData)
        {
            alarmData.id = id;
            alarmData.ErrorCode = ErrorCode;
            alarmData.ErrorDes = ErrorDes;
            alarmData.DeviceInfo = DeviceInfo;
            alarmData.CreateDate = CreateDate;
            alarmData.CreateTime = CreateTime;
            alarmData.HasHandle = HasHandle;
        }
    }
}
