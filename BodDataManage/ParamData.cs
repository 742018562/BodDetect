using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect
{
    public class ParamData
    {
        private float pHData;
        private float doData;
        private string doDataUnit;

        private float temperatureData;
        private string temperatureUnit;

        private float turbidityData;
        private string turbidityUnit;

        private float codData;
        private string codDataUnit;

        private float uv254Data;
        private string uv254DataUnit;

        private float bod;
        private string bodDataUnit;

        private float airTemperatureData;
        private float humidityData;


        public float PHData { get => pHData; set => pHData = value; }
        public float DoData { get => doData; set => doData = value; }
        public float TemperatureData { get => temperatureData; set => temperatureData = value; }
        public float TurbidityData { get => turbidityData; set => turbidityData = value; }
        public float CodData { get => codData; set => codData = value; }
        public float Uv254Data { get => uv254Data; set => uv254Data = value; }
        public float Bod { get => bod; set => bod = value; }
        public string DoDataUnit { get => doDataUnit; set => doDataUnit = value; }
        public string TemperatureUnit { get => temperatureUnit; set => temperatureUnit = value; }
        public string TurbidityUnit { get => turbidityUnit; set => turbidityUnit = value; }
        public string CodDataUnit { get => codDataUnit; set => codDataUnit = value; }
        public string Uv254DataUnit { get => uv254DataUnit; set => uv254DataUnit = value; }
        public string BodDataUnit { get => bodDataUnit; set => bodDataUnit = value; }
        public float AirTemperatureData { get => airTemperatureData; set => airTemperatureData = value; }
        public float HumidityData { get => humidityData; set => humidityData = value; }
    }
}
