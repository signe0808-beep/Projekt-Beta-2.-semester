using CommunityToolkit.Mvvm.ComponentModel;

namespace TESTAvaloniaApplication.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        //ObservableObject er en baseklasse fra MVVM‑toolkittet, som automatisk håndterer property‑notifikationer.
        //Når en ViewModel arver fra ObservableObject, får den indbygget støtte til at informere UI’et, hver gang en property ændrer værdi.
    }
}
