@echo off

dotnet publish -c Release

if %ERRORLEVEL% neq 0 pause