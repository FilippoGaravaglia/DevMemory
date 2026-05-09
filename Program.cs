using AiAgent.Application;

var agent = new AgentService();

Console.WriteLine("Scrivi qualcosa da salvare:");
var input = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(input))
{
    agent.AddMemory(input);
}

Console.WriteLine("\nMemoria attuale:");
agent.ShowMemory();