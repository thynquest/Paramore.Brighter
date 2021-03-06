﻿#region Licence
/* The MIT License (MIT)
Copyright © 2014 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

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
using nUnitShouldAdapter;
using NUnit.Framework;
using paramore.brighter.serviceactivator;
using paramore.brighter.serviceactivator.Ports.Commands;
using paramore.brighter.serviceactivator.Ports.Mappers;

namespace paramore.brighter.commandprocessor.tests.nunit.ControlBus
{
    [TestFixture]
    public class HeartbeatMessageToReplyTests
    {
        private IAmAMessageMapper<HeartbeatReply> _mapper;
        private Message _message;
        private HeartbeatReply _request;
        private const string MESSAGE_BODY = "{\r\n  \"HostName\": \"Test.Hostname\",\r\n  \"Consumers\": [\r\n    {\r\n      \"ConnectionName\": \"Test.Connection\",\r\n      \"State\": 1\r\n    },\r\n    {\r\n      \"ConnectionName\": \"More.Consumers\",\r\n      \"State\": 0\r\n    }\r\n  ]\r\n}";
        private const string TOPIC = "test.topic";
        private readonly Guid _correlationId = Guid.NewGuid();

        [SetUp]
        public void Establish()
        {
            _mapper = new HeartbeatReplyCommandMessageMapper();
            var header = new MessageHeader(messageId: Guid.NewGuid(), topic: TOPIC, messageType: MessageType.MT_COMMAND, timeStamp: DateTime.UtcNow, correlationId: _correlationId);
            var body = new MessageBody(MESSAGE_BODY);
            _message = new Message(header, body);
        }

        [Test]
        public void When_mapping_from_a_message_to_a_heartbeat_reply()
        {
            _request = _mapper.MapToRequest(_message);

            // _should_set_the_sender_address_topic
            _request.SendersAddress.Topic.ShouldEqual(TOPIC);
            // _should_set_the_sender_correlation_id
            _request.SendersAddress.CorrelationId.ShouldEqual(_correlationId);
            // _should_set_the_hostName
            _request.HostName.ShouldEqual("Test.Hostname");
            // _should_contain_the_consumers
            _request.Consumers.ShouldContain(rc => rc.ConnectionName == "Test.Connection" && rc.State == ConsumerState.Open);
            _request.Consumers.ShouldContain(rc => rc.ConnectionName == "More.Consumers" && rc.State == ConsumerState.Shut);

        }
   }
}
