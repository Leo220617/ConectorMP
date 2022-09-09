using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class ZohoApi
    {

        public List<data> data { get; set; }
    }

    public class data
    {
        public string id { get; set; }
        public string Deal_Name { get; set; }
        public string Stage { get; set; }
        public details details { get; set; }
    }
    public class details
    {
        public string id { get; set; }
    }
}