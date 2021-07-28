using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class Cliente
    {
        public string CardCode { get; set; }
        public string clientName { get; set; }
        public string Correo { get; set; }
        public string telefono { get; set; }
        public string Cedula { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string county { get; set; }
        public string street { get; set; }
        public string Address { get; set; }
        public string state { get; set; }
        public string zipcode { get; set; }
        public string block { get; set; }
        public int GroupCode { get; set; }

    }

    
}