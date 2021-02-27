using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.Caching;

namespace FunctionApp
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "sumar")] HttpRequest req,
            ILogger log)
        {
            //Declaraci�n de variables correspondientes a los par�metros
            int a = 0;
            int b = 0;
            int? resultado = null;

            //Declaraci�n de variables a usar para almacenamiento en cach�
            ObjectCache tokenCache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;

            //Validaci�n del tipo de m�todo HTTP para ejecutar el proceso
            if(req.Method == "POST")
            {
                //Obtener el body de la petici�n
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                //Extraer par�metros a y b del body
                a = Convert.ToInt32(data?.a);
                b = Convert.ToInt32(data?.b);
                //Almacenar en cach� el resultado de la suma con el identificador a+b
                tokenCache.Set(new CacheItem(a + "+" + b, a+b), policy);
            }
            else if (req.Method == "GET")
            {
                //Obtener los par�metros por QueryString
                a = Convert.ToInt32(req.Query["a"]);
                b = Convert.ToInt32(req.Query["b"]);
                //Consutar memoria cach� para obtener el resultado de la suma
                CacheItem tokenContents = tokenCache.GetCacheItem(a + "+" + b);
                if(tokenContents != null)
                {
                    resultado = Convert.ToInt32(tokenContents?.Value);
                }
            }
            else
            {
                //Si no se ejecutan los m�todos POST � GET, se retorna un BadRequest
                return new BadRequestResult();
            }

            //Validaci�n del tipo de m�todo para generar el response
            if (req.Method == "POST")
            {
                return new OkObjectResult("Ejecuci�n Exitosa");
            }
            else
            {
                return new OkObjectResult("Resultado:"+ resultado);
            }
        }
    }
}
