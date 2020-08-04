using System.ComponentModel;

namespace BodDetect
{
    public class BodData : INotifyPropertyChanged
    {
        private float pHData;
        private float doData;
        private float temperatureData;
        private float turbidityData;
        private float codData;
        private float uv254Data;
        private float bod;
        private float airTemperatureData;
        private float humidityData;

        public float BodElePot { get; set; }
        public float BodElePotDrop { get; set; }



        public BodData() { }
        private int deviceStatus;

        public float PHData
        {
            get { return pHData; }

            set
            {
                if (pHData != value)
                {
                    pHData = value;
                    OnPropertyChanged("PHData");
                }
            }
        }
        public float DoData
        {
            get => doData;
            set
            {
                if (doData != value)
                {
                    doData = value;
                    OnPropertyChanged("DoData");
                }

            }
        }
        public float TemperatureData
        {
            get => temperatureData;
            set
            {
                if (temperatureData != value)
                {
                    temperatureData = value;
                    OnPropertyChanged("TemperatureData");
                }
            }
        }
        public float TurbidityData
        {
            get => turbidityData;
            set
            {
                if (turbidityData != value)
                {
                    turbidityData = value;
                    OnPropertyChanged("TurbidityData");

                }
            }
        }
        public float CodData
        {
            get => codData;
            set
            {
                if (codData != value)
                {
                    codData = value;
                    OnPropertyChanged("CodData");

                }
            }
        }
        public float Uv254Data
        {
            get => uv254Data;
            set
            {
                if (uv254Data != value)
                {
                    uv254Data = value;
                    OnPropertyChanged("Uv254Data");

                }

            }
        }
        public int DeviceStatus { get => deviceStatus; set => deviceStatus = value; }
        public float Bod
        {
            get => bod;
            set
            {
                if (bod != value)
                {
                    bod = value;
                    OnPropertyChanged("Bod");

                }

            }
        }

        public float AirTemperatureData { get => airTemperatureData; set => airTemperatureData = value; }
        public float HumidityData { get => humidityData; set => humidityData = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string strPropertyInfo)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(strPropertyInfo));
            }
        }
    }
}
