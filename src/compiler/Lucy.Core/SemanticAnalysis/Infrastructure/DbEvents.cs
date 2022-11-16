using System;

namespace Lucy.Core.SemanticAnalysis.Infrastructure;

public enum ResultType
{
    InitialCalculation,
    WasTheSame,
    HasChanged
}
public interface IDbEvent { }
public record InputWasChanged(IQuery Query, object Value) : IDbEvent;
public record InputWasRemoved(IQuery Query) : IDbEvent;
public record CalculationStarted(IQuery Query) : IDbEvent;
public record CalculationFinished(IQuery Query, object Result, TimeSpan ExlusiveHandlerExecutionTime, TimeSpan InclusiveHandlerExecutionTime, TimeSpan OverheadExecutionTime, ResultType ResultType) : IDbEvent;
public record QueryReceived(IQuery Query, IQuery? ParentQuery) : IDbEvent;
public record QueryAnswered(IQuery Query, IQuery? ParentQuery) : IDbEvent;
