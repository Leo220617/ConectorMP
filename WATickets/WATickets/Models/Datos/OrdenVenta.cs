using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class OrdenVenta
    {
        public string codCliente { get; set; }
        public string currency { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime docDueDate { get; set; }
        public string comments { get; set; }
        public int salesPerson { get; set; }
        public bool items { get; set; }
        public DetOrdenVenta[] detalle { get; set; }
    }

    public class DetOrdenVenta
    {
        public string itemCode { get; set; }
        public int quantity { get; set; }
        public string taxCode { get; set; }
        public decimal unitPrice { get; set; }
        public string wareHouseCode { get; set; }
        public int discountPercent { get; set; }
    }
        
}