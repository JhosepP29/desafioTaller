using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace InventarioTallerMecanico
{
    // =============================================================
    //  MODELOS
    // =============================================================
    public class Repuesto
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    // =============================================================
    //  GESTOR PRINCIPAL DEL TALLER
    // =============================================================
    public class GestorTaller
    {
        // Uso de colecciones (Listas/Arreglos)
        public List<Repuesto> inventario = new List<Repuesto>();
        private const string EJEMPLO_CODIGO = "F123";

        // MÓDULO 1: REGISTRO
        public void RegistrarRepuesto()
        {
            
            Console.ReadKey();
        }

        // MÓDULO 2: ORDENAMIENTO (Listado)
        public void ListarInventario()
        {
            
            Console.ReadKey();
        }

        // MÓDULO 3: BÚSQUEDA
        public void BuscarRepuesto()
        {
            
            Console.ReadKey();
        }
    }
}