using System;

namespace InventarioTallerMecanico
{
    class Program
    {
        static void Main(string[] args)
        {
            // Ahora C# reconoce a GestorTaller automáticamente
            GestorTaller gestor = new GestorTaller();
            int opcion = 0;

            do
            {
                Console.Clear();
                Console.WriteLine("==========================================");
                Console.WriteLine("  SISTEMA DE INVENTARIO - TALLER MECÁNICO");
                Console.WriteLine("             CAJAMARCA - PERÚ");
                Console.WriteLine("==========================================");
                Console.WriteLine("  1. Registrar repuesto");
                Console.WriteLine("  2. Listar inventario (Ordenado)");
                Console.WriteLine("  3. Buscar repuesto por código");
                Console.WriteLine("  4. Registrar entrada de repuestos (Compras)");
                Console.WriteLine("  5. Registrar salida de repuestos (Merma/Uso)");
                Console.WriteLine("  6. Registrar despacho para servicio/venta");
                Console.WriteLine("  7. Ver reporte de despachos/ventas");
                Console.WriteLine("  8. Modificar datos de repuesto");
                Console.WriteLine("  9. Eliminar repuesto");
                Console.WriteLine(" 10. Historial de movimientos (Kardex)");
                Console.WriteLine(" 11. Salir");
                Console.WriteLine("==========================================");
                Console.Write("Seleccione una opción: ");
                if (!int.TryParse(Console.ReadLine(), out opcion))
                {
                    opcion = 0;
                }

                switch (opcion)
                {
                    case 1: gestor.RegistrarRepuesto(); break;
                    case 2: gestor.ListarInventario(); break;
                    case 3: gestor.BuscarRepuesto(); break;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10: Error(); break;
                    case 11: Console.WriteLine("\nGracias por usar el sistema del taller. ¡Hasta pronto!"); break;
                    default: Error(); break;
                }
            } while (opcion != 11);
        }
        static void Error()
        {
            Console.WriteLine("Opción no válida o aún no implementada. Intente nuevamente");
            Console.ReadKey();
        }
    }
}