# Working Candle Focus Timer

A minimalist Windows desktop application designed to help users focus on work through simple timer-based sessions.

## Features

- Simple 1-hour focus timer
- Clean, distraction-free interface
- Pause/resume functionality
- Audio notification on completion
- Native Windows application

## Requirements

- Windows 10 or later (64-bit)
- .NET 8.0 Runtime (Windows)

## Usage

1. Download the latest release from the [Releases](../../releases) page
2. Run `WorkingCandle.exe`
3. Click "Start 1h Timer" to begin a focus session
4. Use Pause/Resume buttons as needed
5. The application will play a sound when the timer completes

## Development

### Building from Source

#### Prerequisites

- .NET 8.0 SDK or later
- Windows OS (for Windows Forms)

#### Build Steps

```bash
# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Run the application
dotnet run --project WorkingCandle
```

### Project Structure

```
WorkingCandle/
├── WorkingCandle.sln          # Visual Studio solution file
├── WorkingCandle/             # Main project directory
│   ├── WorkingCandle.csproj   # Project file
│   ├── Program.cs             # Application entry point
│   ├── Form1.cs               # Main form
│   ├── Properties/            # Assembly properties
│   └── Resources/             # Application resources
├── specs/                     # Specifications and documentation
└── README.md                  # This file
```

## CI/CD

The project uses GitHub Actions for continuous integration:
- Builds automatically on push to `master` branch
- Artifacts are uploaded for each successful build
- Release builds will be attached to GitHub releases

## License

This project is provided as-is for personal use.

## Philosophy

Working Candle follows a "no feature bloat" philosophy - it does one thing well: providing a simple, distraction-free focus timer.
