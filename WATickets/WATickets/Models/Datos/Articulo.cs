using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class Articulo
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public List<Hijos> Hijos { get; set; }
    }
    public class Hijos
    {
        public string codPadre { get; set; }
        public string CodigoHijo { get; set; }
        public string NombreHijo { get; set; }
        public decimal Cantidad { get; set; }
        public string CodBodega { get; set; }

    }
}