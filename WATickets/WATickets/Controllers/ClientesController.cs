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


namespace WATickets.Controllers
{
    [Authorize]
    public class ClientesController : ApiController
    {
        Conexion g = new Conexion();
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
    }
}