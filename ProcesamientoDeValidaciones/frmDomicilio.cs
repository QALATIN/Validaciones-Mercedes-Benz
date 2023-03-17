using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcesamientoDeValidaciones
{
    public partial class frmDomicilio : Form
    {
        public frmDomicilio()
        {
            InitializeComponent();
        }


        private string DomicilioFromTelmex(string ocr, string nombres, string paterno, string materno, out string codigo_postal)
        {
            codigo_postal = string.Empty;
            Regex regex = new Regex("^C\\.P\\. [0-9]+-[a-zA-Z]+", RegexOptions.IgnoreCase);
            string cadenota_domicilio = string.Empty;
            cadenota_domicilio = string.Empty;
            //^C\.P\. [0-9]+-[a-zA-Z]+$
            string[] filas = ocr.Split('\n');
            int get_fila_cp = 0;
            for (int x = 0; x < filas.Length; x++)
            {
                if (regex.IsMatch(filas[x]))
                {
                    get_fila_cp = x;
                    break;
                }

            }
            bool valido_nombre = false;
            for (int y = get_fila_cp - 4; y <= get_fila_cp; y++)
            {
                if (y == get_fila_cp)
                {//en este punto se obtiene el código postal
                    MatchCollection joinMatches = regex.Matches(filas[y]);
                    string tmp = string.Empty;

                    if (joinMatches.Count <= 1)
                    {
                        tmp = joinMatches[0].ToString();
                    }

                    tmp = tmp.Substring(0, tmp.LastIndexOf('-'));
                    cadenota_domicilio += tmp;
                    tmp = tmp.Replace("C", "").Replace("P", "").Replace(".", "").Trim();
                    codigo_postal = tmp;

                }
                else
                {
                    if (!valido_nombre)
                    {
                        if (!filas[y].Contains("RFC")) //por si detecta la fila del RFC de telmex
                        {
                            if (string.IsNullOrEmpty(nombres) && string.IsNullOrEmpty(paterno) && string.IsNullOrEmpty(materno))
                            {
                                cadenota_domicilio += string.Format("{0}, ", filas[y].Trim());
                                valido_nombre = true;
                            }
                            else
                            {//hacemos validaciones del nombre en la fila Y 

                                if (!string.IsNullOrEmpty(nombres) && filas[y].Contains(nombres))
                                {
                                    valido_nombre = true;
                                    continue;
                                }
                                else if (!string.IsNullOrEmpty(paterno) && filas[y].Contains(paterno))
                                {
                                    valido_nombre = true;
                                    continue;
                                }
                                else if (!string.IsNullOrEmpty(materno) && filas[y].Contains(materno))
                                {
                                    valido_nombre = true;
                                    continue;
                                }
                                else
                                {
                                    cadenota_domicilio += string.Format("{0}, ", filas[y].Trim());
                                    valido_nombre = true;
                                }
                            }
                        }
                        else
                        {
                            valido_nombre = true;
                            continue;
                        }
                    }
                    else
                    {
                        cadenota_domicilio += string.Format("{0}, ", filas[y].Trim());

                    }
                }
            }

            return cadenota_domicilio.Trim();
        }


        private void frmDomicilio_Load(object sender, EventArgs e)
        {


        




        }

        string[] doms = new string[] {
        "TELEFONOS DE MEXICO S.A.B. de C.V.\r\nParque Via 198, Col. Cuauhtémoc\r\nC.P. 06500 Ciudad de México\r\nRFC: TME840315-KT6 11-ENE-2021 DV 3\r\n\r\nPág 2 de 5\r\nRESIDENCIAL\r\n—\r\nTotal a Pagar: $ 435.00\r\nPagar antes de: 03-FEB-2Q21\r\nMes de Facturación: Enero\r\nTeléfono:555608 8098\r\nFactura No.:060121010025019\r\nROMO MARTINEZ RICARDO\r\nAVE CANAL NACIONAL MZ K LT 65\r\nDEPTO 101\r\nCTM CULHUACAN SECCION Vil\r\nMEXICO, CIUDAD DE MEXICO\r\nC.P. 04489-CR-04831\r\nImporte enviado a cobro en su\r\nBanco Banamex\r\nTMX04489076033305401210601\r\nCL\r\nRFC Público en General: XAXX010101000\r\nr\r\nPagar tu Recibo nunca fue tan fácil\r\nCon Canales Digitales, paga sin salir de casa.\r\nllEill tetmex.com\r\nAPP TELMEX\r\nClaró-\r\nPSY\r\nCLARO PAY\r\nTambién puedes domiciliarlo con cargo a tu tarjeta de crédito o débito o si lo prefieres en:\r\nTelcel, Telecomm, Coppel, Inbursa, Bajío, Banjercito, Multiva, Cí Banco, Mifel, Caja Popular Mexicana y otros.\r\nVer términos, condiciones e información transparente en telmex.com/terminoshogar\r\nResumen del Estado de Cuenta___________________________\r\nSaldo Anterior\t435.00\r\nCargos del Mes\t+ 434.99\r\nSu Pago Gracias\t22-DÍC-20\t- 435.00\r\nCargo por Redondeo\t+ 0.52\r\nCrédito por Redondeo* *\t- 0.51\r\nSaldo al Corte\t$ 435.00\r\n(cuatrocientos treinta y cinco pesos 00/100 M.N.)\r\n*La diferencia de Centavos aplicará en su próximo Estado de Cuenta.\r\nCargos del Mes\r\nServicios de Telecomunicaciones__________367.29\r\nIEPS 3%____________________________________7.71\r\nIVA 16%\t59.99\r\nTotal\t$ 434.99\r\nAtención a Clientes: 800 123 2222\r\nPaga tu recibo fácil y rápido en\r\ntelmex.com/mitelmex\r\nr\r\ninfinitum. o Disney+\r\n■¿^Exceso efe Velocidad\r\n© 2020 Disney y sus entidades relacionadas. Todos tas derechos resewados.\r\nServicio por suscripción de pago. Contentóos sujetos a disponibilidad. Para más información consulte en DisneyPlus.com W\r\n^Detalles de la promoción, beneficios, términos y condiciones en telmex.com/terminoshogar_\r\nf I d1 nf T.l .itt ni\r\ntelmex.com\r\n800123 2222\r\nen Paquete.\r\nCentros de\r\nAtención Telmex\r\nTeléfono: 555608 8098\r\nMes de Facturación: Enero\r\nPagar antes de:03-FEB-2021\r\nDV3\r\nTotal a Pagar por Servicios de\r\nTelecomunicaciones de\r\nTelmex y otros Servicios\r\n$ 435.00\r\n55560880980000435009\r\n55560880980000435009\r\nI\r\nI",
        "_____________\t___________________________________\r\nPíg 2 da 4\r\nHISIDINCIAI\r\nTELMEX\r\nTELEFONOS DE MEXICO S.A.B. do C.V.\r\nPüiqii«» VI* 198, Col. Cuauhléraoo\r\nC P 06500 Ciudad da México\r\nRFC: TME840315-KT6 01-OCT-2022 DV 2\r\nTotal a Pagar: $ 435.00\r\nPagar antes de: 23-OCT-2022\r\nMes do Inclinación: Octubre\r\nTolófono\r\n552162 3292\r\nI achira No\r\n010122100255038\r\nARROYO COELLO BRENDA ARACELI\r\nPRV GIBELLINA\r\nDEP12\r\nCONJUNTO URBANO REAL VERONA\r\nTECAMAC,EM\r\nC.P. 55767-CR -55762\r\nSu estado de cuenta puede ser\r\npagado en cualquier centro de cobro\r\nindicado al reverso de este recibo.\r\nTMX55767089047398010220101\r\nODA\r\nRFC Público en General: XAXX010101000\r\n_________________________________________________________________________________________________________\r\n\r\ninfinitum\r\nllSrcw de Velocidad\r\nStt TODO INCLUIDO\r\nI POR 3 MESES\r\nCentros de Atención Telmex, telmex.com o al 800 123 2222.\r\nPuma Video «. un sor vicio proporcionado por un loicoro, por promoción la i»iuu rnoiwunl ($99) so cobrará n partí dul cuarto mu» Vólido ul 31 do octubre do 2022 Diawy <n un aannco ptopuaonado por un tercero,\r\n[xs promoción la ninla nxinsual do Oiwoyi ($ 159) so cobmió n partir dul cuarto dios. Válido ai 30 da iwviombw río 2022 Otros Inmota», terminen y condcionos en telmo» ccmAornnnoíihcgar\r\nResumen del Estado de Cuenta_______________________\r\nSaldo Anterior\t435.00\r\n_______________\r\nCargos del Mes\t+ 434.99\r\nSu Pago Gracias\t20-Sep-22\t- 435.00\r\nCargo por Redondeo\t_____________________*\r\nCrédito por Redondeo*________________________\r\nSaldo al Corte\t$ 435.00\r\n(cuatrocientos treinta y cinco pesos OO/1OO M.N.)\r\n‘La diferencia de Centavos aplicará en su próximo Estado de Cuenta.\r\nCargos del Mes\r\nServicios de Telecomunicaciones________________________367,29\r\nIEPS 3%\t7.71_____\r\nIVA 16%____________________________________________\t59.99\r\nTotal\t$ 43499\r\n____________________\r\nni»F Atención a Clientes: 800123 2222\r\nin«F Paga tu recibo fácil y rápido en\r\ntelmex.com/mitelmex\r\n_______________________________________________________________________________________________________\r\n/\r\n¡Claro que tienes\r\nentretenimiento!\r\n\\__________\r\nActiva Claro-video sin costo\r\ny disfruta miles de títulos en HD,\r\nahora con tyaramounl+ incluido.\r\nHazlo en\r\nclarovideo.com\r\nClaro video y Riramount+ son servicios proporcionados\r\npor un tercero. Ver términos, condfcknes e Información\r\ntransparente en tehKKcorntarrrinoshogar\r\ny\r\nTeléfono: 552162 3292\r\nMes de Facturación: Octubre\r\nDV2\r\nTotal a Pagar por Servicios de\r\nTelecomunicaciones de\r\nelmexy otros Servicios\r\n$ 435.00\r\n-----------------------------\r\nPagar antes de:23-OCT-2022",
        "Pag 1 de 3\r\nNEGOCIOS J\r\n\r\nTELEFONOS DE MEXICO S.A.B. de C.V.\r\nParque Via 198, Col Cuauhtémoc\r\nCP 06500 Ciudad de México\r\nRFC TME840315-KT6 01-MAY-2022 DV 1\r\nHECTOR RAUL TELLEZ CEPEDA\r\nCLL TLAXCALA 177-2\r\nEDI SI 201\r\nHIPODROMO\r\nCIUDAD DE MEXICO , CIUDAD DE MEXICO\r\nC.P. 06100-CR-06401\r\nRFC Publico en General: XAXX010101000\r\nTotal a Pagar: $ 399.00 ___\r\nPagar antes de: 23-MAY-2022\r\nMes de Facturación: Mayo\r\nTeléfono:552234 3197\r\nFactura No.:\t010122050047948\r\nSu estado de cuenta puede ser\r\npagado en cualquier centro de cobro\r\nindicado al reverso de este recibo.\r\n____________\r\nuky'-'\r\niílt\"\r\nComunícate con tus clientes,\r\nmejora tu administración y mucho más\r\nSolo entra a telmex.com/activacion\r\n____________________________________________________\r\n______\t________\r\nResumen del Estado de Cuenta_______________________\r\nSaldo Anterior\t399.00\r\nCargos del Mes\t* 398.99\r\nSu Pago Gracias\t8-Abt-22\t- 399.00\r\nCargo por Redondeo\t♦ 0.24\r\nCrédito por Redondeo*\t- 0.23\r\nSaldo al Corte\t$ 399.00\r\n(trescientos noventa y nueve pesos 00/100 U N)\r\n1* d*e<enc« de Centavos aplicara en su prdximo Estado de Cuenta\r\n________\r\nActiva hoy las soluciones que tienes\r\nsin costo adicional en tu Paquete\r\n\r\n_________\r\nCargos del Mes\r\nServicios de Telecomunicaciones\t336.90\r\nIEPS 3%\t_______________________________7.06\r\nIVA 16%_ '_____________________________55.03\r\nTotal\t$398.99\r\nAtención a Clientes: 800 123 0321 ó desde su\r\nLinea Telmex *321.\r\n\r\nExtiende tu oficina con Microsoft 365\r\nReúne a todo tu equipo de trabajo a través de videollamadas para que puedan\r\ncolaborar en un solo lugar y obten Word, Excel y Power Point en línea.\r\n______\tContrátalo hoy en tetmex.com/negocio\t____\r\nTeléfono 552234 3197\r\nMes de Facturación: Mayo\r\nPagar antes de 23-MAY-2022\r\nTotal a Pagar por Servicios de\r\nTelecomunicaciones de\r\nTelmex y otros Servicios\r\n$ 399.00\r\n55223431970000399009\r\n55223431970000399009\r\n________________________________________________________________________________________________",
        "\r\nPág 1 de 4\r\nTeléfono: 555666 6715\r\nJulio de 2022.\r\nTELEFONOS DE MEXICO S.A.B. de C.V.\r\nParque Via 198, Col. Cuauhtémoc\r\nC.P. 06500 Ciudad de México\r\nRFC: TME840315-KT6 21-JUL-2022 DV 3\r\nPECHIR ESPARZA IVONNE GUADALUPE\r\nAND EPIGMENIO IBARRA 3\r\nRET 2 ZARAGOZA/AND BAROUS\r\nROMERO DE TERREROS\r\nMEXICO, CIUDAD DE MEXICO\r\nC.P. 04310-CR-04831\r\nUN\r\nRFC: PEEI681105PZ3\r\nTMX04310490173237607221501\r\nTe damos más\r\nentretenimiento\r\nSolo por ser cliente infinitum tienes\r\n\r\nI\r\n\r\nIncluye envíos GRATIS con Amazon Prime\r\nprime\r\no-qJ\r\nActívalo hoy y disfruta:\r\n• Series y películas en 4K\r\n• Acceso en 3 pantallas simultáneas\r\n• Canciones y podcasts en Amazon Music Prime\r\n•Fox Sports incluido sin costo adicional con Prime Video\r\nSolicítalo en Centros de Atención Telmex | telmex.com | 800 123 2222\r\nPrime Video es un servicio proporcionado por un tercero, por promoción la renta mensual ($99) se cobrará a partir del cuarto mes.\r\nVálido al 3 de septiembre 2022. Oferta sujeta a cambios. Cancela en cualquier momento ai 800 123 2222. Amazon, Amazon Prime,\r\nPrime Video y demás logotipos relacionados son marcas registradas de Amazon.com, Inc. Otros beneficios, plazos, detalles de la\r\npromoción, términos, condiciones, folios ele registro IFT y velocidad promedio de descarga en telmex.com/terminoshogar",
        "Pág 2 de 4\r\nRESIDENCIAL\r\nTELEFONOS DE MEXICO S.A.B. de C.V.\r\nParque Via 198, Col. Cuauhtémoc\r\nC.P. 06500 Ciudad de México\r\nRFC: TME840315-KT6 01-JUL-2022 DV 5\r\nGUEVARA CRUZ DANIEL ANTONIO\r\nCLL OLIVOS 5\r\nPRIV DE LOS CIP Y AV DE LOS AL\r\nJARDINES DE SAN MATEO\r\nNAUCALPAN DE JUAREZ , EM\r\nC.P. 53240-CR-53001\r\n\r\nTotal a Pagar: $ 499.00\r\nPagar antes de: 23-JUL-2Q22\r\nMes de Facturación: Julio\r\nTeléfono:555084 1578\r\nFactura No.:010122070179768\r\nSu estado c/e cuenta puede ser\r\npagado en cualquier centro de cobro\r\nindicado al reverso de este recibo.\r\nTMX53240530317020707220101\r\nST\r\nRFC: GUCD790619SK7\r\nA\r\nSolo por ser cliente\r\ninfinitum\r\ntienes\r\nprime video\r\nIncluye envíos GRATIS con Amazon Prime\r\n\r\nSolicítalo en Centros de Atención Telmex teimex.com | 800 123 2222\r\nPrimer Vídeo as un servicio procerotoñado por un terceto, por promoofin ia rería mensual ($99} se ootiará a partir riel •cuarta mes. Váfeb si 3 de septiembre 2022. Otarte sujete a cambios. Cancela en cosiquier momento al 800 123 2222. Amaion, Amazon\r\nPrime, Prime 'Afeo y demás logotipos retedasdos son maros» registradas da .Amazon.com, Ino. Otros tenafcios, plazas, dataltea da ia promoción, términos, condicionas, folios efe registro IFT y velocidad prometí» de descarga en tetmex.com/t0nnnosl-K-igar\r\nResumen del Estado de Cuenta__________________________\r\nSaldo Anterior\t435.00\r\nCargos del Mes\t+ 499.00\r\nSu Pago Gracias\t24-Jun-22\t- 435.00\r\nCargo por Redondeo\t+ 0.99\r\nCrédito por Redondeo* *\t- 0.99\r\nSaldo al Corte\t$ 499.00\r\n(cuatrocientos noventa y nueve pesos 00/100 M.N.)\r\n*La diferencia de Centavos aplicará en su próximo Estado de Cuenta.\r\nCargos del Mes\r\nServicios de Telecomunicaciones___________421.32\r\nEPS 3%______________________________________8.85\r\nIVA 16%\t68.83\r\nTotal\t$ 499.00\r\nAtención a Clientes: 800 123 2222\r\nPaga tu recibo fácil y rápido en\r\ntelmex.com/mitelmex\r\n/---------------------\r\nInfinitum te conecta con tu\r\nCasa Inteligente\r\n\\______________\r\n\r\n\r\nFacilita tu vida\r\ncon un dispositivo de\r\ninternet de las Cosas.\r\nAdquiérelos en telmex.com\r\ncon cargo a tu Recibo Telmex\r\nConsulta modelos participantes y requisitos\r\nefe contratación en ticnda.leimex.com\r\n_________________________________J\r\nTeléfono: 555084 1578\r\nMes de Facturación: Julio\r\nPagar antes de:23-JUL-2022\r\nDV5\r\nTotal a Pagar por Servicios de\r\nTelecomunicaciones de\r\nTelmex y otros Servicios\r\n$ 499.00\r\n55508415780000499002\r\n55508415780000499002\r\nI\r\nI",
            "\r\nPág 2 de 4\r\nRESIDENCIAL\r\nTotal a Pagar: $ 389.00\r\nTELEFONOS DE MEXICO S.A.B. de C.V.\r\nParque Via 198, Col. Cuauhtémoc\r\nC.P. 06500 Ciudad de México\r\nRFC: TME840315-KT6 01-JUL-2022 DV 4\r\nARVIZO AGUILA MARIA GEMA\r\nPOZA RICA 1665\r\n18 DE MARZO\r\nGUADALAJARA JAL , JA\r\nC.P. 44960-CR -44943\r\nPagar antes de: 25-JUL-2Q22\r\nMes de Facturación: Julio\r\nTeléfono:333645 0775\r\nFactura No.:020322070037353\r\nSu estado c/e cuenta puede ser\r\npagado en cualquier centro de cobro\r\nindicado al reverso de este recibo.\r\nTMX44960575130639107220203\r\nRFC Público en General: XAXX010101000\r\nLAT\r\nA\r\nSolo por ser cliente\r\ninfinitum\r\ntienes\r\nprime video\r\nIncluye envíos GRATIS con Amazon Prime\r\n\r\nSolicítalo en Centros de Atención Telmex teimex.com | 800 123 2222\r\nPrimer Vídeo as un servicio propactonacb por un iansiv, por promoofin ia rería mensual ($99} se ootiará a partir riel •cuarto mes. Váfeb si 3 de septiembre 2022. Oferta v.i-efe a cambios. Cancela en cuskfeier momento al 800 123 2222. Amanen, Amazon\r\nPrime, Prima 'Afeo y demás logotipos retedasdos son maros» registradas da Amszon.com, Ino. Otros tenafcios, plazas, dataltea da ia promoción, términos, condicionas, folios efe registro IFT y velocidad prometí» de descarga en tstoieferronvtemmoshogar\r\nResumen del Estado de Cuenta_______________________\r\nSaldo Anterior\t389.00\r\nCargos del Mes\t+ 388.99\r\nSu Pago Gracias\t25-Jun-22\t- 389.00\r\nCargo por Redondeo\t+ 0.78\r\nCrédito por Redondeo*\t- 0.77\r\nSaldo al Corte\t$ 389.00\r\n(trescientos ochenta y nueve pesos 00/100 M.N.)\r\n*La diferencia de Centavos aplicará en su próximo Estado de Cuenta.\r\nCargos del Mes\r\nServicios de Telecomunicaciones___________328.44\r\nIEPS 3%_____________________________________6.90\r\nIVA 16%\t53.65\r\nTotal\t$ 388.99\r\nAtención a Clientes: 800 123 2222\r\nPaga tu recibo fácil y rápido en\r\ntelmex.com/mitelmex\r\n/---------------------\r\nInfinitum te conecta con tu\r\nCasa Inteligente\r\n\\______________\r\n\r\nFacilita tu vida\r\ncon un dispositiva de\r\nInternet de las Cosas.\r\nAdquiérelos en telmex.com\r\ncon cargo a tu Recibo Telmex\r\nConsulta modelos participantes y requisitos\r\nefe contratación en tfenda.lelmex.com\r\n_________________________________J\r\nTeléfono: 333645 0775\r\nMes de Facturación: Julio\r\nPagar antes de:25-JUL-2022\r\nDV4\r\nTotal a Pagar por Servicios de\r\nTelecomunicaciones de\r\nTelmex y otros Servicios\r\n33364507750000389002\r\n33364507750000389002\r\n$ 389.00 ■\r\n□",
        "Pág 1 de 3\r\nRESIDENCIAL\r\nTELEFONOS DE MEXICO S.A.B. de C.V.\r\nParque Via 198, Col. Cuauhtémoc\r\nC.P. 06500 Ciudad de México\r\nRFC: TME840315-KT6 01-AGO-2022 DV 8\r\nTotal a Pagar: $ 564.00\r\nPagar antes de: INMEDIATO\r\nMes de Facturación: Agosto\r\nTeléfono:\t552594 5642\r\n\r\nFactura No.:050122080060404\r\nSUAREZ MEDINA JORGE\r\nPLAZUELA EMILIANO ZAPATA 19\r\nPBLO SAN NICOLAS TETELCO\r\nTLAHUAC, CIUDAD DE MEXICO\r\nC.P. 13700-CR-13611\r\nCuenta Vencida\r\nTMX13700911203904808220501\r\nRFC Público en General: XAXX010101000\r\nTLH\r\nSu estado de cuenta presenta 2 meses\r\nvencidos. Puede ser pagado en cualquier\r\ncentro de cobro indicado al reverso de este\r\nrecibo. Su servicio esta suspendido.\r\nConserve su linea. Acuda a cualquier tienda\r\nTelmex a realizar su pago. Una vez liquidado\r\nsu adeudo, haremos un cargo por\r\nreanudación del servicio.\r\ni\r\nB\r\nSigue disfrutando\r\ntodos los Beneficios\r\nde tus Servicios TELMEX\r\n\r\n\r\nResumen del Estado de Cuenta______________________\r\nSaldo Anterior\t374.00\r\nCargos del Mes\t+ 189.51\r\nSu Pago\t-\t0.00\r\nCargo por Redondeo\t+\t1.49\r\nCrédito por Redondeo*\t-1.00\r\nSaldo al Corte\t$ 564.00\r\n(quinientos sesenta y cuatro pesos 00/100 M.N.)\r\n*La diferencia de Centavos aplicará en su próximo Estado de Cuenta.\r\nCargos del Mes\r\nServicios de Telecomunicaciones__________158.61\r\nIEPS 3%_____________________________________476\r\nIVA 16%\t26.14\r\nTotal\t$189.51\r\nAtención a Clientes: 800 123 2222\r\nPaga tu recibo fácil y rápido en\r\ntelmex.com/mitelmex\r\nHaz tu pago a tiempo solo entra a telmex.com\r\no consulta los establecimientos de pago en el reverso de tu Recibo.\r\n_______________________________________________________________________/_______________________________________________________________________\r\nTeléfono: 552594 5642\r\nMes de Facturación: Agosto\r\nPagar antes de:INMEDIATO\r\nDV8\r\nTotal a Pagar por Servicios de\r\nTelecomunicaciones de\r\nTelmex y otros Servicios\r\n$ 564.00\r\n55259456420000564003\r\n55259456420000564003\r\nI\r\nI",
        "Pág 2 de 4\r\nRESIDENCIAL\r\nTELEFONOS DE MEXICO S.A.B. de C.V.\r\nParque Via 198, Col. Cuauhtémoc\r\nC.P. 06500 Ciudad de México\r\nRFC: TME840315-KT6 21-JUL-2022 DV 9\r\nTotal a Pagar: $ 599.00\r\nPagar antes de: 18-AGQ-2022\r\nMes de Facturación: Julio\r\nTeléfono:824242 3499\r\nFactura No.:130222070025884\r\n\r\nMU OZ CANSINO SANTOS OCTAVIO\r\nCRT NACIONAL 135 SUR\r\nGPE PEREZ Y MATEO ALCORTA\r\nCOL HDA LARRALDENA\r\nSABINAS HIDALGO , NL\r\nC.P. 65276-CR-65251\r\nEste recibo procesa el pago por\r\ndomiciliación Telmex\r\nTMX65276029565422007221302\r\nSHI\r\nRFC Público en General: XAXX010101000\r\nA\r\nSolo por ser cliente\r\ninfinitum\r\ntienes\r\nprime video\r\nIncluye envíos GRATIS con Amazon Prime\r\n\r\nSolicítalo en Centros de Atención Telmex teimex.com | 800 123 2222\r\nPrimer Vídeo as un servicio propactonacb por un iansiv, por promoofin ia rería mensual ($99} se ootiará a partir riel •cuarto mes. Váfeb si 3 de septiembre 2022. Oferta v.i-efe a cambios. Cancela en cuskfeier momento al 800 123 2222. Amanan, Amazon\r\nPrime, Prima 'Afeo y demás logotipos retedasdos son maros» registradas da Amszon.com, Ino. Otros tenafcios, plazas, dataltea da ia promoción, términos, condicionas, folios efe registro IFT y velocidad prometí» de descarga en tstoieferronvtemmoshogar\r\nResumen del Estado de Cuenta_______________________\r\nSaldo Anterior\t599.00\r\nCargos del Mes\t+ 598.99\r\nSu Pago Gracias\t4-Jul-22\t- 599.00\r\nCargo por Redondeo\t+ 0.95\r\nCrédito por Redondeo*\t- 0.94\r\nSaldo al Corte\t$ 599.00\r\n(quinientos noventa y nueve pesos 00/100 M.N.)\r\n*La diferencia de Centavos aplicará en su próximo Estado de Cuenta.\r\nCargos del Mes\r\nServicios de Telecomunicaciones____________505.76\r\nIEPS 3%_____________________________________10.61\r\nIVA 16%\t82.62\r\nTotal\t$ 598.99\r\nAtención a Clientes: 800 123 2222\r\nPaga tu recibo fácil y rápido en\r\ntelmex.com/mitelmex\r\n/---------------------\r\nInfinitum te conecta con tu\r\nCasa Inteligente\r\n\\______________\r\n\r\nFacilita tu vida\r\ncon un dispositiva de\r\nInternet de las Cosas.\r\nAdquiérelos en telmex.com\r\ncon cargo a tu Recibo Telmex\r\nConsulta modelos participantes y requisitos\r\nefe contratación en tfenda.lelmex.com\r\n_________________________________J\r\nTeléfono: 824242 3499\r\nMes de Facturación: Julio\r\nPagar antes de:18-AGO-2022\r\nDV9\r\nTotal a Pagar por Servicios de\r\nTelecomunicaciones de\r\nTelmex y otros Servicios\r\n82424234990000599004\r\n82424234990000599004\r\n$ 599.00 ■\r\n□",
        "Pag 1 de 3\r\nNEGOCIOS J\r\n\r\nTELEFONOS DE MEXICO S.A.B. de C.V.\r\nParque Via 198, Col Cuauhtémoc\r\nCP 06500 Ciudad de México\r\nRFC TME840315-KT6 01-MAY-2022 DV 1\r\nHECTOR RAUL TELLEZ CEPEDA\r\nCLL TLAXCALA 177-2\r\nEDI SI 201\r\nHIPODROMO\r\nCIUDAD DE MEXICO , CIUDAD DE MEXICO\r\nC.P. 06100-CR-06401\r\nRFC Publico en General: XAXX010101000\r\nTotal a Pagar: $ 399.00 ___\r\nPagar antes de: 23-MAY-2022\r\nMes de Facturación: Mayo\r\nTeléfono:552234 3197\r\nFactura No.:\t010122050047948\r\nSu estado de cuenta puede ser\r\npagado en cualquier centro de cobro\r\nindicado al reverso de este recibo.\r\n____________\r\nuky'-'\r\niílt\"\r\nComunícate con tus clientes,\r\nmejora tu administración y mucho más\r\nSolo entra a telmex.com/activacion\r\n____________________________________________________\r\n______\t________\r\nResumen del Estado de Cuenta_______________________\r\nSaldo Anterior\t399.00\r\nCargos del Mes\t* 398.99\r\nSu Pago Gracias\t8-Abt-22\t- 399.00\r\nCargo por Redondeo\t♦ 0.24\r\nCrédito por Redondeo*\t- 0.23\r\nSaldo al Corte\t$ 399.00\r\n(trescientos noventa y nueve pesos 00/100 U N)\r\n1* d*e<enc« de Centavos aplicara en su prdximo Estado de Cuenta\r\n________\r\nActiva hoy las soluciones que tienes\r\nsin costo adicional en tu Paquete\r\n\r\n_________\r\nCargos del Mes\r\nServicios de Telecomunicaciones\t336.90\r\nIEPS 3%\t_______________________________7.06\r\nIVA 16%_ '_____________________________55.03\r\nTotal\t$398.99\r\nAtención a Clientes: 800 123 0321 ó desde su\r\nLinea Telmex *321.\r\n\r\nExtiende tu oficina con Microsoft 365\r\nReúne a todo tu equipo de trabajo a través de videollamadas para que puedan\r\ncolaborar en un solo lugar y obten Word, Excel y Power Point en línea.\r\n______\tContrátalo hoy en tetmex.com/negocio\t____\r\nTeléfono 552234 3197\r\nMes de Facturación: Mayo\r\nPagar antes de 23-MAY-2022\r\nTotal a Pagar por Servicios de\r\nTelecomunicaciones de\r\nTelmex y otros Servicios\r\n$ 399.00\r\n55223431970000399009\r\n55223431970000399009\r\n________________________________________________________________________________________________",
        "\r\nPág 2 de 4\r\nRESIDENCIAL\r\nTotal a Pagar: $ 0.00\r\nTELEFONOS DE MEXICO S.A.B. de C.V.\r\nParque Via 198, Col. Cuauhtémoc\r\nC.P. 06500 Ciudad de México\r\nRFC: TME840315-KT6 21-JUL-2022 DV 3\r\nPagar antes de: 23-AGQ-2022\r\nMes de Facturación: Julio\r\nTeléfono:555666 6715\r\nFactura No.:150122070022016\r\nPECHIR ESPARZA IVONNE GUADALUPE\r\nAND EPIGMENIO IBARRA 3\r\nRET 2 ZARAGOZA/AND BAROUS\r\nROMERO DE TERREROS\r\nMEXICO, CIUDAD DE MEXICO\r\nC.P. 04310-CR-04831\r\nNo pagar.\r\nTMX04310490173237607221501\r\nUN\r\nRFC: PEEI681105PZ3\r\nA\r\nSolo por ser cliente\r\ninfinitum\r\ntienes\r\nprime video\r\nIncluye envíos GRATIS con Amazon Prime\r\n\r\nSolicítalo en Centros de Atención Telmex teimex.com | 800 123 2222\r\nPrimer Vídeo as un servicio propactonacb por un iansiv, por promoofin ia rería maisual ($99} se ootiará a partir del •cuarto mes. Váfeb si 3 de septiembre 2022. Oferta sujeta a cambios. Cancela en eosiquier momento al 800 123 2222. Amalan, Amazon\r\nPrime, Prime Video y demás logotipo» i&cMs son marca» registradas da Amszon.com, Ino. Otros tenafcios, plaza». dótate» da ia promoción, términos, condieiortas. folios efe registro IFT y velocidad prometí» de descarga en tstmeK.com/termiriosriogar\r\nResumen del Estado de Cuenta______________________\r\nSaldo Anterior\t389.00\r\nCargos del Mes\t+ 388.99\r\nSu Pago Gracias\t23-Jul-22\t- 800.00\r\nCargo por Redondeo\t+ 1.64\r\nCrédito por Redondeo*\t- 0.90\r\nSaldo al Corte\t$-21.27\r\n*La diferencia de Centavos aplicará en su próximo Estado de Cuenta.\r\nCargos del Mes\r\nServicios de Telecomunicaciones___________328.44\r\nIEPS 3%_____________________________________6.90\r\nIVA 16%\t53.65\r\nTotal\t$ 388.99\r\nAtención a Clientes: 800 123 2222\r\nPaga tu recibo fácil y rápido en\r\ntelmex.com/mitelmex\r\n/---------------------\r\nInfinitum te conecta con tu\r\nCasa Inteligente\r\n\\______________\r\n\r\nFacilita tu vida\r\ncon un dispositivo de\r\ninterne! de las Cosas.\r\nAdquiérelos en telmex.com\r\ncon cargo a tu Recibo Telmex\r\nConsulta modelos participantes y requisitos\r\nefe contratación en tienda. 1eimex.com\r\n__________________________________J\r\nTeléfono: 555666 6715\r\nMes de Facturación: Julio\r\nPagar antes de:23-AGO-2022\r\nDV3\r\nTotal a Pagar por Servicios de\r\nTelecomunicaciones de\r\nTelmex y otros Servicios\r\n$0.00",
  "\r\nPág 1 de 3\r\nRESIDENCIAL\r\nTotal a Pagar: $ 556.00\r\nTELEFONOS DE MEXICO S.A.B. de C.V.\r\nParque Via 198, Col. Cuauhtémoc\r\nC.P. 06500 Ciudad de México\r\nRFC: TME840315-KT6 21-JUL-2022 DV 4\r\nPagar antes de: 19-AGQ-2022\r\nMes de Facturación: Julio\r\nTeléfono:555611 1289\r\nFactura No.:140122070004673\r\nJIMENEZ HINOJOSA MARIA LIDIA\r\nCDA DE LA PRESA 16\r\nDEP 2 INTERIOR\r\nBARRIO NORTE\r\nMEXICO, CIUDAD DE MEXICO\r\nC.P. 01410-CR -01401\r\nSu estado c/e cuenta puede ser\r\npagado en cualquier centro de cobro\r\nindicado al reverso de este recibo.\r\nTMX01410723373058807221401\r\nMI\r\nRFC: JIHL670803I48\r\n\r\n\r\nHazlo todo desde tu celular.\r\nAprovecha tu APP TELMEX\r\n«Consulta tu saldo, visualiza\r\no envía tu Recibo por mail\r\n® Recibe asistencia\r\nen línea\r\n® Aumenta tu velocidad\r\ny entretenimiento\r\nr.\r\nk-i íiin’.'j, >;í'<ír'jh'. n>,-< .-«.r- f¡}Ri.-ínn\ttt <-xx'.\tb'kir\r\n\r\nResumen del Estado de Cuenta_______________________\r\nSaldo Anterior\t556.00\r\nCargos del Mes\t+ 555.98\r\nSu Pago Gracias\t15-Jul-22\t-556.00\r\nCargo por Redondeo\t+ 1.07\r\nCrédito por Redondeo*\t-1.05\r\nSaldo al Corte\t$ 556.00\r\n(quinientos cincuenta y seis pesos 00/100 M.N.)\r\n*La diferencia de Centavos aplicará en su próximo Estado de Cuenta.\r\nCargos del Mes\r\nServicios de Telecomunicaciones____________328.44\r\nTiendas Telmex______________________________34.48\r\nServicios Especiales_______________________109.48\r\nIEPS 3%______________________________________6.89\r\nIVA 16%\t76.69\r\nTotal\t$ 555.98\r\nAtención a Clientes: 800 123 2222\r\nPaga tu recibo fácil y rápido en\r\ntelmex.com/mitelmex\r\n/---------------------\r\nInfinítum te conecta con tu\r\nCasa Inteligente\r\n\\______________\r\nFacilita tu vida\r\ncari un dispositivo (te\r\nInternet de las Cosas,\r\nAdquiérelos en telmex.com\r\ncon cargo a tu Recibo Telmex\r\nConsulta modelos participantes y requisitos\r\nefe contratación en ticnda.teimex.com\r\n__________________________________y\r\n\r\nTeléfono: 555611 1289\r\nMes de Facturación: Julio\r\nPagar antes de:19-AGO-2022\r\nDV4\r\nTotal a Pagar por Servicios de\r\nTelecomunicaciones de\r\nTelmex y otros Servicios\r\n55561112890000556004\r\n55561112890000556004\r\n$ 556.00 ■\r\n□\r\nTotal por Servicios de\r\nTelecomunicaciones\r\nde Telmex\r\n$ 389.00\r\n55561112890000389002"
        };

        private void button1_Click(object sender, EventArgs e)
        {
            string cadenota_domicilio = string.Empty;

            int x = 0;
            string nombre = string.Empty;
            string paterno = string.Empty;
            string materno = string.Empty;
            string codigo_postal = string.Empty;
            foreach (string ocr in doms)
            {
                cadenota_domicilio = string.Empty;


                if(x == 5)
                {
                    nombre = string.Empty;// "MARIA GEMA";
                    paterno = "ARVIZO";
                }
                if(x == 6)
                {
                    nombre = "JOPRGE";
                    paterno = "SUAREZ";
                    materno = "MEDINA";
                }

                cadenota_domicilio = DomicilioFromTelmex(ocr, nombre, paterno, materno, out codigo_postal);

                nombre = string.Empty;
                paterno = string.Empty;
                materno = string.Empty;

                Console.WriteLine(cadenota_domicilio + "\r\nCódigo Postal: " + codigo_postal);
                x++;
            }



        }
    }
}
