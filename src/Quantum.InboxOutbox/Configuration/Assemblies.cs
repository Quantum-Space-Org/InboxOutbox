using System;
using System.Reflection;

namespace Quantum.InboxOutbox.Configuration;

public record Assemblies
{
    public Assembly[] InboxAssemblies { get; set; } = Array.Empty<Assembly>();
    public Assembly[] OutboxAssemblies { get; set; } = Array.Empty<Assembly>();
}