@echo off
REM COM YAML Validator - Community Edition
REM Double-click or drag-and-drop YAML file to validate

REM Set UTF-8 encoding for Japanese output
chcp 65001 >nul

REM Check if file argument provided
if "%~1"=="" (
    echo.
    echo Usage: Drag and drop a YAML file onto this batch file
    echo   or: validate.bat path\to\file.yaml
    echo.
    pause
    exit /b 1
)

REM Store the file path
set YAML_FILE=%~1

REM Check if file exists
if not exist "%YAML_FILE%" (
    echo.
    echo Error: File not found: %YAML_FILE%
    echo.
    pause
    exit /b 1
)

REM Run the validator
echo.
echo ========================================
echo COM YAML Validator
echo ========================================
echo.
"%~dp0com-validator.exe" "%YAML_FILE%"

REM Store exit code
set VALIDATOR_EXIT=%ERRORLEVEL%

echo.
echo ========================================
if %VALIDATOR_EXIT%==0 (
    echo Validation completed successfully
) else (
    echo Validation failed
)
echo ========================================
echo.

REM Pause to show results before closing
pause
exit /b %VALIDATOR_EXIT%
