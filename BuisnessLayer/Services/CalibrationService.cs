using BusinessLayer.Models;
using Iot.Device.Card.CreditCardProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class CalibrationService
    
    {
        private SensorCalibrationCurve[,] _curves = new SensorCalibrationCurve[4, 4];

        //Her blev der påført vægt på måtten svarende til 0g (LOW), 300g (MEDIUM og 1650g (HIGH)

        private int[,] _calibrationLow = new int[4, 4]
        {
        { 2, 1, 1, 3,},
        { 1, 1, 2, 1 },
        { 1, 2, 1, 3 },
        { 2, 1, 2, 1 } 
        };

        private int[,] _calibrationMedium = new int[4, 4]
       {
        { 50, 52, 51, 55,},
        { 56, 51,58, 52 },
        { 52, 52, 49, 51 },
        { 54, 52, 50, 53 }
       };
        
        private int[,] _calibrationHigh = new int[4, 4]
      {
        { 112, 107, 108, 109,},
        { 110, 109, 110, 108 },
        { 111, 112, 110, 109 },
        { 111, 113, 112, 110 }
      };

        //bygger kalibreringskurven per sensor med tre referencepunkter (low, medium, high)
        public CalibrationService()
        {
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    _curves[r, c] = new SensorCalibrationCurve
                    {
                        ADClow = _calibrationLow[r, c],
                        ADCmedium = _calibrationMedium[r, c],
                        ADChigh = _calibrationHigh[r, c],

                        DailyOffset = 0
                    };
                }
            }
        }
      
        public void SetDailyBaseline(int[,] currentEmptyMatrix)
        {
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    //Vi trækker dagens tomme måling fra det tal, vi fandt nede testen
                    //Hvis vi fx målte 1 på testen, og den i dag siger 5, så er offsettet +4. Derfor starter alle værdierne på 5
                    //vi tester med vores low-vægt (uden vægt)
                    int offset = currentEmptyMatrix[r, c] - _curves[r, c].ADClow;
                    _curves[r, c].DailyOffset = offset;
                }
            }
        }
        //Omsætter de rå ADC-tal fra et punkt ttil en procentsats (0-100%)
        public double GetCalibratedPressure(int r, int c, int rawADC)
        {
            var curve = _curves[r, c];
            //Juster dagens måling, så vi fjerner (offsettet)
            int adjustedADC = rawADC - curve.DailyOffset;

            //hvis der ikke er tryk på måtten, returneres 0
            if (adjustedADC <= curve.ADClow) return 0.0;

            //Finder ud af om vægten ligger i den lette halvdel (Mellem low kg og Medium kg)
            if (adjustedADC > curve.ADClow && adjustedADC <= curve.ADCmedium)
            {
                double rangeADC = curve.ADCmedium - curve.ADClow;
                double position = adjustedADC - curve.ADClow;

                //Ganger brøkdelen med vores valgte medium-vægt fra konstanterne
                return (position / rangeADC) * SystemConstants.CALIBRATION_WEIGHT_MEDIUM;
            }
            
            //Finder ud af om vægten ligger i den tunge halvdel (Mellem Medium kg og High kg)
            else
            {
                double rangeADC2 = curve.ADChigh - curve.ADCmedium;
                double position2 = adjustedADC - curve.ADCmedium;
                //Hvor stort et spring i vægt er der mellem Medium og High (Fx 30 kg - 10 kg = 20 kg)
                double weightDifference = SystemConstants.CALIBRATION_WEIGHT_HIGH - SystemConstants.CALIBRATION_WEIGHT_MEDIUM;
                //Vi starter på medium-vægten og lægger brøkdelen af resten oveni
                return SystemConstants.CALIBRATION_WEIGHT_MEDIUM + ((position2 / rangeADC2) * weightDifference);
            }

        }
    }
}
