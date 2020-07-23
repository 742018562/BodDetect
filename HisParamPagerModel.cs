using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BodDetect
{
   public class HisParamPagerModel : PagerDataModel
    {

        private ObservableCollection<HisDatabase> _fakeSoruce;

        public ObservableCollection<HisDatabase> FakeSource
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

        public override void init()
        {
            CurrentPage = 1;
            PageSize = 10;

            _source = GetHisDatabases().ToList<object>();
            _fakeSoruce = new ObservableCollection<HisDatabase>();

            int index = _source.Count % PageSize;
            if (index > 0)
            {
                TotalPage = _source.Count / PageSize + 1;
            }
            else 
            {
                TotalPage = _source.Count / PageSize;
            }

            List<HisDatabase> result = _source.Take(PageSize).OfType<HisDatabase>().ToList();
            _fakeSoruce.Clear();

            _fakeSoruce.AddRange(result);

            base.init();
        }

        public List<HisDatabase> GetHisDatabases()
        {
            HisDatabase fake = new HisDatabase();
            return fake.GenerateFakeSource();
        }

        public override void UpdataSource(List<object> list)
        {
            List<HisDatabase> valueList = list.OfType<HisDatabase>().ToList();

            _fakeSoruce.Clear();

            _fakeSoruce.AddRange(valueList);

            base.UpdataSource(list);
        }

        public void AddData(HisDatabase hisDatabase) 
        {
            _source.Add(hisDatabase);
            int LastTotalPage = TotalPage;

            int index = _source.Count % PageSize;
            if (index > 0)
            {
                TotalPage = _source.Count / PageSize + 1;
            }
            else
            {
                TotalPage = _source.Count / PageSize;
            }


            if (CurrentPage != LastTotalPage) 
            {
                return;
            }

            if (CurrentPage == LastTotalPage) 
            {
                if (_fakeSoruce.Count < PageSize)
                {
                    _fakeSoruce.Add(hisDatabase);
                }
                else 
                {
                    return;
                }

            }
        }

    }
}
