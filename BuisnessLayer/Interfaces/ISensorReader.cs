using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TESTAvaloniaApplication.BusinessLayer.Models;
using TESTAvaloniaApplication.BusinessLayer.Services;

namespace TESTAvaloniaApplication.BusinessLayer.Interfaces
{
   public interface ISensorReader
    {
        //Hardwaren skal aflevere de 16 punkt-værdier.
        int[,] ReadMatrix();
    }

    //Kan oprettes når den er oprettet under services

    //public interface ISensorReader
    // {
    //    void Initialize(); //Klargør sensor
    //    int[,] ReadMatrix //Læser en 4x4 trykmatrix fra sensoren og returnerer et int[,] array med 16 målepunkter
    // }

}
