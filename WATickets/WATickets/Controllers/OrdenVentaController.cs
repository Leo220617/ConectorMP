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
    public class OrdenVentaController : ApiController
    {
        Conexion g = new Conexion();
        G G = new G();
        ModelCliente db = new ModelCliente();

        object resp;
        public async Task<HttpResponseMessage> Get()
        {


            try
            {
                string SQL = " select t0.CardCode, t0.CardName, t0.NumAtCard, t0.DocNum,t0.DocStatus, t0.Comments, t0.DocDate, t0.DocDueDate, ";
                SQL += " t0.DocCur,t0.VatSum, t0.VatSumFC, t0.DiscSum, t0.DiscSumFC,  t0.DocTotal, t0.DocTotalFC, t0.DocRate ";
                SQL += " from ordr t0  ";



                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "OrdenVenta");


                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK, Ds);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }


        }

        [Route("api/OrdenVenta/Insertar")]
        public async Task<HttpResponseMessage> Post([FromBody] OrdenVenta orden)
        {
            try
            {

                var SQL2 = "select top 1 CardCode from OCRD where E_Mail = '" + orden.cliente.Correo + "'";

                SqlConnection Cn2 = new SqlConnection(g.DevuelveCadena());
                SqlCommand Cmd2 = new SqlCommand(SQL2, Cn2);
                SqlDataAdapter Da2 = new SqlDataAdapter(Cmd2);
                DataSet Ds2 = new DataSet();
                Cn2.Open();
                Da2.Fill(Ds2, "Cliente");

                try
                {
                    orden.codCliente = Ds2.Tables["Cliente"].Rows[0]["CardCode"].ToString();
                    Cn2.Close();
                }
                catch (Exception ed)
                {
                    Cn2.Close();
                    try
                    {
                        var clien = (SAPbobsCOM.BusinessPartners)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                        clien.CardName = orden.cliente.clientName;
                        clien.GroupCode = orden.cliente.GroupCode;
                        clien.EmailAddress = orden.cliente.Correo;
                        clien.Phone1 = orden.cliente.telefono;
                        clien.FederalTaxID = orden.cliente.Cedula;
                        clien.Series = 55;
                        clien.Valid = BoYesNoEnum.tYES;
                        clien.CardType = BoCardTypes.cCustomer;
                        clien.Currency = "##";



                        clien.Addresses.SetCurrentLine(0);

                        clien.City = orden.cliente.city;
                        clien.County = orden.cliente.county;
                        clien.Address = (orden.cliente.street.Length > 49 ? orden.cliente.street.Substring(0, 49) : orden.cliente.street);
                        clien.Block = orden.cliente.block;
                        clien.ZipCode = orden.cliente.zipcode;
                        clien.Country = orden.cliente.country;
                        clien.BillToState = orden.cliente.state;

                        clien.Addresses.AddressName = orden.cliente.Address;
                        clien.Addresses.City = orden.cliente.city;
                        clien.Addresses.Country = orden.cliente.country;
                        clien.Addresses.County = orden.cliente.county;
                        clien.Addresses.Street = (orden.cliente.street.Length > 99 ? orden.cliente.street.Substring(0, 99) : orden.cliente.street);
                        clien.Addresses.TypeOfAddress = "S";
                        clien.Addresses.Block = orden.cliente.block;
                        clien.Addresses.ZipCode = orden.cliente.zipcode;
                        clien.Addresses.State = orden.cliente.state;
                        clien.Addresses.Add();

                        var respuest = clien.Add();

                        if (respuest == 0)
                        {
                            var SQL = "select top 1 CardCode from OCRD where E_Mail = '" + orden.cliente.Correo + "'";

                            SqlConnection Cn = new SqlConnection(g.DevuelveCadena());
                            SqlCommand Cmd = new SqlCommand(SQL, Cn);
                            SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                            DataSet Ds = new DataSet();
                            Cn.Open();
                            Da.Fill(Ds, "Cliente");

                            orden.codCliente = Ds.Tables["Cliente"].Rows[0]["CardCode"].ToString();

                            Cn.Close();
                        }
                        else
                        {
                            throw new Exception(Conexion.Company.GetLastErrorDescription());
                        }

                    }
                    catch (Exception ll)
                    {

                        throw;
                    }

                }




                var client = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                client.DocObjectCode = BoObjectTypes.oOrders;
                client.CardCode = orden.codCliente;
                client.DocCurrency = (orden.currency == "CRC" ? "COL" : orden.currency);
                client.DocDate = orden.creationDate;
                client.DocDueDate = orden.docDueDate;
                client.DocNum = 0; //automatico


                if (orden.items)
                {
                    client.DocType = BoDocumentTypes.dDocument_Items;
                }
                else
                {
                    client.DocType = BoDocumentTypes.dDocument_Service;
                }


                client.HandWritten = BoYesNoEnum.tNO;



                client.NumAtCard = orden.codCliente; //orderid

                client.ReserveInvoice = BoYesNoEnum.tNO;


                client.Series = 6; //6 primario
                client.TaxDate = orden.creationDate; //CreationDate




                client.Comments = orden.comments;
                client.SalesPersonCode = orden.salesPerson; //Quemado 47

                int i = 0;
                foreach (var item in orden.detalle)
                {
                    client.Lines.SetCurrentLine(i);
                    //client.Lines.CostingCode = "";
                    //client.Lines.CostingCode2 = "";
                    //client.Lines.CostingCode3 = "";
                    //client.Lines.CostingCode4 = "";
                    //client.Lines.CostingCode5 = "E-C-01";
                    client.Lines.Currency = orden.currency;
                    client.Lines.DiscountPercent = item.discountPercent;
                    client.Lines.ItemCode = item.itemCode;
                    client.Lines.Quantity = item.quantity;
                    client.Lines.TaxCode = item.taxCode;
                    client.Lines.TaxOnly = BoYesNoEnum.tNO;


                    client.Lines.UnitPrice = Convert.ToDouble(item.unitPrice);
                    client.Lines.WarehouseCode = item.wareHouseCode;
                    client.Lines.Add();
                    i++;
                }

                var respuesta = client.Add();
                if (respuesta == 0)
                {
                    resp = new
                    {

                        Type = "Orden de Venta",
                        Status = "Exitoso",
                        Message = "Orden creada exitosamente en SAP",
                        User = Conexion.Company.UserName,
                        CodCliente = orden.codCliente
                    };
                }
                else
                {
                    resp = new
                    {

                        Type = "Orden de Venta",
                        Status = "Error",
                        Message = Conexion.Company.GetLastErrorDescription(),
                        User = Conexion.Company.UserName,
                        CodCliente = ""
                    };
                }

                Conexion.Desconectar();
                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                resp = new
                {

                    Type = "Orden de Venta",
                    Status = "Error",
                    Message = ex.Message,
                    User = Conexion.Company.UserName,
                    CodCliente = ""
                };
                Conexion.Desconectar();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, resp);
            }
        }



        [Route("api/OrdenVenta/Modify")]
        public async Task<HttpResponseMessage> GetModify([FromUri] string DocNum = "", int cerrado = 0)
        {


            try
            {

                Parametros parametros = db.Parametros.FirstOrDefault();
                HttpClient cliente = new HttpClient();
                var ProductosZoho = new ZohoApiProductos();

                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var SQL = parametros.QOV + "'" + DocNum + "'"; //este es para encontrar el proyecto concatenado con la categoria
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

                var DealName = Concatenacion;



                try
                {
                    HttpResponseMessage response2 = await cliente.GetAsync(parametros.UrlZoho + "/" + Concatenacion);

                    if (response2.IsSuccessStatusCode)
                    {
                        response2.Content.Headers.ContentType.MediaType = "application/json";
                        var resp2 = await response2.Content.ReadAsAsync<ZohoApi>();
                        //Insercion en Zoho 
                        var SQL2 = parametros.QGOV2 + "'" + DocNum + "'"; //este se trae el encabezado de la orden de venta

                        SqlConnection Cn2 = new SqlConnection(conexion);
                        SqlCommand Cmd2 = new SqlCommand(SQL2, Cn2);
                        SqlDataAdapter Da2 = new SqlDataAdapter(Cmd2);
                        DataSet Ds2 = new DataSet();
                        Cn2.Open();
                        Da2.Fill(Ds2, "Encabezado2");


                        if (!string.IsNullOrEmpty(Ds2.Tables["Encabezado2"].Rows[0]["IdZoho"].ToString()))
                        {
                            var ProyectID = resp2.data.Where(a => a.Deal_Name.ToLower().Contains(Concatenacion.ToLower())).FirstOrDefault();

                            if (ProyectID != null)
                            {

                                try
                                {


                                    Cn.Close();



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
                                    encabezado.Impuestos = Math.Round(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["VatSum"]), 2);
                                    encabezado.ImpuestosFC = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["VatSumFC"]));
                                    encabezado.Descuento = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DiscSum"]));
                                    encabezado.DescuentoFC = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DiscSumFC"]));
                                    encabezado.DocTotal = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DocTotal"]));
                                    encabezado.DocTotalFC = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DocTotalFC"]));
                                    encabezado.TipoCambio = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DocRate"]));
                                    encabezado.IdZoho = Ds2.Tables["Encabezado2"].Rows[0]["IdZoho"].ToString();
                                    encabezado.CategoriaCierre = Ds2.Tables["Encabezado2"].Rows[0]["CategoriaCierre"].ToString();

                                    try
                                    {
                                        encabezado.FechaPrimerPago = Convert.ToDateTime(Ds2.Tables["Encabezado2"].Rows[0]["FechaPrimerPago"]);
                                        encabezado.PorPrimerPago = Convert.ToInt32(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["PPP"].ToString()));
                                        encabezado.FechaSegundoPago = Convert.ToDateTime(Ds2.Tables["Encabezado2"].Rows[0]["FechaSegundoPago"]);
                                        encabezado.PorSegundoPago = Convert.ToInt32(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["PSP"].ToString()));
                                        encabezado.FechaTercerPago = Convert.ToDateTime(Ds2.Tables["Encabezado2"].Rows[0]["FechaTercerPago"]);
                                        encabezado.PorTercerPago = Convert.ToInt32(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["PTP"].ToString()));
                                        encabezado.FechaCuartoPago = Convert.ToDateTime(Ds2.Tables["Encabezado2"].Rows[0]["FechaCuartoPago"]);
                                        encabezado.PorCuartoPago = Convert.ToInt32(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["PCP"].ToString()));
                                    }
                                    catch (Exception ex)
                                    {


                                    }


                                    try
                                    {
                                        if (cerrado == 1)
                                        {
                                            encabezado.TotalFacturado = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["TotalFacturado"]));
                                            encabezado.FechaCierre = DateTime.Now;
                                        }
                                        else
                                        {

                                            encabezado.FechaCierre = DateTime.Now;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        encabezado.FechaCierre = DateTime.Now;


                                    }





                                    var SQL1 = parametros.QGOV1 + "'" + encabezado.DocEntry + "'";

                                    SqlConnection Cn1 = new SqlConnection(conexion);
                                    SqlCommand Cmd1 = new SqlCommand(SQL1, Cn1);
                                    SqlDataAdapter Da1 = new SqlDataAdapter(Cmd1);
                                    DataSet Ds1 = new DataSet();
                                    Cn1.Open();
                                    Da1.Fill(Ds1, "Detalle");
                                    encabezado.Detalle = new List<DetOF>();
                                    //Busqueda de productos Zoho
                                    //HttpClient cl = new HttpClient();


                                    //cl.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    //try
                                    //{
                                    //    HttpResponseMessage responseCL = await cl.GetAsync(parametros.UrlProductos);
                                    //    if (responseCL.IsSuccessStatusCode)
                                    //    {
                                    //        var respCL = await responseCL.Content.ReadAsAsync<ZohoApiProductos>();

                                    //        ProductosZoho = respCL;
                                    //    }
                                    //    else
                                    //    {
                                    //        throw new Exception(response2.ReasonPhrase);
                                    //    }
                                    //}
                                    //catch (Exception exc)
                                    //{

                                    //    throw new Exception(exc.Message);
                                    //}

                                    //Termina busqueda de productos Zoho

                                    foreach (DataRow item2 in Ds1.Tables["Detalle"].Rows)
                                    {
                                        DetOF detalle = new DetOF();
                                        detalle.NumLinea = Convert.ToInt32(item2["Linea"]);
                                        detalle.ItemCode = item2["ItemCode"].ToString();

                                        detalle.ZohoProductId = item2["U_IDZOHO"].ToString(); //ProductosZoho.data.Where(a => a.Product_Code == detalle.ItemCode).FirstOrDefault() == null ? "" : ProductosZoho.data.Where(a => a.Product_Code == detalle.ItemCode).FirstOrDefault().id;
                                        detalle.Descripcion = item2["Dscription"].ToString();
                                        detalle.Cantidad = G.Redondeo(Convert.ToDecimal(item2["Quantity"]));
                                        detalle.PorDesc = G.Redondeo(Convert.ToDecimal(item2["DiscPrcnt"]));
                                        detalle.Moneda = item2["Currency"].ToString();
                                        detalle.TipoCambio = G.Redondeo(Convert.ToDecimal(item2["Rate"]));
                                        detalle.PrecioUnitario = G.Redondeo(Convert.ToDecimal(item2["Price"]));
                                        detalle.SubTotal = G.Redondeo(Convert.ToDecimal(item2["SubTotal"]));
                                        detalle.Impuestos = G.Redondeo(Convert.ToDecimal(item2["Impuestos"]));
                                        detalle.TotalDescuentos = G.Redondeo(Convert.ToDecimal(item2["TotalDescuento"]));
                                        detalle.TotalLinea = G.Redondeo(Convert.ToDecimal(item2["Total"]));

                                        if (string.IsNullOrEmpty(detalle.ZohoProductId))
                                        {
                                            ProductZoho item = new ProductZoho();
                                            item.IdProducto = detalle.ItemCode;
                                            item.Descripcion = detalle.Descripcion;
                                            item.Categoria = Categoria;

                                            HttpClient clienteProd = new HttpClient();

                                            var httpContent2Prod = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
                                            clienteProd.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                            try
                                            {
                                                HttpResponseMessage response3 = await clienteProd.PostAsync(parametros.UrlPostProductos, httpContent2Prod);
                                                if (response3.IsSuccessStatusCode)
                                                {
                                                    response3.Content.Headers.ContentType.MediaType = "application/json";
                                                    var res = await response3.Content.ReadAsStringAsync();
                                                    BitacoraZoho bitEncabezado = new BitacoraZoho();
                                                    bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(item);
                                                    bitEncabezado.Fecha = DateTime.Now;
                                                    bitEncabezado.DocNum = DocNum;
                                                    bitEncabezado.RespuestaZoho = res.ToString();
                                                    db.BitacoraZoho.Add(bitEncabezado);
                                                    db.SaveChanges();

                                                    detalle.ZohoProductId = bitEncabezado.RespuestaZoho;


                                                    //Actualizacion en sap 

                                                    var Cn5 = new SqlConnection(conexion);
                                                    var Cmd5 = new SqlCommand();

                                                    Cn5.Open();

                                                    Cmd5.Connection = Cn5;
                                                    var SQLQ = parametros.QGOV.Replace("@reemplazo", "'" + detalle.ZohoProductId + "'");
                                                    SQLQ = SQLQ.Replace("@re2", "'" + detalle.ItemCode + "'");
                                                    Cmd5.CommandText = SQLQ;

                                                    Cmd5.ExecuteNonQuery();
                                                    Cn5.Close();
                                                    Cn5.Dispose();



                                                }
                                            }
                                            catch (Exception exZ)
                                            {

                                                BitacoraErrores be = new BitacoraErrores();
                                                be.DocNum = DocNum;
                                                be.Razon = exZ.Message;
                                                be.StackTrace = exZ.StackTrace;
                                                be.Fecha = DateTime.Now;

                                                db.BitacoraErrores.Add(be);
                                                db.SaveChanges();

                                            }
                                        }


                                        //encabezado.Impuestos += detalle.Impuestos;


                                        encabezado.Detalle.Add(detalle);
                                    }

                                    Cn1.Close();


                                    Cn2.Close();

                                    HttpClient cliente2 = new HttpClient();

                                    var httpContent2 = new StringContent(JsonConvert.SerializeObject(encabezado), Encoding.UTF8, "application/json");
                                    cliente2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    try
                                    {
                                        HttpResponseMessage response3 = await cliente2.PutAsync(parametros.UrlOVZoho, httpContent2);
                                        if (response3.IsSuccessStatusCode)
                                        {
                                            response3.Content.Headers.ContentType.MediaType = "application/json";
                                            var res = await response3.Content.ReadAsStringAsync();
                                            BitacoraZoho bitEncabezado = new BitacoraZoho();
                                            bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(encabezado);
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

                                                Cmd5.CommandText = "UPDATE ORDR set U_IDZOHO = '" + ProyectID.id + "', U_IdZohoDoc = '" + respZoho.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

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
                                    catch (Exception exZ)
                                    {

                                        BitacoraErrores be = new BitacoraErrores();
                                        be.DocNum = DocNum;
                                        be.Razon = exZ.Message;
                                        be.StackTrace = exZ.StackTrace;
                                        be.Fecha = DateTime.Now;

                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();

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
                        }
                        else
                        {
                            var ProyectID = resp2.data.Where(a => a.Deal_Name.ToLower().Contains(Concatenacion.ToLower()) && !a.Stage.ToLower().Contains("Aprobada".ToLower()) && !a.Stage.ToLower().Contains("Cerrado".ToLower())).FirstOrDefault();
                            if (ProyectID != null)
                            {

                                try
                                {
                                    var Cn4 = new SqlConnection(conexion);
                                    var Cmd4 = new SqlCommand();

                                    Cn4.Open();

                                    Cmd4.Connection = Cn4;

                                    Cmd4.CommandText = "UPDATE ORDR set U_IDZOHO = '" + ProyectID.id + "' where DocEntry = '" + DocNum + "'";

                                    Cmd4.ExecuteNonQuery();
                                    Cn4.Close();
                                    Cn4.Dispose();

                                    Cn.Close();



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
                                    encabezado.Impuestos = Math.Round(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["VatSum"]), 2);
                                    encabezado.ImpuestosFC = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["VatSumFC"]));
                                    encabezado.Descuento = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DiscSum"]));
                                    encabezado.DescuentoFC = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DiscSumFC"]));
                                    encabezado.DocTotal = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DocTotal"]));
                                    encabezado.DocTotalFC = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DocTotalFC"]));
                                    encabezado.TipoCambio = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["DocRate"]));
                                    encabezado.IdZoho = ProyectID.id;
                                    encabezado.CategoriaCierre = Ds2.Tables["Encabezado2"].Rows[0]["CategoriaCierre"].ToString();

                                    try
                                    {
                                        encabezado.FechaPrimerPago = Convert.ToDateTime(Ds2.Tables["Encabezado2"].Rows[0]["FechaPrimerPago"]);
                                        encabezado.PorPrimerPago = Convert.ToInt32(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["PPP"].ToString()));
                                        encabezado.FechaSegundoPago = Convert.ToDateTime(Ds2.Tables["Encabezado2"].Rows[0]["FechaSegundoPago"]);
                                        encabezado.PorSegundoPago = Convert.ToInt32(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["PSP"].ToString()));
                                        encabezado.FechaTercerPago = Convert.ToDateTime(Ds2.Tables["Encabezado2"].Rows[0]["FechaTercerPago"]);
                                        encabezado.PorTercerPago = Convert.ToInt32(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["PTP"].ToString()));
                                        encabezado.FechaCuartoPago = Convert.ToDateTime(Ds2.Tables["Encabezado2"].Rows[0]["FechaCuartoPago"]);
                                        encabezado.PorCuartoPago = Convert.ToInt32(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["PCP"].ToString()));
                                    }
                                    catch (Exception ex)
                                    {


                                    }

                                    try
                                    {
                                        if (cerrado == 1)
                                        {
                                            encabezado.TotalFacturado = G.Redondeo(Convert.ToDecimal(Ds2.Tables["Encabezado2"].Rows[0]["TotalFacturado"]));
                                            encabezado.FechaCierre = DateTime.Now;
                                        }
                                        else
                                        {

                                            encabezado.FechaCierre = DateTime.Now;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        encabezado.FechaCierre = DateTime.Now;


                                    }




                                    var SQL1 = parametros.QGOV1 + "'" + encabezado.DocEntry + "'";

                                    SqlConnection Cn1 = new SqlConnection(conexion);
                                    SqlCommand Cmd1 = new SqlCommand(SQL1, Cn1);
                                    SqlDataAdapter Da1 = new SqlDataAdapter(Cmd1);
                                    DataSet Ds1 = new DataSet();
                                    Cn1.Open();
                                    Da1.Fill(Ds1, "Detalle");
                                    encabezado.Detalle = new List<DetOF>();
                                    //Busqueda de productos Zoho
                                    //HttpClient cl = new HttpClient();


                                    //cl.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    //try
                                    //{
                                    //    HttpResponseMessage responseCL = await cl.GetAsync(parametros.UrlProductos);
                                    //    if (responseCL.IsSuccessStatusCode)
                                    //    {
                                    //        var respCL = await responseCL.Content.ReadAsAsync<ZohoApiProductos>();

                                    //        ProductosZoho = respCL;
                                    //    }
                                    //    else
                                    //    {
                                    //        throw new Exception(response2.ReasonPhrase);
                                    //    }
                                    //}
                                    //catch (Exception exc)
                                    //{

                                    //    throw new Exception(exc.Message);
                                    //}

                                    //Termina busqueda de productos Zoho

                                    foreach (DataRow item2 in Ds1.Tables["Detalle"].Rows)
                                    {
                                        DetOF detalle = new DetOF();
                                        detalle.NumLinea = Convert.ToInt32(item2["Linea"]);
                                        detalle.ItemCode = item2["ItemCode"].ToString();

                                        detalle.ZohoProductId = item2["U_IDZOHO"].ToString(); //ProductosZoho.data.Where(a => a.Product_Code == detalle.ItemCode).FirstOrDefault() == null ? "" : ProductosZoho.data.Where(a => a.Product_Code == detalle.ItemCode).FirstOrDefault().id;
                                        detalle.Descripcion = item2["Dscription"].ToString();
                                        detalle.Cantidad = Convert.ToDecimal(item2["Quantity"]);
                                        detalle.PorDesc = Convert.ToDecimal(item2["DiscPrcnt"]);
                                        detalle.Moneda = item2["Currency"].ToString();
                                        detalle.TipoCambio = Convert.ToDecimal(item2["Rate"]);
                                        detalle.PrecioUnitario = G.Redondeo(Convert.ToDecimal(item2["Price"]));
                                        detalle.SubTotal = G.Redondeo(Convert.ToDecimal(item2["SubTotal"]));
                                        detalle.Impuestos = G.Redondeo(Convert.ToDecimal(item2["Impuestos"]));
                                        detalle.TotalDescuentos = G.Redondeo(Convert.ToDecimal(item2["TotalDescuento"]));
                                        detalle.TotalLinea = G.Redondeo(Convert.ToDecimal(item2["Total"]));

                                        if (string.IsNullOrEmpty(detalle.ZohoProductId))
                                        {
                                            ProductZoho item = new ProductZoho();
                                            item.IdProducto = detalle.ItemCode;
                                            item.Descripcion = detalle.Descripcion;
                                            item.Categoria = Categoria;

                                            HttpClient clienteProd = new HttpClient();

                                            var httpContent2Prod = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
                                            clienteProd.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                            try
                                            {
                                                HttpResponseMessage response3 = await clienteProd.PostAsync(parametros.UrlPostProductos, httpContent2Prod);
                                                if (response3.IsSuccessStatusCode)
                                                {
                                                    response3.Content.Headers.ContentType.MediaType = "application/json";
                                                    var res = await response3.Content.ReadAsStringAsync();
                                                    BitacoraZoho bitEncabezado = new BitacoraZoho();
                                                    bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(item);
                                                    bitEncabezado.Fecha = DateTime.Now;
                                                    bitEncabezado.DocNum = DocNum;
                                                    bitEncabezado.RespuestaZoho = res.ToString();
                                                    db.BitacoraZoho.Add(bitEncabezado);
                                                    db.SaveChanges();

                                                    detalle.ZohoProductId = bitEncabezado.RespuestaZoho;


                                                    //Actualizacion en sap 

                                                    var Cn5 = new SqlConnection(conexion);
                                                    var Cmd5 = new SqlCommand();

                                                    Cn5.Open();

                                                    Cmd5.Connection = Cn5;
                                                    var SQLQ = parametros.QGOV.Replace("@reemplazo", "'" + detalle.ZohoProductId + "'");
                                                    SQLQ = SQLQ.Replace("@re2", "'" + detalle.ItemCode + "'");
                                                    Cmd5.CommandText = SQLQ;

                                                    Cmd5.ExecuteNonQuery();
                                                    Cn5.Close();
                                                    Cn5.Dispose();



                                                }
                                            }
                                            catch (Exception exZ)
                                            {

                                                BitacoraErrores be = new BitacoraErrores();
                                                be.DocNum = DocNum;
                                                be.Razon = exZ.Message;
                                                be.StackTrace = exZ.StackTrace;
                                                be.Fecha = DateTime.Now;

                                                db.BitacoraErrores.Add(be);
                                                db.SaveChanges();

                                            }
                                        }


                                        //encabezado.Impuestos += detalle.Impuestos;


                                        encabezado.Detalle.Add(detalle);
                                    }

                                    Cn1.Close();


                                    Cn2.Close();

                                    HttpClient cliente2 = new HttpClient();

                                    var httpContent2 = new StringContent(JsonConvert.SerializeObject(encabezado), Encoding.UTF8, "application/json");
                                    cliente2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    try
                                    {
                                        HttpResponseMessage response3 = await cliente2.PutAsync(parametros.UrlOVZoho, httpContent2);
                                        if (response3.IsSuccessStatusCode)
                                        {
                                            response3.Content.Headers.ContentType.MediaType = "application/json";
                                            var res = await response3.Content.ReadAsStringAsync();
                                            BitacoraZoho bitEncabezado = new BitacoraZoho();
                                            bitEncabezado.JsonEnviado = JsonConvert.SerializeObject(encabezado);
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

                                                Cmd5.CommandText = "UPDATE ORDR set U_IDZOHO = '" + ProyectID.id + "', U_IdZohoDoc = '" + respZoho.data.FirstOrDefault().details.id + "' where DocEntry = '" + DocNum + "'";

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
                                    catch (Exception exZ)
                                    {

                                        BitacoraErrores be = new BitacoraErrores();
                                        be.DocNum = DocNum;
                                        be.Razon = exZ.Message;
                                        be.StackTrace = exZ.StackTrace;
                                        be.Fecha = DateTime.Now;

                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();

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