﻿using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests.Support
{
    public class DelegatingSink : ILogEventSink
    {
        readonly Action<LogEvent> _write;

        public DelegatingSink(Action<LogEvent> write)
        {
            _write = write ?? throw new ArgumentNullException(nameof(write));
        }

        public void Emit(LogEvent logEvent)
        {
            _write(logEvent);
        }
    }
}
