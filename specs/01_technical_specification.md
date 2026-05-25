# Technical Specification: Working Candle Focus Timer

## 1. Overview

Working Candle is a minimalist Windows desktop application designed to help users focus on work through simple timer-based sessions. The application follows a "no feature bloat" philosophy, providing only essential functionality for 1-hour work sessions.

### 1.1 Goals
- Provide a distraction-free focus timer for 1-hour work sessions
- Minimal, intuitive user interface
- Native Windows experience
- Zero configuration required

### 1.2 Non-Goals
- Multiple timer durations
- Task tracking or productivity analytics
- Cross-platform support
- Cloud synchronization
- User accounts or profiles

## 2. Technology Stack

### 2.1 Platform
- **Target OS**: Windows 10 and later (64-bit)
- **Framework**: .NET Framework 4.8 or .NET 6+ Windows
- **UI Framework**: Windows Forms

### 2.2 Programming Language
- **Primary Language**: C#
- **Version**: C# 10.0 or later

### 2.3 Development Tools
- **IDE**: Visual Studio 2022 or later
- **Build System**: MSBuild
- **Version Control**: Git

## 3. Application Architecture

### 3.1 Application Type
- Single-window desktop application
- Single-threaded UI with timer-based updates
- No external dependencies or network connectivity required

### 3.2 Core Components

#### 3.2.1 MainForm (UI Layer)
- Inherits from `System.Windows.Forms.Form`
- Manages all UI elements and user interactions
- Handles state transitions
- Renders progress visualization

#### 3.2.2 TimerController (Business Logic)
- Manages timer state (Stopped, Running, Paused)
- Tracks elapsed time
- Triggers completion events
- Uses `System.Windows.Forms.Timer` for UI updates

#### 3.2.3 NotificationService (Audio System)
- Plays completion sound using `System.Media.SoundPlayer`
- Embeds sound file as resource

#### 3.2.4 StateManager (State Management)
- Implements state machine pattern
- Ensures valid state transitions
- Maintains consistency between UI and timer state

## 4. Application States

### 4.1 State Machine

```
         ┌─────────┐
    ┌───►│ STOPPED │◄───┐
    │    └─────────┘    │
    │         │         │
    │    [Start]        │
    │         │         │
    │         ▼         │
    │    ┌─────────┐   │
    │    │ RUNNING │   │
    │    └─────────┘   │
    │    │         │   │
    │[Stop]   [Pause]  │
    │    │         │   │
    │    │         ▼   │
    │    │    ┌────────┐
    │    │    │ PAUSED │
    │    │    └────────┘
    │    │         │
    └────┘    [Resume]
         ◄────────┘
         [Complete]
```

### 4.2 State Definitions

#### 4.2.1 STOPPED
- **Description**: Initial state, timer is not active
- **Timer Value**: 0:00:00
- **Progress**: 0%
- **Valid Transitions**: → RUNNING

#### 4.2.2 RUNNING
- **Description**: Timer is actively counting
- **Timer Value**: 0:00:00 to 1:00:00
- **Progress**: 0% to 100%
- **Valid Transitions**: → PAUSED, → STOPPED (on completion)
- **Update Frequency**: 1 second

#### 4.2.3 PAUSED
- **Description**: Timer is paused, retaining current progress
- **Timer Value**: Preserved from RUNNING state
- **Progress**: Frozen at current percentage
- **Valid Transitions**: → RUNNING (resume), → STOPPED

## 5. User Interface Specifications

### 5.1 Window Properties
- **Title**: "Working Candle"
- **Size**: 400×300 pixels (fixed, non-resizable)
- **Position**: Center screen on launch
- **Border Style**: Fixed single border
- **Minimize/Maximize**: Minimize enabled, Maximize disabled
- **Always on Top**: Optional (consider for future)

### 5.2 Layout Components

#### 5.2.1 Progress Bar
- **Type**: `ProgressBar` control or custom drawn
- **Position**: Top half of window, centered
- **Size**: 350×50 pixels
- **Style**: Continuous fill (not marquee)
- **Color**: System accent color or #4CAF50 (green)
- **Range**: 0-100 (percentage)
- **Visibility**: Hidden in STOPPED, visible in RUNNING and PAUSED

#### 5.2.2 Time Display
- **Type**: `Label` control
- **Format**: "MM:SS" (e.g., "45:30" for 45 minutes 30 seconds remaining)
- **Display Mode**: Countdown (shows remaining time, not elapsed)
- **Font**: Segoe UI, 36pt, Bold
- **Position**: Below progress bar, centered
- **Color**: System foreground color
- **Visibility**: Visible in RUNNING and PAUSED, hidden in STOPPED

#### 5.2.3 Buttons

**Start Button (STOPPED state)**
- **Text**: "Start 1h Timer"
- **Size**: 200×60 pixels
- **Position**: Centered vertically and horizontally
- **Style**: Primary button appearance

**Pause Button (RUNNING state)**
- **Text**: "Pause"
- **Size**: 150×45 pixels
- **Position**: Bottom center of window

**Resume Button (PAUSED state)**
- **Text**: "Resume"
- **Size**: 150×45 pixels
- **Position**: Bottom center left

**Stop Button (PAUSED state)**
- **Text**: "Stop"
- **Size**: 150×45 pixels
- **Position**: Bottom center right
- **Style**: Secondary or warning style (different color)

### 5.3 UI State Transitions

| State   | Progress Bar | Time Display | Buttons              |
|---------|-------------|--------------|----------------------|
| STOPPED | Hidden      | Hidden       | [Start 1h Timer]     |
| RUNNING | Visible     | Visible      | [Pause]              |
| PAUSED  | Visible     | Visible      | [Resume] [Stop]      |

## 6. Core Functionality

### 6.1 Timer Logic

#### 6.1.1 Duration
- **Fixed Duration**: 3600 seconds (1 hour)
- **No customization**: Hard-coded constant

#### 6.1.2 Update Mechanism
- **Interval**: 1000ms (1 second)
- **Implementation**: `System.Windows.Forms.Timer`
- **Tick Handler**: Updates elapsed time, progress bar, and time display

#### 6.1.3 Time Tracking
```csharp
// Pseudo-code
private DateTime _startTime;
private TimeSpan _pausedDuration;

void OnTimerTick()
{
    TimeSpan elapsed = DateTime.Now - _startTime - _pausedDuration;
    int secondsRemaining = 3600 - (int)elapsed.TotalSeconds;
    int progress = (int)(elapsed.TotalSeconds / 36); // 0-100
    
    // Display remaining time in MM:SS format (countdown)
    int minutesRemaining = secondsRemaining / 60;
    int secondsRemainingDisplay = secondsRemaining % 60;
    string timeDisplay = $"{minutesRemaining:D2}:{secondsRemainingDisplay:D2}";
    
    UpdateUI(timeDisplay, progress);
    
    if (secondsRemaining <= 0)
    {
        OnTimerComplete();
    }
}
```

### 6.2 Button Actions

#### 6.2.1 Start Button
1. Capture current time as `_startTime`
2. Reset `_pausedDuration` to zero
3. Start UI timer
4. Transition to RUNNING state
5. Update UI elements

#### 6.2.2 Pause Button
1. Stop UI timer
2. Capture pause start time
3. Transition to PAUSED state
4. Update UI elements

#### 6.2.3 Resume Button
1. Calculate pause duration and add to `_pausedDuration`
2. Restart UI timer
3. Transition to RUNNING state
4. Update UI elements

#### 6.2.4 Stop Button
1. Stop UI timer
2. Reset all timer values
3. Transition to STOPPED state
4. Update UI elements

## 7. Notification System

### 7.1 Completion Notification

#### 7.1.1 Trigger
- Activated when timer reaches 60:00 (3600 seconds)

#### 7.1.2 Sound Playback
- **Sound File**: Embedded as resource in executable
- **Format**: WAV file (for simplicity and .NET compatibility)
- **Duration**: 2-5 seconds
- **Type**: Pleasant, non-jarring alert sound (e.g., bell, chime)
- **Implementation**: `System.Media.SoundPlayer`

#### 7.1.3 Behavior
1. Play sound once
2. Reset timer to STOPPED state
3. Return window to initial state

### 7.2 Future Considerations
- Windows 10/11 toast notification (low priority)
- Taskbar icon flash (low priority)

## 8. Application Icon

### 8.1 Icon Requirements
- **Format**: .ico file
- **Sizes**: 16×16, 32×32, 48×48, 256×256 pixels
- **Design**: Candle flame or timer-related imagery
- **Style**: Simple, recognizable at small sizes
- **Colors**: Warm colors (orange, yellow) to represent a candle

### 8.2 Icon Usage
- Application executable icon
- Window title bar icon
- Taskbar icon
- Alt+Tab task switcher

## 9. Build and Deployment

### 9.1 Build Configuration

#### 9.1.1 Project Structure
```
WorkingCandle/
├── WorkingCandle.sln
├── WorkingCandle/
│   ├── WorkingCandle.csproj
│   ├── Program.cs
│   ├── MainForm.cs
│   ├── MainForm.Designer.cs
│   ├── TimerController.cs
│   ├── StateManager.cs
│   ├── NotificationService.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   └── Resources/
│       ├── icon.ico
│       └── completion.wav
└── README.md
```

#### 9.1.2 Build Settings
- **Configuration**: Release
- **Platform**: x64
- **Target Framework**: .NET Framework 4.8 or .NET 6+
- **Output Type**: Windows Application (not Console)
- **Assembly Name**: WorkingCandle.exe

#### 9.1.3 Optimization
- Enable code optimization
- Embed all resources
- Single-file executable (if .NET 6+)
- No debug symbols in release build

### 9.2 Deployment Package

#### 9.2.1 Deliverables
- **Primary**: WorkingCandle.exe (standalone executable)
- **Optional**: README.txt with basic instructions

#### 9.2.2 Dependencies
- .NET Framework 4.8 runtime (Windows 10+ has this pre-installed)
- OR .NET Desktop Runtime 6+ (if using .NET 6+)

#### 9.2.3 Installation
- **Method**: Xcopy deployment (no installer needed)
- **User Action**: Download .exe and double-click to run
- **Location**: User can place anywhere (Downloads, Program Files, etc.)

## 10. CI/CD Pipeline

### 10.1 Build Pipeline

#### 10.1.1 Trigger Events
- Push to `master` branch
- Creation of version tags (e.g., `v1.0.0`)
- Manual trigger

#### 10.1.2 Build Steps
1. **Checkout Code**: Clone repository
2. **Setup Environment**: Install .NET SDK
3. **Restore Dependencies**: `dotnet restore` or `nuget restore`
4. **Build Solution**: `msbuild WorkingCandle.sln /p:Configuration=Release /p:Platform=x64`
5. **Run Tests**: Execute unit tests (if any)
6. **Package Artifacts**: Collect .exe file

#### 10.1.3 Build Agent
- **Platform**: Windows (required for Windows Forms)
- **Image**: windows-latest or windows-2022

#### 10.1.4 Example GitHub Actions Workflow
```yaml
name: Build

on:
  push:
    branches: [ master ]
    tags: [ 'v*' ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '6.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: WorkingCandle
        path: WorkingCandle/bin/Release/**/*.exe
```

### 10.2 Release Pipeline

#### 10.2.1 Release Trigger
- Creation of git tag matching pattern `v*.*.*` (e.g., `v1.0.0`)

#### 10.2.2 Release Process
1. **Build**: Execute full build pipeline
2. **Create Release**: Create GitHub release from tag
3. **Attach Executable**: Upload WorkingCandle.exe as release asset
4. **Generate Notes**: Auto-generate release notes from commits

#### 10.2.3 Versioning
- **Scheme**: Semantic Versioning (MAJOR.MINOR.PATCH)
- **Assembly Version**: Match git tag version
- **File Version**: Match git tag version

#### 10.2.4 Example Release Configuration
```yaml
name: Release

on:
  push:
    tags: [ 'v*' ]

jobs:
  release:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '6.0.x'
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Create Release
      uses: softprops/action-gh-release@v1
      with:
        files: WorkingCandle/bin/Release/**/*.exe
        generate_release_notes: true
```

## 11. Error Handling

### 11.1 Exception Handling
- Wrap timer operations in try-catch blocks
- Log errors to Windows Event Log (optional)
- Graceful degradation (e.g., silent failure for sound playback)

### 11.2 Edge Cases
- **System Sleep**: Resume timer when system wakes
- **Date/Time Change**: Recalculate elapsed time
- **Resource Missing**: Handle missing sound file gracefully

## 12. Testing Strategy

### 12.1 Manual Testing
- Test all state transitions
- Verify timer accuracy over full 1-hour duration
- Test pause/resume multiple times
- Verify sound playback
- Test on different Windows versions (10, 11)

### 12.2 Automated Testing (Optional)
- Unit tests for TimerController logic
- Unit tests for StateManager transitions
- Mock-based tests for UI interactions

## 13. Performance Requirements

### 13.1 Resource Usage
- **Memory**: < 50 MB
- **CPU**: < 1% when idle, < 5% when updating UI
- **Startup Time**: < 1 second

### 13.2 Responsiveness
- UI updates within 100ms of user interaction
- No UI freezing or blocking operations

## 14. Future Enhancements (Out of Scope)

### 14.1 Possible Future Features
- Customizable timer duration
- Session history/statistics
- Keyboard shortcuts
- System tray support
- Multiple timer presets
- Dark mode

### 14.2 Technical Debt to Monitor
- Consider WPF migration for better UI capabilities
- Consider cross-platform support with Avalonia UI
- Consider local storage for session tracking

## 15. Acceptance Criteria

### 15.1 Functional Requirements
- ✓ Application starts in STOPPED state
- ✓ Start button initiates 1-hour timer
- ✓ Progress bar accurately reflects elapsed time (0% to 100%)
- ✓ Time display shows remaining time in countdown format (60:00 → 00:00)
- ✓ Pause button pauses timer without losing progress
- ✓ Resume button continues from paused state
- ✓ Stop button resets timer to initial state
- ✓ Sound plays when timer completes
- ✓ Application returns to STOPPED state after completion

### 15.2 Non-Functional Requirements
- ✓ Application window has custom icon
- ✓ Executable is self-contained and portable
- ✓ Application runs on Windows 10 and later
- ✓ UI is clean, simple, and intuitive
- ✓ No installation required

### 15.3 CI/CD Requirements
- ✓ CI pipeline builds executable on every commit
- ✓ Release pipeline attaches .exe to GitHub releases
- ✓ Versioning matches git tags

## 16. Glossary

- **State Machine**: Pattern for managing application states and valid transitions
- **Windows Forms**: .NET UI framework for Windows desktop applications
- **Xcopy Deployment**: Simple deployment method where files are copied directly
- **Semantic Versioning**: Version numbering scheme (MAJOR.MINOR.PATCH)

## 17. References

- [Windows Forms Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)
- [C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [.NET Application Publishing](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [GitHub Actions for .NET](https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net)

---

**Document Version**: 1.0  
**Last Updated**: 2026-05-25  
**Author**: Technical Specification based on specs/00_brief.md
