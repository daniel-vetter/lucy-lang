using Lucy.Core.ProjectManagement;
using Lucy.Feature.LanguageServer.Models;

namespace Lucy.App.LanguageServer.Infrastructure
{
    public static class HelperExtensions
    {
        public static Position2D ToPosition2D(this RpcPosition position) => new Position2D(position.Line, position.Character);

        public static RpcPosition ToRpcPosition(this Position2D position) => new RpcPosition { Line = position.Line, Character = position.Character };
        public static RpcRange ToRpcRange(this Range2D range) => new RpcRange { Start = range.Start.ToRpcPosition(), End = range.End.ToRpcPosition() };
    }
}
