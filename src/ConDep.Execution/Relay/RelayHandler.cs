using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using ConDep.Dsl.Logging;
using ConDep.Execution.Config;
using Microsoft.ServiceBus;

namespace ConDep.Execution.Relay
{
    public class RelayHandler
    {
        private HttpClient _client;

        public ConDepExecutionResult Relay(ArtifactManifest artifactManifest, RelayConfig relayConfig, DeployOptions deployOptions)
        {
            bool success = true;
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

                var result = _client.PostAsJsonAsync("RelayService/Runbook", new RelaySettings
                {
                    ConDepOptions = deployOptions,
                    ConPackArtifact = artifactManifest.ConPack,
                    ApplicationArtifacts = artifactManifest.Apps.ToArray()
                }).Result;

                var startedStatus = result.Content.ReadAsAsync<ExecutionStartedStatus>().Result;

                result = _client.GetAsync(string.Format("RelayService/Runbook/Log/{0}", startedStatus.ExecutionId)).Result;
                var log = result.Content.ReadAsAsync<ExecutionLog>().Result;

                //Console.WriteLine("Started : " + startedStatus.Started);
                Logger.Info(log.Content);

                while (!log.Finished)
                {
                    result =
                        _client.GetAsync(string.Format("RelayService/Runbook/Log/{0}/{1}", startedStatus.ExecutionId,
                            log.End)).Result;
                    log = result.Content.ReadAsAsync<ExecutionLog>().Result;

                    Logger.Info(log.Content);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An exception during Relay occoured.", ex);
            }
            return new ConDepExecutionResult(success);
        } 
    }
}