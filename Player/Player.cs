using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Player.Interfaces;
using Game.Interfaces;

namespace Player
{
    /// <remarks>
    /// Questa classe rappresenta un actor.
    /// Ogni ActorID è associato a un'istanza di questa classe.
    /// L'attributo StatePersistence determina la persistenza e la replica dello stato dell'actor:
    ///  - Persisted: lo stato viene scritto su disco e replicato.
    ///  - Volatile: lo stato viene mantenuto solo in memoria e replicato.
    ///  - None: lo stato viene mantenuto solo in memoria e non viene replicato.
    /// </remarks>
    [StatePersistence(StatePersistence.None)]
    internal class Player : Actor, IPlayer
    {
        public Player(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }

        public Task<bool> JoinGameAsync(ActorId gameId, string playerName)
        {
            var game = ActorProxy.Create<IGame>(gameId, "fabric:/ActorTicTacToeApplication");
            return game.JoinGameAsync(this.Id.GetLongId(), playerName);
        }

        public Task<bool> MakeMoveAsync(ActorId gameId, int x, int y)
        {
            var game = ActorProxy.Create<IGame>(gameId, "fabric:/ActorTicTacToeApplication");
            return game.MakeMoveAsync(this.Id.GetLongId(), x, y);
        }

    }
}
