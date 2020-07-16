using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace BodDetect.UIConvert
{
    public class BodDataToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float data = (float)value;
            int type = System.Convert.ToInt32(parameter);
            string text = string.Empty;
            switch (type)
            {
                case 0:
                    text = data.ToString("F2");
                    break;
                case 1:
                    text = data.ToString("F2");
                    break;
                case 2:
                    text = data.ToString("F2");
                    break;
                case 3:
                    text = data.ToString("F2");
                    break;
                case 4:
                    text = data.ToString("F2");
                    break;
                case 5:
                    text = data.ToString("F1");
                    break;
                case 6:
                    text = data.ToString("F2");
                    break;
                default:
                    break;
            }

            if (data == 0) 
            {
                text = "--";
            }


            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
