using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace BodDetect.UIConvert
{
    class AlarmDataToString : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int type = (int)value;
            string text = string.Empty;
            switch (type)
            {
                case 0:
                    text = "主机";
                    break;
                case 1:
                    text = "BOD";
                    break;
                case 2:
                    text =  "PLC";
                    break;
                case 3:
                    text = "uv254传感器";
                    break;
                case 4:
                    text = "浊度传感器";
                    break;
                case 5:
                    text = "PH传感器";
                    break;
                case 6:
                    text = "DO传感器";
                    break;
                case 7:
                    text = "抽水泵";
                    break;
                case 8:
                    text = "COD化学单元";
                    break;
                default:
                    break;
            }

            return text;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
