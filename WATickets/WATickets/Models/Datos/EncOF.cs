using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Datos
{
    public class EncOF
    {
        public string DocEntry { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string NumAtCard { get; set; }
        public string DocNum { get; set; }
        public string DocStatus { get; set; }
        public string Comments { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public string DocCur { get; set; }
        public decimal Impuestos { get; set; }
        public decimal ImpuestosFC { get; set; }
        public decimal Descuento { get; set; }
        public decimal DescuentoFC { get; set; }
        public decimal DocTotal { get; set; }
        public decimal DocTotalFC { get; set; }
        public decimal TipoCambio { get; set; }
        public string IdZoho { get; set; }
        public List<DetOF> Detalle { get; set; }
    }
    public class DetOF
    {
        public int NumLinea { get; set; }
        public string ItemCode { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PorDesc { get; set; }
        public string Moneda { get; set; }
        public decimal TipoCambio { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal TotalDescuentos { get; set; }
        public decimal TotalLinea { get; set; }
    }
}