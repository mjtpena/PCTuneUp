# PCTuneUp.Tests

Unit tests for the PC TuneUp application using xUnit.

## Test Coverage

This project contains comprehensive unit tests for the core cleanup utilities:

### CleanupUtilitiesTests (22 tests)

#### FormatBytes Tests (9 tests)
- Zero bytes formatting
- Small bytes (< 1KB)
- Kilobytes formatting (single and multiple)
- Megabytes formatting (single and multiple)
- Gigabytes formatting (single, multiple, and fractional)

#### GetDirectorySize Tests (5 tests)
- Non-existent directory handling
- Empty directory
- Directory with files
- Directory with subdirectories

#### DeleteFilesInDirectoryWithStats Tests (5 tests)
- Empty directory deletion
- Directory with files deletion
- Directory with subdirectories deletion
- Non-existent directory handling

#### ScanBrowserCache Tests (4 tests)
- Unknown browser handling
- Chrome cache scanning
- Edge cache scanning
- Firefox cache scanning

#### CleanBrowserCache Tests (1 test)
- Unknown browser handling

## Running Tests

```powershell
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger:"console;verbosity=detailed"

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Requirements

- .NET 10.0 SDK
- xUnit test framework
- The tests can run on any platform (Windows, Linux, macOS)
