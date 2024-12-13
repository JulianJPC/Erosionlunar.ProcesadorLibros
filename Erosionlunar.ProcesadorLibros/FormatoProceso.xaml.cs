using Erosionlunar.ProcesadorLibros.DB;
using Erosionlunar.ProcesadorLibros.Models;
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
            var infoReadyToProcess = getFilesToProcess();
            var elements = getElementos(infoReadyToProcess);
            InitializeComponent();
            DG1.ItemsSource = elements;
            TipoFormato.Items.Add("Visspool");
            TipoFormato.SelectedIndex = 0;
        }

        private List<elementoFormato> getElementos(List<List<string>> theInfo)
        {
            var response = new List<elementoFormato>();
            foreach(var info in theInfo)
            {
                var oneElemento = new elementoFormato();
                oneElemento.IsSelected = true;
                oneElemento.IdArchivo = info[0];
                oneElemento.IdLibro = info[1];
                oneElemento.fCantidad = (Int32.Parse(info[4]) - Int32.Parse(info[3]) + 1).ToString();
                oneElemento.fraccion = info[5];
                if(info.Count > 6)
                {
                    oneElemento.path = info[6];
                }
                response.Add(oneElemento);
            }
            return response;
        }
        private List<List<string>> getFilesToProcess()
        {
            var infoWithoutProcess = getArchivosWithoutProcess();
            var foldersInProcess = Directory.GetDirectories(pathProcesos);
            for(int i = 0; i < infoWithoutProcess.Count; i++)
            {
                foreach(var folder in foldersInProcess)
                {
                    var pathWithTxt = System.IO.Path.Combine(folder, "txt");
                    if (Directory.Exists(pathWithTxt))
                    {
                        var filesInTxt = Directory.GetFiles(pathWithTxt);
                        foreach (var file in filesInTxt)
                        {
                            if(CalculateMD5(file) == infoWithoutProcess[i][2])
                            {
                                infoWithoutProcess[i].Add(file);
                            }
                        }
                    }
                }
            }
            return infoWithoutProcess;
        }
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
        private List<List<string>> getArchivosWithoutProcess()
        {
            string query = "SELECT IdArchivo, IdLibro, HashA, folioI, folioF, fraccion FROM Archivos WHERE IdMedioOptico = 0 AND HashA != '0';";
            List<string> columns = new List<string> { "IdArchivo", "IdLibro", "HashA", "folioI", "folioF", "fraccion" };
            return _context.readQueryList(query, columns);
        }
        private string getPathProcesos()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame = 1;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna)[0];
        }
        private List<string> getDirsVisspool()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame BETWEEN 5 AND 12;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna);
        }
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
        private string getNombreLibro(string idLibro)
        {
            string query = "SELECT nombreL from Libros WHERE idLibro = @elID;";
            string column = "nombreL";
            List<string> parameters = new List<string> { "@elID" };
            List<string> values = new List<string> { idLibro };
            return _context.readQuerySimple(query, parameters, values, column)[0];
        }
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

        private void sentToProceso_Click(object sender, RoutedEventArgs e)
        {
            if(TipoFormato.Text == "Visspool")
            {
                var lasDir = getDirsVisspool();
                var visspoolizador = new VisspoolControl.VisspoolControl(lasDir);
                foreach(elementoFormato oneElement in DG1.ItemsSource)
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
