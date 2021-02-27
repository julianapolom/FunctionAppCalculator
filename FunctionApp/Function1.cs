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
            //Declaración de variables correspondientes a los parámetros
            int a = 0;
            int b = 0;
            int? resultado = null;

            //Declaración de variables a usar para almacenamiento en caché
            ObjectCache tokenCache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;

            //Validación del tipo de método HTTP para ejecutar el proceso
            if(req.Method == "POST")
            {
                //Obtener el body de la petición
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                //Extraer parámetros a y b del body
                a = Convert.ToInt32(data?.a);
                b = Convert.ToInt32(data?.b);
                //Almacenar en caché el resultado de la suma con el identificador a+b
                tokenCache.Set(new CacheItem(a + "+" + b, a+b), policy);
            }
            else if (req.Method == "GET")
            {
                //Obtener los parámetros por QueryString
                a = Convert.ToInt32(req.Query["a"]);
                b = Convert.ToInt32(req.Query["b"]);
                //Consutar memoria caché para obtener el resultado de la suma
                CacheItem tokenContents = tokenCache.GetCacheItem(a + "+" + b);
                if(tokenContents != null)
                {
                    resultado = Convert.ToInt32(tokenContents?.Value);
                }
            }
            else
            {
                //Si no se ejecutan los métodos POST ó GET, se retorna un BadRequest
                return new BadRequestResult();
            }

            //Validación del tipo de método para generar el response
            if (req.Method == "POST")
            {
                return new OkObjectResult("Ejecución Exitosa");
            }
            else
            {
                return new OkObjectResult("Resultado:"+ resultado);
            }
        }
    }
}
