using Erosionlunar.ProcesadorLibros.DB;
using Erosionlunar.ProcesadorLibros.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static iText.IO.Image.Jpeg2000ImageData;
using static iText.Svg.SvgConstants;

namespace Erosionlunar.ProcesadorLibros
{
    /// <summary>
    /// Interaction logic for PreParteProcesos2.xaml
    /// </summary>
    public partial class PreParteProcesos2 : Window
    {
        private readonly DBConector _context;
        private readonly string pathProcesos;
        private PreParteFormulario elFormulario;
        public PreParteProcesos2(int preParteNumero)
        {
            _context = new DBConector();
            pathProcesos = getPathProcesos();
            elFormulario = new PreParteFormulario();
            elFormulario.setIdPreParte(preParteNumero.ToString());
            elFormulario.setIdEmpresa(getIdEmpresaPreParte(elFormulario.numeroP));
            elFormulario.setInicioE(getInicioE(elFormulario.idEmp));
            
            elFormulario.setStartingFiles(getArchivosPreParte(elFormulario.numeroP));
            var theLibrosRaw = getLibrosEmpresa(elFormulario.idEmp);
            elFormulario.setLibros(turnIntoLibros(theLibrosRaw, elFormulario.idEmp));
            for (int i = 0; i < elFormulario.getLibros().Count; i++)
            {
                string idLibro = elFormulario.getIdLibroFromLibros(i);
                Regex elRegex = getRegexLibro(idLibro);
                var processRegexString = getprocessRegexLibro(idLibro);
                var processInfo = getprocessInfoLibro(idLibro);
                if (elRegex != null)
                {
                    elFormulario.setRegexUnLibro(elRegex, i);
                }
                if(processRegexString.Count > 0)
                {
                    elFormulario.setListRegexString(processRegexString, i);
                }
                if(processInfo.Count > 0)
                {
                    elFormulario.setListInfo(processInfo, i);
                }
            }
            elFormulario.getIdLPosibles();
            elFormulario.getFechasPosibles();
            var elementos = elFormulario.createElementos();
            InitializeComponent();
            DG1.ItemsSource = elementos;
            numeroPreParte.Text = elFormulario.numeroP;
        }

        private List<Libro> turnIntoLibros(List<List<string>> rawInfo, string idE)
        {
            var theList = new List<Libro>();
            foreach (var libro in rawInfo)
            {
                theList.Add(new Libro(libro, idE));
            }
            return theList;
        }
        private void volverSeleccion_Click(object sender, RoutedEventArgs e)
        {
            PreParteProcesos newWindow = new PreParteProcesos();
            newWindow.Show();

            // Close the current window
            this.Close();
        }
        private List<string> getArchivosPreParte(string idPreParte)
        {
            string dirArchivos = System.IO.Path.Combine(pathProcesos, idPreParte, "txt");
            var archivosEnCarpeta = Directory.GetFiles(dirArchivos).ToList();
            var carpetasEnDirectorio = Directory.GetDirectories(dirArchivos).ToList();
            foreach (string unaCarpeta in carpetasEnDirectorio)
            {
                archivosEnCarpeta.AddRange(Directory.GetFiles(unaCarpeta).ToList());
            }

            return archivosEnCarpeta;
        }
        private string getPathProcesos()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame = 1;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna)[0];
        }
        private string getLastIdArchivoI()
        {
            string query = "SELECT MAX(idArchivo) from Archivos;";
            string columna = "MAX(idArchivo)";
            return _context.readQuerySimple(query, columna)[0];
        }
        private List<List<string>> getLibrosEmpresa(string IdEmpresa)
        {
            string query = "SELECT IdLibro, NombreL, NombreArchivoL, Codificacion  FROM Libros WHERE IdEmpresa = @theID AND Activo = 1;";
            List<string> columns = new List<string> { "IdLibro", "NombreL", "NombreArchivoL", "Codificacion" };
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { IdEmpresa };
            return _context.readQueryList(query, parameters, values, columns);
        }
        private Regex getRegexLibro(string idLibro)
        {
            string query = "SELECT informacion FROM informacionlibro WHERE idLibro = @theID AND IdTipoInformacion = 15;";
            string column = "informacion";
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { idLibro };
            Regex response = null;
            List<string> resultQuery = _context.readQuerySimple(query, parameters, values, column);
            if (resultQuery.Count == 1)
            {
                response = new Regex(@"^.*" + resultQuery[0] + @".*$", RegexOptions.Compiled);
            }
            return response;
        }
        private List<string> getprocessRegexLibro(string idLibro)
        {
            string query = "SELECT informacion FROM informacionlibro WHERE idLibro = @theID AND IdTipoInformacion = 17;";
            string column = "informacion";
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { idLibro };
            return _context.readQuerySimple(query, parameters, values, column);
        }
        private List<string> getprocessInfoLibro(string idLibro)
        {
            string query = "SELECT informacion FROM informacionlibro WHERE idLibro = @theID AND IdTipoInformacion = 16;";
            string column = "informacion";
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { idLibro };
            return _context.readQuerySimple(query, parameters, values, column);
        }
        private string getInicioE(string idEmpresa)
        {
            string query = "SELECT IEjercicioE FROM Empresas WHERE idEmpresa = @theID;";
            string column = "IEjercicioE";
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { idEmpresa };
            string response = "0";
            List<string> resultQuery = _context.readQuerySimple(query, parameters, values, column);
            if (resultQuery.Count == 1)
            {
                response = resultQuery[0];
            }
            return response;
        }
        private string getIdEmpresaPreParte(string numberPreParte)
        {
            string query = "SELECT idEmpresa FROM PreParte WHERE numeroP = @theID;";
            string column = "idEmpresa";
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { numberPreParte };
            string response = "0";
            List<string> resultQuery = _context.readQuerySimple(query, parameters, values, column);
            if(resultQuery.Count == 1)
            {
                response = resultQuery[0];
            }
            return response;
        }

        public void insertArchivo(string idArch, string idMO, string idL, string fracc, string folioI, string folioF, string asientoI, string asientoF, string elHash, string rami, string esActiva)
        {
            string query = "INSERT INTO Archivos(IdArchivo, IdMedioOptico, IdLibro, Fraccion, FolioI, FolioF, AsientoI, AsientoF, HashA, Ramificacion, EsRamaActiva) VALUES(";
            query += "@idArch, @idMO, @idLibro, @fraccion, @folioI, @folioF, @asientoI, @asientoF, @elHash, @rami, @esActiva);";
            List<string> parameters = new List<string> { "@idArch", "@idMO", "@idLibro", "@fraccion", "@folioI", "@folioF", "@asientoI", "@asientoF", "@elHash", "@rami", "@esActiva" };
            List<string> values = new List<string> { idArch, idMO, idL, fracc, folioI, folioF, asientoI, asientoF, elHash, rami, esActiva };
            _context.WriteQuery(query, parameters, values);
        }

        public List<List<string>> getFoliosAndAsientosF(string idLibro, string theDate, string theFraccion)
        {
            string query = "SELECT FolioF, AsientoF from Archivos INNER JOIN mediosOpticos ON Archivos.IdMedioOptico = mediosOpticos.IdMedioOptico WHERE IdLibro = @elID AND PeriodoMO = @Periodo AND Fraccion = @elParte AND Archivos.EsRamaActiva = True;";
            List<string> columns = new List<string> { "FolioF", "AsientoF" };
            List<string> parameters = new List<string> { "@elID", "@Periodo", "@elParte" };
            List<string> values = new List<string> { idLibro, theDate, theFraccion };
            return _context.readQueryList(query, parameters, values, columns);
        }

        private void sentToProceso_Click(object sender, RoutedEventArgs e)
        {
            var losIdLibro = new List<string>();
            var theDates = new List<DateTime>();
            var theFracciones = new List<string>();
            foreach(elementoFormulario item in DG1.ItemsSource)
            {
                var elIdLibro = elFormulario.getLibroFromNameA(item.shortName);
                losIdLibro.Add(elIdLibro);

                theDates.Add(DateTime.ParseExact(String.Join('/', "01", addCeroMonth(item.month.ToString()), item.year.ToString()), "dd/MM/yyyy", CultureInfo.InvariantCulture));
                theFracciones.Add(item.fraccion.ToString());
            }
            elFormulario.updateFormulario(theFracciones, losIdLibro, theDates);
            var idLibrosToProcess = new List<string>();
            var fraccionesToProcess = new List<string>();
            var datesToProcess = new List<DateTime>();
            //verify input
            List<List<string>> librosBasicInfo = elFormulario.getFirstsLibros();
            foreach(var libro in librosBasicInfo)
            {
                var idLibro = libro[0];
                var queryMonth = libro[1];
                var queryYear = libro[2];
                var queryFraccion = libro[3];
                var monthStart = elFormulario.inicioEjercicio;
                if((queryFraccion == "0" || queryFraccion == "1") && queryMonth != monthStart)
                {
                    var theDate = DateTime.ParseExact(String.Join('/', "01", addCeroMonth(queryMonth), queryYear), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    theDate = theDate.AddMonths(-1);
                    queryMonth = theDate.Month.ToString();
                    queryYear = theDate.Year.ToString();
                }
                else if(queryFraccion != "0" || queryFraccion != "1")
                {
                    queryFraccion = (Int32.Parse(queryFraccion) - 1).ToString();
                }
                var queryDate = addCeroMonth(queryMonth) + queryYear.Substring(2,2);
                if(libro[1] != monthStart)
                {
                    var response = getFoliosAndAsientosF(idLibro, queryDate, queryFraccion);
                    if(response.Count == 0)
                    {
                        elFormulario.addError($"El libro del {libro[1]} {libro[2]} de ID {libro[0]} y fraccion {libro[3]}, no tiene libro previo.");
                    }
                }
            }
            if(elFormulario.theErrors.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, elFormulario.theErrors), "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //Start Procesos
            else
            {
                elFormulario.setPathsFinales();
                var librosAllInfo = elFormulario.getAllLibrosInOrder();
                
                foreach (var libro in librosAllInfo)
                {
                    var idLibro = libro[0];
                    var queryMonth = libro[1];
                    var queryYear = libro[2];
                    var queryFraccion = libro[3];
                    var indexOfFile = Int32.Parse(libro[4]);
                    var monthStart = elFormulario.inicioEjercicio;
                    var firstPageNumber = "1";
                    var firstEntryNumber = "1";
                    if ((queryFraccion == "0" || queryFraccion == "1") && queryMonth != monthStart)
                    {
                        var theDate = DateTime.ParseExact(String.Join('/', "01", addCeroMonth(queryMonth), queryYear), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        theDate = theDate.AddMonths(-1);
                        queryMonth = theDate.Month.ToString();
                        queryYear = theDate.Year.ToString();
                    }
                    else if (queryFraccion != "0" || queryFraccion != "1")
                    {
                        queryFraccion = (Int32.Parse(queryFraccion) - 1).ToString();
                    }
                    var queryDate = addCeroMonth(queryMonth) + queryYear.Substring(2, 2);
                    if (libro[1] != monthStart)
                    {
                        var response = getFoliosAndAsientosF(idLibro, queryDate, queryFraccion);
                        firstPageNumber = (Int32.Parse(response[0][0]) + 1).ToString();
                        firstEntryNumber = (Int32.Parse(response[0][1]) + 1).ToString();
                    }
                    var numberPageAndEntryFinal = elFormulario.proccessOneLibro(indexOfFile, firstPageNumber, firstEntryNumber);
                    var lastNumberPage = numberPageAndEntryFinal[0].ToString();
                    var lastEntry = numberPageAndEntryFinal[1].ToString();
                    var lastId = (Int32.Parse(getLastIdArchivoI()) + 1).ToString();
                    var theHash = CalculateMD5(elFormulario.pathsFinal[indexOfFile]);
                    insertArchivo(lastId, "0", idLibro, libro[3], firstPageNumber, lastNumberPage, firstEntryNumber, lastEntry, theHash, "0", "1");
                }
                MainWindow newWindow = new MainWindow();
                newWindow.Show();
                // Close the current window
                this.Close();

            }
            
        }
        protected string addCeroMonth(string theMonth)
        {
            string response = theMonth;
            if (response.Length == 1)
            {
                response = "0" + response;
            }
            return response;
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
    }
}
