using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.Models
{
    public class PreParteSeleccion
    {
        private int numeroP;
        private int idEmpresa;
        private string nombreE;
        public string SelectedArchivo { get; set; }
        public List<string> ArchivosPreP { get; set; }

        public int NumeroP => numeroP;
        public int IdEmpresa => idEmpresa;
        public string NombreE => nombreE;

        public PreParteSeleccion(int nP, int iE, string nE, List<string> arch)
        {
            numeroP = nP;
            idEmpresa = iE;
            nombreE = nE;
            ArchivosPreP = arch;
            SelectedArchivo = ArchivosPreP[0];
        }
    }
}
