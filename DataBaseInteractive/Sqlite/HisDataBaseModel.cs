using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BodDetect.DataBaseInteractive.Sqlite
{
    public class HisDataBaseModel
    {
        public int id { get; set; }
        public double PH { get; set; }
        public double DO { get; set; }
        public double Temperature { get; set; }
        public double Turbidity { get; set; }
        public double Uv254 { get; set; }
        public double AirTemperature { get; set; }
        public double Humidity { get; set; }
        public int RunNum { get; set; }
        public double Cod { get; set; }
        public double Bod { get; set; }
        public double BodElePot { get; set; }
        public double BodElePotDrop { get; set; }
        public string CreateDate { get; set; }
        public string CreateTime { get; set; }



        public object[] GetObjectList() 
        {
            object[] paramList = new object[15];

            paramList[0] = id;
            paramList[1] = PH;
            paramList[2] = DO;
            paramList[3] = Temperature;
            paramList[4] = Turbidity;
            paramList[5] = Uv254;
            paramList[6] = AirTemperature;
            paramList[7] = Humidity;
            paramList[8] = RunNum;
            paramList[9] = Cod;
            paramList[10] = Bod;
            paramList[11] = BodElePot;
            paramList[12] = BodElePotDrop;
            paramList[13] = CreateDate;
            paramList[14] = CreateTime;

            return paramList;
        }

        public void CopyToHisDatabase(HisDatabase hisDatabase)
        {
            hisDatabase.Id = id;
            hisDatabase.HumidityData = (float)Humidity;
            hisDatabase.PHData = (float)PH;
            hisDatabase.TemperatureData = (float)Temperature;
            hisDatabase.TurbidityData = (float)Turbidity;
            hisDatabase.Uv254Data = (float)Uv254;
            hisDatabase.AirTemperatureData = (float)AirTemperature;
            hisDatabase.Bod = (float)Bod;
            hisDatabase.BodElePot = (float)BodElePot;
            hisDatabase.BodElePotDrop = (float)BodElePotDrop;
            hisDatabase.CodData = (float)Cod;
            hisDatabase.CreateDate = CreateDate;
            hisDatabase.CreateTime = CreateTime;
            hisDatabase.DoData = (float)DO;
        }

    }
}
