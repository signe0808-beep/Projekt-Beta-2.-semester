namespace TESTAvaloniaApplication.BusinessLayer.Models
{
    public enum SystemStateEnum //her defineres der forskellige tilstande programmet skal køre igennem
    {
        Initialisering, //systemet starter op — sker kun én gang ved opstart
        Kalibrering,    //måler den tomme måtte og gemmer det som reference
        Monitorering,   //holder løbende øje med trykket på alle 16 punkter
        Alarm           //et punkt har haft for meget tryk for længe — bruger skal flyttes
    }
   
}
