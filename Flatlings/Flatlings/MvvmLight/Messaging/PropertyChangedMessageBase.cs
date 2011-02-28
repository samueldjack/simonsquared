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
namespace GalaSoft.MvvmLight.Messaging
{
    /// <summary>
    /// Basis class for the <see cref="PropertyChangedMessage{T}" /> class. This
    /// class allows a recipient to register for all PropertyChangedMessages without
    /// having to specify the type T.
    /// </summary>
    ////[ClassInfo(typeof(Messenger))]
    public abstract class PropertyChangedMessageBase : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedMessageBase" /> class.
        /// </summary>
        /// <param name="sender">The message's sender.</param>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected PropertyChangedMessageBase(object sender, string propertyName)
            : base(sender)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedMessageBase" /> class.
        /// </summary>
        /// <param name="sender">The message's sender.</param>
        /// <param name="target">The message's intended target. This parameter can be used
        /// to give an indication as to whom the message was intended for. Of course
        /// this is only an indication, amd may be null.</param>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected PropertyChangedMessageBase(object sender, object target, string propertyName)
            : base(sender, target)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedMessageBase" /> class.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected PropertyChangedMessageBase(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Gets or sets the name of the property that changed.
        /// </summary>
        public string PropertyName
        {
            get;
            protected set;
        }
    }
}