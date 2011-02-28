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

namespace Flatlings.Animations
{
    public class StoryboardPlanner
    {
        private readonly Storyboard _storyboard;
        private TimeSpan _offsetFromStoryboardTime;
        private AnimationFactory _animationFactory;

        internal StoryboardPlanner(Storyboard storyboard)
        {
            _storyboard = storyboard;
            _animationFactory = new AnimationFactory();
        }

        public StoryboardPlanner AfterDelay(TimeSpan timeSpan)
        {
            _offsetFromStoryboardTime += timeSpan;
            return this;
        }

        public StoryboardPlanner Begin(Func<AnimationFactory, Animation> animationBuilder)
        {
            var animation = animationBuilder(_animationFactory);
            AddAnimation(animation);

            return this;
        }

        private void AddAnimation(Animation animation)
        {
            animation.StartTime = _storyboard.CurrentTime + _offsetFromStoryboardTime;

            _storyboard.AddAnimation(animation);
        }

        public StoryboardPlanner Begin(Animation animation)
        {
            AddAnimation(animation);

            return this;
        }
    }
}
