namespace OficinaMecanica.API
{
    public enum ScalarTheme
    {
        Light,
        Dark
    }

    public class ScalarHttpClientOptions
    {
        public string? BaseUrl { get; set; }
    }

    public class ScalarOptions
    {
        public string? Title { get; set; }
        public ScalarTheme Theme { get; set; } = ScalarTheme.Light;
        public ScalarHttpClientOptions DefaultHttpClient { get; set; } = new ScalarHttpClientOptions();
    }

    public static class ScalarExtensions
    {
        // Extensão mínima para evitar CS1061 enquanto a implementação real (biblioteca) não for adicionada.
        public static WebApplication UseScalar(this WebApplication app, Action<ScalarOptions>? configure = null)
        {
            var options = new ScalarOptions();
            configure?.Invoke(options);

            // Não adiciona middleware real — serve apenas para permitir compilação.
            // Substitua pela implementação oficial da biblioteca Scalar UI quando disponível.

            return app;
        }
    }

}
