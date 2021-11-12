using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Avalonia.Shared.PlatformSupport
{
    internal partial class TestRuntimePlatform : StandardRuntimePlatform
    {
        public override RuntimePlatformInfo GetRuntimeInfo()
        {
            return new RuntimePlatformInfo();
        }
    }
}
