using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using TESTAvaloniaApplication.BusinessLayer.Services;

namespace Presentation.ViewModels
{
    //Heatmap henter bucket-værdier fra PressureLogic2 og opdaterer UI automatisk når ALARM_THRESHOLD = 300.0 overskrides
    //anvender BucketToColorConverter til at konverter fra en nummerisk værdi til enten rød eller grå farvefelt
    

    //HeatmapViewModel er ViewModel til varme‑kortet
    //INotifyPropertyChanged, er standard‑interfacet som fortæller UI’et at en værdi er ændret og UI skal opdateres
    public class HeatmapViewModel : INotifyPropertyChanged
    {
        //reference til PressureLogic2 i BusinessLayer, og henter værdi
        //readonly: værdi sættes i constructor
        private readonly PressureLogic2 _logic;

        //et event som hører under INotifyPropertyChanged, minder Ui'et om at en værdi er ændret og UI skal opdatere
        public event PropertyChangedEventHandler PropertyChanged;

        //property som binder direkte til HeatmapView.axaml under Views
        //når HeatmapView binder til  en bestemt Buckets[r, c], er det samme felt som farves rødt eller gråt.
        //data fra ViewModel videresendes fra _logic.
        public double[,] Buckets => _logic.GetBuckets();

        //opretter metode der konverter matrix til en liste så Avalonia forstår vores heatmap
        public List<double> BucketList => Flatten(_logic.GetBuckets());

        //constructor
        public HeatmapViewModel(PressureLogic2 logic)
        {
            //gemmer reference
            _logic = logic;

            //UI opdateres hvert 100 ms
            DispatcherTimer.Run(() =>
            {
                //henter buckets-værdi og opdaterer bindings
                OnPropertyChanged(nameof(Buckets));
                OnPropertyChanged(nameof(BucketList));
                //fortsæt med at kører timeren
                return true;
            },
            TimeSpan.FromMilliseconds(100));
        }

        //metoden
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

        //metode
        //hvis PropertyChanged ikke er null så kaldes dette event
        //new PropertyChangedEventArgs fortæller hvilken property der er ændret
        private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
    }
}

//Sammenhæng med HeatmapView
//1. HeatmapView.axaml binder til Buckets
//2. HeatmapViewModel leverer Buckets fra _logic.
//3. PressureLogic2 opdaterer sine interne buckets, når state machine kører.
//4. DispatcherTimer i ViewModel’en siger hvert 100 ms: “Buckets er ændret.”
//5. Avalonia henter nye værdier → converteren kører → farverne opdateres.
//Du får et live‑opdateret varme‑kort, der afspejler din BusinessLayer uden at UI’et kender til sensorer, state machine eller algoritmer.
