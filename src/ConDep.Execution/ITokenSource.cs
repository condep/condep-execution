using System.Threading;

namespace ConDep.Execution
{
    public interface ITokenSource
    {
        CancellationToken Token { get; }
    }
}