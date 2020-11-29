using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace GrpcGreeterClient
{
    // For asynchronous code, the gRPC generates methods with the same name in the proto file with an "Async" suffix.
    // If the code is not asynchronous, the types, methods and code generated in runtime can be called normally using the same name as
    // declared in the .proto file.
    class Program
    {
        private static string[] _availableServices = new string[3] { "Hello World Service", "Version Service", "Readkey Service" };
        private const int LIMIT = 1000;

        async static Task Main(string[] args)
        {
            try
            {
                int serviceIndex = 0;
                Console.WriteLine("Choose a service to make 100 requests: ");

                foreach (var service in _availableServices)
                {
                    Console.WriteLine($"{serviceIndex} -> {service}");
                    serviceIndex++;
                }

                using var channel = GrpcChannel.ForAddress("https://localhost:5001");
                int commandCount = 0;

                switch (_availableServices[int.Parse(Console.ReadLine())].ToUpper())
                {
                    case "HELLO WORLD SERVICE":
                        var helloWorldClient = new Greeter.GreeterClient(channel);

                        while (commandCount < LIMIT)
                        {
                            var reply = await helloWorldClient.SayHelloAsync(
                                          new HelloRequest { Name = "My first GRPC request" });
                            Console.WriteLine("Greeting: " + reply.Message);
                            commandCount++;
                        }

                        break;
                    case "VERSION SERVICE":
                        var versionClient = new Version.VersionClient(channel);

                        while (commandCount < LIMIT)
                        {
                            var reply = await versionClient.GetVersionAsync(
                                          new VersionRequest { Client = typeof(Program).Namespace });
                            Console.WriteLine("Version: " + reply.Version);
                            Console.WriteLine($"Person message: Name: {reply.Person.Name} Age: {reply.Person.Age}");
                            commandCount++;
                        }
                        break;

                    case "READKEY SERVICE":
                        ReadkeyResponse response;
                        ConsoleKey pressedKey;
                        string stringResponse;
                        var readKeyClient = new Readkey.ReadkeyClient(channel);

                        Console.WriteLine("Each key pressed will be to the server.");
                        Console.WriteLine("Press a key to start sending commands: ");
                        pressedKey = Console.ReadKey().Key;

                        while (pressedKey != ConsoleKey.Escape)
                        {
                            response = await readKeyClient.GetKeyResponseAsync(new ReadkeyRequest { Key = pressedKey.ToString() });
                            stringResponse = response.Received ? "true" : "false";
                            pressedKey = Console.ReadKey().Key;
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}