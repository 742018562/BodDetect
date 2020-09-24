using BodDetect.BodDataManage;
using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.DataBaseInteractive.Sqlite
{
    public class SysStatusInfoModel
    {
        public int id { get; set; }
        public int num { get; set; }
        public string SysStatus { get; set; }

        public string CreateDate { get; set; }
        public string CreateTime { get; set; }

        public object[] GetObjectList()
        {
            object[] paramList = new object[4];

            paramList[0] = num;
            paramList[1] = SysStatus;
            paramList[2] = CreateDate;
            paramList[3] = CreateTime;

            return paramList;
        }

        public void CopyToSysStatusData(SysStatusData sysStatusData) 
        {
            sysStatusData.id = id;
            sysStatusData.num = num;
            sysStatusData.Status = SysStatus;
            sysStatusData.CreateTime = CreateTime;
            sysStatusData.CreateDate = CreateDate;
        }

    }
}
