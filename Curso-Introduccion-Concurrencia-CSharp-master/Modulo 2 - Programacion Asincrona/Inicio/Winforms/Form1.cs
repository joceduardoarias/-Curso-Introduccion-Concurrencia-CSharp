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
        private string apiURL;
        private HttpClient client;
        public Form1()
        {
            InitializeComponent();
            apiURL = "https://localhost:44313";
            client = new HttpClient();
        }

        private async void btnIniciar_Click(object sender, EventArgs e)
        {   
            Console.WriteLine("Iniciando...");
            loadingGIF.Visible = true;
            // Aquí ocurre un problema de deadlock
            // ObtenerValor() es un método asincrónico, pero se está esperando su resultado de manera sincrónica con `.Result`.
            // En una aplicación de WinForms, el hilo principal maneja la UI y tiene un "contexto de sincronización".
            // Cuando `ObtenerValor().Result` se ejecuta, el hilo principal queda bloqueado esperando el resultado.
            // Sin embargo, `ObtenerValor` intenta reanudar su ejecución en el mismo hilo principal cuando `await Esperar();` finaliza.
            // Como el hilo principal está bloqueado esperando `.Result`, el código nunca se reanuda, generando un *deadlock*.
            var valor = ObtenerValor().Result; 
            MessageBox.Show("Pasaron los 5 segundos");
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

        private async Task<string> ObtenerValor()
        {
            await Esperar();
            return "Hola Mundo"; 
        }
    }
}
