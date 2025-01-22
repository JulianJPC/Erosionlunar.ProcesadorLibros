using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models.windowPPP2
{
    public class Archivo
    {
        public string idArchivo {  get; set; }
        public string idMO { get; set; }
        public string idLibro {  get; set; }
        public string fraccion { get; set; }
        public string folioI { get; set; }
        public string folioF {  get; set; }
        public string asientoI { get; set; }
        public string asientoF { get; set; }
        public string theHash { get; set; }
        public string ramificacion { get; set; }
        public string isActive { get; set; }

        public List<string> giveValues()
        {
            var response = new List<string>();
            response.Add(idArchivo);
            response.Add(idMO);
            response.Add(idLibro);
            response.Add(fraccion);
            response.Add(folioI);
            response.Add(folioF);
            response.Add(asientoI);
            response.Add(asientoF);
            response.Add(theHash);
            response.Add(ramificacion);
            response.Add(isActive);
            return response;
        }

    }
}
