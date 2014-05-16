using NChromaprintUI.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NChromaprintUI.Converters
{
    public class FloatToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var number = (float)value;

            if (number * 100 > Settings.Default.SimilarityThreshold)
            {
                return (number * 100).ToString("0.00") + " %";
            }
            else
            {
                return "Below threshold";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
