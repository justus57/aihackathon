# Quick Start Guide

## Setting up your OpenAI API Key

1. **Get an OpenAI API Key:**
   - Go to [OpenAI Platform](https://platform.openai.com/api-keys)
   - Sign up or log in
   - Create a new API key
   - Copy the key (starts with `sk-`)

2. **Configure the Application:**
   - Open `appsettings.json` in the project folder
   - Replace `"YOUR_OPENAI_API_KEY_HERE"` with your actual API key

3. **Run the Application:**
   ```bash
   dotnet run
   ```

## Sample Run

The application will show a menu like this:

```
=== AI-Powered Code Memory Optimizer ===
This tool analyzes C# code and provides memory optimization suggestions using OpenAI.

Choose an option:
1. Analyze sample inefficient code
2. Analyze custom code file
3. Analyze code from clipboard
4. Exit

Enter your choice (1-4):
```

## Expected Output

When you choose option 1 (Analyze sample inefficient code), you'll see:

1. **Memory Usage Analysis:**
   - Before optimization metrics
   - After optimization metrics
   - Percentage improvement

2. **Optimization Suggestions:**
   - String concatenation improvements
   - Collection optimization recommendations
   - Boxing/unboxing elimination
   - Resource disposal patterns
   - LINQ optimization suggestions

3. **Optimized Code:**
   - Complete rewritten code with optimizations applied
   - Before/after comparisons for specific issues

## Cost Estimation

- Each analysis costs approximately $0.02-0.06 in OpenAI API usage
- The tool uses GPT-4 for high-quality analysis
- Monitor your usage at [OpenAI Usage Dashboard](https://platform.openai.com/usage)

## Troubleshooting

**If you get API errors:**
- Check your API key is correct
- Ensure you have API credits available
- Verify internet connectivity

**If the build fails:**
- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` to install packages
- Check all files are in the correct locations
