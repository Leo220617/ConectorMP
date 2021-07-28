using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [EnableCors("*", "*", "*")]
    public class LoginController: ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [Route("api/Login/Conectar")]
        public async Task<HttpResponseMessage> GetLoginAsync([FromUri] string codCompañia)
        {
            try
            {
                var Compañia = db.ConexionSAP.Where(a => a.id == codCompañia).FirstOrDefault();

                if(Compañia == null)
                {
                    throw new Exception("Compañia no existe");
                }

                var token = TokenGenerator.GenerateTokenJwt(Compañia.id, Compañia.id);

                return Request.CreateResponse(HttpStatusCode.OK, token);


            }
            catch (Exception ex)
            {
                G.GuardarTxt("ErrorLogin.txt", ex.ToString());

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }
    }
}