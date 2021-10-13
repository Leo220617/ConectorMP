using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WATickets.Models.Datos;

namespace WATickets.Controllers
{
    [Authorize]
    public class OrdenVentaController: ApiController
    {
        Conexion g = new Conexion();
        G G = new G();
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
        public async Task<HttpResponseMessage> Post([FromBody] OrdenVenta orden )
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
                  orden.codCliente =  Ds2.Tables["Cliente"].Rows[0]["CardCode"].ToString();
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

                        if(respuest == 0)
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

          
                if(orden.items)
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
    }
}