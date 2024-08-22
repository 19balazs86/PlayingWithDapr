@echo off

REM https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite

cd /d "c:\program files\microsoft visual studio\2022\community\common7\ide\extensions\microsoft\Azure Storage Emulator"

REM --> In-memory persistence
azurite.exe --inMemoryPersistence --skipApiVersionCheck

REM This is the command Visual Studio runs automatically as a service dependency
REM azurite.exe --location "%LOCALAPPDATA%\.vstools\azurite" --debug "%LOCALAPPDATA%\.vstools\azurite\debug.log" --skipApiVersionCheck