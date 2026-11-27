using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using InventarioTallerMecanico.Modelos;

namespace InventarioTallerMecanico.Persistencias
{
    public class Persistencia
    {
        private const string ARCHIVO_INVENTARIO = "inventario_taller.txt";
        private const string ARCHIVO_DESPACHOS = "despachos_taller.txt";
        private const string ARCHIVO_KARDEX = "kardex_movimientos.log";

        public void GuardarInventario(List<Repuesto> inventario)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(ARCHIVO_INVENTARIO, false))
                {
                    foreach (Repuesto r in inventario)
                    {
                        sw.WriteLine($"{r.Codigo}|{r.Nombre}|{r.Categoria}|{r.Precio.ToString(CultureInfo.InvariantCulture)}|{r.Stock}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error de archivo] No se pudo guardar el inventario: {ex.Message}");
            }
        }

        public List<Repuesto> CargarInventario()
        {
            List<Repuesto> lista = new List<Repuesto>();
            if (!File.Exists(ARCHIVO_INVENTARIO)) return lista;

            try
            {
                using (StreamReader sr = new StreamReader(ARCHIVO_INVENTARIO))
                {
                    string linea;
                    while ((linea = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(linea)) continue;
                        string[] datos = linea.Split('|');
                        if (datos.Length == 5)
                        {
                            lista.Add(new Repuesto
                            {
                                Codigo = datos[0],
                                Nombre = datos[1],
                                Categoria = datos[2],
                                Precio = decimal.Parse(datos[3], CultureInfo.InvariantCulture),
                                Stock = int.Parse(datos[4])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error de lectura] No se pudo cargar el inventario: {ex.Message}");
            }
            return lista;
        }

        public void GuardarDespacho(Despacho d)
        {
            try
            {
                string registro = $"{d.Fecha:yyyy-MM-dd HH:mm}|{d.CodigoRepuesto}|{d.NombreRepuesto}|{d.Cantidad}|{d.Total.ToString(CultureInfo.InvariantCulture)}\n";
                File.AppendAllText(ARCHIVO_DESPACHOS, registro);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error de archivo] No se pudo guardar el despacho: {ex.Message}");
            }
        }

        public List<Despacho> CargarDespachos()
        {
            List<Despacho> lista = new List<Despacho>();
            if (!File.Exists(ARCHIVO_DESPACHOS)) return lista;

            try
            {
                foreach (string linea in File.ReadLines(ARCHIVO_DESPACHOS))
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    string[] datos = linea.Split('|');
                    if (datos.Length == 5)
                    {
                        lista.Add(new Despacho
                        {
                            Fecha = DateTime.ParseExact(datos[0], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                            CodigoRepuesto = datos[1],
                            NombreRepuesto = datos[2],
                            Cantidad = int.Parse(datos[3]),
                            Total = decimal.Parse(datos[4], CultureInfo.InvariantCulture)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error de lectura] No se pudo cargar el historial de despachos: {ex.Message}");
            }
            return lista;
        }

        public void GuardarMovimientoKardex(MovimientoStock mov)
        {
            try
            {
                string lineaLog = $"{mov.Fecha:yyyy-MM-dd HH:mm}|{mov.CodigoRepuesto}|{mov.NombreRepuesto}|{mov.Tipo}|{mov.Cantidad}|{mov.StockResultante}|{mov.Motivo}\n";
                File.AppendAllText(ARCHIVO_KARDEX, lineaLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error de log] No se pudo escribir en el Kardex: {ex.Message}");
            }
        }

        public List<MovimientoStock> CargarMovimientos()
        {
            List<MovimientoStock> lista = new List<MovimientoStock>();
            if (!File.Exists(ARCHIVO_KARDEX)) return lista;

            try
            {
                foreach (string linea in File.ReadLines(ARCHIVO_KARDEX))
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    string[] datos = linea.Split('|');
                    if (datos.Length == 7)
                    {
                        lista.Add(new MovimientoStock
                        {
                            Fecha = DateTime.ParseExact(datos[0], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                            CodigoRepuesto = datos[1],
                            NombreRepuesto = datos[2],
                            Tipo = (TipoMovimiento)Enum.Parse(typeof(TipoMovimiento), datos[3]),
                            Cantidad = int.Parse(datos[4]),
                            StockResultante = int.Parse(datos[5]),
                            Motivo = datos[6]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error de lectura] No se pudo cargar el archivo Kardex: {ex.Message}");
            }
            return lista;
        }
    }
}