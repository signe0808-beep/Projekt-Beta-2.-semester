using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class SensorCalibrationCurve
    {

        //kalibreringværdier som er fundet ved test
        public int ADClow {  get; set; }
        public int ADCmedium { get; set; }
        public int ADChigh { get; set; }

        // daglige kalibrering med kendt vægt
        public int DailyOffset { get; set; }
    }
}
