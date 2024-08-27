using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winforms
{
    public partial class Form1 : Form
    {
        private string apiUrl;
        private HttpClient client;
        public Form1()
        {
            InitializeComponent();
            apiUrl = "https://localhost:44313/";
            client = new HttpClient();
        }

        private async void btnIniciar_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Iniciando...");

            var reportarProgreso = new Progress<int>(ReportarProgreso);
            loadingGIF.Visible = true;
            
            var tarjetas = await ObtenerTarjetas(250);
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                await ProcesarTarjeta(tarjetas,reportarProgreso); // El orden en que procesas las tarjetas no se puede determinar.
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("HttpRequestException!");
                MessageBox.Show(ex.Message);
            }
            MessageBox.Show($"Operación finalizada en {stopwatch.ElapsedMilliseconds / 1000.0} segundos");
            loadingGIF.Visible = false;
            Console.WriteLine("Finalizado!");
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
        }
        private async Task Esperar()
        {
            await Task.Delay(5000);
        }
        private async Task<string> ObtenerSaludo(string nombre)
        {
            var response = await client.GetAsync($"{apiUrl}saludos1/{nombre}");
            response.EnsureSuccessStatusCode(); // Lanza una excepción si el código de estado no es exitoso
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        private async Task <List<string>> ObtenerTarjetas(int cantidadDeTarjetas)
        {   
            // Si se esta esparando obtener muchos registros creo un nuevo hilo para no bloquear el Hilo 'UI'            
            return await Task.Run(() =>
            {
                var tarjetas = new List<string>();

                for (int i = 0; i < cantidadDeTarjetas; i++)
                {
                    tarjetas.Add(i.ToString().PadLeft(16, '0'));
                }
                
                return tarjetas;
            });
            
            
        }
        private async Task ProcesarTarjeta(List<string> tarjetas, IProgress<int> progress = null)
        {   
            using var semaforo = new SemaphoreSlim(40);

            var tareas = new List<Task<HttpRequestMessage>>();
            var indice = 0;

            tareas = tarjetas.Select(async tarjeta =>
            {
                await semaforo.WaitAsync();
                var content = new StringContent(JsonConvert.SerializeObject(tarjeta), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiUrl}tarjetas", content);
                semaforo.Release();
                
                if (progress != null)
                {
                    var progreso = Interlocked.Increment(ref indice) * 100 / tarjetas.Count;
                    progress.Report(progreso);
                }

                return response.RequestMessage;
            }).ToList();
            
            await Task.WhenAll(tareas); // Espera a que todas las tareas terminen

        }
        private void ReportarProgreso(int progreso)
        {
            pgProcesamiento.Value = progreso;
            Console.WriteLine($"Progreso: {progreso}");
        }
    }
}
