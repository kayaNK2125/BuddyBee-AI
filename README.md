# BuddyBee AI

**An open-source, self-hostable AI assistant framework built around interchangeable AI providers, persistent memory, and practical tools.**

[Live Demo](https://buddy-bee-ai.vercel.app/)

BuddyBee is designed as an AI assistant environment rather than another standalone AI model. It provides a structured application layer around external AI providers, allowing the provider, memory, tools, and persistence layers to work together inside one assistant.

> **Status:** Public v1.0.0

---

## What is BuddyBee?

BuddyBee is an independent AI assistant project built with a replaceable provider architecture.

Instead of tying the entire application to one AI provider, BuddyBee separates the assistant from the underlying models:

```text
                         +----------------+
                         | Google Gemini  |
                         +----------------+
                                  |
User --> BuddyBee --> AI Router --+
                                  |
                         +----------------+
                         |    OpenAI      |
                         +----------------+

                    + Memory
                    + Tools
                    + Persistence
```

This allows BuddyBee's assistant layer to evolve independently of any single model provider.

## Current Features

### AI Providers
- Google Gemini
- OpenAI
- Provider abstraction through a common AI routing layer
- Automatic fallback between configured providers for supported transient failures

### Memory & Persistence
- Persistent conversations
- Memory V1
- MongoDB persistence
- In-memory fallback when MongoDB is not configured
- Conversation and memory separation

### Built-in Tools
- Calculator
- Time
- Automatic tool calling for supported provider flows

## API Key Modes

BuddyBee currently supports two provider modes.

### BuddyBee Managed

Use the provider capacity configured by the BuddyBee deployment for a quick start. Availability and limits depend on the deployment configuration.

### Bring Your Own Key (BYOK)

Connect your own Gemini or OpenAI API key and use your own provider account and quota.

BYOK is the recommended mode for users who want direct control over their provider usage and limits.

## Architecture

```text
+--------------------------------+
|        React Frontend          |
|       TypeScript + Vite        |
+---------------+----------------+
                |
               HTTPS
                |
                v
+--------------------------------+
|       ASP.NET Core API         |
|            .NET 10             |
+---------------+----------------+
                |
                v
+--------------------------------+
|           AI Router            |
|                                |
|        Gemini / OpenAI         |
+---------------+----------------+
                |
                v
+--------------------------------+
|      Memory | Tools | Chat     |
+---------------+----------------+
                |
                v
+--------------------------------+
|            MongoDB             |
|     Conversations + Memories   |
+--------------------------------+
```

The backend uses interfaces for the major application boundaries, including AI providers, conversation storage, memory, and tools.

This keeps provider and persistence implementations replaceable without changing the assistant's core architecture.

## API Key & Privacy Model

BuddyBee supports user-supplied provider credentials through BYOK mode.

When BYOK is used:

- Provider keys are kept in the browser's session storage.
- Keys are not stored in BuddyBee's conversation database.
- Keys are sent to the BuddyBee backend through request headers when making provider requests.
- The backend uses the supplied key for the provider request.
- Users remain responsible for their provider account, API usage, quotas, and applicable provider terms.

BuddyBee does not claim that provider credentials or user data are impossible to access in every deployment configuration. Self-hosters should review and secure their own infrastructure appropriately.

## Self-Hosting

BuddyBee is designed to be self-hostable.

### Backend

Requirements:
- .NET 10
- Gemini and/or OpenAI API credentials
- MongoDB (optional; the application can use in-memory storage when MongoDB is not configured)

The backend can be configured through environment variables or .NET User Secrets during local development.

Example configuration:

```env
GEMINI_API_KEY=your_key
OPENAI_API_KEY=your_key

MongoDb__ConnectionString=your_connection_string
MongoDb__DatabaseName=BuddyBeeDB
```

### Frontend

The frontend uses Vite.

Set:

```env
VITE_API_URL=https://your-buddybee-api.example.com
```

Then:

```bash
npm install
npm run build
```

For local development:

```bash
npm run dev
```

## Tech Stack

### Frontend
- React
- TypeScript
- Vite
- Three.js
- React Markdown
- Lucide React

### Backend
- ASP.NET Core
- .NET 10
- C#
- MongoDB Driver
- Google GenAI SDK
- OpenAI SDK

## Deployment

The current public deployment uses:

```text
Vercel
   |
   v
Railway
   |
   v
MongoDB Atlas
   |
   v
Gemini / OpenAI
```

## Public v1 Scope

BuddyBee v1 focuses on the core assistant foundation:

- AI provider abstraction
- Chat
- Memory
- Persistence
- Practical tools
- BYOK
- Self-hosting

Advanced personal and experimental capabilities are intentionally maintained separately from the public repository.

The public project will evolve based on actual usage, feedback, and contributions rather than exposing private capabilities simply for feature count.

## Roadmap

Potential future directions include:

- Additional tools and integrations
- More provider options
- Expanded memory capabilities
- Community contributions
- Additional self-hosting improvements

The roadmap is intentionally flexible.

## Open Source

BuddyBee is released under the GNU Affero General Public License v3.0 (AGPLv3).

See LICENSE for the complete license text.

AGPLv3 allows people to use, study, modify, and redistribute BuddyBee under the license's conditions, including requirements intended to preserve software freedom when modified versions are provided as network services.

## Third-Party Providers

BuddyBee is an independent project.

BuddyBee is not affiliated with, endorsed by, or sponsored by Google, OpenAI, or any other AI provider.

Gemini and OpenAI are third-party services. Users and self-hosters are responsible for complying with the applicable provider terms, usage policies, and billing requirements when using those services.

## Contributing

Contributions, bug reports, ideas, and improvements are welcome.

If you find a problem or have an idea for improving BuddyBee, open an issue or submit a pull request.

## License

Copyright (C) 2026 Mayank Gupta

BuddyBee is licensed under the GNU Affero General Public License v3.0.

See [LICENSE](LICENSE)