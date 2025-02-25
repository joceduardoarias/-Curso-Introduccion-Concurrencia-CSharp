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
        private CancellationTokenSource cancellationToken;
        public Form1()
        {
            InitializeComponent();
            apiUrl = "https://localhost:44313/";
            client = CreateHttpClient();
        }

        private async void btnIniciar_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Iniciando...");

            cancellationToken = new CancellationTokenSource();

            var reportarProgreso = new Progress<int>(ReportarProgreso);
            loadingGIF.Visible = true;
            
            var tarjetas = await ObtenerTarjetas(1000);
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                await ProcesarTarjeta(tarjetas,reportarProgreso,cancellationToken.Token); // El orden en que procesas las tarjetas no se puede determinar.
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("HttpRequestException!");
                MessageBox.Show(ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Operación cancelada!");
                MessageBox.Show("Operación cancelada!");
            }
            finally
            {
                //cancellationToken.Dispose();
            }

            MessageBox.Show($"Operación finalizada en {stopwatch.ElapsedMilliseconds / 1000.0} segundos");
            loadingGIF.Visible = false;
            Console.WriteLine("Finalizado!");
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            cancellationToken?.Cancel();
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
        private async Task ProcesarTarjeta(List<string> tarjetas, IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {   
            using var semaforo = new SemaphoreSlim(40);

            var tareas = new List<Task<HttpRequestMessage>>();
            
            tareas = tarjetas.Select(async tarjeta =>
            {
                try
                {
                    await semaforo.WaitAsync();
                    var content = new StringContent(JsonConvert.SerializeObject(tarjeta), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{apiUrl}tarjetas", content, cancellationToken);
                    
                    return response.RequestMessage;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }finally
                {
                    semaforo.Release();
                }
                
            }).ToList();
            
            var respuestasTareas =   Task.WhenAll(tareas);

            if (progress != null)
            {
                while (await Task.WhenAny(respuestasTareas, Task.Delay(3000)) != respuestasTareas)
                {
                    var tareasCompletadas = tareas.Count(t => t.IsCompleted);
                    var progreso = tareasCompletadas * 100 / tarjetas.Count;
                    progress.Report(progreso);
                }
            }
        }
        private void ReportarProgreso(int progreso)
        {
            pgProcesamiento.Value = progreso;
            Console.WriteLine($"Progreso: {progreso}");
        }
        public static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            return new HttpClient(handler);
        }
        private async void btnReintentar_Click(object sender, EventArgs e)
        {
            try
            {
                var contenido = await Reintentar(() => ObtenerSaludo("Juan"));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        // Reintentar es un método genérico que recibe una función que devuelve una tarea y la ejecuta hasta que se complete exitosamente
        // o hasta que se alcance el número de reintentos.
        private async Task<T> Reintentar<T>(Func<Task<T>> f, int reintentos = 3, int tiempoEspera = 1000)
        {
            for (int i = 0; i < reintentos -1; i++)
            {
                try
                {
                    await f();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en el intento {i + 1}: {ex.Message}");
                    await Task.Delay(tiempoEspera);
                }
            }

            return await f();
        }
    }
}
