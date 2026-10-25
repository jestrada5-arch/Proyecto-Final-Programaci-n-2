using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using PuntoVentaApi.Data;
using PuntoVentaApi.Models;
using PuntoVentaApi.Services;
using QuestPDF.Fluent;

namespace PuntoVentaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComprasController : ControllerBase
    {
        // Mismo margen que usa Productos al crear uno nuevo: al REGISTRAR una compra,
        // el precio de venta se recalcula siempre sobre el costo más reciente.
        // Editar/eliminar una compra NUNCA toca precio_costo/precio_venta (ver PutCompra/DeleteCompra):
        // solo ajustan existencia, para no pisar un precio más reciente puesto por otra compra.
        private const decimal MargenPorDefecto = 1.25m;

        private readonly PuntoVentaContext _db;

        public ComprasController(PuntoVentaContext db)
        {
            _db = db;
        }

        // GET: api/Compras
        [HttpGet]
        [Authorize(Roles = RolesSistema.Compras)]
        public async Task<ActionResult<IEnumerable<CompraListaDto>>> GetCompras()
        {
            var compras = await _db.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.Detalles)
                .AsNoTracking()
                .OrderByDescending(c => c.FechaIngreso)
                .ToListAsync();

            return compras.Select(c => new CompraListaDto
            {
                IdCompra = c.IdCompra,
                NoOrdenCompra = c.NoOrdenCompra,
                Proveedor = c.Proveedor?.NombreProveedor ?? string.Empty,
                FechaOrden = c.FechaOrden,
                FechaIngreso = c.FechaIngreso,
                UsuarioRegistro = c.Usuario?.NombreUsuario,
                CantidadLineas = c.Detalles.Count,
                Total = c.Detalles.Sum(d => d.Cantidad * d.PrecioCostoUnitario)
            }).ToList();
        }

        // GET: api/Compras/5
        [HttpGet("{id:int}")]
        [Authorize(Roles = RolesSistema.Compras)]
        public async Task<ActionResult<CompraDto>> GetCompra(int id)
        {
            var compra = await ObtenerCompraCompleta(id);
            if (compra is null)
            {
                return NotFound(new { mensaje = "La compra no existe." });
            }

            return Mapear(compra);
        }

        // GET: api/Compras/5/pdf
        [HttpGet("{id:int}/pdf")]
        [Authorize(Roles = RolesSistema.Compras)]
        public async Task<IActionResult> GetCompraPdf(int id)
        {
            var compra = await ObtenerCompraCompleta(id);
            if (compra is null)
            {
                return NotFound(new { mensaje = "La compra no existe." });
            }

            var documento = new ComprobanteCompraDocumento(compra);
            var bytes = documento.GeneratePdf();

            return File(bytes, "application/pdf", $"orden-compra-{compra.NoOrdenCompra}.pdf");
        }

        // POST: api/Compras
        // Registra una compra completa (encabezado + líneas) en una sola transacción:
        // por cada línea aumenta la existencia del producto, actualiza su costo y
        // recalcula el precio de venta con el margen por defecto.
        [HttpPost]
        [Authorize(Roles = RolesSistema.Compras)]
        public async Task<ActionResult<CompraDto>> PostCompra(CompraCrearDto dto)
        {
            if (dto.Detalles is null || dto.Detalles.Count == 0)
            {
                return BadRequest(new { mensaje = "Debe agregar al menos un producto a la compra." });
            }

            if (dto.FechaOrden.Date > DateTime.Today)
            {
                return BadRequest(new { mensaje = "La fecha de la orden no puede ser una fecha futura." });
            }

            if (!await _db.Proveedores.AnyAsync(p => p.IdProveedor == dto.IdProveedor))
            {
                return BadRequest(new { mensaje = "El proveedor indicado no existe." });
            }

            var idsProductos = dto.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var productos = await _db.Productos.Where(p => idsProductos.Contains(p.IdProducto)).ToListAsync();

            if (productos.Count != idsProductos.Count)
            {
                return BadRequest(new { mensaje = "Uno o más productos de la compra ya no existen." });
            }

            await using var transaccion = await _db.Database.BeginTransactionAsync();
            try
            {
                var ultimoOrden = await _db.Compras.Select(c => (int?)c.NoOrdenCompra).MaxAsync() ?? 0;

                var compra = new Compra
                {
                    NoOrdenCompra = ultimoOrden + 1,
                    IdProveedor = dto.IdProveedor,
                    FechaOrden = dto.FechaOrden,
                    FechaIngreso = DateTime.Now,
                    IdUsuario = ObtenerIdUsuarioActual()
                };

                foreach (var linea in dto.Detalles)
                {
                    var producto = productos.First(p => p.IdProducto == linea.IdProducto);

                    compra.Detalles.Add(new CompraDetalle
                    {
                        IdProducto = linea.IdProducto,
                        Cantidad = linea.Cantidad,
                        PrecioCostoUnitario = linea.PrecioCostoUnitario
                    });

                    producto.Existencia += linea.Cantidad;
                    producto.PrecioCosto = linea.PrecioCostoUnitario;
                    producto.PrecioVenta = Math.Round(linea.PrecioCostoUnitario * MargenPorDefecto, 2);
                }

                _db.Compras.Add(compra);
                await _db.SaveChangesAsync();
                await transaccion.CommitAsync();

                var creada = await ObtenerCompraCompleta(compra.IdCompra);
                return CreatedAtAction(nameof(GetCompra), new { id = compra.IdCompra }, Mapear(creada!));
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        // PUT: api/Compras/5
        // Reemplaza las líneas de una compra ya registrada. Solo ajusta la EXISTENCIA
        // de cada producto según la diferencia entre lo que tenía la compra antes y lo
        // que tiene ahora; nunca toca precio_costo/precio_venta. Si el ajuste dejaría a
        // algún producto con existencia negativa (porque ya se vendió), se rechaza todo.
        [HttpPut("{id:int}")]
        [Authorize(Roles = RolesSistema.AdministradorGerente)]
        public async Task<IActionResult> PutCompra(int id, CompraCrearDto dto)
        {
            if (dto.Detalles is null || dto.Detalles.Count == 0)
            {
                return BadRequest(new { mensaje = "Debe agregar al menos un producto a la compra." });
            }

            if (dto.FechaOrden.Date > DateTime.Today)
            {
                return BadRequest(new { mensaje = "La fecha de la orden no puede ser una fecha futura." });
            }

            if (!await _db.Proveedores.AnyAsync(p => p.IdProveedor == dto.IdProveedor))
            {
                return BadRequest(new { mensaje = "El proveedor indicado no existe." });
            }

            var compra = await _db.Compras.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.IdCompra == id);
            if (compra is null)
            {
                return NotFound(new { mensaje = "La compra no existe." });
            }

            var idsNuevos = dto.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var existenNuevos = await _db.Productos.CountAsync(p => idsNuevos.Contains(p.IdProducto));
            if (existenNuevos != idsNuevos.Count)
            {
                return BadRequest(new { mensaje = "Uno o más productos indicados no existen." });
            }

            // cantidad total por producto, antes y después de la edición
            var cantidadAntes = compra.Detalles.GroupBy(d => d.IdProducto).ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));
            var cantidadDespues = dto.Detalles.GroupBy(d => d.IdProducto).ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

            var idsAfectados = cantidadAntes.Keys.Union(cantidadDespues.Keys).ToList();
            var productosAfectados = await _db.Productos.Where(p => idsAfectados.Contains(p.IdProducto)).ToListAsync();

            foreach (var idProducto in idsAfectados)
            {
                var antes = cantidadAntes.GetValueOrDefault(idProducto, 0);
                var despues = cantidadDespues.GetValueOrDefault(idProducto, 0);
                var delta = despues - antes;

                var producto = productosAfectados.First(p => p.IdProducto == idProducto);
                if (producto.Existencia + delta < 0)
                {
                    return Conflict(new
                    {
                        mensaje = $"No se puede actualizar: ya se vendieron unidades de '{producto.NombreProducto}' y la existencia quedaría negativa."
                    });
                }
            }

            await using var transaccion = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var idProducto in idsAfectados)
                {
                    var antes = cantidadAntes.GetValueOrDefault(idProducto, 0);
                    var despues = cantidadDespues.GetValueOrDefault(idProducto, 0);
                    var delta = despues - antes;
                    if (delta == 0) continue;

                    var producto = productosAfectados.First(p => p.IdProducto == idProducto);
                    producto.Existencia += delta;
                }

                compra.IdProveedor = dto.IdProveedor;
                compra.FechaOrden = dto.FechaOrden;

                _db.ComprasDetalle.RemoveRange(compra.Detalles);
                compra.Detalles.Clear();

                foreach (var linea in dto.Detalles)
                {
                    compra.Detalles.Add(new CompraDetalle
                    {
                        IdProducto = linea.IdProducto,
                        Cantidad = linea.Cantidad,
                        PrecioCostoUnitario = linea.PrecioCostoUnitario
                    });
                }

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

        // DELETE: api/Compras/5
        // Elimina la compra y le resta a cada producto la cantidad que esa compra había
        // sumado. Si eso dejaría a algún producto con existencia negativa (porque ya se
        // vendió), se rechaza. Nunca toca precio_costo/precio_venta.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RolesSistema.AdministradorGerente)]
        public async Task<IActionResult> DeleteCompra(int id)
        {
            var compra = await _db.Compras.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.IdCompra == id);
            if (compra is null)
            {
                return NotFound(new { mensaje = "La compra no existe." });
            }

            var cantidadPorProducto = compra.Detalles.GroupBy(d => d.IdProducto).ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));
            var idsProductos = cantidadPorProducto.Keys.ToList();
            var productos = await _db.Productos.Where(p => idsProductos.Contains(p.IdProducto)).ToListAsync();

            foreach (var idProducto in idsProductos)
            {
                var producto = productos.FirstOrDefault(p => p.IdProducto == idProducto);
                if (producto is not null && producto.Existencia - cantidadPorProducto[idProducto] < 0)
                {
                    return Conflict(new
                    {
                        mensaje = $"No se puede eliminar: ya se vendieron unidades de '{producto.NombreProducto}' y la existencia quedaría negativa."
                    });
                }
            }

            await using var transaccion = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var idProducto in idsProductos)
                {
                    var producto = productos.FirstOrDefault(p => p.IdProducto == idProducto);
                    if (producto is not null)
                    {
                        producto.Existencia -= cantidadPorProducto[idProducto];
                    }
                }

                _db.Compras.Remove(compra); // la cascada de la FK borra Compras_detalle
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

        private async Task<Compra?> ObtenerCompraCompleta(int id)
        {
            return await _db.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.Detalles).ThenInclude(d => d.Producto)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCompra == id);
        }

        // Toma el id del usuario autenticado desde el claim "sub" del JWT (ver TokenService.Crear)
        private int? ObtenerIdUsuarioActual()
        {
            var valor = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }

        private static CompraDto Mapear(Compra c)
        {
            var detalles = c.Detalles.Select(d => new CompraDetalleDto
            {
                IdProducto = d.IdProducto,
                Producto = d.Producto?.NombreProducto ?? string.Empty,
                Cantidad = d.Cantidad,
                PrecioCostoUnitario = d.PrecioCostoUnitario,
                Subtotal = d.Cantidad * d.PrecioCostoUnitario
            }).ToList();

            return new CompraDto
            {
                IdCompra = c.IdCompra,
                NoOrdenCompra = c.NoOrdenCompra,
                IdProveedor = c.IdProveedor,
                Proveedor = c.Proveedor?.NombreProveedor ?? string.Empty,
                ProveedorNit = c.Proveedor?.Nit,
                ProveedorDireccion = c.Proveedor?.Direccion,
                ProveedorTelefono = c.Proveedor?.Telefono,
                FechaOrden = c.FechaOrden,
                FechaIngreso = c.FechaIngreso,
                UsuarioRegistro = c.Usuario?.NombreUsuario,
                Detalles = detalles,
                Total = detalles.Sum(d => d.Subtotal)
            };
        }
    }
}
