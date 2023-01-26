using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class EntregaBodega
    {
        public string DocEntry { get; set; }
        public DateTime Fecha { get; set; }
        public string IdZoho { get; set; }
        public bool Completo { get; set; }
    }
}