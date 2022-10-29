using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class FacturaImportacion
    {
        public string DocEntry { get; set; }
        public DateTime Fecha { get; set; }
        public string Naviera { get; set; }
        public int DiasTransito { get; set; }
        public DateTime FechaSalidaEsperada { get; set; }
        public DateTime FechaArriboEsperada { get; set; }
        public DateTime FechaSalidaReal { get; set; }
        public DateTime FechaArriboReal { get; set; }
        public List<OrdenesVentasI> OrdenesVentas { get; set; }
    }

    public class OrdenesVentasI
    {
        public string IdZoho { get; set; }
    }
}