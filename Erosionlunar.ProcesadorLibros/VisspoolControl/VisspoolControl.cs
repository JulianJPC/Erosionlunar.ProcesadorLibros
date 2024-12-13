using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Erosionlunar.ProcesadorLibros.VisspoolControl
{
    public class VisspoolControl
    {
        private string provider;
        private string dirC;
        private string dirH;
        private string dirI;
        private string dirTest;
        private string dirCNew;
        private string dirHNew;
        private string dirINew;
        private string dirTestNew;

        public VisspoolControl(List<string> lasDirs)
        {
            dirC = lasDirs[0];
            dirH = lasDirs[1];
            dirI = lasDirs[2];
            dirTest = lasDirs[3];
            dirCNew = lasDirs[4];
            dirHNew = lasDirs[5];
            dirINew = lasDirs[6];
            dirTestNew = lasDirs[7];
            provider = "Provider=Microsoft.Jet.OLEDB.4.0; ";
        }
        private void executeToMDB(string pathMdb, string query)
        {
            OleDbConnection con = new OleDbConnection(provider + "Data Source = " + pathMdb);
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = con;
            cmd.CommandText = query;
            con.Open(); // open the connection
                        //OleDbDataReader dr = cmd.ExecuteNonQuery();
            cmd.ExecuteNonQuery();
            con.Close();
        }
        private void executeToMDBConConn(OleDbCommand cmd, string linea, string numeroHoja)
        {
            cmd.Parameters.AddWithValue("@linea", linea);
            cmd.Parameters.AddWithValue("@numeroHoja", numeroHoja);

            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
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
        private string getFechaOfPath(string path)
        {
            string fecha = null;
            var fileName = Path.GetFileName(path);
            Match match = Regex.Match(fileName, @"\d{4}");
            if (match.Success)
            {
                fecha = match.Value;
            }
            return fecha;
        }
        private string getNombreCOfPath(string path)
        {
            string nombreC = null;
            var fileName = Path.GetFileName(path);
            Match match = Regex.Match(fileName, @"^[A-Za-z]+");
            if (match.Success)
            {
                nombreC = match.Value;
            }
            return nombreC;
        }
        public string Visspool(string pathAlArchivo, int cantidadFolios, string fraccion, Regex elRegex, string nombreLibro)
        {
            string nombreLibroCorto = getNombreCOfPath(pathAlArchivo);
            string fecha = getFechaOfPath(pathAlArchivo);
            string mes = fecha.Substring(0, 2);
            string year = fecha.Substring(2, 2);
            fecha = "20" + year + "-" + mes + "-01 00:00:00";
            List<string> pathMDBOriginales = new List<string>();
            pathMDBOriginales.Add(dirC);
            pathMDBOriginales.Add(dirH);
            pathMDBOriginales.Add(dirI);
            pathMDBOriginales.Add(dirTest);
            List<string> pathMDBNuevos = new List<string>();
            pathMDBNuevos.Add(dirCNew);
            pathMDBNuevos.Add(dirHNew);
            pathMDBNuevos.Add(dirINew);
            pathMDBNuevos.Add(dirTestNew);
            foreach (string pathUnArchivo in pathMDBNuevos)
            {
                File.Delete(pathUnArchivo);
            }
            for (int indice = 0; indice < pathMDBOriginales.Count; indice++)
            {
                File.Copy(pathMDBOriginales[indice], pathMDBNuevos[indice]);
            }
            string mutacion = DateTime.Now.ToString();
            var sha = new System.Security.Cryptography.SHA256Managed();
            byte[] textBytes = Encoding.UTF8.GetBytes(mutacion + pathAlArchivo);
            byte[] hashBytes = sha.ComputeHash(textBytes);
            mutacion = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            List<string> sqlOrdenes = new List<string>();
            sqlOrdenes.Add("DELETE * FROM Nombres;");
            sqlOrdenes.Add("DELETE * FROM Notas;");
            sqlOrdenes.Add("INSERT INTO Nombres([Nombre], [Campo], [WH], [HH]) VALUES(\'DESHABILITADO\', \'Indice1\', 350, 1);");
            sqlOrdenes.Add("CREATE TABLE Mutacion( [Id] TEXT);");
            sqlOrdenes.Add("INSERT INTO Mutacion([Id]) VALUES(\'" + mutacion + "\');");
            foreach (string ordenes in sqlOrdenes)
            {
                executeToMDB(pathMDBNuevos[3], ordenes);
            }
            executeToMDB(pathMDBNuevos[0], "DELETE * FROM Conbinada;");
            executeToMDB(pathMDBNuevos[2], "DELETE * FROM Indices;");
            executeToMDB(pathMDBNuevos[1], "DELETE * FROM Libro;");

            int tope = 350;
            int dummyCount = 1;
            //string hoja = "";
            OleDbConnection con = new OleDbConnection(provider + "Data Source = " + pathMDBNuevos[1]);
            OleDbConnection conTwo = new OleDbConnection(provider + "Data Source = " + pathMDBNuevos[3]);
            OleDbCommand cmd = new OleDbCommand();
            OleDbCommand cmdThree = new OleDbCommand();

            cmd.Connection = con;

            cmdThree.Connection = conTwo;
            con.Open();
            conTwo.Open();

            //cmd.CommandText = "INSERT INTO [Libro]([Periodo], [Hoja], [HojaNro], [Nombre]) VALUES(@periodo, @linea, @counter, @libroNombre);";

            cmdThree.CommandText = "UPDATE Nombres SET [HH] = @parametro;";
            cmd.CommandText = "INSERT INTO Libro(Periodo, Hoja, HojaNro, Nombre) VALUES(?, ?, ?, ?);";
            cmd.Parameters.Add("Periodo", OleDbType.VarWChar, 100);
            cmd.Parameters.Add("Hoja", OleDbType.VarWChar, 30000);
            cmd.Parameters.Add("HojaNro", OleDbType.Numeric, 40000);
            cmd.Parameters.Add("Nombre", OleDbType.VarWChar, 500);
            cmd.Parameters[2].Scale = 2;
            cmd.Prepare();


            string[] AllText = File.ReadAllLines(pathAlArchivo, Encoding.GetEncoding(1252));
            List<string> listaBien = new List<string>(AllText);
            AllText = null;

            List<string> listaPaginas = Enumerable.Repeat("", cantidadFolios - 1).ToList();
            int finHoja = 0;
            int numeroLineas = listaBien.Count;
            int counterHojas = 0;

            for (int i = 0; i < numeroLineas; i++)
            {
                if (elRegex.IsMatch(listaBien[finHoja]) & i > 5)
                {
                    listaPaginas[counterHojas] = String.Join(new String(new char[] { (char)13, (char)10 }), listaBien.Take(finHoja).ToArray());
                    listaBien.RemoveRange(0, finHoja);
                    counterHojas += 1;
                    finHoja = 0;
                }
                finHoja += 1;
            }
            listaPaginas.Add("");
            listaPaginas[cantidadFolios - 1] = String.Join(new String(new char[] { (char)13, (char)10 }), listaBien.ToArray());

            int numeroPaginas = listaPaginas.Count;
            for (int i = 0; i < numeroPaginas; i++)
            {
                if (listaPaginas[0] != "")
                {
                    while (listaPaginas[0].Length > tope)
                    {
                        int nuevoParametro = listaPaginas[0].Length / 350 + 1;
                        tope = nuevoParametro * 350;
                        cmdThree.Parameters.AddWithValue("@parametro", nuevoParametro);
                        cmdThree.ExecuteNonQuery();
                        cmdThree.Parameters.Clear();
                    }
                    dummyCount = insertarEnBasePreparado(cmd, dummyCount, listaPaginas[0].ToString(), fecha, nombreLibro);
                }
                listaPaginas.RemoveAt(0);
            }
            //dummyCount = insertarEnBasePreparado(cmd, dummyCount, hoja, fecha, nombreLibro);

            con.Close();
            conTwo.Close();
            listaPaginas = null;
            GC.Collect();

            if (dummyCount - 1 != cantidadFolios)
            {
                Console.WriteLine(nombreLibro);
                Console.WriteLine(fecha);
                Console.WriteLine("El visspool tiene:" + (dummyCount - 1).ToString() + " y se supone que tiene que tener: " + cantidadFolios.ToString());
                Environment.Exit(0);
            }
            string dirVisspool = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(pathAlArchivo)), "visspool");
            Directory.CreateDirectory(dirVisspool);
            string dirInical = Path.Combine(dirVisspool, sacarCaracteres(nombreLibro) + mes + year);
            string dirCarpetaArchivos = Path.Combine(dirInical, sacarCaracteres(nombreLibroCorto) + mes + year + "P" + fraccion);
            Directory.CreateDirectory(dirVisspool);
            Directory.CreateDirectory(dirInical);
            Directory.CreateDirectory(dirCarpetaArchivos);
            string pathArchivoPrincipal = Path.Combine(dirInical, nombreLibroCorto + mes + year + "P" + fraccion + ".MDB");
            File.Copy(pathMDBNuevos[3], pathArchivoPrincipal);
            File.Copy(pathMDBNuevos[2], Path.Combine(dirCarpetaArchivos, "I.MDB"));
            File.Copy(pathMDBNuevos[1], Path.Combine(dirCarpetaArchivos, "H.MDB"));
            File.Copy(pathMDBNuevos[0], Path.Combine(dirCarpetaArchivos, "C.MDB"));
            return CalculateMD5(pathArchivoPrincipal);

        }
        private int insertarEnBasePreparado(OleDbCommand cmd, int dummyCount, string hoja, string fecha, string nombreLibro)
        {
            cmd.Parameters[0].Value = fecha;
            cmd.Parameters[1].Value = hoja;
            cmd.Parameters[2].Value = dummyCount;
            cmd.Parameters[3].Value = nombreLibro;
            cmd.ExecuteNonQuery();
            return dummyCount + 1;
        }
        private string sacarCaracteres(string elString)
        {
            string nuevoString = elString.Replace(".", " ");
            nuevoString = nuevoString.Replace(":", "");
            nuevoString = nuevoString.Replace("<", "");
            nuevoString = nuevoString.Replace(">", "");
            nuevoString = nuevoString.Replace("\"", "");
            nuevoString = nuevoString.Replace("\\", "");
            nuevoString = nuevoString.Replace("/", "");
            nuevoString = nuevoString.Replace("|", "");
            nuevoString = nuevoString.Replace("?", "");
            nuevoString = nuevoString.Replace("*", "");
            return nuevoString;
        }
    }
}
