﻿using System;
using System.Linq;
using Domain;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;

namespace EventStoreAdapter
{
    public sealed class SourceProjectionCreator
    {
        private readonly ProjectionsManager _projectionsManager;

        private string ProjectionBodyFor(StreamPrefix streamPrefix) =>
            "fromAll().when({ `$any : function(s,e) { if (e.streamId.startsWith('STREAM_PREFIX')) linkTo('AllEvents-STREAM_PREFIX', e); }});"
                .Replace("STREAM_PREFIX", streamPrefix);

        private SourceProjectionCreator(ProjectionsManager projectionsManager)
        {
            _projectionsManager = projectionsManager;
        }
        
        public static SourceProjectionCreator NewFor(Uri connectionString) => new SourceProjectionCreator(
            new ProjectionsManager(
                new NoEventStoreLogger(),
                connectionString.ToDnsEndPoint(),
                TimeSpan.FromSeconds(60)));
        
        public bool CreateFor(StreamPrefix streamPrefix)
        {
            try
            {
                _projectionsManager.CreateContinuousAsync(
                    $"{streamPrefix}Projection",
                    ProjectionBodyFor(streamPrefix),
                    true,
                    new UserCredentials("admin", "changeit")).Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions.Any(inner => inner is ProjectionCommandConflictException))
                {
                    return false;
                }
                
                throw;
            }
            
            return true;
        }
    }
}