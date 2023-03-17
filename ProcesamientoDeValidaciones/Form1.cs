using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProcesamientoDeValidaciones.Models;

namespace ProcesamientoDeValidaciones
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Validaciones _validacionesPendientes = null;
        public System.Windows.Forms.Timer TiempoValidaciones = null;
        private string accessToken = string.Empty;
        private async void Form1_Load(object sender, EventArgs e)
        {

            _validacionesPendientes = new Validaciones();

            _validacionesPendientes.ProcesaMensaje += _validacionesPendientes_ProcesaMensaje;
            TiempoValidaciones = new System.Windows.Forms.Timer();
            TiempoValidaciones.Interval = 1000;
            TiempoValidaciones.Tick += TiempoValidaciones_Tick;

            //      LatinID.Utilidades.WebServices.Caller caller = new LatinID.Utilidades.WebServices.Caller(ConfigurationManager.AppSettings["url"]);// "https://localhost:44389/");

            //     List<Solicitante> datos =   caller.GetCatalog<Solicitante>("Solicitantes", ConfigurationManager.AppSettings["transacciones"]).ToList();






            //using (var client = new HttpClient())
            //{
            //    var url = "https://localhost:44389/Solicitantes";
            //    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            //    var response = await client.GetStringAsync(url);
            //}

        }

        private void TiempoValidaciones_Tick(object sender, EventArgs e)
        {
            this.TiempoValidaciones.Enabled = false;


            //     _validacionesPendientes_ProcesaMensaje($"Hilo: " + (this._validacionesPendientes.getEstatusHiloEnvio().ToString()), false);




            if (this._validacionesPendientes.getEstatusHiloEnvio() != ThreadState.Stopped)
            {
                this.TiempoValidaciones.Enabled = true;
            }
            else
            {
                //   this.gbArchivo.Enabled = true;
                //  this.gborden.Enabled = true;
                //  this.btnStart.Enabled = true;
                //  this.btnStop.Enabled = false;
                //   this.lblTerminandoEnvio.Text = "";
            }
        }

        private void _validacionesPendientes_ProcesaMensaje(string mensaje, bool estado)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new Action<string, bool>(this._validacionesPendientes_ProcesaMensaje), new object[] { mensaje, estado });
            }
            else // if (/*is.chkExtendidos.Checked ||*/ estado)
            {
                textBox1.Text += mensaje + "\r\n";
                //{
                //    if (this.chkLog.Checked)
                //    {
                //        File.AppendAllText(@".\Log" + DateTime.Now.ToString("ddMMyyyy") + ".txt", (DateTime.Now.ToShortTimeString() + ": " + mensaje) + "\n");
                //    }
                //    this.lstEnvio.Items.Insert(0, DateTime.Now.ToLongTimeString() + ": " + mensaje);
                //    if (this.lstEnvio.Items.Count >= 200)
                //    {
                //        this.lstEnvio.Items.Remove(0xc7);
                //    }
            }
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            _validacionesPendientes.IniciaProcesadoValidaciones();
            TiempoValidaciones.Start();
            //for(int i = 0; i< 100; i++)
            //{
            //   // Thread t = new Thread(EjecutarTarea);
            //   // t.Start();


            //    ThreadPool.QueueUserWorkItem(EjecutarTarea, i);
            //}
            //Console.ReadLine();



        }


        static void EjecutarTarea(Object stateInfo)
        {
            Console.WriteLine($"Thread No. {Thread.CurrentThread.ManagedThreadId} ha comenzado la tarea No. {stateInfo}");
            Thread.Sleep(1000);
            Console.WriteLine($"Thread No. {Thread.CurrentThread.ManagedThreadId} ha terminado la terea No. {stateInfo}");

        }

    }
}
