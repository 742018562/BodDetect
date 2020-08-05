using BodDetect.BodDataManage;
using BodDetect.DataBaseInteractive.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BodDetect.PagerDataModels
{
    public class SysStatusPagerModel : PagerDataModel
    {
        private ObservableCollection<SysStatusData> _sysStatusDataSource;
        public ObservableCollection<SysStatusData> SysStatusDataSource
        {
            get
            {
                return _sysStatusDataSource;
            }
            set
            {
                if (_sysStatusDataSource != value)
                {
                    _sysStatusDataSource = value;
                    OnPropertyChanged("SysStatusDataSource");
                }
            }
        }

        public List<SysStatusData> AllSysStatusData = new List<SysStatusData>();

        public override void init()
        {
            AllSysStatusData = GetSysStatusData();
            _source = AllSysStatusData.OrderByDescending(t=>t.id).ToList<object>();

            _sysStatusDataSource = new ObservableCollection<SysStatusData>();

            int index = _source.Count % PageSize;
            if (index > 0)
            {
                TotalPage = _source.Count / PageSize + 1;
            }
            else
            {
                TotalPage = _source.Count / PageSize;
            }

            List<SysStatusData> result = _source.Take(PageSize).OfType<SysStatusData>().ToList();
            _sysStatusDataSource.Clear();

            _sysStatusDataSource.AddRange(result);

            base.init();
        }

        public List<SysStatusData> GetSysStatusData()
        {
            List<SysStatusData> fake = new List<SysStatusData>();
            List<SysStatusInfoModel> sysStatusInfoModel = BodSqliteHelp.SelectSysStatusData();
            foreach (var item in sysStatusInfoModel)
            {
                SysStatusData sysStatusData = new SysStatusData();
                item.CopyToSysStatusData(sysStatusData);
                fake.Add(sysStatusData);
            }
            return fake;
        }

        public override void UpdataSource(List<object> list)
        {
            List<SysStatusData> valueList = list.OfType<SysStatusData>().ToList();

            _sysStatusDataSource.Clear();

            _sysStatusDataSource.AddRange(valueList);

            base.UpdataSource(list);
        }

        public void AddData(SysStatusData sysStatusData)
        {
            AllSysStatusData.Add(sysStatusData);

            _source = AllSysStatusData.OrderByDescending(t => t.id).ToList<object>();
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
                if (_sysStatusDataSource.Count < PageSize)
                {
                    _sysStatusDataSource.Add(sysStatusData);
                }
                else
                {
                    return;
                }

            }
        }

        public void UpdateDataByDate(DateTime? StartDate, DateTime? EndDate)
        {
            int StartIndex = 0;
            int EndIndex = 0;

            List<SysStatusData> sysStatusDataList = AllSysStatusData.OrderBy(t => t.id).ToList();

            EndIndex = sysStatusDataList.Count - 1;

            if (StartDate != null)
            {
                var temp = sysStatusDataList.FirstOrDefault(t => DateTime.Compare(Convert.ToDateTime(t.CreateDate), (DateTime)StartDate) >= 0);
                if (temp != null)
                {
                    StartIndex = temp.id - 1;
                }
                else
                {
                    StartIndex = -1;
                }

            }

            if (EndDate != null)
            {
                var temp = sysStatusDataList.LastOrDefault(t => DateTime.Compare(Convert.ToDateTime(t.CreateDate), (DateTime)EndDate) <= 0);
                if (temp != null)
                {
                    EndIndex = temp.id - 1;
                }
                else
                {
                    EndIndex = -1;
                }

            }

            List<object> valueList = new List<object>();

            if (StartIndex != -1 && EndIndex != -1)
            {
                valueList = AllSysStatusData.GetRange(StartIndex, EndIndex - StartIndex + 1).OrderByDescending(t => t.id).ToList<object>();

            }

            _source = valueList;
            FirstPageAction();
        }

        public SysStatusPagerModel() 
        {
            CurrentPage = 1;
            PageSize = 10;
        }
    }
}
