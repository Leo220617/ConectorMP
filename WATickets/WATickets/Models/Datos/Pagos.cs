using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class Pagos
    {
        public string IdZohoOdV { get; set; }
        public string DocNum { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal Monto { get; set; }
        public int NoPago { get; set; }
    }
}