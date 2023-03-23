# Basic Concept

You can define stores inspired by MVU patterns such as Flux and Elm to observe state changes more detail.

Some are inspired by Elm and MVU.
And Redux and Flux pattern are same too, but memento is not Redux and Flux.

The state is always changed via reducer, which makes it easier to monitor what happened within the application.

Features

* Less boilarplate and simple usage 
* Is not flux or redux 
* Observe detailed status with command patterns and makes it easier to monitor what happened within the application 
* Immutable and Unidirectional data flow
* Multiple stores but manged by single provider, so can observe and manage as one state tree
* Less rules have been established
* Fragile because there are fewer established rules than Redux and Flux

![](../../Architecture.jpg)

## Vocabulary

Since not to be confused with other pattern concepts, we call in Memento's own words.

#### Provider
It is central place that includes and manages all stores.

#### Store
It has Action and Mutaion on a one-to-one basis with the managed State. There can be multiple stores, please separate according to requirements.

#### Reducer
  Reducer creates a new state from the command. It can be considered Update in MVU pattern, Reducer in Redux or Flux.

#### Message
 Represents something that happened. You think of an Message as an event that describes something that happened in the application. It can be considered Message in MVU pattern, Action in Redux or Flux. Message can have one payload to change the information.

#### Action
Instead of mutating the state, actions dispatch command and change state via Reducers.

Actions can contain arbitrary asynchronous operations.

#### State
The state you want to manage and observe.

#### Services 
It is a concept that summarizes the features with side effects, used with Dependency Injection.

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

## Samples

[C# with Console App](../samples/Memento.Sample.ConsoleApp)

[Blazor App Shared](../samples/Memento.Sample.Blazor)

[Blazor Wasm App](../samples/Memento.Sample.BlazorWasm)

[Blazor Server App](../samples/Memento.Sample.BlazorServer)


