var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AIFileAnalizator_Api>("aifileanalizator-api");

builder.AddExecutable("frontend", "npm", "../frontend", "start" )
    .WithHttpEndpoint(name: "http", port: 5173);

//builder.AddQdrant("qdrant");

builder.Build().Run();
