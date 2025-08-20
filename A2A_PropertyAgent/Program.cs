using A2A;
using A2A.AspNetCore;
using A2AAgent;
using Microsoft.AspNetCore.Builder;

Console.WriteLine("🏠 Starting A2A Property Agent Server...");

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

// Create and attach the PropertyAgent to the TaskManager
PropertyAgent agent = new PropertyAgent();
TaskManager taskManager = new TaskManager();
agent.Attach(taskManager);

Console.WriteLine("✅ Property Agent attached to Task Manager");

// Expose agent via A2A protocol
app.MapA2A(taskManager, "/agent");

Console.WriteLine("🌐 A2A Property Agent is running on http://localhost:5213/agent");
Console.WriteLine("📋 Agent Card available at: http://localhost:5213/agent/card");
Console.WriteLine("🔗 Ready to accept property-related communications!");

await app.RunAsync();