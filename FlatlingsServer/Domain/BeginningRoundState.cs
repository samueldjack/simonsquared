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
using System.Linq;
using SimonSquared.Online.DataContracts;

namespace FlatlingsServer.Domain
{
    public class BeginningRoundState : GameState
    {
        private readonly PuzzleGenerator _puzzleGenerator;

        public BeginningRoundState(Game game, PuzzleGenerator puzzleGenerator) : base(game)
        {
            _puzzleGenerator = puzzleGenerator;
        }

        public override void Enter()
        {
            base.Enter();

            StartTimeUtc = DateTime.UtcNow + TimeSpan.FromSeconds(4);
            Game.Round = _puzzleGenerator.GenerateRound(Game.Scoreboard.RoundsPlayed);
            Game.CurrentPuzzle = -1;

            Observable.Timer(StartTimeUtc).Subscribe(value => Game.TransitionToState(typeof (BeginningPuzzleState)));
        }

        public DateTime StartTimeUtc { get; private set; }
    }
}