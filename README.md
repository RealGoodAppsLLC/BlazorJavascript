# Overview

A nuget package that provides wrappers classes, functions, and interfaces for interoperating with JavaScript from Blazor.

While interop with Javascript is entirely possible with Blazor without this package, you typically would have to do something like:
```csharp
var encoded = jsRuntime.InvokeAsync<string>("btoa", "Encode Me");
var decoded = jsRuntime.InvokeAsync<string>("atob", encoded);
```

This works fine for simple function calls on the window object, but there are several issues:
1) What do you do if you need to get the value of a property?
2) How do you set the value of a property?
3) What happens if you need to use the raw reference return value of a function and pass it into another function?

While this is not an exhaustive list of issues, it started to become obvious that it would be helpful if there was an API that allowed you to do stuff like this:

```csharp
var window = jsRuntime.GetWindow();
var element = window.document.getElementById(jsRuntime.CreateString("myCoolElement"));
var computedProperties = window.getComputedStyle(element);
```

This is the primary goal of the project: Make it easy and natural-feeling to call Javascript from your Blazor project.

## Getting Started

1) Install the [RealGoodApps.BlazorJavascript.Interop](https://www.nuget.org/packages/RealGoodApps.BlazorJavascript.Interop) package from Nuget.
2) Add the following initialization code to your `App.razor`:

```csharp
@code {
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (_jsRuntime is not IJSInProcessRuntime jsInProcessRuntime)
        {
            throw new InvalidCastException("The JS runtime must be in-process.");
        }

        BlazorJavascriptInitialization.Initialize(jsInProcessRuntime);
    }
}
```

3) Start using it! Here is a simple example using `App.razor` that listens to the `window.onresize` event and prints out the current viewport size.

```csharp
@code {
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (_jsRuntime is not IJSInProcessRuntime jsInProcessRuntime)
        {
            throw new InvalidCastException("The JS runtime must be in-process.");
        }

        BlazorJavascriptInitialization.Initialize(jsInProcessRuntime);

        var window = jsInProcessRuntime.GetWindow();

        window?.addEventListener(
            jsInProcessRuntime.CreateString("resize"),
            jsInProcessRuntime.CreateAction(() =>
            {
                var windowWidth = window.innerWidth.ConvertToDotNetInt();
                var windowHeight = window.innerHeight.ConvertToDotNetInt();
                Console.WriteLine($"Window size: {windowWidth}px by {windowHeight}px");
            }));
    }
}
```