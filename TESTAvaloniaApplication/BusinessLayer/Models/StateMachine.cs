using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using TESTAvaloniaApplication.BusinessLayer.Services;
using System.Threading; //giver mulighed for at bruge thread.SLeep (pause i loop)

namespace TESTAvaloniaApplication.BusinessLayer.Models
{
    enum State //her defineres der forskellige tilstande programmet skal køre igennem
    {
        Init,
        Calibration,
        Monitoring,
        Alarm
    }
    class Program //her skal selve state maskinen styres 
    {
        static State currentState = State.Init; //variabel der holder styr op hvilket state systemet er i lige nu, starter som initialize i vores diagram

        static Sensor sensor = new Sensor(); //bruges til at aflæse trykdata (sensor objekt)
        static PressureLogic logic = new PressureLogic(); //(logik-objekt) bruges til at håndtere vurderingen af tryk + tid

        static void Main()
        {
            while (true) //uendelig løkke, systemet kører konstant og er altid aktivt
            {
                switch (currentState) //hopper over i første state
                {
                    case State.Init: //når systemet er i initialize skal den gøre følgende
                        Console.WriteLine("Initialisere system"); //udskrive på display
                        sensor.Init(); //initiliserer sensoren
                        logic.Reset(); //nulstiller timer og tidligere værdier
                        currentState = State.Calibration; //skifter til det næste stadie som er kalibrering
                        break; //stopper den her case


                    case State.Calibration: //hopper videre i kalibrering
                        Console.WriteLine("Kalibrere"); //udskriver på display
                        float reference = sensor.Read(); //aflæser tryk fra sensorene i siddemåtten, som skaber vores referencepunkt

                        logic.SetReference(reference); //gemmer referencen i logikken, så det kan sammenlignes med under navnet reference
                        currentState = State.Monitoring; //går videre til monitorering
                        break; //stopper den her case


                    case State.Monitoring: //hopper videre til hovedtilstand som er monitoring
                        float current = sensor.Read(); //opretter en værdi der aflæser det aktuelle tryk konstant

                        if (logic.IsCritical(current)) //opretter et if statement, der siger at hvis logikken (værdien for tryk) er 
                                                       //kritisk skal den alarmere, hvis ikke skal den forblive i monitorering
                        {
                            currentState = State.Alarm;
                        }
                        else
                        {
                            currentState = State.Monitoring;
                        }
                        break; //stopper den her case


                    case State.Alarm: //hopper over i alarm tilstand
                        Console.WriteLine("ALARM"); //udskriver alarm på display

                        float currentAlarm = sensor.Read(); //bliver ved med at aflæse tryk, stopper ikke med at måle
                        if (logic.IsNormal(currentAlarm)) //hvis logikken (værdien for tryk) falder til en normal tilstand skal den nulstille timeren/logikken og hoppe tilbage til monitorering
                        {
                            logic.Reset();
                            currentState = State.Monitoring;
                        }
                        else //hvis den ikke er det, skal den blive ved med at alarmere
                        {
                            currentState = State.Alarm;
                        }
                        break;
                }
                Thread.Sleep(500); //skaber en pause på 0.5 sekunder, hvilket forhindre RPI'en i at køre på maks konsant
            }
        }
    }
}
