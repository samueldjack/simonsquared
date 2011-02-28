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
using Microsoft.Xna.Framework;

namespace Flatlings
{
    public class Palette
    {
        private Dictionary<int,Color> _colorDictionary;

        public Palette(IEnumerable<Color> colors)
        {
            _colorDictionary =
                colors.Select((color, index) => new {Index = index + 1, Color = color}).ToDictionary(a => a.Index,
                                                                                                     a => a.Color);
        }

        public Color GetTileColor(int index)
        {
            return _colorDictionary[index];
        }
    }
}