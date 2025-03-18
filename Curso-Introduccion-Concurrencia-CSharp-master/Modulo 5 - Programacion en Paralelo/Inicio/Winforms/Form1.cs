using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winforms
{
    public partial class Form1 : Form
    {
        private string apiURL;
        private HttpClient httpClient;
        private CancellationTokenSource cancellationTokenSource;
        public Form1()
        {
            InitializeComponent();
            apiURL = "https://localhost:44313";
            httpClient = new HttpClient();
        }

        private async void btnIniciar_Click(object sender, EventArgs e)
        {
            var directorioBase = AppDomain.CurrentDomain.BaseDirectory;
            var destinoBaseParalela = Path.Combine(directorioBase, @"Imagenes\resultado-Paralela");
            var destinoBaseSecuencial = Path.Combine(directorioBase, @"Imagenes\resultado-Secuencial");
            PrepararEjecucion(destinoBaseParalela, destinoBaseSecuencial);
            
            var images = ObtenerImagenes();
            
            //Procesamiento secuencial
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var image in images)
            {
                await ProcesarImagenes(image, destinoBaseSecuencial);
            }
            
            Console.WriteLine("Secuencial: duración en segundos: {0}", stopwatch.Elapsed.TotalSeconds);
            
            stopwatch.Restart();
            
            //Procesamiento paralelo
            var tasks = images.Select(image => ProcesarImagenes(image, destinoBaseParalela));
            await Task.WhenAll(tasks);
            
            Console.WriteLine("Paralelo: duración en segundos: {0}", stopwatch.Elapsed.TotalSeconds);
            stopwatch.Stop();
        }
        private void btnCancelar_Click(object sender, EventArgs e)
        {
        }
        private void PrepararEjecucion(string destinoBaseParalela, string destinoBaseSecuencial)
        {
            if (!Directory.Exists(destinoBaseParalela))
            {
                Directory.CreateDirectory(destinoBaseParalela);
            }
            if (!Directory.Exists(destinoBaseSecuencial))
            {
                Directory.CreateDirectory(destinoBaseSecuencial);
            }
            BorrarArchivos(destinoBaseParalela, destinoBaseSecuencial);
        }
        private void BorrarArchivos(string destinoBaseParalela, string destinoBaseSecuencial)
        {
            foreach (var file in Directory.GetFiles(destinoBaseParalela))
            {
                File.Delete(file);
            }
            foreach (var file in Directory.GetFiles(destinoBaseSecuencial))
            {
                File.Delete(file);
            }
        }
        private static List<Imagen> ObtenerImagenes()
        {
            var imagenes = new List<Imagen>();
            imagenes.Add(new Imagen { Nombre = "imagen1.jpg", Url = "https://randomuser.me/api/portraits/men/75.jpg" });
            imagenes.Add(new Imagen { Nombre = "imagen2.jpg", Url = "https://randomuser.me/api/portraits/men/76.jpg" });
            imagenes.Add(new Imagen { Nombre = "imagen3.jpg", Url = "https://randomuser.me/api/portraits/men/77.jpg" });
            imagenes.Add(new Imagen { Nombre = "imagen4.jpg", Url = "https://randomuser.me/api/portraits/men/78.jpg" });
            imagenes.Add(new Imagen { Nombre = "imagen5.jpg", Url = "https://randomuser.me/api/portraits/men/79.jpg" });
            return imagenes;
        }
        private async Task ProcesarImagenes(Imagen imagen, string directorio)
        {
            var response = await httpClient.GetAsync(imagen.Url);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Bitmap bitmap = new Bitmap(Image.FromStream(new MemoryStream(bytes)));
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            var destino = Path.Combine(directorio, imagen.Nombre);
            bitmap.Save(destino);
        }
    }
}
