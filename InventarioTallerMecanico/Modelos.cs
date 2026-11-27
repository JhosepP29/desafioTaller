using System;

namespace InventarioTallerMecanico.Modelos
{
    public class Repuesto
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    public class Despacho
    {
        public string CodigoRepuesto { get; set; } = "";
        public string NombreRepuesto { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
        public DateTime Fecha { get; set; }
    }

    public enum TipoMovimiento
    {
        Entrada,
        Salida
    }

    public class MovimientoStock
    {
        public string CodigoRepuesto { get; set; } = "";
        public string NombreRepuesto { get; set; } = "";
        public TipoMovimiento Tipo { get; set; }
        public int Cantidad { get; set; }
        public int StockResultante { get; set; }
        public string Motivo { get; set; } = "";
        public DateTime Fecha { get; set; }
    }
}