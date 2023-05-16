using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class NCInvoice
    {
        public string DocNum { get; set; }
        public string IdZohoOdV { get; set; }
        public DateTime Fecha { get; set; }
        public string Comments { get; set; }
        public decimal NC { get; set; }
        public decimal Invoice { get; set; }
        public decimal Subtotal_NC { get; set; }
        public decimal Subtotal_Invoice { get; set; }
    }
}