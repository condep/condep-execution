using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using ConDep.Dsl.Logging;
using ConDep.Execution.Config;
using Microsoft.ServiceBus;
using System.Threading;

namespace ConDep.Execution.Relay
{
    public class RelayHandler
    {
        private HttpClient _client;

        public ConDepExecutionResult Relay(ArtifactManifest artifactManifest, RelayConfig relayConfig, DeployOptions deployOptions)
        {
            bool success = true;
            ExecutionStartedStatus startedStatus = null;
            try
            {
                var credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(relayConfig.AccessKey,
                    relayConfig.AccessSecret);
                var conString = ServiceBusEnvironment.CreateServiceUri("https", relayConfig.Origin, relayConfig.RelayId);
                var tmp =
                    credentials.GetTokenAsync(
                        "https://" + relayConfig.Origin + ".servicebus.windows.net/" + relayConfig.RelayId, "GET", true,
                        new TimeSpan(0, 20, 0)).Result as SharedAccessSignatureToken;

                _client = new HttpClient {BaseAddress = conString};
                _client.DefaultRequestHeaders.Add("ServiceBusAuthorization", tmp.Token);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //if (!string.IsNullOrWhiteSpace(deployOptions.TraceLevel))
                //{
                //    deployOptions.TraceLevel = 
                //}
                var result = _client.PostAsJsonAsync("RelayService/Runbook", new RelaySettings
                {
                    ConDepOptions = deployOptions,
                    ConPackArtifact = artifactManifest.ConPack,
                    ApplicationArtifacts = artifactManifest.Apps.ToArray()
                }).Result;

                if (!result.IsSuccessStatusCode)
                {
                    Logger.Error("Failed to relay! Status code " + result.StatusCode + ". Reason : " + result.ReasonPhrase);
                    return new ConDepExecutionResult(false);
                }

                startedStatus = result.Content.ReadAsAsync<ExecutionStartedStatus>().Result;

                result = _client.GetAsync(string.Format("RelayService/Runbook/Log/{0}", startedStatus.ExecutionId)).Result;
                if (!result.IsSuccessStatusCode)
                {
                    Logger.Error("Failed to retreive execution log! Status code " + result.StatusCode + ". Reason : " + result.ReasonPhrase);
                    return new ConDepExecutionResult(false);
                }

                var log = result.Content.ReadAsAsync<ExecutionLog>().Result;

                if (!string.IsNullOrWhiteSpace(log.Content))
                {
                    Logger.Info(log.Content);
                }

                while (!log.Finished)
                {
                    result =
                        _client.GetAsync(string.Format("RelayService/Runbook/Log/{0}/{1}", startedStatus.ExecutionId,
                            log.End)).Result;
                    log = result.Content.ReadAsAsync<ExecutionLog>().Result;

                    if(!string.IsNullOrWhiteSpace(log.Content))
                    {
                        Logger.Info(log.Content.TrimEnd('\n', '\r'));
                    }

                    Thread.Sleep(2000);
                }

                Logger.Info("Getting final status.");
                var statusResult = _client.GetAsync(string.Format("RelayService/Runbook/{0}", startedStatus.ExecutionId)).Result;
                if (!statusResult.IsSuccessStatusCode)
                {
                    Logger.Error("Failed to get latest status! Status code " + result.StatusCode + ". Reason : " + result.ReasonPhrase);
                    return new ConDepExecutionResult(false);
                }

                var status = statusResult.Content.ReadAsAsync<ExecutionStatus>().Result;
                if (status.UnhandledExceptions != null)
                {
                    foreach (var unhandledEx in status.UnhandledExceptions)
                    {
                        Logger.Error("Unhandled exception!", unhandledEx.Exception);
                    }
                }

                success = !status.Failed && !status.Cancelled;
            }
            catch (Exception ex)
            {
                Logger.Error("An exception during Relay occoured.", ex);

                if (startedStatus != null)
                {
                    Logger.Info("Trying to get more details...");
                    try
                    {
                        var statusResult = _client.GetAsync("RelayService/Runbook/" + startedStatus.ExecutionId).Result;
                        if (statusResult.IsSuccessStatusCode)
                        {
                            var status = statusResult.Content.ReadAsAsync<ExecutionStatus>().Result;
                            foreach (var unhandledEx in status.UnhandledExceptions)
                            {
                                Logger.Error("Unhandled exception at " + unhandledEx.DateTime.ToLocalTime(),
                                    unhandledEx.Exception);
                            }
                        }
                    }
                    catch (Exception ex2)
                    {
                        Logger.Error("Failed to get more details", ex2);
                    }
                }
            }
            return new ConDepExecutionResult(success);
        } 
    }
}