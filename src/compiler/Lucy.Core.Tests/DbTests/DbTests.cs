using Lucy.Core.SemanticAnalysis.Infrastructure;
using Shouldly;

namespace Lucy.Core.Tests.DbTests;

public class DbTests
{
    [Test]
    public void Requesting_an_input_by_type_should_return_the_correct_input()
    {
        var db = new Db();
        db.SetInput(new ValueAQuery(), new ValueAResult(1));
        db.SetInput(new ValueBQuery(), new ValueBResult(2));

        var valueA = db.Query(new ValueAQuery()).Value;
        var valueB = db.Query(new ValueBQuery()).Value;

        valueA.ShouldBe(1);
        valueB.ShouldBe(2);
    }

    [Test]
    public void Running_a_query_without_parameters_should_execute_the_handler_and_return_the_result()
    {
        var db = new Db();
        db.RegisterHandler(new SumOfValueAAndValueBHandler());

        db.SetInput(new ValueAQuery(), new ValueAResult(1));
        db.SetInput(new ValueBQuery(), new ValueBResult(2));

        var result = db.Query(new SumOfValueAAndValueBQuery());

        result.Value.ShouldBe(3);
    }

    [Test]
    public void Running_a_query_without_parameters_twice_should_return_the_result_both_times()
    {
        var db = new Db();
        db.RegisterHandler(new SumOfValueAAndValueBHandler());

        db.SetInput(new ValueAQuery(), new ValueAResult(1));
        db.SetInput(new ValueBQuery(), new ValueBResult(2));

        var result1 = db.Query(new SumOfValueAAndValueBQuery());
        var result2 = db.Query(new SumOfValueAAndValueBQuery());

        result1.Value.ShouldBe(3);
        result2.Value.ShouldBe(3);
    }

    [Test]
    public void Changing_a_input_without_parameters_should_result_in_a_new_query_result()
    {
        var db = new Db();
        db.RegisterHandler(new SumOfValueAAndValueBHandler());

        db.SetInput(new ValueAQuery(), new ValueAResult(1));
        db.SetInput(new ValueBQuery(), new ValueBResult(2));

        db.Query(new SumOfValueAAndValueBQuery()).Value.ShouldBe(3);

        db.SetInput(new ValueBQuery(), new ValueBResult(5));
        db.Query(new SumOfValueAAndValueBQuery()).Value.ShouldBe(6);
    }

    [Test]
    public void Requesting_an_input_should_return_the_correct_input()
    {
        var db = new Db();
        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));

        var valueA = db.Query(new ValueQuery("A")).Value;
        var valueB = db.Query(new ValueQuery("B")).Value;

        valueA.ShouldBe(1);
        valueB.ShouldBe(2);
    }

    [Test]
    public void Running_a_query_should_execute_the_handler_and_return_the_result()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));

        var result = db.Query(new SumQuery("A", "B"));

        result.Value.ShouldBe(3);
    }
    [Test]
    public void Running_a_query_twice_should_return_the_result_both_times()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));

        var result1 = db.Query(new SumQuery("A", "B"));
        var result2 = db.Query(new SumQuery("A", "B"));

        result1.Value.ShouldBe(3);
        result2.Value.ShouldBe(3);
    }

    [Test]
    public void Changing_the_input_should_invalidate_the_cache()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));

        db.Query(new SumQuery("A", "B")).Value.ShouldBe(3);

        db.SetInput(new ValueQuery("B"), new ValueResult(5));
        db.Query(new SumQuery("A", "B")).Value.ShouldBe(6);
    }

    [Test]
    public void It_should_be_possible_to_run_nested_queries()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());
        db.RegisterHandler(new MultiplySumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));
        db.SetInput(new ValueQuery("C"), new ValueResult(3));
        db.SetInput(new ValueQuery("D"), new ValueResult(4));

        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(21); // (1+2) * (3+4)
        db.Query(new MultiplySumQuery("A", "D", "B", "C")).Value.ShouldBe(25); // (1+4) * (2+3)
        db.Query(new MultiplySumQuery("C", "A", "D", "B")).Value.ShouldBe(24); // (3+1) * (4+2)
        db.Query(new MultiplySumQuery("A", "C", "D", "B")); // (3+1) * (4+2)
    }

    [Test]
    public void Running_a_query_twice_should_use_the_cache()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));

        db.Query(new SumQuery("A", "B")).Value.ShouldBe(3);
        db.Query(new SumQuery("A", "B")).Value.ShouldBe(3);

        var log = db.GetLastQueryExecutionLog();
        log.Calculations.Length.ShouldBe(0);
    }

    [Test]
    public void Running_nested_a_query_twice_should_use_the_cache()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());
        db.RegisterHandler(new MultiplySumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));
        db.SetInput(new ValueQuery("C"), new ValueResult(3));
        db.SetInput(new ValueQuery("D"), new ValueResult(4));

        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(21);
        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(21);
    }

    [Test]
    public void Changing_the_input_should_invalidate_the_nested_query_cache()
    {
        var db = new Db();

        db.RegisterHandler(new SumHandler());
        db.RegisterHandler(new MultiplySumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));
        db.SetInput(new ValueQuery("C"), new ValueResult(3));
        db.SetInput(new ValueQuery("D"), new ValueResult(4));

        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(21);

        db.SetInput(new ValueQuery("C"), new ValueResult(10));
        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(42);

        var log = db.GetLastQueryExecutionLog();
        log.Calculations.Count(x => x.Query is SumQuery).ShouldBe(1);
        log.Calculations.Count(x => x.Query is MultiplySumQuery).ShouldBe(1);
    }

    [Test]
    public void Removing_a_input_should_invalidate_the_query_cache_and_re_execute_the_handler()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));

        db.Query(new SumQuery("A", "B")).Value.ShouldBe(3);

        db.RemoveInput(new ValueQuery("A"));

        var ex = Should.Throw<Exception>(() =>
        {
            db.Query(new SumQuery("A", "B"));
        });

        ex.Message.ShouldBe("For a query of type 'ValueQuery' is no input provided and no query handler registered.");
    }

    [Test]
    public void Removing_a_input_which_does_not_exist_should_throw_an_exception()
    {
        var db = new Db();
        var ex = Should.Throw<Exception>(() =>
        {
            db.RemoveInput(new ValueQuery("A"));
        });

        ex.Message.ShouldBe("The input could not be removed because it did not exist.");
    }

    [Test]
    public void If_the_result_of_an_subquery_is_the_same_as_the_already_cached_result_it_should_not_reexecute_dependent_handler3()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());
        db.RegisterHandler(new MultiplySumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));
        db.SetInput(new ValueQuery("C"), new ValueResult(3));
        db.SetInput(new ValueQuery("D"), new ValueResult(4));

        //This will calculate (1+2)*(3+4)=3 and put it in the cache
        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(21);

        //Reversing A and B
        db.SetInput(new ValueQuery("C"), new ValueResult(4));
        db.SetInput(new ValueQuery("D"), new ValueResult(3));

        //This will calculate (2+1)*(3+4)=3
        //Since 2+1 is still the same as 1+2, the MultiplySumQuery handle should not be executed.
        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(21);

        var log = db.GetLastQueryExecutionLog();
        log.Calculations.Length.ShouldBe(1);
        log.Calculations.Single().Query.ShouldBe(new SumQuery("C", "D"));
    }

    [Test]
    public void If_the_result_of_an_subquery_is_the_same_as_the_already_cached_result_it_should_not_reexecute_dependent_handler()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());
        db.RegisterHandler(new MultiplySumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));
        db.SetInput(new ValueQuery("C"), new ValueResult(3));
        db.SetInput(new ValueQuery("D"), new ValueResult(4));

        //This will calculate (1+2)*(3+4)=3 and put it in the cache
        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(21);

        //Reversing A and B
        db.SetInput(new ValueQuery("A"), new ValueResult(2));
        db.SetInput(new ValueQuery("B"), new ValueResult(1));

        //This will calculate (2+1)*(3+4)=3
        //Since 2+1 is still the same as 1+2, the MultiplySumQuery handle should not be executed.
        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(21);

        var log = db.GetLastQueryExecutionLog();
        log.Calculations.Length.ShouldBe(1);
        log.Calculations.Single().Query.ShouldBe(new SumQuery("A", "B"));
    }


    [Test]
    public void If_the_result_of_an_subquery_is_the_same_as_the_already_cached_result_it_should_not_reexecute_dependent_handler2()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());
        db.RegisterHandler(new MultiplySumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));
        db.SetInput(new ValueQuery("C"), new ValueResult(3));
        db.SetInput(new ValueQuery("D"), new ValueResult(4));

        //This will calculate (1+2)*(3+4)=3 and put it in the cache
        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(21);

        //Reversing A and B
        db.SetInput(new ValueQuery("A"), new ValueResult(2));
        db.SetInput(new ValueQuery("B"), new ValueResult(1));
        db.SetInput(new ValueQuery("C"), new ValueResult(4));
        db.SetInput(new ValueQuery("D"), new ValueResult(3));

        //This will calculate (2+1)*(3+4)=3
        //Since 2+1 is still the same as 1+2, the MultiplySumQuery handle should not be executed.
        db.Query(new MultiplySumQuery("A", "B", "C", "D")).Value.ShouldBe(21);

        var log = db.GetLastQueryExecutionLog();
        log.Calculations.Length.ShouldBe(2);
        log.Calculations[0].Query.ShouldBe(new SumQuery("A", "B"));
        log.Calculations[1].Query.ShouldBe(new SumQuery("C", "D"));
    }


    [Test]
    public void If_the_result_of_an_query_is_the_same_as_the_cached_version_it_should_throw_away_the_new_result_instance()
    {
        var db = new Db();
        db.RegisterHandler(new SumHandler());

        db.SetInput(new ValueQuery("A"), new ValueResult(1));
        db.SetInput(new ValueQuery("B"), new ValueResult(2));

        var r1 = db.Query(new SumQuery("A", "B"));

        db.SetInput(new ValueQuery("A"), new ValueResult(2));
        db.SetInput(new ValueQuery("B"), new ValueResult(1));

        var r2 = db.Query(new SumQuery("A", "B"));

        ReferenceEquals(r1, r2).ShouldBeTrue();
    }
}