using BodDetect.BodDataManage;
using BodDetect.DataBaseInteractive.Sqlite;
using BodDetect.PagerDataModels;
using BodDetect.UIModels.PagerDataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace BodDetect
{
    public class MainWindow_Model : ViewModel
    {

        public SysStatusMsg sysStatusMsg;

        private float _bodData;
        public float BodData 
        {
            get
            {
                return _bodData;
            }
            set
            {
                if (_bodData != value)
                {
                    _bodData = value;
                    OnPropertyChanged("BodData");
                }
            }
        }

        private float _codData;
        public float CodData 
        {
            get
            {
                return _codData;
            }
            set
            {
                if (_codData != value)
                {
                    _codData = value;
                    OnPropertyChanged("CodData");
                }
            }
        }

        private float _phData;
        public float PHData 
        {
            get
            {
                return _phData;
            }
            set
            {
                if (_phData != value)
                {
                    _phData = value;
                    OnPropertyChanged("PHData");
                }
            }
        }

        private float _temperatureData;
        public float TemperatureData 
        {
            get
            {
                return _temperatureData;
            }
            set
            {
                if (_temperatureData != value)
                {
                    _temperatureData = value;
                    OnPropertyChanged("TemperatureData");
                }
            }
        }

        private float _doData;
        public float DoData
        {
            get
            {
                return _doData;
            }
            set
            {
                if (_doData != value)
                {
                    _doData = value;
                    OnPropertyChanged("DoData");
                }
            }
        }

        private float _uv254Data;
        public float Uv254Data
        {
            get => _uv254Data;
            set
            {
                if (_uv254Data != value)
                {
                    _uv254Data = value;
                    OnPropertyChanged("Uv254Data");

                }

            }
        }

        private float _turbidityData;
        public float TurbidityData
        {
            get => _turbidityData;
            set
            {
                if (_turbidityData != value)
                {
                    _turbidityData = value;
                    OnPropertyChanged("TurbidityData");

                }
            }
        }

        private float _airTemperatureData;
        public float AirTemperatureData
        {
            get
            {
                return _airTemperatureData;
            }
            set
            {
                if (_airTemperatureData != value)
                {
                    _airTemperatureData = value;
                    OnPropertyChanged("AirTemperatureData");
                }
            }
        }

        private float _humidityData;
        public float HumidityDataData
        {
            get
            {
                return _humidityData;
            }
            set
            {
                if (_humidityData != value)
                {
                    _humidityData = value;
                    OnPropertyChanged("HumidityDataData");
                }
            }
        }

        public Visibility _debugMode;

        public Visibility DebugMode 
        {
            get 
            {
                return _debugMode;
            }
            set 
            {
                if (_debugMode != value) 
                {
                    _debugMode = value;
                    OnPropertyChanged("DebugMode");
                }
            }
        }

        private HisParamPagerModel _hisParamData;
        public HisParamPagerModel HisParamData 
        {
            get 
            {
                return _hisParamData;
            }
            set 
            {
                if (_hisParamData != value)
                {
                    _hisParamData = value;
                }
            }
        }

        private SysStatusPagerModel _sysStatusDataModel;

        public SysStatusPagerModel SysStatusDataModel 
        {
            get 
            {
                return _sysStatusDataModel;
            }
            set 
            {
                if (_sysStatusDataModel != value) 
                {
                    _sysStatusDataModel = value;
                }
            }
        }

        private AlramPagerModel _alramPagerModel;

        public AlramPagerModel AlramPagerModels
        {
            get
            {
                return _alramPagerModel;
            }
            set
            {
                if (_alramPagerModel != value)
                {
                    _alramPagerModel = value;
                }
            }
        }

        private DevStatusModels _devStatusModels;

        public DevStatusModels DevStatusModel 
        {
            get 
            {
                return _devStatusModels;
            }
            set 
            {
                if (_devStatusModels != value) 
                {
                    _devStatusModels = value;
                }
            }
        }

        private MaintainInfoPagerModel _maintainInfoPagerModels;
        public MaintainInfoPagerModel MaintainInfoPagerModels 
        {
            get
            {
                return _maintainInfoPagerModels;
            }
            set
            {
                if (_maintainInfoPagerModels != value)
                {
                    _maintainInfoPagerModels = value;
                }
            }
        }

        public MainWindow_Model()
        {

            PagerInit();

            DevStatusModel = new DevStatusModels();
            DevStatusModel.init();

            HisDatabase hisDatabaseLast = new HisDatabase();

            int count = HisParamData.AllHisData.Count;
            hisDatabaseLast = HisParamData.AllHisData[count - 1];

            BodData = hisDatabaseLast.Bod;
            CodData = hisDatabaseLast.CodData;
            Uv254Data = hisDatabaseLast.Uv254Data;
            PHData = hisDatabaseLast.PHData;
            TurbidityData = hisDatabaseLast.TurbidityData;
            TemperatureData = hisDatabaseLast.TemperatureData;
            DoData = hisDatabaseLast.DoData;

            Hour = 1;
            Minute = 2;
            Seccond = 3;
        }

        public void PagerInit() 
        {
            HisParamData = new HisParamPagerModel();
            HisParamData.init();

            SysStatusDataModel = new SysStatusPagerModel();
            SysStatusDataModel.init();

            AlramPagerModels = new AlramPagerModel();
            AlramPagerModels.init();

            MaintainInfoPagerModels = new MaintainInfoPagerModel();
            MaintainInfoPagerModels.init();
        }

        private int _Hour;
        public int Hour
        {
            get { return _Hour; }
            set { _Hour = value; OnPropertyChanged("Hour"); }
        }

        private int _Minute;
        public int Minute
        {
            get { return _Minute; }
            set { _Minute = value; OnPropertyChanged("Minute"); }
        }

        private int _Seccond;
        public int Seccond
        {
            get { return _Seccond; }
            set { _Seccond = value; OnPropertyChanged("Seccond"); }
        }

        public void UpdateSensorStatus() 
        {
            string Redpath = @"pack://application:,,,/Resources/red.png";
            string Greenpath = @"pack://application:,,,/Resources/green.png";

            DateTime dateTime = DateTime.Now;

            if (Uv254Data == 0)
            {
                DevStatusModel.UV254_Sensor_ImgSource = Redpath;
                DevStatusModel.UV254_Sensor_Status = "异常";

                AlarmData alarmData = new AlarmData();
                alarmData.id = AlramPagerModels.AllAlarmData.Count + 1;
                alarmData.DeviceInfo = 3;
                alarmData.ErrorCode = 5;
                alarmData.ErrorDes = "uv254传感器断开";
                alarmData.CreateDate = dateTime.ToLongDateString();
                alarmData.CreateTime = dateTime.ToLongTimeString();

                AlramPagerModels.AddData(alarmData);

                AlramInfoModel alramInfoModel = new AlramInfoModel();
                alarmData.CopyToAlramInfoModel(alramInfoModel);
                BodSqliteHelp.InsertAlramInfo(alramInfoModel);
            }
            else
            {
                DevStatusModel.UV254_Sensor_ImgSource = Greenpath;
                DevStatusModel.UV254_Sensor_Status = "正常";
            }

            if (TurbidityData == 0)
            {
                DevStatusModel.Tur_Sensor_ImgSource= Redpath;
                DevStatusModel.Tur_Sensor_Status = "异常";

                AlarmData alarmData = new AlarmData();
                alarmData.id = AlramPagerModels.AllAlarmData.Count + 1;
                alarmData.DeviceInfo = 4;
                alarmData.ErrorCode = 6;
                alarmData.ErrorDes = "浊度传感器断开";
                alarmData.CreateDate = dateTime.ToLongDateString();
                alarmData.CreateTime = dateTime.ToLongTimeString();

                AlramPagerModels.AddData(alarmData);

                AlramInfoModel alramInfoModel = new AlramInfoModel();
                alarmData.CopyToAlramInfoModel(alramInfoModel);
                BodSqliteHelp.InsertAlramInfo(alramInfoModel);
            }
            else
            {
                DevStatusModel.Tur_Sensor_ImgSource = Greenpath;
                DevStatusModel.Tur_Sensor_Status = "正常";
            }

            if (PHData == 0)
            {
                DevStatusModel.PH_Sensor_ImgSource = Redpath;
                DevStatusModel.PH_Sensor_Status = "异常";

                AlarmData alarmData = new AlarmData();
                alarmData.id = AlramPagerModels.AllAlarmData.Count + 1;
                alarmData.DeviceInfo = 5;
                alarmData.ErrorCode = 7;
                alarmData.ErrorDes = "PH传感器断开";
                alarmData.CreateDate = dateTime.ToLongDateString();
                alarmData.CreateTime = dateTime.ToLongTimeString();

                AlramPagerModels.AddData(alarmData);

                AlramInfoModel alramInfoModel = new AlramInfoModel();
                alarmData.CopyToAlramInfoModel(alramInfoModel);
                BodSqliteHelp.InsertAlramInfo(alramInfoModel);
            }
            else
            {
                DevStatusModel.PH_Sensor_ImgSource = Greenpath;
                DevStatusModel.PH_Sensor_Status = "正常";
            }

            if (DoData == 0)
            {
                DevStatusModel.Do_Sensor_ImgSource = Redpath;
                DevStatusModel.DO_Sensor_Status = "异常";

                AlarmData alarmData = new AlarmData();
                alarmData.id = AlramPagerModels.AllAlarmData.Count + 1;
                alarmData.DeviceInfo = 6;
                alarmData.ErrorCode = 8;
                alarmData.ErrorDes = "DO传感器断开";
                alarmData.CreateDate = dateTime.ToLongDateString();
                alarmData.CreateTime = dateTime.ToLongTimeString();

                AlramPagerModels.AddData(alarmData);

                AlramInfoModel alramInfoModel = new AlramInfoModel();
                alarmData.CopyToAlramInfoModel(alramInfoModel);
                BodSqliteHelp.InsertAlramInfo(alramInfoModel);
            }
            else
            {
                DevStatusModel.Do_Sensor_ImgSource = Greenpath;
                DevStatusModel.DO_Sensor_Status = "正常";
            }
        }

        public void UpdatePlcStatus(bool Connect, bool RunStatus) 
        {
            DateTime dateTime = DateTime.Now;

            if (Connect)
            {
                DevStatusModel.PLC_Status = "正常";
                DevStatusModel.PLC_Status_ImgSource = DevStatusModels.Greenpath;
            }
            else 
            {
                DevStatusModel.PLC_Status = "异常";
                DevStatusModel.PLC_Status_ImgSource = DevStatusModels.Redpath;
                DevStatusModel.BOD_Alram_ImgSource = DevStatusModels.Redpath;
                AlarmData alarmData = new AlarmData();
                alarmData.id = AlramPagerModels.AllAlarmData.Count + 1;
                alarmData.DeviceInfo = 2;
                alarmData.ErrorCode = 3;
                alarmData.ErrorDes = "PLC通讯断开";
                alarmData.CreateDate = dateTime.ToLongDateString();
                alarmData.CreateTime = dateTime.ToLongTimeString();

                AlramPagerModels.AddData(alarmData);

                AlramInfoModel alramInfoModel = new AlramInfoModel();
                alarmData.CopyToAlramInfoModel(alramInfoModel);
                BodSqliteHelp.InsertAlramInfo(alramInfoModel);
            }

            if (RunStatus)
            {
                DevStatusModel.PLC_Run_Status = "正常";
                DevStatusModel.PLC__Run_Status_ImgSource = DevStatusModels.Greenpath;
            }
            else 
            {
                DevStatusModel.PLC_Run_Status = "异常";
                DevStatusModel.PLC__Run_Status_ImgSource = DevStatusModels.Redpath;

                DevStatusModel.PLC_Status = "异常";
                DevStatusModel.PLC_Status_ImgSource = DevStatusModels.Redpath;
                DevStatusModel.BOD_Alram_ImgSource = DevStatusModels.Redpath;
                AlarmData alarmData = new AlarmData();
                alarmData.id = AlramPagerModels.AllAlarmData.Count + 1;
                alarmData.DeviceInfo = 2;
                alarmData.ErrorCode = 4;
                alarmData.ErrorDes = "PLC运行状态异常";
                alarmData.CreateDate = dateTime.ToLongDateString();
                alarmData.CreateTime = dateTime.ToLongTimeString();

                AlramPagerModels.AddData(alarmData);

                AlramInfoModel alramInfoModel = new AlramInfoModel();
                alarmData.CopyToAlramInfoModel(alramInfoModel);
                BodSqliteHelp.InsertAlramInfo(alramInfoModel);

            }
        }


        //private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        //private string _title = "BodDetect";

        //public string Title
        //{
        //    get { return _title; }
        //    set { _title = value; OnPropertyChanged(); }
        //}

        //private bool _btnEnabled = true;

        //public bool BtnEnabled
        //{
        //    get { return _btnEnabled; }
        //    set { _btnEnabled = value; OnPropertyChanged(); }
        //}

        //private ICommand _cmdSample;

        //public ICommand CmdSample => _cmdSample ?? (_cmdSample = new AsyncCommand(async () =>
        //{
        //    Title = "Busy...";
        //    BtnEnabled = false;
        //    //do something
        //    await Task.Delay(2000);
        //    Title = "Arthas.Demo";
        //    BtnEnabled = true;
        //}));

        //private ICommand _cmdSampleWithParam;

        //public ICommand CmdSampleWithParam => _cmdSampleWithParam ?? (_cmdSampleWithParam = new AsyncCommand<string>(async str =>
        //{
        //    string value = str;
        //    Title = $"Hello I'm {str} currently";
        //    BtnEnabled = false;
        //    //do something
        //    await Task.Delay(2000);
        //    Title = "Arthas.Demo";
        //    BtnEnabled = true;
        //}));
    }

    #region Command

    public class AsyncCommand : ICommand
    {
        protected readonly Predicate<object> _canExecute;
        protected Func<Task> _asyncExecute;

        public AsyncCommand(Func<Task> asyncExecute, Predicate<object> canExecute = null)
        {
            _asyncExecute = asyncExecute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public async void Execute(object parameter)
        {
            await _asyncExecute();
        }
    }

    public class AsyncCommand<T> : ICommand
    {
        protected readonly Predicate<T> _canExecute;
        protected Func<T, Task> _asyncExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncCommand(Func<T, Task> asyncExecute, Predicate<T> canExecute = null)
        {
            _asyncExecute = asyncExecute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public async void Execute(object parameter)
        {
            await _asyncExecute((T)parameter);
        }
    }

    #endregion Command
}