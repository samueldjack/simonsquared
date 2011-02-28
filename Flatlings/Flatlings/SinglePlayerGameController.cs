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
using Flatlings.Animations;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Xna.Framework;

namespace Flatlings
{
    class SinglePlayerGameController : GameController
    {
        private int _currentLevelNumber= 35;
        private double _hardness;
        private decimal _numberOfNailedPuzzlesInARow;
        private decimal _score;
        private Puzzle _currentPuzzle;

        public SinglePlayerGameController(Storyboard storyboard, CommonContentManager contentManager, Messenger messenger) : base(storyboard, contentManager, messenger)
        {
        }

        public override Puzzle GetNextPuzzle()
        {
            var puzzle = Puzzle.CreateFromLevelTemplate(ContentManager.Levels[_currentLevelNumber],
                                                            new PuzzleCreationContext()
                                                            {
                                                                Palette = ContentManager.Palette,
                                                                Storyboard = Storyboard,
                                                                TileModel = ContentManager.BaseCubeModel,
                                                                LayoutSize = new Vector2(70, 135),
                                                                Hardness = _hardness,
                                                            });

            _currentLevelNumber++;
            _hardness += 0.05;

            if (_currentLevelNumber >= ContentManager.Levels.Length)
            {
                _currentLevelNumber = 0;
            }

            _currentPuzzle = puzzle;
            return puzzle;
        }

        public override void PuzzleCompleted()
        {
            UpdateScore(_currentPuzzle);
            PublishGameEvent(Flatlings.GameEvents.Won);
            PublishGameEvent(Flatlings.GameEvents.StartingNewGame);
        }

        public override int Score
        {
            get { return (int)_score; }
        }

        private void UpdateScore(Puzzle currentPuzzle)
        {
            if (currentPuzzle.TargetMoveCount == currentPuzzle.PlayerMoveCount)
            {
                _numberOfNailedPuzzlesInARow++;
            }
            else
            {
                _numberOfNailedPuzzlesInARow = 1;
            }

            var basicPoint = _numberOfNailedPuzzlesInARow * _numberOfNailedPuzzlesInARow;
            var scoreForGame = (2 * currentPuzzle.TargetMoveCount - currentPuzzle.PlayerMoveCount) * basicPoint;
            scoreForGame = 10 + (scoreForGame < 0 ? 0 : scoreForGame);

            _score += scoreForGame;
        }

        public override void Begin()
        {
            _hardness = 0.00;
            PublishGameEvent(Flatlings.GameEvents.StartingNewGame);
            IsPlaying = true;
        }

        public override void End()
        {
            IsPlaying = false;
        }
    }
}
