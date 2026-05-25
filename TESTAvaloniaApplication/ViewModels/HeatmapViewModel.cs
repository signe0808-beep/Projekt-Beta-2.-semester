using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using BusinessLayer.Services;
using TESTAvaloniaApplication.BusinessLayer.Interfaces;


namespace Presentation.ViewModels
{    
    public class HeatmapViewModel : INotifyPropertyChanged
    {
        //reference til Pressuremonitor i BusinessLayer
        private readonly IPressureMonitor _logic;

        public event PropertyChangedEventHandler PropertyChanged;

        //Bro mellem _logic og UI'et så Buckets[r, c] farves de korrekte farver
        public double[,] Buckets => _logic.GetBuckets();

        //anvender metoden Flatten
        public List<double> BucketList => Flatten(_logic.GetBuckets());

        //constructor
        public HeatmapViewModel(IPressureMonitor logic)
        {
            //gemmer reference
            _logic = logic;

            //UI opdateres hvert 100 ms
            DispatcherTimer.Run(() =>
            {
                //fortæller BucketList er ændret — henter automatisk den nye værdi via BucketList property
                OnPropertyChanged(nameof(Buckets));
                OnPropertyChanged(nameof(BucketList));
                //fortsæt med at kører timeren
                return true;
            },
            TimeSpan.FromMilliseconds(100));
        }

        //metode der konverter matrix(4x4) til en liste så Avalonia forstår vores heatmap
        private List<double> Flatten(double[,] matrix)
        {
            var list = new List<double>();
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    list.Add(matrix[r, c]);
                }
            }
            return list;
        }

        //besked: property er ændret
        //new PropertyChangedEventArgs(prpertyName) fortæller hvilken property der er ændret
        private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
    }
}
