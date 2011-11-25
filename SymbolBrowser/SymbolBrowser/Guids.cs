// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.SymbolBrowser
{
    static class GuidList
    {
        public const string guidSymbolBrowserPkgString = "9fca83c0-2304-4ac9-a3cf-1443e583728c";
        public const string guidSymbolBrowserCmdSetString = "6b97f5e7-96e5-4190-b201-cbea57b12684";
        public const string guidToolWindowPersistanceString = "075dff8e-02ea-41b7-9f1a-0710b80ad237";

        public static readonly Guid guidSymbolBrowserCmdSet = new Guid(guidSymbolBrowserCmdSetString);
    };
}