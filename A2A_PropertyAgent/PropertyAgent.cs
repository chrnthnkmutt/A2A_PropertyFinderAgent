using A2A;

namespace A2AAgent;

public class PropertyAgent
{
    public void Attach(ITaskManager taskManager)
    {
        taskManager.OnMessageReceived = ProcessMessageAsync;
        taskManager.OnAgentCardQuery = GetAgentCardAsync;
    }

    private async Task<Message> ProcessMessageAsync(MessageSendParams messageSendParams, CancellationToken ct)
    {
        // Get incoming message text
        string request = messageSendParams.Message.Parts.OfType<TextPart>().First().Text;

        // Process the property-related request
        string response = ProcessPropertyRequest(request);

        // Create and return a property agent response
        return new Message()
        {
            Role = MessageRole.Agent,
            MessageId = Guid.NewGuid().ToString(),
            ContextId = messageSendParams.Message.ContextId,
            Parts = [new TextPart() { Text = response }]
        };
    }

    private string ProcessPropertyRequest(string request)
    {
        // Analyze the request and provide property-related insights
        var lowerRequest = request.ToLower();

        if (lowerRequest.Contains("property query"))
        {
            return "✅ Property query processed and logged for analytics. The search results have been curated and enhanced with market insights.";
        }
        else if (lowerRequest.Contains("ai response"))
        {
            return "🏠 Property recommendation acknowledged. This interaction has been recorded for improving our property matching algorithms.";
        }
        else if (lowerRequest.Contains("hello") || lowerRequest.Contains("hi"))
        {
            return "🏠 Welcome to A2A Property Agent! I'm here to help facilitate property searches and provide real estate insights. How can I assist you today?";
        }
        else if (lowerRequest.Contains("property") || lowerRequest.Contains("real estate"))
        {
            return "🏡 I've received your property-related request. I can help with market analysis, property comparisons, and search refinements. The information has been processed and is ready for analysis.";
        }
        else
        {
            return $"🤖 A2A Property Agent received: {request}\n\n💡 I specialize in property-related communications and can help enhance your real estate search experience.";
        }
    }

    private async Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
    {
        return new AgentCard()
        {
            Name = "A2A Property Agent",
            Description = "A specialized agent for real estate and property search communications. Facilitates property queries, market insights, and search optimization.",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = new AgentCapabilities() { Streaming = true },
            Skills = [],
        };
    }
}
