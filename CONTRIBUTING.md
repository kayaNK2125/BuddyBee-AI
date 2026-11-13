# Contributing to BuddyBee

Thank you for your interest in contributing to **BuddyBee AI**! We welcome contributions, bug reports, and ideas from the community.

Please take a moment to review this guide before submitting changes.

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) 22 & npm (v10+)
- [Git](https://git-scm.com/)
- Optional: [Docker](https://www.docker.com/) and [MongoDB](https://www.mongodb.com/) (the backend includes an in-memory fallback if MongoDB is not present).

### Local Setup

1. **Fork and clone the repository:**
   ```bash
   git clone https://github.com/<your-username>/BuddyBee-AI.git
   cd BuddyBee-AI
   ```

2. **Configure Backend Settings:**
   ```bash
   cd BuddyBee.Api
   # Copy sample settings if needed, or configure User Secrets / Environment Variables
   dotnet restore
   dotnet build
   ```

3. **Configure Frontend Settings:**
   ```bash
   cd ../Frontend
   npm ci
   cp .env.example .env
   ```

4. **Run the Application:**
   - **Terminal 1 (Backend):**
     ```bash
     cd BuddyBee.Api
     dotnet run
     ```
   - **Terminal 2 (Frontend):**
     ```bash
     cd Frontend
     npm run dev
     ```

---

## Running Tests & Verification

Before submitting any code, verify that all builds, tests, and linting checks pass cleanly:

### 1. Backend Tests
The repository includes an xUnit test project [`BuddyBee.Tests`](BuddyBee.Tests/) that verifies arithmetic parsers, exact rational arithmetic, and tool calling:

```bash
# Run tests from the repository root:
dotnet test
```

### 2. Frontend Checks
Run the Oxlint linter and TypeScript compiler checks:

```bash
cd Frontend
npm run lint
npm run build
```

---

## Branching & Pull Request Workflow

1. **Create a topic branch:**
   Branch off the latest `main` or active development branch with a descriptive name:
   - `feat/add-new-tool`
   - `fix/expression-parser-exponent`
   - `docs/update-self-hosting-guide`

2. **Keep changes focused:**
   Submit pull requests that address a single issue or implement a cohesive feature. Avoid bundling unrelated formatting or dependency changes.

3. **Write clear commit messages:**
   Follow standard conventional commit conventions:
   - `feat: implement support for timezone conversion in TimeTool`
   - `fix: handle negative fractions in BigRational.ToString`
   - `test: add unit test cases for malformed scientific notation`

4. **Open a Pull Request:**
   - Provide a clear summary of what was changed and why.
   - Note any configuration or environment changes introduced.
   - Confirm that `dotnet test`, `npm run lint`, and `npm run build` pass.

---

## Code Style & Architectural Guidelines

### C# / .NET 10
- **Nullable Reference Types**: The project enables nullable reference types (`<Nullable>enable</Nullable>`). Ensure new code properly handles potential null values.
- **Provider & Tool Abstraction**: All tools implement [`ITool`](BuddyBee.Api/Interfaces/ITool.cs), and all AI model providers implement [`IAIProvider`](BuddyBee.Api/Interfaces/IAIProvider.cs). Never couple controllers directly to concrete provider SDKs.
- **Error Handling**: Use domain-specific exceptions (e.g. `AIProviderException`, `SearchProviderException`) and return safe `ToolResult` errors instead of allowing unhandled exceptions to crash the application.
- **Security & Privacy**: Never log or persist user API keys. Always respect the BYOK privacy model.

### TypeScript / React
- **Strict Typing**: Avoid `any`. Define and reuse interfaces in `src/types/` or `src/api/types.ts`.
- **Component Architecture**: Keep UI components modular and maintain CSS stylesheets alongside their components.
- **Key Storage**: User-provided provider keys must remain exclusively in `sessionStorage` (via `src/utils/keyStorage.ts`) and never be stored in persistent storage or written to conversational history.
- **Linting**: Ensure code adheres to `oxlint` rules defined in `.oxlintrc.json`.

---

## Reporting Issues

If you encounter a bug or have a feature request:
- Check existing [GitHub Issues](https://github.com/kayaNK2125/BuddyBee-AI/issues) to avoid duplicates.
- Use our issue templates for [Bug Reports](.github/ISSUE_TEMPLATE/bug_report.md) or [Feature Requests](.github/ISSUE_TEMPLATE/feature_request.md) to provide reproducible context.

---

## License

By contributing to BuddyBee, you agree that your contributions will be licensed under the **GNU Affero General Public License v3.0 (AGPLv3)**.

