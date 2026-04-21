using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TESTAvaloniaApplication.BusinessLayer.Models;
using TESTAvaloniaApplication.BusinessLayer.Services;

namespace TESTAvaloniaApplication.BusinessLayer.Interfaces
{
    public interface Interface1
    {
        public interface IStateMachine
        {
            SystemStateEnum CurrentState { get; } //Property fortæller hvilken state systemet er i

            //UI‑laget (Avalonia) skal kunne starte og stoppe StateMachine
            void start();
            void stop();

        }


        public interface IPressureLogic2
        {
            SystemStateEnum CurrentState { get; } //Property fortæller hvilken state logikken er i

            void ProcessTick(double deltaTime, int[,] matrix); //Kaldes ved hvert tick fra state maskinen
                                                               // deltaTime = tiden siden sidste tick og matrix = 4x4 trykmålinger fra sensoren
            void Reset();
        }

        //Kan oprettes når den er oprettet under services
        //public interface ISensorReader
        // {
        //    void Initialize(); //Klargør sensor
        //    int[,] ReadMatrix //Læser en 4x4 trykmatrix fra sensoren og returnerer et int[,] array med 16 målepunkter
        // }
    }
}
