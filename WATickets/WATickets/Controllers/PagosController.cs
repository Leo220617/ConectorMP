using Newtonsoft.Json;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WATickets.Models.Cliente;
using WATickets.Models.Datos;

namespace WATickets.Controllers
{
    [Authorize]

    public class PagosController : ApiController
    {
        Conexion g = new Conexion();
        G G = new G();
        ModelCliente db = new ModelCliente();

        object resp;

        [Route("api/Pagos/Modify")]
        public async Task<HttpResponseMessage> GetModify([FromUri] string DocNum = "")
        {


            try
            {

                Parametros parametros = db.Parametros.FirstOrDefault();
                HttpClient cliente = new HttpClient();
             

                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var SQL = parametros.SQLPagos + "'" + DocNum + "'";  
                var conexion = g.DevuelveCadena();
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Encabezado");

                Pagos pago = new Pagos();
                pago.IdZohoOdV = Ds.Tables["Encabezado"].Rows[0]["IdZoho"].ToString();
                pago.DocNum = Ds.Tables["Encabezado"].Rows[0]["DocNum"].ToString();
                pago.FechaCreacion = Convert.ToDateTime( Ds.Tables["Encabezado"].Rows[0]["Fecha"].ToString());
                pago.Monto = Convert.ToDecimal( Ds.Tables["Encabezado"].Rows[0]["Total"].ToString());
                pago.NoPago = Convert.ToInt32(Ds.Tables["Encabezado"].Rows[0]["NoPago"].ToString());

                HttpClient clienteProd = new HttpClient();

                var httpContent2Prod = new StringContent(JsonConvert.SerializeObject(pago), Encoding.UTF8, "application/json");
                clienteProd.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage response3 = await clienteProd.PutAsync(parametros.UrlPutPagos, httpContent2Prod);
                    if (response3.IsSuccessStatusCode)
                    {
                        response3.Content.Headers.ContentType.MediaType = "application/json";
                        var res = await response3.Content.ReadAsStringAsync();
                        BitacoraZoho bitEncabezado = new BitacoraZoho();
                        bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(pago);
                        bitEncabezado.Fecha = DateTime.Now;
                        bitEncabezado.DocNum = DocNum;
                        bitEncabezado.RespuestaZoho = res.ToString();
                        db.BitacoraZoho.Add(bitEncabezado);
                        db.SaveChanges();

                    }
                }
                catch (Exception ex)
                {

                    BitacoraErrores be = new BitacoraErrores();
                    be.DocNum = DocNum;
                    be.Razon = ex.Message;
                    be.StackTrace = ex.StackTrace;
                    be.Fecha = DateTime.Now;

                    db.BitacoraErrores.Add(be);
                    db.SaveChanges();
                }

                Cn.Close();






                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                BitacoraErrores be = new BitacoraErrores();
                be.DocNum = DocNum;
                be.Razon = ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;

                db.BitacoraErrores.Add(be);
                db.SaveChanges();



                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }


        }
    }
}