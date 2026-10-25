using System.ComponentModel.DataAnnotations;

namespace PuntoVentaApi.Models
{
    // Una línea que el cliente envía al registrar una compra
    public class CompraDetalleEntradaDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto válido.")]
        public int IdProducto { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }

        [Range(0.01, 99999.99, ErrorMessage = "El costo unitario debe ser mayor a cero.")]
        public decimal PrecioCostoUnitario { get; set; }
    }

    // Cuerpo que se envía para registrar una compra nueva (maestro-detalle)
    public class CompraCrearDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un proveedor.")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "La fecha de orden es obligatoria.")]
        public DateTime FechaOrden { get; set; }

        [Required(ErrorMessage = "Debe agregar al menos un producto a la compra.")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un producto a la compra.")]
        public List<CompraDetalleEntradaDto> Detalles { get; set; } = new();
    }

    // Línea para mostrar el detalle de una compra ya registrada
    public class CompraDetalleDto
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioCostoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    // Compra completa, con su detalle (para GET por id y como respuesta del POST)
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

    // Fila resumida para el listado de compras
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
