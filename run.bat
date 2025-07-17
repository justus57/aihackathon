@echo off
echo =============================================================
echo    AI-Powered Code Memory Optimizer
echo =============================================================
echo.
echo This tool analyzes C# code for memory optimization opportunities
echo using OpenAI's GPT-4 model.
echo.
echo Make sure you have set your OpenAI API key in appsettings.json
echo before running this application.
echo.
echo Press any key to start the application...
pause > nul

cd /d "%~dp0aihackathon"
dotnet run

echo.
echo Application has finished running.
echo Press any key to exit...
pause > nul
