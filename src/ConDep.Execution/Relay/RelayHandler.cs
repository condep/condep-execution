using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.ServiceModel.Description;
using ConDep.Dsl.Config;
using ConDep.Execution.Config;
using Microsoft.ServiceBus;
using Newtonsoft.Json;

namespace ConDep.Execution.Relay
{
    public class RelayHandler
    {
        private HttpClient _client;

        public RelayHandler()
        {
        }

        public void Relay(ArtifactManifest artifactManifest, RelayConfig relayConfig, DeployOptions deployOptions)
        {
            var credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(relayConfig.AccessKey, relayConfig.AccessSecret);
            var conString = ServiceBusEnvironment.CreateServiceUri("https", relayConfig.Origin, relayConfig.RelayId);
            var tmp = credentials.GetTokenAsync("https://" + relayConfig.Origin + ".servicebus.windows.net/" + relayConfig.RelayId, "GET", true, new TimeSpan(0, 20, 0)).Result as SharedAccessSignatureToken;

            _client = new HttpClient { BaseAddress = conString };
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

            Console.WriteLine("Started : " + startedStatus.Started);
            Console.Write(log.Content);

            while (!log.Finished)
            {
                result = _client.GetAsync(string.Format("RelayService/Runbook/Log/{0}/{1}", startedStatus.ExecutionId, log.End)).Result;
                log = result.Content.ReadAsAsync<ExecutionLog>().Result;

                Console.Write(log.Content);
            }

            Console.WriteLine();
            Console.WriteLine("Finished");

            //var address = ServiceBusEnvironment.CreateServiceUri("https", relayConfig.Origin, relayConfig.RelayId);
            //var messageHandler = new HttpClientHandler { Credentials = new NetworkCredential(userName, password) };
            //_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //var cf = new ChannelFactory<IOfferCommandRelay>(
            //    new WebHttpRelayBinding(EndToEndWebHttpSecurityMode.Transport, RelayClientAuthenticationType.RelayAccessToken),
            //    new EndpointAddress(ServiceBusEnvironment.CreateServiceUri("https", relayConfig.Origin, relayConfig.RelayId)));

            //cf.Endpoint.Behaviors.Add(new WebHttpBehavior());
            //cf.Endpoint.Behaviors.Add(
            //    new TransportClientEndpointBehavior
            //    {
            //        TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(relayConfig.AccessKey, relayConfig.AccessSecret)
            //    });

            ////Debugger.Launch();
            //using (var ch = cf.CreateChannel())
            //{
            //    //System.Console.WriteLine(ch.Ping());
            //    var status = ch.StartRunbook(artifactManifest.ConPack, artifactManifest.Apps.ToArray());//, deployOptions);

            //    status = ch.GetRunbookStatus(status.ExecutionId.ToString(), "0");
            //    System.Console.WriteLine(status.Log.Content);

            //    while (!status.Finished)
            //    {
            //        status = ch.GetRunbookStatus(status.ExecutionId.ToString(), status.Log.End.ToString());
            //        System.Console.Write(status.Log.Content);
            //    }
            //}
        } 
    }
}