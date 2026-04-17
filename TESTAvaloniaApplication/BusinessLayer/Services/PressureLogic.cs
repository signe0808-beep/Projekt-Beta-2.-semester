using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TESTAvaloniaApplication.BusinessLayer.Services
{
        internal class PressureLogic //klasse der skal håndtere, vurdering af tryk, timer logikken og beslutning om hvornår der skal sættes alarm
        {
            float reference; //gemmer baseline fra kalibreringsniveau, hvilket vi kan beregne afvigelse ud fra
            float lastPressure; //gemmer sidste måling (bruges til at opdage ændringer)

            int timer = 0; //tæller hvor længe trykket har været belastet i sekunder
            int changeTimer; //tæller hvor lang tid en ændring har været stabil

            float pressureThreshold = 10; //grænse for hvornår trykket er for højt (10 er en tilfældig variabel)
            float changeThreshold = 5; //hvor stor en ændring der tæller som et nyt punkt

            int timeThreshold = 5; //Hvor mange målinger før noget bliver kritisk
            int changeTimeThreshold = 5; //hvor længe en ændring skal være stået til før den accepteres (undgå korte ryk)
            public void SetReference(float refValue)
            {
                reference = refValue; //gemmer referencen fra kalibreringen
                lastPressure = refValue; //starter med samme værdi som referencen, da det er sidste tryk
            }
            public void Reset()
            {
                timer = 0; //nulstiller timeren, hvilket brugers når et tryk normaliseres.
                changeTimer = 0; //nustiller ændringstiden (hvis den ikke har været længe nok)
            }

            public bool IsCritical(float current) //funktionen der afgør hvorvidt en alarm skal igangsættes
            {
                float deviation = Math.Abs(current - reference); //beregner hvor meget trykket afviger fra baseline
                float change = Math.Abs(current - lastPressure); // ændring siden sidste måling

                if (change > changeThreshold) //hvis ændringen er større end fx 5, så betragtes det som en reél ændring, brugeren har flyttet sig

                {
                    changeTimer++; // hvis ja, skal timeren forstætte, da ændringen fortsætter 
                }
                else //hvis nek
                {
                    changeTimer = 0; // ændringen forsvandt → ignorér
                }

                //Hvis ændringen varer længe nok → accepter nyt punkt, brugeren har ændret sin siddestilling til et nyt punkt
                if (changeTimer >= changeTimeThreshold)
                {
                    timer = 0; // nulstiller belastningstiden, da målingen starter forfra
                }

                if (deviation > pressureThreshold)
                {
                    timer++; //hvis trykket er højere end grænsen, skal timeren begynde at tælle belastningstid
                }
                else
                {
                    timer = 0; //hvis trykket ikke er højt, skal den reset timeren som svarer til at der ingen belastning er
                }

                if (timer >= timeThreshold) //hvis timeren er større eller ligmed grænseværdien, er det sandt og systemet skal gå i kritisk alarm
                {
                    return true;
                }
                return false; //ellers ikke i kritisk tilstand
            }
            public bool IsNormal(float current) //den her tjekker konstant i alarm tilstand
            {
                float deviation = Math.Abs(current - reference); //er der sket en afvigelse

                return deviation < pressureThreshold; //hvis tryk er under grænsen er systemet ok igen, her måler vi selv når vi er i alarm tilstand
            }

        }
    
}
