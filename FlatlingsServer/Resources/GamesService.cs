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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using FlatlingsServer.Domain;
using SimonSquared.Online.DataContracts;

namespace FlatlingsServer.Resources
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceContract]
    public class GamesService
    {
        private readonly GameManager _gameManager;

        public GamesService(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        [WebGet(UriTemplate = "/{gameId}")]
        public GameDto GetGame(string gameId, HttpResponseMessage response)
        {
            var game = _gameManager.FindGame(new Guid(gameId));

            if (game == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = "No game exists with Guid " + gameId;
                return null;
            }

            return ConvertGameToGameResource(game);
        }

        [WebGet(UriTemplate = "/{gameId}/State")]
        public GameStatusDto GetGameStatus(string gameId, HttpResponseMessage response)
        {
            var game = _gameManager.FindGame(new Guid(gameId));

            response.Headers.CacheControl = new CacheControlHeaderValue() {NoCache = true};
            if (game == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = "No game exists with Guid " + gameId;
                return null;
            }

            return game.GetStateSnapshot();
        }

        [WebGet(UriTemplate = "/{gameId}/CurrentRoundData")]
        public Round GetGamesCurrentRound(string gameId, HttpResponseMessage response)
        {
            var game = _gameManager.FindGame(new Guid(gameId));

            response.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
            if (game == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = "No game exists with Guid " + gameId;
                return null;
            }

            return game.Round;
        }

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{gameId}/State")]
        [ServiceKnownType(typeof(BeginGameUpdate))]
        public void UpdateGameStatus(string gameId, GameStatusUpdate update, HttpResponseMessage response)
        {
            var game = _gameManager.FindGame(new Guid(gameId));

            if (game == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = "No game exists with Guid " + gameId;
                return;
            }

            game.ProcessUpdate(update);
        }

        [WebGet(UriTemplate = "/")]
        public IList<GameDto> GetAll()
        {
            var resources = _gameManager.GetAll().Where(game => game.IsJoinable).Select(ConvertGameToGameResource).ToList();

            return resources;
        }

        [WebInvoke(Method = "POST", UriTemplate = "/")]
        public GameDto StartNewGame(StartGameRequest request)
        {
            var game = _gameManager.AddGame(request.GameName,
                                            new Player {Id = new Guid(request.OwnerId), Name = request.OwnerName});

            return ConvertGameToGameResource(game);
        }

        [WebGet(UriTemplate="/{gameId}/Players")]
        public IList<PlayerDto> GetAllPlayers(string gameId, HttpResponseMessage response)
        {
            var game = _gameManager.FindGame(new Guid(gameId));

            response.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };

            if (game == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = "No game exists with Guid " + gameId;
            }

            return game.Players.Select(ConvertPlayerToPlayerDto).ToList();
        }

        [WebGet(UriTemplate = "/{gameId}/Score")]
        public ScoreboardDto GetScore(string gameId, HttpResponseMessage response)
        {
            var game = _gameManager.FindGame(new Guid(gameId));

            response.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };

            if (game == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = "No game exists with Guid " + gameId;
            }

            return game.GetScoreSnapshot();
        }

        [WebInvoke(UriTemplate = "/{gameId}/Players", Method = "POST")]
        public void AddPlayer(string gameId, PlayerDto playerDto, HttpResponseMessage response)
        {
            var game = _gameManager.FindGame(new Guid(gameId));

            if (game == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = "No game exists with Guid " + gameId;
            }

            var player = new Player() {Id = new Guid(playerDto.Id), Name = playerDto.Name};
            game.AddPlayer(player);
        }

        [WebInvoke(UriTemplate = "/{gameId}/Players/{playerId}", Method = "DELETE")]
        public void RemovePlayer(string gameId, string playerId, HttpResponseMessage response)
        {
            var game = _gameManager.FindGame(new Guid(gameId));

            if (game == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = "No game exists with Guid " + gameId;
            }

            game.RemovePlayer(playerId);
        }

        private GameDto ConvertGameToGameResource(Game game)
        {
            var uri = "Games/" + game.Id;

            return new GameDto()
                       {
                           Id = game.Id.ToString(),
                           Name = game.Name,
                           OwnerName = game.OwnerName,
                       };
        }

        private PlayerDto ConvertPlayerToPlayerDto(Player player)
        {
            return new PlayerDto() {Id = player.Id.ToString(), Name = player.Name};
        }
    }
}
