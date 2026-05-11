using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TESTAvaloniaApplication.BusinessLayer.Models;
using BusinessLayer.Models;

namespace Presentation.Converters
{
    //converter gør feltet rødt, når bucket ≥ ALARM_THRESHOLD.
    //Det er altså et oversætteled  der tager et tal fra bucket værdien og returnere en farve
    public class BucketToColorConverter : IValueConverter
    {
        

        //convert-metoden modtager en bucket‑værdien fra ViewModel, sammenligner med alarmgrænsen, hvorefter den returnerer rødt eller gråt felt til UI
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Sikrer at value er en double (undgår crash ved null eller forkert type)
            if (value is not double bucketValue)
                return Brushes.LightGray;

            //hvis alarmen er gået bliver feltet rødt
            if (bucketValue >= SystemConstants.ALARM_THRESHOLD)
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
