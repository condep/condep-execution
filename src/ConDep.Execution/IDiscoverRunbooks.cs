using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution
{
    public interface IDiscoverRunbooks
    {
        Runbook GetRunbook(ConDepSettings settings);
    }
}