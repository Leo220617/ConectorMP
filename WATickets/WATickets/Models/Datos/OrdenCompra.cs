using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class OrdenCompra
    {
       
        public string DocEntry { get; set; }
        public DateTime Fecha { get; set; }
        public bool Completo { get; set; }
        public string Comentarios { get; set; }
        public string IdZoho { get; set; }
        
    }
    
}
 