using System.Reflection;
using Core;
using Identifications;
using Marten;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Marten.Exceptions;
using Npgsql;
using Oakton;
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

        opts.Policies.UseDurableLocalQueues();
        opts.Policies.AutoApplyTransactions();

        var conn = builder.Configuration.GetConnectionString("rabbitmq") ?? "amqp://guest:guest@localhost:5672";
        opts.UseRabbitMq(new Uri(conn))
            .DeclareExchange(
                "events",
                ex =>
                {
                    ex.BindQueue("events");
                    ex.IsDurable = true;
                }
            ).DeclareExchange(
                "integration_events",
                ex =>
                {
                    ex.BindQueue("integration_events");
                    ex.IsDurable = true;
                }
            )
            .EnableWolverineControlQueues()
            .AutoProvision();

        opts.Publish(
            p =>
            {
                p.MessagesImplementing<IdentificationEvent>();
                p.ToRabbitExchange("events", ex => ex.IsDurable = true)
                    .UseDurableOutbox();
            }
        );


        opts.Publish(
            p =>
            {
                p.MessagesImplementing<IntegrationEvent>();
                p.ToRabbitExchange("integration_events", ex => ex.IsDurable = true)
                    .UseDurableOutbox();
            }
        );
        
        opts.ListenToRabbitQueue("events").UseDurableInbox();
    }
);

builder.Services.AddWolverineHttp();

builder.Services
    .AddMarten(
        sp =>
        {
            var opts = new StoreOptions
            {
                DatabaseSchemaName = "identifications",
                Events = { AppendMode = EventAppendMode.Quick },
            };
            
            opts.DisableNpgsqlLogging = true;

            opts.Projections.Snapshot<Identification>(SnapshotLifecycle.Async);
            
            opts.Schema.For<Identification>().AddSubClassHierarchy();

            var listener = new IdentificationChangeListener(sp.GetRequiredService<IServiceScopeFactory>());
            opts.Projections.AsyncListeners.Add(listener);

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