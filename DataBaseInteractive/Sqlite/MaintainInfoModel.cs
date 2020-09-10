using BodDetect.BodDataManage;
using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.DataBaseInteractive.Sqlite
{
    public class MaintainInfoModel
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string AlramId { get; set; }
        public string Info { get; set; }
        public string CreateDate { get; set; }
        public string CreateTime { get; set; }

        public object[] GetObjectList()
        {
            object[] paramList = new object[7];

            paramList[0] = id;
            paramList[1] = Name;
            paramList[2] = AlramId;
            paramList[3] = Info;
            paramList[4] = CreateDate;
            paramList[5] = CreateTime;
            return paramList;
        }

        public void CopyToAlarmData(MaintainInfo maintainInfo)
        {
            maintainInfo.id = id;
            maintainInfo.Name = Name;
            maintainInfo.AlramId = AlramId;
            maintainInfo.Info = Info;
            maintainInfo.CreateDate = CreateDate;
            maintainInfo.CreateTime = CreateTime;
        }
    }
}
