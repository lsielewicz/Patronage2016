using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Patronage2016.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
       public object Convert(object value, Type targetType, object parameter, string language)
       {
           if (value == null && !(value is bool))
               return Visibility.Visible;

            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
       }

       public object ConvertBack(object value, Type targetType, object parameter, string language)
       {
           throw new NotImplementedException();
       }
    }
}
