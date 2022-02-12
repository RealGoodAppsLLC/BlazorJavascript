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
var window = GlobalThis.GetWindow(jsRuntime);
var document = window.GetDocument();
var element = document.GetElementById("myCoolElement");
var computedProperties = window.GetComputedStyle(element);
```

This is the primary goal of the project: Make it easy and natural-feeling to call Javascript from your Blazor project.