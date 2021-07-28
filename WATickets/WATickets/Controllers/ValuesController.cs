using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WATickets.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        // GET api/values
        G G = new G();
        public string Get()
        {


            try
            {

                
                int resp = Conexion.Company.Connect();
                
                if (resp != 0)
                {


                    return Conexion.Company.GetLastErrorDescription();
                }
                else
                {

                    return resp.ToString();
                }

            }
            catch (Exception ex)
            {

                return ex.Message;
            }


        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
