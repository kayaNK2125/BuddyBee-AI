# BuddyBee Frontend

The official web frontend for the **BuddyBee AI** assistant. Built with **React 19**, **TypeScript**, **Vite**, and **Three.js**.

---

## Features

- **Conversational UI**: Clean, responsive chat workspace supporting streaming typewriter effects, Markdown formatting, and multi-turn context.
- **Kinetic Chrysalis 3D Presence**: Custom Three.js background layer with particle simulations that visually react to assistant states (`idle`, `thinking`, `generating`, `error`).
- **Bring Your Own Key (BYOK)**: In-browser settings modal enabling users to provide Gemini and OpenAI API keys. Keys are kept strictly in `sessionStorage` and sent via HTTPS request headers, never written to backend databases.
- **Real-Time Connectivity Status**: Health check polling against the backend `/api/ping` endpoint with visual latency status.
- **Offline Landing Page**: Configurable maintenance mode toggle (`MAINTENANCE_MODE` in `src/App.tsx`) with roadmap highlights and GitHub links.

---

## Prerequisites

- [Node.js](https://nodejs.org/) (v20+ or v22+)
- npm (v10+)

---

## Getting Started

### 1. Install Dependencies
```bash
cd Frontend
npm ci
```

### 2. Configure Environment Variables
Copy the example environment configuration:
```bash
cp .env.example .env
```

Edit `.env` to point to your running BuddyBee API instance:
```dotenv
# Default local ASP.NET Core API development port:
VITE_API_URL=http://localhost:5168

# Or when using Docker Compose:
# VITE_API_URL=http://localhost:8080
```

> **Note:** `VITE_API_URL` is baked into static assets at build time. It must be accessible directly from the client's browser, not through internal container networks.

### 3. Run the Development Server
```bash
npm run dev
```
Open [http://localhost:5173](http://localhost:5173) in your browser.

---

## Available Scripts

| Command | Description |
| :--- | :--- |
| `npm run dev` | Starts the Vite development server with Hot Module Replacement (HMR) |
| `npm run build` | Runs TypeScript type checking (`tsc -b`) and bundles production assets with Vite |
| `npm run lint` | Lints the TypeScript and React codebase using `oxlint` |
| `npm run preview` | Serves the production build locally from `dist/` |

---

## Project Structure

```text
Frontend/
├── public/                  # Static assets and favicon
├── src/
│   ├── api/                 # API client, HTTP interceptors, endpoint contracts
│   ├── components/
│   │   ├── chat/            # Chat container, message items, command capsule
│   │   ├── layout/          # Header, navigation, and sidebar drawers
│   │   ├── offline/         # Maintenance and offline fallback landing page
│   │   ├── settings/        # BYOK API key configuration modal
│   │   └── three/           # Chrysalis 3D WebGL canvas component
│   ├── hooks/               # Custom hooks (conversation, typewriter, auto-scroll)
│   ├── state/               # React Context provider for conversation state
│   ├── styles/              # Design tokens, typography, and reset styles
│   ├── three/               # Three.js particle systems, shaders, and state machines
│   ├── types/               # Shared TypeScript models and DTO interfaces
│   ├── utils/               # Key storage and local state helpers
│   ├── App.tsx              # Shell layout and maintenance mode switcher
│   └── main.tsx             # Application entry point
├── Dockerfile               # Multi-stage production container build
├── nginx.conf               # Nginx server block with SPA route fallback
├── package.json             # Dependencies and build scripts
└── vite.config.ts           # Vite bundler configuration
```

---

## Docker Deployment

The frontend includes a multi-stage `Dockerfile` and `nginx.conf`:

```bash
docker build --build-arg VITE_API_URL=http://localhost:8080 -t buddybee-frontend .
docker run -p 5173:80 buddybee-frontend
```

The Nginx configuration handles SPA routing by redirecting all sub-routes to `/index.html` via `try_files $uri $uri/ /index.html;`.
