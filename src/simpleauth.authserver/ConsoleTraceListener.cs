namespace SimpleAuth.AuthServer;

using System;
using System.Diagnostics;

/// <summary>Directs tracing or debugging output to either the standard output or the standard error stream.</summary>
public sealed class ConsoleTraceListener : TextWriterTraceListener
{
    /// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.ConsoleTraceListener" /> sealed class with trace output written to the standard output stream.</summary>
    public ConsoleTraceListener()
        : base(Console.Out)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.ConsoleTraceListener" /> sealed class with an option to write trace output to the standard output stream or the standard error stream.</summary>
    /// <param name="useErrorStream">
    /// <see langword="true" /> to write tracing and debugging output to the standard error stream; <see langword="false" /> to write tracing and debugging output to the standard output stream.</param>
    public ConsoleTraceListener(bool useErrorStream)
        : base(useErrorStream ? Console.Error : Console.Out)
    {
    }

    /// <summary>Closes the output to the stream specified for this trace listener.</summary>
    public override void Close()
    {
    }
}