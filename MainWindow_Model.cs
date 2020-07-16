using BodDetect.BodDataManage;
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


        private int _pageSize;

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    OnPropertyChanged("PageSize");
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

        private int _currentPage;

        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }

            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged("CurrentPage");
                }
            }
        }

        private int _totalPage;

        public int TotalPage
        {
            get
            {
                return _totalPage;
            }

            set
            {
                if (_totalPage != value)
                {
                    _totalPage = value;
                    OnPropertyChanged("TotalPage");
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



        private ICommand _firstPageCommand;

        public ICommand FirstPageCommand
        {
            get
            {
                return _firstPageCommand;
            }

            set
            {
                _firstPageCommand = value;
            }
        }

        private ICommand _previousPageCommand;

        public ICommand PreviousPageCommand
        {
            get
            {
                return _previousPageCommand;
            }

            set
            {
                _previousPageCommand = value;
            }
        }

        private ICommand _nextPageCommand;

        public ICommand NextPageCommand
        {
            get
            {
                return _nextPageCommand;
            }

            set
            {
                _nextPageCommand = value;
            }
        }

        private ICommand _lastPageCommand;

        public ICommand LastPageCommand
        {
            get
            {
                return _lastPageCommand;
            }

            set
            {
                _lastPageCommand = value;
            }
        }


        private ObservableCollection<FakeDatabase> _fakeSoruce;

        public ObservableCollection<FakeDatabase> FakeSource
        {
            get
            {
                return _fakeSoruce;
            }
            set
            {
                if (_fakeSoruce != value)
                {
                    _fakeSoruce = value;
                    OnPropertyChanged("FakeSource");
                }
            }
        }

        private List<FakeDatabase> _source;

        public MainWindow_Model()
        {
            _currentPage = 1;

            _pageSize = 20;

            FakeDatabase fake = new FakeDatabase();

            _source = fake.GenerateFakeSource();

            _totalPage = _source.Count / _pageSize;

            _fakeSoruce = new ObservableCollection<FakeDatabase>();

            List<FakeDatabase> result = _source.Take(20).ToList();

            _fakeSoruce.Clear();

            _fakeSoruce.AddRange(result);

            _firstPageCommand = new DelegateCommand(FirstPageAction);

            _previousPageCommand = new DelegateCommand(PreviousPageAction);

            _nextPageCommand = new DelegateCommand(NextPageAction);

            _lastPageCommand = new DelegateCommand(LastPageAction);

            Hour = 1;
            Minute = 2;
            Seccond = 3;
        }

        private void FirstPageAction()
        {
            CurrentPage = 1;

            var result = _source.Take(_pageSize).ToList();

            _fakeSoruce.Clear();

            _fakeSoruce.AddRange(result);
        }

        private void PreviousPageAction()
        {
            if (CurrentPage == 1)
            {
                return;
            }

            List<FakeDatabase> result = new List<FakeDatabase>();

            if (CurrentPage == 2)
            {
                result = _source.Take(_pageSize).ToList();
            }
            else
            {
                result = _source.Skip((CurrentPage - 2) * _pageSize).Take(_pageSize).ToList();
            }

            _fakeSoruce.Clear();

            _fakeSoruce.AddRange(result);

            CurrentPage--;
        }

        private void NextPageAction()
        {
            if (CurrentPage == _totalPage)
            {
                return;
            }

            List<FakeDatabase> result = new List<FakeDatabase>();

            result = _source.Skip(CurrentPage * _pageSize).Take(_pageSize).ToList();

            _fakeSoruce.Clear();

            _fakeSoruce.AddRange(result);

            CurrentPage++;
        }

        private void LastPageAction()
        {
            CurrentPage = TotalPage;

            int skipCount = (_totalPage - 1) * _pageSize;
            int takeCount = _source.Count - skipCount;

            var result = _source.Skip(skipCount).Take(takeCount).ToList();

            _fakeSoruce.Clear();

            _fakeSoruce.AddRange(result);
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