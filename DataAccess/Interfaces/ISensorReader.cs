using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ISensorReader
    {
        //Hardwaren skal aflevere de 16 punkt-værdier.
        int[,] ReadMatrix();
    }
}
