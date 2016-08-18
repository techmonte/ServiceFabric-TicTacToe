using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace Game.Interfaces
{
    /// <summary>
    /// Questa interfaccia consente di definire i metodi esposti da un actor.
    /// I client usano questa interfaccia per interagire con l'actor che la implementa.
    /// </summary>
    public interface IGame : IActor
    {
        Task<bool> JoinGameAsync(long playerId, string playerName);
        Task<int[]> GetGameBoardAsync();
        Task<string> GetWinnerAsync();
        Task<bool> MakeMoveAsync(long playerId, int x, int y);
        Task<bool> ResetGameAsync();
    }
}
