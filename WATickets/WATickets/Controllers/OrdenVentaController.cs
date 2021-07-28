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
                        User = Conexion.Company.UserName
                    };
                }
                else
                {
                    resp = new
                    {

                        Type = "Orden de Venta",
                        Status = "Error",
                        Message = Conexion.Company.GetLastErrorDescription(),
                        User = Conexion.Company.UserName
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
                    User = Conexion.Company.UserName
                };
                Conexion.Desconectar();
               
                return Request.CreateResponse(HttpStatusCode.InternalServerError, resp);
            }
        }
    }
}