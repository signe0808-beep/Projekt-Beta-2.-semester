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

        void RunStateMachineTick(); //Kaldes ved hvert tick fra state maskinen
        void StartSystem(); //start af systemet
        double[,] GetBuckets(); //Gør det muligt for skærm at hente punkterne
    }
}
