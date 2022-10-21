using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class OrdenCompra
    {
        public string IdZoho { get; set; }
        public string DocEntry { get; set; }
        public DateTime Fecha { get; set; }
        public OfertaCompra1 OfertaCompra { get; set; }
    }
    
}
public class OfertaCompra1
{
    public string DocEntry { get; set; }

}