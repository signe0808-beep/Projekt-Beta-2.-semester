using BusinessLayer.Models;
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

        //UDFYLDES TIL KALIBERING 
        private int[,] _calibrationLow = new int[4, 4]
        {
        { 0, 0, 0, 0,},
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 } 
        };
        //UDFYLDES TIL KALIBERING 
        private int[,] _calibrationMedium = new int[4, 4]
       {
        { 0, 0, 0, 0,},
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 }
       };
        //UDFYLDES TIL KALIBERING 
        private int[,] _calibrationHigh = new int[4, 4]
      {
        { 0, 0, 0, 0,},
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 }
      };

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
                    // Hvis vi fx målte 1000 på testen, og den i dag siger 980, så er offsettet -20.
                    //vi tester med vores low-vægt
                    int offset = currentEmptyMatrix[r, c] - _curves[r, c].ADClow;
                    _curves[r, c].DailyOffset = offset;
                }
            }
        }
        // Omsætter et råt ADC-tal fra ét felt til en procent (0-100%)
        public double GetCalibratedPressure(int r, int c, int rawADC)
        {
            var curve = _curves[r, c];
            // Juster dagens måling, så vi fjerner fejlen(offsettet)
            int adjustedADC = rawADC - curve.DailyOffset;

            //hvis der ikke er tryk
            if (adjustedADC >= curve.ADClow) return 0.0;
            // Ligger vægten i den lette halvdel? (Mellem low kg og Medium kg)
            if (adjustedADC <= curve.ADClow && adjustedADC > curve.ADCmedium)
            {
                double rangeADC = curve.ADClow - curve.ADCmedium;
                double position = curve.ADClow - adjustedADC;

                // Ganger brøkdelen med vores valgte medium-vægt fra konstanterne
                return (position / rangeADC) * SystemConstants.CALIBRATION_WEIGHT_MEDIUM;
            }
            //Ligger vi i den tunge halvdel? (Mellem Medium kg og High kg
            else
            {
                double rangeADC2 = curve.ADCmedium - curve.ADChigh;
                double position2 = curve.ADCmedium - adjustedADC;
                // Hvor stort et spring i vægt er der mellem Medium og High? (Fx 30 kg - 10 kg = 20 kg)
                double weightDifference = SystemConstants.CALIBRATION_WEIGHT_HIGH - SystemConstants.CALIBRATION_WEIGHT_MEDIUM;
                // Vi starter på medium-vægten og lægger brøkdelen af resten oveni
                return SystemConstants.CALIBRATION_WEIGHT_MEDIUM + ((position2 / rangeADC2) * weightDifference);
            }

        }
    }
}
