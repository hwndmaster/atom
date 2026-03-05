// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
#pragma warning disable S2094 // This empty class is required for record support in netstandard2.0
    internal static class IsExternalInit
    {
    }
#pragma warning restore S2094
}
#endif
