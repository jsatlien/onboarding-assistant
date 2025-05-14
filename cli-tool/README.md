# Metadata Generator CLI Tool

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
