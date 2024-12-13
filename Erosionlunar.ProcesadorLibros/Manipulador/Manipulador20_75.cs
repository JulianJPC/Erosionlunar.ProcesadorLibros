using Erosionlunar.ProcesadorLibros.Abstract;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Erosionlunar.ProcesadorLibros.Manipulador
{
    public class Manipulador20_75 : ABSManipulador
    {
        public override DateTime getFecha(string dirA)
        {
            int qLines = 7; 
            List<string> lasLineas = getLines(dirA, qLines);
            Regex regex = new Regex(@"\d{1,2}\s/\s\d{4}");
            Match match = regex.Match(lasLineas[qLines - 1]);
            string fechaRaw = match.ValueSpan.ToString();
            List<string> fechaEnPartes = fechaRaw.Split('/').ToList();
            fechaEnPartes[0] = addCeroMonth(fechaEnPartes[0].Trim());
            fechaEnPartes[1] = fechaEnPartes[1].Trim();
            var laFecha = DateTime.ParseExact(String.Join('/', "01", fechaEnPartes[0], fechaEnPartes[1]), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            return laFecha;
        }
    }
}
