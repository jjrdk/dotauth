using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass)]
[assembly: Parallelization(Mode = ParallelMode.Collections)]
