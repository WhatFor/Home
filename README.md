# WhatFor

### Local Dev

Run a local dev server with:

```bash
sh dev.sh
```

which will:

- Start a file watcher to monitor `./src` for changes. Upon a change, execute `build.cs`.
- Start a local file server from `./dist` on port `3000`.

The `dev.sh` script invokes both `watch.sh` and `serve.sh`. Watch is responsible for calling `dotnet run build.cs`.

### Deployment

On the target server:

- `git fetch && git pull`
- `dotnet run build.cs`
- Point nginx to `./dist`
