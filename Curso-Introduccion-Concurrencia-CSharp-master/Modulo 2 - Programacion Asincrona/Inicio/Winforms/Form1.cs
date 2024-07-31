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
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnIniciar_Click(object sender, EventArgs e)
        {   
            Console.WriteLine("Iniciando...");
            await Esperar(); //Cuando se ejecuta el await, el hilo principal se libera y se ejecuta el metodo Esperar en otro hilo.
            MessageBox.Show("Pasaron los 5 segundos");
            Console.WriteLine("Finalizado!");
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
        }
        private async Task Esperar()
        {
            await Task.Delay(5000);
        }
    }
}
