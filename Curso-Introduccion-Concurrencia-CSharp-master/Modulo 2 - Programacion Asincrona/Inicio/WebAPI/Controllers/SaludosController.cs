using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Helpers;
using ApplicationException = System.ApplicationException;

namespace WebAPI.Controllers
{
    [Route("saludos")]
    [ApiController]
    public class SaludosController : ControllerBase
    {
        [HttpGet("{nombre}")]
        public ActionResult<string> ObtenerSaludo(string nombre)
        {
            return $"Hola, {nombre}!";
        }

        [HttpGet("delay/{nombre}")]
        public async Task<ActionResult<string>> ObtenerSaludoConDelay(string nombre)
        {
            
            var esperar = RandomGen.NextDouble() * 10 + 1;
            // ❌ Problema: Se está llamando a un método `async void`, lo cual es peligroso
            // porque su ejecución ocurre en un hilo separado y no se puede capturar excepciones de manera adecuada.
            // Si este método falla, la excepción no podrá ser manejada correctamente y puede hacer que la aplicación falle inesperadamente.
            OperacionVoidAsync();
            return $"Hola, {nombre}!";
        }
        // ❌ Peligro de `async void`:
        // 1. No devuelve un `Task`, lo que significa que no se puede hacer `await` ni capturar excepciones.
        // 2. Si ocurre una excepción dentro del método, esta no puede ser manejada con `try-catch` en el llamador.
        // 3. Como la ejecución es independiente, no hay garantía de que el método se complete antes de que el request termine.
        private async void OperacionVoidAsync()
        {
            await Task.Delay(1000);
            // ❌ Esta excepción no podrá ser atrapada por el llamador, lo que puede hacer que la aplicación se caiga silenciosamente.
            throw new ApplicationException();
        }
        [HttpGet("adios/{nombre}")]
        public async Task<ActionResult<string>> ObtenerAdiosConDelay(string nombre)
        {
            var esperar = RandomGen.NextDouble() * 10 + 1;
            await Task.Delay((int)esperar * 1000);
            return $"Bye, {nombre}!";
        }
    }
}
