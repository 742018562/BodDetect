using BodDetect.BodDataManage;
using BodDetect.DataBaseInteractive.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BodDetect.UIModels.PagerDataModels
{
    public class AlramPagerModel : PagerDataModel
    {
        private ObservableCollection<AlarmData> _alarmDataSoruce;

        public ObservableCollection<AlarmData> AlarmDataSource
        {
            get
            {
                return _alarmDataSoruce;
            }
            set
            {
                if (_alarmDataSoruce != value)
                {
                    _alarmDataSoruce = value;
                    OnPropertyChanged("AlarmDataSource");
                }
            }
        }

        public List<AlarmData> AllAlarmData;

        public override void init()
        {
            CurrentPage = 1;
            PageSize = 10;
            AllAlarmData = GetHisDatabases();
            _source = AllAlarmData.ToList<object>();

            _alarmDataSoruce = new ObservableCollection<AlarmData>();

            int index = _source.Count % PageSize;
            if (index > 0)
            {
                TotalPage = _source.Count / PageSize + 1;
            }
            else
            {
                TotalPage = _source.Count / PageSize;
            }

            List<AlarmData> result = _source.Take(PageSize).OfType<AlarmData>().ToList();
            _alarmDataSoruce.Clear();

            _alarmDataSoruce.AddRange(result);

            base.init();
        }

        public List<AlarmData> GetHisDatabases()
        {
            List<AlarmData> fake = new List<AlarmData>();
            List<AlramInfoModel> hisDataBaseModels = BodSqliteHelp.SelectAlramInfo();
            foreach (var item in hisDataBaseModels)
            {
                AlarmData hisDatabase = new AlarmData();
                item.CopyToAlarmData(hisDatabase);
                fake.Add(hisDatabase);
            }
            return fake;

        }

        public override void UpdataSource(List<object> list)
        {
            List<AlarmData> valueList = list.OfType<AlarmData>().ToList();

            _alarmDataSoruce.Clear();

            _alarmDataSoruce.AddRange(valueList);

            base.UpdataSource(list);
        }

        public void AddData(AlarmData hisDatabase)
        {
            hisDatabase.id = AllAlarmData.Count + 1;
            _source.Add(hisDatabase);
            AllAlarmData.Add(hisDatabase);
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
                if (_alarmDataSoruce.Count < PageSize)
                {
                    _alarmDataSoruce.Add(hisDatabase);
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

            List<AlarmData> hisDatabasesList = AllAlarmData.OrderBy(t => t.id).ToList();

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
                valueList = AllAlarmData.GetRange(StartIndex, EndIndex - StartIndex + 1).ToList<object>();
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
