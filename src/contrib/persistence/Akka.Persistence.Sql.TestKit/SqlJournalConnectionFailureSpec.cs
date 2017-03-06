﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.Sql.TestKit
{
    public abstract class SqlJournalConnectionFailureSpec : Akka.TestKit.Xunit2.TestKit
    {
        protected static readonly string DefaultInvalidConnectionString = "INVALID_CONNECTION_STRING";

        public SqlJournalConnectionFailureSpec(Config config = null, ITestOutputHelper output = null)
            : base(config)
        {
        }

        [Fact]
        public void Persistent_actor_should_throw_exception_upon_connection_failure()
        {
            EventFilter.Exception<Exception>().ExpectOne(() =>
            {
                var pref = Sys.ActorOf(Props.Create(() => new ReceiveAnyPersistentActor("persistent-test-actor")));
                pref.Tell("save");
            });

            ExpectNoMsg();
        }

        private class ReceiveAnyPersistentActor : TestReceivePersistentActor
        {
            public ReceiveAnyPersistentActor(string pid) : base(pid)
            {
                Command<string>(str => str.StartsWith("s"), e =>
                {
                    Persist(e, h =>
                    {
                        Sender.Tell("persisted:" + e);
                    });
                });
                CommandAny(o => Sender.Tell("any:" + o, Self));
            }
        }

        private abstract class TestReceivePersistentActor : ReceivePersistentActor
        {
            public readonly LinkedList<object> State = new LinkedList<object>();
            private readonly string _persistenceId;

            protected TestReceivePersistentActor(string persistenceId)
            {
                _persistenceId = persistenceId;
            }

            public override string PersistenceId
            {
                get { return _persistenceId; }
            }
        }
    }
}
