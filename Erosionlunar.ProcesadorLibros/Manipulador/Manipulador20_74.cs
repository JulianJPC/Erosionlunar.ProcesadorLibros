using Erosionlunar.ProcesadorLibros.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Manipulador
{
    public class Manipulador20_74 : ABSManipulador
    {
        public override DateTime getFecha(string dirA)
        {
            return getDateTimeGeneric(5, dirA);
        }
    }
}
