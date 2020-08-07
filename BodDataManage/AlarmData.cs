using BodDetect.DataBaseInteractive.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public class AlarmData
    {

        public AlarmData(int id, int DeviceInfo, int AlramNum, string AlramInfo,bool HasHandle)
        {
            this.id = id;
            this.DeviceInfo = DeviceInfo;
            this.ErrorCode = AlramNum;
            this.ErrorDes = AlramInfo;
            this.HasHandle = HasHandle;
        }

        public int id { get; set; }
        public int DeviceInfo { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorDes { set; get; }
        public Boolean HasHandle { get; set; }

        public string CreateDate { get; set; }
        public string CreateTime { get; set; }

        public void CopyToAlramInfoModel(AlramInfoModel alramInfoModel) 
        {
            alramInfoModel.id = id;
            alramInfoModel.DeviceInfo = DeviceInfo;
            alramInfoModel.ErrorCode = ErrorCode;
            alramInfoModel.ErrorDes = ErrorDes;
            alramInfoModel.HasHandle = HasHandle;
            alramInfoModel.CreateTime = CreateTime;
            alramInfoModel.CreateDate = CreateDate;
        }
    }
}
