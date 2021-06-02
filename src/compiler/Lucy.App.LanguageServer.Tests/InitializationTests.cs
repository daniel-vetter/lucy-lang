using FluentAssertions;
using Lucy.Feature.LanguageServer.Models;
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

            result.ServerInfo.Name.Should().Be("Lucy language server");
            result.ServerInfo.Version.Should().NotBeNull();
            result.Capabilities.HoverProvider.Should().BeTrue();
            result.Capabilities.TextDocumentSync.Should().NotBeNull();
            result.Capabilities.TextDocumentSync!.OpenClose.Should().BeTrue();
            result.Capabilities.TextDocumentSync!.Change.Should().Be(RpcTextDocumentSyncKind.Incremental);
        }
    }
}
