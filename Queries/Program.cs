using System.Reflection;
using Marten;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Marten.Exceptions;
using Npgsql;
using Oakton;
using Queries;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Http;
using Wolverine.Marten;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddWolverine(
    opts =>
    {
        opts.ApplicationAssembly = Assembly.GetEntryAssembly();

        opts.OnException<NpgsqlException>()
            .Or<MartenCommandException>()
            .RetryWithCooldown(
                TimeSpan.FromMilliseconds(50),
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(250)
            );

        opts.Policies.AutoApplyTransactions();

        opts.UseRabbitMq(new Uri(builder.Configuration.GetConnectionString("rabbitmq")))
            .EnableWolverineControlQueues()
            .AutoProvision();

        opts.ListenToRabbitQueue("integration_events")
            .UseDurableInbox();
    }
);

builder.Services.AddWolverineHttp();

builder.Services
    .AddMarten(
        sp =>
        {
            var opts = new StoreOptions
            {
                DatabaseSchemaName = "queries",
                Events = { AppendMode = EventAppendMode.Quick },
            };
            
            opts.Schema.For<Applicant>()
                .Duplicate(x => x.LastName) // B-tree индекс по LastName
                .Duplicate(x => x.IdentificationsCount)
                .Duplicate(x => x.LatestIdentificationStatus); // По count

            return opts;
        }
    )
    .AddAsyncDaemon(DaemonMode.Solo)
    .IntegrateWithWolverine(x => x.UseFastEventForwarding = true)
    .UseLightweightSessions()
    .UseNpgsqlDataSource();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapWolverineEndpoints();

return await app.RunOaktonCommands(args);