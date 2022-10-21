
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
        public string UrlProductos { get; set; }
        public string QOV { get; set; }
        public string QGOV { get; set; }
        public string QGOV1 { get; set; }
        public string QGOV2 { get; set; }
        public string UrlOVZoho { get; set; }
        public string SQLProductos { get; set; }
        public string UrlPostProductos { get; set; }
        public string SQLProducto { get; set; }
        public string SQLPagos { get; set; }
        public string UrlPutPagos { get; set; }
        public string SQLOfertasCompra { get; set; }
        public string UrlPutOfertasCompra { get; set; }
        public string SQLOrdenCompra { get; set; }
        public string UrlPutOrdenCompra { get; set; }
    }
}