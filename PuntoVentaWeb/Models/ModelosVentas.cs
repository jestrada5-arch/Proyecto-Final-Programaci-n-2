using System.ComponentModel.DataAnnotations;

namespace PuntoVentaWeb.Models
{
    // Una línea en el carrito que el cajero está armando
    public class VentaDetalleEntradaDto
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public int Existencia { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; } = 1;

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    // Cuerpo que se envía a la API para registrar la venta
    public class VentaCrearDto
    {
        public int IdCliente { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime FechaFactura { get; set; } = DateTime.Today;
        public List<VentaDetalleEntradaDto> Detalles { get; set; } = new();
    }

    public class VentaDetalleDto
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

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
