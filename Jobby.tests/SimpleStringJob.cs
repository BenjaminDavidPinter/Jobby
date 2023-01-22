using Jobby.lib.Core.JobTypes;
using Jobby.Lib.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Jobby.tests;
public class SimpleStringJob
{
    ServiceCollection _collection {get;set;}
    ServiceProvider _provider {get;set;}
    [SetUp]
    public void Setup()
    {
        _collection = new();
        _collection.AddScoped(typeof(IJobbyJobRunner<>), typeof(JobbyJobRunner<>));
        _collection.AddScoped(typeof(IJobbyJobQueue<>), typeof(JobbyJobQueue<>));

        _provider = _collection.BuildServiceProvider();
    }

    [Test]
    public void TestSimpleString()
    {
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
        testRunner.StartJobs();
        System.Threading.Thread.Sleep(1000);
        Assert.IsTrue(testRunner._backingQueue._JobResultInternal
        .First(x => x.Item1 == "Simple String Job")
        .Item2.Any(y => y.Contains("Hello Jobby!")));
    }
}

public class StringJob : IJobbyJob<string>
{
    public string JobName {get => "Simple String Job";}
    public TimeSpan CycleTime {get => TimeSpan.FromMilliseconds(250);}
    public TimeSpan TimeOut {get => TimeSpan.FromDays(1);}
    public TimeOnly StartTime {get => new TimeOnly(00,00);}
    public TimeOnly EndTime {get => new TimeOnly(23,59);}

    public string Run()
    {
        return $"Hello Jobby! {DateTime.Now.ToString()}";
    }
}