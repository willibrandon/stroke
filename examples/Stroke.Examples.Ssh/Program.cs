using Stroke.Examples.Ssh;

// Example router - maps command line arguments to examples
// Faithful ports of Python Prompt Toolkit SSH examples
var examples = new Dictionary<string, Func<CancellationToken, Task>>(StringComparer.OrdinalIgnoreCase)
{
    ["asyncssh-server"] = AsyncsshServer.RunAsync,
};

// Handle Ctrl+C gracefully
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

if (args.Length == 0)
{
    Console.WriteLine("Stroke SSH Examples");
    Console.WriteLine("===================");
    Console.WriteLine();
    Console.WriteLine("Faithful ports of Python Prompt Toolkit SSH examples.");
    Console.WriteLine();
    Console.WriteLine("Usage: dotnet run -- <example-name>");
    Console.WriteLine();
    Console.WriteLine("Available examples:");
    foreach (var name in examples.Keys.Order())
    {
        Console.WriteLine($"  {name}");
    }
    Console.WriteLine();
    Console.WriteLine("After starting, connect with: ssh localhost -p 2222");
    Console.WriteLine();
    Console.WriteLine("NOTE: SSH examples require a host key file.");
    Console.WriteLine("Generate one with: ssh-keygen -t rsa -f ssh_host_key -N \"\"");
    return 0;
}

var exampleName = args[0];
if (!examples.TryGetValue(exampleName, out var runExample))
{
    Console.Error.WriteLine($"Error: Unknown example '{exampleName}'");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Available examples:");
    foreach (var name in examples.Keys.Order())
    {
        Console.Error.WriteLine($"  {name}");
    }
    return 1;
}

try
{
    Console.WriteLine($"Starting {exampleName} on port 2222...");
    Console.WriteLine("Connect with: ssh localhost -p 2222");
    Console.WriteLine("Press Ctrl+C to stop the server.");
    await runExample(cts.Token);
    return 0;
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nServer stopped.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
