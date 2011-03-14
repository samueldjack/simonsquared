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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using Autofac;
using AutoMapper;
using SimonSquared.Online.DataContracts;

namespace FlatlingsServer.Domain
{
    public class Game
    {
        private readonly IComponentContext _componentContext;
        private Scoreboard _scoreboard = new Scoreboard();

        Dictionary<string, Player> _players = new Dictionary<string, Player>();
        ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim();

        private DateTime _lastActivity;

        public Game(IComponentContext componentContext)
        {
            _componentContext = componentContext;
            TransitionToState(typeof(WaitingForPlayersState));
        }

        public bool IsInactive
        {
            get
            {
                _stateLock.EnterReadLock();
        
                try
                {
                    return (DateTime.UtcNow - _lastActivity).TotalMinutes > InactivityTime;
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        private void MarkAsActive()
        {
           _stateLock.EnterWriteLock();
           try
           {
               _lastActivity = DateTime.UtcNow;
           }
            finally
           {
               _stateLock.ExitWriteLock();
           }
        }

        public void ProcessUpdate(GameStatusUpdate update)
        {
            _stateLock.EnterWriteLock();
    
            try
            {
                _currentState.ProcessUpdate(update);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }

            MarkAsActive();
        }

        internal void TransitionToState(Type stateType)
        {
            if (_stateLock.IsWriteLockHeld)
            {
                TransitionToStateCore(stateType);
            }
            else
            {
                _stateLock.EnterWriteLock();
                try
                {
                    TransitionToStateCore(stateType);
                }
                finally
                {
                    _stateLock.ExitWriteLock();
                }
            }
        }

        private void TransitionToStateCore(Type stateType)
        {
            var gameState = _componentContext.Resolve(stateType, new TypedParameter(typeof(Game),this)) as GameState;
            var stateId = 0;

            if (_currentState != null)
            {
                _currentState.Leave();
                stateId = _currentState.Id + 1;
            }

            _currentState = gameState;
            _currentState.Id = stateId;

            gameState.Enter();
        }

        public Guid Id { get; set; }

        public string OwnerName { get; set; }

        public string Name { get; set; }

        private GameState _currentState;
        
        public GameStatusDto GetStateSnapshot()
        {
            _stateLock.EnterReadLock();

            try
            {
                var snapshot = Mapper.Engine.Map<GameState, GameStatusDto>(_currentState);
                return snapshot;
            }
            finally
            {
                _stateLock.ExitReadLock();
            }

            MarkAsActive();
        }

        public ScoreboardDto GetScoreSnapshot()
        {
            _stateLock.EnterReadLock();
            try
            {
                var snapshot = Mapper.Engine.Map<Scoreboard, ScoreboardDto>(Scoreboard);
                return snapshot;
            }
            finally
            {
                _stateLock.ExitReadLock();
            }
            MarkAsActive();
        }

        public void AddPlayer(Player player)
        {
            _stateLock.EnterWriteLock();
            try
            {

                _players.Add(player.Id.ToString(), player); 
                Scoreboard.AddPlayer(player);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
            MarkAsActive();
        }

        public ICollection<Player> Players
        {
            get { return _players.Values.ToList(); }
        }

        public ICollection<Player> GetPlayersSnapshot()
        {
            _stateLock.EnterReadLock();
            try
            {
                return _players.Values.ToList();
            }
            finally
            {
                _stateLock.ExitReadLock();
            }
            MarkAsActive();
        }

        public bool IsJoinable { get; internal set; }

        private Round _round;
        private string _ownerId;
        private static readonly int InactivityTime = 1;

        internal Round Round
        {
            get { return _round; }
            set { _round = value; }
        }

        public void SetOwner(Player owner)
        {
            AddPlayer(owner);
            _ownerId = owner.Id.ToString();
        }

        internal int CurrentPuzzle { get; set; }

        internal Scoreboard Scoreboard
        {
            get { return _scoreboard; }
        }

        public void RemovePlayer(string playerId)
        {
            _stateLock.EnterWriteLock();
            try
            {
                _players.Remove(playerId);
                Scoreboard.RemovePlayer(playerId);

                if (_players.Count == 0)
                {
                    TransitionToStateCore(typeof(GameAbandonedState));
                }
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }

            MarkAsActive();
        }
    }
}