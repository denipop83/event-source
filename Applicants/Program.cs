using System.Reflection;
using Applicants;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Exceptions;
using Npgsql;
using Oakton;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Http;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var storeOptions = new StoreOptions
{
    DatabaseSchemaName = "applicants",
    Events = { AppendMode = EventAppendMode.Quick },
};

storeOptions.Projections.Snapshot<ApplicantAggregate>(SnapshotLifecycle.Inline);

builder.Services.AddOpenApi();

builder.Services.AddWolverineHttp();

builder.Host.UseWolverine(
    opts =>
    {
        opts.ApplicationAssembly = Assembly.GetEntryAssembly();
                
        opts.OnException<NpgsqlException>()
            .Or<MartenCommandException>()
            .RetryWithCooldown(
                TimeSpan.FromMilliseconds(50), 
                TimeSpan.FromMilliseconds(100), 
                TimeSpan.FromMilliseconds(250));

        opts.Policies.AutoApplyTransactions();
                
        opts.UseRabbitMq(new Uri(builder.Configuration.GetConnectionString("rabbitmq")))
            .EnableWolverineControlQueues()
            .AutoProvision();

        opts.PublishAllMessages()
            .ToRabbitExchange("events", ex => ex.IsDurable = true)
            .UseDurableOutbox();
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapWolverineEndpoints();
return await app.RunOaktonCommands(args);