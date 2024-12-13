using Erosionlunar.ProcesadorLibros.Abstract;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Manipulador
{
    public class Manipulador20_111:ABSManipulador
    {
        public override DateTime getFecha(string dirA)
        {
            return getDateTimeGenericPDF(180, dirA);
        }
    }
}
