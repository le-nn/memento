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



| Link | Summary | 
| -------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | 
| [Tutorials with pure C#](./BasicConcept.md)              | The tutorials for implemented with pure C# in simple console application.                                                                                                                                 | 
| <br>[Tutorials with Blazor](./Blazor/GettingStandard.md) | Practical Uses of the Framework. In practice, it is mostly used with UI frameworks.<br><br>Here is a tutorial on how to use it with Blazor.                                                             | 
| [Middleware](./Middlware.md)                             | Middleware can be implemented to interrupt the process when updating the state.<br><br>Middleware can be extended for various purposes, such as implementing your own Logger or supporting ReduxDevTools. | 
| [Redux Dev Tools](./ReduxDevTools.md)                    | ReduxDevTools is a tool for debugging application's state changes.<br><br>State can be time traveled and history can be viewed in ReduxDevTools.<br>                                                      | 






## Samples

[C# with Console App](../samples/Memento.Sample.ConsoleApp)

[Blazor App Shared](../samples/Memento.Sample.Blazor)

[Blazor Wasm App](../samples/Memento.Sample.BlazorWasm)

[Blazor Server App](../samples/Memento.Sample.BlazorServer)


