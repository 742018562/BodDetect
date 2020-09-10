using BodDetect.DataBaseInteractive.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public class MaintainInfo
    {
        public MaintainInfo() { }

        public int id { get; set; }
        public string Name { get; set; }
        public string AlramId { get; set; }
        public string Info { get; set; }
        public string CreateDate { get; set; }
        public string CreateTime { get; set; }
        public void CopyToMaintainInfoModel(MaintainInfoModel maintainInfoModel)
        {
            maintainInfoModel.id = id;
            maintainInfoModel.Name = Name;
            maintainInfoModel.AlramId = AlramId;
            maintainInfoModel.Info = Info;
            maintainInfoModel.CreateTime = CreateTime;
            maintainInfoModel.CreateDate = CreateDate;
        }
    }
}
