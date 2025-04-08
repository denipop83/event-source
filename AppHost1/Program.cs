var builder = DistributedApplication.CreateBuilder(args);

var marten = builder.AddPostgres("marten")
    .WithPgAdmin();

var username = builder.AddParameter("username", value: "guest");
var password = builder.AddParameter("password", value: "guest");
var rabbit = builder.AddRabbitMQ("rabbitmq", username, password)
    .WithManagementPlugin();


var identificationsService = builder.AddProject<Projects.Identifications>("svc-identifications")
    .WithReference(marten)
    .WithReference(rabbit)
    .WaitFor(marten)
    .WaitFor(rabbit);

var queriesService = builder.AddProject<Projects.Queries>("svc-queries")
    .WithReference(marten)
    .WithReference(rabbit)
    .WaitFor(marten)
    .WaitFor(rabbit);

builder.Build().Run();