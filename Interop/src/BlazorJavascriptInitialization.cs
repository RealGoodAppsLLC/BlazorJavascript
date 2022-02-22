using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.JSInterop;

namespace RealGoodApps.BlazorJavascript.Interop
{
    public static class BlazorJavascriptInitialization
    {
        private static readonly List<IJSInProcessRuntime> InitializedRuntimes = new();

        public static void Initialize(IJSInProcessRuntime jsInProcessRuntime)
        {
            if (InitializedRuntimes.Contains(jsInProcessRuntime))
            {
                return;
            }

            var resource =
                typeof(BlazorJavascriptInitialization).Assembly.GetManifestResourceStream(
                    "RealGoodApps.BlazorJavascript.Interop.Javascript.script.js");

            if (resource == null)
            {
                throw new Exception("Unable to locate BlazorJavascript JS bridge!");
            }

            using (resource)
            using (var reader = new StreamReader(resource))
            {
                jsInProcessRuntime.InvokeVoid("eval", reader.ReadToEnd());
                InitializedRuntimes.Add(jsInProcessRuntime);
            }
        }
    }
}
