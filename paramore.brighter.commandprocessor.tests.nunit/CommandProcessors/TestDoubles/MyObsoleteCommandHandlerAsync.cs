﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace paramore.brighter.commandprocessor.tests.nunit.CommandProcessors.TestDoubles
{
    internal class MyObsoleteCommandHandlerAsync : RequestHandlerAsync<MyCommand>
    {
        private static MyCommand s_command;

        public MyObsoleteCommandHandlerAsync()
        {
            s_command = null;
        }

        [MyPreValidationHandlerAsyncAttribute(step: 2, timing: HandlerTiming.Before)]
        [MyPostLoggingHandlerAsyncAttribute(step: 1, timing: HandlerTiming.After)]
        [Obsolete] // even with attributes non inheriting from MessageHandlerDecoratorAttribute it should not fail
        public override async Task<MyCommand> HandleAsync(MyCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return command;
            }

            LogCommand(command);
            return await base.HandleAsync(command, cancellationToken).ConfigureAwait(ContinueOnCapturedContext);
        }

        public static bool Shouldreceive(MyCommand expectedCommand)
        {
            return s_command != null && expectedCommand.Id == s_command.Id;
        }

        private void LogCommand(MyCommand request)
        {
            s_command = request;
        }
    }
}
