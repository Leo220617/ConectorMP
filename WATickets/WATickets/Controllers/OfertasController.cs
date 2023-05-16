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
                ZohoApi respuesta = new ZohoApi();
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
                oferta.Fecha = oferta.Fecha.AddHours(DateTime.Now.Hour);
                oferta.Fecha = oferta.Fecha.AddMinutes(DateTime.Now.Minute);
                oferta.CodProveedor = Ds.Tables["Encabezado"].Rows[0]["CodProveedor"].ToString().ToString();
                oferta.NomProveedor = Ds.Tables["Encabezado"].Rows[0]["Proveedor"].ToString().ToString();
                oferta.IdZoho = Oportunidad;


                //HttpClient clienteProd = new HttpClient();
                cliente = new HttpClient();
                var httpContent2Prod = new StringContent(JsonConvert.SerializeObject(oferta), Encoding.UTF8, "application/json");
                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage response3 = await cliente.PutAsync(parametros.UrlPutOfertasCompra, httpContent2Prod);
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

                            respuesta = respZoho;

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

                try
                {


                    var Cn4 = new SqlConnection(conexion);
                    var Cmd4 = new SqlCommand();

                    Cn4.Open();

                    Cmd4.Connection = Cn4;

                    Cmd4.CommandText = "UPDATE OPQT set U_IDZOHO = '" + Oportunidad + "', U_IdZohoDoc = '" + respuesta.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

                    Cmd4.ExecuteNonQuery();
                    Cn4.Close();
                    Cn4.Dispose();

                    Cn.Close();

                  
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

                    orden.Fecha = orden.Fecha.AddHours(DateTime.Now.Hour);
                    orden.Fecha = orden.Fecha.AddMinutes(DateTime.Now.Minute);
                    orden.Comentarios = item["Comentarios"].ToString();
                    orden.Completo = item["Completo"].ToString() == "0" ? false : true;

                    orden.IdZoho = Oportunidad;

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

                            Cmd5.CommandText = "UPDATE OPOR set U_IDZOHO = '" + Oportunidad + "', U_IdZohoDoc = '" + respZoho.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

                            Cmd5.ExecuteNonQuery();
                            Cn5.Close();
                            Cn5.Dispose();
                            Cn.Close();

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


        [Route("api/Importacion/Modify")]
        public async Task<HttpResponseMessage> GetModifyImportacion([FromUri] string DocNum = "")
        {


            try
            {

                Parametros parametros = db.Parametros.FirstOrDefault();
                HttpClient cliente = new HttpClient();


                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var SQL = parametros.SQLFacturaImportacion + "'" + DocNum + "'";
                var conexion = g.DevuelveCadena();
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Encabezado");
                var Oportunidad = "";


                FacturaImportacion FI = new FacturaImportacion();

                FI.Oportunidades_Zoho = new List<Oportunidades_Zoho>();

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

                    FI.DocEntry = item["DocEntry"].ToString();
                    FI.Fecha = Convert.ToDateTime(item["Fecha"].ToString());
                    FI.Fecha = FI.Fecha.AddHours(DateTime.Now.Hour);
                    FI.Fecha = FI.Fecha.AddMinutes(DateTime.Now.Minute);
                    FI.Naviera = item["Naviera"].ToString();
                    FI.DiasTransito = Convert.ToInt32(item["DiasTransito"].ToString());

                    
                    try
                    {
                        FI.FechaSalidaEsperada = Convert.ToDateTime(item["FechaSalidaEsperada"].ToString());
                        FI.FechaArriboEsperada = Convert.ToDateTime(item["FechaArriboEsperada"].ToString());
                        FI.FechaSalidaReal = Convert.ToDateTime(item["FechaSalidaReal"].ToString());
                        FI.FechaArriboReal = Convert.ToDateTime(item["FechaArriboReal"].ToString());

                    }
                    catch (Exception)
                    {

                        
                    }


                    var OrdenID = new Oportunidades_Zoho();
                    OrdenID.IdZoho = Oportunidad;
                    OrdenID.Completo = item["Completo"].ToString() == "0" ? false : true;
                    FI.Oportunidades_Zoho.Add(OrdenID);
                }







                HttpClient clienteProd = new HttpClient();

                var httpContent2Prod = new StringContent(JsonConvert.SerializeObject(FI), Encoding.UTF8, "application/json");
                clienteProd.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage response3 = await clienteProd.PutAsync(parametros.UrlPutFacturaImportacion, httpContent2Prod);
                    if (response3.IsSuccessStatusCode)
                    {
                        response3.Content.Headers.ContentType.MediaType = "application/json";
                        var res = await response3.Content.ReadAsStringAsync();
                        BitacoraZoho bitEncabezado = new BitacoraZoho();
                        bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(FI);
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

                            Cmd5.CommandText = "UPDATE OPCH set U_IDZOHO = '" + Oportunidad + "', U_IdZohoDoc = '" + respZoho.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

                            Cmd5.ExecuteNonQuery();
                            Cn5.Close();
                            Cn5.Dispose();
                            Cn.Close();

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



        [Route("api/Entrada/Modify")]
        public async Task<HttpResponseMessage> GetEntrada([FromUri] string DocNum = "")
        {


            try
            {

                Parametros parametros = db.Parametros.FirstOrDefault();
                HttpClient cliente = new HttpClient();


                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var SQL = parametros.SQLEntradaBodega + "'" + DocNum + "'";
                var conexion = g.DevuelveCadena();
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Encabezado");
                var Oportunidad = "";


                EntradaMercancias EM = new EntradaMercancias();

                EM.Oportunidades_Zoho = new List<Oportunidades_Zoho>();

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

                    EM.DocEntry = item["DocEntry"].ToString();
                    EM.Fecha = Convert.ToDateTime(item["Fecha"].ToString());
                    EM.Fecha = EM.Fecha.AddHours(DateTime.Now.Hour);
                    EM.Fecha = EM.Fecha.AddMinutes(DateTime.Now.Minute);


                    var OrdenID = new Oportunidades_Zoho();
                    OrdenID.IdZoho = Oportunidad;
                    EM.Oportunidades_Zoho.Add(OrdenID);
                }







                HttpClient clienteProd = new HttpClient();

                var httpContent2Prod = new StringContent(JsonConvert.SerializeObject(EM), Encoding.UTF8, "application/json");
                clienteProd.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage response3 = await clienteProd.PutAsync(parametros.UrlPutEntradaBodega, httpContent2Prod);
                    if (response3.IsSuccessStatusCode)
                    {
                        response3.Content.Headers.ContentType.MediaType = "application/json";
                        var res = await response3.Content.ReadAsStringAsync();
                        BitacoraZoho bitEncabezado = new BitacoraZoho();
                        bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(EM);
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

                            Cmd5.CommandText = "UPDATE OPDN set U_IDZOHO = '" + Oportunidad + "', U_IdZohoDoc = '" + respZoho.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

                            Cmd5.ExecuteNonQuery();
                            Cn5.Close();
                            Cn5.Dispose();
                            Cn.Close();

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




        [Route("api/Entrega/Modify")]
        public async Task<HttpResponseMessage> GetEntrega([FromUri] string DocNum = "")
        {


            try
            {

                Parametros parametros = db.Parametros.FirstOrDefault();
                HttpClient cliente = new HttpClient();


                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var SQL = parametros.SQLEntregaBodega + "'" + DocNum + "'";
                var conexion = g.DevuelveCadena();
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Encabezado");
                var Oportunidad = "";


                EntregaBodega EM = new EntregaBodega();



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

                    EM.DocEntry = item["DocEntry"].ToString();
                    EM.Fecha = Convert.ToDateTime(item["Fecha"].ToString());
                    EM.Fecha = EM.Fecha.AddHours(DateTime.Now.Hour);
                    EM.Fecha = EM.Fecha.AddMinutes(DateTime.Now.Minute);
                    EM.Completo = item["Completo"].ToString() == "0" ? false : true;

                    EM.IdZoho = Oportunidad;
                }







                HttpClient clienteProd = new HttpClient();

                var httpContent2Prod = new StringContent(JsonConvert.SerializeObject(EM), Encoding.UTF8, "application/json");
                clienteProd.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage response3 = await clienteProd.PutAsync(parametros.UrlPutEntregaBodega, httpContent2Prod);
                    if (response3.IsSuccessStatusCode)
                    {
                        response3.Content.Headers.ContentType.MediaType = "application/json";
                        var res = await response3.Content.ReadAsStringAsync();
                        BitacoraZoho bitEncabezado = new BitacoraZoho();
                        bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(EM);
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

                            Cmd5.CommandText = "UPDATE ODLN set U_IDZOHO = '" + Oportunidad + "', U_IdZohoDoc = '" + respZoho.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

                            Cmd5.ExecuteNonQuery();
                            Cn5.Close();
                            Cn5.Dispose();
                            Cn.Close();

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

        [Route("api/NCInvoice/Modify")]
        public async Task<HttpResponseMessage> GetNCInvoice([FromUri] string DocNum = "", int factura = 0)
        {


            try
            {

                Parametros parametros = db.Parametros.FirstOrDefault();
                HttpClient cliente = new HttpClient();


                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var SQL = "";
                if(factura == 0)
                {
                      SQL = parametros.SQLNC + "'" + DocNum + "'";

                }
                else
                {
                      SQL = parametros.SQLInvoice + "'" + DocNum + "'";

                }
                var conexion = g.DevuelveCadena();
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Encabezado");
             


                NCInvoice NCFac = new NCInvoice();

                if(Ds.Tables["Encabezado"].Rows.Count > 0)
                {
                    foreach (DataRow item in Ds.Tables["Encabezado"].Rows)
                    {
                         

                        NCFac.DocNum = item["DocNum"].ToString();
                        NCFac.Fecha = Convert.ToDateTime(item["Fecha"].ToString());
                        NCFac.Fecha = NCFac.Fecha.AddHours(DateTime.Now.Hour);
                        NCFac.Fecha = NCFac.Fecha.AddMinutes(DateTime.Now.Minute);
                        NCFac.IdZohoOdV = item["IdZohoOdV"].ToString();

                        NCFac.Comments = item["Comments"].ToString();
                        NCFac.NC = G.Redondeo(Convert.ToDecimal(item["NC"]));
                        NCFac.Invoice = G.Redondeo(Convert.ToDecimal(item["Invoice"]));
                        NCFac.Subtotal_NC = G.Redondeo(Convert.ToDecimal(item["Subtotal_NC"]));
                        NCFac.Subtotal_Invoice = G.Redondeo(Convert.ToDecimal(item["Subtotal_Invoice"]));


                    }

                    HttpClient clienteProd = new HttpClient();

                    var httpContent2Prod = new StringContent(JsonConvert.SerializeObject(NCFac), Encoding.UTF8, "application/json");
                    clienteProd.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    try
                    {
                        HttpResponseMessage response3 = await clienteProd.PostAsync(parametros.UrlPostNCInvoice, httpContent2Prod);
                        if (response3.IsSuccessStatusCode)
                        {
                            response3.Content.Headers.ContentType.MediaType = "application/json";
                            var res = await response3.Content.ReadAsStringAsync();
                            BitacoraZoho bitEncabezado = new BitacoraZoho();
                            bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(NCFac);
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
                                if(factura == 1)
                                {
                                    Cmd5.CommandText = "UPDATE OINV set  U_IdZohoDoc = '" + respZoho.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

                                }
                                else
                                {
                                    Cmd5.CommandText = "UPDATE ORIN set  U_IdZohoDoc = '" + respZoho.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

                                }


                                Cmd5.ExecuteNonQuery();
                                Cn5.Close();
                                Cn5.Dispose();
                                Cn.Close();

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


                }
















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