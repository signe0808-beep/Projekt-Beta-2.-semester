using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using System.Drawing;

namespace Presentation.Converters
{
    //converter gør feltet rødt, når bucket ≥ ALARM_THRESHOLD
    public class BucketToColorConverter : IValueConverter
    {
        //tærskelsvværdi som matcher vores PressureLocig2
        private const double ALARM_THRESHOLD = 300.0;

        //convert-metoden modtager en bucket‑værdien fra ViewModel, sammenligner med alarmgrænsen, hvorefter den returnerer rødt eller gråt felt til UI
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double bucketValue = (double)value;

            //hvis alarmen er gået bliver feltet rødt
            if (bucketValue >= ALARM_THRESHOLD)
                return Brushes.Red;

            //ellers gråt felt
            return Brushes.LightGray;
        }

        //metoden CovertBack omplementeres ikke, da farven ikke skal konverteres tilbage til bucket-værdier.
        //der kastes NotImplementedException
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
