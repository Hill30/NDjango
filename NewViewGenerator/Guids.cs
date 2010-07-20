// Guids.cs
// MUST match guids.h
using System;

namespace NewViewGenerator
{
    static class GuidList
    {
        public const string guidNewViewGeneratorPkgString = "2780818b-02fb-4990-a051-7cee3fd09157";
        public const string guidNewViewGeneratorCmdSetString = "7686279e-0421-4c87-8ed3-a484a22b58f3";
        public const string UICONTEXT_ViewsSelectedString = "5ebe12b1-2c8a-4e83-85d5-1f26eb36561c";
        public const uint cmdidNewViewGenerator = 0x0101;
        public static readonly Guid guidNewViewGeneratorCmdSet = new Guid(guidNewViewGeneratorCmdSetString);
        public static readonly Guid UICONTEXT_ViewsSelected = new Guid(UICONTEXT_ViewsSelectedString);
    };
}