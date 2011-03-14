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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimonSquared.Online.DataContracts;

namespace FlatlingsServer.Domain
{
    public class WaitingForPlayersState : GameState
    {
        public WaitingForPlayersState(Game game) : base(game)
        {
        }

        private int _startCount;

        public override void Enter()
        {
            Game.IsJoinable = true;
            base.Enter();
        }

        public override void ProcessUpdate(SimonSquared.Online.DataContracts.GameStatusUpdate update)
        {
            if (update is BeginGameUpdate)
            {
                _startCount++;
            }

            if (Game.Players.Count >= 1 && _startCount >= 1)
            {
                Game.IsJoinable = false;
                Game.TransitionToState(typeof (BeginningRoundState));
            }
        }
    }
}