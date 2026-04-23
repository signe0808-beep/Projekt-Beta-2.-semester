using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TESTAvaloniaApplication.BusinessLayer.Models;

namespace TESTAvaloniaApplication.BusinessLayer.Interfaces
{
    public interface IPressureLogic2
    {
        SystemStateEnum CurrentState { get; } //Property fortæller hvilken state logikken er i

        void RunStateMachineTick(double deltaTime, int[,] matrix); //Kaldes ved hvert tick fra state maskinen
                                                           // deltaTime = tiden siden sidste tick og matrix = 4x4 trykmålinger fra sensoren
        void StartSystem();
    }
}
