using Erosionlunar.ProcesadorLibros.DB;
using Erosionlunar.ProcesadorLibros.Models;
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
    /// Interaction logic for ParteProcesos.xaml
    /// </summary>
    public partial class ParteProcesos : Window
    {
        private readonly DBConector _context;
        private readonly string pathProcesos;
        public ParteProcesos()
        {
            _context = new DBConector();
            pathProcesos = getPathProcesos();
            var infoReadyToProcess = getFilesToParte();
            var elements = getElementos(infoReadyToProcess);
            InitializeComponent();
            TipoParte.Items.Add("CD-DVD");
            TipoParte.SelectedIndex = 0;

            DG1.ItemsSource = elements;
        }
        private List<elementoParte> getElementos(List<List<string>> theInfo)
        {
            var response = new List<elementoParte>();
            int counter = 1;
            foreach (var info in theInfo)
            {
                var oneElemento = new elementoParte();
                oneElemento.IsSelected = true;
                oneElemento.IdArchivo = info[0];
                
                oneElemento.IdLibro = info[1];
                oneElemento.hashF = info[2];
                oneElemento.IdEmpresa = info[3];
                oneElemento.pageNumberS = info[4];
                oneElemento.pageNumberF = info[5];
                oneElemento.fraccion = info[6];
                if (info.Count > 9)
                {
                    oneElemento.path = info[9];
                    oneElemento.size = calcularPeso(oneElemento.path);
                }
                oneElemento.indexMO = counter;
                oneElemento.NameMO = info[7];
                oneElemento.periodoMO = DateTime.Parse(info[8]);
                counter++;
                response.Add(oneElemento);
            }
            return response;
        }
        private List<string> getPathVisspool(string pathF)
        {
            var dirCarpeta = System.IO.Path.GetDirectoryName(pathF);
            var getNameFile = System.IO.Path.GetFileNameWithoutExtension(pathF);
            var dirCosas = System.IO.Path.Combine(dirCarpeta, getNameFile);
            var archivosMdb = Directory.GetFiles(dirCosas).ToList();
            return archivosMdb;
        }
        public double calcularPeso(string pathF)
        {
            var response = 0.00;
            FileInfo fileInfo = new FileInfo(pathF);
            long fileSizeInBytes = fileInfo.Length;
            if (Regex.IsMatch(pathF, @"^.*visspool.*$"))
            {
                var archivosMdb = getPathVisspool(pathF);
                foreach (string unArch in archivosMdb)
                {
                    FileInfo unFileInfo = new FileInfo(unArch);
                    fileSizeInBytes += unFileInfo.Length;
                }
            }
            return fileSizeInBytes / (1024.0 * 1024.0);
        }
        private List<List<string>> getFilesToParte()
        {
            var infoWithoutProcess = getArchivosWithoutProcess();
            var foldersInProcess = Directory.GetDirectories(pathProcesos);
            for (int i = 0; i < infoWithoutProcess.Count; i++)
            {
                foreach (var folder in foldersInProcess)
                {
                    var pathWithTxt = System.IO.Path.Combine(folder, "txt");
                    var pathWithVisspool = System.IO.Path.Combine(folder, "visspool");
                    if (Directory.Exists(pathWithTxt))
                    {
                        var filesInTxt = Directory.GetFiles(pathWithTxt);
                        foreach (var file in filesInTxt)
                        {
                            if (CalculateMD5(file) == infoWithoutProcess[i][2])
                            {
                                infoWithoutProcess[i].Add(file);
                            }
                        }
                    }
                    if (Directory.Exists(pathWithVisspool))
                    {
                        var folderInViss = Directory.GetDirectories(pathWithVisspool);
                        foreach(string oneFolder in folderInViss)
                        {
                            var filesInViss = Directory.GetFiles(oneFolder);
                            foreach(var file in filesInViss)
                            {
                                if(CalculateMD5(file) == infoWithoutProcess[i][2])
                                {
                                    infoWithoutProcess[i].Add(file);
                                }
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
            string query = "SELECT Archivos.IdArchivo, Archivos.IdLibro, HashA, IdEmpresa, folioI, folioF, fraccion, NombreL, fecha  FROM Archivos";
            query += " INNER JOIN Libros ON libros.IdLibro = Archivos.IdLibro";
            query += " INNER JOIN ArchivosFechas ON ArchivosFechas.IdArchivo = Archivos.IdArchivo";
            query += " WHERE IdMedioOptico = 0 AND HashA != '0';";
            List<string> columns = new List<string> { "IdArchivo", "IdLibro", "HashA", "IdEmpresa", "folioI", "folioF", "fraccion", "NombreL" };
            return _context.readQueryList(query, columns);
        }
        private string getPathProcesos()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame = 1;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna)[0];
        }

        private void volverSeleccion_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Focus(); // Bring the existing window to the front.
                    return;
                }
            }

            // If no instance exists, create and show a new one.
            MainWindow otherWindow = new MainWindow();
            otherWindow.Show();
            this.Close(); // Close the current window if desired.
        }

        private void sentToProceso_Click(object sender, RoutedEventArgs e)
        {
            var response = new List<elementoParte>();
            foreach(elementoParte oneElement in DG1.ItemsSource)
            {
                if (oneElement.IsSelected)
                {
                    response.Add(oneElement);
                }
            }

            // If no instance exists, create and show a new one.
            ParteCDProcesos otherWindow = new ParteCDProcesos(response);
            otherWindow.Show();
            this.Close(); // Close the current window if desired.
        }
    }
}
