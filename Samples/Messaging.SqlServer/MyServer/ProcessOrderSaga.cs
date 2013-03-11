﻿namespace MyServer
{
    using System;
    using System.Diagnostics;
    using Common;
    using MyMessages.Commands;
    using MyMessages.Events;
    using MyMessages.RequestResponse;
    using NServiceBus;
    using NServiceBus.Saga;

    public class ProcessOrderSaga : Saga<OrderData>,
                                    ISagaStartedBy<InventoryResponse>,
                                    IHandleMessages<CancelOrder>,
                                    IHandleTimeouts<BuyersRemorseIsOver>
    {
        public void Handle(InventoryResponse message)
        {
            if (DebugFlagMutator.Debug)
            {
                Debugger.Break();
            }

            Data.OrderNumber = message.OrderNumber;
            Data.VideoIds = message.VideoIds;
            Data.ClientId = message.ClientId;

            RequestTimeout(TimeSpan.FromSeconds(20), new BuyersRemorseIsOver());
            Console.Out.WriteLine("Starting cool down period for order #{0}.", Data.OrderNumber);
        }

        public void Timeout(BuyersRemorseIsOver state)
        {
            if (DebugFlagMutator.Debug)
            {
                Debugger.Break();
            }

            Bus.Publish<OrderAccepted>(e =>
                {
                    e.OrderNumber = Data.OrderNumber;
                    e.VideoIds = Data.VideoIds;
                    e.ClientId = Data.ClientId;
                });

            MarkAsComplete();

            Console.Out.WriteLine("Cooling down period for order #{0} has elapsed.", Data.OrderNumber);
        }

        public void Handle(CancelOrder message)
        {
            if (DebugFlagMutator.Debug)
            {
                   Debugger.Break();
            }

            MarkAsComplete();

            Bus.Publish(Bus.CreateInstance<OrderCancelled>(o =>
                {
                    o.OrderNumber = message.OrderNumber;
                    o.ClientId = message.ClientId;
                }));

            Console.Out.WriteLine("Order #{0} was cancelled.", message.OrderNumber);
        }

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<InventoryResponse>(m => m.OrderNumber)
                .ToSaga(s=>s.OrderNumber);
            ConfigureMapping<CancelOrder>(m => m.OrderNumber)
                .ToSaga(s=>s.OrderNumber);
        }
    }

    public class OrderData : ISagaEntity
    {
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }

        [Unique]
        public int OrderNumber { get; set; }
        public string[] VideoIds { get; set; }
        public string ClientId { get; set; }
    }

    public class BuyersRemorseIsOver
    {
    }
}