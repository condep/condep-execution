using System.Linq;
using ConDep.Dsl.Config;
using ConDep.Execution;
using NUnit.Framework;

namespace ConDep.Dsl.Tests
{
    [TestFixture]
    public class AppDependencyTests
    {
        [Test]
        public void TestThat_ArtifactWithDependencyIsDetected()
        {
            var dependencyHandler = new RunbookDependencyHandler();
            Assert.That(dependencyHandler.HasDependenciesDefined(new MyArtifactDependentOnStandardArtifact()), Is.True);
        }

        [Test]
        public void TestThat_ArtifactWithoutDependencyIsNotDetected()
        {
            var dependencyHandler = new RunbookDependencyHandler();
            Assert.That(dependencyHandler.HasDependenciesDefined(new MyStandardArtifact1()), Is.False);
        }

        [Test]
        public void TestThat_ArtifactWithDependencyDetectsCorrectDependency()
        {
            var dependencyHandler = new RunbookDependencyHandler();
            var settings = new ConDepSettings {Options = {Assembly = GetType().Assembly}};

            var artifact = new MyArtifactDependentOnStandardArtifact();
            var runbooks = dependencyHandler.GetDependeciesForRunbook(artifact, settings);
            Assert.That(runbooks.First(), Is.InstanceOf<MyStandardArtifact1>());
        }

        [Test]
        public void TestThat_ArtifactWithMultipleDependenciesReturnsCorrectDependenciesInCorrectOrder()
        {
            var dependencyHandler = new RunbookDependencyHandler();
            var settings = new ConDepSettings { Options = { Assembly = GetType().Assembly } };

            var artifact = new MyArtifactWithMultipleDependencies();
            var runbooks = dependencyHandler.GetDependeciesForRunbook(artifact, settings);

            Assert.That(runbooks.Count, Is.EqualTo(2));
            Assert.That(runbooks[0], Is.InstanceOf<MyStandardArtifact1>());
            Assert.That(runbooks[1], Is.InstanceOf<MyStandardArtifact2>());
        }

        [Test]
        public void TestThat_ArtifactWithHierarchicalDependenciesReturnsCorrectDependenciesInCorrectOrder()
        {
            var dependencyHandler = new RunbookDependencyHandler();
            var settings = new ConDepSettings { Options = { Assembly = GetType().Assembly } };

            var artifact = new MyArtifactWithHierarchicalDependencies();
            var runbooks = dependencyHandler.GetDependeciesForRunbook(artifact, settings);

            Assert.That(runbooks.Count, Is.EqualTo(2));
            Assert.That(runbooks[0], Is.InstanceOf<MyStandardArtifact1>());
            Assert.That(runbooks[1], Is.InstanceOf<MyArtifactDependentOnStandardArtifact>());
        }
    }

    public class MyStandardArtifact1 : Runbook
    {
        public override void Execute(IOfferOperations dsl, ConDepSettings settings)
        {
            
        }
    }

    public class MyStandardArtifact2 : Runbook
    {
        public override void Execute(IOfferOperations dsl, ConDepSettings settings)
        {
        }
    }

    public class MyArtifactDependentOnStandardArtifact : Runbook, IDependOn<MyStandardArtifact1>
    {
        public override void Execute(IOfferOperations dsl, ConDepSettings settings)
        {
            
        }
    }

    public class MyArtifactWithMultipleDependencies : Runbook, IDependOn<MyStandardArtifact1>, IDependOn<MyStandardArtifact2>
    {
        public override void Execute(IOfferOperations dsl, ConDepSettings settings)
        {
        }
    }

    public class MyArtifactWithHierarchicalDependencies : Runbook, IDependOn<MyArtifactDependentOnStandardArtifact>
    {
        public override void Execute(IOfferOperations dsl, ConDepSettings settings)
        {
            
        }
    }
}