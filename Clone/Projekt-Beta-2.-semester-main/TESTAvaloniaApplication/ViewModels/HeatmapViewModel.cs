using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Media;
using Avalonia.Threading;
using TESTAvaloniaApplication.BusinessLayer.Interfaces;
using TESTAvaloniaApplication.BusinessLayer.Models;
using TESTAvaloniaApplication.BusinessLayer.Services;

namespace Presentation.ViewModels
{
    //Heatmap henter bucket-værdier fra PressureLogic2 og opdaterer UI automatisk når ALARM_THRESHOLD = 300.0 overskrides
    //anvender BucketToColorConverter til at konverter fra en nummerisk værdi til enten rød eller grå farvefelt
    

    //HeatmapViewModel er ViewModel til varme‑kortet
    //INotifyPropertyChanged, er standard‑interface som fortæller UI’et at en værdi er ændret og UI skal opdateres
    public class HeatmapViewModel : INotifyPropertyChanged
    {
        //reference til PressureLogic2 i BusinessLayer, og henter værdi
        //readonly: værdi sættes i constructor
        private readonly PressureLogic2 _logic;
        private System.Timers.Timer _timer;
      
        //et event som hører under INotifyPropertyChanged, minder Ui'et om at en værdi er ændret og UI skal opdatere
        public event PropertyChangedEventHandler? PropertyChanged;

        // Her gemmer vi bucket værdier (IKKE farver)
        //ObservableCollection er en liste‑type, der automatisk giver besked til UI’et, når elementer tilføjes, fjernes eller ændres.
        //her bruges den til at gemme de 16 bucket‑værdier.ObservableCollection står for, at UI’et viser nye målinger når HeatmapViewModel opdaterer (100 ms).
        public ObservableCollection<double> Buckets { get; } = new();


        //bool for alarm
        public bool IsAlarm => _logic.CurrentState == SystemStateEnum.Alarm;


        //constructor
        public HeatmapViewModel(PressureLogic2 logic)
        {
            //gemmer reference
            _logic = logic;

            // Opret 16 pladser
            for (int i = 0; i < 16; i++)
                Buckets.Add(0);

            _logic.StartSystem();

            _timer = new System.Timers.Timer(100);
            _timer.Elapsed += (_, _) => UpdateUI();
            _timer.Start();
            
        }

        //metoden UpdateBucketList
        private void UpdateUI()
        {
            var data = _logic.GetBuckets();

            Dispatcher.UIThread.Post(() =>
            {
                int index = 0;

                for (int r = 0; r < 4; r++)
                {
                    for (int c = 0; c < 4; c++)
                    {
                        Buckets[index] = data[r, c];
                        index++;
                    }
                }

                PropertyChanged?.Invoke(this, new(nameof(IsAlarm)));
            });

        }        
    }
}

//Sammenhæng med HeatmapView
//1. HeatmapView.axaml binder til Buckets
//2. HeatmapViewModel leverer Buckets fra _logic.
//3. PressureLogic2 opdaterer sine interne buckets, når state machine kører.
//4. DispatcherTimer i ViewModel’en siger hvert 100 ms: “Buckets er ændret.”
//5. Avalonia henter nye værdier → converteren kører → farverne opdateres.
//Du får et live‑opdateret varme‑kort, der afspejler din BusinessLayer uden at UI’et kender til sensorer, state machine eller algoritmer.
