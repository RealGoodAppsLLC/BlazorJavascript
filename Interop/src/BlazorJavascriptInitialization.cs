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

            var resourceGenerated =
                typeof(BlazorJavascriptInitialization).Assembly.GetManifestResourceStream(
                    "RealGoodApps.BlazorJavascript.Interop.Generated.Javascript.script.js");

            if (resourceGenerated == null)
            {
                throw new Exception("Unable to locate generated portion of BlazorJavascript JS bridge!");
            }

            using (resource)
            using (resourceGenerated)
            using (var reader = new StreamReader(resource))
            using (var readerGenerated = new StreamReader(resourceGenerated))
            {
                jsInProcessRuntime.InvokeVoid("eval", reader.ReadToEnd());
                jsInProcessRuntime.InvokeVoid("eval", readerGenerated.ReadToEnd());
                InitializedRuntimes.Add(jsInProcessRuntime);
            }
        }
    }
}
