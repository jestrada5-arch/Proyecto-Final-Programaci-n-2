namespace PuntoVentaApi.Services
{
    public sealed class ProductImageStorage(string rootPath)
    {
        public const string RequestPath = "/images/productos";

        public string RootPath { get; } = Path.GetFullPath(rootPath);
    }
}
