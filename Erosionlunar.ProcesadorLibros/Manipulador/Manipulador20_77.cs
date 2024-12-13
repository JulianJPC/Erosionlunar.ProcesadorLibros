using Erosionlunar.ProcesadorLibros.Abstract;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace Erosionlunar.ProcesadorLibros.Manipulador
{
    public class Manipulador20_77 : ABSManipulador
    {
        public override DateTime getFecha(string dirA)
        {
            return getDateTimeGenericPDF(142, dirA);
        }
    }
}
