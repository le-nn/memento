# Memento

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Flexible and easy unidirectional store-pattern container for state management with Dependency Injection for Frontend app on .NET or JS/TS.

[DEMO](https://le-nn.github.io/memento/) with React in Typescript

# Basic Concept

You can define stores inspired by MVU patterns such as Flux and Elm to observe state changes more detail.

Some are inspired by Elm and MVU.
And Redux and Flux pattern are same too, but memento is not Redux and Flux.

#### Features

* Less boilarplate and simple usage 
* Is not flux or redux
* Observe detailed status with message patterns and makes it easier to monitor what happened within the application 
* Immutable and Unidirectional data flow
* Multiple stores but manged by single provider, so can observe and manage as one state tree
* Less rules have been established
* Fragile because there are fewer established rules than Redux and Flux

# Concepts and Data Flow

Note the concept is a bit different from Flux and Redux

<img width="800px" src="./Architecture.jpg"/>

## Rules

* State should always be read-only.
* To mutate state our app should mutate via mutation in the action method
* Every mutation that processes in the action will create new state to reflect the old state combined with the changes expected for the action.
* The UI then uses the new state to render its display.

<img width="800px" src="./overview.png"/>

# Compatibility and bindings

### The currently supported framework bindings are as follows

| Lang    | Framework                   |
| ------- | --------------------------- |
| TS/JS   | React                       |
| C#      | Blazor                      |

### Current packages and status

| Package Name    | Version | Lang       | Platform            | Package manager | Package provider       |
| --------------- | ------- | ---------- | ------------------- | --------------- | ---------------------- |
| memento.js      |         | TS/JS      | node.js 14 or later | npm or yarn     | https://www.npmjs.com/ |
| memento.react   |         | TS/JS      | node.js 14 or later | npm or yarn     | https://www.npmjs.com/ |
| Memento.Core    | 0.0.4   | C# or .NET | .NET 7 or later     | Nuget           | [Nuget](https://www.nuget.org/packages/Memento.Core) |
| Memento.Blazor  | 0.0.4   | Blazor     | .NET 7 or later     | Nuget           | [Nuget](https://www.nuget.org/packages/Memento.Blazor) |

# Documentation

[Basic Concept with Typescript/Javascript](./docs/BasicConcept/Tutorial.ts.md)

[Basic Concept with C#](./docs/BasicConcept/Tutorial.cs.md)

[React](./docs/React/GettingStandard.md)

[Blazor](./docs/Blazor/GettingStandard.md)

# Demo

Here is a demo site built with React in Typescript.
[DEMO](https://le-nn.github.io/memento/)


# License
Designed with ♥ le-nn. Licensed under the MIT License.
