﻿using Newtonsoft.Json;
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
    public class OFVController : ApiController
    {
        Conexion g = new Conexion();
        ModelCliente db = new ModelCliente();
        G G = new G();
        object resp;

        public async Task<HttpResponseMessage> Get([FromUri] string IdZoho = "")
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault();
               // var SQL = parametros.QGOFV + "'" + IdZoho + "'";
                var SQL = parametros.QGOFV;
                var conexion = g.DevuelveCadena();
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Encabezado");

                List<EncOF> enc = new List<EncOF>();



                foreach (DataRow item in Ds.Tables["Encabezado"].Rows)

                {
                    EncOF encabezado = new EncOF();
                    encabezado.DocEntry = item["DocEntry"].ToString();
                    encabezado.CardCode = item["CardCode"].ToString();
                    encabezado.CardName = item["CardName"].ToString();
                    encabezado.NumAtCard = item["NumAtCard"].ToString();
                    encabezado.DocNum = item["DocNum"].ToString();
                    encabezado.DocStatus = item["DocStatus"].ToString();
                    encabezado.Comments = item["Comments"].ToString();
                    encabezado.DocDate = Convert.ToDateTime(item["DocDate"]);
                    encabezado.DocDueDate = Convert.ToDateTime(item["DocDueDate"]);
                    encabezado.DocCur = item["DocCur"].ToString();
                    encabezado.Impuestos = Convert.ToDecimal(item["VatSum"]);
                    encabezado.ImpuestosFC = Convert.ToDecimal(item["VatSumFC"]);
                    encabezado.Descuento = Convert.ToDecimal(item["DiscSum"]);
                    encabezado.DescuentoFC = Convert.ToDecimal(item["DiscSumFC"]);
                    encabezado.DocTotal = Convert.ToDecimal(item["DocTotal"]);
                    encabezado.DocTotalFC = Convert.ToDecimal(item["DocTotalFC"]);
                    encabezado.TipoCambio = Convert.ToDecimal(item["DocRate"]);
                    encabezado.IdZoho = item["IdZoho"].ToString();

                    var SQL1 = parametros.QGOFV1 + "'" + encabezado.DocEntry + "'";

                    SqlConnection Cn1 = new SqlConnection(conexion);
                    SqlCommand Cmd1 = new SqlCommand(SQL1, Cn1);
                    SqlDataAdapter Da1 = new SqlDataAdapter(Cmd1);
                    DataSet Ds1 = new DataSet();
                    Cn1.Open();
                    Da1.Fill(Ds1, "Detalle");
                    encabezado.Detalle = new List<DetOF>();
                    foreach (DataRow item2 in Ds1.Tables["Detalle"].Rows)
                    {
                        DetOF detalle = new DetOF();
                        detalle.NumLinea = Convert.ToInt32(item2["Linea"]);
                        detalle.ItemCode = item2["ItemCode"].ToString();
                        detalle.Descripcion = item2["Dscription"].ToString();
                        detalle.Cantidad = Convert.ToDecimal(item2["Quantity"]);
                        detalle.PorDesc = Convert.ToDecimal(item2["DiscPrcnt"]);
                        detalle.Moneda =  item2["Currency"].ToString();
                        detalle.TipoCambio = Convert.ToDecimal(item2["Rate"]);
                        detalle.PrecioUnitario = Convert.ToDecimal(item2["Price"]);
                        detalle.SubTotal = Convert.ToDecimal(item2["SubTotal"]);
                        detalle.Impuestos = Convert.ToDecimal(item2["Impuestos"]);
                        detalle.TotalDescuentos = Convert.ToDecimal(item2["TotalDescuento"]);
                        detalle.TotalLinea = Convert.ToDecimal(item2["Total"]);
                        encabezado.Detalle.Add(detalle);
                    }

                    Cn1.Close();
                    enc.Add(encabezado);
                }
                    
                Cn.Close();
                return Request.CreateResponse(HttpStatusCode.OK,enc);
            }
            catch (Exception ex)
            {

                BitacoraErrores be = new BitacoraErrores();
                be.DocNum = IdZoho;
                be.Razon = ex.Message;
                be.StackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;

                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }


        [Route("api/OFV/Modify")]
        public async Task<HttpResponseMessage> GetModify([FromUri] string DocNum = "")
        {


            try
            {

                Parametros parametros = db.Parametros.FirstOrDefault();
                HttpClient cliente = new HttpClient();


                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    HttpResponseMessage response2 = await cliente.GetAsync(parametros.UrlZoho);
                    if (response2.IsSuccessStatusCode)
                    {
                        response2.Content.Headers.ContentType.MediaType = "application/json";
                        var resp2 = await response2.Content.ReadAsAsync<ZohoApi>();

                        var SQL = parametros.QOFV + "'" + DocNum + "'";
                        var conexion = g.DevuelveCadena();
                        SqlConnection Cn = new SqlConnection(conexion);
                        SqlCommand Cmd = new SqlCommand(SQL, Cn);
                        SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                        DataSet Ds = new DataSet();
                        Cn.Open();
                        Da.Fill(Ds, "Encabezado");

                        var Categoria = Ds.Tables["Encabezado"].Rows[0]["Categoria"].ToString();
                        var Proyecto = Ds.Tables["Encabezado"].Rows[0]["Proyecto"].ToString();

                        var Concatenacion = Proyecto + " - " + Categoria;
                        var ProyectID = resp2.data.Where(a => a.Deal_Name.ToLower().Contains(Concatenacion.ToLower())).FirstOrDefault();

                        if (ProyectID != null)
                        {

                            try
                            {
                                var Cn4 = new SqlConnection(conexion);
                                var Cmd4 = new SqlCommand();

                                Cn4.Open();

                                Cmd4.Connection = Cn4;

                                Cmd4.CommandText = "UPDATE OQUT set U_IDZOHO = '" + resp2.data.FirstOrDefault().id + "' where DocEntry = '" + DocNum + "'";

                                Cmd4.ExecuteNonQuery();
                                Cn4.Close();
                                Cn4.Dispose();

                                Cn.Close();


                                //Insercion en Zoho 
                                var SQL2 = parametros.QGOFV2 + "'" + DocNum + "'";

                                SqlConnection Cn2 = new SqlConnection(conexion);
                                SqlCommand Cmd2 = new SqlCommand(SQL2, Cn2);
                                SqlDataAdapter Da2 = new SqlDataAdapter(Cmd2);
                                DataSet Ds2 = new DataSet();
                                Cn2.Open();
                                Da2.Fill(Ds2, "Encabezado2");
                                EncOF encabezado = new EncOF();
                                encabezado.DocEntry = Ds2.Tables["Encabezado2"].Rows[0]["DocEntry"].ToString();
                                encabezado.CardCode = Ds2.Tables["Encabezado2"].Rows[0]["CardCode"].ToString();
                                encabezado.CardName = Ds2.Tables["Encabezado2"].Rows[0]["CardName"].ToString();
                                encabezado.NumAtCard = Ds2.Tables["Encabezado2"].Rows[0]["NumAtCard"].ToString();
                                encabezado.DocNum = Ds2.Tables["Encabezado2"].Rows[0]["DocNum"].ToString();
                                encabezado.DocStatus = Ds2.Tables["Encabezado2"].Rows[0]["DocStatus"].ToString();
                                encabezado.Comments = Ds2.Tables["Encabezado2"].Rows[0]["Comments"].ToString();
                                encabezado.DocDate = Convert.ToDateTime(Ds2.Tables["Encabezado2"].Rows[0]["DocDate"]);
                                encabezado.DocDueDate = Convert.ToDateTime(Ds2.Tables["Encabezado2"].Rows[0]["DocDueDate"]);
                                encabezado.DocCur = Ds2.Tables["Encabezado2"].Rows[0]["DocCur"].ToString();
                                encabezado.Impuestos = Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["VatSum"]);
                                encabezado.ImpuestosFC = Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["VatSumFC"]);
                                encabezado.Descuento = Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DiscSum"]);
                                encabezado.DescuentoFC = Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DiscSumFC"]);
                                encabezado.DocTotal = Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DocTotal"]);
                                encabezado.DocTotalFC = Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DocTotalFC"]);
                                encabezado.TipoCambio = Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DocRate"]);
                                encabezado.IdZoho = Ds2.Tables["Encabezado2"].Rows[0]["IdZoho"].ToString();

                                var SQL1 = parametros.QGOFV1 + "'" + encabezado.DocEntry + "'";

                                SqlConnection Cn1 = new SqlConnection(conexion);
                                SqlCommand Cmd1 = new SqlCommand(SQL1, Cn1);
                                SqlDataAdapter Da1 = new SqlDataAdapter(Cmd1);
                                DataSet Ds1 = new DataSet();
                                Cn1.Open();
                                Da1.Fill(Ds1, "Detalle");
                                encabezado.Detalle = new List<DetOF>();
                                foreach (DataRow item2 in Ds1.Tables["Detalle"].Rows)
                                {
                                    DetOF detalle = new DetOF();
                                    detalle.NumLinea = Convert.ToInt32(item2["Linea"]);
                                    detalle.ItemCode = item2["ItemCode"].ToString();
                                    detalle.Descripcion = item2["Dscription"].ToString();
                                    detalle.Cantidad = Convert.ToDecimal(item2["Quantity"]);
                                    detalle.PorDesc = Convert.ToDecimal(item2["DiscPrcnt"]);
                                    detalle.Moneda = item2["Currency"].ToString();
                                    detalle.TipoCambio = Convert.ToDecimal(item2["Rate"]);
                                    detalle.PrecioUnitario = Convert.ToDecimal(item2["Price"]);
                                    detalle.SubTotal = Convert.ToDecimal(item2["SubTotal"]);
                                    detalle.Impuestos = Convert.ToDecimal(item2["Impuestos"]);
                                    detalle.TotalDescuentos = Convert.ToDecimal(item2["TotalDescuento"]);
                                    detalle.TotalLinea = Convert.ToDecimal(item2["Total"]);
                                    encabezado.Detalle.Add(detalle);
                                }

                                Cn1.Close();


                                Cn2.Close();

                                HttpClient cliente2 = new HttpClient();

                                var httpContent2 = new StringContent(JsonConvert.SerializeObject(encabezado), Encoding.UTF8, "application/json");
                                cliente2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                try
                                {
                                    HttpResponseMessage response3 = await cliente2.PutAsync(parametros.UrlPutZoho, httpContent2);
                                    if (response3.IsSuccessStatusCode)
                                    {
                                        response3.Content.Headers.ContentType.MediaType = "application/json";
                                        var res = await response3.Content.ReadAsStringAsync();

                                       
                                    }
                                }
                                catch (Exception)
                                {

                                     
                                }
                                //Insercion en Zoho
                            }
                            catch (Exception ex)
                            {

                                throw new Exception(ex.Message);
                            }

                        }
                        else
                        {
                            throw new Exception("No se encontro el proyecto");
                        }




                        return Request.CreateResponse(HttpStatusCode.OK);
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