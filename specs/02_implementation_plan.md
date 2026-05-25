# Implementation Plan: Working Candle Focus Timer

## Document Overview

This document provides a step-by-step implementation plan for the Working Candle Focus Timer application, based on the technical specification in `01_technical_specification.md`.

**Document Version**: 1.0  
**Last Updated**: 2026-05-25  
**Based On**: Technical Specification v1.0

---

## Implementation Phases

The implementation is divided into 6 phases, each building upon the previous one. Each phase includes specific tasks, deliverables, and testing requirements.

---

## Phase 1: Project Setup and Infrastructure

**Duration**: 1-2 days  
**Goal**: Establish the development environment, project structure, and basic CI/CD pipeline

### Tasks

#### 1.1 Repository and Project Initialization
- [ ] Create Visual Studio solution file (`WorkingCandle.sln`)
- [ ] Create C# Windows Forms project (`WorkingCandle.csproj`)
- [ ] Configure project for .NET Framework 4.8 or .NET 6+
- [ ] Set output type to Windows Application (not Console)
- [ ] Set target platform to x64
- [ ] Configure assembly name as `WorkingCandle.exe`

#### 1.2 Project Structure Setup
```
WorkingCandle/
├── WorkingCandle.sln
├── WorkingCandle/
│   ├── WorkingCandle.csproj
│   ├── Program.cs (entry point)
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   └── Resources/ (to be populated later)
└── README.md
```

#### 1.3 Initial CI/CD Pipeline
- [ ] Create `.github/workflows/build.yml` for continuous integration
- [ ] Configure GitHub Actions to use Windows runner
- [ ] Set up build triggers for `master` branch pushes
- [ ] Configure basic build steps:
  - Checkout code
  - Setup .NET SDK
  - Restore dependencies
  - Build solution in Release configuration
- [ ] Verify CI pipeline runs successfully

#### 1.4 Version Control Configuration
- [ ] Create `.gitignore` for Visual Studio/C# projects
- [ ] Exclude build artifacts (`bin/`, `obj/`, `*.user`, etc.)
- [ ] Create initial commit with project skeleton

### Deliverables
- ✓ Buildable Visual Studio solution
- ✓ Working CI pipeline on GitHub Actions
- ✓ Clean repository structure

### Testing
- Build project locally in Visual Studio
- Verify CI pipeline completes successfully
- Verify no build warnings or errors

---

## Phase 2: Core Application Structure

**Duration**: 2-3 days  
**Goal**: Implement basic application architecture and state management

### Tasks

#### 2.1 Create Core Components (Empty Shells)
- [ ] Create `MainForm.cs` and `MainForm.Designer.cs`
  - Inherit from `System.Windows.Forms.Form`
  - Set window title to "Working Candle"
  - Set window size to 400×300 pixels
  - Set FormBorderStyle to FixedSingle
  - Disable maximize button
  - Set StartPosition to CenterScreen
- [ ] Create `TimerController.cs` class
  - Add fields for timer state tracking
  - Add placeholder methods for Start, Pause, Resume, Stop
- [ ] Create `StateManager.cs` class
  - Define enum for states (Stopped, Running, Paused)
  - Implement state transition validation
- [ ] Create `NotificationService.cs` class
  - Add placeholder for sound playback

#### 2.2 Implement State Machine
- [ ] In `StateManager.cs`, implement:
  - `CurrentState` property (enum: Stopped, Running, Paused)
  - `CanTransitionTo(State newState)` method
  - `TransitionTo(State newState)` method with validation
  - State change event/delegate for UI updates
- [ ] Define valid state transitions:
  - Stopped → Running
  - Running → Paused
  - Running → Stopped (on completion)
  - Paused → Running (resume)
  - Paused → Stopped

#### 2.3 Wire Components Together
- [ ] In `Program.cs`:
  - Initialize application
  - Create and show MainForm
- [ ] In `MainForm.cs`:
  - Instantiate StateManager
  - Instantiate TimerController
  - Instantiate NotificationService
  - Subscribe to state change events

#### 2.4 Basic Form Display
- [ ] Implement MainForm initialization
- [ ] Ensure application window displays correctly
- [ ] Verify window properties (size, title, border style)

### Deliverables
- ✓ Application launches and displays window
- ✓ State machine with validated transitions
- ✓ Core components instantiated and wired

### Testing
- Run application and verify window appears
- Unit test state transitions in StateManager
- Verify window properties match specification
- Test that invalid state transitions are prevented

---

## Phase 3: Timer Logic Implementation

**Duration**: 3-4 days  
**Goal**: Implement complete timer functionality with accurate time tracking

### Tasks

#### 3.1 Implement TimerController
- [ ] Add private fields:
  - `System.Windows.Forms.Timer _uiTimer` (1000ms interval)
  - `DateTime _startTime`
  - `TimeSpan _pausedDuration`
  - `DateTime? _pauseStartTime`
  - `const int TIMER_DURATION_SECONDS = 3600`
- [ ] Implement `Start()` method:
  - Capture current time as `_startTime`
  - Reset `_pausedDuration` to zero
  - Start `_uiTimer`
  - Fire TimerStarted event
- [ ] Implement `Pause()` method:
  - Stop `_uiTimer`
  - Capture pause start time
  - Fire TimerPaused event
- [ ] Implement `Resume()` method:
  - Calculate pause duration and add to `_pausedDuration`
  - Restart `_uiTimer`
  - Clear `_pauseStartTime`
  - Fire TimerResumed event
- [ ] Implement `Stop()` method:
  - Stop `_uiTimer`
  - Reset all timer values
  - Fire TimerStopped event

#### 3.2 Implement Timer Tick Logic
- [ ] Create `_uiTimer.Tick` event handler
- [ ] Calculate elapsed time:
  ```csharp
  TimeSpan elapsed = DateTime.Now - _startTime - _pausedDuration;
  ```
- [ ] Calculate remaining time:
  ```csharp
  int secondsRemaining = 3600 - (int)elapsed.TotalSeconds;
  ```
- [ ] Calculate progress percentage:
  ```csharp
  int progress = (int)(elapsed.TotalSeconds / 36); // 0-100
  ```
- [ ] Fire TimerTick event with time and progress data
- [ ] Check for completion:
  ```csharp
  if (secondsRemaining <= 0) OnTimerComplete();
  ```

#### 3.3 Add Timer Events
- [ ] Define event delegates:
  - `TimerStarted`
  - `TimerPaused`
  - `TimerResumed`
  - `TimerStopped`
  - `TimerTick(int secondsRemaining, int progressPercent)`
  - `TimerCompleted`

#### 3.4 Edge Case Handling
- [ ] Handle system sleep/wake scenario
- [ ] Handle system time changes
- [ ] Add bounds checking for time calculations
- [ ] Ensure timer never shows negative values

### Deliverables
- ✓ Complete timer logic with accurate time tracking
- ✓ Proper pause/resume functionality
- ✓ Event-driven architecture for UI updates

### Testing
- Unit test timer calculations with mock DateTime
- Test Start, Pause, Resume, Stop sequences
- Test pause/resume multiple times in sequence
- Test timer completion at 3600 seconds
- Manual test: Run full 1-hour timer (or use reduced duration for testing)
- Verify time accuracy with external stopwatch

---

## Phase 4: User Interface Implementation

**Duration**: 3-5 days  
**Goal**: Create complete, state-aware user interface

### Tasks

#### 4.1 Create UI Controls (STOPPED State)
- [ ] Add Start button to MainForm:
  - Text: "Start 1h Timer"
  - Size: 200×60 pixels
  - Position: Centered vertically and horizontally
  - Add Click event handler
- [ ] Initially hide progress bar and time label

#### 4.2 Create UI Controls (RUNNING State)
- [ ] Add ProgressBar control:
  - Size: 350×50 pixels
  - Position: Top half, centered
  - Style: Continuous
  - Range: 0-100
  - Initially hidden
- [ ] Add time display Label:
  - Format: "MM:SS"
  - Font: Segoe UI, 36pt, Bold
  - Position: Below progress bar, centered
  - Initially hidden
- [ ] Add Pause button:
  - Text: "Pause"
  - Size: 150×45 pixels
  - Position: Bottom center
  - Initially hidden

#### 4.3 Create UI Controls (PAUSED State)
- [ ] Add Resume button:
  - Text: "Resume"
  - Size: 150×45 pixels
  - Position: Bottom center left
  - Initially hidden
- [ ] Add Stop button:
  - Text: "Stop"
  - Size: 150×45 pixels
  - Position: Bottom center right
  - Initially hidden

#### 4.4 Implement UI State Management
- [ ] Create `UpdateUIForState(State state)` method
- [ ] Implement UI updates for STOPPED state:
  - Show Start button
  - Hide Progress bar, Time display, Pause, Resume, Stop buttons
- [ ] Implement UI updates for RUNNING state:
  - Hide Start button
  - Show Progress bar, Time display, Pause button
  - Hide Resume, Stop buttons
- [ ] Implement UI updates for PAUSED state:
  - Hide Start, Pause buttons
  - Show Progress bar, Time display, Resume, Stop buttons

#### 4.5 Wire Button Events
- [ ] Start button Click:
  - Validate state transition
  - Call TimerController.Start()
  - Transition StateManager to Running
  - Update UI
- [ ] Pause button Click:
  - Call TimerController.Pause()
  - Transition StateManager to Paused
  - Update UI
- [ ] Resume button Click:
  - Call TimerController.Resume()
  - Transition StateManager to Running
  - Update UI
- [ ] Stop button Click:
  - Call TimerController.Stop()
  - Transition StateManager to Stopped
  - Update UI

#### 4.6 Implement Timer Display Updates
- [ ] Subscribe to TimerController.TimerTick event
- [ ] Update time display in MM:SS countdown format:
  ```csharp
  int minutes = secondsRemaining / 60;
  int seconds = secondsRemaining % 60;
  timeLabel.Text = $"{minutes:D2}:{seconds:D2}";
  ```
- [ ] Update progress bar value:
  ```csharp
  progressBar.Value = progressPercent;
  ```

#### 4.7 UI Polish
- [ ] Apply consistent colors (system accent or #4CAF50 for progress)
- [ ] Ensure proper control alignment and spacing
- [ ] Set appropriate tab order for controls
- [ ] Test UI responsiveness (updates within 100ms)

### Deliverables
- ✓ Complete, functional user interface
- ✓ All state transitions working correctly
- ✓ Accurate time display and progress bar

### Testing
- Test all button clicks in each state
- Verify UI updates correctly on state changes
- Test time display formatting (leading zeros, countdown)
- Verify progress bar fills from 0% to 100%
- Test UI with rapid state changes
- Verify window cannot be resized or maximized

---

## Phase 5: Notifications and Resources

**Duration**: 2-3 days  
**Goal**: Implement completion notification and application resources

### Tasks

#### 5.1 Create/Acquire Application Resources
- [ ] Create or acquire completion sound file:
  - Format: WAV
  - Duration: 2-5 seconds
  - Type: Pleasant, non-jarring (bell or chime)
  - Save as `Resources/completion.wav`
- [ ] Create or acquire application icon:
  - Format: .ico
  - Sizes: 16×16, 32×32, 48×48, 256×256
  - Design: Candle flame or timer imagery
  - Colors: Warm colors (orange, yellow)
  - Save as `Resources/icon.ico`

#### 5.2 Embed Resources in Project
- [ ] Add `completion.wav` to project as embedded resource
- [ ] Add `icon.ico` to project as embedded resource
- [ ] Set icon.ico as application icon in project properties
- [ ] Verify resources are embedded in .exe

#### 5.3 Implement NotificationService
- [ ] Add `System.Media.SoundPlayer` instance
- [ ] Implement `PlayCompletionSound()` method:
  - Load embedded sound resource
  - Play sound once
  - Handle exceptions gracefully (silent failure)
- [ ] Add error handling for missing sound file

#### 5.4 Wire Completion Notification
- [ ] Subscribe to TimerController.TimerCompleted event
- [ ] In completion handler:
  - Call NotificationService.PlayCompletionSound()
  - Transition to STOPPED state
  - Update UI to initial state
- [ ] Test notification triggers at timer completion

### Deliverables
- ✓ Application icon visible in window and taskbar
- ✓ Completion sound plays when timer finishes
- ✓ All resources embedded in executable

### Testing
- Run full timer cycle and verify sound plays
- Test sound playback with speakers on/off
- Verify icon appears correctly in:
  - Window title bar
  - Taskbar
  - Alt+Tab switcher
- Test with missing sound resource (graceful degradation)

---

## Phase 6: Build, Deployment, and Release

**Duration**: 2-3 days  
**Goal**: Finalize build configuration, create release pipeline, and deploy

### Tasks

#### 6.1 Configure Release Build
- [ ] Set Release configuration in project
- [ ] Enable code optimization
- [ ] Remove debug symbols
- [ ] Configure for single-file executable (if .NET 6+)
- [ ] Set assembly version matching semantic versioning
- [ ] Test Release build locally

#### 6.2 Create Release Pipeline
- [ ] Create `.github/workflows/release.yml`
- [ ] Configure trigger for version tags (`v*.*.*`)
- [ ] Implement release workflow:
  - Build in Release configuration
  - Create GitHub release from tag
  - Attach WorkingCandle.exe as release asset
  - Auto-generate release notes
- [ ] Set up semantic versioning
- [ ] Configure assembly and file version from git tag

#### 6.3 Testing and Quality Assurance
- [ ] Perform full manual testing:
  - Test on Windows 10
  - Test on Windows 11
  - Test all state transitions
  - Run full 1-hour timer
  - Test pause/resume multiple times
  - Verify sound playback
  - Verify icon display
- [ ] Performance testing:
  - Verify memory usage < 50 MB
  - Verify CPU usage < 1% idle, < 5% active
  - Verify startup time < 1 second
- [ ] Create test checklist document

#### 6.4 Documentation
- [ ] Update README.md with:
  - Application description
  - Usage instructions
  - System requirements
  - Build instructions
  - Download links
- [ ] Verify all specification acceptance criteria are met
- [ ] Document known issues or limitations

#### 6.5 First Release
- [ ] Create version tag `v1.0.0`
- [ ] Trigger release pipeline
- [ ] Verify executable is attached to release
- [ ] Download and test release build
- [ ] Update release notes if needed

### Deliverables
- ✓ Production-ready executable
- ✓ Automated release pipeline
- ✓ Complete documentation
- ✓ Published v1.0.0 release

### Testing
- Test downloaded executable on clean Windows machine
- Verify no .NET runtime errors
- Test all functionality in released build
- Verify version information in executable properties

---

## Optional Enhancements (Post v1.0)

These features are out of scope for v1.0 but may be considered for future releases:

### Optional Testing
- [ ] Create unit tests for TimerController
- [ ] Create unit tests for StateManager
- [ ] Create mock-based UI tests
- [ ] Add automated tests to CI pipeline

### Optional Features (Future Versions)
- [ ] Keyboard shortcuts (Space for Start/Pause)
- [ ] System tray icon support
- [ ] Windows toast notifications
- [ ] Taskbar icon flash on completion
- [ ] "Always on Top" option
- [ ] Dark mode support

---

## Risk Mitigation

### Technical Risks

| Risk | Impact | Mitigation Strategy |
|------|--------|---------------------|
| Timer accuracy drift | High | Use DateTime calculations instead of increment counters |
| System sleep affecting timer | Medium | Handle system wake events, recalculate elapsed time |
| Sound playback failure | Low | Implement graceful degradation with try-catch |
| Resource missing in build | Medium | Verify embedded resources in build pipeline |
| Windows Forms UI freezing | Medium | Use Timer for async updates, no blocking operations |

### Process Risks

| Risk | Impact | Mitigation Strategy |
|------|--------|---------------------|
| Scope creep | Medium | Strict adherence to "no feature bloat" philosophy |
| Over-engineering | Medium | Keep implementation simple, avoid unnecessary abstractions |
| CI/CD pipeline failures | Low | Test pipeline early, use stable GitHub Actions |

---

## Success Metrics

### Functional Metrics
- All 15 acceptance criteria from specification are met
- Timer accuracy: ±1 second over 1-hour period
- UI responsiveness: < 100ms for all interactions
- Zero critical bugs in v1.0 release

### Non-Functional Metrics
- Build time: < 5 minutes in CI pipeline
- Executable size: < 5 MB
- Memory footprint: < 50 MB
- Zero external dependencies (beyond .NET runtime)

---

## Timeline Summary

| Phase | Duration | Cumulative Days |
|-------|----------|-----------------|
| Phase 1: Project Setup | 1-2 days | 1-2 |
| Phase 2: Core Structure | 2-3 days | 3-5 |
| Phase 3: Timer Logic | 3-4 days | 6-9 |
| Phase 4: User Interface | 3-5 days | 9-14 |
| Phase 5: Notifications | 2-3 days | 11-17 |
| Phase 6: Build & Release | 2-3 days | 13-20 |

**Estimated Total**: 13-20 working days (approximately 3-4 weeks)

---

## Appendix A: Development Checklist

Use this checklist to track implementation progress:

### Phase 1: Project Setup
- [ ] Visual Studio solution created
- [ ] Project structure established
- [ ] CI pipeline configured
- [ ] Version control set up

### Phase 2: Core Structure
- [ ] MainForm created
- [ ] TimerController created
- [ ] StateManager created
- [ ] NotificationService created
- [ ] Components wired together

### Phase 3: Timer Logic
- [ ] Timer start/stop implemented
- [ ] Pause/resume implemented
- [ ] Time calculations accurate
- [ ] Edge cases handled
- [ ] Events implemented

### Phase 4: User Interface
- [ ] All buttons created
- [ ] Progress bar implemented
- [ ] Time display implemented
- [ ] State-based UI updates working
- [ ] UI polish completed

### Phase 5: Notifications
- [ ] Sound resource acquired
- [ ] Icon resource acquired
- [ ] Resources embedded
- [ ] Sound playback implemented
- [ ] Icon displayed

### Phase 6: Release
- [ ] Release build configured
- [ ] Release pipeline created
- [ ] Full QA testing completed
- [ ] Documentation updated
- [ ] v1.0.0 released

---

## Appendix B: Testing Checklist

### Functional Testing
- [ ] Application starts in STOPPED state
- [ ] Start button initiates timer
- [ ] Timer counts down from 60:00 to 00:00
- [ ] Progress bar fills from 0% to 100%
- [ ] Pause button pauses timer
- [ ] Resume button continues timer
- [ ] Stop button resets timer
- [ ] Sound plays on completion
- [ ] Application resets to STOPPED after completion
- [ ] Multiple pause/resume cycles work correctly

### UI Testing
- [ ] Window size is 400×300 pixels
- [ ] Window cannot be resized
- [ ] Window cannot be maximized
- [ ] Window can be minimized
- [ ] Window title is "Working Candle"
- [ ] Icon appears in title bar
- [ ] Icon appears in taskbar
- [ ] All buttons are properly sized and positioned
- [ ] Time display is readable and accurate
- [ ] Progress bar is smooth and accurate

### Platform Testing
- [ ] Works on Windows 10 (latest update)
- [ ] Works on Windows 11 (latest update)
- [ ] .NET runtime is available or error is clear
- [ ] No installation required

### Performance Testing
- [ ] Memory usage < 50 MB
- [ ] CPU usage < 1% when idle
- [ ] CPU usage < 5% when running
- [ ] Startup time < 1 second
- [ ] UI responds within 100ms

### Edge Case Testing
- [ ] System sleep during timer
- [ ] System wake during timer
- [ ] System time change during timer
- [ ] Missing sound resource
- [ ] Rapid button clicks
- [ ] Close window during timer

---

## Appendix C: Command Reference

### Local Development
```bash
# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build --configuration Debug

# Build in Release mode
dotnet build --configuration Release

# Run application
dotnet run --project WorkingCandle

# Clean build artifacts
dotnet clean
```

### Version Control
```bash
# Create version tag
git tag -a v1.0.0 -m "Release version 1.0.0"

# Push tag to trigger release
git push origin v1.0.0

# List all tags
git tag -l
```

### CI/CD
- CI builds automatically on push to `master`
- Release builds automatically on push of version tags
- Artifacts available in GitHub Actions
- Releases published to GitHub Releases page

---

**End of Implementation Plan**
