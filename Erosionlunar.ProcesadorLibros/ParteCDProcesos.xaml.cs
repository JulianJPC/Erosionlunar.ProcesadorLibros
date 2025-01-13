using Erosionlunar.ProcesadorLibros.DB;
using Erosionlunar.ProcesadorLibros.Models;
using Org.BouncyCastle.Crmf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Erosionlunar.ProcesadorLibros
{
    /// <summary>
    /// Interaction logic for ParteCDProcesos.xaml
    /// </summary>
    public partial class ParteCDProcesos : Window
    {
        private readonly DBConector _context;
        private readonly string pathProcesos;
        private string pathToBaseMDB;
        public ParteCDProcesos(List<elementoParte> losElementos)
        {
            _context = new DBConector();
            pathProcesos = getPathProcesos();
            pathToBaseMDB = getPathBaseMDB();
            var rawPrePartes = getPrePartesSinParte();
            InitializeComponent();
            DG1.ItemsSource = losElementos;
            foreach(List<string> oneResponse in rawPrePartes)
            {
                numeroPreParte.Items.Add(oneResponse[0]);
            }
        }
        private string getPathProcesos()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame = 1;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna)[0];
        }
        private string getPathBaseMDB()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame = 13;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna)[0];
        }
        private List<List<string>> getPrePartesSinParte()
        {
            string query = @"SELECT 
                                    PreParte.numeroP, 
                                    PreParte.idEmpresa, 
                                    Empresas.nombreCortoE    
                            FROM 
                                    PreParte
                            LEFT JOIN 
                                    Partes ON Partes.numeroP = PreParte.numeroP
                            INNER JOIN 
                                    Empresas ON PreParte.idEmpresa = Empresas.idEmpresa
                            WHERE 
                                    Partes.numeroP IS NULL;";
            List<string> columna = new List<string> { "numeroP", "idEmpresa", "nombreCortoE" };
            return _context.readQueryList(query, columna);
        }
        private void crearCheckSum(string checkSumPath, List<elementoParte> theElements)
        {
            System.IO.File.WriteAllText(checkSumPath, "");
            foreach (elementoParte oneElement in theElements)
            {
                System.IO.File.AppendAllText(checkSumPath, System.IO.Path.GetFileName(oneElement.path) + "||" + oneElement.hashF + "\n");
            }
        }
        private void sentToProceso_Click(object sender, RoutedEventArgs e)
        {
            var dirsIsos = new List<string>();
            var theMOs = new List<elementoMO>();
            var preParteNumero = numeroPreParte.SelectedValue.ToString();
            int qMOs = 0;
            foreach (elementoParte oneElement in DG1.ItemsSource)
            {
                if(oneElement.indexMO > qMOs)
                {
                    qMOs = oneElement.indexMO;
                }
            }
            var lastIdMO = getLastIdMO();
            for(int i = 1; i <= qMOs; i++)
            {
                var newMO = new elementoMO();
                lastIdMO = (Int32.Parse(lastIdMO) + 1).ToString();
                newMO.idMO = lastIdMO;
                newMO.theElements = new List<elementoParte>();
                foreach(elementoParte oneElement in DG1.ItemsSource)
                {
                    if(oneElement.indexMO == i)
                    {
                        newMO.nameMO = oneElement.NameMO;
                        newMO.theElements.Add(oneElement);
                    }
                }
            }
            
            string pathFolder = System.IO.Path.Join(pathProcesos, preParteNumero);
            var checkSumPath = System.IO.Path.Combine(pathFolder, "checksum.txt");
            
            
            crearCheckSum(checkSumPath, losElementos);
            //Crear ISO
            int lengthNombreMO = 20;
            if(losElementos[0].NameMO.Length < 20)
            {
                lengthNombreMO = losElementos[0].NameMO.Length; 
            }
            string nombreISO = losElementos[0].NameMO.Substring(0, lengthNombreMO);
            string pathISO = System.IO.Path.Combine(pathFolder, nombreISO);
            dirsIsos.Add(pathISO);
            var direArchivos = new List<string>();
            var direArchivosISO = new List<string>();
            var direFoldersISO = new List<string>();

            direArchivos.Add(checkSumPath);
            direArchivosISO.Add(System.IO.Path.GetFileName(checkSumPath));

            foreach (elementoParte oneElement in losElementos)
            {
                bool esVisspool = false;
                direArchivos.Add(oneElement.path);
                direArchivosISO.Add(@"Libros\" + System.IO.Path.GetFileName(oneElement.path));
                if (System.IO.Path.GetExtension(oneElement.path).ToLower() == ".mdb") { esVisspool = true; }
                if (esVisspool)
                {
                    direArchivos.AddRange(getDireArchMDB(oneElement.path));
                    direArchivos.AddRange(getBaseArch());
                    direArchivosISO.AddRange(getDireArchISO(oneElement.path));
                    direArchivosISO.AddRange(getBaseArchISO());
                    direFoldersISO.Add(getDireFolderParaISO(oneElement.path));
                    direFoldersISO.AddRange(getFolderISO());
                }
            }
            

            foreach (ArchivosFixModel unArchivo in unMO.getArchivos())
            {
                isoControl.crearIso(direFoldersISO, direArchivos, direArchivosISO, pathISO, nombreISO, unMO.PeriodoMOV);
            }
        }
        public string getDireFolderParaISO(string pathF) 
        {
            string nombreArchivo = System.IO.Path.GetFileNameWithoutExtension(pathF);
            return @"Libros\" + nombreArchivo;
        }
        public List<string> getDireArchISO(string pathF)
        {
            string carpetaArchivo = System.IO.Path.GetDirectoryName(pathF);
            string nombreArchivo = System.IO.Path.GetFileNameWithoutExtension(pathF);
            string carpetaPrincipal = System.IO.Path.Combine(carpetaArchivo, nombreArchivo);
            List<string> archivosSecundarios = Directory.GetFiles(carpetaPrincipal).ToList();
            var archivosSecundariosISO = new List<string>();
            foreach (string unPathA in archivosSecundarios)
            {
                archivosSecundariosISO.Add(@"Libros\" + nombreArchivo + @"\" + System.IO.Path.GetFileName(unPathA));
            }
            return archivosSecundariosISO;
        }
        public List<string> getDireArchMDB(string pathF)
        {
            var responseList = new List<string>();
            string carpetaArchivo = System.IO.Path.GetDirectoryName(pathF);
            string nombreArchivo = System.IO.Path.GetFileNameWithoutExtension(pathF);
            string carpetaPrincipal = System.IO.Path.Combine(carpetaArchivo, nombreArchivo);
            List<string> archivosSecundarios = Directory.GetFiles(carpetaPrincipal).ToList();
            responseList.AddRange(archivosSecundarios);
            return responseList;
        }
        private List<string> getBaseArch()
        {
            var losArchivos = new List<string>();
            string[] archivosEnBaseMDB = Directory.GetFiles(pathToBaseMDB);
            string[] carpetasEnBaseMDB = Directory.GetDirectories(pathToBaseMDB);
            string[] archivosEnInstalador = { };
            string[] archivosEnSoporte = { };
            foreach (string carpeta in carpetasEnBaseMDB)
            {
                if (Regex.IsMatch(carpeta, @"^.*Instalador$"))
                {
                    archivosEnInstalador = Directory.GetFiles(carpeta);
                }
                else if (Regex.IsMatch(carpeta, @"^.*Soporte$"))
                {
                    archivosEnSoporte = Directory.GetFiles(carpeta);
                }
            }
            losArchivos.AddRange(archivosEnInstalador);
            losArchivos.AddRange(archivosEnBaseMDB);
            losArchivos.AddRange(archivosEnSoporte);
            return losArchivos;
        }
        private List<string> getBaseArchISO()
        {
            var losArchivos = new List<string>();
            string[] archivosEnBaseMDB = Directory.GetFiles(pathToBaseMDB);
            string[] carpetasEnBaseMDB = Directory.GetDirectories(pathToBaseMDB);
            string[] archivosEnInstalador = { };
            string[] archivosEnSoporte = { };
            foreach (string carpeta in carpetasEnBaseMDB)
            {
                if (Regex.IsMatch(carpeta, @"^.*Instalador$"))
                {
                    archivosEnInstalador = Directory.GetFiles(carpeta);
                }
                else if (Regex.IsMatch(carpeta, @"^.*Soporte$"))
                {
                    archivosEnSoporte = Directory.GetFiles(carpeta);
                }
            }
            foreach (string archivo in archivosEnInstalador)
            {
                losArchivos.Add(@"Instalador\" + System.IO.Path.GetFileName(archivo));
            }
            foreach (string archivo in archivosEnBaseMDB)
            {
                losArchivos.Add(System.IO.Path.GetFileName(archivo));
            }
            foreach (string archivo in archivosEnSoporte)
            {
                losArchivos.Add(@"Soporte\" + System.IO.Path.GetFileName(archivo));
            }
            return losArchivos;
        }
        private List<string> getFolderISO()
        {
            var lasFolder = new List<string>();
            string[] archivosEnBaseMDB = Directory.GetFiles(pathToBaseMDB);
            string[] carpetasEnBaseMDB = Directory.GetDirectories(pathToBaseMDB);
            string[] archivosEnInstalador = { };
            string[] archivosEnSoporte = { };
            foreach (string carpeta in carpetasEnBaseMDB)
            {
                if (Regex.IsMatch(carpeta, @"^.*Instalador$"))
                {
                    archivosEnInstalador = Directory.GetFiles(carpeta);
                }
                else if (Regex.IsMatch(carpeta, @"^.*Soporte$"))
                {
                    archivosEnSoporte = Directory.GetFiles(carpeta);
                }
            }
            lasFolder = carpetasEnBaseMDB.ToList();
            return lasFolder;
        }
    }
}
