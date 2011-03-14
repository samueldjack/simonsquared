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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Phone.Reactive;
using Microsoft.Xna.Framework;
using SimonSquared.Online.DataContracts;

namespace Flatlings
{
    public enum GameRole
    {
        Initiator,
        Player
    }

    public enum MultiplayerGameState
    {
        WaitingForPlayers,
        BeginningRound,
        PlayingRoundBeginningPuzzle,
        PlayingRoundSolvingPuzzle,
        PlayingRoundWonPuzzle,
        PlayingRoundLostPuzzle,
        RoundFinished,
        GameFinished,
    }

    public class MultiplayerGame
    {
        private readonly string _gameId;
        private readonly GameRole _role;
        private readonly GameServerClient _gameServerClient;
        private readonly PlayerDto _player;
        private BehaviorSubject<GameStatusDto> _serverGameStatus = new BehaviorSubject<GameStatusDto>(new WaitingForPlayersStateDto());
        private BehaviorSubject<MultiplayerGameState> _gameState = new BehaviorSubject<MultiplayerGameState>(MultiplayerGameState.WaitingForPlayers);
        private StateHandler _stateHandler;
        private GameStatusDto _lastState;
        private IDisposable _statusSubscription;
        private BehaviorSubject<ScoreboardDto> _scoreboard = new BehaviorSubject<ScoreboardDto>(null);

        protected GameServerClient GameServerClient
        {
            get { return _gameServerClient; }
        }

        public string Name { get; private set; }

        public MultiplayerGame(string name, string gameId, GameRole role, GameServerClient gameServerClient, PlayerDto player)
        {
            _gameId = gameId;
            _role = role;
            _gameServerClient = gameServerClient;
            _player = player;
            Name = name;

            Players = new ObservableCollection<PlayerDto>();

            TransitionToState(typeof(WaitingForPlayersState));
            StartListeningToGameState();
        }

        private void StartListeningToGameState()
        {
            _statusSubscription = (from interval in Observable.Interval(TimeSpan.FromSeconds(1))
                                    from gameStatus in GameServerClient.GetGameStatus(_gameId)
                                    select gameStatus)
                                    .Where(status => status != null)
                                    .DistinctUntilChanged(status => status.Id)
                                    .Subscribe(PassStatusToStateHandler);
        }

        private void PassStatusToStateHandler(GameStatusDto status)
        {
            LastState = status;
            if (_stateHandler != null)
            {
                _stateHandler.HandleServerStateChange(status);
            }    
        }

        public void StartGame()
        {
            _gameServerClient.PostStatusUpdate(_gameId, new BeginGameUpdate());
        }

        public void LeaveGame()
        {
            _gameServerClient.LeaveGame(_gameId, _player.Id);

            if (_statusSubscription != null)
            {
                _statusSubscription.Dispose();
            }

            TransitionToState(typeof(AbandonedGame));
        }

        private void TransitionToState(Type stateHandlerType)
        {
            var state = Activator.CreateInstance(stateHandlerType, this) as StateHandler;

            if (CurrentStateHandler != null)
            {
                CurrentStateHandler.Leave();
            }

            CurrentStateHandler = state;

            CurrentStateHandler.Enter();
        }

        protected StateHandler CurrentStateHandler
        {
            get { return _stateHandler; }
            private set
            {
                _stateHandler = value;
            }
        }
        public ObservableCollection<PlayerDto> Players { get; private set; }

        public GameRole Role
        {
            get { return _role; }
        }

        protected IObservable<GameStatusDto> ServerGameStatus
        {
            get { return _serverGameStatus; }
        }

        public MultiplayerGameState GameState
        {
            get { return _gameState.First(); }
            set
            {
                _gameState.OnNext(value);
            }
        }
        public BehaviorSubject<MultiplayerGameState> GameStateObservable
        {
            get { return _gameState; }
        }

        protected DateTimeOffset UnadjustedStartTime { get; set; }

        public DateTimeOffset StartTimeUtc { get { return UnadjustedStartTime - ServerClockSkew; }}

        public TimeSpan ServerClockSkew { get; set; }
        protected Round RoundData { get; private set; }

        public PuzzleDto CurrentPuzzle { get; private set; }

        protected int CurrentRoundIndex { get; private set; }

        public GameStatusDto LastState
        {
            get { return _lastState; }
            set { _lastState = value; }
        }

        public IObservable<ScoreboardDto> Scoreboard
        {
            get { return _scoreboard; }
        }

        public void PuzzleCompleted()
        {
            _gameServerClient.PostStatusUpdate(_gameId, new PuzzleCompletedUpdate() { PlayerId = _player.Id });
        }


        public class StateHandler
        {
            private readonly MultiplayerGame _parent;
            private ResourceCleaner _resourceCleaner  = new ResourceCleaner();

            public StateHandler(MultiplayerGame parent)
            {
                _parent = parent;
            }

            protected MultiplayerGame Parent
            {
                get { return _parent; }
            }

            public virtual void Enter()
            {
                
            }

            public virtual void Leave()
            {
                _resourceCleaner.CleanUp();
            }

            public virtual void HandleServerStateChange(GameStatusDto state)
            {
                
            }

            protected void DisposeOnLeave(IDisposable disposable)
            {
                _resourceCleaner.AddResourceRequiringCleanup(disposable);
            }
        }

        private class WaitingForPlayersState : StateHandler
        {
            public WaitingForPlayersState(MultiplayerGame parent) : base(parent)
            {
            }

            public override void Enter()
            {
                var updatingPlayerList = (from interval in Observable.Interval(TimeSpan.FromSeconds(1))
                                    from playerList in Parent.GameServerClient.ListPlayers(Parent._gameId)
                                    select playerList);

                var playerListSubscription = updatingPlayerList.Subscribe(RefreshPlayerList);
                DisposeOnLeave(playerListSubscription);

                Parent.GameServerClient.DetermineClockSkew().Subscribe(clockSkew => Parent.ServerClockSkew = clockSkew);

                Parent.GameState = MultiplayerGameState.WaitingForPlayers;
            }

            public override void HandleServerStateChange(GameStatusDto state)
            {
                if (state is RoundStartingStateDto)
                {
                    Parent.TransitionToState(typeof(GameStartingState));
                }
            }
            
            private void RefreshPlayerList(List<PlayerDto> players)
            {
                Parent.Players.Clear();

                foreach (var playerDto in players)
                {
                    Parent.Players.Add(playerDto);
                }
            }

        }

        private class GameStartingState : StateHandler
        {
            public GameStartingState(MultiplayerGame parent)
                : base(parent)
            {

            }

            public override void Enter()
            {
                var gameStartingState = Parent.LastState as RoundStartingStateDto;
                
                Parent.UnadjustedStartTime = gameStartingState.StartTimeUtc;
                Parent.GameServerClient.GetGameRound(Parent._gameId).Subscribe(round => Parent.RoundData = round); 
            
                Parent.GameState = MultiplayerGameState.BeginningRound;
            }

            public override void HandleServerStateChange(GameStatusDto state)
            {
               if (state is BeginningPuzzleStateDto)
               {
                   Parent.TransitionToState(typeof (PuzzleStartingState));
               }
            }
        }

        private class PuzzleStartingState : StateHandler
        {
            public PuzzleStartingState(MultiplayerGame parent) : base(parent)
            {
            }

            public override void Enter()
            {
                base.Enter();

                var puzzleStartingState = Parent.LastState as BeginningPuzzleStateDto;

                Parent.UnadjustedStartTime = puzzleStartingState.StartTimeUtc;
                Parent.GameState = MultiplayerGameState.PlayingRoundBeginningPuzzle;
                Parent.CurrentRoundIndex = puzzleStartingState.CurrentPuzzle;
                Parent.CurrentPuzzle = Parent.RoundData.Puzzles[Parent.CurrentRoundIndex];

                Observable.Timer(Parent.StartTimeUtc)
                    .Subscribe(time => Parent.TransitionToState(typeof (SolvingPuzzleState)));
            }
        }

        private class SolvingPuzzleState : StateHandler
        {
            public SolvingPuzzleState(MultiplayerGame parent) : base(parent)
            {
            }

            public override void Enter()
            {
                base.Enter();

                Parent.GameState = MultiplayerGameState.PlayingRoundSolvingPuzzle;
            }

            public override void HandleServerStateChange(GameStatusDto state)
            {
                base.HandleServerStateChange(state);

                if (state is BeginningPuzzleStateDto)
                {
                    var beginningPuzzleState = (state as BeginningPuzzleStateDto);
                    if (beginningPuzzleState.WinnerOfPreviousPuzzle != null)
                    {
                        if (beginningPuzzleState.WinnerOfPreviousPuzzle == Parent._player.Id)
                        {
                            Parent.GameState = MultiplayerGameState.PlayingRoundWonPuzzle;
                        }
                        else
                        {
                            Parent.GameState = MultiplayerGameState.PlayingRoundLostPuzzle;
                        }
                    }
                    Parent.TransitionToState(typeof (PuzzleStartingState));
                }
                else if (state is RoundCompletedStateDto)
                {
                    Parent.TransitionToState(typeof(RoundEndedState));
                }
            }
        }

        private class RoundEndedState : StateHandler
        {
            public RoundEndedState(MultiplayerGame parent)
                : base(parent)
            {
            }

            public override void Enter()
            {
                base.Enter();

                Parent.GameState = MultiplayerGameState.RoundFinished;

                Parent.GameServerClient.GetGameScore(Parent._gameId)
                    .Subscribe(UpdateScoreboard);
            }

            public override void HandleServerStateChange(GameStatusDto state)
            {
                base.HandleServerStateChange(state);

                if (state is RoundStartingStateDto)
                {
                    Parent.TransitionToState(typeof(GameStartingState));
                }
            }

            private void UpdateScoreboard(ScoreboardDto scoreboard)
            {
               Parent._scoreboard.OnNext(scoreboard);
            }
        }

        private class AbandonedGame : StateHandler
        {
            public AbandonedGame(MultiplayerGame parent) : base(parent)
            {
            }
        }
    }

    
}
