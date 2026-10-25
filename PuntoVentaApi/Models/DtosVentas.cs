using System.ComponentModel.DataAnnotations;

namespace PuntoVentaApi.Models
{
    // Una línea que el cajero agrega a la venta. El precio NO lo manda el cliente:
    // siempre se toma del precio_venta del producto en el momento de vender.
    public class VentaDetalleEntradaDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto válido.")]
        public int IdProducto { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }
    }

    // Cuerpo que se envía para registrar una venta nueva (maestro-detalle)
    public class VentaCrearDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente.")]
        public int IdCliente { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un empleado.")]
        public int IdEmpleado { get; set; }

        [Required(ErrorMessage = "La fecha de factura es obligatoria.")]
        public DateTime FechaFactura { get; set; }

        [Required(ErrorMessage = "Debe agregar al menos un producto a la venta.")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un producto a la venta.")]
        public List<VentaDetalleEntradaDto> Detalles { get; set; } = new();
    }

    // Línea para mostrar el detalle de una venta ya registrada
    public class VentaDetalleDto
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    // Venta completa, con su detalle
    public class VentaDto
    {
        public int IdVenta { get; set; }
        public string Serie { get; set; } = string.Empty;
        public int NoFactura { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaIngreso { get; set; }

        public int IdCliente { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string? ClienteNit { get; set; }

        public int IdEmpleado { get; set; }
        public string Empleado { get; set; } = string.Empty;

        public List<VentaDetalleDto> Detalles { get; set; } = new();
        public decimal Total { get; set; }
    }

    // Fila resumida para el listado de ventas
    public class VentaListaDto
    {
        public int IdVenta { get; set; }
        public string Serie { get; set; } = string.Empty;
        public int NoFactura { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Empleado { get; set; } = string.Empty;
        public int CantidadLineas { get; set; }
        public decimal Total { get; set; }
    }
}
