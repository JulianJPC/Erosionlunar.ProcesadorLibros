using Erosionlunar.ProcesadorLibros.DB;
using Erosionlunar.ProcesadorLibros.Models;
using Erosionlunar.ProcesadorLibros.Models.windowPPP2;
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

            var idEmpresa = getIdEmpresaPreParte(preParteNumero.ToString());
            var startEx = getInicioE(idEmpresa);

            var basicPreParte = new PreParteBasic();
            basicPreParte.idPreParte = preParteNumero.ToString();
            basicPreParte.idEmpresa = idEmpresa;
            basicPreParte.startExcercise = startEx;
            elFormulario.setBasicPreParte(basicPreParte);

            var theLibrosRaw = getLibrosEmpresa(elFormulario.idEmp);
            var theLibros = turnIntoLibros(theLibrosRaw, elFormulario.idEmp);
            elFormulario.setLibros(theLibros);
            for (int i = 0; i < elFormulario.getLibros().Count; i++)
            {
                string idLibro = elFormulario.getIdLibroFromLibros(i);
                Regex elRegex = getRegexLibro(idLibro);
                var processRegexString = getprocessRegexLibro(idLibro);
                var processInfo = getprocessInfoLibro(idLibro);
                var theInfo = new basicInfoProcess();
                theInfo.libroRegex = elRegex;
                theInfo.rawProcessRegex = processRegexString;
                theInfo.infosForProcess = processInfo;
                elFormulario.setBasicInfoProcess(theInfo, i);
            }
            var filesOfPreParte = getArchivosPreParte(elFormulario.numeroP);
            elFormulario.setStartingFiles(filesOfPreParte);

            var elementos = elFormulario.createElementos();
            InitializeComponent();
            DG1.ItemsSource = elementos;
            numeroPreParte.Text = elFormulario.numeroP;
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
        /// Gets the maximum value of Id Archivo.
        /// </summary>
        private string getLastIdArchivoI()
        {
            string query = "SELECT MAX(idArchivo) from Archivos;";
            string columna = "MAX(idArchivo)";
            return _context.readQuerySimple(query, columna)[0];
        }
        /// <summary>
        /// Gets the maximum value of Id Archivos Fecha.
        /// </summary>
        private string getLastIdFechaArchivo()
        {
            string query = "SELECT MAX(idArchivosFechas) from ArchivosFechas;";
            string columna = "MAX(idArchivo)";
            return _context.readQuerySimple(query, columna)[0];
        }
        /// <summary>
        /// Given an Id empresa it returns the Libros of that empresas. The Id Libro, name of the libro, name of the file of libro and the encoding.
        /// </summary>
        /// <param name="IdEmpresa">String with the value of the Id Empresa.</param>
        private List<List<string>> getLibrosEmpresa(string IdEmpresa)
        {
            string query = "SELECT IdLibro, NombreL, NombreArchivoL, Codificacion  FROM Libros WHERE IdEmpresa = @theID AND Activo = 1;";
            List<string> columns = new List<string> { "IdLibro", "NombreL", "NombreArchivoL", "Codificacion" };
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { IdEmpresa };
            return _context.readQueryList(query, parameters, values, columns);
        }
        /// <summary>
        /// Given an Id Libro it returns the regex as a string of the name of the libro in the file.
        /// </summary>
        /// <param name="idLibro">String with the value of the Id Libro.</param>
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
        /// <summary>
        /// Given an Id Libro it returns the regex as a string that are used in the process of the file.
        /// </summary>
        /// <param name="idLibro">String with the value of the Id Libro.</param>
        private List<string> getprocessRegexLibro(string idLibro)
        {
            string query = "SELECT informacion FROM informacionlibro WHERE idLibro = @theID AND IdTipoInformacion = 17;";
            string column = "informacion";
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { idLibro };
            return _context.readQuerySimple(query, parameters, values, column);
        }
        /// <summary>
        /// Given an Id Libro it returns the information used in the process as strings.
        /// </summary>
        /// <param name="idLibro">String with the value of the Id Libro.</param>
        private List<string> getprocessInfoLibro(string idLibro)
        {
            string query = "SELECT informacion FROM informacionlibro WHERE idLibro = @theID AND IdTipoInformacion = 16;";
            string column = "informacion";
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { idLibro };
            return _context.readQuerySimple(query, parameters, values, column);
        }
        /// <summary>
        /// Given an Id Empresa it returns the start of the exercise.
        /// </summary>
        /// <param name="idEmpresa">String with the value of the Id Empresa.</param>
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
        /// <summary>
        /// Given a number of PreParte it return the Id Empresa.
        /// </summary>
        /// <param name="numberPreParte">String with the value of number of preparte.</param>
        private string getIdEmpresaPreParte(string numberPreParte)
        {
            string query = "SELECT idEmpresa FROM PreParte WHERE numeroP = @theID;";
            string column = "idEmpresa";
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { numberPreParte };
            string response = "0";
            List<string> resultQuery = _context.readQuerySimple(query, parameters, values, column);
            if (resultQuery.Count == 1)
            {
                response = resultQuery[0];
            }
            return response;
        }
        /// <summary>
        /// Givien an Archivo it creates the query to insert its values to the DB.
        /// </summary>
        /// <param name="theArchivo">One Archivo full of values.</param>
        public QueryDB createQArchivo(Archivo theArchivo)
        {
            var response = new QueryDB();
            string query = "INSERT INTO Archivos(IdArchivo, IdMedioOptico, IdLibro, Fraccion, FolioI, FolioF, AsientoI, AsientoF, HashA, Ramificacion, EsRamaActiva) VALUES(";
            query += "@idArch, @idMO, @idLibro, @fraccion, @folioI, @folioF, @asientoI, @asientoF, @elHash, @rami, @esActiva);";
            List<string> parameters = new List<string> { "@idArch", "@idMO", "@idLibro", "@fraccion", "@folioI", "@folioF", "@asientoI", "@asientoF", "@elHash", "@rami", "@esActiva" };
            List<string> values = theArchivo.giveValues();
            response.query = query;
            response.parameteres = parameters;
            response.values = values;
            return response;
        }
        /// <summary>
        /// Givien a FechaArchivo it creates the query to insert its values to the DB.
        /// </summary>
        /// <param name="theFechaArchivo">One FechaArchivo full of values.</param>
        public QueryDB createQFechaArchivo(FechaArchivo theFechaArchivo)
        {
            var response = new QueryDB();
            string query = "INSERT INTO ArchivosFechas(idArchivosFechas, fecha, idArchivo, idLibro) VALUES(";
            query += "@idArchF, @fecha, @idArchivo, @idLibro);";
            List<string> parameters = new List<string> { "@idArchF", "@fecha", "@idArchivo", "@idLibro" };
            List<string> values = new List<string> { theFechaArchivo.id, theFechaArchivo.idArchivo, theFechaArchivo.fecha, theFechaArchivo.idLibro };
            response.query = query;
            response.parameteres = parameters;
            response.values = values;
            return response;
        }
        /// <summary>
        /// Given an id Libro,a fecha and a fraccion, searchs for the last number page and last entries
        /// used im the DB.
        /// </summary>
        /// <param name="idLibro">String with the value of the Id Libro.</param>
        /// <param name="theDate">String with the format to insert into the DB</param> 
        /// <param name="theFraccion">String with the value of a fraccion.</param>
        public List<List<string>> getFoliosAndAsientosF(string idLibro, string theDate, string theFraccion)
        {
            string query = "SELECT FolioF, AsientoF from Archivos INNER JOIN mediosOpticos ON Archivos.IdMedioOptico = mediosOpticos.IdMedioOptico WHERE IdLibro = @elID AND PeriodoMO = @Periodo AND Fraccion = @elParte AND Archivos.EsRamaActiva = True;";
            List<string> columns = new List<string> { "FolioF", "AsientoF" };
            List<string> parameters = new List<string> { "@elID", "@Periodo", "@elParte" };
            List<string> values = new List<string> { idLibro, theDate, theFraccion };
            return _context.readQueryList(query, parameters, values, columns);
        }
        /// <summary>
        /// Given the raw information of a list of libros an its id Empresa it 
        /// makes Libros classes and returns the list.
        /// </summary>
        private List<Libro> turnIntoLibros(List<List<string>> rawInfo, string idE)
        {
            var theList = new List<Libro>();
            foreach (var libro in rawInfo)
            {
                theList.Add(new Libro(libro, idE));
            }
            return theList;
        }
        /// <summary>
        /// Given an id PreParte it searchs into the disk for the folder of that PreParte
        /// and looks for files inside it. Then returns a list of the paths of the files found.
        /// </summary>
        /// <param name="idPreParte">String with the value of an id PreParte</param>
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
        /// <summary>
        /// It opens a new window to ask for a numerical value then it returns it. The text show in the window is the 
        /// same that is given.
        /// </summary>
        /// <param name="text">Text to show in the function.</param>
        private int AskForNumber(string text)
        {
            var dialog = new numberInput(text);
            var response = -1;
            if (dialog.ShowDialog() == true)
            {
                int number = dialog.Result.Value;
                MessageBox.Show($"Numero escrito: {number}", "Input número");
            }
            else
            {
                MessageBox.Show("Operacion Abortada.", "Cancelado");
            }
            return response;
        }
        /// <summary>
        /// Starts the process to modify the files.
        /// </summary>
        private void sentToProceso_Click(object sender, RoutedEventArgs e)
        {
            elFormulario.updateFormulario(DG1.ItemsSource);
            var amountFiles = elFormulario.getNumberFiles();
            var listQuerys = new List<QueryDB>();
            var lastId = Int32.Parse(getLastIdArchivoI());
            var lastIdFecha = Int32.Parse(getLastIdFechaArchivo());
            for (int i =0; i < amountFiles; i++)
            {
                int firstPageNumber;
                int firstEntryNumber;
                var isStartingExercise = elFormulario.isFileStartingMonth(i);
                var queryFolio = elFormulario.getQueryFolio(i);
                var lastNumbersBD = getFoliosAndAsientosF(queryFolio[0], queryFolio[1], queryFolio[2]); ;
                var isPreviousInList = elFormulario.isPrevious(i);

                var IdLibroFile = elFormulario.getIdLibroFile(i);
                var fraccionFile = elFormulario.getFraccionFile(i);
                var hashFile = elFormulario.getHashFile(i);
                var idToUse = (lastId + 1).ToString();
                lastId++;

                var newArchivo = new Archivo();
                newArchivo.ramificacion = "0";
                newArchivo.isActive = "1";
                newArchivo.idMO = "0";
                newArchivo.idArchivo = idToUse;
                newArchivo.idLibro = IdLibroFile;
                newArchivo.fraccion = fraccionFile;
                newArchivo.theHash = hashFile;

                var newFechaArchivo = new FechaArchivo();
                var dateFile = elFormulario.getDateFile(i);

                newFechaArchivo.id = (lastIdFecha + 1).ToString();
                newFechaArchivo.fecha = dateFile.ToString("yyyy-MM-dd");
                newFechaArchivo.idLibro = newArchivo.idLibro;
                newFechaArchivo.idArchivo = newArchivo.idArchivo;

                if (isStartingExercise)
                {
                    firstPageNumber = 1;
                    firstEntryNumber = 1;
                }
                else if(isPreviousInList)
                {
                    var numbersPrevious = elFormulario.getPreviousNumbers(i);
                    firstPageNumber = numbersPrevious[0] + 1;
                    firstEntryNumber = numbersPrevious[1] + 1;
                }
                else if(lastNumbersBD.Count > 0)
                {
                    firstPageNumber = (Int32.Parse(lastNumbersBD[0][0]) + 1);
                    firstEntryNumber = (Int32.Parse(lastNumbersBD[0][1]) + 1);
                }
                else
                {
                    //Ask for the PageNumber and Entry
                    firstPageNumber = AskForNumber($"Escribir número de Folio de libro {newArchivo.idLibro}, fecha {newFechaArchivo.fecha} y fraccion {newArchivo.fraccion}.") ;
                    if(firstPageNumber == -1)
                    {
                        Application.Current.Shutdown();
                    }
                    firstEntryNumber = AskForNumber($"Escribir número de Asiento de libro {newArchivo.idLibro}, fecha {newFechaArchivo.fecha} y fraccion {newArchivo.fraccion}.");
                    if(firstEntryNumber == -1)
                    {
                        Application.Current.Shutdown();
                    }
                }

                var numberPageAndEntryFinal = elFormulario.proccessOneLibro(i, firstPageNumber, firstEntryNumber);
                var lastNumberPage = numberPageAndEntryFinal[0];
                var lastEntry = numberPageAndEntryFinal[1];

                newArchivo.folioI = firstPageNumber.ToString();
                newArchivo.folioF = lastNumberPage.ToString();
                newArchivo.asientoI = firstEntryNumber.ToString();
                newArchivo.asientoF = lastEntry.ToString();

                elFormulario.setLastNumbers(lastNumberPage, lastEntry, i);
                listQuerys.Add(createQArchivo(newArchivo));
                listQuerys.Add(createQFechaArchivo(newFechaArchivo));
            }
            _context.ExecuteQueriesWithTransaction(listQuerys);
            MainWindow newWindow = new MainWindow();
            newWindow.Show();
            // Close the current window
            this.Close();
        }
        
        /// <summary>
        /// Returns to the PreParteProceso window.
        /// </summary>
        private void volverSeleccion_Click(object sender, RoutedEventArgs e)
        {
            PreParteProcesos newWindow = new PreParteProcesos();
            newWindow.Show();

            // Close the current window
            this.Close();
        }
    }
}
