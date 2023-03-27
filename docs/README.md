# Basic Concept

Easy unidirectional store and redo/undo library for state management for frontend apps on Blazor/.NET

We provides a Store that allows you to share state between components.
All stores are managed by a single provider and can subscribe to state change notifications.
Undirectional flow and immutable change of state provides a predictable architecture.
In addition, we provide a store that easily implements Redo/Undo by managing in immutable states.

## DEMO Page

https://le-nn.github.io/memento/

If you have ReduxDevTool installed,
DevTool will launch automatically.
You can do state history and time travel.


[See ReduxDevTools Docs](./ReduxDevTool.md) for details of usage.

### Features

* Less boilerplate, less rule and simple usage 
* Immutable state and Unidirectional flow
* Multiple stores but manged by single provider, so can observe and manage as one state tree
* Observe detailed status with command patterns and makes it easier to monitor what happened within the application 

![](../../Architecture.jpg)

## Compatibility and bindings

| Package Name                                                                                | Platform            | Desctiption                                                 |
| ------------------------------------------------------------------------------------------- | ------------------- | ----------------------------------------------------------- |
| [Memento.Core](https://www.nuget.org/packages/Memento.Core)                                 | .NET 6 or later     | Core Package of Memento                                     |
| [Memento.Blazor](https://www.nuget.org/packages/Memento.Blazor)                             | .NET 6 or later     | Provides Observing state changes on Blazor Component.       |
| [Memento.ReduxDevTool.Remote](https://www.nuget.org/packages/Memento.ReduxDevTool.Remote)   | .NET 6 or later     | Connect and Interact with applications via WebSocket.       |
| [Memento.ReduxDevTool.Browser](https://www.nuget.org/packages/Memento.ReduxDevTool.Browser) | .NET 6 or later     | Interact with ReduxDevTools via JavaScript interop.         |
| [Memento.ReduxDevTool](https://www.nuget.org/packages/Memento.ReduxDevTool)                 | .NET 6 or later     | Provides basic functionality to interact with ReduxDevTools. Interop is required. |

## Tutorials

The tutorials implemented in each language and simple console application are following

[Tutorials with plane C#](./Tutorial.cs.md)

Practical usage on the Framework
In reality, most of them will be used together with the UI framework.
Describes the concepts common to all languages and frameworks is following.

[Tutorials with Blazor](./Blazor/GettingStandard.md)

ReduxDevTools is a tool for debugging application's state changes.
State can be time traveled and history can be viewed in ReduxDevTools.

[Redux Dev Tools](./ReduxDevTools.md)

## Samples

[C# with Console App](../samples/Memento.Sample.ConsoleApp)

[Blazor App Shared](../samples/Memento.Sample.Blazor)

[Blazor Wasm App](../samples/Memento.Sample.BlazorWasm)

[Blazor Server App](../samples/Memento.Sample.BlazorServer)


