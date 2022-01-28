 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BitacoraZoho")]
    public partial class BitacoraZoho
    {
        public int id { get; set; }
        public string DocNum { get; set; }
        public string JsonEnviado { get; set; }
        public DateTime Fecha { get; set; }
        public string RespuestaZoho { get; set; }
    }
}