# Working Candle v1.0.0 - Test Checklist

**Test Date**: TBD  
**Tester**: TBD  
**Version**: 1.0.0  
**Build**: Release

---

## Pre-Test Setup

- [ ] Download WorkingCandle.exe from release
- [ ] Verify file size is approximately 365 KB
- [ ] Check file properties show version 1.0.0
- [ ] Test on clean Windows 10 machine
- [ ] Test on clean Windows 11 machine
- [ ] Verify .NET 8.0 Runtime (Windows Desktop) is installed

---

## Functional Testing

### Basic Operation
- [ ] Application starts in STOPPED state
- [ ] Window title displays "Working Candle"
- [ ] Application icon appears in title bar
- [ ] Application icon appears in taskbar
- [ ] Window size is 400×300 pixels
- [ ] Window cannot be resized
- [ ] Window cannot be maximized
- [ ] Window can be minimized
- [ ] Start button displays "Start 1h Timer"
- [ ] Start button is centered and visible

### Timer Start
- [ ] Click Start button initiates timer
- [ ] Timer displays "60:00" initially
- [ ] Progress bar appears
- [ ] Progress bar starts at 0%
- [ ] Time display appears below progress bar
- [ ] Pause button and Stop button appear
- [ ] Start button is hidden

### Timer Running
- [ ] Timer counts down from 60:00 to 00:00
- [ ] Time display updates every second
- [ ] Time display shows leading zeros (e.g., "09:05")
- [ ] Progress bar fills from 0% to 100%
- [ ] Progress bar updates smoothly
- [ ] Pause button remains visible
- [ ] UI remains responsive during countdown

### Pause Functionality
- [ ] Click Pause button pauses timer
- [ ] Timer stops counting down
- [ ] Progress bar stops moving
- [ ] Resume button appears
- [ ] Stop button appears
- [ ] Pause button is hidden

### Resume Functionality
- [ ] Click Resume button continues timer
- [ ] Timer continues from paused time
- [ ] Progress bar continues from paused position
- [ ] Pause button reappears
- [ ] Resume and Stop buttons are hidden
- [ ] Multiple pause/resume cycles work correctly

### Stop Functionality
- [ ] Click Stop button (when running or paused) resets timer
- [ ] Application returns to initial STOPPED state
- [ ] Start button reappears
- [ ] Progress bar, time display, Resume, Pause, and Stop buttons are hidden
- [ ] Can start new timer after stopping

### Timer Completion
- [ ] Timer reaches 00:00
- [ ] Completion sound plays
- [ ] Sound is pleasant and non-jarring
- [ ] Application automatically returns to STOPPED state
- [ ] All UI elements reset correctly
- [ ] Can start new timer after completion

---

## UI Testing

### Layout and Appearance
- [ ] All buttons are properly sized and positioned
- [ ] Time display is large and readable (36pt)
- [ ] Progress bar is visible and appropriately sized
- [ ] All text is clear and properly aligned
- [ ] Controls are centered horizontally
- [ ] Spacing between elements is appropriate
- [ ] No UI elements overlap
- [ ] No cut-off text or controls

### Interaction
- [ ] All buttons respond to clicks
- [ ] Button clicks provide immediate feedback
- [ ] No lag or delay in UI updates
- [ ] Mouse cursor changes appropriately over buttons
- [ ] Tab order is logical (if using keyboard)
- [ ] Window can be moved on screen
- [ ] Window can be closed at any time

### Visual Feedback
- [ ] Progress bar fills smoothly without jumps
- [ ] Time display updates without flickering
- [ ] State transitions are smooth
- [ ] No visual glitches during operation

---

## Edge Case Testing

### Rapid Interactions
- [ ] Rapid Start clicks don't cause issues
- [ ] Rapid Pause clicks work correctly
- [ ] Quick Pause-Resume-Pause sequence works
- [ ] Clicking Stop immediately after Pause works

### System Events
- [ ] Timer continues accurately after system sleep/wake
- [ ] Timer handles system time changes gracefully
- [ ] Application works correctly after screen lock/unlock
- [ ] Multiple monitor setup doesn't cause issues
- [ ] Application works with different DPI settings

### Resource Management
- [ ] Sound plays correctly with audio enabled
- [ ] Application handles missing sound gracefully (if sound disabled)
- [ ] Application icon displays correctly
- [ ] No memory leaks during extended use
- [ ] Application closes cleanly (no hanging processes)

### Window Management
- [ ] Alt+Tab correctly switches to/from application
- [ ] Minimizing and restoring works correctly
- [ ] Timer continues running when minimized
- [ ] Closing window stops timer immediately
- [ ] Application doesn't prevent Windows shutdown

---

## Performance Testing

### Resource Usage
- [ ] Memory usage < 50 MB (check Task Manager)
- [ ] CPU usage < 1% when idle
- [ ] CPU usage < 5% when running
- [ ] Startup time < 1 second
- [ ] No disk I/O during operation
- [ ] No network activity

### Responsiveness
- [ ] Application starts within 1 second
- [ ] Button clicks respond within 100ms
- [ ] UI updates occur within 100ms
- [ ] No UI freezing or stuttering
- [ ] No delays during state transitions

### Accuracy
- [ ] Timer is accurate to ±1 second over 1 hour
- [ ] Progress bar accurately reflects time remaining
- [ ] Pause/resume doesn't affect timer accuracy
- [ ] Multiple pause/resume cycles remain accurate

---

## Platform Testing

### Windows 10
- [ ] Application installs (or runs) without errors
- [ ] All functional tests pass
- [ ] Performance metrics are met
- [ ] UI renders correctly
- [ ] Sound plays correctly
- [ ] Icon displays correctly

### Windows 11
- [ ] Application installs (or runs) without errors
- [ ] All functional tests pass
- [ ] Performance metrics are met
- [ ] UI renders correctly
- [ ] Sound plays correctly
- [ ] Icon displays correctly
- [ ] Follows Windows 11 design guidelines

### .NET Runtime
- [ ] Application runs with .NET 8.0 Runtime installed
- [ ] Clear error message if .NET Runtime missing
- [ ] Error message includes download link or instructions

---

## Security Testing

### Application Security
- [ ] No unnecessary permissions requested
- [ ] No network connections made
- [ ] No file system access beyond resources
- [ ] Application runs without admin privileges
- [ ] Code signing present (if applicable)
- [ ] No anti-virus false positives

---

## Installation Testing

### First Run
- [ ] No installation required
- [ ] Runs from any directory
- [ ] No registry modifications
- [ ] No AppData directory created
- [ ] Application is truly portable

### Uninstallation
- [ ] Simply delete executable to uninstall
- [ ] No leftover files or registry entries
- [ ] No user data to back up

---

## Acceptance Criteria Verification

From Technical Specification:

1. [ ] Application provides a 1-hour focus timer
2. [ ] User can start, pause, resume, and stop the timer
3. [ ] UI displays countdown and progress
4. [ ] Application plays sound on completion
5. [ ] Executable runs on Windows 10+ without installation
6. [ ] Application is a single .exe file with no dependencies
7. [ ] UI is clean and distraction-free
8. [ ] Application uses < 50 MB memory
9. [ ] Timer accuracy is ±1 second over 1 hour
10. [ ] Application startup time is < 1 second
11. [ ] UI updates occur within 100ms of user action
12. [ ] Application follows Windows UI conventions
13. [ ] No configuration files or settings
14. [ ] Application can be moved to any directory
15. [ ] No internet connection required

---

## Known Issues

Document any issues found during testing:

| Issue # | Description | Severity | Status |
|---------|-------------|----------|--------|
| | | | |

---

## Test Summary

**Overall Result**: [ ] PASS / [ ] FAIL  
**Total Tests**: ___  
**Tests Passed**: ___  
**Tests Failed**: ___  
**Critical Issues**: ___  
**Minor Issues**: ___

**Recommendation**:
- [ ] Approve for release
- [ ] Requires fixes before release
- [ ] Requires further testing

**Tester Signature**: _______________  
**Date**: _______________

---

## Notes

Additional observations or comments:

