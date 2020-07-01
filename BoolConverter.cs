using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace BodDetect
{
    public class BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            bool Value = (bool)value;

            if (Value)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (value == null)
            //{
            //    return DependencyProperty.UnsetValue;
            //}

            //if ((Visibility)value == Visibility.Visible)
            //{
            //    return false;
            //}
            //else if ((Visibility)value == Visibility.Collapsed || (Visibility)value == Visibility.Hidden)
            //{
            //    return true;
            //}

            //return DependencyProperty.UnsetValue; ;

            throw new NotImplementedException();
        }
    }
}
