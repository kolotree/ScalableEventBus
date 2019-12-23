﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using HttpClientAdapter;
using ProtoActorAdapter;
using ProtoActorAdapter.Logging;

namespace TestApplication
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var httpApplyDomainEventStrategy = new HttpApplyDomainEventStrategy(new Uri("https://webhook.site/9705d5a2-8189-4bdc-959f-ed5540e5cdc9"));
            
            var eventsToSend = Enumerable.Range(1, 53)
                .Select(i => DomainEventBuilder.New()
                    .WithNumber(i)
                    .ForAggregate($"Aggregate{i}")
                    .WithData("{}")
                    .WithMetadata("{}")
                    .Using(httpApplyDomainEventStrategy)
                    .Build())
                .ToList();

            var domainEventApplier = await ProtoActorDomainEventApplierBuilder.New()
                .Using(new EventStoreConfiguration(
                    "localhost",
                    1113,
                    "admin",
                    "changeit",
                    "TestSnapshot",
                    TimeSpan.FromSeconds(10)))
                .DecorateWith(ConsoleLogger.New())
                .Build();

            var lastDispatchedDomainEvent = await domainEventApplier.ReadLastDispatchedDomainEvent();
            foreach (var domainEvent in eventsToSend.Where(e => e.Number > lastDispatchedDomainEvent))
            {
                domainEventApplier.Pass(domainEvent);
            }
            
            Console.ReadLine();
        }
    }
}