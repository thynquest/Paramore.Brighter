﻿using System;
using System.Linq;
using nUnitShouldAdapter;
using NUnit.Framework;
using NUnit.Specifications;
using paramore.brighter.commandprocessor.tests.nunit.CommandProcessors.TestDoubles;
using TinyIoC;

namespace paramore.brighter.commandprocessor.tests.nunit.CommandProcessors
{
    [TestFixture]
    public class PipelineMixedHandlersAsyncTests
    {
        private PipelineBuilder<MyCommand> _pipelineBuilder;
        private IHandleRequestsAsync<MyCommand> _pipeline;
        private Exception _exception;

        [SetUp]
        public void Establish()
        {
            var registry = new SubscriberRegistry();
            registry.RegisterAsync<MyCommand, MyMixedImplicitHandlerAsync>();

            var container = new TinyIoCContainer();
            var handlerFactory = new TinyIocHandlerFactoryAsync(container);
            container.Register<IHandleRequestsAsync<MyCommand>, MyMixedImplicitHandlerAsync>();
            container.Register<IHandleRequests<MyCommand>, MyLoggingHandler<MyCommand>>();

            _pipelineBuilder = new PipelineBuilder<MyCommand>(registry, handlerFactory);
        }

        [Test]
        public void When_Building_An_Async_Pipeline_That_Has_Sync_Handlers()
        {
            _exception = Catch.Exception(() => _pipeline = _pipelineBuilder.BuildAsync(new RequestContext(), false).First());

            _exception.ShouldNotBeNull();
            _exception.ShouldBeOfExactType(typeof (ConfigurationException));
            _exception.Message.ShouldContain(typeof (MyLoggingHandler<>).Name);
        }
    }
}
