using PuntoVentaApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PuntoVentaApi.Services
{
    // Genera el PDF de una orden de compra ya registrada (encabezado + detalle + total)
    public class ComprobanteCompraDocumento : IDocument
    {
        private readonly Compra _compra;

        public ComprobanteCompraDocumento(Compra compra)
        {
            _compra = compra;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Element(ComponerEncabezado);
                page.Content().PaddingTop(15).Element(ComponerContenido);

                page.Footer().AlignCenter().Text(texto =>
                {
                    texto.Span("Comprobante generado el ").FontSize(8).FontColor(Colors.Grey.Medium);
                    texto.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void ComponerEncabezado(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem(2).Column(col =>
                {
                    col.Item().Text(DatosNegocio.Nombre).FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text(DatosNegocio.Direccion).FontSize(9);
                    col.Item().Text($"NIT: {DatosNegocio.Nit}").FontSize(9);
                });

                row.RelativeItem(1).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(col =>
                {
                    col.Item().AlignCenter().Text("ORDEN DE COMPRA").Bold().FontSize(11);

                    col.Item().PaddingTop(4).Row(r =>
                    {
                        r.RelativeItem().Text("No. Orden:").SemiBold().FontSize(9);
                        r.RelativeItem().AlignRight().Text(_compra.NoOrdenCompra.ToString()).FontSize(9);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Fecha:").SemiBold().FontSize(9);
                        r.RelativeItem().AlignRight().Text(_compra.FechaOrden.ToString("dd/MM/yyyy")).FontSize(9);
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Registro:").SemiBold().FontSize(9);
                        r.RelativeItem().AlignRight().Text(_compra.FechaIngreso.ToString("dd/MM/yyyy HH:mm")).FontSize(9);
                    });
                });
            });
        }

        private void ComponerContenido(IContainer container)
        {
            var total = _compra.Detalles.Sum(d => d.Cantidad * d.PrecioCostoUnitario);

            container.Column(col =>
            {
                col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(datos =>
                {
                    datos.Item().Text("DATOS DEL PROVEEDOR").FontSize(8).FontColor(Colors.Grey.Medium).Bold();
                    datos.Item().PaddingTop(2).Text(_compra.Proveedor?.NombreProveedor ?? string.Empty).Bold().FontSize(11);

                    if (!string.IsNullOrWhiteSpace(_compra.Proveedor?.Nit))
                    {
                        datos.Item().Text($"NIT: {_compra.Proveedor!.Nit}").FontSize(9);
                    }
                    if (!string.IsNullOrWhiteSpace(_compra.Proveedor?.Direccion))
                    {
                        datos.Item().Text($"Dirección: {_compra.Proveedor!.Direccion}").FontSize(9);
                    }
                    if (!string.IsNullOrWhiteSpace(_compra.Proveedor?.Telefono))
                    {
                        datos.Item().Text($"Teléfono: {_compra.Proveedor!.Telefono}").FontSize(9);
                    }

                    datos.Item().PaddingTop(4).Text($"Recibido por: {_compra.Usuario?.NombreUsuario ?? "—"}").FontSize(9);
                });

                col.Item().PaddingTop(15).Table(tabla =>
                {
                    tabla.ColumnsDefinition(columnas =>
                    {
                        columnas.ConstantColumn(30);
                        columnas.RelativeColumn(4);
                        columnas.RelativeColumn(1);
                        columnas.RelativeColumn(2);
                        columnas.RelativeColumn(2);
                    });

                    tabla.Header(encabezado =>
                    {
                        encabezado.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Pos").Bold().FontSize(9);
                        encabezado.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Descripción").Bold().FontSize(9);
                        encabezado.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text("Cant.").Bold().FontSize(9);
                        encabezado.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Costo unit.").Bold().FontSize(9);
                        encabezado.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Importe").Bold().FontSize(9);
                    });

                    var pos = 1;
                    foreach (var linea in _compra.Detalles)
                    {
                        var fondo = pos % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;

                        tabla.Cell().Background(fondo).Padding(5).Text(pos.ToString()).FontSize(9);
                        tabla.Cell().Background(fondo).Padding(5).Text(linea.Producto?.NombreProducto ?? "(producto eliminado)").FontSize(9);
                        tabla.Cell().Background(fondo).Padding(5).AlignCenter().Text(linea.Cantidad.ToString()).FontSize(9);
                        tabla.Cell().Background(fondo).Padding(5).AlignRight().Text($"Q {linea.PrecioCostoUnitario:0.00}").FontSize(9);
                        tabla.Cell().Background(fondo).Padding(5).AlignRight().Text($"Q {(linea.Cantidad * linea.PrecioCostoUnitario):0.00}").FontSize(9);

                        pos++;
                    }
                });

                col.Item().PaddingTop(10).AlignRight().Element(caja =>
                {
                    caja.Width(180).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL").Bold().FontSize(12);
                        r.RelativeItem().AlignRight().Text($"Q {total:0.00}").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                    });
                });
            });
        }
    }
}
