using A2A;
using PropertyFinder.Models;
using PropertyFinder.Services;
using System.Net.ServerSentEvents;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using DotNetEnv;

Console.WriteLine("🏠 A2A Property Finder Client");
Console.WriteLine("==============================");
Console.WriteLine("Welcome to the AI-powered Property Finding Assistant!");
Console.WriteLine("Ask me about properties, and I'll help you find the perfect match.");

// Load environment variables from .env file
Env.Load();

// Initialize Property Service
PropertyService propertyService = new PropertyService();
Console.WriteLine($"✅ Loaded {propertyService.GetAllAvailableProperties().Count} available properties");

var groqApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
if (string.IsNullOrEmpty(groqApiKey))
{
    Console.WriteLine("❌ GROQ_API_KEY environment variable not set.");
    Console.WriteLine("Please ensure you have a .env file with your Groq API key.");
    return;
}

// Initialize OpenAI client for Groq
var openAIClient = new OpenAIClient(new ApiKeyCredential(groqApiKey), new OpenAIClientOptions
{
    Endpoint = new Uri("https://api.groq.com/openai/v1") // Groq's OpenAI-compatible base URL
});

var chatClient = openAIClient.GetChatClient("openai/gpt-oss-20b");

// Function to generate property-specific GPT response
async Task<string> GeneratePropertyAssistantResponse(string userInput, string propertyData)
{
    try
    {
        Console.WriteLine("🧠 Generating AI property assistant response...");
        var systemMessage = @"You are a professional real estate assistant integrated with an A2A (Agent-to-Agent) communication system. 
                            You help users find properties based on their needs and preferences. 
                            When property data is provided, use it to give specific recommendations and insights.
                            Be helpful, professional, and provide clear property information.
                            If no specific properties match the user's criteria, suggest alternatives or ask clarifying questions.
                            Keep responses informative but conversational.";

        var userMessage = string.IsNullOrEmpty(propertyData) 
            ? userInput 
            : $"User Query: {userInput}\n\nProperty Search Results:\n{propertyData}\n\nPlease provide a helpful response based on the user's query and the property data above.";

        var chatCompletion = await chatClient.CompleteChatAsync(
            [
                ChatMessage.CreateSystemMessage(systemMessage),
                ChatMessage.CreateUserMessage(userMessage)
            ]
        );
        
        return chatCompletion.Value.Content[0].Text;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error generating AI response: {ex.Message}");
        return $"I apologize, but I encountered an error while processing your request. However, here's the property information I found:\n\n{propertyData}";
    }
}

try
{
    // 1. Get the agent card
    Console.WriteLine("\n🔗 Connecting to A2A agent...");
    A2ACardResolver cardResolver = new(new Uri("http://localhost:5213/"));
    AgentCard echoAgentCard = await cardResolver.GetAgentCardAsync();

    Console.WriteLine($"✅ Connected to agent: {echoAgentCard.Name}");
    Console.WriteLine($"📝 Description: {echoAgentCard.Description}");
    Console.WriteLine($"🌊 Streaming support: {echoAgentCard.Capabilities?.Streaming}");

    // 2. Create an A2A client to communicate with the agent
    A2AClient agentClient = new(new Uri(echoAgentCard.Url));

    // 3. Interactive chat loop
    Console.WriteLine("\n💬 Starting interactive property search chat...");
    Console.WriteLine("Type 'exit' to quit, 'help' for examples, or ask me about properties!");
    Console.WriteLine("\nExample queries:");
    Console.WriteLine("- 'Show me apartments under $500,000'");
    Console.WriteLine("- 'I need a 3 bedroom house'");
    Console.WriteLine("- 'Find pet-friendly properties'");
    Console.WriteLine("- 'What condos do you have?'");
    Console.WriteLine(new string('-', 60));

    while (true)
    {
        Console.Write("\n🏠 You: ");
        string? userInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userInput))
            continue;

        if (userInput.ToLower() == "exit")
        {
            Console.WriteLine("👋 Thanks for using the A2A Property Finder! Goodbye!");
            break;
        }

        if (userInput.ToLower() == "help")
        {
            Console.WriteLine(@"
🏠 Property Search Help:
• Search by type: 'apartment', 'house', 'condo', 'loft', 'townhouse', 'penthouse'
• Filter by price: 'under $500,000', 'between $400,000 and $600,000'
• Filter by bedrooms: '2 bedroom', '3 bed house'
• Pet-friendly: 'pet friendly properties', 'allows dogs'
• General search: 'downtown Seattle', 'waterfront', 'modern'
• Type 'exit' to quit");
            continue;
        }

        try
        {
            // Process property query
            string propertyResults = propertyService.ProcessPropertyQuery(userInput);
            
            // Generate AI-enhanced response
            string aiResponse = await GeneratePropertyAssistantResponse(userInput, propertyResults);
            
            Console.WriteLine($"\n🤖 AI Assistant: {aiResponse}");

            // Create message for A2A agent
            Message userMessage = new()
            {
                Role = MessageRole.User,
                MessageId = Guid.NewGuid().ToString(),
                Parts = [new TextPart { Text = $"Property Query: {userInput}\n\nAI Response: {aiResponse}" }]
            };

            // Send to A2A Agent (Non-Streaming)
            Console.WriteLine("\n📡 Sending to A2A Agent for processing...");
            try
            {
                Message agentResponse = (Message)await agentClient.SendMessageAsync(new MessageSendParams { Message = userMessage });
                string agentResponseText = ((TextPart)agentResponse.Parts[0]).Text;
                Console.WriteLine($"� A2A Agent Response: {agentResponseText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error communicating with A2A agent: {ex.Message}");
            }

            Console.WriteLine(new string('-', 60));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error processing your request: {ex.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Fatal error: {ex.Message}");
    
    if (ex.Message.Contains("Connection refused") || ex.Message.Contains("No connection"))
    {
        Console.WriteLine("\n💡 Make sure the A2A agent is running on http://localhost:5213/");
        Console.WriteLine("   You may need to start the agent server first.");
        Console.WriteLine("   You can still search properties locally without the agent connection.");
        
        // Fallback to local property search
        Console.WriteLine("\n🔄 Falling back to local property search mode...");
        await RunLocalPropertySearch();
    }
    else if (ex.Message.Contains("API"))
    {
        Console.WriteLine("\n💡 Check your GROQ_API_KEY in the .env file");
        Console.WriteLine("   Make sure the API key is valid and has sufficient credits.");
    }
}

// Local property search fallback
async Task RunLocalPropertySearch()
{
    Console.WriteLine("\n💬 Local Property Search Mode");
    Console.WriteLine("Type 'exit' to quit or search for properties:");
    
    while (true)
    {
        Console.Write("\n🏠 You: ");
        string? userInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit")
            break;

        try
        {
            string propertyResults = propertyService.ProcessPropertyQuery(userInput);
            Console.WriteLine($"\n🔍 Search Results:\n{propertyResults}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}