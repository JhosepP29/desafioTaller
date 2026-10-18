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
            Console.WriteLine("\n--- REGISTRAR NUEVO REPUESTO ---");
            Repuesto r = new Repuesto
            {
                Codigo = LeerCodigoNuevo(),
                Nombre = LeerTextoNoVacio("Descripción del repuesto: ", "La descripción no puede estar vacía."),
                Categoria = LeerTextoNoVacio("Categoría (Ej: Frenos, Motor, Lubricantes): ", "La categoría no puede estar vacía."),
                Precio = LeerDecimalNoNegativo("Precio unitario: S/ "),
                Stock = LeerEnteroNoNegativo("Stock inicial en almacén: ")
            };
            inventario.Add(r);
            Console.WriteLine("\nRepuesto registrado correctamente en el sistema del taller.");

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
        // =========================================================
        // MÉTODOS AUXILIARES DE VALIDACIÓN
        // =========================================================
        private string LeerCodigoNuevo()
        {
            while (true)
            {
                Console.Write($"Código de pieza (ej: {EJEMPLO_CODIGO}): ");
                string entrada = LeerCodigoConTeclado();
                if (inventario.Any(rep => rep.Codigo == entrada))
                {
                    Console.WriteLine("Ya existe un repuesto con ese código. Ingrese uno diferente.");
                    continue;
                }
                return entrada;
            }
        }

        private string LeerCodigoConTeclado()
        {
            char[] buffer = new char[4];
            int posicion = 0;
            while (true)
            {
                ConsoleKeyInfo tecla = Console.ReadKey(intercept: true);
                if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (posicion > 0)
                    {
                        posicion--;
                        Console.Write("\b \b");
                    }
                    continue;
                }
                if (tecla.Key == ConsoleKey.Enter)
                {
                    if (posicion == 4) break;
                    continue;
                }
                if (posicion >= 4) continue;

                char c = tecla.KeyChar;
                if (posicion == 0 && char.IsLetter(c))
                {
                    buffer[posicion] = char.ToUpper(c);
                    Console.Write(buffer[posicion]);
                    posicion++;
                }
                else if (posicion >= 1 && char.IsDigit(c))
                {
                    buffer[posicion] = c;
                    Console.Write(c);
                    posicion++;
                    if (posicion == 4) break;
                }
            }
            Console.WriteLine();
            return new string(buffer);
        }

        private string LeerTextoNoVacio(string mensaje, string mensajeError)
        {
            string texto;
            do
            {
                Console.Write(mensaje);
                texto = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(texto)) Console.WriteLine(mensajeError);
            } while (string.IsNullOrWhiteSpace(texto));
            return texto.Trim();
        }

        private decimal LeerDecimalNoNegativo(string mensaje)
        {
            decimal valor;
            while (true)
            {
                Console.Write(mensaje);
                if (decimal.TryParse(Console.ReadLine(), NumberStyles.Number, CultureInfo.InvariantCulture, out valor) && valor >= 0)
                    return valor;
                Console.WriteLine("Valor inválido. Ingrese un precio válido (ej: 120.50).");
            }
        }

        private int LeerEnteroNoNegativo(string mensaje)
        {
            int valor;
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out valor) && valor >= 0) return valor;
                Console.WriteLine("Formato incorrecto. Ingrese un número entero sin signos negativos.");
            }
        }
    }
}
