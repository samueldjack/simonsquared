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

namespace Flatlings.Animations
{
    public enum RepeatMode
    {
        None,
        Forever
    }

    public abstract class Animation
    {
        public TimeSpan Duration { get; set; }

        public Animation(TimeSpan duration)
        {
            Duration = duration;
        }

        public Animation()
        {
        }

        public TimeSpan StartTime { get; set; }

        public RepeatMode RepeatMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the animation should reverse when repeating.
        /// </summary>
        public bool AutoReverse { get; set; }

        public void UpdateForTime(TimeSpan currentElapsedTime)
        {
            var normalisedTime = GetNormalisedTimeInAnimation(currentElapsedTime);
            if (normalisedTime.HasValue)
            {
                SetValueForTime(normalisedTime.Value);
            }
        }

        protected abstract void SetValueForTime(double normalisedTime);

        public virtual bool HasExpired(TimeSpan currentElapsedTime)
        {
            return (RepeatMode == RepeatMode.None && (StartTime + Duration < currentElapsedTime));
        }

        /// <summary>
        /// Returns a value between 0 and 1 indicating the normalised time within the animation.
        /// </summary>
        /// <param name="currentElapsedTime">The current elapsed time.</param>
        /// <returns></returns>
        protected double? GetNormalisedTimeInAnimation(TimeSpan currentElapsedTime)
        {
            var normalisedPosition = 0.0;

            var millisecondsAfterStart = currentElapsedTime.TotalMilliseconds - StartTime.TotalMilliseconds;
            if (millisecondsAfterStart < 0)
            {
                return null;
            } 
            else if (millisecondsAfterStart > Duration.TotalMilliseconds && RepeatMode == RepeatMode.None)
            {
                return 1;
            }
            else
            {
                var quotient = millisecondsAfterStart / Duration.TotalMilliseconds;
                var integerPart = Math.Floor(quotient);

                normalisedPosition = quotient - integerPart;

                if (AutoReverse && (integerPart % 2) == 1)
                {
                    normalisedPosition = 1 - normalisedPosition;
                }
            }

            return normalisedPosition;
        }
    }
}