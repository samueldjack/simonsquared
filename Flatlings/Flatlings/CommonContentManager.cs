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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SimonSquared.Online.DataContracts;

namespace Flatlings
{
    public class CommonContentManager : IGameComponent
    {
        private readonly Game _game;
        private Model _baseCubeModel;
        private LevelTemplate[] _levels;
        private Palette _palette;
        private Texture2D _logo;

        public CommonContentManager(Game game)
        {
            _game = game;
        }

        /// <summary>
        /// Called when the component should be initialized. This method can be used for tasks like querying for services the component needs and setting up non-graphics resources.
        /// </summary>
        public void Initialize()
        {
            LoadContent(_game.Content);
        }

        public Model BaseCubeModel
        {
            get { return _baseCubeModel; }
        }

        public LevelTemplate[] Levels
        {
            get { return _levels; }
        }

        public Palette Palette
        {
            get { return _palette; }
        }

        public Texture2D Logo
        {
            get { return _logo; }
        }

        protected void LoadContent(ContentManager contentManager)
        {
            _baseCubeModel = contentManager.Load<Model>("Models\\Tile");
            _levels = contentManager.Load<LevelTemplate[]>("Levels\\Levels").OrderBy(l => l.Shapes.Length).ToArray();
            _logo = contentManager.Load<Texture2D>("Images\\SimonSquaredLogo200x200");
            _palette = CreateDefaultPalette();
        }

        private Palette CreateDefaultPalette()
        {
            return new Palette(new[]
                                   {
                                       new Color(255, 0, 0), new Color(0, 255, 0), new Color(0, 0, 255),
                                       new Color(255, 255, 0),
                                       new Color(255, 0, 0), new Color(0, 255, 0), new Color(0, 0, 255),
                                       new Color(255, 255, 0),
                                   });
        }
    }
}
