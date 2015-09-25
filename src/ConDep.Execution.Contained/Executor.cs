using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ConDep.Execution.Contained
{
    public enum ExecutionStatus
    {
        Success,
        Failure,
        Cancelled
    }

    public class ExecutionResult
    {
        private List<Tuple<DateTime, Exception>> _exceptions = new List<Tuple<DateTime, Exception>>();

        public ExecutionStatus Status { get; set; }

        public void AddException(DateTime dateTime, Exception ex)
        {
            _exceptions.Add(new Tuple<DateTime, Exception>(dateTime, ex));
        }

        public void AddException(Exception ex)
        {
            _exceptions.Add(new Tuple<DateTime, Exception>(DateTime.UtcNow, ex));
        }

        public IEnumerable<Tuple<DateTime, Exception>> Exceptions { get { return _exceptions; } }

        public bool HasExceptions()
        {
            return _exceptions.Count > 0;
        }
    }

    public class Executor : MarshalByRefObject, IDisposable
    {
        private string _assemblySearchPath;

        public ExecutionResult Execute(string baseDir, string assemblyFilePath, string artifact, string env)
        {
            _assemblySearchPath = baseDir;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveConDepAssembly;
            //ToDo: Handle config settings
            var settings = CreateConDepSettings(env, artifact);

            try
            {
                settings.Options.Assembly = Assembly.LoadFile(assemblyFilePath);

                //ConDepGlobals.Reset();
                //try
                //{
                //    var result = ConDepConfigurationExecutor.ExecuteFromAssembly(settings, tokenSource.Token);

                //    if (result.Success)
                //    {
                //        Logger.Info("ConDep finished execution run successfully");
                //    }
                //    else
                //    {
                //        Logger.Error("ConDep finished execution run un-successfully");
                //    }
                //    return CreateResult(result);
                //}
                //catch (OperationCanceledException)
                //{
                //    return new ExecutionResult { Status = ExecutionStatus.Cancelled };
                //}
                return new ExecutionResult();
            }
            catch (AggregateException aggEx)
            {
                var result = new ExecutionResult();
                aggEx.Handle(inner =>
                {
                    if (inner is OperationCanceledException)
                    {
                        result.Status = ExecutionStatus.Cancelled;
                        //Logger.Warn("ConDep execution cancelled.");
                    }
                    else
                    {
                        result.AddException(inner);
                        //Logger.Error("Unhandled exception during deployment", inner);
                    }

                    return true;
                });

                //Logger.Error("ConDep finished execution run with errors");
                return result;
            }
            catch (Exception ex)
            {
                var result = new ExecutionResult();
                try
                {
                    result.AddException(ex);
                    //Logger.Error("Unhandled exception during deployment", ex);
                    //Logger.Error("ConDep finished execution run with errors");
                    return result;
                }
                catch (Exception innerEx)
                {
                    result.AddException(innerEx);
                    return result;
                }
            }
        }

        private Assembly ResolveConDepAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = args.Name.Split(',')[0];
            var assemblyLocation = Path.Combine(_assemblySearchPath, assemblyName);

            if (File.Exists(assemblyLocation + ".dll"))
            {
                return Assembly.LoadFrom(assemblyLocation + ".dll");
            }
            return null;

        }

        private static dynamic CreateConDepSettings(string env, string artifact)
        {
            var settings = (dynamic)AppDomain.CurrentDomain.CreateInstanceAndUnwrap("ConDep.Dsl", "ConDep.Dsl.Config.ConDepSettings");
            var options = (dynamic)AppDomain.CurrentDomain.CreateInstanceAndUnwrap("ConDep.Dsl", "ConDep.Dsl.Config.ConDepOptions");

            options.Application = artifact;
            options.Environment = env;

            settings.Options = options;


            //settings.Options = new ConDepOptions
            //    {
            //        Application = artifact,
            //        Environment = env
            //    },
            //    Config = config
            //};

            //foreach (var server in settings.Config.Servers.Where(server => !server.DeploymentUser.IsDefined()))
            //{
            //    server.DeploymentUser = settings.Config.DeploymentUser;
            //}
            return settings;
        }

        //private ExecutionResult CreateResult(ConDepExecutionResult result)
        //{
        //    var execResult = new ExecutionResult();
        //    if (result.Success)
        //    {
        //        execResult.Status = ExecutionStatus.Success;
        //    }
        //    else if (result.Cancelled)
        //    {
        //        execResult.Status = ExecutionStatus.Cancelled;
        //    }
        //    else if (!result.Success)
        //    {
        //        execResult.Status = ExecutionStatus.Failure;
        //    }

        //    if (result.HasExceptions())
        //    {
        //        foreach (var ex in result.ExceptionMessages)
        //        {
        //            execResult.AddException(ex.DateTime, ex.Exception);
        //        }
        //    }

        //    return execResult;
        //}


        public void Dispose()
        {
        }
    }
}