using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class ZohoApiProductos
    {
        public List<datas> data { get; set; }
    }

    public class datas
    {
        public string id { get; set; }
        public string Product_Code { get; set; }
    }
}