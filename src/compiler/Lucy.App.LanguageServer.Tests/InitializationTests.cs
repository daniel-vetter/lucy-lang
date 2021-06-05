using Lucy.Feature.LanguageServer.Models;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Lucy.App.LanguageServer.Tests
{
    public class InitializationTests
    {
        [Fact]
        public async Task Should_initialize_correctly()
        {
            var server = new LanguageServer();
            var result = await server.Initialze();

            result.ServerInfo.Name.ShouldBe("Lucy language server");
            result.ServerInfo.Version.ShouldNotBeNull();
            result.Capabilities.HoverProvider.ShouldBe(true);
            result.Capabilities.TextDocumentSync.ShouldNotBeNull();
            result.Capabilities.TextDocumentSync!.OpenClose.ShouldBe(true);
            result.Capabilities.TextDocumentSync!.Change.ShouldBe(RpcTextDocumentSyncKind.Incremental);
        }
    }

    public class DocumentSyncTests
    {
        [Fact]
        public async Task Test()
        {
            var server = new LanguageServer();
            await server.Initialze();

        }
    }
}
