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
        .Item2.Any(y => y == "Hello Jobby!"));
    }
}

public class StringJob : IJobbyJob<string>
{
    public string JobName {get;set;}
    public TimeSpan CycleTime {get;set;}
    public TimeSpan TimeOut {get;set;}

    public StringJob(){
        JobName = "Simple String Job";
        CycleTime = TimeSpan.FromSeconds(1);
        TimeOut = TimeSpan.FromDays(1);
    }

    public string Run()
    {
        return "Hello Jobby!";
    }
}