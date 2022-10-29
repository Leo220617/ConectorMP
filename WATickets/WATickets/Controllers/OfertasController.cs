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

    public class OfertasController : ApiController
    {
        Conexion g = new Conexion();
        G G = new G();
        ModelCliente db = new ModelCliente();

        object resp;

        [Route("api/OfertaCompra/Modify")]
        public async Task<HttpResponseMessage> GetModify([FromUri] string DocNum = "")
        {


            try
            {

                Parametros parametros = db.Parametros.FirstOrDefault();
                HttpClient cliente = new HttpClient();


                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var SQL = parametros.SQLOfertasCompra + "'" + DocNum + "'";
                var conexion = g.DevuelveCadena();
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Encabezado");
                var Oportunidad = "";
                var Concantenacion = "";

                OfertaCompra oferta = new OfertaCompra(); 

                
                    Concantenacion = Ds.Tables["Encabezado"].Rows[0]["Concatenacion"].ToString();



                    try
                    {
                        HttpResponseMessage response2 = await cliente.GetAsync(parametros.UrlZoho + "/" + Concantenacion);
                        if (response2.IsSuccessStatusCode)
                        {
                            response2.Content.Headers.ContentType.MediaType = "application/json";
                            var resp2 = await response2.Content.ReadAsAsync<ZohoApi>();

                            var ProyectID = resp2.data.Where(a => a.Deal_Name.ToLower().Contains(Concantenacion.ToLower()) && !a.Stage.ToLower().Contains("Aprobada".ToLower()) && !a.Stage.ToLower().Contains("Cerrado".ToLower())).FirstOrDefault();
                            if (ProyectID != null)
                            {
                                Oportunidad = ProyectID.id.ToString();
                            }
                            else
                            {
                                throw new Exception("No se encontro el proyecto");

                            }
                        }
                        else
                        {
                            throw new Exception(response2.ReasonPhrase);
                        }
                    }
                    catch (Exception ex)
                    {

                        throw new Exception(ex.Message);
                    }

                    oferta.DocEntry = Ds.Tables["Encabezado"].Rows[0]["DocEntryOFC"].ToString();
                    oferta.Fecha = Convert.ToDateTime(Ds.Tables["Encabezado"].Rows[0]["Fecha"].ToString());
                    oferta.CodProveedor = Ds.Tables["Encabezado"].Rows[0]["CodProveedor"].ToString().ToString();
                    oferta.NomProveedor = Ds.Tables["Encabezado"].Rows[0]["Proveedor"].ToString().ToString();
                    oferta.IdZoho = Oportunidad;
                
 
                HttpClient clienteProd = new HttpClient();

                var httpContent2Prod = new StringContent(JsonConvert.SerializeObject(oferta), Encoding.UTF8, "application/json");
                clienteProd.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage response3 = await clienteProd.PutAsync(parametros.UrlPutOfertasCompra, httpContent2Prod);
                    if (response3.IsSuccessStatusCode)
                    {
                        response3.Content.Headers.ContentType.MediaType = "application/json";
                        var res = await response3.Content.ReadAsStringAsync();
                        BitacoraZoho bitEncabezado = new BitacoraZoho();
                        bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(oferta);
                        bitEncabezado.Fecha = DateTime.Now;
                        bitEncabezado.DocNum = DocNum;
                        bitEncabezado.RespuestaZoho = res.ToString();
                        db.BitacoraZoho.Add(bitEncabezado);
                        db.SaveChanges();

                        try
                        {
                            var respZoho = await response3.Content.ReadAsAsync<ZohoApi>();

                            var Cn5 = new SqlConnection(conexion);
                            var Cmd5 = new SqlCommand();

                            Cn5.Open();

                            Cmd5.Connection = Cn5;

                            Cmd5.CommandText = "UPDATE OPQT set U_IDZOHO = '" + respZoho.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

                            Cmd5.ExecuteNonQuery();
                            Cn5.Close();
                            Cn5.Dispose();

                        }
                        catch (Exception ex3)
                        {

                            BitacoraErrores be = new BitacoraErrores();
                            be.DocNum = DocNum;
                            be.Razon = ex3.Message;
                            be.StackTrace = ex3.StackTrace;
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                        }

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


        [Route("api/OrdenCompra/Modify")]
        public async Task<HttpResponseMessage> GetModifyOrden([FromUri] string DocNum = "")
        {


            try
            {

                Parametros parametros = db.Parametros.FirstOrDefault();
                HttpClient cliente = new HttpClient();


                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var SQL = parametros.SQLOrdenCompra + "'" + DocNum + "'";
                var conexion = g.DevuelveCadena();
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Encabezado");
                var Oportunidad = "";


                OrdenCompra orden = new OrdenCompra();

                orden.OrdenesVentas = new List<OrdenesVentas>();

                foreach (DataRow item in Ds.Tables["Encabezado"].Rows)
                {
                    try
                    {
                        HttpResponseMessage response2 = await cliente.GetAsync(parametros.UrlZoho + "/" + item["Concatenacion"].ToString());
                        if (response2.IsSuccessStatusCode)
                        {
                            response2.Content.Headers.ContentType.MediaType = "application/json";
                            var resp2 = await response2.Content.ReadAsAsync<ZohoApi>();

                            var ProyectID = resp2.data.Where(a => a.Deal_Name.ToLower().Contains(item["Concatenacion"].ToString().ToLower()) && !a.Stage.ToLower().Contains("Aprobada".ToLower()) && !a.Stage.ToLower().Contains("Cerrado".ToLower())).FirstOrDefault();
                            if (ProyectID != null)
                            {
                                Oportunidad = ProyectID.id.ToString();
                            }
                            else
                            {
                                throw new Exception("No se encontro el proyecto");

                            }
                        }
                        else
                        {
                            throw new Exception(response2.ReasonPhrase);
                        }
                    }
                    catch (Exception ex)
                    {

                        throw new Exception(ex.Message);
                    }

                    orden.DocEntry = item["DocEntry"].ToString();
                    orden.Fecha = Convert.ToDateTime(item["Fecha"].ToString());
                    orden.Comentarios = item["Comentarios"].ToString();
                    orden.Completo = item["Completo"].ToString() == "0" ? false : true;
                    var OrdenID = new OrdenesVentas();
                    OrdenID.IdZoho = Oportunidad;
                    orden.OrdenesVentas.Add(OrdenID);
                }


               

              


                HttpClient clienteProd = new HttpClient();

                var httpContent2Prod = new StringContent(JsonConvert.SerializeObject(orden), Encoding.UTF8, "application/json");
                clienteProd.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage response3 = await clienteProd.PutAsync(parametros.UrlPutOrdenCompra, httpContent2Prod);
                    if (response3.IsSuccessStatusCode)
                    {
                        response3.Content.Headers.ContentType.MediaType = "application/json";
                        var res = await response3.Content.ReadAsStringAsync();
                        BitacoraZoho bitEncabezado = new BitacoraZoho();
                        bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(orden);
                        bitEncabezado.Fecha = DateTime.Now;
                        bitEncabezado.DocNum = DocNum;
                        bitEncabezado.RespuestaZoho = res.ToString();
                        db.BitacoraZoho.Add(bitEncabezado);
                        db.SaveChanges();

                        try
                        {
                            var respZoho = await response3.Content.ReadAsAsync<ZohoApi>();

                            var Cn5 = new SqlConnection(conexion);
                            var Cmd5 = new SqlCommand();

                            Cn5.Open();

                            Cmd5.Connection = Cn5;

                            Cmd5.CommandText = "UPDATE OPOR set U_IDZOHO = '" + respZoho.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

                            Cmd5.ExecuteNonQuery();
                            Cn5.Close();
                            Cn5.Dispose();

                        }
                        catch (Exception ex3)
                        {

                            BitacoraErrores be = new BitacoraErrores();
                            be.DocNum = DocNum;
                            be.Razon = ex3.Message;
                            be.StackTrace = ex3.StackTrace;
                            be.Fecha = DateTime.Now;

                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                        }

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