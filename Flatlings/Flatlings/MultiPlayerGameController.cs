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
using Flatlings.Messages;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Reactive;
using Microsoft.Xna.Framework;

namespace Flatlings
{
    
    public class MultiPlayerGameController : GameController
    {
        private readonly MultiplayerGameManager _gameManager;
        private ResourceCleaner _gameSubscriptions = new ResourceCleaner();
        private MultiplayerGame _game;
        private bool _notifiedOfCompletion;
        
        public MultiPlayerGameController(MultiplayerGameManager gameManager, Storyboard storyboard, CommonContentManager contentManager, Messenger messenger) : base(storyboard, contentManager, messenger)
        {
            _gameManager = gameManager;
        }

        public override void Begin()
        {
            base.Begin();

            _gameManager.CurrentGameObservable.Subscribe(HandleGameChanged);
            IsPlaying = true;
        }

        public override void End()
        {
            base.End();

            IsPlaying = false;
        }

        public override int Score
        {
            get { return 0; }
        }

        private void HandleGameChanged(MultiplayerGame game)
        {
            _gameSubscriptions.CleanUp();
            _game = game;

            if (game != null)
            {
                HandleState(game, MultiplayerGameState.BeginningRound,
                            () => PublishGameEvent(Flatlings.GameEvents.Waiting));
                HandleState(game, MultiplayerGameState.PlayingRoundBeginningPuzzle,
                            () => PublishGameEvent(Flatlings.GameEvents.Countdown));
                HandleState(game, MultiplayerGameState.PlayingRoundWonPuzzle,
                            () => PublishGameEvent(Flatlings.GameEvents.Won));
                HandleState(game, MultiplayerGameState.PlayingRoundLostPuzzle,
                            () => PublishGameEvent(Flatlings.GameEvents.Lost));
                HandleState(game, MultiplayerGameState.PlayingRoundSolvingPuzzle,
                            () =>
                                {
                                    _notifiedOfCompletion = false;
                                    PublishGameEvent(Flatlings.GameEvents.StartingNewGame);
                                });
                HandleState(game, MultiplayerGameState.RoundFinished,
                    () => Messenger.Send(new TransitionToScreenMessage() { ScreenName = "Scoreboard"}));
            }
        }

        public override void PuzzleCompleted()
        {
            base.PuzzleCompleted();

            if (!_notifiedOfCompletion)
            {
                _game.PuzzleCompleted();
                _notifiedOfCompletion = true;
            }
        }

        private void HandleState(MultiplayerGame game, MultiplayerGameState multiplayerGameState, Action action)
        {
            var subscription = game.GameStateObservable.Where(state => state == multiplayerGameState)
                .Subscribe(state => action());

            _gameSubscriptions.AddResourceRequiringCleanup(subscription);
        }

        public override Puzzle GetNextPuzzle()
        {
            var puzzleData = _game.CurrentPuzzle;

            var puzzle = Puzzle.CreateFromPuzzleData(puzzleData, new PuzzleCreationContext()
                                                            {
                                                                Hardness = 0,
                                                                Palette = ContentManager.Palette,
                                                                Storyboard = Storyboard,
                                                                LayoutSize = new Vector2(70, 140),
                                                                TileModel = ContentManager.BaseCubeModel
                                                            });

            return puzzle;
        }

        public override TimeSpan CountdownTimeRemaining
        {
            get
            {
                return _game.StartTimeUtc - DateTimeOffset.UtcNow;
            }
        }
    }
}
