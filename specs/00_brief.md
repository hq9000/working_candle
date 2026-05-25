# overview

this is a windows app for focusing on work.

requirements:
- no feature bloat, extremely simple.
- native windows app


# states:

stopped, running, paused

# interface

## stopped state

- start 1h timer button


## running state

progress bar
pause button

## paused state

progress bar (not moving)
resume button
stop button

# Notification about end of an hour

plays a sound

# deployment

.exe file with appropriate icon

# Tech

use Windows Forms, C# for backend

# CI

the exe file should be build in CI pipeline and attached to the release.

