using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Game.Interfaces;

namespace Game
{
    /// <remarks>
    /// Questa classe rappresenta un actor.
    /// Ogni ActorID è associato a un'istanza di questa classe.
    /// L'attributo StatePersistence determina la persistenza e la replica dello stato dell'actor:
    ///  - Persisted: lo stato viene scritto su disco e replicato.
    ///  - Volatile: lo stato viene mantenuto solo in memoria e replicato.
    ///  - None: lo stato viene mantenuto solo in memoria e non viene replicato.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class Game : Actor, IGame
    {
        public Game(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }

        /// <summary>
        /// Questo metodo viene chiamato ogni volta che si attiva un actor.
        /// Un actor viene attivato la prima volta che viene richiamato uno dei relativi metodi.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorState state = this.StateManager.GetOrAddStateAsync("GameState", new ActorState()
            {
                Board = new int[9],
                Winner = "",
                Players = new List<Tuple<long, string>>(),
                NextPlayerIndex = 0,
                NumberOfMoves = 0
            }).Result;

            return Task.FromResult(true);
        }

        public async Task<bool> ResetGameAsync()
        {
            ActorState state = await StateManager.GetStateAsync<ActorState>("GameState");
            state.Board = new int[9];
            state.Winner = "";
            state.NextPlayerIndex = 0;
            state.NumberOfMoves = 0;
            await StateManager.AddOrUpdateStateAsync<ActorState>("GameState", state, (key, value) => state);
            return true;
        }

        public async Task<int[]> GetGameBoardAsync()
        {
            ActorState state = await StateManager.GetStateAsync<ActorState>("GameState");
            return state.Board;
        }

        public async Task<string> GetWinnerAsync()
        {
            ActorState state = await StateManager.GetStateAsync<ActorState>("GameState");
            return state.Winner;
        }

        public async Task<bool> JoinGameAsync(long playerId, string playerName)
        {
            ActorState state = await StateManager.GetStateAsync<ActorState>("GameState");

            if (state.Players.Count >= 2 || state.Players.FirstOrDefault(p => p.Item2 == playerName) != null)
                return false;

            state.Players.Add(new Tuple<long, string>(playerId, playerName));
            return true;
        }

        public async Task<bool> MakeMoveAsync(long playerId, int x, int y)
        {
            ActorState state = await StateManager.GetStateAsync<ActorState>("GameState");

            if (x < 0 || x > 2 || y < 0 || y > 2 || state.Players.Count != 2 || state.NumberOfMoves >= 9 || state.Winner != "") return false;

            int index = state.Players.FindIndex(p => p.Item1 == playerId);

            if (index == state.NextPlayerIndex)
            {
                if (state.Board[y * 3 + x] == 0)
                {
                    int piece = index * 2 - 1;
                    state.Board[y * 3 + x] = piece;
                    state.NumberOfMoves++;

                    if (await HasWon(piece * 3))
                        state.Winner = state.Players[index].Item2 + " (" + (piece == -1 ? "X" : "O") + ")";
                    else if (state.Winner == "" && state.NumberOfMoves >= 9)
                        state.Winner = "TIE";

                    state.NextPlayerIndex = (state.NextPlayerIndex + 1) % 2;

                    await StateManager.AddOrUpdateStateAsync<ActorState>("GameState", state, (key, value) => state);

                    return true;
                }
                else return false;
            }
            else return false;
        }

        private async Task<bool> HasWon(int sum)
        {
            ActorState state = await StateManager.GetStateAsync<ActorState>("GameState");

            return state.Board[0] + state.Board[1] + state.Board[2] == sum ||
                state.Board[3] + state.Board[4] + state.Board[5] == sum ||
                state.Board[6] + state.Board[7] + state.Board[8] == sum ||
                state.Board[0] + state.Board[3] + state.Board[6] == sum ||
                state.Board[1] + state.Board[4] + state.Board[7] == sum ||
                state.Board[2] + state.Board[5] + state.Board[8] == sum ||
                state.Board[0] + state.Board[4] + state.Board[8] == sum ||
                state.Board[2] + state.Board[4] + state.Board[6] == sum;
        }
    }
}
