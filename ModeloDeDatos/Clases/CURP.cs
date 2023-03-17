using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ModeloDeDatos.Clases
{
    public enum EstatusCURP
    {
        AN,
        AH,
        CRA,
        RCN,
        RCC,
        BD,
        BDA,
        BCC,
        BCN,
        NA,
    }

    public class CURP
    {
        [JsonProperty("curp")]

        public int CodigoError { get; set; }

        public int Transaccion { get; set; }

        [JsonProperty("success")]
        public bool EsCorrecto { get; set; }

        [JsonProperty("message")]
        public string Mensaje { get; set; }

        [JsonProperty("responseData")]
        public Data ResponseData { get; set; }

        public class Documento
        {
            public string entidadRegistro { get; set; }
            public string tomo { get; set; }
            public string claveMunicipioRegistro { get; set; }

            [JsonProperty("anioReg")]
            public string AnioRegistro { get; set; }


            [JsonProperty("claveEntidadRegistro")]
            public string ClaveEntidad { get; set; }
            public string foja { get; set; }

            [JsonProperty("numActa")]
            public string NumeroDeActa { get; set; }
            public string libro { get; set; }

            [JsonProperty("municipioRegistro")]
            public string MunicipioRegistro { get; set; }
        }

        public class Data
        {
            [JsonProperty("curp")]
            public string CURP { get; set; }

            [JsonProperty("codigoValidacion")]
            public string CodigoDeValidacion { get; set; }

            [JsonProperty("codigoMensaje")]
            public string CodigoDeMensaje { get; set; }

            [JsonProperty("nombre")]
            public string Nombres { get; set; }

            [JsonProperty("apellidoPaterno")]
            public string ApellidoPaterno { get; set; }

            [JsonProperty("apellidoMaterno")]
            public string ApellidoMaterno { get; set; }

            private string _s = "";
            [JsonProperty("sexo")]
            public string Sexo
            {
                get
                {
                    return _s;
                }
                set
                {
                    if (value == "HOMBRE")
                        _s = "H";
                    else if (value == "MUJER")
                        _s = "M";
                    else
                        _s = string.Empty;
                }
            }

            protected string NombreEstadoNacimiento = string.Empty;
            protected int iEntidad = -1;
            protected string sEntidad = string.Empty;
            [JsonProperty("estadoNacimiento")]
            public string EstadoNacimiento
            {
                get { return NombreEstadoNacimiento; }
                set
                {
                    NombreEstadoNacimiento = value;
                    KeyValuePair<int, string> item = GetClaveEstado(value);
                    iEntidad = item.Key;
                    sEntidad = item.Value;
                }
            }

            public string ClaveEntidad
            {
                get
                {
                    return sEntidad;
                }
            }
            public int NumeroEntidad
            {
                get
                {
                    return iEntidad;
                }
            }

            private Dictionary<int, string> CatalogoEstados = new Dictionary<int, string>()
            {
                { 1, "AS"},{2, "BC"},{3, "BS"},{4, "CC"},{5, "CS"},{6, "CH"},{7, "CL"},{8, "CM"},{9, "DF"},
                {10, "DG"},{11, "GT"},{12, "GR"},{13, "HG"},{14, "JC"},{15, "MC"},{16, "MN"},{17, "MS"},{18, "NT"},
                {19, "NL"},{20, "OC"},{21, "PL"},{22, "QT"},{23, "QR"},{24, "SP"},{25, "SL"},{26, "SR"},{27, "TC"},
                {28, "TS"},{29, "TL"},{30, "VZ"},{31, "YN"},{32, "ZS"},{33, "NE" },{-1,"" }
            };

            private KeyValuePair<int, string> GetClaveEstado(string estado)
            {
                if (string.IsNullOrEmpty(estado))
                    return CatalogoEstados.FirstOrDefault(x => x.Key == -1);

                switch (estado.ToUpper())
                {
                    case "AGUASCALIENTES": return CatalogoEstados.FirstOrDefault(x => x.Key == 1);
                    case "BAJA CALIFORNIA": return CatalogoEstados.FirstOrDefault(x => x.Key == 2);
                    case "BAJA CALIFORNIA SUR": return CatalogoEstados.FirstOrDefault(x => x.Key == 3);
                    case "CAMPECHE": return CatalogoEstados.FirstOrDefault(x => x.Key == 4);
                    case "CHIAPAS": return CatalogoEstados.FirstOrDefault(x => x.Key == 5);
                    case "CHIHUAHUA": return CatalogoEstados.FirstOrDefault(x => x.Key == 6);
                    case "COAHUILA DE ZARAGOZA": return CatalogoEstados.FirstOrDefault(x => x.Key == 7);
                    case "COLIMA": return CatalogoEstados.FirstOrDefault(x => x.Key == 8);
                    case "DISTRITO FEDERAL": return CatalogoEstados.FirstOrDefault(x => x.Key == 9);
                    case "DURANGO": return CatalogoEstados.FirstOrDefault(x => x.Key == 10);
                    case "GUANAJUATO": return CatalogoEstados.FirstOrDefault(x => x.Key == 11);
                    case "GUERRERO": return CatalogoEstados.FirstOrDefault(x => x.Key == 12);
                    case "HIDALGO": return CatalogoEstados.FirstOrDefault(x => x.Key == 13);
                    case "JALISCO": return CatalogoEstados.FirstOrDefault(x => x.Key == 14);
                    case "MEXICO": return CatalogoEstados.FirstOrDefault(x => x.Key == 15);
                    case "MICHOACAN DE OCAMPO": return CatalogoEstados.FirstOrDefault(x => x.Key == 16);
                    case "MORELOS": return CatalogoEstados.FirstOrDefault(x => x.Key == 17);
                    case "NAYARIT": return CatalogoEstados.FirstOrDefault(x => x.Key == 18);
                    case "NUEVO LEON": return CatalogoEstados.FirstOrDefault(x => x.Key == 19);
                    case "OAXACA": return CatalogoEstados.FirstOrDefault(x => x.Key == 20);
                    case "PUEBLA": return CatalogoEstados.FirstOrDefault(x => x.Key == 21);
                    case "QUERETARO DE ARTEAGA": return CatalogoEstados.FirstOrDefault(x => x.Key == 22);
                    case "QUINTANA ROO": return CatalogoEstados.FirstOrDefault(x => x.Key == 23);
                    case "SAN LUIS POTOSI": return CatalogoEstados.FirstOrDefault(x => x.Key == 24);
                    case "SINALOA": return CatalogoEstados.FirstOrDefault(x => x.Key == 25);
                    case "SONORA": return CatalogoEstados.FirstOrDefault(x => x.Key == 26);
                    case "TABASCO": return CatalogoEstados.FirstOrDefault(x => x.Key == 27);
                    case "TAMAULIPAS": return CatalogoEstados.FirstOrDefault(x => x.Key == 28);
                    case "TLAXCALA": return CatalogoEstados.FirstOrDefault(x => x.Key == 29);
                    case "VERACRUZ": return CatalogoEstados.FirstOrDefault(x => x.Key == 30);
                    case "YUCATAN": return CatalogoEstados.FirstOrDefault(x => x.Key == 31);
                    case "ZACATECAS": return CatalogoEstados.FirstOrDefault(x => x.Key == 32);
                    case "NACIDO EN EL EXTRANJERO": return CatalogoEstados.FirstOrDefault(x => x.Key == 33);
                }
                return CatalogoEstados.FirstOrDefault(x => x.Key == -1);
            }

            private string NombrePaisNacimiento = string.Empty;
            protected string _clvPais = string.Empty;
            [JsonProperty("paisNacimiento")]
            public string ClavePaisNacimiento
            {
                get { return _clvPais; }
                set { _clvPais = GetClavePais(value); NombrePaisNacimiento = value; }
            }

            public string PaisNacimiento
            {
                get
                {
                    return NombrePaisNacimiento;
                }
            }
            private string GetClavePais(string pais)
            {
                if (string.IsNullOrEmpty(pais))
                    return "";
                switch (pais.ToUpper())
                {
                    case "ARUBA": return "ABW";
                    case "AFGANISTAN": return "AFG";
                    case "ANGOLA": return "AGO";
                    case "ALBANIA": return "ALB";
                    case "ANDORRA": return "AND";
                    case "EMIRATOS ARABES UNIDOS": return "ARE";
                    case "ARGENTINA": return "ARG";
                    case "ARMENIA": return "ARM";
                    case "ANTIGUA Y BARBUDA": return "ATG";
                    case "AUSTRALIA": return "AUS";
                    case "AUSTRIA": return "AUT";
                    case "AZERBAIYAN": return "AZE";
                    case "BURUNDI": return "BDI";
                    case "BELGICA": return "BEL";
                    case "BENIN": return "BEN";
                    case "BURKINA FASO": return "BFA";
                    case "BANGLADESH": return "BGD";
                    case "BULGARIA": return "BGR";
                    case "BAHREIN": return "BHR";
                    case "BAHAMAS": return "BHS";
                    case "BOSNIA Y HERZEGOVINA": return "BIH";
                    case "BELARUS": return "BLR";
                    case "BELICE": return "BLZ";
                    case "BOLIVIA": return "BOL";
                    case "BRASIL": return "BRA";
                    case "BARBADOS": return "BRB";
                    case "BRUNEI DARUSSALAM": return "BRN";
                    case "BHUTAN": return "BTN";
                    case "BOTSWANA": return "BWA";
                    case "REPUBLICA CENTROAFRICANA": return "CAF";
                    case "CANADA": return "CAN";
                    case "COSTA DE MARFIL": return "CIV";
                    case "CAMERUN": return "CMR";
                    case "CONGO": return "COD";
                    case "ISLAS COOK": return "COK";
                    case "COLOMBIA": return "COL";
                    case "COMORAS": return "COM";
                    case "CABO VERDE": return "CPV";
                    case "COSTA RICA": return "CRI";
                    case "CUBA": return "CUB";
                    case "CHIPRE": return "CYP";
                    case "REPUBLICA CHECA": return "CZE";
                    case "SUIZA": return "CHE";
                    case "CHILE": return "CHL";
                    case "CHINA": return "CHN";
                    case "ALEMANIA": return "DEU";
                    case "DJIBOUTI": return "DJI";
                    case "DOMINICA": return "DMA";
                    case "DINAMARCA": return "DNK";
                    case "REPUBLICA DOMINICANA": return "DOM";
                    case "ARGELIA": return "DZA";
                    case "ECUADOR": return "ECU";
                    case "EGIPTO": return "EGY";
                    case "ESPAÑA": return "ESP";
                    case "ESTONIA": return "EST";
                    case "ETIOPIA": return "ETH";
                    case "FINLANDIA": return "FIN";
                    case "FIJI": return "FJI";
                    case "FRANCIA": return "FRA";
                    case "MICRONESIA": return "FSM";
                    case "GABON": return "GAB";
                    case "REINO UNIDO": return "GBR";
                    case "GEORGIA": return "GEO";
                    case "GHANA": return "GHA";
                    case "GIBRALTAR": return "GIB";
                    case "GUINEA": return "GIN";
                    case "GAMBIA": return "GMB";
                    case "GUINEA-BISSAU": return "GNB";
                    case "GUINEA ECUATORIAL": return "GNQ";
                    case "GRECIA": return "GRC";
                    case "GRANADA": return "GRD";
                    case "GROENLANDIA": return "GRL";
                    case "GUATEMALA": return "GTM";
                    case "GUYANA": return "GUY";
                    case "HONDURAS": return "HND";
                    case "CROACIA": return "HRV";
                    case "HAITI": return "HTI";
                    case "HUNGRIA": return "HUN";
                    case "INDONESIA": return "IDN";
                    case "INDIA": return "IND";
                    case "IRLANDA": return "IRL";
                    case "IRAN": return "IRN";
                    case "IRAQ": return "IRQ";
                    case "ISLANDIA": return "ISL";
                    case "ISRAEL": return "ISR";
                    case "ITALIA": return "ITA";
                    case "JAMAICA": return "JAM";
                    case "JORDANIA": return "JOR";
                    case "JAPON": return "JPN";
                    case "KAZAJSTAN": return "KAZ";
                    case "KENYA": return "KEN";
                    case "KIRGUISTAN": return "KGZ";
                    case "CAMBOYA": return "KHM";
                    case "KIRIBATI": return "KIR";
                    case "SAINT KITTS Y NEVIS": return "KNA";
                    case "COREA DEL SUR": return "KOR";
                    case "KUWAIT": return "KWT";
                    case "LAO": return "LAO";
                    case "LIBANO": return "LBN";
                    case "LIBERIA": return "LBR";
                    case "LIBIA": return "LBY";
                    case "SANTA LUCIA": return "LCA";
                    case "LIECHTENSTEIN": return "LIE";
                    case "SRI LANKA": return "LKA";
                    case "LESOTHO": return "LSO";
                    case "LITUANIA": return "LTU";
                    case "LUXEMBURGO": return "LUX";
                    case "LETONIA": return "LVA";
                    case "MARRUECOS": return "MAR";
                    case "MONACO": return "MCO";
                    case "MOLDOVA": return "MDA";
                    case "MADAGASCAR": return "MDG";
                    case "MALDIVAS": return "MDV";
                    case "MEXICO": return "MEX";
                    case "ISLAS MARSHALL": return "MHL";
                    case "MACEDONIA": return "MKD";
                    case "MALI": return "MLI";
                    case "MALTA": return "MLT";
                    case "MYANMAR": return "MMR";
                    case "MONGOLIA": return "MNG";
                    case "MOZAMBIQUE": return "MOZ";
                    case "MAURITANIA": return "MRT";
                    case "MARTINICA": return "MTQ";
                    case "MAURICIO": return "MUS";
                    case "MALAWI": return "MWI";
                    case "MALASIA": return "MYS";
                    case "NAMIBIA": return "NAM";
                    case "NIGER": return "NER";
                    case "NIGERIA": return "NGA";
                    case "NICARAGUA": return "NIC";
                    case "HOLANDA": return "NLD";
                    case "NORUEGA": return "NOR";
                    case "NEPAL": return "NPL";
                    case "NAURU": return "NRU";
                    case "NUEVA ZELANDIA": return "NZL";
                    case "OMAN": return "OMN";
                    case "PAKISTAN": return "PAK";
                    case "PANAMA": return "PAN";
                    case "PERU": return "PER";
                    case "FILIPINAS": return "PHL";
                    case "PALAU": return "PLW";
                    case "PAPUA NUEVA GUINEA": return "PNG";
                    case "POLONIA": return "POL";
                    case "PUERTO RICO": return "PRI";
                    case "COREA DEL NORTE": return "PRK";
                    case "PORTUGAL": return "PRT";
                    case "PARAGUAY": return "PRY";
                    case "QATAR": return "QAT";
                    case "RUMANIA": return "ROM";
                    case "RUSIA": return "RUS";
                    case "RWANDA": return "RWA";
                    case "ARABIA SAUDITA": return "SAU";
                    case "SUDAN": return "SDN";
                    case "SENEGAL": return "SEN";
                    case "SINGAPUR": return "SGP";
                    case "ISLAS SALOMON": return "SLB";
                    case "SIERRA LEONA": return "SLE";
                    case "EL SALVADOR": return "SLV";
                    case "SAN MARINO": return "SMR";
                    case "SOMALIA": return "SOM";
                    case "SANTO TOME Y PRINCIPE": return "STP";
                    case "SURINAME": return "SUR";
                    case "ESLOVENIA": return "SVN";
                    case "SUECIA": return "SWE";
                    case "SWAZILANDIA": return "SWZ";
                    case "SEYCHELLES": return "SYC";
                    case "SIRIA": return "SYR";
                    case "CHAD": return "TCD";
                    case "TOGO": return "TGO";
                    case "TAILANDIA": return "THA";
                    case "TAYIKISTAN": return "TJK";
                    case "TURKMENISTAN": return "TKM";
                    case "TONGA": return "TON";
                    case "TRINIDAD Y TOBAGO": return "TTO";
                    case "TUNEZ": return "TUN";
                    case "TURQUIA": return "TUR";
                    case "TUVALU": return "TUV";
                    case "TANZANIA": return "TZA";
                    case "UGANDA": return "UGA";
                    case "UCRANIA": return "UKR";
                    case "URUGUAY": return "URY";
                    case "ESTADOS UNIDOS": return "USA";
                    case "UZBEKISTAN": return "UZB";
                    case "VATICANO": return "VAT";
                    case "SAN VICENTE Y LAS GRANADINAS": return "VCT";
                    case "VENEZUELA": return "VEN";
                    case "VIET NAM": return "VNM";
                    case "VANUATU": return "VUT";
                    case "SAMOA": return "WSM";
                    case "ESLOVAQUIA": return "SVK";
                    case "YEMEN": return "YEM";
                    case "YUGOSLAVIA": return "YUG";
                    case "SUDAFRICA": return "ZAF";
                    case "ZAMBIA": return "ZMB";
                    case "ZIMBABWE": return "ZWE";
                }
                return "";
            }

            [JsonIgnore]
            public DateTime FechaNacimiento { get; set; }

            [JsonProperty("fechaNacimiento")]
            public string sFechaNacimiento
            {
                get { return FechaNacimiento.ToString("dd/MM/yyyy"); }
                // set { MyDate = DateTime.Parse(value); }
                set
                {
                    if (string.IsNullOrEmpty(value))
                        FechaNacimiento = DateTime.MinValue;
                    else
                        FechaNacimiento = DateTime.ParseExact(value, "dd/MM/yyyy", null);
                }
            }



            private string NotificacionEstatusCurp = string.Empty;

            [JsonIgnore]
            public EstatusCURP Estatus { get; set; }


            private bool _AlertaRojaPorEstatusCurp = false;

            public bool AlertaPorEstatus
            {
                get
                {
                    return _AlertaRojaPorEstatusCurp;
                }
            }
            public string estatusCurp
            {
                get
                {
                    return NotificacionEstatusCurp;
                }
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        NotificacionEstatusCurp = "La clave de estatus del CURP es nula o no identificada (NA).";
                        Estatus = EstatusCURP.NA; //no apluca
                        _AlertaRojaPorEstatusCurp = true;
                    }
                    else
                    {
                        switch (value.ToUpper())
                        {
                            case "AN": Estatus = EstatusCURP.AN; NotificacionEstatusCurp = $"Alta normal de CURP ({value})."; break;
                            case "AH": Estatus = EstatusCURP.AH; NotificacionEstatusCurp = $"Alta con homonimia ({value})."; break;
                            case "CRA": Estatus = EstatusCURP.CRA; NotificacionEstatusCurp = $"CURP Reactivada ({value})."; break;
                            case "RCN": Estatus = EstatusCURP.RCN; NotificacionEstatusCurp = $"Registro de cambio no afectando a CURP ({value})."; break;
                            case "RCC": Estatus = EstatusCURP.RCC; NotificacionEstatusCurp = $"Registro de cambio afectando a CURP ({value})."; break;
                            case "BD": Estatus = EstatusCURP.BD; NotificacionEstatusCurp = $"Baja por defunción ({value})."; _AlertaRojaPorEstatusCurp = true; break;
                            case "BDA": Estatus = EstatusCURP.BDA; NotificacionEstatusCurp = $"Baja por duplicidad ({value})."; _AlertaRojaPorEstatusCurp = true; break;
                            case "BCC": Estatus = EstatusCURP.BCC; NotificacionEstatusCurp = $"Baja por cambio de CURP ({value})."; _AlertaRojaPorEstatusCurp = true; break;
                            case "BCN": Estatus = EstatusCURP.BCN; NotificacionEstatusCurp = $"Baja no afectando a CURP ({value})."; _AlertaRojaPorEstatusCurp = true; break;
                        }
                    }
                }
            }



          //      [JsonConverter(typeof(StringEnumConverter)), JsonProperty("estatusCurp")]
         //   public EstatusCURP Estatus { get; set; }

            [JsonProperty("docProbatorio")]
            public int ConDocumentoProbatorio { get; set; }

            [JsonProperty("datosDocProbatorio")]
            public Documento DocumentoProbatorio { get; set; }

            //[JsonProperty("documento")]
            //public string Documento
            //{
            //    get; set;
            //}
        }
    }
}
