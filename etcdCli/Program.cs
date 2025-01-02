using System;
using dotnet_etcd;
using DotNetEnv;
using Grpc.Core;
using Google.Protobuf;
class EtcdCredentials
{
    public string? EtcdEndpoints { get; set; }
    public string? EtcdUsername { get; set; }
    public string? EtcdPassword { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // for debuging
        // Env.Load("../../../.env");

        Env.Load();
        EtcdCredentials etcdCredentials = new EtcdCredentials()
        {
            EtcdEndpoints = Environment.GetEnvironmentVariable("etcdEndpoints")!,
            EtcdUsername = Environment.GetEnvironmentVariable("etcdUsername")!,
            EtcdPassword = Environment.GetEnvironmentVariable("etcdPassword")!
        };

        var credentials = ChannelCredentials.Insecure;

        EtcdClient etcdClient = new EtcdClient("http://" + etcdCredentials.EtcdEndpoints, configureChannelOptions: options =>
        {
            options.Credentials = credentials;
        });

        Console.WriteLine("Welcome to the etcd CLI!, (Type 'help' for commands)");


        while (true)
        {
            string command = Console.ReadLine()!.ToLower().Trim();
            string[] parts = command.Split(' ');

            switch (parts[0])
            {
                case "help":
                    HandleHelp();
                    break;
                case "set":
                    HandleSet(etcdClient, etcdCredentials, parts[1], parts[2]);
                    break;
                case "get":
                    HandleGet(etcdClient, etcdCredentials, parts[1]);
                    break;
                case "delete":
                    HandleDelete(etcdClient, etcdCredentials, parts[1]);
                    break;
                case "list":
                    HandleList(etcdClient, etcdCredentials);
                    break;
                case "watch":
                    HandleWatch(etcdClient, etcdCredentials, parts[1]);
                    break;
                case "exit":
                    HandleExit();
                    break;
                default:
                    Console.WriteLine("Invalid command. Type 'help' for commands.");
                    break;
            }
        }
    }

    static Metadata Metadata(EtcdClient etcdClient, EtcdCredentials etcdCredentials)
    {
        var authRes = etcdClient.Authenticate(new Etcdserverpb.AuthenticateRequest()
        {
            Name = etcdCredentials.EtcdUsername,
            Password = etcdCredentials.EtcdPassword,
        });

        var metadata = new Metadata();
        metadata.Add(new Metadata.Entry("token", authRes.Token));
        return metadata;
    }

    static void HandleHelp()
    {
        Console.WriteLine(@"Commands:
            help                Show this help message
            set <key> <value>   Set a key-value pair in etcd
            get <key>           Get the value of a key from etcd
            delete <key>        Delete a key from etcd
            watch <key>         Watch a key in etcd
            list                List all keys stored in etcd
            exit                Exit the console
        ");
    }

    static void HandleSet(EtcdClient etcdClient, EtcdCredentials etcdCredentials, string key, string value)
    {
        try
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                etcdClient.Put(key, value, Metadata(etcdClient, etcdCredentials));

                Console.WriteLine($"Key '{key}' set with value '{value}'.");
            }
            else
            {
                Console.WriteLine("failed to set key-value pair. Please provide a key and value.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"failed to set key {key}: {ex.Message}");
            throw;
        }
    }

    static void HandleGet(EtcdClient etcdClient, EtcdCredentials etcdCredentials, string key)
    {
        try
        {
            if (!string.IsNullOrEmpty(key))
            {
                var res = etcdClient.Get(key, Metadata(etcdClient, etcdCredentials));

                Console.WriteLine($"Key '{key}' = '{res.Kvs[0].Value.ToStringUtf8()}'.");
            }
            else
            {
                Console.WriteLine("failed to get key. Please provide a key.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"failed to get key {key}: {ex.Message}");
            throw;
        }
    }

    static void HandleDelete(EtcdClient etcdClient, EtcdCredentials etcdCredentials, string key)
    {
        try
        {
            if (!string.IsNullOrEmpty(key))
            {
                etcdClient.Delete(key, Metadata(etcdClient, etcdCredentials));

                Console.WriteLine($"Key '{key}' deleted.");
            }
            else
            {
                Console.WriteLine("failed to delete key. Please provide a key.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"failed to delete key {key}: {ex.Message}");
            throw;
        }
    }

    static void HandleList(EtcdClient etcdClient, EtcdCredentials etcdCredentials)
    {
        try
        {
            var res = etcdClient.GetRange("", Metadata(etcdClient, etcdCredentials));

            if (res.Kvs.Count == 0)
            {
                Console.WriteLine("No keys found.");
                return;
            }

            foreach (var kv in res.Kvs)
            {
                Console.WriteLine($"Key: '{kv.Key.ToStringUtf8()}', Value: '{kv.Value.ToStringUtf8()}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"failed to list keys: {ex.Message}");
            throw;
        }
    }

    static void HandleWatch(EtcdClient etcdClient, EtcdCredentials etcdCredentials, string key)
    {
        try
        {
            if (!string.IsNullOrEmpty(key))
            {
                etcdClient.Watch(new Etcdserverpb.WatchRequest { CreateRequest = new Etcdserverpb.WatchCreateRequest { Key = ByteString.CopyFromUtf8(key) } }, response =>
                {
                    foreach (var ev in response.Events)
                    {
                        Console.WriteLine($"Event: {ev.Type}, Key: {ev.Kv.Key.ToStringUtf8()}, Value: {ev.Kv.Value.ToStringUtf8()}");
                    }
                }, Metadata(etcdClient, etcdCredentials));
            }
            else
            {
                Console.WriteLine("failed to watch key. Please provide a key.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"failed to watch key {key}: {ex.Message}");
            throw;
        }
    }

    static void HandleExit()
    {
        Environment.Exit(0);
    }
}
