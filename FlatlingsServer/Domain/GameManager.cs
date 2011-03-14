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
using System.Collections.Concurrent;
using Autofac;

namespace FlatlingsServer.Domain
{
    public class GameManager
    {
        private readonly Func<Game> _gameFactory;
        ConcurrentDictionary<Guid, Game> _games = new ConcurrentDictionary<Guid, Game>();

        public GameManager(Func<Game> gameFactory)
        {
            _gameFactory = gameFactory;
            BeginGarbageCollector();
        }

        private void BeginGarbageCollector()
        {
            Observable.Interval(TimeSpan.FromMinutes(1))
                .Subscribe(_ => RemoveGarbageGames());
        }

        private void RemoveGarbageGames()
        {
            foreach (var game in _games.Values)
            {
                if (game.IsInactive)
                {
                    Game removedGame;
                    _games.TryRemove(game.Id, out removedGame);
                }
            }
        }

        public Game AddGame(string gameName, Player owner)
        {
            var game = _gameFactory();
            game.Id = Guid.NewGuid();
            game.Name = gameName;

            game.SetOwner(owner);

            _games.TryAdd(game.Id, game);

            return game;
        }

        public IList<Game> GetAll()
        {
            return _games.Values.ToArray();
        }

        public Game FindGame(Guid gameId)
        {
            Game game;
            return _games.TryGetValue(gameId, out game) ? game : null;
        }
    }
}
