
namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Parametros")]
    public partial class Parametros
    {
        public int id { get; set; }
        public string UrlZoho { get; set; }
        public string QOFV { get; set; }
        public string QGOFV { get; set; }
        public string QGOFV1 { get; set; }
        public string QGOFV2 { get; set; }
        public string UrlPutZoho { get; set; }
    }
}