﻿using System;
using Domain;
using EventStoreAdapter;
using Ports;

namespace EventStoreReaderTestApp
{
    internal sealed class EventStoreReceiver : IEventStoreStreamMessageReceiver
    {
        public void Receive(DomainEventBuilder builder)
        {
            Console.WriteLine(builder.Build());
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var subscription = new EventStoreReader("tcp://localhost:1113").SubscribeTo(
                StreamName.Of("Library"), new EventStoreReceiver());

            Console.ReadLine();
            subscription.Dispose();
        }
    }
}