# Onboarding Assistant

An AI-powered Onboarding Assistant designed as a standalone, reusable SDK component capable of dynamically guiding users through an application's workflows, answering user questions contextually, and highlighting UI elements.

## Project Overview

This project implements a complete Onboarding Assistant solution with three main components:

1. **Frontend SDK**: A Vue.js component library that provides a floating assistant UI
2. **Backend API**: A .NET Core API that integrates with OpenAI's Assistant API
3. **CLI Tool**: A Node.js-based CLI tool for generating contextual metadata files for direct upload to OpenAI Assistant

Additionally, a demo application is included to showcase the Onboarding Assistant in action.

## Project Structure

```
OnboardingAssistant/
├── frontend/                 # Vue.js SDK component library
│   ├── src/
│   │   ├── components/       # Vue components
│   │   │   ├── AssistantWidget.vue
│   │   │   └── ConversationArea.vue
│   │   ├── assets/           # Static assets
│   │   └── index.js          # Main entry point
│   ├── package.json          # Dependencies
│   ├── vite.config.js        # Build configuration
│   └── Dockerfile            # Docker configuration
│
├── backend/                  # .NET Core API
│   ├── Controllers/          # API endpoints
│   │   └── AssistantController.cs
│   ├── Services/             # Business logic
│   │   ├── OpenAIService.cs
│   │   └── EmbeddingService.cs
│   ├── Models/               # Data models
│   │   ├── QueryRequest.cs
│   │   ├── QueryResponse.cs
│   │   └── RouteContext.cs
│   ├── Data/                 # Preprocessed metadata
│   │   ├── dashboard.json
│   │   └── work-order-create.json
│   ├── Program.cs            # Application entry point
│   ├── appsettings.json      # Configuration
│   └── Dockerfile            # Docker configuration
│
├── OnboardingAssistantCLI/    # Metadata generation tool
│   ├── bin/                  # CLI executable
│   ├── lib/                  # Core functionality
│   └── README.md             # CLI tool documentation
│
├── demo-app/                 # Demo application
│   ├── src/
│   │   ├── views/            # Page components
│   │   ├── components/       # UI components
│   │   ├── assets/           # Static assets
│   │   ├── App.vue           # Root component
│   │   └── main.js           # Entry point
│   ├── package.json          # Dependencies
│   └── vite.config.js        # Build configuration
│
├── docker-compose.yml        # Docker Compose configuration
└── README.md                 # Project documentation
```

## Getting Started

### Prerequisites

- Node.js 18+ and npm for frontend development
- .NET 8 SDK for backend development
- Node.js 18+ for the CLI tool
- OpenAI API key for Assistant API integration
- Docker and Docker Compose (optional, for containerized deployment)

### Installation

#### 1. Clone the repository

```bash
git clone <repository-url>
cd OnboardingAssistant
```

#### 2. Set up the Frontend SDK

```bash
cd frontend
npm install
npm run build
```

#### 3. Set up the Backend API

```bash
cd backend
dotnet restore
```

Update the `appsettings.json` file with your OpenAI API key and Assistant ID:

```json
{
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY",
    "AssistantId": "YOUR_ASSISTANT_ID"
  }
}
```

#### 4. Set up the Demo Application

```bash
cd demo-app
npm install
```

## Running the Application

### Running the Backend API

```bash
cd backend
dotnet run
```

The API will be available at `http://localhost:5000`.

### Running the Frontend SDK Development Server

```bash
cd frontend
npm run dev
```

### Running the Demo Application

```bash
cd demo-app
npm run dev
```

The demo application will be available at `http://localhost:3000`.

### Using Docker Compose

To run the entire application stack using Docker Compose:

```bash
# Set your OpenAI API key and Assistant ID as environment variables
export OPENAI_API_KEY=your_api_key
export OPENAI_ASSISTANT_ID=your_assistant_id

# Start the services
docker-compose up -d
```

## Using the CLI Tool

The Onboarding Assistant CLI tool generates contextual metadata files for direct upload to OpenAI Assistant.

### Installation

```bash
npm install -g onboarding-assistant-cli
```

### Usage

```bash
# Generate frontend context files
onboarding-assistant generate-frontend -s <source-dir> -o <output-dir> -c <config-file>

# Generate backend context files
onboarding-assistant generate-backend -s <source-dir> -o <output-dir> -c <config-file>
```

The generated context files can be directly uploaded to your OpenAI Assistant through the OpenAI web interface.

For more details, see the [CLI Tool README](https://github.com/jsatlien/onboarding-assistant-cli).

## Integrating the Onboarding Assistant

### In a Vue.js Application

1. Install the package:

```bash
npm install --save path/to/onboarding-assistant-sdk
```

2. Import and register the component:

```javascript
import { createApp } from 'vue'
import App from './App.vue'
import OnboardingAssistant from 'onboarding-assistant-sdk'

const app = createApp(App)
app.use(OnboardingAssistant)
app.mount('#app')
```

3. Use the component in your template:

```html
<template>
  <div id="app">
    <!-- Your app content -->
    
    <!-- Onboarding Assistant Widget -->
    <AssistantWidget 
      api-base-url="http://localhost:5000/api"
      assistant-name="Your Assistant Name"
    />
  </div>
</template>
```

## Customization

The Onboarding Assistant can be customized through props passed to the `AssistantWidget` component:

- `api-base-url`: The base URL for the backend API
- `assistant-name`: The name of the assistant
- `initial-position`: The initial position of the floating widget

## Deployment

The application is containerized using Docker and can be deployed to any container orchestration platform like Kubernetes.

For production deployment, make sure to:

1. Set up proper environment variables for API keys
2. Configure CORS settings in the backend
3. Set up proper SSL/TLS for secure communication

### CORS Configuration

The Onboarding Assistant backend includes built-in CORS support to allow cross-origin requests from client applications. This is essential when your client application is hosted on a different domain than the backend API.

#### Development Environment

In development, the backend allows requests from any origin by default, making local testing easier.

#### Production Environment

For production, you must explicitly specify which origins are allowed to access the API:

1. **Configure allowed origins** in `appsettings.json`:

   ```json
   "AllowedOrigins": [
     "https://your-client-app.com",
     "https://another-client-app.com"
   ]
   ```

2. **Environment-specific settings** can be configured in `appsettings.Production.json`.

#### Alternative Approaches

If CORS becomes problematic in your deployment scenario, consider these alternatives:

1. **Reverse Proxy**: Configure a reverse proxy (like Nginx) to serve both your client application and the Onboarding Assistant backend from the same domain, eliminating CORS issues entirely.

2. **Backend-for-Frontend (BFF)**: Create a lightweight API within your client application's domain that forwards requests to the Onboarding Assistant backend.

3. **Same-Origin Deployment**: Deploy the Onboarding Assistant backend to the same domain as your client application.

## License

MIT

