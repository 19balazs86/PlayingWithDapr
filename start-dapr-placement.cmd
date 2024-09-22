@echo off

REM It is not necessary to go to this folder. Dapr should register itself in the Path env-variable
cd /d %USERPROFILE%\.dapr\bin

REM Dapr scheduler uses the default health check port 8080 and metrics port 9090
REM You must change them if you want to run the scheduler and placement services together

placement.exe --port 6050 --healthz-port 8088 --metrics-port 9099