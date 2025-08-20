# A2A Property Finder

A comprehensive property finding system using Agent-to-Agent (A2A) communication with AI-powered assistance.

## Overview

This system consists of two main components:

1. **A2A_PropertyAgent** - The server agent that processes property-related communications
2. **A2A_PropertyClient** - The interactive client that allows users to search for properties using natural language

## Features

- 🏠 **Natural Language Property Search** - Ask questions in plain English
- 🤖 **AI-Powered Responses** - Enhanced responses using Groq's GPT models
- 🔗 **Agent-to-Agent Communication** - Demonstrates A2A protocol
- 📊 **Rich Property Data** - Mock JSON data with 8 diverse properties
- 🔍 **Smart Filtering** - Search by price, bedrooms, type, amenities, and more
- 🐕 **Pet-Friendly Options** - Filter for pet-friendly properties

## Property Database

The system includes mock data for 8 properties in the Seattle area:
- Downtown apartments
- Suburban houses
- Luxury condos and penthouses
- Affordable starter homes
- Various price ranges ($325k - $1.2M)

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK
- Valid Groq API key (for AI responses)

### 1. Start the Property Agent Server

```bash
cd A2A_PropertyAgent
dotnet run
```

The server will start on `http://localhost:5213/agent`

### 2. Run the Property Client

```bash
cd A2A_PropertyClient
dotnet run
```

### 3. Environment Setup

Make sure you have a `.env` file in the A2A_PropertyClient directory with your Groq API key:

```
GROQ_API_KEY=your_actual_api_key_here
```

## How to Use

Once the client is running, you can ask natural language questions about properties:

### Example Queries

**Search by Property Type:**
- "Show me apartments"
- "I'm looking for a house"
- "What condos do you have?"

**Search by Price:**
- "Properties under $500,000"
- "Between $400,000 and $600,000"
- "Budget around $750k"

**Search by Bedrooms:**
- "I need a 3 bedroom house"
- "2 bedroom apartments"
- "4+ bedrooms"

**Special Requirements:**
- "Pet-friendly properties"
- "Allows dogs"
- "Properties with parking"

**Location-based:**
- "Downtown Seattle"
- "Waterfront properties"
- "Near schools"

**General Search:**
- "Modern apartment"
- "Family home with yard"
- "Luxury properties"

### Commands
- Type `help` for usage examples
- Type `exit` to quit the application

## System Architecture

### A2A Protocol Flow

1. **Client** processes user input and searches local property database
2. **AI Enhancement** - Groq API generates intelligent responses
3. **A2A Communication** - Sends enhanced data to property agent
4. **Agent Processing** - Property agent logs and analyzes the interaction
5. **Response** - Agent provides acknowledgment and insights

### Fallback Mode

If the A2A agent server is not running, the client automatically falls back to local-only property search mode, allowing you to still search properties without the agent communication.

## Project Structure

```
A2A_PropertyFinder/
├── A2A_PropertyAgent/          # Server agent
│   ├── Program.cs              # Server startup
│   ├── PropertyAgent.cs        # Property-specific agent logic
│   └── EchoAgent.cs           # Original echo agent (kept for reference)
└── A2A_PropertyClient/         # Interactive client
    ├── Program.cs              # Main client application
    ├── Property.cs             # Property model
    ├── PropertyService.cs      # Property search logic
    ├── properties.json         # Mock property data
    └── .env                    # API keys (not in repo)
```

## Sample Property Data

The system includes properties like:
- **Modern Downtown Apartment** - $450k, 2BR/2BA, downtown Seattle
- **Luxury Waterfront Condo** - $950k, 3BR/3BA, lake views
- **Family Home with Large Yard** - $825k, 4BR/3BA, Redmond
- **Affordable Starter Home** - $425k, 2BR/1.5BA, Renton

## Technology Stack

- **.NET 8.0** - Application framework
- **A2A Protocol** - Agent-to-agent communication
- **OpenAI SDK** - AI integration via Groq
- **System.Text.Json** - JSON serialization
- **DotNetEnv** - Environment variable management

## Development Notes

- The property search uses intelligent text matching across titles, descriptions, addresses, and amenities
- Price ranges are extracted from natural language using regex patterns
- The AI responses are contextually aware of the specific property data found
- The A2A communication demonstrates real-world agent interaction patterns

## Troubleshooting

**Connection Issues:**
- Ensure the A2A_PropertyAgent server is running on port 5213
- Check firewall settings if needed

**API Issues:**
- Verify your Groq API key is valid and has sufficient credits
- Check the .env file formatting

**Build Issues:**
- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` in both project directories
