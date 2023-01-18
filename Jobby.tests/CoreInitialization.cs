using Jobby.Lib.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Jobby.tests;
public class CoreInitialization
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
    public void EnsureDependencyChain()
    {
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
        Assert.IsNotNull(testRunner);
    }

    [Test]
    public void EnsureQueuesAreInitialized(){
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
        Assert.IsNotNull(testRunner._backingQueue._JobQueueInternal);
        Assert.IsNotNull(testRunner._backingQueue._JobResultInternal);
    }
}