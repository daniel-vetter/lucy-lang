using Lucy.Core.SemanticAnalysis.Infrastructure;
using Shouldly;

namespace Lucy.Core.Tests.DbTests;

[DbInputs]
public interface ITestInputs
{
    int ValueA();
    int ValueB();
    int IntValue(string key);
    string StringValue(string key);
}

public static class Handler
{
    [DbQuery]
    public static int GetSumOfValueAAndValueB(IDb db)
    {
        return db.GetValueA() + db.GetValueB();
    }

    [DbQuery]
    public static string GetConcatenation(IDb db, string key1, string key2)
    {
        return db.GetStringValue(key1) + db.GetStringValue(key2);
    }

    [DbQuery]
    public static int GetSum(IDb db, string key1, string key2)
    {
        return db.GetIntValue(key1) + db.GetIntValue(key2);
    }

    [DbQuery]
    public static int GetMultiSum(IDb db, string addition1Left, string addition1Right, string addition2Left, string addition2Right)
    {
        var sum1 = db.GetSum(addition1Left, addition1Right);
        var sum2 = db.GetSum(addition2Left, addition2Right);
        return sum1 * sum2;
    }
}

public class DbTests
{
    [Test]
    public void Requesting_an_input_by_type_should_return_the_correct_input()
    {
        var db = new Db();
        db.SetValueA(1);
        db.SetValueB(2);

        var valueA = db.GetValueA();
        var valueB = db.GetValueB();
        
        valueA.ShouldBe(1);
        valueB.ShouldBe(2);
    }

    [Test]
    public void Running_a_query_without_parameters_should_execute_the_handler_and_return_the_result()
    {
        var db = new Db();
        db.RegisterHandler(new GetSumOfValueAAndValueBGeneratedHandler());

        db.SetValueA(1);
        db.SetValueB(2);

        var result = db.GetSumOfValueAAndValueB();

        result.ShouldBe(3);
    }

    [Test]
    public void Running_a_query_without_parameters_twice_should_return_the_result_both_times()
    {
        var db = new Db();
        db.RegisterHandler(new GetSumOfValueAAndValueBGeneratedHandler());

        db.SetValueA(1);
        db.SetValueB(2);

        var result1 = db.GetSumOfValueAAndValueB();
        var result2 = db.GetSumOfValueAAndValueB();

        result1.ShouldBe(3);
        result2.ShouldBe(3);
    }

    [Test]
    public void Changing_a_input_without_parameters_should_result_in_a_new_query_result()
    {
        var db = new Db();
        db.RegisterHandler(new GetSumOfValueAAndValueBGeneratedHandler());

        db.SetValueA(1);
        db.SetValueB(2);

        db.GetSumOfValueAAndValueB().ShouldBe(3);

        db.SetValueB(3);
        db.GetSumOfValueAAndValueB().ShouldBe(4);
    }

    [Test]
    public void Requesting_an_input_should_return_the_correct_input()
    {
        var db = new Db();
        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);

        var valueA = db.GetIntValue("A");
        var valueB = db.GetIntValue("B");

        valueA.ShouldBe(1);
        valueB.ShouldBe(2);
    }

    [Test]
    public void Running_a_query_should_execute_the_handler_and_return_the_result()
    {
        var db = new Db();
        db.RegisterHandlerFromCurrentAssembly();

        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);

        db.GetSum("A", "B").ShouldBe(3);
    }
    [Test]
    public void Running_a_query_twice_should_return_the_result_both_times()
    {
        var db = new Db();
        db.RegisterHandlerFromCurrentAssembly();

        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);

        var result1 = db.GetSum("A", "B");
        var result2 = db.GetSum("A", "B");

        result1.ShouldBe(3);
        result2.ShouldBe(3);
    }

    [Test]
    public void Changing_the_input_should_invalidate_the_cache()
    {
        var db = new Db();
        db.RegisterHandlerFromCurrentAssembly();
        
        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);

        db.GetSum("A", "B").ShouldBe(3);

        db.SetIntValue("B", 5);
        db.GetSum("A", "B").ShouldBe(6);
    }

    [Test]
    public void It_should_be_possible_to_run_nested_queries()
    {
        var db = new Db();
        db.RegisterHandlerFromCurrentAssembly();

        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);
        db.SetIntValue("C", 3);
        db.SetIntValue("D", 4);

        db.GetMultiSum("A", "B", "C", "D").ShouldBe(21); // (1+2) * (3+4)
        db.GetMultiSum("A", "D", "B", "C").ShouldBe(25); // (1+4) * (2+3)
        db.GetMultiSum("C", "A", "D", "B").ShouldBe(24); // (3+1) * (4+2)
        db.GetMultiSum("A", "C", "D", "B").ShouldBe(24); // (3+1) * (4+2)
    }

    [Test]
    public void Running_a_query_twice_should_use_the_cache()
    {
        var db = new Db(true);
        db.RegisterHandlerFromCurrentAssembly();

        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);

        db.GetSum("A", "B").ShouldBe(3);
        db.GetSum("A", "B").ShouldBe(3);

        var log = db.GetLastQueryMetrics();
        log.Calculations.Length.ShouldBe(0);
    }

    [Test]
    public void Running_nested_a_query_twice_should_use_the_cache()
    {
        var db = new Db(true);
        db.RegisterHandlerFromCurrentAssembly();

        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);
        db.SetIntValue("C", 3);
        db.SetIntValue("D", 4);

        db.GetMultiSum("A", "B", "C", "D").ShouldBe(21);
        var log1 = db.GetLastQueryMetrics();
        log1.Calculations.Select(x => x.Query).Count(x => x is GetSumInput).ShouldBe(2);
        log1.Calculations.Select(x => x.Query).Count(x => x is GetMultiSumInput).ShouldBe(1);

        db.GetMultiSum("A", "B", "C", "D").ShouldBe(21);

        var log2 = db.GetLastQueryMetrics();
        log2.Calculations.Select(x => x.Query).Count(x => x is GetSumInput).ShouldBe(0);
        log2.Calculations.Select(x => x.Query).Count(x => x is GetMultiSumInput).ShouldBe(0);
    }

    [Test]
    public void Changing_the_input_should_invalidate_the_nested_query_cache()
    {
        var db = new Db(true);
        db.RegisterHandlerFromCurrentAssembly();

        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);
        db.SetIntValue("C", 3);
        db.SetIntValue("D", 4);

        db.GetMultiSum("A", "B", "C", "D").ShouldBe(21);

        db.SetIntValue("C", 10);
        db.GetMultiSum("A", "B", "C", "D").ShouldBe(42);

        var log = db.GetLastQueryMetrics();
        log.Calculations.Select(x => x.Query).Count(x => x is GetSumInput).ShouldBe(1);
        log.Calculations.Select(x => x.Query).Count(x => x is GetMultiSumInput).ShouldBe(1);
    }

    [Test]
    public void Removing_a_input_should_invalidate_the_query_cache_and_re_execute_the_handler()
    {
        var db = new Db();
        db.RegisterHandlerFromCurrentAssembly();

        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);

        db.GetSum("A", "B").ShouldBe(3);

        db.RemoveIntValue("A");

        var ex = Should.Throw<Exception>(() =>
        {
            db.GetSum("A", "B");
        });

        ex.Message.ShouldBe("For a query of type 'IntValueInputQuery' with parameter '{\"key\":\"A\"}' is no input provided and no query handler registered.");
    }

    [Test]
    public void Removing_a_input_which_does_not_exist_should_throw_an_exception()
    {
        var db = new Db();
        var ex = Should.Throw<Exception>(() =>
        {
            db.RemoveIntValue("A");
        });

        ex.Message.ShouldBe("The input could not be removed because it did not exist.");
    }

    [Test]
    public void If_the_result_of_an_subquery_is_the_same_as_the_already_cached_result_it_should_not_reexecute_dependent_handler3()
    {
        var db = new Db(true); 
        db.RegisterHandlerFromCurrentAssembly();
        
        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);
        db.SetIntValue("C", 3);
        db.SetIntValue("D", 4);

        //This will calculate (1+2)*(3+4)=3 and put it in the cache
        db.GetMultiSum("A", "B", "C", "D").ShouldBe(21);

        //Reversing A and B
        db.SetIntValue("C", 4);
        db.SetIntValue("D", 3);

        //This will calculate (2+1)*(3+4)=3
        //Since 2+1 is still the same as 1+2, the MultiplySumQuery handle should not be executed.
        db.GetMultiSum("A", "B", "C", "D").ShouldBe(21);

        var log = db.GetLastQueryMetrics();
        log.Calculations.Length.ShouldBe(1);
        log.Calculations.Single().Query.ShouldBe(new GetSumInput("C", "D"));
    }

    [Test]
    public void If_the_result_of_an_subquery_is_the_same_as_the_already_cached_result_it_should_not_reexecute_dependent_handler()
    {
        var db = new Db(true);
        db.RegisterHandlerFromCurrentAssembly();
        
        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);
        db.SetIntValue("C", 3);
        db.SetIntValue("D", 4);

        //This will calculate (1+2)*(3+4)=3 and put it in the cache
        db.GetMultiSum("A", "B", "C", "D").ShouldBe(21);

        //Reversing A and B
        db.SetIntValue("A", 2);
        db.SetIntValue("B", 1);

        //This will calculate (2+1)*(3+4)=3
        //Since 2+1 is still the same as 1+2, the MultiplySumQuery handle should not be executed.
        db.GetMultiSum("A", "B", "C", "D").ShouldBe(21);

        var log = db.GetLastQueryMetrics();
        log.Calculations.Length.ShouldBe(1);
        log.Calculations.Single().Query.ShouldBe(new GetSumInput("A", "B"));
    }
    
    [Test]
    public void If_the_result_of_an_subquery_is_the_same_as_the_already_cached_result_it_should_not_reexecute_dependent_handler2()
    {
        var db = new Db(true);
        db.RegisterHandlerFromCurrentAssembly();
        
        db.SetIntValue("A", 1);
        db.SetIntValue("B", 2);
        db.SetIntValue("C", 3);
        db.SetIntValue("D", 4);

        //This will calculate (1+2)*(3+4)=3 and put it in the cache
        db.GetMultiSum("A", "B", "C", "D").ShouldBe(21);

        //Reversing A and B
        db.SetIntValue("A", 2);
        db.SetIntValue("B", 1);
        db.SetIntValue("C", 4);
        db.SetIntValue("D", 3);

        //This will calculate (2+1)*(3+4)=3
        //Since 2+1 is still the same as 1+2, the MultiplySumQuery handle should not be executed.
        db.GetMultiSum("A", "B", "C", "D").ShouldBe(21);

        var log = db.GetLastQueryMetrics();
        log.Calculations.Length.ShouldBe(2);
        log.Calculations[0].Query.ShouldBe(new GetSumInput("A", "B"));
        log.Calculations[1].Query.ShouldBe(new GetSumInput("C", "D"));
    }


    [Test]
    public void If_the_result_of_an_query_is_the_same_as_the_cached_version_it_should_throw_away_the_new_result_instance()
    {
        var db = new Db();
        db.RegisterHandlerFromCurrentAssembly();

        db.SetStringValue("A", "1");
        db.SetStringValue("B", "2");

        var r1 = db.GetConcatenation("A", "B");

        db.SetStringValue("A", "12");
        db.SetStringValue("B", "");

        var r2 = db.GetConcatenation("A", "B");
        
        ReferenceEquals(r1, r2).ShouldBeTrue();
    }
}