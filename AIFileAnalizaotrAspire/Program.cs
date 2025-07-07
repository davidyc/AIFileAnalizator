using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var qdrant = builder.AddQdrant("qdrant")
                    .WithLifetime(ContainerLifetime.Persistent)
                    .WithDataBindMount(source: @"C:\Qdrant\Data");

var api = builder.AddProject<Projects.AIFileAnalizator_Api>("aifileanalizator-api")
      .WithReference(qdrant)
       .WaitFor(qdrant); 

builder.AddNpmApp("frontend", "../frontend")
    .WaitFor(api);


builder.Build().Run();
