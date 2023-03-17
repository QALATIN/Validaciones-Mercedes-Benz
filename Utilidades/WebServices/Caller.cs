using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LatinID.Utilidades.WebServices
{
    public class Caller
    {
        private string _url = string.Empty;
        public Caller(string url)
        {
            _url = url;
        }


        public IEnumerable<T> GetCatalog<T>(string invoke, params string[] parametros)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                //HTTP GET
                string addInvoke = string.Join("/", parametros);
                if (!string.IsNullOrEmpty(addInvoke))
                    invoke += "/" + addInvoke;
                //  var responseTask = client.GetAsync("CatPerfiles");
                var responseTask = client.GetAsync(invoke);

                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    using (Task<Stream> s = result.Content.ReadAsStreamAsync())
                    {

                        s.Wait();
                        using (StreamReader lectura = new StreamReader(s.Result))
                        {
                            string resultado = lectura.ReadToEnd();
                            //         ListadoPerfiles = JsonConvert.DeserializeObject<IEnumerable<CatPerfile>>(resultado);

                         var datos =  JsonConvert.DeserializeObject<IEnumerable<T>>(resultado, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });



                            return datos;
                        }
                    }
                }
                else //web api sent error response 
                {
                    //log response status here..
                    return Enumerable.Empty<T>();
                    //   ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            //     return string.Empty;
        }

        public async Task<T> GetSingleItemAsync<T>(string invoke, params string[] parametros)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                //HTTP GET
                string addInvoke = string.Join("/", parametros);
                if (!string.IsNullOrEmpty(addInvoke))
                    invoke += "/" + addInvoke;
                //  var responseTask = client.GetAsync("CatPerfiles");
                var responseTask = client.GetAsync(invoke);

                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    using (Stream s = await result.Content.ReadAsStreamAsync())
                    {
                        using (StreamReader lectura = new StreamReader(s))
                        {
                            string resultado = lectura.ReadToEnd();
                            //         ListadoPerfiles = JsonConvert.DeserializeObject<IEnumerable<CatPerfile>>(resultado);
                            return JsonConvert.DeserializeObject<T>(resultado);
                        }
                    }
                }
                else //web api sent error response 
                {
                    return default(T);
                }
            }
        }

        public bool isException { get; set; }
        public string Error { get; set; }

        public bool Put(string invoke, string id, string jsonStringContent)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_url);
                    using (HttpContent httpContent = new StringContent(jsonStringContent))
                    {
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        HttpResponseMessage response = client.PutAsync(invoke + "/" + id, httpContent).Result;


                        if (response.IsSuccessStatusCode)
                        {
                            Error = string.Empty;
                            isException = false;
                            return true;

                        }
                        else
                        {

                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isException = true;
                Error = ex.Message;
                return false;
            }
        }
    }
}
