﻿using System.Net.NetworkInformation;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Hangfire.Polly.Example.Utils;
using Testcontainers.PostgreSql;

namespace Hangfire.Polly.Example;

public interface IContainer
{
    IDatabaseContainer Instance { get; }
    ushort PublicPort { get; }
    string HostName { get; }
    string ConnectionString(string? database = null);
}

public class Container : IContainer
{
    private readonly ILogger<Container> _logger;
    private const int NgpsqlPort = 5432;
    private const string Database = "postgres";
    private const string Username = "postgres";
    private const string Password = "mysecretpassword";
    private const string ImageUri = "docker.io/library/postgres:13.18-alpine";

    private readonly PostgreSqlContainer _postgreSql;

    public IDatabaseContainer Instance => _postgreSql;
    public ushort PublicPort => _postgreSql.GetMappedPublicPort(NgpsqlPort);
    public string HostName => _postgreSql.Hostname;

    public Container(ILogger<Container> logger, IServiceProvider provider)
    {
        _logger = logger;
        _logger.LogInformation("Starting build process for {Host}", nameof(Container));
        var hostPort = GetPortFrom(NgpsqlPort);
        _postgreSql = new PostgreSqlBuilder()
                .WithImage(ImageUri)
                .WithLogger(provider.GetRequiredService<ILogger<PostgreSqlContainer>>())
                .WithPortBinding(hostPort, NgpsqlPort)
                .WithUsername(Username)
                .WithPassword(Password)
                .WithDatabase(Database)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(NgpsqlPort))
                .Build()
            ;

        _postgreSql.StartAsync().Wait();
    }

    public string ConnectionString(string? database = null)
    {
        var db = database.ExistsOr(Database);
        _logger.LogInformation("Generating connection string for: {Db}", db);

        var result = string.Join
        (
            ";",
            $"Server={HostName},{PublicPort}",
            $"Database={db}",
            $"User Id={Username}",
            $"Password={Password}",
            $"TrustServerCertificate=True"
        );
        return result;
    }

    private static int GetPortFrom(int from)
    {
        var ip = IPGlobalProperties.GetIPGlobalProperties();
        var ports = ip.GetActiveTcpListeners()
                .Concat(ip.GetActiveUdpListeners())
                .Select(x => x.Port)
                .ToHashSet()
            ;

        var result = from;
        while (ports.Contains(result))
        {
            result++;
        }

        return result;
    }
}