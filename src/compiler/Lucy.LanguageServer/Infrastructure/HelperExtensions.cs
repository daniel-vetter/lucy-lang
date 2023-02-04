using Lucy.App.LanguageServer.Models;
using Lucy.Core.ProjectManagement;

namespace Lucy.App.LanguageServer.Infrastructure;

public static class HelperExtensions
{
    public static Position2D ToPosition2D(this RpcPosition position) => new(position.Line, position.Character);

    public static RpcPosition ToRpcPosition(this Position2D position) => new() { Line = position.Line, Character = position.Character };
    public static RpcRange ToRpcRange(this Range2D range) => new() { Start = range.Start.ToRpcPosition(), End = range.End.ToRpcPosition() };
}