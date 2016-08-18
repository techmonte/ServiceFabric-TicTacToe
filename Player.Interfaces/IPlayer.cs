using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace Player.Interfaces
{
    /// <summary>
    /// Questa interfaccia consente di definire i metodi esposti da un actor.
    /// I client usano questa interfaccia per interagire con l'actor che la implementa.
    /// </summary>
    public interface IPlayer : IActor
    {
        Task<bool> JoinGameAsync(ActorId gameId, string playerName);
        Task<bool> MakeMoveAsync(ActorId gameId, int x, int y);
    }
}
