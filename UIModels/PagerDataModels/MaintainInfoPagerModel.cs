using BodDetect.BodDataManage;
using BodDetect.DataBaseInteractive.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BodDetect.UIModels.PagerDataModels
{

    public class MaintainInfoPagerModel : PagerDataModel
    {
        private ObservableCollection<MaintainInfo> _maintainInfoSoruce;

        public ObservableCollection<MaintainInfo> MaintainInfoSource
        {
            get
            {
                return _maintainInfoSoruce;
            }
            set
            {
                if (_maintainInfoSoruce != value)
                {
                    _maintainInfoSoruce = value;
                    OnPropertyChanged("MaintainInfoSource");
                }
            }
        }

        public List<MaintainInfo> AllMaintainInfo;

        public override void init()
        {
            CurrentPage = 1;
            PageSize = 10;
            AllMaintainInfo = GetHisDatabases();
            _source = AllMaintainInfo.ToList<object>();

            _maintainInfoSoruce = new ObservableCollection<MaintainInfo>();

            int index = _source.Count % PageSize;
            if (index > 0)
            {
                TotalPage = _source.Count / PageSize + 1;
            }
            else
            {
                TotalPage = _source.Count / PageSize;
            }

            List<MaintainInfo> result = _source.Take(PageSize).OfType<MaintainInfo>().ToList();
            _maintainInfoSoruce.Clear();

            _maintainInfoSoruce.AddRange(result);

            base.init();
        }

        public List<MaintainInfo> GetHisDatabases()
        {
            List<MaintainInfo> fake = new List<MaintainInfo>();
            List<MaintainInfoModel> hisDataBaseModels = BodSqliteHelp.SelectMaintainInfo();
            foreach (var item in hisDataBaseModels)
            {
                MaintainInfo hisDatabase = new MaintainInfo();
                item.CopyToAlarmData(hisDatabase);
                fake.Add(hisDatabase);
            }
            return fake;

        }

        public override void UpdataSource(List<object> list)
        {
            List<MaintainInfo> valueList = list.OfType<MaintainInfo>().ToList();

            _maintainInfoSoruce.Clear();

            _maintainInfoSoruce.AddRange(valueList);

            base.UpdataSource(list);
        }

        public void AddData(MaintainInfo hisDatabase)
        {
            hisDatabase.id = AllMaintainInfo.Count + 1;
            _source.Add(hisDatabase);
            AllMaintainInfo.Add(hisDatabase);
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
                if (_maintainInfoSoruce.Count < PageSize)
                {
                    _maintainInfoSoruce.Add(hisDatabase);
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

            List<MaintainInfo> hisDatabasesList = AllMaintainInfo.OrderBy(t => t.id).ToList();

            EndIndex = hisDatabasesList.Count - 1;

            if (StartDate != null)
            {
                var temp = hisDatabasesList.FirstOrDefault(t => DateTime.Compare(Convert.ToDateTime(t.CreateDate), (DateTime)StartDate) >= 0);
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
                var temp = hisDatabasesList.LastOrDefault(t => DateTime.Compare(Convert.ToDateTime(t.CreateDate), (DateTime)EndDate) <= 0);
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
                valueList = AllMaintainInfo.GetRange(StartIndex, EndIndex - StartIndex + 1).ToList<object>();
            }

            _source = valueList;
            int index = _source.Count % PageSize;
            if (index > 0)
            {
                TotalPage = _source.Count / PageSize + 1;
            }
            else
            {
                TotalPage = _source.Count / PageSize;
            }
            FirstPageAction();
        }

    }
}
