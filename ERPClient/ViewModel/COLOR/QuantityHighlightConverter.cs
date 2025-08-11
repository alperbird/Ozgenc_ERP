using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ERPClient.ViewModel.COLOR
{
    public class QuantityHighlightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return false;

            bool TryDec(object o, out decimal v)
            {
                switch (o)
                {
                    case null: v = 0; return false;
                    case int i: v = i; return true;
                    case long l: v = l; return true;
                    case double d: v = (decimal)d; return true;
                    case float f: v = (decimal)f; return true;
                    case decimal m: v = m; return true;
                    case string s: return decimal.TryParse(s, out v);
                    default: return decimal.TryParse(o.ToString(), out v);
                }
            }

            if (TryDec(values[0], out var received) && TryDec(values[1], out var ordered))
                return received < ordered;

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
