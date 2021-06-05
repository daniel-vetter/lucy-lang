using Lucy.App.LanguageServer.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Lucy.App.LanguageServer.Tests
{
    public class LanguageServer
    {
        private ServiceProvider _sp;

        public LanguageServer()
        {
            _sp = LanguageServerApp
                .CreateServiceCollection()
                .AddSingleton<IFileSystem, InMemoryFileSystem>()
                .BuildServiceProvider();
        }

        public T Get<T>() where T : class
        {
            return _sp.GetRequiredService<T>();
        }
    }

}
