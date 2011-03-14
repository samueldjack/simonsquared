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
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using AutoMapper;
using FlatlingsServer.Domain;
using FlatlingsServer.Infrastructure;
using FlatlingsServer.Resources;
using FlatlingsServer.Services;
using Microsoft.ServiceModel.Http;
using SimonSquared.Online.DataContracts;

namespace FlatlingsServer
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var containerBuilder = new ContainerBuilder();
            var levelsRepository = new LevelDataRepository();
            levelsRepository.LoadLevelData();
            containerBuilder.RegisterType<GameManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<GamesService>();
            containerBuilder.RegisterType<TimeService>();
            containerBuilder.RegisterInstance(Mapper.Engine)
                .ExternallyOwned()
                .AsImplementedInterfaces();
            containerBuilder.RegisterInstance(levelsRepository).ExternallyOwned().AsSelf();
            containerBuilder.RegisterType<PuzzleGenerator>().AsSelf().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Game>().AsSelf().InstancePerDependency();
            containerBuilder.RegisterAssemblyTypes(typeof (GameState).Assembly)
                .AssignableTo<GameState>()
                .InstancePerDependency()
                .AsSelf();

            var container = containerBuilder.Build();

            var serviceConfiguration = new ServiceConfiguration(container);

            routes.AddServiceRoute<GamesService>("Games", serviceConfiguration);
            routes.AddServiceRoute<TimeService>("Time", serviceConfiguration);

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            InitiateAutoMapper();
        }

        private void InitiateAutoMapper()
        {
            Mapper.CreateMap<GameState, GameStatusDto>()
                .Include<WaitingForPlayersState, WaitingForPlayersStateDto>()
                .Include<BeginningRoundState, RoundStartingStateDto>()
                .Include<SolvingPuzzleState, SolvingPuzzleStateDto>()
                .Include<BeginningPuzzleState, BeginningPuzzleStateDto>()
                .Include<RoundEndedState, RoundCompletedStateDto>()
                .Include<GameAbandonedState, GameAbandonedStateDto>();
 
            Mapper.CreateMap<WaitingForPlayersState, WaitingForPlayersStateDto>();
            Mapper.CreateMap<BeginningRoundState, RoundStartingStateDto>();
            Mapper.CreateMap<BeginningPuzzleState, BeginningPuzzleStateDto>();
            Mapper.CreateMap<SolvingPuzzleState, SolvingPuzzleStateDto>();
            Mapper.CreateMap<RoundEndedState, RoundCompletedStateDto>();
            Mapper.CreateMap<GameAbandonedState, GameAbandonedStateDto>();

            Mapper.CreateMap<Scoreboard, ScoreboardDto>();
            Mapper.CreateMap<PlayerScore, ScoreDto>();
        }
    }
}