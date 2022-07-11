# Getting Standard

[React](https://reactjs.org/) is an UI framework.
This tutorials is for React in Typescript, but can be used in Javascript as well.

# Install

```
yarn add memento.react
```

or

```
npm install --save memento.react
```

## Use Typescript Decorator Setup on React

- Skip this section if you want to use it with js

Also for TypeScript you will need to enable ```experimentalDecorators``` and ```emitDecoratorMetadata``` flags within your tsconfig.json

If you want to build on bable (Gatsby, Create React App, etc.), you'd need the following Babel plugin.
add the babel package plugin-proposal-decorators.

```
yarn add -D @babel/plugin-proposal-decorators babel-plugin-transform-typescript-metadata
```
or
```
npm install -D @babel/plugin-proposal-decorators babel-plugin-transform-typescript-metadata
```

Add the following configuration to your ```.babelrc``` or ```babel.config.js``` file ```plugins``` section.

```
["@babel/plugin-proposal-decorators", { "legacy": true }],
["babel-plugin-transform-typescript-metadata"]
```

# Tutorial Overview

As a example, implement counters in various patterns.
All examples are written in TypeScript, but JavaScript can also be used.

## Simple usecase

- Skip this section if you do not need simple usecase

The most simple usecase is as a store container.
This is not supported observe detailed with message pattern.
Looks very simple and easy to handle,
but as it larger and becomes more complex, it can break the order.

[See](./Simple.md)

## Flux usecase

## Dependency Injection

Memento has a DI container.
DI is an abbreviation for Dependency Inject.
Services implemented as side effects such as 
HTTP Requests
asynchronous,
DB access,
and algorithm implementation 
can be accessed from actions using dependency injection.
It makes to be able to split any features.


[See](./DependencyInjection.md)

## API Refelence

[See](./API.md)

## DevTools

[See](./DevTools.md)