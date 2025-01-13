using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models
{
    public class elementoMO
    {
        public string idMO { get; set; }
        public string nameMO { get; set; }
        public string numberPageMO { get; set; }
        public DateTime periodMO { get; set; }
        public List<elementoParte> theElements { get; set; }
        public void makeFoliosMO()
        {
            string folioI = "";
            string folioF = "";
            string foliosFinal = "";
            int unIdLibro = getByIndiceIdLibro(0);
            foreach (ArchivosFixModel unA in getArchivos())
            {
                if (unA.FraccionV == 0 || unA.FraccionV == 1)
                {
                    folioI = unA.FolioIV.ToString();
                }
                folioF = unA.FolioFV.ToString();
                if (unIdLibro != unA.IdLibroV)
                {
                    foliosFinal = "Folios En Actas";
                }
            }
            if (foliosFinal != "") { foliosFinal = folioI + " A " + folioF; }
            setFoliosMO(foliosFinal);
        }
    }
}
