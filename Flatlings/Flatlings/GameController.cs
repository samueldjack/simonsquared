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
using Microsoft.Phone.Reactive;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SimonSquared.Online.DataContracts;

namespace Flatlings
{
    public enum GameEvents
    {
        Waiting,
        Countdown,
        StartingNewGame,
        Won,
        Lost,
    }

    public abstract class GameController
    {
        private readonly Storyboard _storyboard;
        private readonly CommonContentManager _contentManager;
        private readonly Messenger _messenger;
        private Subject<GameEvents> _gameEventsSubject = new Subject<GameEvents>();

        public GameController(Storyboard storyboard, CommonContentManager contentManager, Messenger messenger)
        {
            LastGameEvent = Flatlings.GameEvents.Waiting;
            _storyboard = storyboard;
            _contentManager = contentManager;
            _messenger = messenger;
        }

        public abstract Puzzle GetNextPuzzle();

        public virtual void PuzzleCompleted()
        {
        }

        public virtual void Begin()
        {
            
        }

        public virtual void Pause()
        {
            
        }

        public virtual void Resume()
        {
            
        }

        public virtual void End()
        {
            
        }

        public virtual TimeSpan CountdownTimeRemaining { get { return new TimeSpan();} }

        public abstract int Score { get; }

        protected void PublishGameEvent(GameEvents gameEvent)
        {
            _gameEventsSubject.OnNext(gameEvent);
            LastGameEvent = gameEvent;
        }

        public IObservable<GameEvents> GameEvents { get { return _gameEventsSubject; } }


        public Storyboard Storyboard
        {
            get { return _storyboard; }
        }

        public CommonContentManager ContentManager
        {
            get { return _contentManager; }
        }

        public GameEvents LastGameEvent { get; private set; }

        public Messenger Messenger
        {
            get { return _messenger; }
        }

        public bool IsPlaying { get; protected set; }
    }
}
