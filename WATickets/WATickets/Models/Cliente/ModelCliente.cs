namespace WATickets.Models.Cliente
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ModelCliente : DbContext
    {
        public ModelCliente()
            : base("name=ModelCliente")
        {
        }

        public virtual DbSet<ConexionSAP> ConexionSAP { get; set; }
        public virtual DbSet<BitacoraErrores> BitacoraErrores { get; set; }
        public virtual DbSet<BitacoraZoho> BitacoraZoho { get; set; }

        public virtual DbSet<Parametros> Parametros { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

        }
    }
}
