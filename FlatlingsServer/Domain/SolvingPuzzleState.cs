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
using System.Web;
using SimonSquared.Online.DataContracts;

namespace FlatlingsServer.Domain
{
    public class SolvingPuzzleState : GameState
    {
        public SolvingPuzzleState(Game game) : base(game)
        {
        }

        public override void ProcessUpdate(SimonSquared.Online.DataContracts.GameStatusUpdate update)
        {
            base.ProcessUpdate(update);

            if (update is PuzzleCompletedUpdate)
            {
                Game.Scoreboard.PuzzleWon((update as PuzzleCompletedUpdate).PlayerId);

                if (Game.CurrentPuzzle < Game.Round.Puzzles.Count - 1)
                {
                    Game.TransitionToState(typeof(BeginningPuzzleState));
                }
                else
                {
                    Game.Scoreboard.RoundCompleted();
                    Game.TransitionToState(typeof(RoundEndedState));
                }
            }
        }

        public override void Leave()
        {
            base.Leave();
        }
    }
}