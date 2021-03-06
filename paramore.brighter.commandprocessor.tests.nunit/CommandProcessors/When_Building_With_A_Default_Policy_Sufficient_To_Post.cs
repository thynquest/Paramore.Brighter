﻿#region Licence
/* The MIT License (MIT)
Copyright © 2015 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System;
using System.Linq;
using nUnitShouldAdapter;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Specifications;
using paramore.brighter.commandprocessor.tests.nunit.CommandProcessors.TestDoubles;

namespace paramore.brighter.commandprocessor.tests.nunit.CommandProcessors
{
    [TestFixture]
    public class PostCommandTests
    {
        private CommandProcessor _commandProcessor;
        private readonly MyCommand _myCommand = new MyCommand();
        private Message _message;
        private FakeMessageStore _fakeMessageStore;
        private FakeMessageProducer _fakeMessageProducer;

        [SetUp]
        public void Establish()
        {
            _myCommand.Value = "Hello World";

            _fakeMessageStore = new FakeMessageStore();
            _fakeMessageProducer = new FakeMessageProducer();

            _message = new Message(
                header:
                    new MessageHeader(messageId: _myCommand.Id, topic: "MyCommand", messageType: MessageType.MT_COMMAND),
                body: new MessageBody(JsonConvert.SerializeObject(_myCommand))
                );

            var messageMapperRegistry =
                new MessageMapperRegistry(new SimpleMessageMapperFactory(() => new MyCommandMessageMapper()));
            messageMapperRegistry.Register<MyCommand, MyCommandMessageMapper>();

            _commandProcessor = CommandProcessorBuilder.With()
                .Handlers(new HandlerConfiguration(new SubscriberRegistry(), new EmptyHandlerFactory()))
                .DefaultPolicy()
                .TaskQueues(new MessagingConfiguration((IAmAMessageStore<Message>)_fakeMessageStore, (IAmAMessageProducer) _fakeMessageProducer, messageMapperRegistry))
                .RequestContextFactory(new InMemoryRequestContextFactory())
                .Build();
        }

        [Test]
        public void When_Building_With_A_Default_Policy_Sufficient_To_Post()
        {
            _commandProcessor.Post(_myCommand);

            //_should_store_the_message_in_the_sent_command_message_repository
            _fakeMessageStore.MessageWasAdded.ShouldBeTrue();
           //_should_send_a_message_via_the_messaging_gateway
            _fakeMessageProducer.MessageWasSent.ShouldBeTrue();
           //_should_convert_the_command_into_a_message
            _fakeMessageStore.Get().First().ShouldEqual(_message);
        }

        internal class EmptyHandlerFactory : IAmAHandlerFactory
        {
            public IHandleRequests Create(Type handlerType)
            {
                return null;
            }

            public void Release(IHandleRequests handler) {}
        }
    }
}
