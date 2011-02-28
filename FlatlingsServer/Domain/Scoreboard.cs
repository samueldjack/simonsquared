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

namespace FlatlingsServer.Domain
{
    public class Scoreboard
    {
        public Dictionary<string, PlayerScore> _scores = new Dictionary<string, PlayerScore>();

        public void AddPlayer(Player player)
        {
            _scores.Add(player.Id.ToString(), new PlayerScore()
                                       {
                                           PlayerId = player.Id, 
                                           PlayerName = player.Name
                                       });
        }

        public void PuzzleWon(string playerId)
        {
            var player = _scores[playerId];
            player.Score++;
            MostRecentWinnerId = playerId;
        }

        public void RoundCompleted()
        {
            RoundsPlayed++;
        }

        public int RoundsPlayed { get; private set; }

        public IList<PlayerScore> Scores
        {
            get { return _scores.Values.ToList(); }
        }

        public string MostRecentWinnerId { get; private set; }
    }
}