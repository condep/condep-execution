using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution
{
    public interface IDiscoverRunbooks
    {
        IProvideRunbook GetRunbook(ConDepSettings settings);
    }
}