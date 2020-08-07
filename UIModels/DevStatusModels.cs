using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public class DevStatusModels : ViewModel
    {
        public const string Redpath = @"pack://application:,,,/Resources/red.png";
        public const string Greenpath = @"pack://application:,,,/Resources/green.png";

        public DevStatusModels()
        {
        }

        public void init()
        {
            _cod_Sensor_Status = "正常";
            _cod_Sensor_ImgSource = Greenpath;
            _do_Sensor_Status = "正常";
            _do_Sensor_ImgSource = Greenpath;
            _uv254_Sensor_Status = "正常";
            _uv254_Sensor_ImgSource = Greenpath;
            _pH_Sensor_Status = "正常";
            _ph_Sensor_ImgSource = Greenpath;
            _tur_Sensor_Status = "正常";
            _tur_Sensor_ImgSource = Greenpath;
            _plc_Status = "正常";
            _plc_Status_ImgSource = Greenpath;
            _plc_Run_Status = "正常";
            _plc_Run_Status_ImgSource = Greenpath;
            _BOD_Connect_Status = "正常";
            _BOD_Connect_ImgSource = Greenpath;
            _BOD_Run_Status = "正常";
            _BOD_Run_ImgSource = Greenpath;
            _BOD_Alram_Status = "无";
            _BOD_Alram_ImgSource = Greenpath;
            _BOD_Sample_Status = "空闲";
            _BOD_Sample_ImgSource = Greenpath;

        }

        private string _cod_Sensor_Status;

        public string Cod_Sensor_Status
        {
            get
            {
                return _cod_Sensor_Status;
            }
            set
            {
                if (_cod_Sensor_Status != value)
                {
                    _cod_Sensor_Status = value;
                    OnPropertyChanged("Cod_Sensor_Status");
                }
            }
        }

        private string _cod_Sensor_ImgSource;

        public string Cod_Sensor_ImgSource
        {
            get
            {
                return _cod_Sensor_ImgSource;
            }
            set
            {
                if (_cod_Sensor_ImgSource != value)
                {
                    _cod_Sensor_ImgSource = value;
                    OnPropertyChanged("Cod_Sensor_ImgSource");
                }
            }
        }


        private string _do_Sensor_Status;

        public string DO_Sensor_Status
        {
            get
            {
                return _do_Sensor_Status;
            }
            set
            {
                if (_do_Sensor_Status != value)
                {
                    _do_Sensor_Status = value;
                    OnPropertyChanged("DO_Sensor_Status");
                }
            }
        }

        private string _do_Sensor_ImgSource;

        public string Do_Sensor_ImgSource
        {
            get
            {
                return _do_Sensor_ImgSource;
            }
            set
            {
                if (_do_Sensor_ImgSource != value)
                {
                    _do_Sensor_ImgSource = value;
                    OnPropertyChanged("Do_Sensor_ImgSource");
                }
            }
        }

        private string _uv254_Sensor_Status;

        public string UV254_Sensor_Status
        {
            get
            {
                return _uv254_Sensor_Status;
            }
            set
            {
                if (_uv254_Sensor_Status != value)
                {
                    _uv254_Sensor_Status = value;
                    OnPropertyChanged("UV254_Sensor_Status");
                }
            }
        }

        private string _uv254_Sensor_ImgSource;

        public string UV254_Sensor_ImgSource
        {
            get
            {
                return _uv254_Sensor_ImgSource;
            }
            set
            {
                if (_uv254_Sensor_ImgSource != value)
                {
                    _uv254_Sensor_ImgSource = value;
                    OnPropertyChanged("UV254_Sensor_ImgSource");
                }
            }
        }

        private string _pH_Sensor_Status;

        public string PH_Sensor_Status
        {
            get
            {
                return _pH_Sensor_Status;
            }
            set
            {
                if (_pH_Sensor_Status != value)
                {
                    _pH_Sensor_Status = value;
                    OnPropertyChanged("PH_Sensor_Status");
                }
            }
        }

        private string _ph_Sensor_ImgSource;

        public string PH_Sensor_ImgSource
        {
            get
            {
                return _ph_Sensor_ImgSource;
            }
            set
            {
                if (_ph_Sensor_ImgSource != value)
                {
                    _ph_Sensor_ImgSource = value;
                    OnPropertyChanged("PH_Sensor_ImgSource");
                }
            }
        }

        private string _tur_Sensor_Status;

        public string Tur_Sensor_Status
        {
            get
            {
                return _tur_Sensor_Status;
            }
            set
            {
                if (_tur_Sensor_Status != value)
                {
                    _tur_Sensor_Status = value;
                    OnPropertyChanged("Tur_Sensor_Status");
                }
            }
        }

        private string _tur_Sensor_ImgSource;

        public string Tur_Sensor_ImgSource
        {
            get
            {
                return _tur_Sensor_ImgSource;
            }
            set
            {
                if (_tur_Sensor_ImgSource != value)
                {
                    _tur_Sensor_ImgSource = value;
                    OnPropertyChanged("Tur_Sensor_ImgSource");
                }
            }
        }

        private string _plc_Status;

        public string PLC_Status
        {
            get
            {
                return _plc_Status;
            }
            set
            {
                if (_plc_Status != value)
                {
                    _plc_Status = value;
                    OnPropertyChanged("PLC_Status");
                }
            }
        }

        private string _plc_Status_ImgSource;

        public string PLC_Status_ImgSource
        {
            get
            {
                return _plc_Status_ImgSource;
            }
            set
            {
                if (_plc_Status_ImgSource != value)
                {
                    _plc_Status_ImgSource = value;
                    OnPropertyChanged("PLC_Status_ImgSource");
                }
            }
        }


        private string _plc_Run_Status;

        public string PLC_Run_Status
        {
            get
            {
                return _plc_Run_Status;
            }
            set
            {
                if (_plc_Run_Status != value)
                {
                    _plc_Run_Status = value;
                    OnPropertyChanged("PLC_Run_Status");
                }
            }
        }

        private string _plc_Run_Status_ImgSource;

        public string PLC__Run_Status_ImgSource
        {
            get
            {
                return _plc_Run_Status_ImgSource;
            }
            set
            {
                if (_plc_Run_Status_ImgSource != value)
                {
                    _plc_Run_Status_ImgSource = value;
                    OnPropertyChanged("PLC__Run_Status_ImgSource");
                }
            }
        }

        private string _BOD_Connect_Status;

        public string BOD_Connect_Status
        {
            get
            {
                return _BOD_Connect_Status;
            }
            set
            {
                if (_BOD_Connect_Status != value)
                {
                    _BOD_Connect_Status = value;
                    OnPropertyChanged("BOD_Connect_Status");
                }
            }
        }

        private string _BOD_Connect_ImgSource;

        public string BOD_Connect_ImgSource
        {
            get
            {
                return _BOD_Connect_ImgSource;
            }
            set
            {
                if (_BOD_Connect_ImgSource != value)
                {
                    _BOD_Connect_ImgSource = value;
                    OnPropertyChanged("BOD_Connect_ImgSource");
                }
            }
        }

        private string _BOD_Run_Status;

        public string BOD_Run_Status
        {
            get
            {
                return _BOD_Run_Status;
            }
            set
            {
                if (_BOD_Run_Status != value)
                {
                    _BOD_Run_Status = value;
                    OnPropertyChanged("BOD_Run_Status");
                }
            }
        }
        private string _BOD_Run_ImgSource;

        public string BOD_Run_ImgSource
        {
            get
            {
                return _BOD_Run_ImgSource;
            }
            set
            {
                if (_BOD_Run_ImgSource != value)
                {
                    _BOD_Run_ImgSource = value;
                    OnPropertyChanged("BOD_Run_ImgSource");
                }
            }
        }

        private string _BOD_Sample_Status;

        public string BOD_Sample_Status
        {
            get
            {
                return _BOD_Sample_Status;
            }
            set
            {
                if (_BOD_Sample_Status != value)
                {
                    _BOD_Sample_Status = value;
                    OnPropertyChanged("BOD_Sample_Status");
                }
            }
        }

        private string _BOD_Sample_ImgSource;

        public string BOD_Sample_ImgSource
        {
            get
            {
                return _BOD_Sample_ImgSource;
            }
            set
            {
                if (_BOD_Sample_ImgSource != value)
                {
                    _BOD_Sample_ImgSource = value;
                    OnPropertyChanged("BOD_Sample_ImgSource");
                }
            }
        }

        private string _BOD_Alram_Status;

        public string BOD_Alram_Status
        {
            get
            {
                return _BOD_Alram_Status;
            }
            set
            {
                if (_BOD_Alram_Status != value)
                {
                    _BOD_Alram_Status = value;
                    OnPropertyChanged("BOD_Alram_Status");
                }
            }
        }

        private string _BOD_Alram_ImgSource;

        public string BOD_Alram_ImgSource
        {
            get
            {
                return _BOD_Alram_ImgSource;
            }
            set
            {
                if (_BOD_Alram_ImgSource != value)
                {
                    _BOD_Alram_ImgSource = value;
                    OnPropertyChanged("BOD_Alram_ImgSource");
                }
            }
        }
    }
}
