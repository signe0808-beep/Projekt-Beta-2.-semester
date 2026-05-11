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
    //Heatmap henter bucket-værdier fra PressureMonitor og opdaterer UI automatisk når ALARM_THRESHOLD = 300.0 overskrides
    //anvender BucketToColorConverter til at konverter fra en nummerisk værdi til enten rød eller grå farvefelt
    

    //HeatmapViewModel er ViewModel til varme‑kortet
    //INotifyPropertyChanged, er standard‑interfacet som fortæller UI’et at en værdi er ændret og UI skal opdateres
    public class HeatmapViewModel : INotifyPropertyChanged
    {
        //reference til Pressuremonitor i BusinessLayer, og henter værdi
        //readonly: værdi sættes i constructor og kan IKKE udskiftes bagefter
        private readonly IPressureMonitor _logic;

        //et event som hører under INotifyPropertyChanged, minder Ui'et om at en værdi er ændret og UI skal opdatere
        //Det er en aftale med avalonia der siger at når der bliver kaldt propertyChanged, skal der hente nye værdier. Hvis det ikke stod her, ville ui ikke opdatere sig selv
        public event PropertyChangedEventHandler PropertyChanged;

        //property som binder direkte til HeatmapView.axaml under Views
        //når HeatmapView binder til  en bestemt Buckets[r, c], er det samme felt som farves rødt eller gråt.
        //data fra ViewModel videresendes fra _logic.
        public double[,] Buckets => _logic.GetBuckets();

        //opretter metode der konverter matrix til en liste så Avalonia forstår vores heatmap
        public List<double> BucketList => Flatten(_logic.GetBuckets());

        //constructor
        public HeatmapViewModel(IPressureMonitor logic)
        {
            //gemmer reference
            _logic = logic;

            //UI opdateres hvert 100 ms
            DispatcherTimer.Run(() =>
            {
                //siger til Avalonia at BucketList er ændret — Avalonia går selv hen og henter den nye værdi via BucketList propertyen
                OnPropertyChanged(nameof(Buckets));
                OnPropertyChanged(nameof(BucketList));
                //fortsæt med at kører timeren
                return true;
            },
            TimeSpan.FromMilliseconds(100));
        }

        //metoden der oversætter matricen til en liste
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

        //sender besked til Avalonia om at en property er ændret — kun hvis nogen lytter (?.)
        //new PropertyChangedEventArgs fortæller hvilken property der er ændret
        //det er kommunikationsvejen mellem denne klasse og Avalonia
        private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
    }
}

//Sammenhæng med HeatmapView
//1. HeatmapView.axaml binder til Buckets
//2. HeatmapViewModel leverer Buckets fra _logic.
//3. PressureMonitor opdaterer sine interne buckets, når state machine kører.
//4. DispatcherTimer i ViewModel’en siger hvert 100 ms: “Buckets er ændret.”
//5. Avalonia henter nye værdier → converteren kører → farverne opdateres.
//Du får et live‑opdateret varme‑kort, der afspejler din BusinessLayer uden at UI’et kender til sensorer, state machine eller algoritmer.
//TEST
