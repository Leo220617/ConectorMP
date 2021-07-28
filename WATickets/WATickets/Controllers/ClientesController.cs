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
    public class ClientesController : ApiController
    {
        Conexion g = new Conexion();
        object resp;
        G G = new G();

        public async Task<HttpResponseMessage> Get()
        {


            try
            {
                string SQL = " select t0.CardCode as CodCliente, t0.CardName as NomCliente,t0.LicTradNum as Identificacion, t0.E_Mail as Correo, t0.Phone1 as Telefono, t0.Address as Direccion ";
                SQL += " from ocrd t0 ";
                SQL += " where t0.CardType = 'C'  ";
         


                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Clientes");


                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK, Ds);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }


        }


        [Route("api/Clientes/Insertar")]
        public async Task<HttpResponseMessage> Post([FromBody] Cliente cliente)
        {
            try
            {
                var client = (SAPbobsCOM.BusinessPartners)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);
                //client.CardCode = cliente.CardCode;
                client.CardName = cliente.clientName;
                client.GroupCode = cliente.GroupCode;
                client.EmailAddress = cliente.Correo;
                client.Phone1 = cliente.telefono;
                client.FederalTaxID = cliente.Cedula;
                client.Series = 55;
                client.Valid = BoYesNoEnum.tYES;
                client.CardType = BoCardTypes.cCustomer;
                client.Currency = "##";



                client.Addresses.SetCurrentLine(0);

                client.City = cliente.city;
                client.County = cliente.county;
                client.Address = (cliente.street.Length > 49 ? cliente.street.Substring(0, 49) : cliente.street);
                client.Block = cliente.block;
                client.ZipCode = cliente.zipcode;
                client.Country = cliente.country;
                client.BillToState = cliente.state;

                client.Addresses.AddressName = client.Address;
                client.Addresses.City = cliente.city;
                client.Addresses.Country = cliente.country;
                client.Addresses.County = cliente.county;
                client.Addresses.Street = (cliente.street.Length > 99 ? cliente.street.Substring(0, 99) : cliente.street);
                client.Addresses.TypeOfAddress = "S";
                client.Addresses.Block = cliente.block;
                client.Addresses.ZipCode = cliente.zipcode;
                client.Addresses.State = cliente.state;
                client.Addresses.Add();

                var respuest = client.Add();

                if (respuest != 0)
                {
                    resp = new
                    {

                        Type = "Cliente",
                        Status = "Error",
                        Message = Conexion.Company.GetLastErrorDescription(),
                        User = Conexion.Company.UserName
                    };
                }
                else
                {
                    resp = new
                    {

                        Type = "Cliente",
                        Status = "Exito",
                        Message = "Cliente creado correctamente " + client.CardCode,
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

                    Type = "Cliente",
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