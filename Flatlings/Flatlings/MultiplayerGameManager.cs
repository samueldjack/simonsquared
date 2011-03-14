#region License
// This file is part of Simon Squared
// 
// Simon Squared is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Simon Squared is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
// along with Simon Squared. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonSquared.Online.DataContracts;
using Microsoft.Phone.Reactive;

namespace Flatlings
{
    public class MultiplayerGameManager
    {
        private GameServerClient _client;
        private Microsoft.Phone.Reactive.BehaviorSubject<MultiplayerGame> _multiplayerGameProperty = new Microsoft.Phone.Reactive.BehaviorSubject<MultiplayerGame>(null);
        private PlayerDto _currentPlayer;

        public MultiplayerGameManager()
        {
            _client = new GameServerClient();
        }

        public void SetPlayerName(string name)
        {
            Name = name;
            _currentPlayer = new PlayerDto() {Id = Guid.NewGuid().ToString(), Name = name};
        }

        public void StartNewGame(string gameName)
        {
            AbandonAnyPreviousGame();

            _client.StartGame(gameName, _currentPlayer)
                .Subscribe(gameResource =>
                               {
                                   if (gameResource != null)
                                   {
                                       CurrentGame = new MultiplayerGame(gameResource.Name, gameResource.Id, GameRole.Initiator, _client, _currentPlayer);
                                   }
                               });
        }

        private void AbandonAnyPreviousGame()
        {
            if (CurrentGame != null)
            {
                CurrentGame.LeaveGame();
            }
            CurrentGame = null;
        }

        public void JoinGame(string gameId)
        {
            CurrentGame = null;

            _client.GetGame(gameId)
                .Subscribe(game =>
                               {
                                   if (game != null)
                                   {
                                       CurrentGame = new MultiplayerGame(game.Name, game.Id, GameRole.Player, _client, _currentPlayer);
                                       _client.JoinGame(gameId, _currentPlayer);
                                   }
                               });
        }

        public string Name { get; private set; }

        public MultiplayerGame CurrentGame
        {
            get { return _multiplayerGameProperty.First(); }
            private set { _multiplayerGameProperty.OnNext(value); }
        }

        public IObservable<MultiplayerGame> CurrentGameObservable { get { return _multiplayerGameProperty; } }

        public IObservable<List<GameDto>> ListAvailableGames()
        {
            return _client.ListAvailableGames();
        }

        public void LeaveGame()
        {
            AbandonAnyPreviousGame();
        }
    }
}
