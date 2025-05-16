# Onboarding Assistant CLI Tools

This directory contains CLI tools for the Onboarding Assistant:

1. **Metadata Generator**: Scans source code to extract contextual metadata
2. **Embedding Upload Tool**: Converts metadata to embeddings and uploads them to OpenAI

## Complete Workflow

Here's the complete workflow for setting up and using the Onboarding Assistant:

1. **Set up an OpenAI Assistant**
   - Create an OpenAI account and get an API key
   - Create a new Assistant with appropriate instructions
   - Copy the Assistant ID

2. **Configure the tools**
   - Create or update `config/assistant_config.yaml` with your API key and Assistant ID

3. **Extract metadata from your application**
   - Run `python metadata_generator.py <source_dir> --output-dir ./output`
   - This generates JSON files with contextual information about your app

4. **Generate and upload embeddings**
   - Run `python upload_embeddings.py --config ./config/assistant_config.yaml`
   - This creates embeddings for each route and stores them locally

5. **Integrate the Onboarding Assistant into your application**
   - Follow the integration instructions in the main project README

## Metadata Generator

This tool scans source code files (.vue, .cs, .json) to extract contextual metadata for the Onboarding Assistant. The metadata is output as structured JSON files that can be used for RAG (Retrieval Augmented Generation) embedding with OpenAI.

## Prerequisites

- Python 3.6 or higher

## Installation

No installation is required. Simply ensure you have Python installed.

## Usage

```bash
python metadata_generator.py <source_dir> --output-dir <output_dir>
```

### Arguments

- `source_dir`: The directory containing the source code to scan (required)
- `--output-dir`, `-o`: The directory where metadata JSON files will be output (default: "./output")

### Example

```bash
# Scan the ServiceManager app and output metadata to the backend Data directory
python metadata_generator.py C:/path/to/ServiceManager --output-dir ../backend/Data
```

## Output Format

The tool generates JSON files with the following structure:

```json
{
  "route": "/dashboard",
  "description": "Main dashboard displaying widgets.",
  "elements": [
    {"id": "widget-list", "description": "List of user widgets"},
    {"id": "refresh-button", "description": "Refresh data"}
  ],
  "apiCalls": ["/api/dashboard"],
  "dependencies": ["WidgetComponent", "ChartComponent"],
  "userActions": ["View widgets", "Refresh data"]
}
```

## Framework Compatibility

The tool is designed to work with various frontend frameworks, with different levels of support:

- **Vue.js**: Optimized support with detailed extraction of components, props, and Vue-specific patterns
- **React**: Basic support for component extraction and JSX parsing
- **Angular**: Basic support for component and template extraction
- **Plain HTML/JS**: Can extract elements with IDs and comments
- **.NET/C#**: Extracts API routes and controller actions from backend code

While the tool works best with Vue.js applications, it will still extract useful metadata from other frameworks. Future versions will include enhanced support for React, Angular, and other popular frameworks.

## How It Works

The tool uses pattern matching and static analysis to extract:

1. **Routes**: Determined from file paths or explicit route definitions
2. **UI Elements**: Extracted from HTML elements with IDs or ref attributes
3. **API Calls**: Found in axios/fetch calls or API controller definitions
4. **Dependencies**: Identified from component imports and registrations
5. **User Actions**: Inferred from method names and event handlers

For best results, include descriptive comments in your code, especially near UI elements. When using non-Vue frameworks, adding clear ID attributes to important UI elements and descriptive comments will improve extraction quality.

## OpenAI Assistant Setup

Before extracting and uploading metadata, you need to set up an OpenAI Assistant to use with the Onboarding Assistant.

### Step 1: Set Up Your OpenAI Account

1. Visit https://platform.openai.com/ and sign in.
2. Go to https://platform.openai.com/api-keys.
3. Click "Create new secret key" and copy it.
4. Store this key securely — you'll add it to your `assistant_config.yaml` file later.

### Step 2: Manually Create an Assistant

Navigate to https://platform.openai.com/assistants.

Click "Create Assistant".

Fill out the fields as described below.

#### Assistant Configuration

| Field | What to Enter |
| ----- | ------------- |
| Name | Any friendly name for your assistant (e.g., Walli, Repair Assistant) |
| Instructions | Paste in the Assistant Instructions Template from below and fill in the variables |
| Model | See model guidance below |
| Tools | See tool guidance below |

#### Model Selection

| Model | Cost | Context Length | Notes |
| ----- | ---- | -------------- | ----- |
| gpt-4o (recommended) | 💰💰 | 128k tokens | Best speed + price/performance ratio. Great for production use. |
| gpt-4-turbo | 💰💰💰 | 128k tokens | Similar to gpt-4o, slightly slower, formerly the best option. |
| gpt-3.5-turbo | 💰 | 16k tokens | Budget-friendly, less accurate with instructions and metadata. |

✅ Recommendation: Use gpt-4o for the best balance of cost, speed, and understanding.

#### Tool Selection

✅ **File Search (RECOMMENDED – Enable)**
- Allows the assistant to retrieve answers from attached files.
- In the future, you may upload your metadata or configuration files here.
- For now, it can be left unused, but enable it to future-proof your setup.

❌ **Code Interpreter (DISABLE)**
- Used for writing and executing Python code inside the assistant.
- Useful for tasks like data analysis or config file validation.
- Not needed for this POC and introduces unnecessary complexity.

❌ **Functions (DISABLE)**
- Enables the assistant to call external APIs or frontend functions (e.g., highlight_element).
- While this is powerful for deep integration with your UI, it's out of scope for the current implementation.
- Leave it disabled for now. You can add this later if you build out the frontend bridge.

#### Assistant Instructions Template

```
You are {{ASSISTANT_NAME}}, a contextual onboarding assistant for a web application called {{APP_NAME}}, which operates in the {{APP_DOMAIN}} domain.

Your job is to help users:
- Understand what each page of the app does
- Find and interact with relevant UI elements on the current screen
- Navigate through the application as needed

You receive metadata from the developer that includes:
- The current route (e.g. "/workorders/new")
- A description of what the page does
- A list of UI elements with their labels or IDs
- Any associated API calls or user actions

Use this information to answer user questions clearly and concisely. Highlight UI elements when needed. Do not make assumptions about application internals beyond the provided metadata.

If users ask for your name or what to call you, tell them you are {{ASSISTANT_NAME}}.

Remain friendly, professional, and grounded in the current page context at all times.
```

> ⚠️ **Important**: Replace `{{APP_NAME}}` and `{{APP_DOMAIN}}` with your actual app name and domain/industry. Replace `{{ASSISTANT_NAME}}` with a friendly name for your assistant (e.g., "Riley", "Fixie").

### Step 3: Copy the Assistant ID

Once the assistant is saved, you'll see an ID like:

```
asst_abc123456789
```

Add this to your assistant config file:

```yaml
assistant_id: "asst_abc123456789"
```

You're now ready to run the metadata extraction and embedding tools!

## Embedding Upload Tool

This tool automates the process of generating and uploading text embeddings to OpenAI's Embedding API. It converts metadata files into embedding-ready format and stores the resulting embeddings locally.

### Prerequisites

- Python 3.6 or higher
- OpenAI Python client library (`pip install openai`)
- PyYAML (`pip install pyyaml`)
- tqdm (`pip install tqdm`)

### Configuration

Before using the tool, you need to create a YAML configuration file with your OpenAI API credentials and other settings. A sample configuration file is provided at `config/assistant_config.yaml`.

```yaml
# OpenAI API credentials
openai_api_key: "sk-your-api-key-here"  # Replace with your actual OpenAI API key
assistant_id: "asst_your-assistant-id-here"  # Replace with your OpenAI Assistant ID

# Embedding model to use
embedding_model: "text-embedding-3-small"

# Paths for metadata and embeddings
metadata_path: "./output"  # Path to the metadata files generated by metadata_generator.py
index_path: "./output/embeddings.json"  # Path where embeddings will be stored

# Format for embeddings (currently only OpenAI is supported)
embedding_format: "openai"  # Future: pinecone, weaviate
```

### Usage

```bash
python upload_embeddings.py --config <config_file_path>
```

### Arguments

- `--config`: Path to the YAML configuration file (default: "./config/assistant_config.yaml")
- `--force`: Force processing of all files, even if unchanged
- `--verbose`: Print verbose output
- `--quiet`: Suppress progress bar and non-error output

### Example Workflow

```bash
# Step 1: Generate metadata from source code
python metadata_generator.py ./src --output-dir ./output

# Step 2: Upload embeddings to OpenAI
python upload_embeddings.py --config ./config/assistant_config.yaml
```

### Features

- **File Hashing**: Tracks file changes to avoid reprocessing unchanged files
- **Error Handling**: Retries failed API calls with exponential backoff
- **Progress Tracking**: Shows a progress bar with ETA
- **Detailed Logging**: Logs errors to `logs/errors.log`

### Output

The tool generates two output files:

1. **embeddings.json**: Contains the embeddings for each route

```json
{
  "/route": {
    "embedding": [0.123, 0.456, ...],
    "text": "ROUTE: /route\n\nDESCRIPTION: ..."
  }
}
```

2. **hashes.json**: Tracks file hashes to avoid reprocessing unchanged files

```json
{
  "/route": {
    "hash": "ae1f2c...4e",
    "last_updated": "2025-05-15T14:00:00Z"
  }
}
```
