using Newtonsoft.Json;

namespace ModeloDeDatos.Clases
{
    public class Georeferencia
    {
        [JsonProperty("success")]
        public bool EsCorrecto { get; set; }

        [JsonProperty("message")]
        public string Mensaje { get; set; }

        public string Map { get; set; }

    }
}
