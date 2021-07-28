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
    public class ProductosController: ApiController
    {
        Conexion g = new Conexion();
        G G = new G();

        public async Task<HttpResponseMessage> Get()
        {


            try
            {
                string SQL = " select t0.ItemCode as IdProducto, t1.CardCode as DuenoProducto, t1.itemName as NombreProducto,t3.ItmsGrpNam as Categoria, t1.validFor as Activo, t2.Price as Precio, t1.VATLiable as Impuesto, t0.OnHand - t0.IsCommited as Stock, t1.U_BYL_UoM   as UnidadMedida, t0.WhsCode as CodBodega, t4.WhsName as NomBodega ";
                SQL += " from oitw t0 ";
                SQL += " inner join oitm t1 on t0.ItemCode = t1.ItemCode  ";
                SQL += " inner join itm1 t2 on t0.ItemCode = t2.ItemCode  and t2.PriceList = '2' inner join OITB t3 on t1.ItmsGrpCod = t3.ItmsGrpCod inner join owhs t4 on t0.WhsCode = t4.WhsCode ";
                SQL += " where t0.OnHand - t0.IsCommited > 0 and t3.ItmsGrpCod in (110,112,147,109,127,113,120,146) ";
        

                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());  
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Productos");

                
                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK,Ds);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }


        }
    }
}