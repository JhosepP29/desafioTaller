using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InventarioTallerMecanico.Modelos;

namespace InventarioTallerMecanico.ValidadorDatos
{
    public static class Validador
    {
        private const string EJEMPLO_CODIGO = "F123";
        private const int UMBRAL_CANTIDAD_SOSPECHOSA = 500;

        public static void CancelarOperacion()
        {
            Console.WriteLine("\n[Operación cancelada] Presione cualquier tecla para volver al menú...");
            Console.ReadKey(true);
        }

        public static bool HayRepuestosRegistrados(List<Repuesto> inventario)
        {
            if (inventario.Count == 0)
            {
                Console.WriteLine("No hay repuestos registrados. Registre uno primero en la Opción 1.");
                Console.ReadKey(true);
                return false;
            }
            return true;
        }

        public static bool TrySeleccionarRepuestoExistente(List<Repuesto> inventario, out Repuesto repuesto)
        {
            while (true)
            {
                Console.Write("\nIngrese el código del repuesto (o ESC para volver): ");
                if (!TryLeerCodigoConTeclado(out string entrada))
                {
                    repuesto = null;
                    return false;
                }

                Repuesto r = inventario.FirstOrDefault(x => x.Codigo == entrada);
                if (r == null)
                {
                    Console.WriteLine("Repuesto no encontrado. Verifique el código en la tabla superior o presione ESC.");
                    continue;
                }

                repuesto = r;
                return true;
            }
        }

        public static bool TryLeerCodigoNuevo(List<Repuesto> inventario, out string codigo)
        {
            while (true)
            {
                Console.Write($"Código de pieza (ej: {EJEMPLO_CODIGO}): ");
                if (!TryLeerCodigoConTeclado(out string entrada))
                {
                    codigo = null;
                    return false;
                }

                if (inventario.Any(rep => rep.Codigo == entrada))
                {
                    Console.WriteLine("Ya existe un repuesto con ese código. Ingrese uno diferente o presione ESC.");
                    continue;
                }

                codigo = entrada;
                return true;
            }
        }

        public static bool TryLeerCodigoConTeclado(out string codigo)
        {
            char[] buffer = new char[4];
            int posicion = 0;

            while (true)
            {
                ConsoleKeyInfo tecla = Console.ReadKey(intercept: true);

                if (tecla.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    codigo = null;
                    return false;
                }

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
            codigo = new string(buffer);
            return true;
        }

        public static bool TryLeerTextoConTeclado(string mensaje, string mensajeError, bool permitirVacio, out string resultado)
        {
            while (true)
            {
                Console.Write(mensaje);
                string entrada = "";

                while (true)
                {
                    ConsoleKeyInfo tecla = Console.ReadKey(intercept: true);

                    if (tecla.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine();
                        resultado = null;
                        return false;
                    }

                    if (tecla.Key == ConsoleKey.Backspace)
                    {
                        if (entrada.Length > 0)
                        {
                            entrada = entrada.Substring(0, entrada.Length - 1);
                            Console.Write("\b \b");
                        }
                        continue;
                    }

                    if (tecla.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        break;
                    }

                    char c = tecla.KeyChar;
                    // Bloquea caracteres de control y el delimitador '|' para proteger el archivo plano
                    if (!char.IsControl(c) && c != '|' && entrada.Length < 45)
                    {
                        entrada += c;
                        Console.Write(c);
                    }
                }

                if (permitirVacio && string.IsNullOrWhiteSpace(entrada))
                {
                    resultado = "";
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(entrada))
                {
                    resultado = entrada.Trim();
                    return true;
                }

                Console.WriteLine(mensajeError);
            }
        }

        public static bool TryLeerDecimalConTeclado(string mensaje, bool permitirVacio, out decimal? valor)
        {
            Console.Write(mensaje);
            string entrada = "";
            bool tienePunto = false;
            int decimales = 0;

            while (true)
            {
                ConsoleKeyInfo tecla = Console.ReadKey(intercept: true);

                if (tecla.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    valor = null;
                    return false;
                }

                if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (entrada.Length > 0)
                    {
                        char ultimo = entrada[entrada.Length - 1];
                        if (ultimo == '.')
                        {
                            tienePunto = false;
                        }
                        else if (tienePunto && decimales > 0)
                        {
                            decimales--;
                        }
                        entrada = entrada.Substring(0, entrada.Length - 1);
                        Console.Write("\b \b");
                    }
                    continue;
                }

                if (tecla.Key == ConsoleKey.Enter)
                {
                    if (permitirVacio && entrada.Length == 0)
                    {
                        Console.WriteLine();
                        valor = null;
                        return true;
                    }

                    if (entrada.Length > 0 && entrada != "0." &&
                        decimal.TryParse(entrada, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsed))
                    {
                        Console.WriteLine();
                        valor = parsed;
                        return true;
                    }
                    continue;
                }

                char c = tecla.KeyChar;

                if (char.IsDigit(c))
                {
                    if (tienePunto)
                    {
                        if (decimales < 2)
                        {
                            entrada += c;
                            decimales++;
                            Console.Write(c);
                        }
                    }
                    else if (entrada.Length < 8)
                    {
                        entrada += c;
                        Console.Write(c);
                    }
                }
                else if ((c == '.' || c == ',') && !tienePunto)
                {
                    if (entrada.Length == 0)
                    {
                        entrada = "0.";
                        Console.Write("0.");
                    }
                    else
                    {
                        entrada += ".";
                        Console.Write(".");
                    }
                    tienePunto = true;
                }
            }
        }

        public static bool TryLeerEnteroConTeclado(string mensaje, bool permitirCero, out int valor)
        {
            Console.Write(mensaje);
            string entrada = "";

            while (true)
            {
                ConsoleKeyInfo tecla = Console.ReadKey(intercept: true);

                if (tecla.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    valor = 0;
                    return false;
                }

                if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (entrada.Length > 0)
                    {
                        entrada = entrada.Substring(0, entrada.Length - 1);
                        Console.Write("\b \b");
                    }
                    continue;
                }

                if (tecla.Key == ConsoleKey.Enter)
                {
                    if (entrada.Length > 0 && int.TryParse(entrada, out int parsed))
                    {
                        if (permitirCero || parsed > 0)
                        {
                            Console.WriteLine();
                            valor = parsed;
                            return true;
                        }
                    }
                    continue;
                }

                char c = tecla.KeyChar;
                if (char.IsDigit(c) && entrada.Length < 7)
                {
                    if (!permitirCero && entrada.Length == 0 && c == '0')
                        continue;

                    if (entrada == "0")
                    {
                        entrada = c.ToString();
                        Console.Write($"\b{c}");
                    }
                    else
                    {
                        entrada += c;
                        Console.Write(c);
                    }
                }
            }
        }

        public static bool TryLeerCantidadConConfirmacion(string mensaje, out int cantidad)
        {
            while (true)
            {
                if (!TryLeerEnteroConTeclado(mensaje, false, out cantidad))
                {
                    return false;
                }

                if (cantidad > UMBRAL_CANTIDAD_SOSPECHOSA)
                {
                    if (!TryLeerConfirmacion($"La cantidad ({cantidad}) es inusualmente alta. ¿Confirma? (S/N): ", out bool confirmado))
                    {
                        return false;
                    }

                    if (!confirmado)
                    {
                        Console.WriteLine("Cantidad descartada. Vuelva a ingresarla o presione ESC.");
                        continue;
                    }
                }
                return true;
            }
        }

        public static bool TryLeerConfirmacion(string mensaje, out bool confirmado)
        {
            Console.Write(mensaje);
            while (true)
            {
                ConsoleKeyInfo tecla = Console.ReadKey(intercept: true);

                if (tecla.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    confirmado = false;
                    return false;
                }

                char c = char.ToUpper(tecla.KeyChar);
                if (c == 'S' || c == 'N')
                {
                    Console.WriteLine(c);
                    confirmado = (c == 'S');
                    return true;
                }
            }
        }
    }
}