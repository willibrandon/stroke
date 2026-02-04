# SSH Examples

Port of Python Prompt Toolkit's SSH server example.

## Usage

```bash
# Generate host key in PEM format (required by FxSsh)
ssh-keygen -t rsa -m PEM -f ssh_host_key -N ""

# Run the server
dotnet run -- asyncssh-server
```

Connect from another terminal:

```bash
ssh -o StrictHostKeyChecking=no localhost -p 2222
```
