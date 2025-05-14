@echo off
echo Onboarding Assistant Development Environment

REM Check if OpenAI API key is set
if "%OPENAI_API_KEY%"=="" (
    echo Warning: OPENAI_API_KEY environment variable is not set.
    echo Please set your OpenAI API key using:
    echo set OPENAI_API_KEY=your_api_key
    echo.
)

if "%OPENAI_ASSISTANT_ID%"=="" (
    echo Warning: OPENAI_ASSISTANT_ID environment variable is not set.
    echo Please set your OpenAI Assistant ID using:
    echo set OPENAI_ASSISTANT_ID=your_assistant_id
    echo.
)

echo Starting development servers...

REM Start the backend API
start cmd /k "cd backend && echo Starting .NET Core API... && dotnet run"

REM Wait for backend to start
timeout /t 5

REM Start the demo application
start cmd /k "cd demo-app && echo Starting Demo Application... && npm run dev"

echo.
echo Development servers started:
echo - Backend API: http://localhost:5000
echo - Demo Application: http://localhost:3000
echo.
echo Press any key to stop all servers...

pause > nul

REM Kill all node and dotnet processes (be careful with this in a real environment)
taskkill /f /im dotnet.exe > nul 2>&1
taskkill /f /im node.exe > nul 2>&1

echo All servers stopped.
