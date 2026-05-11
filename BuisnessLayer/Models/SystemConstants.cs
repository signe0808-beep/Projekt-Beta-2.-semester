using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class SystemConstants
    {
        public const double DECAY_CONSTANT = 20.0;     // Hvor meget der siver ud af spanden pr. sek SKAL EVT RETTES, DETTE ER TILFÆLDIGT TAL
                                                        //dette betyder at punktet skal ændres med 5000% for alarm
        public const double ALARM_THRESHOLD = 300.0;  // Grænsen for alarm (timeThreshold) SKAL HELT SIKKERT OGSÅ RETTES
                                                       //dette tal betyder nu at punktet skal ændre modstand med mindst 15%
        public const int NOISE_FLOOR = 10;            // pressureThreshold SKAL MÅLES OG RETTES EFTER
    }
}
