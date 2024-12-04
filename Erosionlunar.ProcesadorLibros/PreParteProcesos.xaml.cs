using Erosionlunar.ProcesadorLibros.DB;
using Erosionlunar.ProcesadorLibros.Models;
using Mysqlx.Cursor;
using Mysqlx.Prepare;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Interaction logic for PreParteProcesos.xaml
    /// </summary>
    public partial class PreParteProcesos : Window
    {
        private readonly DBConector _context;
        private readonly string pathProcesos;
        public PreParteProcesos()
        {
            _context = new DBConector();
            pathProcesos = getPathProcesos();
            var prepartesSinProceso = getPrePartesSinParte();
            getArchivosPrePartes(prepartesSinProceso);
            
            InitializeComponent();
            PopulateGrid(prepartesSinProceso);
            
        }



        private void getArchivosPrePartes(List<List<string>> losPrePartes)
        {
            for(int i = 0; i < losPrePartes.Count; i++)
            {
                string dirArchivos = System.IO.Path.Combine(pathProcesos, losPrePartes[i][0], "txt");
                if (!Directory.Exists(dirArchivos))
                {
                    losPrePartes[i].Add("No existe Carpeta Archivos");
                }
                else
                {
                    var archivosEnCarpeta = Directory.GetFiles(dirArchivos).ToList();
                    var carpetasEnDirectorio = Directory.GetDirectories(dirArchivos).ToList();
                    foreach(string unaCarpeta in carpetasEnDirectorio)
                    {
                        archivosEnCarpeta.AddRange(Directory.GetFiles(unaCarpeta).ToList());
                    }
                    
                    losPrePartes[i].Add("");
                    foreach(string dirArchivo in archivosEnCarpeta)
                    {
                        string archivo = System.IO.Path.GetFileName(dirArchivo);
                        losPrePartes[i][3] += " | " + archivo;
                    }
                    
                    
                }
            }
        }
        private List<List<string>> getPrePartesSinParte()
        {
            string query = @"SELECT 
                                    PreParte.numeroP, 
                                    PreParte.idEmpresa, 
                                    Empresas.nombreE    
                            FROM 
                                    PreParte
                            LEFT JOIN 
                                    Partes ON Partes.numeroP = PreParte.numeroP
                            INNER JOIN 
                                    Empresas ON PreParte.idEmpresa = Empresas.idEmpresa
                            WHERE 
                                    Partes.numeroP IS NULL;";
            List<string> columna = new List<string> { "numeroP", "idEmpresa", "nombreE" };
            return _context.readQueryList(query, columna);
        }
        private string getPathProcesos()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame = 1;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna)[0];
        }
        private void PopulateGrid(List<List<string>> prePartesInfo)
        {
            DynamicGrid.Children.Clear();
            DynamicGrid.RowDefinitions.Clear();
            DynamicGrid.ColumnDefinitions.Clear();

            // Define two columns for the Grid
            DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            // Define rows dynamically
            for (int i = 0; i < prePartesInfo.Count; i++)
            {
                // Add a new row definition
                DynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });


                // Create the first TextBlock (NombreEmpresa)
                TextBlock numeroParteTextBlock = new TextBlock
                {
                    Text = prePartesInfo[i][0],
                    Margin = new Thickness(5)
                };
                Grid.SetRow(numeroParteTextBlock, i);
                Grid.SetColumn(numeroParteTextBlock, 0);
                // Create the first TextBlock (NombreEmpresa)
                TextBlock IdEmpresaTextBlock = new TextBlock
                {
                    Text = prePartesInfo[i][1],
                    Margin = new Thickness(5)
                };
                Grid.SetRow(IdEmpresaTextBlock, i);
                Grid.SetColumn(IdEmpresaTextBlock, 1);

                // Create the first TextBlock (NombreEmpresa)
                TextBlock nombreEmpresaTextBlock = new TextBlock
                {
                    Text = prePartesInfo[i][2],
                    Margin = new Thickness(5)
                };
                Grid.SetRow(nombreEmpresaTextBlock, i);
                Grid.SetColumn(nombreEmpresaTextBlock, 2);

                // Create the second TextBlock (ArchivosEmpresa)
                TextBlock archivosEmpresaTextBlock = new TextBlock
                {
                    Text = prePartesInfo[i][3],
                    Margin = new Thickness(5)
                };
                Grid.SetRow(archivosEmpresaTextBlock, i);
                Grid.SetColumn(archivosEmpresaTextBlock, 3);

                // Add the TextBlocks to the Grid
                DynamicGrid.Children.Add(numeroParteTextBlock);
                DynamicGrid.Children.Add(IdEmpresaTextBlock);
                DynamicGrid.Children.Add(nombreEmpresaTextBlock);
                DynamicGrid.Children.Add(archivosEmpresaTextBlock);
            }

            
        }
    }
}
