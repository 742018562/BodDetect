using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BodDetect
{
    public class PagerDataModel:ViewModel
    {
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

        private void FirstPageAction()
        {
            CurrentPage = 1;

            var result = _source.Take(PageSize).ToList();

            UpdataSource(result);
        }

        private void PreviousPageAction()
        {
            if (CurrentPage == 1)
            {
                return;
            }

            List<object> result = new List<object>();

            if (CurrentPage == 2)
            {
                result = _source.Take(PageSize).ToList();
            }
            else
            {
                result = _source.Skip((CurrentPage - 2) * PageSize).Take(PageSize).ToList();
            }

            UpdataSource(result);

            CurrentPage--;
        }

        private void NextPageAction()
        {
            if (CurrentPage == TotalPage)
            {
                return;
            }

            List<object> result = new List<object>();

            result = _source.Skip(CurrentPage * PageSize).Take(PageSize).ToList();

            UpdataSource(result);

            CurrentPage++;
        }

        private void LastPageAction()
        {
            CurrentPage = TotalPage;

            int skipCount = (TotalPage - 1) * PageSize;
            int takeCount = _source.Count - skipCount;

            var result = _source.Skip(skipCount).Take(takeCount).ToList();
            UpdataSource(result);
        }

        public List<object> _source = new List<object>();


        public virtual void init() 
        {

            _firstPageCommand = new DelegateCommand(FirstPageAction);

            _previousPageCommand = new DelegateCommand(PreviousPageAction);

            _nextPageCommand = new DelegateCommand(NextPageAction);

            _lastPageCommand = new DelegateCommand(LastPageAction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        public virtual void UpdataSource(List<object> list) 
        {
            //_fakeSoruce.Clear();

            //_fakeSoruce.AddRange(result);
        } 


    }
}
