using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TESTAvaloniaApplication.BusinessLayer.Interfaces;
using TESTAvaloniaApplication.BusinessLayer.Models;
using DataAccess.Interfaces;
using System.Timers;

namespace BusinessLayer.Services
{
    public class PressureMonitor:IPressureLogic2
    {
        //Denne klasse skal styre tid og tilstande. Den regner ikke selv noget ud, men fortæller hvem der skal
        public SystemStateEnum CurrentState { get; private set; } = SystemStateEnum.Initialisering;

        private ISensorReader _sensor;
        private CalibrationService _calibrationService;
        private LeakyBucketCalculator _leakyBucketCalculator;

        public PressureMonitor(ISensorReader sensor)
        {
            _sensor= sensor;
            _leakyBucketCalculator= new LeakyBucketCalculator();
            _calibrationService= new CalibrationService();
            _tickTimer = new System.Timers.Timer(100);
            _tickTimer.Elapsed += OnTimerElapsed;
        }

    }
}
