﻿using BodDetect.DataBaseInteractive.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect
{
    public class HisDatabase :ParamData
    {
        public int Id { get; set; }

        public float BodElePot { get; set; }
        public string ElePotUnit { get; set; }

        public float BodElePotDrop { get; set; }
        public string ElePotDropUnit { get; set; }

        public string CreateDate { get; set; }
        public string CreateTime { get; set; }

        public List<HisDatabase> GenerateFakeSource()
        {
            List<HisDatabase> source = new List<HisDatabase>();
            DateTime dateTime = DateTime.Now;
            for (int i = 0; i < 3; i++)
            {
                dateTime = dateTime.AddSeconds(5);
                HisDatabase item = new HisDatabase()
                {
                    Id = i,
                    Bod = i,
                    CodData = i,
                    Uv254Data = i,
                    TurbidityData = i,
                    PHData = i,
                    TemperatureData = i,
                    CreateDate = dateTime.ToLongDateString(),
                    CreateTime = dateTime.ToLongTimeString()
                };

                source.Add(item);
            }

            return source;
        }

        public void CopyToHisDataBaseModel(HisDataBaseModel model) 
        {
            model.AirTemperature = AirTemperatureData;
            model.Bod = Bod;
            model.Cod = CodData;
            model.CreateDate = CreateDate;
            model.CreateTime = CreateTime;
            model.DO = DoData;
            model.Humidity = HumidityData;
            model.id = Id;
            model.PH = PHData;
            model.RunNum = 0;
            model.Temperature = TemperatureData;
            model.Turbidity = TurbidityData;
            model.Uv254 = Uv254Data;
            model.BodElePot = BodElePot;
            model.BodElePotDrop = BodElePotDrop;
        }

        public HisDatabase()
        {

        }
    }
}
