# Working Candle Focus Timer

A minimalist Windows desktop application designed to help users focus on work through simple timer-based sessions. Following a "no feature bloat" philosophy, Working Candle does one thing well: providing a simple, distraction-free focus timer.

## Features

- **Simple 1-hour focus timer** - Start with one click
- **Clean, distraction-free interface** - Minimal UI that stays out of your way
- **Pause/resume functionality** - Take breaks without losing progress
- **Audio notification on completion** - Pleasant sound when timer completes
- **Native Windows application** - No installation required, runs standalone
- **Lightweight** - Uses less than 50 MB of memory
- **Fast startup** - Launches in under 1 second

## System Requirements

- **Operating System**: Windows 10 or later (64-bit)
- **Runtime**: .NET 8.0 Runtime (Windows Desktop)
- **Memory**: < 50 MB RAM
- **Disk Space**: < 5 MB

## Installation

### Download and Run

1. Download the latest `WorkingCandle.exe` from the [Releases](../../releases/latest) page
2. Double-click to run (no installation required)
3. If prompted, install the [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0/runtime)

The application is a single executable file with no external dependencies beyond the .NET runtime.

## Usage

### Starting a Focus Session

1. Launch `WorkingCandle.exe`
2. Click **"Start 1h Timer"** to begin a 60-minute focus session
3. The timer displays countdown in MM:SS format
4. A progress bar shows visual progress

### Pausing and Resuming

- Click **"Pause"** to temporarily stop the timer
- Click **"Resume"** to continue from where you left off
- The timer accurately tracks elapsed time, accounting for pause duration

### Stopping a Session

- Click **"Stop"** (available when paused) to reset the timer
- The application returns to the initial state

### Completion

- When the timer reaches 00:00, a pleasant sound plays
- The application automatically resets to the initial state
- You can then start a new focus session

## Development

### Building from Source

#### Prerequisites

- .NET 8.0 SDK or later
- Windows OS (for Windows Forms development)
- Visual Studio 2022 or later (optional, but recommended)

#### Build Steps

```bash
# Clone the repository
git clone https://github.com/hq9000/working_candle.git
cd working_candle

# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build --configuration Debug

# Build in Release mode
dotnet build --configuration Release

# Run the application
dotnet run --project WorkingCandle

# Publish as single-file executable
dotnet publish WorkingCandle/WorkingCandle.csproj --configuration Release -o ./publish /p:PublishSingleFile=true
```

### Project Structure

```
WorkingCandle/
├── .github/
│   └── workflows/
│       ├── build.yml           # CI build workflow
│       └── release.yml         # Release workflow
├── WorkingCandle/              # Main project directory
│   ├── WorkingCandle.csproj    # Project file
│   ├── Program.cs              # Application entry point
│   ├── MainForm.cs             # Main form implementation
│   ├── MainForm.Designer.cs    # Form designer code
│   ├── StateManager.cs         # State machine implementation
│   ├── TimerController.cs      # Timer logic
│   ├── NotificationService.cs  # Sound playback
│   └── Resources/              # Application resources
│       ├── completion.wav      # Completion sound
│       └── icon.ico            # Application icon
├── specs/                      # Technical specifications
│   ├── 01_technical_specification.md
│   └── 02_implementation_plan.md
├── WorkingCandle.sln           # Visual Studio solution file
└── README.md                   # This file
```

### Architecture

The application follows a simple, event-driven architecture:

- **StateManager**: Manages application state (Stopped, Running, Paused) with validated transitions
- **TimerController**: Handles timer logic with accurate time tracking using DateTime calculations
- **NotificationService**: Plays audio notifications using embedded resources
- **MainForm**: Windows Forms UI that responds to state changes and user input

### Testing

The application uses the following testing approach:

- **Manual Testing**: Full test checklist for each release (see `specs/03_test_checklist_v1.0.0.md`)
- **CI/CD Validation**: Automated builds on every push to master
- **Release Testing**: Comprehensive QA before each release

## CI/CD

The project uses GitHub Actions for continuous integration and deployment:

### Build Pipeline (`.github/workflows/build.yml`)
- Triggers on push to `master` branch
- Builds in Release configuration
- Uploads build artifacts

### Release Pipeline (`.github/workflows/release.yml`)
- Triggers on version tags (`v*.*.*`)
- Builds production-ready single-file executable
- Creates GitHub release with auto-generated notes
- Attaches `WorkingCandle.exe` as release asset

### Creating a Release

```bash
# Create and push a version tag
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# The release workflow will automatically:
# 1. Build the application
# 2. Create a GitHub release
# 3. Attach the executable
```

## Technical Details

### Timer Accuracy

The timer uses `DateTime` calculations rather than incrementing counters to ensure accuracy:
- Tracks start time and pause duration
- Calculates elapsed time on each tick
- Remains accurate even if the system sleeps

### State Management

Valid state transitions:
- Stopped → Running (Start)
- Running → Paused (Pause)
- Running → Stopped (Complete)
- Paused → Running (Resume)
- Paused → Stopped (Stop)

Invalid transitions are prevented by the StateManager.

### Resource Embedding

All resources (icon, sound) are embedded in the executable:
- No external file dependencies
- Graceful degradation if resources fail to load

## Performance Characteristics

- **Memory Usage**: < 50 MB
- **CPU Usage**: < 1% idle, < 5% active
- **Startup Time**: < 1 second
- **Executable Size**: < 5 MB
- **UI Responsiveness**: < 100ms for all interactions

## Known Limitations

- Timer duration is fixed at 1 hour (by design)
- Windows only (requires Windows Forms)
- No system tray support in v1.0
- No customization options (by design - no feature bloat)

## Version History

See [Releases](../../releases) for version history and changelogs.

## License

This project is provided as-is for personal use.

## Contributing

This project follows a "no feature bloat" philosophy. The core functionality is intentionally minimal and complete. Feature requests that add complexity are unlikely to be accepted.

Bug reports and performance improvements are welcome. Please open an issue on GitHub.

## Philosophy

Working Candle follows a "no feature bloat" philosophy - it does one thing well: providing a simple, distraction-free focus timer. This intentional minimalism means:

- No customizable timer durations
- No themes or appearance options
- No statistics or history tracking
- No integrations with other tools
- No configuration files or settings

The goal is to provide a tool that just works, every time, without any complexity or distractions.

## Support

- **Issues**: [GitHub Issues](../../issues)
- **Documentation**: See the [specs/](./specs) directory
- **Questions**: Open a GitHub Discussion
