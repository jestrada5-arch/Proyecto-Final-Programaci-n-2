using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVentaApi.Data;
using PuntoVentaApi.Models;
using PuntoVentaApi.Services;
using QuestPDF.Fluent;

namespace PuntoVentaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        // Única serie que usa esta tienda; si más adelante necesitan varias cajas con
        // series distintas (A, B, C...) esto se puede volver un parámetro.
        private const string SerieFactura = "A";

        private readonly PuntoVentaContext _db;

        public VentasController(PuntoVentaContext db)
        {
            _db = db;
        }

        // GET: api/Ventas
        [HttpGet]
        [Authorize(Roles = RolesSistema.Ventas)]
        public async Task<ActionResult<IEnumerable<VentaListaDto>>> GetVentas()
        {
            var ventas = await _db.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.Detalles)
                .AsNoTracking()
                .OrderByDescending(v => v.FechaIngreso)
                .ToListAsync();

            return ventas.Select(v => new VentaListaDto
            {
                IdVenta = v.IdVenta,
                Serie = v.Serie,
                NoFactura = v.NoFactura,
                FechaFactura = v.FechaFactura,
                FechaIngreso = v.FechaIngreso,
                Cliente = NombreCompleto(v.Cliente?.Nombres, v.Cliente?.Apellidos),
                Empleado = NombreCompleto(v.Empleado?.Nombres, v.Empleado?.Apellidos),
                CantidadLineas = v.Detalles.Count,
                Total = v.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
            }).ToList();
        }

        // GET: api/Ventas/5
        [HttpGet("{id:int}")]
        [Authorize(Roles = RolesSistema.Ventas)]
        public async Task<ActionResult<VentaDto>> GetVenta(int id)
        {
            var venta = await ObtenerVentaCompleta(id);
            if (venta is null)
            {
                return NotFound(new { mensaje = "La venta no existe." });
            }

            return Mapear(venta);
        }

        // GET: api/Ventas/5/pdf
        [HttpGet("{id:int}/pdf")]
        [Authorize(Roles = RolesSistema.Ventas)]
        public async Task<IActionResult> GetVentaPdf(int id)
        {
            var venta = await ObtenerVentaCompleta(id);
            if (venta is null)
            {
                return NotFound(new { mensaje = "La venta no existe." });
            }

            var documento = new ComprobanteVentaDocumento(venta);
            var bytes = documento.GeneratePdf();

            return File(bytes, "application/pdf", $"factura-{venta.Serie}-{venta.NoFactura}.pdf");
        }

        // POST: api/Ventas
        // Registra una venta completa (encabezado + líneas) en una sola transacción:
        // valida existencia disponible, descuenta stock y calcula subtotales/total.
        // El precio de cada línea siempre se toma de Productos.precio_venta, nunca lo
        // manda el cliente, para que nadie pueda vender a un precio distinto al oficial.
        [HttpPost]
        [Authorize(Roles = RolesSistema.Ventas)]
        public async Task<ActionResult<VentaDto>> PostVenta(VentaCrearDto dto)
        {
            if (dto.Detalles is null || dto.Detalles.Count == 0)
            {
                return BadRequest(new { mensaje = "Debe agregar al menos un producto a la venta." });
            }

            if (dto.FechaFactura.Date > DateTime.Today)
            {
                return BadRequest(new { mensaje = "La fecha de la factura no puede ser una fecha futura." });
            }

            if (!await _db.Clientes.AnyAsync(c => c.IdCliente == dto.IdCliente))
            {
                return BadRequest(new { mensaje = "El cliente indicado no existe." });
            }

            if (!await _db.Empleados.AnyAsync(e => e.IdEmpleado == dto.IdEmpleado))
            {
                return BadRequest(new { mensaje = "El empleado indicado no existe." });
            }

            // cantidad total solicitada por producto (por si el mismo producto viene en más de una línea)
            var cantidadPorProducto = dto.Detalles
                .GroupBy(d => d.IdProducto)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

            var idsProductos = cantidadPorProducto.Keys.ToList();
            var productos = await _db.Productos.Where(p => idsProductos.Contains(p.IdProducto)).ToListAsync();

            if (productos.Count != idsProductos.Count)
            {
                return BadRequest(new { mensaje = "Uno o más productos de la venta ya no existen." });
            }

            foreach (var producto in productos)
            {
                var solicitado = cantidadPorProducto[producto.IdProducto];
                if (solicitado > producto.Existencia)
                {
                    return Conflict(new
                    {
                        mensaje = $"Existencia insuficiente de '{producto.NombreProducto}'. Disponible: {producto.Existencia}, solicitado: {solicitado}."
                    });
                }
            }

            await using var transaccion = await _db.Database.BeginTransactionAsync();
            try
            {
                var ultimoFactura = await _db.Ventas
                    .Where(v => v.Serie == SerieFactura)
                    .Select(v => (int?)v.NoFactura)
                    .MaxAsync() ?? 0;

                var venta = new Venta
                {
                    Serie = SerieFactura,
                    NoFactura = ultimoFactura + 1,
                    FechaFactura = dto.FechaFactura,
                    IdCliente = dto.IdCliente,
                    IdEmpleado = dto.IdEmpleado,
                    FechaIngreso = DateTime.Now
                };

                foreach (var linea in dto.Detalles)
                {
                    var producto = productos.First(p => p.IdProducto == linea.IdProducto);

                    venta.Detalles.Add(new VentaDetalle
                    {
                        IdProducto = linea.IdProducto,
                        Cantidad = linea.Cantidad,
                        PrecioUnitario = producto.PrecioVenta
                    });

                    producto.Existencia -= linea.Cantidad;
                }

                _db.Ventas.Add(venta);
                await _db.SaveChangesAsync();
                await transaccion.CommitAsync();

                var creada = await ObtenerVentaCompleta(venta.IdVenta);
                return CreatedAtAction(nameof(GetVenta), new { id = venta.IdVenta }, Mapear(creada!));
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/Ventas/5
        // Anula una venta (ej. devolución del cliente) y repone la existencia de cada
        // producto vendido en esa venta.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RolesSistema.AdministradorGerente)]
        public async Task<IActionResult> DeleteVenta(int id)
        {
            var venta = await _db.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.IdVenta == id);
            if (venta is null)
            {
                return NotFound(new { mensaje = "La venta no existe." });
            }

            var idsProductos = venta.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var productos = await _db.Productos.Where(p => idsProductos.Contains(p.IdProducto)).ToListAsync();

            await using var transaccion = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var detalle in venta.Detalles)
                {
                    var producto = productos.FirstOrDefault(p => p.IdProducto == detalle.IdProducto);
                    if (producto is not null)
                    {
                        producto.Existencia += detalle.Cantidad;
                    }
                }

                _db.Ventas.Remove(venta); // la cascada de la FK borra Ventas_detalle
                await _db.SaveChangesAsync();
                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }

            return NoContent();
        }

        private async Task<Venta?> ObtenerVentaCompleta(int id)
        {
            return await _db.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Empleado)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdVenta == id);
        }

        private static string NombreCompleto(string? nombres, string? apellidos)
        {
            if (string.IsNullOrWhiteSpace(nombres) && string.IsNullOrWhiteSpace(apellidos))
            {
                return string.Empty;
            }
            return $"{nombres} {apellidos}".Trim();
        }

        private static VentaDto Mapear(Venta v)
        {
            var detalles = v.Detalles.Select(d => new VentaDetalleDto
            {
                IdProducto = d.IdProducto,
                Producto = d.Producto?.NombreProducto ?? string.Empty,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            }).ToList();

            return new VentaDto
            {
                IdVenta = v.IdVenta,
                Serie = v.Serie,
                NoFactura = v.NoFactura,
                FechaFactura = v.FechaFactura,
                FechaIngreso = v.FechaIngreso,
                IdCliente = v.IdCliente,
                Cliente = NombreCompleto(v.Cliente?.Nombres, v.Cliente?.Apellidos),
                ClienteNit = v.Cliente?.Nit,
                IdEmpleado = v.IdEmpleado,
                Empleado = NombreCompleto(v.Empleado?.Nombres, v.Empleado?.Apellidos),
                Detalles = detalles,
                Total = detalles.Sum(d => d.Subtotal)
            };
        }
    }
}
