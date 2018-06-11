using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using ConDep.Dsl;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.Harvesters;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;
using ConDep.Dsl.Validation;
using ConDep.Execution.Config;
using ConDep.Execution.Logging;
using ConDep.Execution.Validation;

namespace ConDep.Execution
{
    public class ConDepConfigurationExecutor : MarshalByRefObject, ITokenSource, IDisposable
    {
        private bool _cancelled;
        private bool _serverNodeInstalled;
        private static string _assemblySearchPath;
        private readonly CancellationTokenSource _cts;

        public ConDepConfigurationExecutor()
        {
            _cts = new CancellationTokenSource();
        }

        //private static Assembly ResolveConDepAssemblies(object sender, ResolveEventArgs args)
        //{
        //    var assemblyName = args.Name.Split(',')[0];
        //    var assemblyLocation = Path.Combine(typeof(ConDepConfigurationExecutor).Assembly.CodeBase, assemblyName);

        //    if (File.Exists(assemblyLocation + ".dll"))
        //    {
        //        return Assembly.LoadFrom(assemblyLocation + ".dll");
        //    }
        //    return null;
        //}

        //private static Assembly ResolveAssemblyLocation(object sender, ResolveEventArgs args)
        //{
        //    var assemblyName = args.Name.Split(',')[0];
        //    var assemblyLocation = Path.Combine(_assemblySearchPath, assemblyName);

        //    if (File.Exists(assemblyLocation + ".dll"))
        //    {
        //        return Assembly.LoadFrom(assemblyLocation + ".dll");
        //    }
        //    return null;
        //}

        public ConDepExecutionResult Execute(Guid executionId, string assemblyPath, ConDepOptions options, ITokenSource token)
        {
            throw new NotImplementedException();
            //try
            //{
            //    // 1. Initialize Logger
            //    // 2. Load ConDep Assembly and assign to Options
            //    // 3. Load Env Config and add to Settings
            //    // 4. 
            //    Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath));
            //    Logger.Initialize(new RelayApiLogger(executionId));
            //    Logger.TraceLevel = options.TraceLevel > 0 ? options.TraceLevel : TraceLevel.Info;

            //    Logger.Info("Trace level set to " + Logger.TraceLevel);

            //    var configAssemblyLoader = new ConDepAssemblyHandler(assemblyPath);
            //    options.Assembly = configAssemblyLoader.GetAssembly();

            //    var conDepSettings = new ConDepSettings
            //    {
            //        Options = options
            //    };
            //    conDepSettings.Config = ConfigHandler.GetEnvConfig(conDepSettings);

            //    if (conDepSettings.Options.Assembly == null) throw new ArgumentException("assembly");

            //    var lbLookup = new LoadBalancerLookup(conDepSettings.Config.LoadBalancer);
            //    var runbookConfigHandler = new RunbookConfigurationHandler(new RunbookHandler(), new RunbookDependencyHandler(), lbLookup.GetLoadBalancer());
            //    var sequenceManager = runbookConfigHandler.CreateExecutionSequence(conDepSettings);

            //    var clientValidator = new ClientValidator();

            //    var serverInfoHarvester = HarvesterFactory.GetHarvester(conDepSettings);
            //    var serverValidator = new RemoteServerValidator(conDepSettings.Config.Servers,
            //                                                    serverInfoHarvester, new PowerShellExecutor());


            //    if (conDepSettings.Options.DryRun)
            //    {
            //        Logger.Warn("Showing execution sequence from dry run:");
            //        sequenceManager.DryRun(conDepSettings);
            //        return new ConDepExecutionResult(true);
            //    }

            //    return Execute(conDepSettings, clientValidator, serverValidator, sequenceManager, token.Token);
            //}
            //catch (Exception ex)
            //{
            //    try
            //    {
            //        Logger.Error("An error sneaked by.", ex);
            //    }
            //    catch { }

            //    var result = new ConDepExecutionResult(false);
            //    result.AddException(ex);
            //    return result;
            //    //throw;
            //}
        }

        public static ConDepExecutionResult ExecuteFromAssembly(ConDepSettings conDepSettings, CancellationToken token)
        {
            try
            {
                if (conDepSettings.Options.Assembly == null) throw new ArgumentException("assembly");

                var lbLookup = new LoadBalancerLookup(conDepSettings.Config.LoadBalancer);
                var runbookConfigurationHandler = new RunbookConfigurationHandler(new RunbookHandler(), new RunbookDependencyHandler(), lbLookup.GetLoadBalancer());
                //var sequenceManager = artifactConfigHandler.CreateExecutionSequence(conDepSettings);

                var clientValidator = new ClientValidator();

                var serverInfoHarvester = HarvesterFactory.GetHarvester(conDepSettings);
                var serverValidator = new RemoteServerValidator(conDepSettings.Config.Servers,
                                                                serverInfoHarvester, new PowerShellExecutor());


                //if (conDepSettings.Options.DryRun)
                //{
                //    Logger.Warn("Showing execution sequence from dry run:");
                //    sequenceManager.DryRun(conDepSettings);
                //    return new ConDepExecutionResult(true);
                //}
                runbookConfigurationHandler.LoadBindingRedirects(conDepSettings);
                ConDepExecutionResult result = new ConDepConfigurationExecutor().Execute(conDepSettings, clientValidator, serverValidator, runbookConfigurationHandler.GetRunbooksToExecute(conDepSettings), new ServerHandler(), token);
                runbookConfigurationHandler.UnloadBindingRedirects();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("An error sneaked by.", ex);
                throw;
            }
        }

        public static Task<ConDepExecutionResult> ExecuteFromAssemblyAsync(ConDepSettings conDepSettings, CancellationToken token)
        {
            return Task.Factory.StartNew(() => ExecuteFromAssembly(conDepSettings, token), token);
        }

        internal ConDepExecutionResult Execute(ConDepSettings settings, IValidateClient clientValidator, IValidateServer serverValidator, IEnumerable<Runbook> runbooks, IDiscoverServers serverHandler, CancellationToken token)
        {
            if (settings == null) { throw new ArgumentException("settings"); }
            if (settings.Config == null) { throw new ArgumentException("settings.Config"); }
            if (settings.Options == null) { throw new ArgumentException("settings.Options"); }
            if (clientValidator == null) { throw new ArgumentException("clientValidator"); }
            if (serverValidator == null) { throw new ArgumentException("serverValidator"); }
            if (runbooks == null) { throw new ArgumentException("runbook"); }

            ServicePointManager.ServerCertificateValidationCallback = ValidateConDepNodeServerCert;

            try
            {
                Validate(clientValidator, serverValidator);

                ExecutePreOps(settings, token);
                _serverNodeInstalled = true;

                //Todo: Result of merge. Not sure if this is correct.
                token.Register(() => Cancel(settings, token));

                var lbLocator = new LoadBalancerLookup(settings.Config.LoadBalancer);
                foreach (var runbook in runbooks)
                {
                    var servers = serverHandler.GetServers(runbook, settings);
                    settings.Config.Servers = servers.ToList();
                    runbook.Execute(new OperationsBuilder(1, settings, lbLocator, token), settings);
                }

                return new ConDepExecutionResult(true);
            }
            catch (OperationCanceledException)
            {
                Cancel(settings, token);
                return new ConDepExecutionResult(false) { Cancelled = true };
            }
            catch (AggregateException aggEx)
            {
                var result = new ConDepExecutionResult(false);
                aggEx.Handle(inner =>
                {
                    if (inner is OperationCanceledException)
                    {
                        Cancel(settings, token);
                        result.Cancelled = true;
                        Logger.Warn("ConDep execution cancelled.");
                    }
                    else
                    {
                        result.AddException(inner);
                        Logger.Error("ConDep execution failed.", inner);
                    }

                    return true;
                });
                return result;
            }
            catch (Exception ex)
            {
                var result = new ConDepExecutionResult(false);
                result.AddException(ex);
                Logger.Error("ConDep execution failed.", ex);
                return result;
            }
            finally
            {
                if (!_cancelled) ExecutePostOps(settings, token);
            }
        }

        private bool ValidateConDepNodeServerCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var cert = new X509Certificate2(certificate);
            return DateTime.Now <= cert.NotAfter
                   && DateTime.Now >= cert.NotBefore;
        }

        private void Cancel(ConDepSettings settings, CancellationToken token)
        {
            Logger.WithLogSection("Cancellation", () =>
            {
                try
                {
                    var tokenSource = new CancellationTokenSource();
                    Logger.Warn("Cancelling execution gracefully!");
                    _cancelled = true;
                    if (_serverNodeInstalled) ExecutePostOps(settings, tokenSource.Token);
                }
                catch (AggregateException aggEx)
                {
                    foreach (var ex in aggEx.InnerExceptions)
                    {
                        Logger.Error("Failure during cancellation", ex);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failure during cancellation", ex);
                }
            });
        }

        private static void Validate(IValidateClient clientValidator, IValidateServer serverValidator)
        {
            clientValidator.Validate();

            if (!serverValidator.Validate())
            {
                throw new ConDepValidationException("Not all servers fulfill ConDep's requirements. Aborting execution.");
            }
        }

        public static void ExecutePreOps(ConDepSettings conDepSettings, CancellationToken token)
        {
            Logger.WithLogSection("Executing pre-operations", () =>
            {
                foreach (var server in conDepSettings.Config.Servers)
                {
                    Logger.WithLogSection(server.Name, () =>
                    {
                        //Todo: This will not work with ConDep server. After first run, this key will always exist.
                        if (!ConDepGlobals.ServersWithPreOps.ContainsKey(server.Name))
                        {
                            var remotePreOps = new PreRemoteOps(new PowerShellExecutor());
                            var dsl = new RemoteOperationsBuilder(server, conDepSettings, token);
                            remotePreOps.Execute(dsl, server, conDepSettings, token);
                            ConDepGlobals.ServersWithPreOps.Add(server.Name, server);
                        }
                    });
                }
            });
        }

        private static void ExecutePostOps(ConDepSettings conDepSettings, CancellationToken token)
        {
            foreach (var server in conDepSettings.Config.Servers)
            {
                //Todo: This will not work with ConDep server. After first run, this key will always exist.
                if (ConDepGlobals.ServersWithPreOps.ContainsKey(server.Name))
                {
                    var remotePostOps = new PostRemoteOps();
                    var dsl = new RemoteOperationsBuilder(server, conDepSettings, token);
                    remotePostOps.Execute(dsl, server, conDepSettings, token);
                    ConDepGlobals.ServersWithPreOps.Remove(server.Name);
                }
            }
        }

        public void Dispose()
        {
            
        }

        public void Cancel()
        {
            
        }

        public CancellationToken Token { get { return _cts.Token; } }
    }
}