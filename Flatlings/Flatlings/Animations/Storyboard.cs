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

namespace Flatlings.Animations
{
    public class Storyboard
    {
        List<Animation> _animations = new List<Animation>();
        private TimeSpan _currentElapsedTime;
        private TimeSpan _lastPurged = TimeSpan.MinValue;
        private StoryboardPlanner _currentPlan;

        public TimeSpan CurrentTime
        {
            get { return _currentElapsedTime; }
        }

        public void BeginAnimationNow(Animation animation)
        {
            animation.StartTime = _currentElapsedTime;
            _animations.Add(animation);
        }

        public StoryboardPlanner Plan()
        {
            if (_currentPlan != null)
            {
                return _currentPlan;
            }
            else
            {
                return new StoryboardPlanner(this);
            }
        }

        public void Update()
        {
            foreach (var animation in _animations)
            {
                animation.UpdateForTime(_currentElapsedTime);
            }

            if (_currentElapsedTime > _lastPurged + TimeSpan.FromSeconds(1))
            {
                PurgeExpiredAnimations(_currentElapsedTime);
            }
        }

        public void AdvanceTimeTo(TimeSpan currentElapsedTime)
        {
            _currentElapsedTime = currentElapsedTime;
        }

        private void PurgeExpiredAnimations(TimeSpan currentElapsedTime)
        {
            var currentAnimations = _animations.Where(a => !a.HasExpired(currentElapsedTime)).ToArray();

            _animations.Clear();
            _animations.AddRange(currentAnimations);

            _lastPurged = currentElapsedTime;
        }

        public void AddAnimation(Animation animation)
        {
            _animations.Add(animation);
        }

        public void RemoveAnimation(Animation animation)
        {
            _animations.Remove(animation);
        }

        public IDisposable BeginPlanning()
        {
            _currentPlan = new StoryboardPlanner(this);

            return new DelegateInvokingDisposer(() => _currentPlan = null);
        }
    }

    public class DelegateInvokingDisposer : IDisposable
    {
        private Action _action;

        public DelegateInvokingDisposer(Action action)
        {
            _action = action;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _action();
        }
    }
}
