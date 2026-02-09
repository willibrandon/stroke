# Demos

Terminal recordings generated with [VHS](https://github.com/charmbracelet/vhs) from `.tape` source files.

## Requirements

- [VHS](https://github.com/charmbracelet/vhs) (`brew install vhs`)
- [.NET 10+](https://dotnet.microsoft.com/)

## Usage

Generate all demos:

```sh
for tape in demo/*.tape; do vhs "$tape"; done
```

Or a single demo:

```sh
vhs demo/sqlite-cli.tape
```

Output files (`.gif`, `.mp4`, `.webm`) are gitignored — regenerate from the `.tape` source.

## Troubleshooting

If you get `ERR_CONNECTION_REFUSED` from ttyd, try running with `sudo`:

```sh
sudo vhs demo/sqlite-cli.tape
```
