using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media;
using BusinessLayer.Models;
using TESTAvaloniaApplication.BusinessLayer.Models;

namespace Presentation.Converters
{
    //Oversætteled fra bucket-værdier og returnere en farve
    public class BucketToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Sikrer at value er af typen: double (undgår crash ved null eller forkert type)
            if (value is not double bucketValue)
                return Brushes.LightGray;

            //hvis bucket-værdien er over tærskelværdien bliver feltet rødt
            if (bucketValue >= SystemConstants.ALARM_THRESHOLD)
                return Brushes.Red;

            //ellers gråt felt
            return Brushes.LightGray;
        }

        //IvalueConverter kræver to metoder, derfor oprettes ConvertBack, men implementeres ikke.
        //Farven skal ikke konverteres tilbage til bucket-værdier, der kastes derfor NotImplementedException.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
