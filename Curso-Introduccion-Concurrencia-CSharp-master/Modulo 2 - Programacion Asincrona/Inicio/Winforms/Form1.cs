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
            loadingGIF.Visible = true;
            await Esperar(); //Cuando se ejecuta el await, el hilo principal se libera y se ejecuta el metodo Esperar en otro hilo.
            var nombre = txtInput.Text;
            try
            {
                var saludo = await ObtenerSaludo(nombre);
                MessageBox.Show(saludo);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("HttpRequestException!");
                MessageBox.Show(ex.Message);
            }
                        
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
    }
}
