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
using System.Web;
using PipelineExtensions;
using SimonSquared.Online.DataContracts;
using System.Linq;

namespace FlatlingsServer.Domain
{
    public class LevelDataRepository
    {
        private LevelTemplate[] _levels;

        public void LoadLevelData()
        {
            var fileName = HttpRuntime.AppDomainAppPath + "/Content/levels.csv";
            var loader = new LevelDataReader();
            _levels = loader.ReadLevelTemplates(fileName);
        }

        public LevelTemplate GetLevel(int number)
        {
            var actualNumber = number >= _levels.Length ? _levels.Length - 1 : number;
            return _levels[actualNumber];
        }
    }
}