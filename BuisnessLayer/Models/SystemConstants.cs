using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//disse værdier er sat ud fra en person der vejer 80-90kg
namespace BusinessLayer.Models
{ 
    public class SystemConstants
    {
        // Hvor meget % der siver ud af spandene (punkterne) pr. sek 
        public const double DECAY_FACTOR = 0.08;
                                                        
        //dette betyder at punktet skal ændres med x% for alarm
        public const double ALARM_THRESHOLD = 5000;  // Grænsen for alarm (timeThreshold) SKAL HELT SIKKERT OGSÅ RETTES
                                                     
        //dette tal betyder nu at punktet skal ændre modstand med mindst x%
        public const int NOISE_FLOOR = 15;            // pressureThreshold SKAL MÅLES OG RETTES EFTER

        public const double CALIBRATION_WEIGHT_MEDIUM = 400; // Fra 300g

        public const double CALIBRATION_WEIGHT_HIGH = 700;   // Fra 1,65kg
    }
}
