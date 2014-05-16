using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NChromaprintUI.Converters
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var valueIsTrue = (bool)value;
			var valueIsNegated = IsNegationSet(parameter);
			
			// XOR
			var isVisible = valueIsTrue ^ valueIsNegated;

			return isVisible ? Visibility.Visible : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		private bool IsNegationSet(object parameter)
		{
			if (parameter != null)
			{
                if (parameter is string)
				{
                    if (((string)parameter).ToLower().Equals("negate"))
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
