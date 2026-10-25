namespace PuntoVentaApi.Models
{
    public static class RolesSistema
    {
        public const string Administrador = "Administrador";
        public const string Gerente = "Gerente";
        public const string Bodeguero = "Bodeguero";
        public const string Cajero = "Cajero";

        public const string AdministradorGerente = Administrador + "," + Gerente;
        public const string Todos = Administrador + "," + Gerente + "," + Bodeguero + "," + Cajero;
        public const string Compras = Administrador + "," + Gerente + "," + Bodeguero;
        public const string Ventas = Administrador + "," + Gerente + "," + Cajero;
        public const string AdministradorGerenteCajero = Administrador + "," + Gerente + "," + Cajero;

        public static bool EsValido(string? rol) =>
            rol is Administrador or Gerente or Bodeguero or Cajero;
    }
}
