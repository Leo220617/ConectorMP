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
    public class ProductosController : ApiController
    {
        Conexion g = new Conexion();
        G G = new G();

        public async Task<HttpResponseMessage> Get([FromUri] bool modificados = false, bool recienCreados = false)
        {


            try
            {
                string SQL = "";
                if (!modificados && !recienCreados)
                {
                    SQL = " select distinct t0.ItemCode as IdProducto, t1.CardCode as DuenoProducto, t1.itemName as NombreProducto,t3.ItmsGrpNam as Categoria, t1.validFor as Activo, t2.Price as Precio, t1.VATLiable as Impuesto, t0.OnHand - t0.IsCommited as Stock, t1.U_BYL_UoM   as UnidadMedida, t0.WhsCode as CodBodega, t4.WhsName as NomBodega , t1.U_M2PorCaja as M2PorCaja, t1.U_FactorDeVenta as FactorVenta, t3.ItmsGrpCod as CodigoGrupo  ";
                    SQL += " from oitw t0 ";
                    SQL += " inner join oitm t1 on t0.ItemCode = t1.ItemCode  ";
                    SQL += " inner join itm1 t2 on t0.ItemCode = t2.ItemCode  and t2.PriceList = '2' inner join OITB t3 on t1.ItmsGrpCod = t3.ItmsGrpCod inner join owhs t4 on t0.WhsCode = t4.WhsCode ";
                    SQL += " where t0.OnHand - t0.IsCommited > 0 and t3.ItmsGrpCod in (110,112,147,109,127,113,120,146) and t0.WhsCode = '01' ";
                }
                else if (modificados && !recienCreados)
                {

                    SQL = " ( select distinct t0.ItemCode as IdProducto, t1.CardCode as DuenoProducto, t1.itemName as NombreProducto,t3.ItmsGrpNam as Categoria, t1.validFor as Activo, t2.Price as Precio, t1.VATLiable as Impuesto, t0.OnHand - t0.IsCommited as Stock, t1.U_BYL_UoM   as UnidadMedida, t0.WhsCode as CodBodega, t4.WhsName as NomBodega, t1.U_M2PorCaja as M2PorCaja, t1.U_FactorDeVenta as FactorVenta, t3.ItmsGrpCod as CodigoGrupo  ";
                    SQL += " from oitw t0 ";
                    SQL += " inner join oitm t1 on t0.ItemCode = t1.ItemCode  ";
                    SQL += " inner join itm1 t2 on t0.ItemCode = t2.ItemCode  and t2.PriceList = '2' inner join OITB t3 on t1.ItmsGrpCod = t3.ItmsGrpCod inner join owhs t4 on t0.WhsCode = t4.WhsCode ";
                    SQL += " inner join inv1 t5 on t5.ItemCode = t0.ItemCode inner join oinv t6 on t6.DocEntry = t5.DocEntry ";
                    SQL += " where t0.OnHand - t0.IsCommited > 0 and t3.ItmsGrpCod in (110,112,147,109,127,113,120,146) and t0.WhsCode = '01' and t6.DocDate > DATEADD(DAY,-2,GETDATE() ))";
                    SQL += " UNION ";
                    SQL += " ( select distinct t0.ItemCode as IdProducto, t1.CardCode as DuenoProducto, t1.itemName as NombreProducto,t3.ItmsGrpNam as Categoria, t1.validFor as Activo, t2.Price as Precio, t1.VATLiable as Impuesto, t0.OnHand - t0.IsCommited as Stock, t1.U_BYL_UoM   as UnidadMedida, t0.WhsCode as CodBodega, t4.WhsName as NomBodega, t1.U_M2PorCaja as M2PorCaja, t1.U_FactorDeVenta as FactorVenta, t3.ItmsGrpCod as CodigoGrupo  ";
                    SQL += " from oitw t0 ";
                    SQL += " inner join oitm t1 on t0.ItemCode = t1.ItemCode  ";
                    SQL += " inner join itm1 t2 on t0.ItemCode = t2.ItemCode  and t2.PriceList = '2' inner join OITB t3 on t1.ItmsGrpCod = t3.ItmsGrpCod inner join owhs t4 on t0.WhsCode = t4.WhsCode ";
                    SQL += " inner join rdr1 t5 on t5.ItemCode = t0.ItemCode inner join ordr t6 on t6.DocEntry = t5.DocEntry ";
                    SQL += " where t0.OnHand - t0.IsCommited > 0 and t3.ItmsGrpCod in (110,112,147,109,127,113,120,146) and t0.WhsCode = '01' and t6.DocDate > DATEADD(DAY,-2,GETDATE() )) ";

                }
                else if (!modificados && recienCreados)
                {
                    SQL = " select distinct t0.ItemCode as IdProducto, t1.CardCode as DuenoProducto, t1.itemName as NombreProducto,t3.ItmsGrpNam as Categoria, t1.validFor as Activo, t2.Price as Precio, t1.VATLiable as Impuesto, t0.OnHand - t0.IsCommited as Stock, t1.U_BYL_UoM   as UnidadMedida, t0.WhsCode as CodBodega, t4.WhsName as NomBodega , t1.U_M2PorCaja as M2PorCaja, t1.U_FactorDeVenta as FactorVenta, t3.ItmsGrpCod as CodigoGrupo  ";
                    SQL += " from oitw t0  ";
                    SQL += " inner join oitm t1 on t0.ItemCode = t1.ItemCode ";
                    SQL += " inner join itm1 t2 on t0.ItemCode = t2.ItemCode  and t2.PriceList = '2' inner join OITB t3 on t1.ItmsGrpCod = t3.ItmsGrpCod inner join owhs t4 on t0.WhsCode = t4.WhsCode ";
                    SQL += " where t0.OnHand - t0.IsCommited > 0  and t3.ItmsGrpCod in (110,112,147,109,127,113,120,146) and t0.WhsCode = '01' and t1.CreateDate >= DATEADD(DAY,-2,GETDATE() ) ";
                }
                else
                {
                    SQL = " ( select distinct t0.ItemCode as IdProducto, t1.CardCode as DuenoProducto, t1.itemName as NombreProducto,t3.ItmsGrpNam as Categoria, t1.validFor as Activo, t2.Price as Precio, t1.VATLiable as Impuesto, t0.OnHand - t0.IsCommited as Stock, t1.U_BYL_UoM   as UnidadMedida, t0.WhsCode as CodBodega, t4.WhsName as NomBodega, t1.U_M2PorCaja as M2PorCaja, t1.U_FactorDeVenta as FactorVenta, t3.ItmsGrpCod as CodigoGrupo ";
                    SQL += " from oitw t0 ";
                    SQL += " inner join oitm t1 on t0.ItemCode = t1.ItemCode  ";
                    SQL += " inner join itm1 t2 on t0.ItemCode = t2.ItemCode  and t2.PriceList = '2' inner join OITB t3 on t1.ItmsGrpCod = t3.ItmsGrpCod inner join owhs t4 on t0.WhsCode = t4.WhsCode ";
                    SQL += " inner join inv1 t5 on t5.ItemCode = t0.ItemCode inner join oinv t6 on t6.DocEntry = t5.DocEntry ";
                    SQL += " where t0.OnHand - t0.IsCommited > 0 and t3.ItmsGrpCod in (110,112,147,109,127,113,120,146) and t0.WhsCode = '01' and t6.DocDate > DATEADD(DAY,-2,GETDATE() ))";
                    SQL += " UNION ";
                    SQL += " ( select distinct t0.ItemCode as IdProducto, t1.CardCode as DuenoProducto, t1.itemName as NombreProducto,t3.ItmsGrpNam as Categoria, t1.validFor as Activo, t2.Price as Precio, t1.VATLiable as Impuesto, t0.OnHand - t0.IsCommited as Stock, t1.U_BYL_UoM   as UnidadMedida, t0.WhsCode as CodBodega, t4.WhsName as NomBodega, t1.U_M2PorCaja as M2PorCaja, t1.U_FactorDeVenta as FactorVenta, t3.ItmsGrpCod as CodigoGrupo  ";
                    SQL += " from oitw t0 ";
                    SQL += " inner join oitm t1 on t0.ItemCode = t1.ItemCode  ";
                    SQL += " inner join itm1 t2 on t0.ItemCode = t2.ItemCode  and t2.PriceList = '2' inner join OITB t3 on t1.ItmsGrpCod = t3.ItmsGrpCod inner join owhs t4 on t0.WhsCode = t4.WhsCode ";
                    SQL += " inner join rdr1 t5 on t5.ItemCode = t0.ItemCode inner join ordr t6 on t6.DocEntry = t5.DocEntry ";
                    SQL += " where t0.OnHand - t0.IsCommited > 0 and t3.ItmsGrpCod in (110,112,147,109,127,113,120,146) and t0.WhsCode = '01' and t6.DocDate > DATEADD(DAY,-2,GETDATE() )) ";
                    SQL += " UNION ";
                    SQL += " (select distinct t0.ItemCode as IdProducto, t1.CardCode as DuenoProducto, t1.itemName as NombreProducto,t3.ItmsGrpNam as Categoria, t1.validFor as Activo, t2.Price as Precio, t1.VATLiable as Impuesto, t0.OnHand - t0.IsCommited as Stock, t1.U_BYL_UoM   as UnidadMedida, t0.WhsCode as CodBodega, t4.WhsName as NomBodega, t1.U_M2PorCaja as M2PorCaja, t1.U_FactorDeVenta as FactorVenta, t3.ItmsGrpCod as CodigoGrupo   ";
                    SQL += " from oitw t0  ";
                    SQL += " inner join oitm t1 on t0.ItemCode = t1.ItemCode ";
                    SQL += " inner join itm1 t2 on t0.ItemCode = t2.ItemCode  and t2.PriceList = '2' inner join OITB t3 on t1.ItmsGrpCod = t3.ItmsGrpCod inner join owhs t4 on t0.WhsCode = t4.WhsCode ";
                    SQL += " where t0.OnHand - t0.IsCommited > 0  and t3.ItmsGrpCod in (110,112,147,109,127,113,120,146) and t0.WhsCode = '01' and t1.CreateDate >= DATEADD(DAY,-2,GETDATE() )) ";
                }



                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Productos");


                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK, Ds);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }


        }

        [Route("api/Productos/GetComponentes")]
        public async Task<HttpResponseMessage> GetPadres()
        {
            try
            {
                var SQL = " SELECT T0.[Code]'Codigo Padre',T2.[ItemName]'Descripcion Padre',  T1.[ChildNum] 'Num.Linea', T1.[Quantity], T1.[Code] 'Componente', (SELECT TOP 1 Z0.ItemName FROM OITM Z0 WHERE Z0.itemcode=t1.code) 'Descripcion Componente',T1.[Warehouse]  ";
                SQL += " FROM OITT T0  INNER JOIN ITT1 T1 ON T0.[Code] = T1.[Father] INNER JOIN OITM T2 ON T0.[Code] = T2.[ItemCode] WHERE T2.[ItmsGrpCod] IN ('101','113','120') ";
                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Productos");


                List<Articulo> articulos = new List<Articulo>();
                List<Articulo> articulosAcu = new List<Articulo>();
                List<Hijos> hijos = new List<Hijos>();

                foreach (DataRow item in Ds.Tables["Productos"].Rows)
                {
                    if(Convert.ToInt32(item["Num.Linea"]) == 0)
                    {
                        Articulo art = new Articulo();
                        art.Codigo = item["Codigo Padre"].ToString();
                        art.Nombre = item["Descripcion Padre"].ToString();

                        articulos.Add(art);
                    }
                    else
                    {
                        Hijos hij = new Hijos();
                        hij.codPadre = item["Codigo Padre"].ToString();
                        hij.CodigoHijo = item["Componente"].ToString();
                        hij.NombreHijo = item["Descripcion Componente"].ToString();
                        hij.Cantidad = Convert.ToDecimal(item["Quantity"]);
                        hij.CodBodega = item["Warehouse"].ToString();
                        hijos.Add(hij);
                    }
                }

                foreach(var i in articulos)
                {
                    Articulo art = new Articulo();
                    art.Codigo = i.Codigo;
                    art.Nombre = i.Nombre;
                    art.Hijos = new List<Hijos>();

                    foreach (var j in hijos.Where(a => a.codPadre == art.Codigo).ToList())
                    {
                        Hijos hij = new Hijos();
                        hij.codPadre = j.codPadre;
                        hij.CodigoHijo = j.CodigoHijo;
                        hij.NombreHijo = j.NombreHijo;
                        hij.Cantidad = j.Cantidad;
                        hij.CodBodega = j.CodBodega;
                        art.Hijos.Add(hij);
                    }
                    articulosAcu.Add(art);
                }

                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK, articulosAcu);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }
    }
}