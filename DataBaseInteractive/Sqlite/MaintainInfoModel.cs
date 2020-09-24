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
            object[] paramList = new object[5];

            paramList[0] = Name;
            paramList[1] = AlramId;
            paramList[2] = Info;
            paramList[3] = CreateDate;
            paramList[4] = CreateTime;
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
            maintainInfo.IsInsert = false;
        }
    }
}
