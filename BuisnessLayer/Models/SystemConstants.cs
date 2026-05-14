using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class SystemConstants
        //det hele er i procent
    {
        // Hvor meget % der siver ud af spanden pr. sek SKAL EVT RETTES, DETTE ER TILFÆLDIGT TAL
        public const double DECAY_FACTOR = 0.10;
                                                        
        //dette betyder at punktet skal ændres med x% for alarm
        public const double ALARM_THRESHOLD = 300.0;  // Grænsen for alarm (timeThreshold) SKAL HELT SIKKERT OGSÅ RETTES
                                                     
        //dette tal betyder nu at punktet skal ændre modstand med mindst x%
        public const int NOISE_FLOOR = 10;            // pressureThreshold SKAL MÅLES OG RETTES EFTER

        public const double CALIBRATION_WEIGHT_MEDIUM = 10000.0; // Fx 10 kg

        public const double CALIBRATION_WEIGHT_HIGH = 30000.0;   // Fx 30 kg
    }
}
