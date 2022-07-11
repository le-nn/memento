# Dev Tools

Redux Dev Tools is a browser plugin for Chrome and Firefox.

[Github](https://github.com/zalmoxisus/redux-devtools-extension)
[Chrome Store](https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd)

Originally for Redux, but now available on Memento.

## To enable integration, follow these steps

1. Install Extension from official store.
2. Call ```devToolMiddleware``` with options and add result to ```createProvider``` options.

NOTE: ReduxDevTools allows the user to alter the state of your store directly.
This might be a security flaw, so you should only reference this package in Debug builds.

```ts
import { devToolMiddleware,createProvider } from "@memento/core"

export const provider = createProvider({
    stores: [
        ... // Any Stores
    ],
    middlewares: [
        devToolMiddleware({
            // Your options
        })
    ],
    services: [
        ... // Any Services
    ]
})
```

## Options 

| Args    | Type        | Default           | Description             |
| ------- | ----------- | ----------------- | ----------------------- |
| name    | string      | Memento  Devtool  | Instance name           |
| maxAge  | number      |                   |                         |
| latency | number      | 800               |                         |
| trace   | boolean     | false             |                         |