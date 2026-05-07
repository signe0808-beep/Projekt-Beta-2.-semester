using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Presentation.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        //privat felt - holder den aktuelle status-tekst.
        // UI læser ikke direkte fra dette felt, men fra property'en nedenunder.
        private string statusText = "Status: Kalibrer system";

        //public property som UI binder til.
        //ændres værdien, kaldes OnPropertyChanged(), så UI opdateres automatisk.
        public string StatusText 
        { get => statusText;
          set
            {
                statusText = value;
                OnPropertyChanged(); //fortæller at der er sket en ændring
            }
        }

        //kommando som knappen i XAML binder til.
        //trykkes der på knappen, kaldes StartCalibration().
        public ICommand StartCalibrationCommand { get; }

        public MainWindowViewModel()
        {
            //opretter kommandoen og kobles til metoden StartCalibration().
            //RelayCommand er en simpel ICommand-implementation
            StartCalibrationCommand = new RelayCommand(async () => await StartSystem());
        }

        //metode der kører når der er trykket på knappen
        private async Task StartSystem()
        {
            //UI opdateres når knappen trykkes
            StatusText = "Status: Vent 5 sekunder...";

            //efter 5 sekunder ændres teksten
            await Task.Delay(5000);

            //dette er hvad teksetn ændres til efter de 5 sekunder
            StatusText = "Status: System er klar til brug, venligst sæt dig";
        }

        //event som bruges af INotifyPropertyChanged, opdatering når der sker ændring
        public event PropertyChangedEventHandler? PropertyChanged;

        //metode der udløser PropertyChanged-eventet
        //CallerMemberName gør, at vi ikke behøver skrive property-navnet manuelt.
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
