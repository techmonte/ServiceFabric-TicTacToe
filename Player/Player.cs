﻿using System;
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
    /// This class represents an actor.
    /// Each ActorID is associated with an instance of this class.
    /// The StatePersistence attribute determines the persistence and replication of the actor's state:
    /// - Persisted: The status is written to disk and replicated.
    /// - Volatile: The state is kept only in memory and replicated.
    /// - None: The state is kept only in memory and is not replicated.
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
