using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using InventarioTallerMecanico.Modelos;

namespace InventarioTallerMecanico.Persistencias
{
    public class Exportar
    {
        private const string NOMBRE_ARCHIVO = "Reporte_TallerMecanico.xlsx";

        public void GenerarReporteExcel(
            List<Repuesto> inventario,
            List<Despacho> despachos,
            List<MovimientoStock> movimientos,
            int stockMinimoCritico)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    // HOJA 1: INVENTARIO ACTUAL
                    var wsInv = workbook.Worksheets.Add("Inventario");
                    wsInv.Cell(1, 1).Value = "Código";
                    wsInv.Cell(1, 2).Value = "Descripción";
                    wsInv.Cell(1, 3).Value = "Categoría";
                    wsInv.Cell(1, 4).Value = "Precio Unitario (S/)";
                    wsInv.Cell(1, 5).Value = "Stock Actual";
                    wsInv.Cell(1, 6).Value = "Valorización (S/)";
                    wsInv.Cell(1, 7).Value = "Estado";

                    FormatearEncabezadoExcel(wsInv.Range("A1:G1"));

                    int filaInv = 2;
                    foreach (var r in inventario.OrderBy(x => x.Codigo))
                    {
                        wsInv.Cell(filaInv, 1).Value = r.Codigo;
                        wsInv.Cell(filaInv, 2).Value = r.Nombre;
                        wsInv.Cell(filaInv, 3).Value = r.Categoria;
                        wsInv.Cell(filaInv, 4).Value = r.Precio;
                        wsInv.Cell(filaInv, 4).Style.NumberFormat.Format = "#,##0.00";
                        wsInv.Cell(filaInv, 5).Value = r.Stock;
                        wsInv.Cell(filaInv, 6).FormulaA1 = $"=D{filaInv}*E{filaInv}";
                        wsInv.Cell(filaInv, 6).Style.NumberFormat.Format = "#,##0.00";
                        wsInv.Cell(filaInv, 7).Value = r.Stock < stockMinimoCritico ? "STOCK CRÍTICO" : "Normal";
                        filaInv++;
                    }

                    if (inventario.Count > 0)
                    {
                        wsInv.Cell(filaInv, 5).Value = "TOTAL ALMACÉN:";
                        wsInv.Cell(filaInv, 5).Style.Font.Bold = true;
                        wsInv.Cell(filaInv, 6).FormulaA1 = $"=SUM(F2:F{filaInv - 1})";
                        wsInv.Cell(filaInv, 6).Style.Font.Bold = true;
                        wsInv.Cell(filaInv, 6).Style.NumberFormat.Format = "#,##0.00";
                    }
                    wsInv.Columns().AdjustToContents();

                    // HOJA 2: DESPACHOS Y VENTAS
                    var wsDesp = workbook.Worksheets.Add("Despachos");
                    wsDesp.Cell(1, 1).Value = "Fecha y Hora";
                    wsDesp.Cell(1, 2).Value = "Código";
                    wsDesp.Cell(1, 3).Value = "Repuesto";
                    wsDesp.Cell(1, 4).Value = "Cantidad";
                    wsDesp.Cell(1, 5).Value = "Total Facturado (S/)";

                    FormatearEncabezadoExcel(wsDesp.Range("A1:E1"));

                    int filaDesp = 2;
                    foreach (var d in despachos.OrderBy(x => x.Fecha))
                    {
                        wsDesp.Cell(filaDesp, 1).Value = d.Fecha.ToString("dd/MM/yyyy HH:mm");
                        wsDesp.Cell(filaDesp, 2).Value = d.CodigoRepuesto;
                        wsDesp.Cell(filaDesp, 3).Value = d.NombreRepuesto;
                        wsDesp.Cell(filaDesp, 4).Value = d.Cantidad;
                        wsDesp.Cell(filaDesp, 5).Value = d.Total;
                        wsDesp.Cell(filaDesp, 5).Style.NumberFormat.Format = "#,##0.00";
                        filaDesp++;
                    }

                    if (despachos.Count > 0)
                    {
                        wsDesp.Cell(filaDesp, 4).Value = "TOTAL FACTURADO:";
                        wsDesp.Cell(filaDesp, 4).Style.Font.Bold = true;
                        wsDesp.Cell(filaDesp, 5).FormulaA1 = $"=SUM(E2:E{filaDesp - 1})";
                        wsDesp.Cell(filaDesp, 5).Style.Font.Bold = true;
                        wsDesp.Cell(filaDesp, 5).Style.NumberFormat.Format = "#,##0.00";
                    }
                    wsDesp.Columns().AdjustToContents();

                    // HOJA 3: HISTORIAL DE MOVIMIENTOS (KARDEX)
                    var wsKardex = workbook.Worksheets.Add("Kardex");
                    wsKardex.Cell(1, 1).Value = "Fecha y Hora";
                    wsKardex.Cell(1, 2).Value = "Código";
                    wsKardex.Cell(1, 3).Value = "Repuesto";
                    wsKardex.Cell(1, 4).Value = "Movimiento";
                    wsKardex.Cell(1, 5).Value = "Cantidad";
                    wsKardex.Cell(1, 6).Value = "Stock Resultante";
                    wsKardex.Cell(1, 7).Value = "Motivo";

                    FormatearEncabezadoExcel(wsKardex.Range("A1:G1"));

                    int filaKar = 2;
                    foreach (var m in movimientos.OrderBy(x => x.Fecha))
                    {
                        wsKardex.Cell(filaKar, 1).Value = m.Fecha.ToString("dd/MM/yyyy HH:mm");
                        wsKardex.Cell(filaKar, 2).Value = m.CodigoRepuesto;
                        wsKardex.Cell(filaKar, 3).Value = m.NombreRepuesto;
                        wsKardex.Cell(filaKar, 4).Value = m.Tipo == TipoMovimiento.Entrada ? "Entrada" : "Salida";
                        wsKardex.Cell(filaKar, 5).Value = m.Cantidad;
                        wsKardex.Cell(filaKar, 6).Value = m.StockResultante;
                        wsKardex.Cell(filaKar, 7).Value = m.Motivo;
                        filaKar++;
                    }
                    wsKardex.Columns().AdjustToContents();

                    // Sobrescribe automáticamente el archivo anterior si ya existía
                    workbook.SaveAs(NOMBRE_ARCHIVO);
                }

                Console.WriteLine($"\nReporte exportado exitosamente a: {Path.GetFullPath(NOMBRE_ARCHIVO)}");
                Console.WriteLine("Contiene 3 hojas: Inventario, Despachos y Kardex.");
            }
            catch (IOException)
            {
                Console.WriteLine($"\nCierre el archivo '{NOMBRE_ARCHIVO}' en Excel antes de volver a exportarlo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Error al exportar Excel] {ex.Message}");
            }
        }

        private void FormatearEncabezadoExcel(IXLRange rango)
        {
            rango.Style.Font.Bold = true;
            rango.Style.Font.FontColor = XLColor.White;
            rango.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            rango.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }
}