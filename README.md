# AI-Powered Code Memory Optimizer

This project uses OpenAI's GPT-4 to analyze C# code and provide memory optimization suggestions. It identifies common memory inefficiencies and rewrites code to be more memory-efficient.

## Features

- **AI-Powered Analysis**: Uses OpenAI GPT-4 to analyze code for memory optimization opportunities
- **Memory Usage Tracking**: Measures and displays memory usage before and after optimizations
- **Detailed Suggestions**: Provides specific recommendations with before/after code examples
- **Multiple Input Methods**: 
  - Analyze sample inefficient code
  - Analyze custom code files
  - Analyze code from clipboard input
- **Code Rewriting**: Generates optimized versions of your code
- **Save Optimized Code**: Option to save the optimized code to a file

## Memory Optimization Areas

The tool focuses on these key areas:

1. **String Operations**: StringBuilder usage, string interpolation
2. **Collection Management**: Proper capacity initialization, efficient LINQ
3. **Object Allocation**: Reducing unnecessary object creation
4. **Resource Disposal**: Proper using statements and IDisposable patterns
5. **Boxing/Unboxing**: Eliminating performance-costly conversions
6. **Lazy Initialization**: Deferring expensive operations
7. **Value vs Reference Types**: Optimal type selection
8. **Async/Await Patterns**: Memory-efficient asynchronous code
9. **Cache-Friendly Structures**: Optimizing data layout
10. **LINQ Optimizations**: Efficient query patterns

## Setup

### Prerequisites

- .NET 8.0 or later
- OpenAI API key (get one from [OpenAI Platform](https://platform.openai.com/api-keys))

### Installation

1. Clone or download the project
2. Navigate to the project directory
3. Install dependencies:
   ```bash
   dotnet restore
   ```

### Configuration

1. Open `appsettings.json`
2. Replace `"YOUR_OPENAI_API_KEY_HERE"` with your actual OpenAI API key:
   ```json
   {
     "OpenAI": {
       "ApiKey": "sk-your-actual-api-key-here"
     }
   }
   ```

Alternatively, you can set the API key as an environment variable:
```bash
set OpenAI__ApiKey=sk-your-actual-api-key-here
```

## Usage

### Running the Application

```bash
dotnet run
```

### Menu Options

1. **Analyze Sample Code**: Analyzes the included sample inefficient code
2. **Analyze Custom File**: Analyzes a C# file you specify
3. **Analyze Clipboard Code**: Analyzes code you paste into the console
4. **Exit**: Closes the application

### Sample Analysis Output

The tool provides:
- Memory usage before and after optimization
- Detailed optimization suggestions with severity levels
- Before/after code comparisons
- Complete optimized code
- Estimated memory improvement percentage

## Sample Code

The project includes sample inefficient code that demonstrates common memory problems:

- String concatenation in loops
- Boxing/unboxing in collections
- Inefficient LINQ operations
- Improper resource disposal
- Unnecessary object allocations

## Technical Details

### Memory Profiling

The tool uses .NET's built-in memory profiling capabilities:
- `GC.GetTotalMemory()` for allocated memory
- `Process.WorkingSet64` for working set
- `GC.CollectionCount()` for garbage collection metrics

### AI Integration

- Uses OpenAI's GPT-4 model for code analysis
- Sends structured prompts for consistent analysis
- Parses JSON responses for optimization suggestions

### Architecture

- **Models**: Data structures for analysis results
- **Services**: Core logic for optimization and memory profiling
- **Sample Code**: Examples of inefficient code patterns

## Limitations

- Currently supports C# code only
- Memory improvements are estimated (not measured from actual execution)
- Requires internet connection for OpenAI API calls
- API usage costs apply based on OpenAI pricing

## Contributing

Feel free to contribute by:
- Adding more sample inefficient code patterns
- Improving memory measurement accuracy
- Adding support for other languages
- Enhancing the AI prompts for better analysis

## License

This project is provided as-is for educational and development purposes.

## Support

If you encounter issues:
1. Check your OpenAI API key is valid and has sufficient credits
2. Ensure you have internet connectivity
3. Verify .NET 8.0 is installed
4. Check that all NuGet packages are restored

## API Costs

This tool uses OpenAI's API, which has usage costs. The tool typically uses:
- Model: GPT-4
- Average tokens per analysis: 1,000-2,000
- Cost: Approximately $0.02-0.06 per analysis (as of 2024)

Monitor your usage at [OpenAI Usage Dashboard](https://platform.openai.com/usage).
