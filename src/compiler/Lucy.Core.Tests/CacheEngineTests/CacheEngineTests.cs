using System.Collections.Immutable;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;
using Shouldly;

namespace Lucy.Core.Tests.CacheEngineTests;

[QueryGroup]
public class TestInputs
{
    public virtual int ValueA { get; set; }
    public virtual int ValueB { get; set; }

    public virtual ImmutableDictionary<string, int> IntValuesByKey { get; set; } = ImmutableDictionary<string, int>.Empty;
    public virtual ImmutableDictionary<string, string> StringValuesByKey { get; set; } = ImmutableDictionary<string, string>.Empty;
}

[QueryGroup]
public class ValueProvider
{
    private readonly TestInputs _testInputs;

    public ValueProvider(TestInputs testInputs)
    {
        _testInputs = testInputs;
    }

    public virtual int GetIntValue(string key)
    {
        return _testInputs.IntValuesByKey[key];
    }

    public virtual string GetStringValue(string key)
    {
        return _testInputs.StringValuesByKey[key];
    }
}

[QueryGroup]
public class Calculator
{
    private readonly TestInputs _testInputs;
    private readonly ValueProvider _valueProvider;

    public Calculator(TestInputs testInputs, ValueProvider valueProvider)
    {
        _testInputs = testInputs;
        _valueProvider = valueProvider;
    }

    public virtual int GetSumOfValueAAndValueB()
    {
        return _testInputs.ValueA + _testInputs.ValueB;
    }

    public virtual int GetSum(string key1, string key2)
    {
        return _valueProvider.GetIntValue(key1) + _valueProvider.GetIntValue(key2);
    }

    public virtual int GetProductOfSum(string key1, string key2, string key3, string key4)
    {
        return GetSum(key1, key2) * GetSum(key3, key4);
    }

    public virtual string GetConcatenation(string key1, string key2)
    {
        return _valueProvider.GetStringValue(key1) + _valueProvider.GetStringValue(key2);
    }
}

public class CacheEngineTests
{
    private CacheEngine CreateCacheEngine()
    {
        return new CacheEngine(new QueryGroupCollection().AddFromCurrentAssembly(), new QueryMetricsRecorder());
    }
    
    [Test]
    public void Requesting_a_input_should_return_the_correct_input()
    {
        var ce = CreateCacheEngine();
        ce.Get<TestInputs>().ValueA = 1;
        ce.Get<TestInputs>().ValueB = 2;

        var valueA = ce.Get<TestInputs>().ValueA;
        var valueB = ce.Get<TestInputs>().ValueB;

        valueA.ShouldBe(1);
        valueB.ShouldBe(2);
    }

    [Test]
    public void Requesting_a_input_after_it_was_changed_should_return_the_new_value()
    {
        var ce = CreateCacheEngine();
        ce.Get<TestInputs>().ValueA = 1;
        ce.Get<TestInputs>().ValueB = 2;

        ce.Get<TestInputs>().ValueA.ShouldBe(1);
        ce.Get<TestInputs>().ValueB.ShouldBe(2);

        ce.Get<TestInputs>().ValueB = 3;

        ce.Get<TestInputs>().ValueA.ShouldBe(1);
        ce.Get<TestInputs>().ValueB.ShouldBe(3);
    }

    [Test]
    public void Running_a_query_without_parameters_should_execute_the_handler_and_return_the_result()
    {
        var ce = CreateCacheEngine();
        ce.Get<TestInputs>().ValueA = 1;
        ce.Get<TestInputs>().ValueB = 2;

        var result = ce.Get<Calculator>().GetSumOfValueAAndValueB();

        result.ShouldBe(3);
    }

    [Test]
    public void Running_a_query_without_parameters_twice_should_return_the_same_result_both_times()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().ValueA = 1;
        ce.Get<TestInputs>().ValueB = 2;

        var result1 = ce.Get<Calculator>().GetSumOfValueAAndValueB();
        var result2 = ce.Get<Calculator>().GetSumOfValueAAndValueB();

        result1.ShouldBe(3);
        result2.ShouldBe(3);
    }

    [Test]
    public void Changing_a_input_should_result_in_a_new_query_result()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().ValueA = 1;
        ce.Get<TestInputs>().ValueB = 2;

        ce.Get<Calculator>().GetSumOfValueAAndValueB().ShouldBe(3);

        ce.Get<TestInputs>().ValueB = 3;

        ce.Get<Calculator>().GetSumOfValueAAndValueB().ShouldBe(4);
    }

    [Test]
    public void Running_a_query_should_execute_the_handler_and_return_the_result()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty.Add("A", 1);

        ce.Get<ValueProvider>().GetIntValue("A").ShouldBe(1);
    }

    [Test]
    public void Running_a_query_twice_should_return_the_result_both_times()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty
            .Add("A", 1)
            .Add("B", 2);

        var result1 = ce.Get<Calculator>().GetSum("A", "B");
        var result2 = ce.Get<Calculator>().GetSum("A", "B");

        result1.ShouldBe(3);
        result2.ShouldBe(3);
    }

    [Test]
    public void Changing_the_input_should_invalidate_the_cache()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty
            .Add("A", 1)
            .Add("B", 2);

        ce.Get<Calculator>().GetSum("A", "B").ShouldBe(3);

        ce.Get<TestInputs>().IntValuesByKey = ce.Get<TestInputs>().IntValuesByKey
            .SetItem("B", 5);

        ce.Get<Calculator>().GetSum("A", "B").ShouldBe(6);
    }

    [Test]
    public void It_should_be_possible_to_run_nested_queries()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty
            .Add("A", 1)
            .Add("B", 2)
            .Add("C", 3)
            .Add("D", 4);

        ce.Get<Calculator>().GetProductOfSum("A", "B", "C", "D").ShouldBe(21); // (1+2) * (3+4)
        ce.Get<Calculator>().GetProductOfSum("A", "D", "B", "C").ShouldBe(25); // (1+4) * (2+3)
        ce.Get<Calculator>().GetProductOfSum("C", "A", "D", "B").ShouldBe(24); // (3+1) * (4+2)
        ce.Get<Calculator>().GetProductOfSum("A", "C", "D", "B").ShouldBe(24); // (3+1) * (4+2)
    }

    [Test]
    public void Running_a_query_twice_should_use_the_cache()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty
            .Add("A", 1)
            .Add("B", 2);

        ce.Get<Calculator>().GetSum("A", "B").ShouldBe(3);
        ce.Get<Calculator>().GetSum("A", "B").ShouldBe(3);

        var log = ce.GetLastQueryMetrics();
        log.Calculations.Length.ShouldBe(0);
    }

    [Test]
    public void Running_nested_a_query_twice_should_use_the_cache()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty
            .Add("A", 1)
            .Add("B", 2)
            .Add("C", 3)
            .Add("D", 4);

        ce.Get<Calculator>().GetProductOfSum("A", "B", "C", "D").ShouldBe(21);

        var log1 = ce.GetLastQueryMetrics();
        log1.Calculations.Count(x => x.Is<Calculator>(c => c.GetSum)).ShouldBe(2);
        log1.Calculations.Count(x => x.Is<Calculator>(c => c.GetProductOfSum)).ShouldBe(1);

        ce.Get<Calculator>().GetProductOfSum("A", "B", "C", "D").ShouldBe(21);

        var log2 = ce.GetLastQueryMetrics();
        log2.Calculations.Count(x => x.Is<Calculator>(c => c.GetSum)).ShouldBe(0);
        log2.Calculations.Count(x => x.Is<Calculator>(c => c.GetProductOfSum)).ShouldBe(0);
    }

    [Test]
    public void Changing_the_input_should_invalidate_the_nested_query_cache()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty
            .Add("A", 1)
            .Add("B", 2)
            .Add("C", 3)
            .Add("D", 4);

        ce.Get<Calculator>().GetProductOfSum("A", "B", "C", "D").ShouldBe(21);

        ce.Get<TestInputs>().IntValuesByKey = ce.Get<TestInputs>().IntValuesByKey
            .SetItem("C", 10);

        ce.Get<Calculator>().GetProductOfSum("A", "B", "C", "D").ShouldBe(42);

        var log = ce.GetLastQueryMetrics();
        log.Calculations.Count(x => x.Is<Calculator>(q => q.GetSum)).ShouldBe(1);
        log.Calculations.Count(x => x.Is<Calculator>(q => q.GetProductOfSum)).ShouldBe(1);
    }

    [Test]
    public void If_the_result_of_an_subquery_is_the_same_as_the_already_cached_result_it_should_not_reexecute_dependent_handler()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty
            .Add("A", 1)
            .Add("B", 2)
            .Add("C", 3)
            .Add("D", 4);

        //This will calculate (1+2)*(3+4)=3 and put it in the cache
        ce.Get<Calculator>().GetProductOfSum("A", "B", "C", "D").ShouldBe(21);

        //Reversing A and B
        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty
            .Add("A", 1)
            .Add("B", 2)
            .Add("C", 4)
            .Add("D", 3);

        //This will calculate (2+1)*(3+4)=3
        //Since 2+1 is still the same as 1+2, the GetProductOfSum query should not be executed.
        ce.Get<Calculator>().GetProductOfSum("A", "B", "C", "D").ShouldBe(21);

        var log = ce.GetLastQueryMetrics();
        log.Calculations.Count(x => x.Is<Calculator>(c => c.GetSum)).ShouldBe(1);
        log.Calculations.Count(x => x.Is<Calculator>(c => c.GetProductOfSum)).ShouldBe(0);
    }

    [Test]
    public void If_the_result_of_an_subquery_is_the_same_as_the_already_cached_result_it_should_not_reexecute_dependent_handler2()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty
            .Add("A", 1)
            .Add("B", 2)
            .Add("C", 3)
            .Add("D", 4);

        //This will calculate (1+2)*(3+4)=3 and put it in the cache
        ce.Get<Calculator>().GetProductOfSum("A", "B", "C", "D").ShouldBe(21);

        //Reversing A and B
        ce.Get<TestInputs>().IntValuesByKey = ImmutableDictionary<string, int>.Empty
            .Add("A", 2)
            .Add("B", 1)
            .Add("C", 4)
            .Add("D", 3);

        //This will calculate (2+1)*(3+4)=3
        //Since 2+1 is still the same as 1+2, the MultiplySumQuery handle should not be executed.
        ce.Get<Calculator>().GetProductOfSum("A", "B", "C", "D").ShouldBe(21);

        var log = ce.GetLastQueryMetrics();
        log.Calculations.Count(x => x.Is<Calculator>(c => c.GetSum)).ShouldBe(2);
        log.Calculations.Count(x => x.Is<Calculator>(c => c.GetProductOfSum)).ShouldBe(0);
    }

    [Test]
    public void If_the_result_of_an_query_is_the_same_as_the_cached_version_it_should_throw_away_the_new_result_instance()
    {
        var ce = CreateCacheEngine();

        ce.Get<TestInputs>().StringValuesByKey = ImmutableDictionary<string, string>.Empty
            .Add("A", "1")
            .Add("B", "2");

        var r1 = ce.Get<Calculator>().GetConcatenation("A", "B");

        ce.Get<TestInputs>().StringValuesByKey = ImmutableDictionary<string, string>.Empty
            .Add("A", "12")
            .Add("B", "");

        var r2 = ce.Get<Calculator>().GetConcatenation("A", "B");

        ReferenceEquals(r1, r2).ShouldBeTrue();
    }
}