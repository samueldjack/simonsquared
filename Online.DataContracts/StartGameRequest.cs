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
using System.Net;
using System.Runtime.Serialization;
using ProtoBuf;

namespace SimonSquared.Online.DataContracts
{
    [ProtoContract]
    [DataContract]
    public class StartGameRequest
    {
        [ProtoMember(1)]
        [DataMember]
        public string OwnerName { get; set; }

        [ProtoMember(2)]
        [DataMember]
        public string GameName { get; set; }

        [ProtoMember(3)]
        [DataMember]
        public string OwnerId { get; set; }
    }
}
