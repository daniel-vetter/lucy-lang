using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.Tests.DbTests
{
    public record SumOfValueAAndValueBQuery : IQuery<SumOfValueAAndValueBResult>;
    public record SumOfValueAAndValueBResult(int Value);

    public class SumOfValueAAndValueBHandler : QueryHandler<SumOfValueAAndValueBQuery, SumOfValueAAndValueBResult>
    {
        public override SumOfValueAAndValueBResult Handle(IDb runner, SumOfValueAAndValueBQuery query)
        {
            var valueA = runner.Query(new ValueAQuery()).Value;
            var valueB = runner.Query(new ValueBQuery()).Value;

            return new SumOfValueAAndValueBResult(valueA + valueB);
        }
    }

    public record ValueAQuery : IQuery<ValueAResult>;
    public record ValueAResult(int Value);

    public record ValueBQuery : IQuery<ValueBResult>;
    public record ValueBResult(int Value);

    public record SumQuery(string Key1, string Key2) : IQuery<NumberResult>;
    public record MultiplySumQuery(string Addition1Left, string Addition1Right, string Addition2Left, string Addition2Right) : IQuery<NumberResult>;

    public record NumberResult(int Value)
    {
        public override string ToString() => $"NumberResult: {Value}";
    }

    public class SumHandler : QueryHandler<SumQuery, NumberResult>
    {
        public override NumberResult Handle(IDb db, SumQuery query)
        {
            var value1 = db.Query(new ValueQuery(query.Key1)).Value;
            var value2 = db.Query(new ValueQuery(query.Key2)).Value;

            return new NumberResult(value1 + value2);
        }
    }

    public class MultiplySumHandler : QueryHandler<MultiplySumQuery, NumberResult>
    {
        public override NumberResult Handle(IDb db, MultiplySumQuery query)
        {
            var result1 = db.Query(new SumQuery(query.Addition1Left, query.Addition1Right));
            var result2 = db.Query(new SumQuery(query.Addition2Left, query.Addition2Right));

            return new NumberResult(result1.Value * result2.Value);
        }
    }

    public record ValueQuery(string Key) : IQuery<ValueResult>;

    public record ValueResult(int Value)
    {
        public override string ToString() => $"Value: {Value}";
    }
}
