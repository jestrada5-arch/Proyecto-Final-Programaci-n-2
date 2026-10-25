using System.ComponentModel.DataAnnotations;

namespace PuntoVentaWeb.Models
{
    // Una línea de la compra que se está armando en pantalla
    public class CompraDetalleEntradaDto
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string? Marca { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; } = 1;

        [Range(0.01, 99999.99, ErrorMessage = "El costo debe ser mayor a cero.")]
        public decimal PrecioCostoUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioCostoUnitario;
    }

    // Cuerpo que se envía a la API para registrar la compra
    public class CompraCrearDto
    {
        public int IdProveedor { get; set; }
        public DateTime FechaOrden { get; set; } = DateTime.Today;
        public List<CompraDetalleEntradaDto> Detalles { get; set; } = new();
    }

    public class CompraDetalleDto
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioCostoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CompraDto
    {
        public int IdCompra { get; set; }
        public int NoOrdenCompra { get; set; }
        public int IdProveedor { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public string? ProveedorNit { get; set; }
        public string? ProveedorDireccion { get; set; }
        public string? ProveedorTelefono { get; set; }
        public DateTime FechaOrden { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string? UsuarioRegistro { get; set; }
        public List<CompraDetalleDto> Detalles { get; set; } = new();
        public decimal Total { get; set; }
    }

    public class CompraListaDto
    {
        public int IdCompra { get; set; }
        public int NoOrdenCompra { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public DateTime FechaOrden { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string? UsuarioRegistro { get; set; }
        public int CantidadLineas { get; set; }
        public decimal Total { get; set; }
    }
}
