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
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Phone.Reactive;
using RestSharp;
using RestSharp.Deserializers;
using SimonSquared.Online.DataContracts;

namespace Flatlings
{
    public class GameServerClient
    {
        private RestClient _restClient;
        private Type[] _knownTypes;
        private const string ServerUrl = "http://dell-i7860-1:8888/SimonSquared";

        public GameServerClient()
        {
            _restClient = new RestClient(ServerUrl);
            _restClient.AddDefaultHeader("Accept", "application/xml");
            _restClient.AddHandler("application/xml", new DataContractDeserializer(_knownTypes));
            _knownTypes = GetKnownTypes();
        }

        private Type[] GetKnownTypes()
        {
            var types = (from type in typeof (GameStatusDto).Assembly.GetTypes()
                           let dataContractAttribute =
                               Attribute.GetCustomAttribute(type, typeof (DataContractAttribute))
                           where dataContractAttribute != null
                         select type)
                           .ToArray();
            return types;
        }

        public IObservable<TimeSpan> DetermineClockSkew()
        {
            var timeOffset = (from tick in Observable.Generate(0, _ => true, value => value, value => value + 1).Take(5)
                              from timeCheck in GetServerTime().Timestamp()
                              let estimatedOneWayLatency =
                                  (timeCheck.Timestamp.UtcDateTime - timeCheck.Value.ClientReportedTime).
                                      TotalMilliseconds/2
                              let estimatedTimeAtServer =
                                  timeCheck.Value.TimeAtServer + TimeSpan.FromMilliseconds(estimatedOneWayLatency)
                              let predictedClockSkew = estimatedTimeAtServer - timeCheck.Timestamp
                              select new {predictedClockSkew, estimatedOneWayLatency})
                .MinBy(p => p.estimatedOneWayLatency)
                .Select(p => p.predictedClockSkew);

            return timeOffset;
        }

        private IObservable<TimeCheck> GetServerTime()
        {
            var request = new RestRequest("Time?timeAtClient={timeNow}");
            request.AddUrlSegment("timeNow", DateTime.UtcNow.ToString("O"));

            return ExecuteRequest<TimeCheck>(request);
        }

        public IObservable<GameStatusDto> GetGameStatus(string gameId)
        {
            var request = new RestRequest("Games/{id}/State");
            request.AddUrlSegment("id", gameId);

            return ExecuteRequest<GameStatusDto>(request);
        }

        public IObservable<Unit> PostStatusUpdate(string gameId, GameStatusUpdate update)
        {
            var request = new RestRequest("Games/{id}/State", Method.POST)
                .AddUrlSegment("id", gameId)
                .AddBody<GameStatusUpdate>(update, _knownTypes);

            return ExecuteRequest(request);
        }

        public IObservable<GameDto> StartGame(string gameName, PlayerDto owner)
        {
            var restRequest = new RestRequest("Games", Method.POST)
                                  {
                                      RequestFormat = DataFormat.Json,
                                  };
            restRequest.AddBody<StartGameRequest>(new StartGameRequest() { GameName = gameName, OwnerName = owner.Name, OwnerId = owner.Id }, _knownTypes);

            return ExecuteRequest<GameDto>(restRequest);
        }

        public IObservable<GameDto> GetGame(string gameId)
        {
            var request = new RestRequest("Games/{id}");
            request.AddUrlSegment("id", gameId);

            return ExecuteRequest<GameDto>(request);
        }

        public IObservable<Round> GetGameRound(string gameId)
        {
            var request = new RestRequest("Games/{id}/CurrentRoundData");
            request.AddUrlSegment("id", gameId);

            return ExecuteRequest<Round>(request);
        }

        public IObservable<ScoreboardDto> GetGameScore(string gameId)
        {
            var request = new RestRequest("Games/{id}/Score");
            request.AddUrlSegment("id", gameId);

            return ExecuteRequest<ScoreboardDto>(request);
        }

        public IObservable<Unit> JoinGame(string gameId, PlayerDto player)
        {
            var request = new RestRequest("Games/{gameId}/Players", Method.POST);
            request.AddUrlSegment("gameId", gameId);
            request.AddBody(player, _knownTypes);

            return ExecuteRequest(request);
        }

        public IObservable<Unit> LeaveGame(string gameId, string playerId)
        {
            var request = new RestRequest("Games/{gameId}/Players/{playerId}", Method.DELETE);
            request.AddUrlSegment("gameId", gameId);
            request.AddUrlSegment("playerId", playerId);

            return ExecuteRequest(request);
        }

        private IObservable<TResult> ExecuteRequest<TResult>(RestRequest restRequest) where TResult : new()
        {
            var subject = new AsyncSubject<TResult>();

            _restClient.ExecuteAsync<TResult>(restRequest, response => HandleResponse(response, subject));

            return subject;
        }

        private IObservable<Unit> ExecuteRequest(RestRequest restRequest)
        {
            var subject = new AsyncSubject<Unit>();

            _restClient.ExecuteAsync(restRequest, response => HandleResponse(response, subject));

            return subject;
        }

        public IObservable<List<PlayerDto>> ListPlayers(string gameId)
        {
            var request = new RestRequest("Games/{gameId}/Players");
            request.AddUrlSegment("gameId", gameId);

            return ExecuteRequest<List<PlayerDto>>(request);
        }

        private void HandleResponse<T>(RestResponse<T> response, AsyncSubject<T> subject)
        {
            subject.OnNext(response.Data);
            subject.OnCompleted();
        }

        private void HandleResponse(RestResponse response, AsyncSubject<Unit> subject)
        {
            subject.OnNext(new Unit());
            subject.OnCompleted();
        }

        public IObservable<List<GameDto>> ListAvailableGames()
        {
            var restRequest = new RestRequest("Games", Method.GET);

            return ExecuteRequest<List<GameDto>>(restRequest);
        }
    }

    public class DataContractDeserializer : IDeserializer
    {
        private readonly Type[] _knownTypes;

        public DataContractDeserializer(Type[] knownTypes)
        {
            _knownTypes = knownTypes;
        }

        public T Deserialize<T>(RestResponse response) where T : new()
        {
            var memoryStream = new MemoryStream(response.RawBytes);
            var serializer = new DataContractSerializer(typeof (T), _knownTypes);
            return (T) serializer.ReadObject(memoryStream);
        }

        public string RootElement
        {
            get; set; 
        }

        public string Namespace
        {
            get;
            set;
        }

        public string DateFormat
        {
            get;
            set;
        }
    }
}
