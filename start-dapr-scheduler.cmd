@echo off

REM From Dapr version 1.14, there is a scheduler feature that gets initialized with the command: 'dapr init --slim'

REM When you run your application from Visual Studio, it misses the scheduler service because it is not started automatically in self-hosted mode
REM When you run your application with 'dapr run -f .', it does not encounter the same issue

REM It is not necessary to go to this folder. Dapr should register itself in the Path env-variable
REM However, the scheduler creates a data folder where you run it, so it is practical to run it from the dapr\bin folder
cd /d %USERPROFILE%\.dapr\bin

REM 6060 is the port that Visual Studio runner misses
scheduler.exe --port 6060