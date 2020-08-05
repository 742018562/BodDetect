using BodDetect.DataBaseInteractive.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public class SysStatusData
    {
        public int id { get; set; }

        /// <summary>
        /// BOD采样流程id
        /// </summary>
        public int num { get; set; }

        public string Status { get; set; }

        public string CreateDate { get; set; }

        public string CreateTime { get; set; }

        public void CopyToSysStatusInfoModel(SysStatusInfoModel model)
        {
            model.id = id;
            model.num = num;
            model.SysStatus = Status;
            model.CreateDate = CreateDate;
            model.CreateTime = CreateTime;
        }

        public SysStatusData() { }

    }
}
