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
using Microsoft.Phone.Reactive;

namespace Flatlings
{
    class DispatcherScheduler : IScheduler
    {
        private readonly Dispatcher _dispatcher;

        public static IScheduler Current { get; private set; }

        public static void SetDispatcher(Dispatcher dispatcher)
        {
            Current = new DispatcherScheduler(dispatcher);
        }

        protected DispatcherScheduler(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Schedules an action to be executed.
        /// </summary>
        /// <returns>
        /// Returns 
        /// <see cref="T:System.IDisposable"/>
        /// .
        /// </returns>
        /// <param name="action"/>
        public IDisposable Schedule(Action action)
        {
            return _dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Schedules action to be executed after the specified time span.
        /// </summary>
        /// <returns>
        /// Returns 
        /// <see cref="T:System.IDisposable"/>
        /// .
        /// </returns>
        /// <param name="action"/><param name="dueTime"/>
        public IDisposable Schedule(Action action, TimeSpan dueTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the scheduler's notion of current time.
        /// </summary>
        /// <returns>
        /// Returns 
        /// <see cref="T:System.DateTimeOffset"/>
        /// .
        /// </returns>
        public DateTimeOffset Now
        {
            get { throw new NotImplementedException(); }
        }
    }
}
