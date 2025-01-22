using Erosionlunar.ProcesadorLibros.DB;
using Erosionlunar.ProcesadorLibros.Models.windowF;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for FormatoProceso.xaml
    /// </summary>
    public partial class FormatoProceso : Window
    {
        private readonly DBConector _context;
        private readonly string pathProcesos;
        public FormatoProceso()
        {
            _context = new DBConector();
            pathProcesos = getPathProcesos();
            var infoWithoutProcess = getArchivosWithoutProcess();
            var infoReadyToProcess = getFilesToProcess(infoWithoutProcess);
            var elements = createElementos(infoReadyToProcess);
            InitializeComponent();
            DG1.ItemsSource = elements;
            TipoFormato.Items.Add("Visspool");
            TipoFormato.SelectedIndex = 0;
        }
        /// <summary>
        /// Given a list of Archivos creates a list elementoFormato with the same information and returns it.
        /// </summary>
        /// <param name="theInfo">List of Archivos full of values.</param>
        private List<elementoFormato> createElementos(List<Archivo> theInfo)
        {
            var response = new List<elementoFormato>();
            foreach(var info in theInfo)
            {
                var oneElemento = new elementoFormato();
                oneElemento.IsSelected = true;
                oneElemento.IdArchivo = info.id;
                oneElemento.IdLibro = info.idLibro;
                oneElemento.fCantidad = (Int32.Parse(info.pageNumberE) - Int32.Parse(info.pageNumberS) + 1).ToString();
                oneElemento.fraccion = info.fraccion;
                oneElemento.path = info.pathArchivo;
                response.Add(oneElemento);
            }
            return response;
        }
        /// <summary>
        /// Gets the files to process. First it gets from the DB the files without process
        /// then for every folder in pathProcesos looks for the folder txt and if it finds it
        /// compares the hash of those files to the hashes of the files asked in the DB.
        /// Then adds the list of unprocess files the paths. 
        /// </summary>
        private List<Archivo> getFilesToProcess(List<List<string>> rawArchivos)
        {
            var theArchivosWithoutProcess = createArchivos(rawArchivos);
            var theArchivosToFormat = new List<Archivo>();
            var foldersInProcess = Directory.GetDirectories(pathProcesos);
            var pathsWithTXT = new List<string>();
            var pathsToFiles = new List<string>();
            var pathsToFilesMD5 = new List<string>();
            foreach (var folder in foldersInProcess)
            {
                var pathWithTxt = System.IO.Path.Combine(folder, "txt");
                if (Directory.Exists(pathWithTxt))
                {
                    pathsWithTXT.Add(pathWithTxt);
                }
            }
            foreach(string onePath in pathsWithTXT)
            {
                var filesInTxt = Directory.GetFiles(onePath);
                pathsToFiles.AddRange(filesInTxt);
            }
            foreach(string oneFile in pathsToFiles)
            {
                pathsToFilesMD5.Add(CalculateMD5(oneFile));
                
            }
            foreach(Archivo oneArchivo in theArchivosWithoutProcess)
            {
                var ifHasHash = pathsToFilesMD5.Contains(oneArchivo.hashA);
                if (ifHasHash)
                {
                    var indexHash = pathsToFilesMD5.IndexOf(oneArchivo.hashA);
                    oneArchivo.pathArchivo = pathsToFiles[indexHash];
                    theArchivosToFormat.Add(oneArchivo);
                }
            }
            return theArchivosToFormat;
        }
        /// <summary>
        /// Given the raw information from the DB creates a list of Archivos.
        /// </summary>
        /// <param name="rawArchivos">String of list of Archivos from the DB</param>
        private List<Archivo> createArchivos(List<List<string>> rawArchivos)
        {
            var response = new List<Archivo>();
            foreach(List<string> oneRawArchivo in rawArchivos)
            {
                var newArchivo = new Archivo();
                newArchivo.id = oneRawArchivo[0];
                newArchivo.idLibro = oneRawArchivo[1];
                newArchivo.hashA = oneRawArchivo[2];
                newArchivo.pageNumberS = oneRawArchivo[3];
                newArchivo.pageNumberE = oneRawArchivo[4];
                newArchivo.fraccion = oneRawArchivo[5];
                response.Add(newArchivo);
            }
            return response;
        }
        /// <summary>
        /// Given a string of a path of a file it calculates the MD5 hash of it and returns it.
        /// </summary>
        /// <param name="filename">String with the value of a path to a file in disk.</param>
        private string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }
        /// <summary>
        /// Gets from the DB the archivos with idMO = 0 and with a hash.
        /// </summary>
        private List<List<string>> getArchivosWithoutProcess()
        {
            string query = "SELECT IdArchivo, IdLibro, HashA, folioI, folioF, fraccion FROM Archivos WHERE IdMedioOptico = 0 AND HashA != '0';";
            List<string> columns = new List<string> { "IdArchivo", "IdLibro", "HashA", "folioI", "folioF", "fraccion" };
            return _context.readQueryList(query, columns);
        }
        /// <summary>
        /// Gets the path where procesos are stored in the disk.
        /// </summary>
        private string getPathProcesos()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame = 1;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna)[0];
        }
        /// <summary>
        /// Gets the path where the visspool files are stored.
        /// </summary>
        private List<string> getDirsVisspool()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame BETWEEN 5 AND 12;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna);
        }
        /// <summary>
        /// Gets the Regex used to create visspool from the idLibro given.
        /// </summary>
        /// <param name="idLibro">String of an id Libro.</param>
        private Regex getRegexVisspool(string idLibro)
        {
            string query = "SELECT Informacion from informacionlibro WHERE idLibro = @elID AND IdTipoInformacion = 3;";
            string column = "Informacion";
            List<string> parameters = new List<string> { "@elID" };
            List<string> values = new List<string> { idLibro};
            Regex response = null;
            List<string> resultQuery = _context.readQuerySimple(query, parameters, values, column);
            if (resultQuery.Count == 1)
            {
                response = new Regex(@"^.*" + resultQuery[0] + @".*$", RegexOptions.Compiled);
            }
            return response;
        }
        /// <summary>
        /// Gets the name of a libro from its id.
        /// </summary>
        /// <param name="idLibro">String of an id Libro.</param>
        private string getNombreLibro(string idLibro)
        {
            string query = "SELECT nombreL from Libros WHERE idLibro = @elID;";
            string column = "nombreL";
            List<string> parameters = new List<string> { "@elID" };
            List<string> values = new List<string> { idLibro };
            return _context.readQuerySimple(query, parameters, values, column)[0];
        }
        /// <summary>
        /// Given the id of am Archivo it updates its hash with a given hash.
        /// </summary>
        /// <param name="idArch">String of an id Archivo.</param>
        /// <param name="newHash">String of a hash.</param>
        public void updateHashArchivo(string idArch, string newHash)
        {
            string query = "Update Archivos SET hashA = @newHash  WHERE IdArchivo = @idArch";
            List<string> parameters = new List<string> { "@idArch", "@newHash" };
            List<string> values = new List<string> { idArch, newHash };
            _context.WriteQuery(query, parameters, values);
        }

        private void volverSeleccion_Click(object sender, RoutedEventArgs e)
        {
            MainWindow newWindow = new MainWindow();
            newWindow.Show();

            // Close the current window
            this.Close();
        }
        /// <summary>
        /// Starts formating the files first takes the choosed format
        /// and then if the elementos where selected it formats them.
        /// At the end it updates the BD.
        /// </summary>
        private void sentToProceso_Click(object sender, RoutedEventArgs e)
        {
            var selectedElementos = new List<elementoFormato>();
            foreach (elementoFormato oneElement in DG1.ItemsSource)
            {
                if (oneElement.IsSelected)
                {
                    selectedElementos.Add(oneElement);
                }
            }
            if (TipoFormato.Text == "Visspool")
            {
                var lasDir = getDirsVisspool();
                var visspoolizador = new VisspoolControl.VisspoolControl(lasDir);
                foreach(elementoFormato oneElement in selectedElementos)
                {
                    var theRegex = getRegexVisspool(oneElement.IdLibro);
                    var nombreLibro = getNombreLibro(oneElement.IdLibro);
                    string nuevoHash = visspoolizador.Visspool(oneElement.path, Int32.Parse(oneElement.fCantidad), oneElement.fraccion, theRegex, nombreLibro);
                    updateHashArchivo(oneElement.IdArchivo, nuevoHash);
                }
            }
        }
    }

}
