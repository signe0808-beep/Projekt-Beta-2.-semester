using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TESTAvaloniaApplication.BusinessLayer.Models;

namespace TESTAvaloniaApplication.BusinessLayer.Interfaces
{
    public interface IPressureMonitor
    {
        SystemStateEnum CurrentState { get; } //Property fortæller hvilken state logikken er i
        void StartSystem(); //start af systemet
        double[,] GetBuckets(); //Gør det muligt for skærm at hente punkterne
    }
}
