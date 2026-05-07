//importerede namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace TESTAvaloniaApplication.Presentation.ViewModels
{
    //CellViewModel repræsenterer en celle i heatmap
    //den implementerer INotifyPropertyChanged, UI opdateres altså automatisk, når farven/data ændres.
    public class CellViewModel : INotifyPropertyChanged
    {
        private double _value; //gemmer den data for cellen.
        private IBrush _color = Avalonia.Media.Brushes.Gray; //gemmer cellens farve, starter som grå (neutral).

        private const double THRESHOLD = 300.0; // tærskelværdi

        public event PropertyChangedEventHandler? PropertyChanged; //når en property ændres, udløses dette event, så Avalonia kan opdatere UI’et

        public IBrush Color
        {
            get => _color; //returnerer cellens farve.
            private set //sikrer, at KUN klassen selv kan ændre farven.
            {
                _color = value;
                PropertyChanged?.Invoke(this, new(nameof(Color)));
                //når farven ændres, kaldes PropertyChanged, så UI’et opdateres automatisk
            }
        }

        //metode kaldes, når cellens data ændres
        //opdaterer _value med den nye måling
        public void Update(double bucketValue)
        {
            _value = bucketValue;

            // Hvis bucket er over tærsklen → rød
            if (_value >= THRESHOLD)
            {
                Color = Avalonia.Media.Brushes.Red;
            }
            else
            {
                // Under tærsklen → grå
                Color = Avalonia.Media.Brushes.Gray;
            }
        }
    }
}
