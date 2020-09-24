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
            object[] paramList = new object[6];

            paramList[0] = DeviceInfo;
            paramList[1] = ErrorCode;
            paramList[2] = ErrorDes;
            paramList[3] = HasHandle;
            paramList[4] = CreateDate;
            paramList[5] = CreateTime;
            return paramList;
        }

        public void CopyToAlarmData(AlarmData alarmData)
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
