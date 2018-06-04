using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Game
{
    internal static class Program
    {
        /// <summary>
        /// Questo è il punto di ingresso del processo host del servizio.
        /// </summary>
        private static void Main()
        {
            try
            {
                // Questa riga consente di registrare un Servizio Actor per ospitare la classe actor con il runtime di Service Fabric.
                // Il contenuto dei file ServiceManifest.xml e ApplicationManifest.xml
                // viene popolato automaticamente quando si compila questo progetto.
                // Per altre informazioni, vedere http://aka.ms/servicefabricactorsplatform

                ActorRuntime.RegisterActorAsync<Game>(
                   (context, actorType) => new ActorService(context, actorType, (a,b) => new Game(a,b))).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
