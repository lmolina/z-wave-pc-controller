# Z-Wave CLI

Command-line interface for the Z-Wave PC Controller, providing scriptable access to Z-Wave network management.

## Status

🚧 **Currently in Development** - Session 0: Environment Setup Complete

This CLI is being built using Test-Driven Development (TDD) following the migration plan in `../CLI_MIGRATION_PLAN.md`.

### Completed
- ✅ .NET 8 project structure created
- ✅ Test framework configured (xUnit, FluentAssertions, Moq)
- ✅ Dependency injection setup
- ✅ Solution file updated

### Next Steps
1. Install .NET 8 SDK
2. Verify project builds: `dotnet build`
3. Run tests: `dotnet test`
4. Begin **Iteration 0** (MVP): Connection and basic info commands

## Prerequisites

- .NET 8 SDK or later
- USB Z-Wave controller or network access to Z-Wave gateway
- Linux (Ubuntu 22.04+ recommended) or Windows

## Building

```bash
# Restore dependencies
dotnet restore

# Build the CLI
dotnet build ZWaveCLI/ZWaveCLI.csproj

# Run tests
dotnet test ZWaveCLI.Tests/ZWaveCLI.Tests.csproj

# Run the CLI
dotnet run --project ZWaveCLI/ZWaveCLI.csproj -- --help
```

## Linux Setup

### Serial Port Permissions

Add your user to the `dialout` group to access serial ports:

```bash
sudo usermod -aG dialout $USER
# Log out and log back in for changes to take effect
```

### Optional: udev Rules

Create `/etc/udev/rules.d/99-zwave-controller.rules`:

```
# Silicon Labs CP210x USB to UART Bridge
SUBSYSTEM=="tty", ATTRS{idVendor}=="10c4", ATTRS{idProduct}=="ea60", MODE="0666", GROUP="dialout"

# FTDI USB Serial
SUBSYSTEM=="tty", ATTRS{idVendor}=="0403", ATTRS{idProduct}=="6001", MODE="0666", GROUP="dialout"
```

Reload udev rules:
```bash
sudo udevadm control --reload-rules
sudo udevadm trigger
```

## Planned Commands (from CLI_MIGRATION_PLAN.md)

### Iteration 0: MVP (Target: First Working Version)
- `list-ports` - List available serial ports
- `connect <port>` - Connect to Z-Wave controller
- `info` - Display controller information

### Future Iterations
- Node management (list, info, request-node-info)
- Command sending (send, basic-set, basic-get, watch)
- Network management (add-node, remove-node, reset, learn-mode)
- Configuration (config-get, config-set, config-list)
- Security (secure-inclusion, dsk-input, list-keys, set-key)
- Advanced features (provisioning, firmware-update, topology, diagnostics)

## Project Structure

```
ZWaveCLI/
├── Program.cs              # Entry point with dependency injection
├── Commands/               # Command implementations (to be created)
├── Services/               # CLI-specific services (to be created)
├── Models/                 # CLI data models (to be created)
└── ZWaveCLI.csproj        # Project file

ZWaveCLI.Tests/
├── Commands/               # Command tests
├── Services/               # Service tests
├── Integration/            # Integration tests
├── Helpers/                # Test utilities (TestDataBuilder, etc.)
└── ZWaveCLI.Tests.csproj  # Test project file
```

## Development Approach

This CLI follows a **lean, iterative TDD approach** with 25-40 minute focused sessions:

1. **Red**: Write failing tests first
2. **Green**: Implement minimal code to pass tests
3. **Refactor**: Improve code quality while keeping tests green

See `../CLI_MIGRATION_PLAN.md` for detailed iteration plans.

## Architecture

The CLI is a thin wrapper around the `ZWaveController` backend library:

```
┌─────────────────────┐
│   CLI Commands      │ ← Thin wrapper (this project)
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   ZWaveController   │ ← Backend logic (../ZWaveController)
│   - Services        │
│   - Models          │
│   - Sessions        │
└─────────────────────┘
```

This design allows:
- Easy testing of CLI commands
- Reuse of backend logic for future GUI
- Clean separation of concerns

## Contributing

When implementing new features:

1. **Write tests first** in `ZWaveCLI.Tests/`
2. Run tests to verify they fail (Red)
3. Implement minimal code in `ZWaveCLI/`
4. Run tests to verify they pass (Green)
5. Refactor if needed, keeping tests green
6. Commit with conventional commit messages:
   - `test: add tests for [feature]`
   - `feat: implement [feature]`
   - `refactor: improve [aspect]`

## License

See LICENSE in the root directory.
