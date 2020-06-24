using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Game
{
    [EventSource(Name = "MyCompany-ActorTicTacToeApplication-Game")]
    internal sealed class ActorEventSource : EventSource
    {
        public static readonly ActorEventSource Current = new ActorEventSource();

        static ActorEventSource()
        {
            // Workaround for the problem of not verifying ETW tasks until the task infrastructure is initialized.
            // This issue will be corrected in the.NET Framework 4.6.2.
           Task.Run(() => { });
        }

        // To apply singleton semantics, the instance constructor is private
        private ActorEventSource() : base() { }

        #region Parole chiave
        // You can use event keywords to organize events into categories. 
        // Each keyword is a bit flag.A single event can be associated with multiple keywords through the EventAttribute.Keywords property.
        // You must define keywords as a public class named 'Keywords' within the EventSource element in which they are used.
        public static class Keywords
        {
            public const EventKeywords HostInitialization = (EventKeywords)0x1L;
        }
        #endregion

        #region Events
        // Definire un metodo di istanza per ogni evento da registrare e al quale applicare un attributo [Event].
        // Il nome del metodo corrisponde al nome dell'evento.
        // Passare tutti i parametri che si vuole registrare con l'evento. Sono consentiti solo tipi integer primitivi, DateTime, Guid e string.
        // Ogni implementazione del metodo dell'evento deve verificare se l'origine evento è abilitata e, se lo è, chiamare il metodo WriteEvent() per generare l'evento.
        // Il numero e i tipi degli argomenti passati a ciascun metodo di evento devono essere identici a quelli passati a WriteEvent().
        // Inserire l'attributo [NonEvent] in tutti i metodi che non definiscono un evento.
        // Per altre informazioni, vedere https://msdn.microsoft.com/it-it/library/system.diagnostics.tracing.eventsource.aspx

        [NonEvent]
        public void Message(string message, params object[] args)
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                Message(finalMessage);
            }
        }

        private const int MessageEventId = 1;
        [Event(MessageEventId, Level = EventLevel.Informational, Message = "{0}")]
        public void Message(string message)
        {
            if (this.IsEnabled())
            {
                WriteEvent(MessageEventId, message);
            }
        }

        [NonEvent]
        public void ActorMessage(Actor actor, string message, params object[] args)
        {
            if (this.IsEnabled()
                && actor.Id != null
                && actor.ActorService != null
                && actor.ActorService.Context != null
                && actor.ActorService.Context.CodePackageActivationContext != null)
            {
                string finalMessage = string.Format(message, args);
                ActorMessage(
                    actor.GetType().ToString(),
                    actor.Id.ToString(),
                    actor.ActorService.Context.CodePackageActivationContext.ApplicationTypeName,
                    actor.ActorService.Context.CodePackageActivationContext.ApplicationName,
                    actor.ActorService.Context.ServiceTypeName,
                    actor.ActorService.Context.ServiceName.ToString(),
                    actor.ActorService.Context.PartitionId,
                    actor.ActorService.Context.ReplicaId,
                    actor.ActorService.Context.NodeContext.NodeName,
                    finalMessage);
            }
        }

        // Per generare eventi che si verificano molto frequentemente può risultare utile usare l'API WriteEventCore API.
        // In questo modo la gestione dei parametri risulta più efficace, ma è richiesta l'allocazione esplicita della struttura EventData e del codice unsafe.
        // Per abilitare questo percorso del codice, definire il simbolo di compilazione condizionale UNSAFE e attivare il supporto del codice unsafe nelle proprietà del progetto.
        private const int ActorMessageEventId = 2;
        [Event(ActorMessageEventId, Level = EventLevel.Informational, Message = "{9}")]
        private
#if UNSAFE
            unsafe
#endif
            void ActorMessage(
            string actorType,
            string actorId,
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
#if !UNSAFE
            WriteEvent(
                    ActorMessageEventId,
                    actorType,
                    actorId,
                    applicationTypeName,
                    applicationName,
                    serviceTypeName,
                    serviceName,
                    partitionId,
                    replicaOrInstanceId,
                    nodeName,
                    message);
#else
                const int numArgs = 10;
                fixed (char* pActorType = actorType, pActorId = actorId, pApplicationTypeName = applicationTypeName, pApplicationName = applicationName, pServiceTypeName = serviceTypeName, pServiceName = serviceName, pNodeName = nodeName, pMessage = message)
                {
                    EventData* eventData = stackalloc EventData[numArgs];
                    eventData[0] = new EventData { DataPointer = (IntPtr) pActorType, Size = SizeInBytes(actorType) };
                    eventData[1] = new EventData { DataPointer = (IntPtr) pActorId, Size = SizeInBytes(actorId) };
                    eventData[2] = new EventData { DataPointer = (IntPtr) pApplicationTypeName, Size = SizeInBytes(applicationTypeName) };
                    eventData[3] = new EventData { DataPointer = (IntPtr) pApplicationName, Size = SizeInBytes(applicationName) };
                    eventData[4] = new EventData { DataPointer = (IntPtr) pServiceTypeName, Size = SizeInBytes(serviceTypeName) };
                    eventData[5] = new EventData { DataPointer = (IntPtr) pServiceName, Size = SizeInBytes(serviceName) };
                    eventData[6] = new EventData { DataPointer = (IntPtr) (&partitionId), Size = sizeof(Guid) };
                    eventData[7] = new EventData { DataPointer = (IntPtr) (&replicaOrInstanceId), Size = sizeof(long) };
                    eventData[8] = new EventData { DataPointer = (IntPtr) pNodeName, Size = SizeInBytes(nodeName) };
                    eventData[9] = new EventData { DataPointer = (IntPtr) pMessage, Size = SizeInBytes(message) };

                    WriteEventCore(ActorMessageEventId, numArgs, eventData);
                }
#endif
        }

        private const int ActorHostInitializationFailedEventId = 3;
        [Event(ActorHostInitializationFailedEventId, Level = EventLevel.Error, Message = "Actor host initialization failed", Keywords = Keywords.HostInitialization)]
        public void ActorHostInitializationFailed(string exception)
        {
            WriteEvent(ActorHostInitializationFailedEventId, exception);
        }
        #endregion

        #region Private Method
#if UNSAFE
            private int SizeInBytes(string s)
            {
                if (s == null)
                {
                    return 0;
                }
                else
                {
                    return (s.Length + 1) * sizeof(char);
                }
            }
#endif
        #endregion
    }
}
