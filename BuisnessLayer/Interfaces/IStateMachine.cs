using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TESTAvaloniaApplication.BusinessLayer.Models;

namespace TESTAvaloniaApplication.BusinessLayer.Interfaces
{
    public interface IStateMachine
    {
        SystemStateEnum CurrentState { get; } //Property fortæller hvilken state systemet er i 

        //UI‑laget (Avalonia) skal kunne starte og stoppe StateMachine
        void start();
        void stop();

    }
}
