using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class EntradaMercancias
    {
        public string DocEntry { get; set; }
        public DateTime Fecha { get; set; }
        public List<Oportunidades_Zoho> Oportunidades_Zoho { get; set; }

    }
}