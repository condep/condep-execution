using System.Threading;
using ConDep.Dsl;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;

namespace ConDep.Execution
{
    internal class PostRemoteOps : RemoteOperation
    {
        public override Result Execute(IOfferRemoteOperations remote, ServerConfig server, ConDepSettings settings, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Logger.WithLogSection($"Stopping ConDepNode on server {server.Name}", () =>
            {
                var executor = new PowerShellExecutor();
                executor.Execute(server, "Stop-ConDepNode", mod =>
                {
                    mod.LoadConDepModule = false;
                    mod.LoadConDepNodeModule = true;
                }, logOutput: false);
            });

            return Result.SuccessUnChanged();
        }

        public override string Name { get { return "Post Remote Operation"; } }

        public void DryRun()
        {
            Logger.WithLogSection(Name, () => {});
        }

        public Result Result { get; set; }
    }
}