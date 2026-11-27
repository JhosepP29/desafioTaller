using System;
using System.Collections.Generic;
using System.Linq;
using InventarioTallerMecanico.Modelos;
using InventarioTallerMecanico.Persistencias;
using InventarioTallerMecanico.ValidadorDatos;

namespace InventarioTallerMecanico
{
    public class GestorTaller
    {
        private const int STOCK_MINIMO_CRITICO = 3;

        // Colecciones encapsuladas en memoria
        private List<Repuesto> inventario;
        private List<Despacho> despachos;
        private List<MovimientoStock> movimientos;

        // Instancias de los servicios especializados
        private readonly Persistencia persistencia;
        private readonly Exportar exportar;

        public GestorTaller()
        {
            persistencia = new Persistencia();
            exportar = new Exportar();

            inventario = persistencia.CargarInventario();
            despachos = persistencia.CargarDespachos();
            movimientos = persistencia.CargarMovimientos();
        }

        //  OPCIÓN 1: REGISTRAR REPUESTO
        public void RegistrarRepuesto()
        {
            Console.WriteLine("\n--- REGISTRAR NUEVO REPUESTO ---");
            Console.WriteLine("(Presione ESC en cualquier campo para cancelar y volver al menú)\n");

            if (!Validador.TryLeerCodigoNuevo(inventario, out string codigo)) { Validador.CancelarOperacion(); return; }
            if (!Validador.TryLeerTextoConTeclado("Descripción del repuesto: ", "La descripción no puede estar vacía.", false, out string nombre)) { Validador.CancelarOperacion(); return; }
            if (!Validador.TryLeerTextoConTeclado("Categoría (Ej: Frenos, Motor, Soldadura): ", "La categoría no puede estar vacía.", false, out string categoria)) { Validador.CancelarOperacion(); return; }
            if (!Validador.TryLeerDecimalConTeclado("Precio unitario: S/ ", false, out decimal? precio)) { Validador.CancelarOperacion(); return; }
            if (!Validador.TryLeerEnteroConTeclado("Stock inicial en almacén: ", true, out int stock)) { Validador.CancelarOperacion(); return; }

            Repuesto r = new Repuesto
            {
                Codigo = codigo,
                Nombre = nombre,
                Categoria = categoria,
                Precio = precio.Value,
                Stock = stock
            };

            inventario.Add(r);
            persistencia.GuardarInventario(inventario);

            if (r.Stock > 0)
            {
                RegistrarMovimiento(r, TipoMovimiento.Entrada, r.Stock, "Inventario inicial");
            }

            Console.WriteLine("\nRepuesto registrado y guardado en archivo correctamente.");
            Console.ReadKey(true);
        }

        //  OPCIÓN 2: LISTAR INVENTARIO (ORDENADO Y VALORIZADO)
        public void ListarInventario(bool pausarAlFinal = true)
        {
            Console.WriteLine("\n--- INVENTARIO DE REPUESTOS ---");
            if (inventario.Count == 0)
            {
                Console.WriteLine("No hay repuestos registrados en el almacén.");
                if (pausarAlFinal) Console.ReadKey(true);
                return;
            }

            Console.WriteLine("\n{0,-8} {1,-25} {2,-15} {3,-12} {4,-8}",
                "Código", "Descripción", "Categoría", "Precio", "Stock");
            Console.WriteLine(new string('-', 76));

            decimal valorTotalAlmacen = 0;

            foreach (Repuesto r in inventario.OrderBy(rep => rep.Codigo))
            {
                string alertaStock = r.Stock < STOCK_MINIMO_CRITICO ? " [STOCK CRÍTICO]" : "";
                Console.WriteLine("{0,-8} {1,-25} {2,-15} {3,-12} {4,-8}{5}",
                    r.Codigo, r.Nombre, r.Categoria, $"S/ {r.Precio:N2}", r.Stock, alertaStock);

                valorTotalAlmacen += r.Precio * r.Stock;
            }

            Console.WriteLine(new string('-', 76));
            Console.WriteLine($"Total de ítems registrados : {inventario.Count}");
            Console.WriteLine($"Valorización del almacén   : S/ {valorTotalAlmacen:N2}");

            if (pausarAlFinal) Console.ReadKey(true);
        }

        //  OPCIÓN 3: BUSCAR REPUESTO POR CÓDIGO
        public void BuscarRepuesto()
        {
            Console.WriteLine("\n--- BUSCAR REPUESTO POR CÓDIGO ---");
            if (!Validador.HayRepuestosRegistrados(inventario)) return;

            Console.WriteLine("(Presione ESC para volver al menú)\n");
            Console.Write("Ingrese el código exacto de la pieza (ej: F123): ");
            if (!Validador.TryLeerCodigoConTeclado(out string codigoBusqueda)) { Validador.CancelarOperacion(); return; }

            Repuesto r = inventario.FirstOrDefault(x => x.Codigo == codigoBusqueda);

            if (r != null)
            {
                Console.WriteLine("\nREPUESTO ENCONTRADO:");
                Console.WriteLine($"Código    : {r.Codigo}");
                Console.WriteLine($"Nombre    : {r.Nombre}");
                Console.WriteLine($"Categoría : {r.Categoria}");
                Console.WriteLine($"Precio    : S/ {r.Precio:N2}");
                Console.WriteLine($"Stock     : {r.Stock} unidades disponibles");
                if (r.Stock < STOCK_MINIMO_CRITICO)
                {
                    Console.WriteLine("ALERTA    : Stock por debajo del mínimo permitido (< 3 unidades).");
                }
            }
            else
            {
                Console.WriteLine($"\nNo se encontró ningún repuesto con el código '{codigoBusqueda}'.");
            }

            Console.ReadKey(true);
        }

        //  OPCIÓN 4: REGISTRAR ENTRADA DE REPUESTOS (COMPRAS)
        public void EntradaStock()
        {
            Console.WriteLine("\n--- ENTRADA DE REPUESTOS (COMPRAS) ---");
            if (!Validador.HayRepuestosRegistrados(inventario)) return;

            ListarInventario(false);
            Console.WriteLine("\n(Presione ESC en cualquier momento para cancelar y volver al menú)");

            if (!Validador.TrySeleccionarRepuestoExistente(inventario, out Repuesto r)) { Validador.CancelarOperacion(); return; }
            if (!Validador.TryLeerCantidadConConfirmacion($"\nCantidad comprada de \"{r.Nombre}\" que ingresa: ", out int cantidad)) { Validador.CancelarOperacion(); return; }

            r.Stock += cantidad;
            persistencia.GuardarInventario(inventario);
            RegistrarMovimiento(r, TipoMovimiento.Entrada, cantidad, "Compra / Reposición");

            Console.WriteLine($"\nEntrada registrada. Nuevo stock de \"{r.Nombre}\": {r.Stock} unidades.");
            Console.ReadKey(true);
        }

        //  OPCIÓN 5: REGISTRAR SALIDA DE REPUESTOS (MERMA / USO)
        public void SalidaStock()
        {
            Console.WriteLine("\n--- SALIDA DE REPUESTOS (MERMA / USO INTERNO) ---");
            if (!Validador.HayRepuestosRegistrados(inventario)) return;

            ListarInventario(false);
            Console.WriteLine("\n(Presione ESC en cualquier momento para cancelar y volver al menú)");

            if (!Validador.TrySeleccionarRepuestoExistente(inventario, out Repuesto r)) { Validador.CancelarOperacion(); return; }

            if (r.Stock == 0)
            {
                Console.WriteLine($"\nEl repuesto \"{r.Nombre}\" tiene stock 0. No se pueden registrar salidas.");
                Console.ReadKey(true);
                return;
            }

            int cantidad;
            while (true)
            {
                if (!Validador.TryLeerCantidadConConfirmacion($"\nCantidad de \"{r.Nombre}\" que sale (merma o uso): ", out cantidad))
                {
                    Validador.CancelarOperacion();
                    return;
                }
                if (cantidad > r.Stock)
                {
                    Console.WriteLine($"Stock insuficiente. Disponible: {r.Stock} unidades.");
                    continue;
                }
                break;
            }

            if (!Validador.TryLeerTextoConTeclado("Motivo (Ej: Merma, Soldadura interna, Garantía): ", "Debe indicar un motivo.", false, out string motivo))
            {
                Validador.CancelarOperacion();
                return;
            }

            r.Stock -= cantidad;
            persistencia.GuardarInventario(inventario);
            RegistrarMovimiento(r, TipoMovimiento.Salida, cantidad, $"Salida: {motivo}");

            Console.WriteLine($"\nSalida registrada. Stock restante de \"{r.Nombre}\": {r.Stock} unidades.");
            Console.ReadKey(true);
        }

        //  OPCIÓN 6: REGISTRAR DESPACHO PARA SERVICIO
        public void RegistrarDespacho()
        {
            Console.WriteLine("\n--- REGISTRAR DESPACHO PARA SERVICIO / VENTA ---");
            if (!Validador.HayRepuestosRegistrados(inventario)) return;

            ListarInventario(false);
            Console.WriteLine("\n(Agregue uno o varios repuestos al despacho | Presione ESC para cancelar)");

            List<Despacho> carrito = new List<Despacho>();

            while (true)
            {
                if (!Validador.TrySeleccionarRepuestoExistente(inventario, out Repuesto r))
                {
                    Validador.CancelarOperacion();
                    return;
                }

                int yaEnCarrito = carrito.Where(x => x.CodigoRepuesto == r.Codigo).Sum(x => x.Cantidad);
                int stockDisponible = r.Stock - yaEnCarrito;

                if (stockDisponible <= 0)
                {
                    Console.WriteLine($"\nEl repuesto \"{r.Nombre}\" ya no tiene stock disponible (Stock: {r.Stock}, en carrito: {yaEnCarrito}).");
                    if (carrito.Count > 0)
                    {
                        if (!Validador.TryLeerConfirmacion("¿Desea agregar otro repuesto diferente? (S/N): ", out bool otro))
                        {
                            Validador.CancelarOperacion();
                            return;
                        }
                        if (!otro) break;
                    }
                    continue;
                }

                int cantidad;
                while (true)
                {
                    if (!Validador.TryLeerCantidadConConfirmacion($"\nCantidad de \"{r.Nombre}\" a despachar (Disp: {stockDisponible}): ", out cantidad))
                    {
                        Validador.CancelarOperacion();
                        return;
                    }
                    if (cantidad > stockDisponible)
                    {
                        Console.WriteLine($"Stock insuficiente. Disponible para agregar: {stockDisponible} unidades.");
                        continue;
                    }
                    break;
                }

                Despacho itemExistente = carrito.FirstOrDefault(x => x.CodigoRepuesto == r.Codigo);
                if (itemExistente != null)
                {
                    itemExistente.Cantidad += cantidad;
                    itemExistente.Total = itemExistente.Cantidad * r.Precio;
                }
                else
                {
                    carrito.Add(new Despacho
                    {
                        CodigoRepuesto = r.Codigo,
                        NombreRepuesto = r.Nombre,
                        Cantidad = cantidad,
                        Total = cantidad * r.Precio,
                        Fecha = DateTime.Now
                    });
                }

                Console.WriteLine($"-> Agregado: {cantidad} x \"{r.Nombre}\" (Subtotal: S/ {cantidad * r.Precio:N2})");

                if (!Validador.TryLeerConfirmacion("\n¿Desea agregar OTRO repuesto a este mismo despacho? (S/N): ", out bool agregarMas))
                {
                    Validador.CancelarOperacion();
                    return;
                }

                if (!agregarMas) break;
            }

            Console.WriteLine("\n================= RESUMEN DEL DESPACHO =================");
            Console.WriteLine("{0,-8} {1,-25} {2,-10} {3,-12}", "Código", "Repuesto", "Cantidad", "Subtotal");
            Console.WriteLine(new string('-', 60));

            decimal totalDespacho = 0;
            foreach (Despacho item in carrito)
            {
                Console.WriteLine("{0,-8} {1,-25} {2,-10} {3,-12}",
                    item.CodigoRepuesto, item.NombreRepuesto, item.Cantidad, $"S/ {item.Total:N2}");
                totalDespacho += item.Total;
            }

            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"TOTAL A COBRAR : S/ {totalDespacho:N2}");
            Console.WriteLine("========================================================");

            if (!Validador.TryLeerConfirmacion("\n¿Confirmar y procesar todo el despacho? (S/N): ", out bool confirmarDespacho) || !confirmarDespacho)
            {
                Validador.CancelarOperacion();
                return;
            }

            DateTime fechaOperacion = DateTime.Now;
            foreach (Despacho item in carrito)
            {
                Repuesto rep = inventario.First(x => x.Codigo == item.CodigoRepuesto);
                rep.Stock -= item.Cantidad;
                item.Fecha = fechaOperacion;

                despachos.Add(item);
                persistencia.GuardarDespacho(item);
                RegistrarMovimiento(rep, TipoMovimiento.Salida, item.Cantidad, "Despacho servicio/venta");
            }

            persistencia.GuardarInventario(inventario);

            Console.WriteLine("\nDespacho múltiple procesado, facturado y descontado del stock correctamente.");
            Console.ReadKey(true);
        }

        // =========================================================
        //  OPCIÓN 7: VER REPORTE DE DESPACHOS / VENTAS
        // =========================================================
        public void ReporteDespachos()
        {
            Console.WriteLine("\n--- REPORTE DE DESPACHOS Y VENTAS ---");
            if (despachos.Count == 0)
            {
                Console.WriteLine("No hay despachos registrados todavía.");
                Console.ReadKey(true);
                return;
            }

            Console.WriteLine("\n{0,-17} {1,-8} {2,-25} {3,-10} {4,-12}",
                "Fecha", "Código", "Repuesto", "Cantidad", "Total");
            Console.WriteLine(new string('-', 76));

            decimal totalGeneral = 0;

            foreach (Despacho d in despachos.OrderBy(x => x.Fecha))
            {
                Console.WriteLine("{0,-17} {1,-8} {2,-25} {3,-10} {4,-12}",
                    d.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    d.CodigoRepuesto, d.NombreRepuesto,
                    d.Cantidad, $"S/ {d.Total:N2}");

                totalGeneral += d.Total;
            }

            Console.WriteLine(new string('-', 76));
            Console.WriteLine($"Total de despachos realizados : {despachos.Count}");
            Console.WriteLine($"Monto total facturado         : S/ {totalGeneral:N2}");
            Console.ReadKey(true);
        }

        //  OPCIÓN 8: MODIFICAR DATOS DE REPUESTO
        public void ModificarRepuesto()
        {
            Console.WriteLine("\n--- MODIFICAR DATOS DE REPUESTO ---");
            if (!Validador.HayRepuestosRegistrados(inventario)) return;

            ListarInventario(false);
            Console.WriteLine("\n(Presione ESC para cancelar | ENTER vacío conserva el valor actual)");

            if (!Validador.TrySeleccionarRepuestoExistente(inventario, out Repuesto r)) { Validador.CancelarOperacion(); return; }

            Console.WriteLine($"\nEditando \"{r.Nombre}\" ({r.Codigo}):\n");

            if (!Validador.TryLeerTextoConTeclado($"Descripción actual [{r.Nombre}]: ", "", true, out string nuevoNombre)) { Validador.CancelarOperacion(); return; }
            if (!Validador.TryLeerTextoConTeclado($"Categoría actual [{r.Categoria}]: ", "", true, out string nuevaCategoria)) { Validador.CancelarOperacion(); return; }
            if (!Validador.TryLeerDecimalConTeclado($"Precio actual [S/ {r.Precio:N2}]: ", true, out decimal? nuevoPrecio)) { Validador.CancelarOperacion(); return; }

            if (!string.IsNullOrWhiteSpace(nuevoNombre)) r.Nombre = nuevoNombre;
            if (!string.IsNullOrWhiteSpace(nuevaCategoria)) r.Categoria = nuevaCategoria;
            if (nuevoPrecio.HasValue) r.Precio = nuevoPrecio.Value;

            persistencia.GuardarInventario(inventario);
            Console.WriteLine($"\nStock actual: {r.Stock} (use opciones 4, 5 o 6 para alterar stock).");
            Console.WriteLine("Datos del repuesto actualizados y guardados correctamente.");
            Console.ReadKey(true);
        }

        //  OPCIÓN 9: ELIMINAR REPUESTO
        public void EliminarRepuesto()
        {
            Console.WriteLine("\n--- ELIMINAR REPUESTO DEL CATÁLOGO ---");
            if (!Validador.HayRepuestosRegistrados(inventario)) return;

            ListarInventario(false);
            Console.WriteLine("\n(Presione ESC en cualquier momento para cancelar y volver al menú)");

            if (!Validador.TrySeleccionarRepuestoExistente(inventario, out Repuesto r)) { Validador.CancelarOperacion(); return; }

            if (!Validador.TryLeerConfirmacion($"\n¿Confirma eliminar \"{r.Nombre}\" ({r.Codigo})? (S/N): ", out bool confirmado))
            {
                Validador.CancelarOperacion();
                return;
            }

            if (confirmado)
            {
                int stockRestante = r.Stock;
                r.Stock = 0;
                if (stockRestante > 0)
                {
                    RegistrarMovimiento(r, TipoMovimiento.Salida, stockRestante, "Baja / Eliminación del catálogo");
                }

                inventario.Remove(r);
                persistencia.GuardarInventario(inventario);
                Console.WriteLine("\nRepuesto eliminado y archivo actualizado.");
            }
            else
            {
                Console.WriteLine("\nEliminación no confirmada. No se realizaron cambios.");
            }

            Console.ReadKey(true);
        }

        //  OPCIÓN 10: HISTORIAL DE MOVIMIENTOS (KARDEX)
        public void HistorialMovimientos()
        {
            Console.WriteLine("\n--- HISTORIAL DE MOVIMIENTOS (KARDEX) ---");
            if (movimientos.Count == 0)
            {
                Console.WriteLine("No existen movimientos registrados en el historial.");
                Console.ReadKey(true);
                return;
            }

            Console.WriteLine("\n{0,-17} {1,-8} {2,-22} {3,-9} {4,-9} {5,-12} {6}",
                "Fecha", "Código", "Repuesto", "Tipo", "Cantidad", "Stock Final", "Motivo");
            Console.WriteLine(new string('-', 98));

            foreach (MovimientoStock m in movimientos.OrderBy(x => x.Fecha))
            {
                string tipoTexto = m.Tipo == TipoMovimiento.Entrada ? "Entrada" : "Salida";
                Console.WriteLine("{0,-17} {1,-8} {2,-22} {3,-9} {4,-9} {5,-12} {6}",
                    m.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    m.CodigoRepuesto, m.NombreRepuesto,
                    tipoTexto, m.Cantidad, m.StockResultante, m.Motivo);
            }

            Console.WriteLine(new string('-', 98));
            int totalEntradas = movimientos.Where(m => m.Tipo == TipoMovimiento.Entrada).Sum(m => m.Cantidad);
            int totalSalidas = movimientos.Where(m => m.Tipo == TipoMovimiento.Salida).Sum(m => m.Cantidad);

            Console.WriteLine($"Total unidades ingresadas : {totalEntradas}");
            Console.WriteLine($"Total unidades retiradas  : {totalSalidas}");
            Console.ReadKey(true);
        }

        //  OPCIÓN 11: EXPORTAR REPORTE INTEGRAL A EXCEL (.XLSX)
        public void ExportarReporteExcel()
        {
            Console.WriteLine("\n--- EXPORTAR REPORTE INTEGRAL A EXCEL ---");
            exportar.GenerarReporteExcel(inventario, despachos, movimientos, STOCK_MINIMO_CRITICO);
            Console.ReadKey(true);
        }

        //  MÉTODO INTERNO DE COORDINACIÓN KARDEX
        private void RegistrarMovimiento(Repuesto r, TipoMovimiento tipo, int cantidad, string motivo)
        {
            MovimientoStock mov = new MovimientoStock
            {
                CodigoRepuesto = r.Codigo,
                NombreRepuesto = r.Nombre,
                Tipo = tipo,
                Cantidad = cantidad,
                StockResultante = r.Stock,
                Motivo = motivo,
                Fecha = DateTime.Now
            };

            movimientos.Add(mov);
            persistencia.GuardarMovimientoKardex(mov);
        }
    }
}