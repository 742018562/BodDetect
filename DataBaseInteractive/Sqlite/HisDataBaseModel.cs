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
            object[] paramList = new object[14];

            paramList[0] = PH;
            paramList[1] = DO;
            paramList[2] = Temperature;
            paramList[3] = Turbidity;
            paramList[4] = Uv254;
            paramList[5] = AirTemperature;
            paramList[6] = Humidity;
            paramList[7] = RunNum;
            paramList[8] = Cod;
            paramList[9] = Bod;
            paramList[10] = BodElePot;
            paramList[11] = BodElePotDrop;
            paramList[12] = CreateDate;
            paramList[13] = CreateTime;

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
