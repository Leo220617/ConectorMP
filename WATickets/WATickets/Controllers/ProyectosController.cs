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
    public class ProyectosController : ApiController
    {
        Conexion g = new Conexion();
        G G = new G();

        public async Task<HttpResponseMessage> Get()
        {


            try
            {
                string SQL = " select t0.PrjCode as CodProyecto, t0.PrjName as NomProyecto, t0.U_Disenador as Diseñador ";
                SQL += " from oprj t0 ";
                SQL += " where t0.Active = 'Y' ";



                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Proyectos");


                Cn.Close();

                return Request.CreateResponse(HttpStatusCode.OK, Ds);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }


        }


        [Route("api/Proyectos/Insertar")]
        public async Task<HttpResponseMessage> Post([FromBody] Proyectos proyecto)
        {
            try
            {

                string SQL2 = " select t0.PrjCode as CodProyecto ";
                SQL2 += " from oprj t0 ";
                SQL2 += " where t0.Active = 'Y' and t0.PrjCode = '" + proyecto.codProyecto + "' ";



                SqlConnection Cn2 = new SqlConnection(g.DevuelveCadena());
                SqlCommand Cmd2 = new SqlCommand(SQL2, Cn2);
                SqlDataAdapter Da2 = new SqlDataAdapter(Cmd2);
                DataSet Ds2 = new DataSet();
                Cn2.Open();
                Da2.Fill(Ds2, "Proyectos");

                var proye2 = "";
                try
                {
                    proye2 = Ds2.Tables["Proyectos"].Rows[0]["CodProyecto"].ToString();

                }
                catch (Exception ex)
                {

                }

                Cn2.Close();

                if (!string.IsNullOrEmpty(proye2))
                {
                    throw new Exception("Ya fue creado");
                }




                var oInvoice = (SAPbobsCOM.CompanyService)Conexion.Company.GetCompanyService();
                var oInvoice1 = (SAPbobsCOM.ProjectsService) oInvoice.GetBusinessService(SAPbobsCOM.ServiceTypes.ProjectsService);
                var oInvoice2 = (SAPbobsCOM.Project)oInvoice1.GetDataInterface(SAPbobsCOM.ProjectsServiceDataInterfaces.psProject);

                oInvoice2.Code = proyecto.codProyecto;
                oInvoice2.Name = proyecto.nomProyecto;
                oInvoice2.Active = BoYesNoEnum.tYES;
                oInvoice2.UserFields.Item("U_Disenador").Value = proyecto.diseñador;
                oInvoice2.UserFields.Item("U_Porcentaje").Value = proyecto.porcComision;

                var resp = oInvoice1.AddProject(oInvoice2);

                

                Conexion.Desconectar();

                string SQL = " select t0.PrjCode as CodProyecto ";
                SQL += " from oprj t0 ";
                SQL += " where t0.Active = 'Y' and t0.PrjCode = '"+proyecto.codProyecto+"' ";



                SqlConnection Cn = new SqlConnection(g.DevuelveCadena());
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Proyectos");

                var proye = "";
                try
                {
                    proye = Ds.Tables["Proyectos"].Rows[0]["CodProyecto"].ToString();

                }
                catch (Exception ex)
                {
                    
                }

                Cn.Close();

                if(string.IsNullOrEmpty(proye))
                {
                    throw new Exception("No fue creado");
                }

                return Request.CreateResponse(HttpStatusCode.OK, proye);
            }
            catch (Exception ex)
            {
                Conexion.Desconectar();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }






    }
}