using AutoFixture;
using FakeItEasy;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MeepleZone.Systeme.BaseTest;

public abstract class BaseTest<T> : IAsyncLifetime where T : class
{
    public Fixture Fixture;

    public FakeTimeProvider FakeTimeProvider;

    public T InstanceTest = null!;

    protected BaseTest()
    {
        Fixture = new Fixture();
        FakeTimeProvider = new FakeTimeProvider();
        Fixture.Register<TimeProvider>(() => FakeTimeProvider);
    }

    public virtual void Initialize()
    {
        InitialiserDependence();
        InstanceTest = Fixture.Build<T>()
            .OmitAutoProperties()
            .Create();
    }

    public virtual void InitialiserDependence()
    {
    }

    public Fake<TMock> CreerFausse<TMock>(bool strict = true) where TMock : class
    {
        var fausse = strict ? new Fake<TMock>(x => x.Strict()) : new Fake<TMock>();
        Fixture.Register(() => fausse.FakedObject);

        return fausse;
    }

    public ValueTask InitializeAsync()
    {
        Initialize();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
