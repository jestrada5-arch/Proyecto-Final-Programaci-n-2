using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using PuntoVentaWeb.Models;

namespace PuntoVentaWeb.Services
{
    // Guarda el token JWT mientras el usuario tiene la sesión abierta.
    // El token se conserva en el almacenamiento de sesión del navegador (cifrado), así sobrevive a un F5.
    public class SesionService
    {
        private const string Clave = "pv_sesion";
        private readonly ProtectedSessionStorage _almacen;

        public SesionService(ProtectedSessionStorage almacen)
        {
            _almacen = almacen;
        }

        public string? Token { get; private set; }
        public string? Usuario { get; private set; }
        public string? Rol { get; private set; }
        public DateTime Expira { get; private set; }

        // true cuando ya se intentó leer la sesión guardada en el navegador
        public bool Restaurada { get; private set; }

        public bool Iniciada => !string.IsNullOrEmpty(Token) &&
            !string.IsNullOrWhiteSpace(Rol) &&
            Expira > DateTime.UtcNow;

        public bool PuedeAdministrar => Rol is "Administrador" or "Gerente";
        public bool EsBodeguero => Rol == "Bodeguero";
        public bool EsCajero => Rol == "Cajero";

        public event Action? Cambio;

        public async Task RestaurarAsync()
        {
            if (Restaurada)
            {
                return;
            }

            try
            {
                var guardada = await _almacen.GetAsync<LoginRespuesta>(Clave);
                if (guardada.Success && guardada.Value is not null && guardada.Value.Expira > DateTime.UtcNow)
                {
                    Token = guardada.Value.Token;
                    Usuario = guardada.Value.Usuario;
                    Rol = guardada.Value.Rol;
                    Expira = guardada.Value.Expira;
                }
            }
            catch (Exception)
            {
                // si no se puede leer la sesión guardada, se pide iniciar sesión de nuevo
            }

            Restaurada = true;
            Cambio?.Invoke();
        }

        public async Task IniciarAsync(LoginRespuesta respuesta)
        {
            Token = respuesta.Token;
            Usuario = respuesta.Usuario;
            Rol = respuesta.Rol;
            Expira = respuesta.Expira;
            Restaurada = true;

            try
            {
                await _almacen.SetAsync(Clave, respuesta);
            }
            catch (Exception)
            {
                // la sesión sigue activa en memoria aunque no se pueda guardar en el navegador
            }

            Cambio?.Invoke();
        }

        public async Task CerrarAsync()
        {
            Token = null;
            Usuario = null;
            Rol = null;
            Expira = default;

            try
            {
                await _almacen.DeleteAsync(Clave);
            }
            catch (Exception)
            {
                // nada que borrar
            }

            Cambio?.Invoke();
        }
    }
}
