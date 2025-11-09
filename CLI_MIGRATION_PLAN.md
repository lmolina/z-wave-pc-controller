# Z-Wave PC Controller: CLI Migration Plan (TDD, Lean, Iterative)

**Version:** 1.1
**Date:** 2025-11-08
**Approach:** Test-Driven Development, Lean Iterations, 25-40 min sessions
**Target:** Linux-compatible CLI + GUI applications sharing a common backend library

---

## Table of Contents

1. [End Goal: Dual Architecture](#end-goal-dual-architecture)
2. [Strategy Overview](#strategy-overview)
3. [MVP Definition](#mvp-definition)
4. [Project Setup](#project-setup)
5. [Iteration Plan](#iteration-plan)
6. [Session Structure](#session-structure)
7. [Testing Strategy](#testing-strategy)
8. [Implementation Roadmap](#implementation-roadmap)
9. [Session Breakdown by Iteration](#session-breakdown-by-iteration)
10. [GUI Development Strategy](#gui-development-strategy)

---

## End Goal: Dual Architecture

### Vision: CLI + GUI Sharing Common Backend

The migration delivers **two applications** on a **single shared library**:

```
┌─────────────────────────────────────────────────────────────┐
│                    USER INTERFACES                          │
│  ┌──────────────────────┐      ┌──────────────────────┐    │
│  │   GUI Application    │      │   CLI Application    │    │
│  │   (Avalonia UI)      │      │   (System.CommandLine│    │
│  │                      │      │                      │    │
│  │ - Rich visuals       │      │ - Scriptable         │    │
│  │ - User-friendly      │      │ - Automation         │    │
│  │ - Topology maps      │      │ - CI/CD integration  │    │
│  │ - Wizards            │      │ - Quick operations   │    │
│  │ - Real-time updates  │      │ - Headless servers   │    │
│  └──────────┬───────────┘      └──────────┬───────────┘    │
└─────────────┼──────────────────────────────┼────────────────┘
              │                              │
              └──────────────┬───────────────┘
                             │
              ┌──────────────▼───────────────┐
              │  Shared Presentation Layer   │
              │  (ViewModels, Commands)      │
              │  - MVVM ViewModels           │
              │  - Command pattern           │
              │  - INotifyPropertyChanged    │
              │  - Data binding sources      │
              └──────────────┬───────────────┘
                             │
              ┌──────────────▼───────────────┐
              │   ZWaveController Library    │
              │   (Platform-agnostic .NET 8) │
              │                              │
              │  Services Layer:             │
              │  ├─ ConnectionService        │
              │  ├─ NodeManagementService    │
              │  ├─ ConfigurationService     │
              │  ├─ SecurityService          │
              │  └─ TopologyService          │
              │                              │
              │  Session Management:         │
              │  ├─ BasicControllerSession   │
              │  ├─ ZipControllerSession     │
              │  └─ SessionContainer         │
              │                              │
              │  Protocol Layer:             │
              │  ├─ Z-Wave protocol logic    │
              │  ├─ Command classes          │
              │  ├─ Security (S0/S2)         │
              │  └─ Transport (Serial/TCP)   │
              └──────────────┬───────────────┘
                             │
              ┌──────────────▼───────────────┐
              │   Platform Abstractions      │
              │  ├─ ISerialPortProvider      │
              │  ├─ IWebCamCapture           │
              │  └─ IDeviceMonitor           │
              └──────────────┬───────────────┘
                             │
                    ┌────────┴────────┐
                    │                 │
         ┌──────────▼────────┐  ┌────▼──────────────┐
         │  Linux Platform   │  │ Windows Platform  │
         │  - V4L2 webcam    │  │ - WMI device info │
         │  - udev monitor   │  │ - Native DLLs     │
         │  - sysfs parsing  │  │ - WPF (legacy)    │
         └───────────────────┘  └───────────────────┘
```

### Benefits of This Architecture

| Aspect | CLI Benefits | GUI Benefits | Shared Benefits |
|--------|--------------|--------------|-----------------|
| **Development** | Fast feedback, easy testing | Proven backend functionality | Single codebase to maintain |
| **Use Cases** | Automation, scripting, CI/CD | User-friendly operations | Consistent behavior |
| **Testing** | Comprehensive test coverage | GUI tests focus on UI only | Backend tested independently |
| **Deployment** | Lightweight, headless servers | Desktop workstations | Both from same build |
| **Learning Curve** | Power users, developers | General users | Documentation reuse |
| **Debugging** | Easy to isolate issues | Visual feedback | Shared logging/telemetry |

### Development Phases

```
Phase 1: Backend + CLI (THIS PLAN)     Phase 2: GUI (FUTURE)
├─ Iterations 0-7                      ├─ Reuse ViewModels
├─ ZWaveController library             ├─ Avalonia UI views
├─ CLI application                     ├─ Visual components
├─ 125-165 tests                       ├─ GUI-specific tests
├─ .deb package                        └─ Unified installer
└─ 24 hours (~6 weeks part-time)       └─ 16-20 hours additional

Timeline: 10-12 weeks total for both CLI and GUI
```

### Why Build CLI First?

1. **Validate Backend on Linux**
   - Prove ZWaveController library works without WPF dependencies
   - Identify platform-specific issues early
   - Test serial communication, networking

2. **Immediate Value**
   - Usable tool from Iteration 0 (2.5 hours)
   - Automation capabilities
   - Scriptable workflows

3. **Establish Patterns**
   - Command execution model
   - Service layer design
   - Error handling
   - Logging strategy

4. **Comprehensive Testing**
   - Backend fully tested before GUI
   - GUI tests only need to cover UI logic
   - Regression suite for both applications

5. **Parallel Development**
   - CLI can evolve independently
   - GUI can be developed by different team/person
   - Shared library ensures consistency

### Shared Components

#### What Gets Shared (90%+)

```csharp
// ZWaveController library (fully shared)
namespace ZWaveController
{
    public interface IConnectionService { }
    public interface INodeManagementService { }
    public interface IConfigurationService { }
    public interface ISecurityService { }

    public class BasicControllerSession : IControllerSession { }
    public class ZipControllerSession : IControllerSession { }
}

// ViewModels layer (shared between CLI and GUI)
namespace ZWaveController.ViewModels
{
    public class ConnectionViewModel : INotifyPropertyChanged
    {
        private readonly IConnectionService _connectionService;

        public ObservableCollection<PortInfo> AvailablePorts { get; }
        public ICommand ConnectCommand { get; }

        // Used by CLI: Direct method calls
        public async Task<bool> ConnectAsync(string port) { }

        // Used by GUI: Data binding
        public string SelectedPort { get; set; }
    }

    public class NodeManagementViewModel { }
    public class ConfigurationViewModel { }
}
```

#### What Differs (<10%)

```csharp
// CLI-specific (thin wrapper)
namespace ZWaveCLI.Commands
{
    public class ConnectCommand
    {
        private readonly ConnectionViewModel _viewModel;

        public async Task<int> ExecuteAsync(string port)
        {
            var result = await _viewModel.ConnectAsync(port);
            Console.WriteLine(result ? "✓ Connected" : "✗ Failed");
            return result ? 0 : 1;
        }
    }
}

// GUI-specific (XAML + code-behind)
namespace ZWaveGUI.Views
{
    public partial class ConnectionView : UserControl
    {
        public ConnectionView()
        {
            InitializeComponent();
            DataContext = new ConnectionViewModel(/* DI */);
        }
    }
}
```

```xml
<!-- ConnectionView.axaml (Avalonia) -->
<UserControl>
  <StackPanel>
    <ComboBox ItemsSource="{Binding AvailablePorts}"
              SelectedItem="{Binding SelectedPort}"/>
    <Button Command="{Binding ConnectCommand}"
            Content="Connect"/>
  </StackPanel>
</UserControl>
```

### Migration Roadmap

```
┌─────────────────────────────────────────────────────────────┐
│ CURRENT STATE (Windows only)                                │
│ ├─ ZWaveControllerUI (WPF)                                  │
│ └─ ZWaveController (.NET Framework 4.8)                     │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 1: Backend + CLI (Iterations 0-7, ~24 hours)         │
│ ├─ ZWaveController (.NET 8, Linux-compatible)              │
│ ├─ ZWaveController.ViewModels (Shared presentation layer)  │
│ └─ ZWaveCLI (Command-line interface)                       │
│    ✓ Fully functional on Linux                             │
│    ✓ Feature parity with GUI core operations               │
│    ✓ 125-165 tests, 80%+ coverage                          │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│ PHASE 2: GUI Development (~16-20 hours)                    │
│ ├─ ZWaveGUI (Avalonia UI)                                  │
│    ├─ Reuse ViewModels from Phase 1                        │
│    ├─ XAML views (port from WPF or create new)             │
│    ├─ Custom controls (topology map, etc.)                 │
│    └─ GUI-specific tests                                   │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│ END STATE (Linux + Windows)                                │
│ ├─ ZWaveController (shared backend)                        │
│ ├─ ZWaveController.ViewModels (shared presentation)        │
│ ├─ ZWaveCLI (automation, scripting)                        │
│ └─ ZWaveGUI (user-friendly operations)                     │
│    ✓ Both applications maintained in parallel              │
│    ✓ Consistent behavior across CLI and GUI                │
│    ✓ Users choose interface based on preference            │
└─────────────────────────────────────────────────────────────┘
```

### Success Criteria

**Phase 1 Complete (CLI):**
- ✅ CLI runs on Linux and Windows
- ✅ All core features implemented
- ✅ 125-165 passing tests
- ✅ Comprehensive documentation
- ✅ .deb and NuGet packages

**Phase 2 Complete (GUI):**
- ✅ GUI runs on Linux and Windows
- ✅ Feature parity with CLI
- ✅ Reuses 90%+ of backend code
- ✅ Rich visualizations (topology, graphs)
- ✅ Unified installer (CLI + GUI)

**Both Applications:**
- ✅ Share ZWaveController library
- ✅ Consistent command execution
- ✅ Same configuration format
- ✅ Interoperable (CLI can script GUI workflows)

---

## Strategy Overview

### Core Principles

1. **CLI First, GUI Later**
   - Build command-line interface to validate backend functionality
   - Avoid UI complexity initially
   - GUI becomes a frontend to proven backend

2. **Test-Driven Development (TDD)**
   - Write tests before implementation (Red-Green-Refactor)
   - Automated regression testing
   - Living documentation through tests

3. **Lean & Iterative**
   - Small, deliverable increments
   - Each iteration adds working functionality
   - Fail fast, learn fast

4. **Time-boxed Sessions**
   - 25-40 minute focused sessions (Pomodoro technique)
   - Clear goals per session
   - Regular breaks to maintain focus

### Why CLI First?

| Benefit | Description |
|---------|-------------|
| **Reduced Complexity** | No UI framework dependencies (WPF/Avalonia) |
| **Faster Feedback** | Test backend logic without UI overhead |
| **Cross-Platform Ready** | CLI works on any .NET platform |
| **Better Testing** | Easier to write automated tests |
| **Scriptable** | Automation and batch operations |
| **Debugging** | Simpler to isolate issues |

### Development Flow

```
┌─────────────┐
│  Iteration  │
│   Planning  │
└──────┬──────┘
       │
       ▼
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│ Write Tests │────▶│ Implement    │────▶│   Refactor  │
│    (Red)    │     │  Code (Green)│     │   (Clean)   │
└─────────────┘     └──────────────┘     └──────┬──────┘
       ▲                                          │
       └──────────────────────────────────────────┘
                    (Repeat)
```

---

## MVP Definition

### Minimum Viable Product (Iteration 0)

**Goal:** Connect to a Z-Wave controller and retrieve basic information

**User Story:**
> As a Z-Wave developer, I want to connect to my USB controller and see its version information, so I can verify the connection is working.

**Acceptance Criteria:**
- ✅ CLI application runs on Linux
- ✅ Detects available serial ports
- ✅ Connects to specified Z-Wave controller
- ✅ Retrieves and displays controller version
- ✅ Graceful error handling (no controller, connection failure)
- ✅ Help text explains usage

**Command Example:**
```bash
$ zwavecli list-ports
Available ports:
  /dev/ttyUSB0 - Silicon Labs CP210x UART Bridge (VID:10C4 PID:EA60)
  /dev/ttyUSB1 - FTDI USB Serial Device

$ zwavecli connect /dev/ttyUSB0
Connecting to /dev/ttyUSB0...
Connected!
Controller: Z-Wave 7.19.1 (Static Controller)
Chip: 700 series
Home ID: 0x12345678
Node ID: 1

$ zwavecli info
Controller Information:
  Version: Z-Wave 7.19.1
  Library: Static Controller
  Chip Type: 700 series
  Home ID: 0x12345678
  Node ID: 1
  Capabilities: SUC, SIS
```

### Success Metrics

- ✅ Connection success rate: 100% (when controller present)
- ✅ Test coverage: >80%
- ✅ All tests green
- ✅ No runtime exceptions for normal usage
- ✅ Runs on Ubuntu 22.04 LTS

---

## Project Setup

### Session 0: Environment Setup (30 min)

**Goal:** Set up development environment and project structure

**Tasks:**
1. Install .NET 8 SDK on Linux
2. Create solution structure
3. Set up testing framework
4. Configure CI/CD basics

**Commands:**
```bash
# Install .NET 8 (Ubuntu)
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Create solution
dotnet new sln -n ZWaveControllerCLI

# Create CLI project
dotnet new console -n ZWaveCLI -f net8.0
dotnet sln add ZWaveCLI/ZWaveCLI.csproj

# Create test project
dotnet new xunit -n ZWaveCLI.Tests -f net8.0
dotnet sln add ZWaveCLI.Tests/ZWaveCLI.Tests.csproj
dotnet add ZWaveCLI.Tests reference ZWaveCLI

# Add existing backend library
dotnet sln add ../ZWaveController/ZWaveController_netcore.csproj
dotnet add ZWaveCLI reference ../ZWaveController/ZWaveController_netcore.csproj

# Add packages
cd ZWaveCLI
dotnet add package System.CommandLine --version 2.0.0-beta4.22272.1
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Logging.Console

cd ../ZWaveCLI.Tests
dotnet add package FluentAssertions
dotnet add package Moq
dotnet add package Verify.Xunit
```

**Project Structure:**
```
ZWaveControllerCLI/
├── ZWaveCLI/
│   ├── Program.cs
│   ├── Commands/
│   ├── Services/
│   ├── Models/
│   └── ZWaveCLI.csproj
├── ZWaveCLI.Tests/
│   ├── Commands/
│   ├── Services/
│   ├── Integration/
│   └── ZWaveCLI.Tests.csproj
└── ZWaveControllerCLI.sln
```

**Acceptance Criteria:**
- ✅ `dotnet build` succeeds
- ✅ `dotnet test` runs (even with no tests)
- ✅ Can run `dotnet run --project ZWaveCLI`

---

## Iteration Plan

### Overview

| Iteration | Goal | Duration | Key Features |
|-----------|------|----------|--------------|
| **0** | MVP - Connect & Info | 4-5 sessions (2-3 hours) | Serial port detection, connection, version info |
| **1** | Node Discovery | 3-4 sessions (2 hours) | List nodes, node info |
| **2** | Basic Commands | 4-5 sessions (2.5 hours) | Send commands, view responses |
| **3** | Network Management | 6-8 sessions (4 hours) | Add/remove nodes, reset |
| **4** | Configuration | 4-5 sessions (2.5 hours) | Get/set parameters |
| **5** | Security | 5-6 sessions (3.5 hours) | S2 inclusion, key management |
| **6** | Advanced Features | 8-10 sessions (5 hours) | Smart Start, firmware update, topology |
| **7** | Polish & Package | 3-4 sessions (2 hours) | Error handling, logging, distribution |

**Total Estimated Time:** 37-47 sessions (20-25 hours)

---

## Session Structure

### Standard Session Template (25-40 min)

```
┌─────────────────────────────────────────┐
│ Session: [Feature Name]                 │
│ Duration: 25-40 min                     │
│ Iteration: X                            │
└─────────────────────────────────────────┘

1. Plan (5 min)
   - Review acceptance criteria
   - Identify test cases
   - Sketch API/interface

2. Test (10-15 min)
   - Write failing test(s)
   - Run tests (verify RED)
   - Commit: "test: add tests for [feature]"

3. Implement (10-15 min)
   - Write minimal code to pass tests
   - Run tests (verify GREEN)
   - Commit: "feat: implement [feature]"

4. Refactor (5 min)
   - Clean up code
   - Remove duplication
   - Improve naming
   - Run tests (verify still GREEN)
   - Commit: "refactor: improve [aspect]"

5. Reflect (2 min)
   - What worked?
   - What's next?
   - Update task list
```

### Session Types

1. **Red Session** (Test-focused)
   - Write comprehensive tests
   - Document expected behavior
   - No implementation yet

2. **Green Session** (Implementation)
   - Make tests pass
   - Minimal code
   - No gold plating

3. **Refactor Session** (Quality)
   - Improve code structure
   - Extract reusable components
   - Update documentation

4. **Integration Session** (E2E)
   - Test with real hardware
   - Fix integration issues
   - Update tests based on findings

---

## Testing Strategy

### Test Pyramid

```
        ┌─────────────┐
        │   Manual    │  ← Integration tests with real hardware (10%)
        └─────────────┘
       ┌───────────────┐
       │ Integration   │  ← In-process integration tests (20%)
       └───────────────┘
      ┌─────────────────┐
      │   Unit Tests    │  ← Fast, isolated tests (70%)
      └─────────────────┘
```

### Test Categories

#### 1. Unit Tests (70%)

**Goal:** Test individual components in isolation

**Example:**
```csharp
public class SerialPortDetectorTests
{
    [Fact]
    public void GetPorts_ShouldReturnAvailablePorts()
    {
        // Arrange
        var detector = new SerialPortDetector();

        // Act
        var ports = detector.GetPorts();

        // Assert
        ports.Should().NotBeNull();
        ports.Should().AllSatisfy(p => p.Name.Should().NotBeNullOrEmpty());
    }

    [Theory]
    [InlineData("/dev/ttyUSB0", true)]
    [InlineData("/dev/ttyACM0", true)]
    [InlineData("/dev/invalid", false)]
    public void IsValidPort_ShouldValidatePortName(string portName, bool expected)
    {
        // Arrange
        var detector = new SerialPortDetector();

        // Act
        var result = detector.IsValidPort(portName);

        // Assert
        result.Should().Be(expected);
    }
}
```

#### 2. Integration Tests (20%)

**Goal:** Test component interactions (mocked external dependencies)

**Example:**
```csharp
public class ConnectionServiceTests
{
    [Fact]
    public async Task ConnectAsync_WithValidPort_ShouldSucceed()
    {
        // Arrange
        var mockController = new Mock<IControllerSession>();
        mockController.Setup(c => c.Connect(It.IsAny<IDataSource>()))
            .Returns(CommunicationStatuses.Done);

        var service = new ConnectionService(mockController.Object);
        var dataSource = new SerialPortDataSource("/dev/ttyUSB0", BaudRates.Rate_115200);

        // Act
        var result = await service.ConnectAsync(dataSource);

        // Assert
        result.Should().BeTrue();
        mockController.Verify(c => c.Connect(dataSource), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_WithInvalidPort_ShouldReturnFalse()
    {
        // Arrange
        var mockController = new Mock<IControllerSession>();
        mockController.Setup(c => c.Connect(It.IsAny<IDataSource>()))
            .Returns(CommunicationStatuses.Timeout);

        var service = new ConnectionService(mockController.Object);
        var dataSource = new SerialPortDataSource("/dev/invalid", BaudRates.Rate_115200);

        // Act
        var result = await service.ConnectAsync(dataSource);

        // Assert
        result.Should().BeFalse();
    }
}
```

#### 3. End-to-End Tests (10%)

**Goal:** Test with real hardware (semi-automated)

**Example:**
```csharp
[Collection("Hardware")]  // Runs only when hardware available
public class HardwareIntegrationTests
{
    [Fact(Skip = "Requires USB controller")]
    public async Task FullConnectionFlow_WithRealController_ShouldWork()
    {
        // Arrange
        var cli = new Program();
        var port = "/dev/ttyUSB0"; // Configure via environment variable

        // Act
        var exitCode = await cli.RunAsync(["connect", port]);

        // Assert
        exitCode.Should().Be(0);
        // Additional assertions on controller state
    }
}
```

### Test Helpers

**Builders for Test Data:**
```csharp
public class TestDataBuilder
{
    public static IDataSource CreateSerialPort(string port = "/dev/ttyUSB0")
        => new SerialPortDataSource(port, BaudRates.Rate_115200);

    public static Mock<IControllerSession> CreateMockController()
    {
        var mock = new Mock<IControllerSession>();
        mock.Setup(c => c.Controller.Version).Returns("Z-Wave 7.19.1");
        return mock;
    }
}
```

### TDD Workflow Example

```bash
# 1. Write failing test
dotnet test --filter "FullyQualifiedName~SerialPortDetectorTests"
# Result: RED (test fails)

# 2. Implement minimal code
# Edit SerialPortDetector.cs

# 3. Run tests again
dotnet test --filter "FullyQualifiedName~SerialPortDetectorTests"
# Result: GREEN (test passes)

# 4. Refactor
# Improve code quality

# 5. Run all tests
dotnet test
# Result: All GREEN
```

---

## Implementation Roadmap

### Iteration 0: MVP - Connect & Info

**Goal:** Connect to controller and display version information

**Sessions:** 4-5 (2-3 hours)

#### Session 0.1: Serial Port Detection (30 min)

**Test First:**
```csharp
[Fact]
public void GetAvailablePorts_ShouldReturnSystemPorts()
{
    var detector = new SerialPortDetector();
    var ports = detector.GetAvailablePorts();
    ports.Should().NotBeNull();
}

[Fact]
public void GetPortInfo_ShouldIncludeDescription()
{
    var detector = new SerialPortDetector();
    var port = detector.GetPortInfo("/dev/ttyUSB0");
    port.Name.Should().Be("/dev/ttyUSB0");
    port.Description.Should().NotBeNullOrEmpty();
}
```

**Implementation:**
- Create `SerialPortDetector` service
- Use `System.IO.Ports.SerialPort.GetPortNames()`
- Parse `/sys/class/tty/` for device info (Linux)
- Return `PortInfo` list

**Commit:**
```
test: add serial port detection tests
feat: implement serial port detection on Linux
refactor: extract sysfs parsing to helper
```

---

#### Session 0.2: Connection Command (35 min)

**Test First:**
```csharp
[Fact]
public async Task ConnectCommand_WithValidPort_ShouldConnect()
{
    var mockService = new Mock<IConnectionService>();
    mockService.Setup(s => s.ConnectAsync(It.IsAny<string>()))
        .ReturnsAsync(true);

    var command = new ConnectCommand(mockService.Object);
    var result = await command.ExecuteAsync("/dev/ttyUSB0");

    result.Should().Be(0);
    mockService.Verify(s => s.ConnectAsync("/dev/ttyUSB0"), Times.Once);
}

[Fact]
public async Task ConnectCommand_WithInvalidPort_ShouldReturnError()
{
    var mockService = new Mock<IConnectionService>();
    mockService.Setup(s => s.ConnectAsync(It.IsAny<string>()))
        .ReturnsAsync(false);

    var command = new ConnectCommand(mockService.Object);
    var result = await command.ExecuteAsync("/dev/invalid");

    result.Should().Be(1);
}
```

**Implementation:**
- Create `ConnectCommand` using `System.CommandLine`
- Create `IConnectionService` interface
- Implement `ConnectionService` using `BasicControllerSession`
- Wire up dependency injection

**Commit:**
```
test: add connect command tests
feat: implement connect command
feat: add connection service with DI
```

---

#### Session 0.3: Version Information (30 min)

**Test First:**
```csharp
[Fact]
public async Task InfoCommand_WhenConnected_ShouldDisplayVersion()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.IsConnected).Returns(true);
    mockSession.Setup(s => s.Controller.Version).Returns("Z-Wave 7.19.1");

    var command = new InfoCommand(mockSession.Object);
    var result = await command.ExecuteAsync();

    result.Should().Be(0);
    // Verify output contains version
}

[Fact]
public async Task InfoCommand_WhenNotConnected_ShouldReturnError()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.IsConnected).Returns(false);

    var command = new InfoCommand(mockSession.Object);
    var result = await command.ExecuteAsync();

    result.Should().Be(1);
}
```

**Implementation:**
- Create `InfoCommand`
- Query controller version, chip type, home ID, etc.
- Format output (use `Spectre.Console` for nice tables)

**Commit:**
```
test: add info command tests
feat: implement info command
feat: add formatted console output
```

---

#### Session 0.4: List Ports Command (25 min)

**Test First:**
```csharp
[Fact]
public async Task ListPortsCommand_ShouldDisplayAvailablePorts()
{
    var mockDetector = new Mock<ISerialPortDetector>();
    mockDetector.Setup(d => d.GetAvailablePorts())
        .Returns(new[]
        {
            new PortInfo("/dev/ttyUSB0", "Silicon Labs CP210x"),
            new PortInfo("/dev/ttyUSB1", "FTDI USB Serial")
        });

    var command = new ListPortsCommand(mockDetector.Object);
    var result = await command.ExecuteAsync();

    result.Should().Be(0);
}
```

**Implementation:**
- Create `ListPortsCommand`
- Display ports in table format
- Add `--verbose` flag for detailed info

**Commit:**
```
test: add list-ports command tests
feat: implement list-ports command
```

---

#### Session 0.5: Integration & Manual Testing (40 min)

**Tasks:**
1. Connect real USB controller
2. Test full flow:
   ```bash
   dotnet run -- list-ports
   dotnet run -- connect /dev/ttyUSB0
   dotnet run -- info
   ```
3. Fix any issues found
4. Update tests based on real behavior
5. Add error handling for edge cases

**Commit:**
```
test: add integration tests for MVP flow
fix: handle controller disconnect gracefully
docs: add usage examples to README
```

---

### Iteration 1: Node Discovery

**Goal:** List nodes in network and view node information

**Sessions:** 3-4 (2 hours)

#### Session 1.1: List Nodes Command (35 min)

**Test First:**
```csharp
[Fact]
public async Task ListNodesCommand_ShouldDisplayNodes()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.GetNodes())
        .Returns(new[]
        {
            new Node { Id = 1, Name = "Controller" },
            new Node { Id = 2, Name = "Switch" },
            new Node { Id = 3, Name = "Sensor" }
        });

    var command = new ListNodesCommand(mockSession.Object);
    var result = await command.ExecuteAsync();

    result.Should().Be(0);
}
```

**Implementation:**
- Create `ListNodesCommand`
- Query all nodes from controller
- Display: Node ID, Name, Status, Type

**Commit:**
```
test: add list-nodes command tests
feat: implement list-nodes command
```

---

#### Session 1.2: Node Info Command (35 min)

**Test First:**
```csharp
[Theory]
[InlineData(2)]
[InlineData(5)]
public async Task NodeInfoCommand_WithValidNodeId_ShouldDisplayInfo(int nodeId)
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.GetNodeInfo(nodeId))
        .Returns(new NodeInfo
        {
            NodeId = nodeId,
            Listening = true,
            CommandClasses = new[] { 0x20, 0x25 }
        });

    var command = new NodeInfoCommand(mockSession.Object);
    var result = await command.ExecuteAsync(nodeId);

    result.Should().Be(0);
}

[Fact]
public async Task NodeInfoCommand_WithInvalidNodeId_ShouldReturnError()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.GetNodeInfo(999))
        .Returns((NodeInfo)null);

    var command = new NodeInfoCommand(mockSession.Object);
    var result = await command.ExecuteAsync(999);

    result.Should().Be(1);
}
```

**Implementation:**
- Create `NodeInfoCommand`
- Display detailed node information
- Show command classes, capabilities, security

**Commit:**
```
test: add node-info command tests
feat: implement node-info command with details
```

---

#### Session 1.3: Request Node Info (30 min)

**Test First:**
```csharp
[Fact]
public async Task RequestNodeInfoCommand_ShouldUpdateNodeInfo()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.RequestNodeInfo(2))
        .ReturnsAsync(true);

    var command = new RequestNodeInfoCommand(mockSession.Object);
    var result = await command.ExecuteAsync(2);

    result.Should().Be(0);
    mockSession.Verify(s => s.RequestNodeInfo(2), Times.Once);
}
```

**Implementation:**
- Create `RequestNodeInfoCommand`
- Send NIF request
- Wait for response

**Commit:**
```
test: add request-node-info command tests
feat: implement request-node-info command
```

---

#### Session 1.4: Integration Testing (30 min)

**Tasks:**
- Test with real Z-Wave network
- Verify node discovery
- Fix timing issues
- Add verbose logging option

**Commit:**
```
test: add integration tests for node discovery
fix: handle timeout in node info requests
feat: add --verbose flag for debug logging
```

---

### Iteration 2: Basic Commands

**Goal:** Send commands to nodes and view responses

**Sessions:** 4-5 (2.5 hours)

#### Session 2.1: Send Command (40 min)

**Test First:**
```csharp
[Fact]
public async Task SendCommand_WithValidPayload_ShouldSucceed()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.SendData(2, It.IsAny<byte[]>(), It.IsAny<TxOptions>()))
        .ReturnsAsync(true);

    var command = new SendCommand(mockSession.Object);
    var result = await command.ExecuteAsync(2, "25 01"); // Basic Set On

    result.Should().Be(0);
}

[Theory]
[InlineData("invalid")]
[InlineData("ZZ")]
public async Task SendCommand_WithInvalidPayload_ShouldReturnError(string payload)
{
    var command = new SendCommand(Mock.Of<IControllerSession>());
    var result = await command.ExecuteAsync(2, payload);

    result.Should().Be(1);
}
```

**Implementation:**
- Create `SendCommand`
- Parse hex payload
- Send to specified node
- Display response

**Commit:**
```
test: add send command tests
feat: implement send command with hex parsing
```

---

#### Session 2.2: Basic Commands (Switch, Dimmer) (35 min)

**Test First:**
```csharp
[Theory]
[InlineData(true)]   // On
[InlineData(false)]  // Off
public async Task BasicSetCommand_ShouldSendCorrectPayload(bool on)
{
    var mockSession = new Mock<IControllerSession>();
    var expectedPayload = new byte[] { 0x20, 0x01, (byte)(on ? 0xFF : 0x00) };

    mockSession.Setup(s => s.SendData(2, expectedPayload, It.IsAny<TxOptions>()))
        .ReturnsAsync(true);

    var command = new BasicSetCommand(mockSession.Object);
    var result = await command.ExecuteAsync(2, on);

    result.Should().Be(0);
}

[Fact]
public async Task BasicGetCommand_ShouldRequestValue()
{
    var mockSession = new Mock<IControllerSession>();
    var command = new BasicGetCommand(mockSession.Object);
    var result = await command.ExecuteAsync(2);

    mockSession.Verify(s => s.SendData(2,
        It.Is<byte[]>(b => b[0] == 0x20 && b[1] == 0x02),
        It.IsAny<TxOptions>()), Times.Once);
}
```

**Implementation:**
- Create `BasicSetCommand` (on/off)
- Create `BasicGetCommand`
- Add convenience commands for common operations

**Commit:**
```
test: add basic command class tests
feat: implement basic set/get commands
```

---

#### Session 2.3: Command History/Log (30 min)

**Test First:**
```csharp
[Fact]
public void CommandLogger_ShouldRecordCommands()
{
    var logger = new CommandLogger();
    logger.Log(2, new byte[] { 0x20, 0x01, 0xFF }, true);

    var history = logger.GetHistory();
    history.Should().HaveCount(1);
    history[0].NodeId.Should().Be(2);
    history[0].Success.Should().BeTrue();
}
```

**Implementation:**
- Create `CommandLogger` service
- Record all sent commands
- Add `history` command to view log
- Save to file (optional)

**Commit:**
```
test: add command logger tests
feat: implement command history logging
feat: add history command
```

---

#### Session 2.4: Watch Mode (30 min)

**Test First:**
```csharp
[Fact]
public async Task WatchCommand_ShouldDisplayIncomingFrames()
{
    var mockSession = new Mock<IControllerSession>();
    var frameReceived = new ManualResetEventSlim();

    mockSession.Setup(s => s.FrameReceived += It.IsAny<EventHandler<FrameEventArgs>>())
        .Callback<EventHandler<FrameEventArgs>>(handler =>
        {
            handler?.Invoke(this, new FrameEventArgs(new byte[] { 0x00, 0x04, 0x00 }));
            frameReceived.Set();
        });

    var command = new WatchCommand(mockSession.Object);
    var task = command.ExecuteAsync();

    frameReceived.Wait(TimeSpan.FromSeconds(1));
    task.Should().NotBeNull();
}
```

**Implementation:**
- Create `WatchCommand`
- Subscribe to frame received events
- Display frames in real-time
- Press Ctrl+C to exit

**Commit:**
```
test: add watch command tests
feat: implement watch command for monitoring
```

---

#### Session 2.5: Integration & Polish (35 min)

**Tasks:**
- Test with real devices
- Verify command sequences
- Add retry logic for failed commands
- Improve error messages

**Commit:**
```
test: add integration tests for basic commands
fix: retry failed commands up to 3 times
docs: add command examples to README
```

---

### Iteration 3: Network Management

**Goal:** Add/remove nodes, reset controller

**Sessions:** 6-8 (4 hours)

#### Session 3.1: Add Node Start (40 min)

**Test First:**
```csharp
[Fact]
public async Task AddNodeCommand_ShouldStartInclusionMode()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.StartAddNode(AddNodeMode.Any, It.IsAny<Action<AddNodeStatus>>()))
        .Returns(true);

    var command = new AddNodeCommand(mockSession.Object);
    var result = await command.ExecuteAsync();

    result.Should().Be(0);
    mockSession.Verify(s => s.StartAddNode(It.IsAny<AddNodeMode>(), It.IsAny<Action<AddNodeStatus>>()), Times.Once);
}
```

**Implementation:**
- Create `AddNodeCommand`
- Start inclusion mode
- Handle callbacks (node found, protocol done)
- Display progress

**Commit:**
```
test: add node inclusion tests
feat: implement add-node command
```

---

#### Session 3.2: Add Node Callbacks (35 min)

**Test First:**
```csharp
[Fact]
public async Task AddNodeCommand_WhenNodeAdded_ShouldDisplayNodeInfo()
{
    var mockSession = new Mock<IControllerSession>();
    AddNodeStatus receivedStatus = null;

    mockSession.Setup(s => s.StartAddNode(AddNodeMode.Any, It.IsAny<Action<AddNodeStatus>>()))
        .Callback<AddNodeMode, Action<AddNodeStatus>>((mode, callback) =>
        {
            receivedStatus = new AddNodeStatus
            {
                Status = AddNodeStatusType.Done,
                NodeId = 5
            };
            callback(receivedStatus);
        })
        .Returns(true);

    var command = new AddNodeCommand(mockSession.Object);
    var result = await command.ExecuteAsync();

    result.Should().Be(0);
    receivedStatus.Should().NotBeNull();
    receivedStatus.NodeId.Should().Be(5);
}
```

**Implementation:**
- Handle status callbacks
- Display progress messages
- Timeout after 30 seconds
- Stop inclusion on Ctrl+C

**Commit:**
```
test: add node inclusion callback tests
feat: implement inclusion status handling
feat: add timeout and cancellation support
```

---

#### Session 3.3: Remove Node (35 min)

**Test First:**
```csharp
[Fact]
public async Task RemoveNodeCommand_ShouldStartExclusionMode()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.StartRemoveNode(It.IsAny<Action<RemoveNodeStatus>>()))
        .Returns(true);

    var command = new RemoveNodeCommand(mockSession.Object);
    var result = await command.ExecuteAsync();

    result.Should().Be(0);
}
```

**Implementation:**
- Create `RemoveNodeCommand`
- Start exclusion mode
- Handle callbacks
- Display removed node info

**Commit:**
```
test: add node exclusion tests
feat: implement remove-node command
```

---

#### Session 3.4: Remove Failed Node (30 min)

**Test First:**
```csharp
[Fact]
public async Task RemoveFailedNodeCommand_WithFailedNode_ShouldRemove()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.IsFailedNode(5)).Returns(true);
    mockSession.Setup(s => s.RemoveFailedNode(5)).ReturnsAsync(true);

    var command = new RemoveFailedNodeCommand(mockSession.Object);
    var result = await command.ExecuteAsync(5);

    result.Should().Be(0);
}

[Fact]
public async Task RemoveFailedNodeCommand_WithHealthyNode_ShouldReturnError()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.IsFailedNode(5)).Returns(false);

    var command = new RemoveFailedNodeCommand(mockSession.Object);
    var result = await command.ExecuteAsync(5);

    result.Should().Be(1);
}
```

**Implementation:**
- Create `RemoveFailedNodeCommand`
- Check if node is failed first
- Remove from network
- Clean up routing info

**Commit:**
```
test: add remove-failed-node tests
feat: implement remove-failed-node command
```

---

#### Session 3.5: Reset Controller (25 min)

**Test First:**
```csharp
[Fact]
public async Task ResetCommand_WithConfirmation_ShouldReset()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.SetDefault()).ReturnsAsync(true);

    var command = new ResetCommand(mockSession.Object);
    var result = await command.ExecuteAsync(confirm: true);

    result.Should().Be(0);
    mockSession.Verify(s => s.SetDefault(), Times.Once);
}

[Fact]
public async Task ResetCommand_WithoutConfirmation_ShouldNotReset()
{
    var mockSession = new Mock<IControllerSession>();

    var command = new ResetCommand(mockSession.Object);
    var result = await command.ExecuteAsync(confirm: false);

    result.Should().Be(1);
    mockSession.Verify(s => s.SetDefault(), Times.Never);
}
```

**Implementation:**
- Create `ResetCommand`
- Require `--confirm` flag
- Warn about data loss
- Reset controller to factory defaults

**Commit:**
```
test: add reset command tests
feat: implement reset command with confirmation
```

---

#### Session 3.6: Learn Mode (30 min)

**Test First:**
```csharp
[Fact]
public async Task LearnModeCommand_ShouldStartLearning()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.StartLearnMode(It.IsAny<Action<LearnModeStatus>>()))
        .Returns(true);

    var command = new LearnModeCommand(mockSession.Object);
    var result = await command.ExecuteAsync();

    result.Should().Be(0);
}
```

**Implementation:**
- Create `LearnModeCommand`
- Start learn mode
- Handle callbacks
- Display new network info

**Commit:**
```
test: add learn mode tests
feat: implement learn-mode command
```

---

#### Session 3.7 & 3.8: Integration Testing (60-80 min)

**Tasks:**
- Test full inclusion/exclusion flow
- Add multiple nodes
- Remove nodes
- Test failed node removal
- Test reset (with test controller!)
- Document workflows

**Commit:**
```
test: add integration tests for network management
fix: handle edge cases in inclusion/exclusion
docs: add network management examples
feat: add --timeout option for inclusion/exclusion
```

---

### Iteration 4: Configuration

**Goal:** Get and set configuration parameters

**Sessions:** 4-5 (2.5 hours)

#### Session 4.1: Get Configuration (35 min)

**Test First:**
```csharp
[Fact]
public async Task ConfigGetCommand_ShouldRetrieveParameter()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.GetConfiguration(2, 1))
        .ReturnsAsync(new ConfigParameter
        {
            Number = 1,
            Value = 10,
            Size = 1
        });

    var command = new ConfigGetCommand(mockSession.Object);
    var result = await command.ExecuteAsync(2, 1);

    result.Should().Be(0);
}
```

**Implementation:**
- Create `ConfigGetCommand`
- Query parameter value
- Display with description (if available from XML)

**Commit:**
```
test: add config get tests
feat: implement config-get command
```

---

#### Session 4.2: Set Configuration (35 min)

**Test First:**
```csharp
[Theory]
[InlineData(1, 10, 1)]
[InlineData(2, 255, 1)]
[InlineData(3, 1000, 2)]
public async Task ConfigSetCommand_ShouldSetParameter(int param, int value, int size)
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.SetConfiguration(2, param, value, size))
        .ReturnsAsync(true);

    var command = new ConfigSetCommand(mockSession.Object);
    var result = await command.ExecuteAsync(2, param, value, size);

    result.Should().Be(0);
}
```

**Implementation:**
- Create `ConfigSetCommand`
- Set parameter value
- Verify set completed

**Commit:**
```
test: add config set tests
feat: implement config-set command
```

---

#### Session 4.3: List Configuration (30 min)

**Test First:**
```csharp
[Fact]
public async Task ConfigListCommand_ShouldDisplayAllParameters()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.GetAllConfiguration(2))
        .ReturnsAsync(new[]
        {
            new ConfigParameter { Number = 1, Value = 10 },
            new ConfigParameter { Number = 2, Value = 255 }
        });

    var command = new ConfigListCommand(mockSession.Object);
    var result = await command.ExecuteAsync(2);

    result.Should().Be(0);
}
```

**Implementation:**
- Create `ConfigListCommand`
- Query all parameters
- Display in table format

**Commit:**
```
test: add config list tests
feat: implement config-list command
```

---

#### Session 4.4: Configuration from XML (35 min)

**Test First:**
```csharp
[Fact]
public void ConfigurationProvider_ShouldLoadFromXml()
{
    var provider = new ConfigurationProvider();
    var config = provider.GetDeviceConfiguration(manufacturer: 0x0086, product: 0x0001);

    config.Should().NotBeNull();
    config.Parameters.Should().NotBeEmpty();
}
```

**Implementation:**
- Create `ConfigurationProvider`
- Load device XML configs
- Match by manufacturer/product ID
- Provide parameter descriptions

**Commit:**
```
test: add configuration provider tests
feat: implement XML configuration loading
```

---

#### Session 4.5: Integration Testing (30 min)

**Tasks:**
- Test with real devices
- Verify parameter changes
- Test various parameter sizes (1, 2, 4 bytes)

**Commit:**
```
test: add integration tests for configuration
fix: handle parameter size edge cases
docs: add configuration examples
```

---

### Iteration 5: Security

**Goal:** S2 inclusion and key management

**Sessions:** 5-6 (3.5 hours)

#### Session 5.1: Security Command Classes (40 min)

**Test First:**
```csharp
[Fact]
public async Task SecureInclusionCommand_ShouldStartS2Inclusion()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.StartSecureInclusion(It.IsAny<SecurityScheme>()))
        .Returns(true);

    var command = new SecureInclusionCommand(mockSession.Object);
    var result = await command.ExecuteAsync(SecurityScheme.S2_Authenticated);

    result.Should().Be(0);
}
```

**Implementation:**
- Create `SecureInclusionCommand`
- Support S0, S2_Unauthenticated, S2_Authenticated, S2_Access
- Handle KEX negotiation

**Commit:**
```
test: add secure inclusion tests
feat: implement secure-inclusion command
```

---

#### Session 5.2: DSK Input (35 min)

**Test First:**
```csharp
[Fact]
public async Task DskInputCommand_WithValidDSK_ShouldProvide()
{
    var mockSession = new Mock<IControllerSession>();
    var dsk = "12345-12345-12345-12345-12345-12345-12345-12345";

    var command = new DskInputCommand(mockSession.Object);
    var result = await command.ExecuteAsync(dsk);

    result.Should().Be(0);
    mockSession.Verify(s => s.ProvideDSK(It.IsAny<byte[]>()), Times.Once);
}

[Theory]
[InlineData("invalid")]
[InlineData("12345")]
public async Task DskInputCommand_WithInvalidDSK_ShouldReturnError(string dsk)
{
    var command = new DskInputCommand(Mock.Of<IControllerSession>());
    var result = await command.ExecuteAsync(dsk);

    result.Should().Be(1);
}
```

**Implementation:**
- Create `DskInputCommand`
- Parse DSK format
- Validate checksum
- Provide to controller

**Commit:**
```
test: add DSK input tests
feat: implement dsk-input command with validation
```

---

#### Session 5.3: Key Management (35 min)

**Test First:**
```csharp
[Fact]
public async Task ListKeysCommand_ShouldDisplayGrantedKeys()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.GetGrantedKeys(2))
        .Returns(new[] { SecurityScheme.S2_Authenticated, SecurityScheme.S2_Access });

    var command = new ListKeysCommand(mockSession.Object);
    var result = await command.ExecuteAsync(2);

    result.Should().Be(0);
}
```

**Implementation:**
- Create `ListKeysCommand`
- Display granted security keys per node
- Show security level

**Commit:**
```
test: add key management tests
feat: implement list-keys command
```

---

#### Session 5.4: Set Security Keys (35 min)

**Test First:**
```csharp
[Fact]
public async Task SetKeyCommand_ShouldConfigureNetworkKey()
{
    var mockSession = new Mock<IControllerSession>();
    var key = new byte[16]; // 128-bit key
    new Random().NextBytes(key);

    var command = new SetKeyCommand(mockSession.Object);
    var result = await command.ExecuteAsync(SecurityScheme.S2_Authenticated, key);

    result.Should().Be(0);
}
```

**Implementation:**
- Create `SetKeyCommand`
- Generate or input custom keys
- Store securely

**Commit:**
```
test: add set-key command tests
feat: implement key configuration
```

---

#### Session 5.5 & 5.6: Integration Testing (60-80 min)

**Tasks:**
- Test S2 inclusion flow
- Verify DSK exchange
- Test key grant/deny
- Test secure communication
- Document security workflows

**Commit:**
```
test: add integration tests for S2 inclusion
fix: handle KEX timeout
docs: add security examples
feat: add --insecure flag to skip S2
```

---

### Iteration 6: Advanced Features

**Goal:** Smart Start, firmware update, topology

**Sessions:** 8-10 (5 hours)

#### Session 6.1-6.2: Smart Start Provisioning (60-80 min)

**Test First:**
```csharp
[Fact]
public async Task AddProvisioningCommand_ShouldAddDSKToList()
{
    var mockSession = new Mock<IControllerSession>();
    var dsk = new byte[16];

    var command = new AddProvisioningCommand(mockSession.Object);
    var result = await command.ExecuteAsync(dsk, name: "Door Sensor");

    result.Should().Be(0);
}

[Fact]
public async Task ListProvisioningCommand_ShouldDisplayEntries()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.GetProvisioningList())
        .Returns(new[]
        {
            new ProvisioningEntry { DSK = new byte[16], Name = "Door Sensor" }
        });

    var command = new ListProvisioningCommand(mockSession.Object);
    var result = await command.ExecuteAsync();

    result.Should().Be(0);
}
```

**Implementation:**
- Create `AddProvisioningCommand`
- Create `ListProvisioningCommand`
- Create `RemoveProvisioningCommand`
- Manage Smart Start list

**Commit:**
```
test: add smart start provisioning tests
feat: implement provisioning list management
```

---

#### Session 6.3-6.4: Firmware Update (60-80 min)

**Test First:**
```csharp
[Fact]
public async Task FirmwareUpdateCommand_WithValidFile_ShouldUpdate()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.StartFirmwareUpdate(2, It.IsAny<byte[]>()))
        .ReturnsAsync(true);

    var command = new FirmwareUpdateCommand(mockSession.Object);
    var result = await command.ExecuteAsync(2, "firmware.bin");

    result.Should().Be(0);
}
```

**Implementation:**
- Create `FirmwareUpdateCommand`
- Read firmware file
- Send fragments
- Display progress bar
- Handle errors

**Commit:**
```
test: add firmware update tests
feat: implement firmware-update command
feat: add progress indicator
```

---

#### Session 6.5-6.6: Network Topology (60-80 min)

**Test First:**
```csharp
[Fact]
public async Task TopologyCommand_ShouldDisplayNeighbors()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.GetNodeNeighbors(2))
        .Returns(new[] { 1, 3, 5 });

    var command = new TopologyCommand(mockSession.Object);
    var result = await command.ExecuteAsync();

    result.Should().Be(0);
}
```

**Implementation:**
- Create `TopologyCommand`
- Query neighbor info for all nodes
- Display as ASCII art or graph format
- Export to DOT format (Graphviz)

**Commit:**
```
test: add topology mapping tests
feat: implement topology command
feat: add --export-dot option
```

---

#### Session 6.7-6.8: Routes & Diagnostics (60-80 min)

**Test First:**
```csharp
[Fact]
public async Task GetRouteCommand_ShouldDisplayRoutingInfo()
{
    var mockSession = new Mock<IControllerSession>();
    mockSession.Setup(s => s.GetRoute(2, 5))
        .Returns(new Route { Hops = new[] { 2, 3, 5 } });

    var command = new GetRouteCommand(mockSession.Object);
    var result = await command.ExecuteAsync(2, 5);

    result.Should().Be(0);
}
```

**Implementation:**
- Create `GetRouteCommand`
- Create `SetRouteCommand`
- Create `DiagnosticsCommand` (statistics, health)

**Commit:**
```
test: add routing and diagnostics tests
feat: implement route management
feat: add diagnostics command
```

---

#### Session 6.9-6.10: Integration & Polish (60-80 min)

**Tasks:**
- Test all advanced features
- Performance optimization
- Memory leak detection
- Error handling improvements

**Commit:**
```
test: add integration tests for advanced features
fix: optimize topology query performance
refactor: extract common command patterns
```

---

### Iteration 7: Polish & Package

**Goal:** Production-ready CLI with packaging

**Sessions:** 3-4 (2 hours)

#### Session 7.1: Comprehensive Help (30 min)

**Tasks:**
- Improve help text for all commands
- Add examples to help output
- Create man page (Unix)
- Add command aliases

**Commit:**
```
docs: improve command help text
docs: add usage examples to all commands
docs: generate man page
```

---

#### Session 7.2: Error Handling & Logging (40 min)

**Tasks:**
- Consistent error codes
- Structured logging (Serilog)
- Log levels (--verbose, --debug)
- Log to file option

**Commit:**
```
feat: add structured logging
feat: implement error code constants
docs: document exit codes
```

---

#### Session 7.3: Configuration File (30 min)

**Test First:**
```csharp
[Fact]
public void ConfigurationLoader_ShouldLoadFromFile()
{
    var config = ConfigurationLoader.Load("~/.zwavecli/config.json");
    config.DefaultPort.Should().NotBeNullOrEmpty();
}
```

**Implementation:**
- Create `~/.zwavecli/config.json`
- Store default port, log level, etc.
- Override with CLI arguments

**Commit:**
```
test: add configuration file tests
feat: implement config file support
```

---

#### Session 7.4: Packaging (40 min)

**Tasks:**
- Create publish scripts
- Self-contained Linux binary
- .deb package
- Installation script

**Commands:**
```bash
# Publish
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true

# Create .deb
# Use dpkg-deb or install dotnet-deb tool
```

**Commit:**
```
build: add publish script
build: create .deb package
docs: add installation instructions
```

---

## Session Breakdown by Iteration

### Summary Table

| Iteration | Feature | Sessions | Est. Time | Tests | LOC |
|-----------|---------|----------|-----------|-------|-----|
| 0 | MVP (Connect & Info) | 5 | 2.5h | 15-20 | 500 |
| 1 | Node Discovery | 4 | 2h | 10-15 | 300 |
| 2 | Basic Commands | 5 | 2.5h | 15-20 | 400 |
| 3 | Network Management | 8 | 4h | 20-25 | 600 |
| 4 | Configuration | 5 | 2.5h | 15-20 | 350 |
| 5 | Security | 6 | 3.5h | 15-20 | 450 |
| 6 | Advanced Features | 10 | 5h | 25-30 | 800 |
| 7 | Polish & Package | 4 | 2h | 10-15 | 200 |
| **Total** | **Full CLI** | **47** | **24h** | **125-165** | **3600** |

---

## Testing Strategy (Detailed)

### Test Coverage Goals

| Type | Coverage | Count | Execution Time |
|------|----------|-------|----------------|
| Unit Tests | 80%+ | 100-120 | <1 second |
| Integration Tests | Key Flows | 30-40 | <10 seconds |
| E2E Tests | Happy Paths | 5-10 | 1-5 minutes |

### Continuous Testing

**Pre-commit Hook:**
```bash
#!/bin/bash
# .git/hooks/pre-commit

dotnet test --no-build --verbosity quiet
if [ $? -ne 0 ]; then
    echo "Tests failed. Commit aborted."
    exit 1
fi
```

**CI/CD (GitHub Actions):**
```yaml
name: CI

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal
```

### Test Organization

```
ZWaveCLI.Tests/
├── Unit/
│   ├── Commands/
│   │   ├── ConnectCommandTests.cs
│   │   ├── InfoCommandTests.cs
│   │   └── ...
│   ├── Services/
│   │   ├── SerialPortDetectorTests.cs
│   │   ├── ConnectionServiceTests.cs
│   │   └── ...
│   └── Helpers/
│       └── TestDataBuilder.cs
├── Integration/
│   ├── ConnectionFlowTests.cs
│   ├── NodeManagementFlowTests.cs
│   └── ...
├── E2E/
│   └── FullWorkflowTests.cs (requires hardware)
└── TestUtilities/
    ├── Mocks/
    └── Fixtures/
```

---

## Progress Tracking

### Session Checklist Template

```markdown
## Session X.Y: [Feature Name]

**Date:** YYYY-MM-DD
**Duration:** [Planned] / [Actual]
**Status:** [ ] Not Started | [x] In Progress | [x] Complete

### Goals
- [ ] Write tests for [feature]
- [ ] Implement [feature]
- [ ] Refactor [aspect]

### Tests Written
- [ ] Test 1: [description]
- [ ] Test 2: [description]
- [ ] Test 3: [description]

### Implementation
- [ ] Create [class/file]
- [ ] Add [functionality]
- [ ] Handle [edge case]

### Commits
- [ ] test: [commit message]
- [ ] feat: [commit message]
- [ ] refactor: [commit message]

### Blockers
- None / [describe issue]

### Next Session
- [What to tackle next]

### Learnings
- [What went well]
- [What to improve]
```

### Iteration Progress Dashboard

```markdown
# CLI Migration Progress

| Iteration | Status | Sessions Done | Tests | Coverage | Notes |
|-----------|--------|---------------|-------|----------|-------|
| 0: MVP | ✅ Complete | 5/5 | 18 | 85% | Works on Linux |
| 1: Nodes | 🟡 In Progress | 2/4 | 6 | 70% | List works, info WIP |
| 2: Commands | ⭕ Not Started | 0/5 | 0 | - | - |
| 3: Network | ⭕ Not Started | 0/8 | 0 | - | - |
| 4: Config | ⭕ Not Started | 0/5 | 0 | - | - |
| 5: Security | ⭕ Not Started | 0/6 | 0 | - | - |
| 6: Advanced | ⭕ Not Started | 0/10 | 0 | - | - |
| 7: Polish | ⭕ Not Started | 0/4 | 0 | - | - |

**Overall:** 7/47 sessions (15%)
**Next:** Session 1.3 - Request Node Info
```

---

## Development Environment

### Required Tools

```bash
# .NET 8 SDK
dotnet --version  # Should be 8.0.x

# Git
git --version

# Optional: VS Code with C# extension
code --version
```

### Recommended VS Code Extensions

- C# (ms-dotnettools.csharp)
- C# Dev Kit (ms-dotnettools.csdevkit)
- Test Explorer (formulahendry.dotnet-test-explorer)
- GitLens (eamodio.gitlens)
- Code Spell Checker (streetsidesoftware.code-spell-checker)

### Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| Run Tests | Ctrl+; Ctrl+A |
| Run Current Test | Ctrl+; Ctrl+C |
| Debug Test | Ctrl+; Ctrl+D |
| Build Solution | Ctrl+Shift+B |
| Run Without Debug | Ctrl+F5 |

---

## Appendix: Command Reference

### Planned CLI Commands

```
zwavecli - Z-Wave network controller CLI

Usage:
  zwavecli [command] [options]

Connection:
  list-ports              List available serial ports
  connect <port>          Connect to Z-Wave controller
  disconnect              Disconnect from controller
  info                    Display controller information

Nodes:
  list-nodes              List all nodes in network
  node-info <id>          Display detailed node information
  request-node-info <id>  Request node information frame

Commands:
  send <id> <payload>     Send raw command to node
  basic-set <id> <on|off> Set basic value (on/off)
  basic-get <id>          Get basic value
  watch                   Monitor incoming frames

Network Management:
  add-node                Add node to network (inclusion)
  remove-node             Remove node from network (exclusion)
  remove-failed <id>      Remove failed node
  reset --confirm         Reset controller to factory defaults
  learn-mode              Start controller learn mode

Configuration:
  config-get <id> <param>         Get configuration parameter
  config-set <id> <param> <value> Set configuration parameter
  config-list <id>                List all parameters

Security:
  secure-inclusion <scheme>  Start secure inclusion (S0/S2)
  dsk-input <dsk>            Provide DSK for inclusion
  list-keys <id>             List granted keys for node
  set-key <scheme> <key>     Configure network key

Advanced:
  add-provisioning <dsk>     Add to Smart Start list
  list-provisioning          List provisioning entries
  firmware-update <id> <file> Update node firmware
  topology                   Display network topology
  get-route <from> <to>      Get routing information
  diagnostics                Show network diagnostics

Options:
  -h, --help      Show help
  -v, --verbose   Verbose output
  --debug         Debug logging
  --version       Show version

Examples:
  zwavecli connect /dev/ttyUSB0
  zwavecli list-nodes
  zwavecli basic-set 2 on
  zwavecli add-node
  zwavecli config-get 2 1
```

---

## GUI Development Strategy

### Overview

Once the CLI is complete (Iterations 0-7), the GUI development becomes **significantly simpler** because:

1. ✅ Backend is proven and tested on Linux
2. ✅ ViewModels already exist and are tested
3. ✅ Command execution patterns established
4. ✅ All business logic is working

The GUI becomes a **thin presentation layer** on top of the proven backend.

### Phase 2 Iterations (After CLI Complete)

| Iteration | Feature | Sessions | Time | Notes |
|-----------|---------|----------|------|-------|
| **GUI-0** | Setup & Main Window | 3-4 | 2h | Avalonia project, DI, navigation |
| **GUI-1** | Connection UI | 2-3 | 1.5h | Port list, connect button, status |
| **GUI-2** | Node Management UI | 4-5 | 2.5h | Node list, add/remove, node info |
| **GUI-3** | Command UI | 3-4 | 2h | Command sender, history, watch |
| **GUI-4** | Configuration UI | 3-4 | 2h | Parameter editor, XML configs |
| **GUI-5** | Security UI | 3-4 | 2h | S2 wizard, DSK input, key management |
| **GUI-6** | Advanced UI | 4-5 | 2.5h | Topology map, firmware update |
| **GUI-7** | Polish & Package | 3-4 | 2h | Themes, icons, installer |

**Total:** 25-33 sessions (16-20 hours)

### Shared ViewModel Example

The same ViewModel serves both CLI and GUI:

```csharp
// ZWaveController.ViewModels/ConnectionViewModel.cs
// This single class is used by BOTH CLI and GUI
public class ConnectionViewModel : INotifyPropertyChanged
{
    private readonly IConnectionService _connectionService;
    private readonly ISerialPortDetector _portDetector;

    public ObservableCollection<PortInfo> AvailablePorts { get; }

    private string _selectedPort;
    public string SelectedPort
    {
        get => _selectedPort;
        set
        {
            _selectedPort = value;
            OnPropertyChanged();
            ConnectCommand.NotifyCanExecuteChanged();
        }
    }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            _isConnected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public string StatusText => IsConnected
        ? $"✓ Connected to {SelectedPort}"
        : "Not connected";

    public IAsyncCommand ConnectCommand { get; }
    public IAsyncCommand DisconnectCommand { get; }
    public ICommand RefreshPortsCommand { get; }

    public ConnectionViewModel(
        IConnectionService connectionService,
        ISerialPortDetector portDetector)
    {
        _connectionService = connectionService;
        _portDetector = portDetector;

        AvailablePorts = new ObservableCollection<PortInfo>();

        ConnectCommand = new AsyncCommand(
            execute: ConnectAsync,
            canExecute: () => !string.IsNullOrEmpty(SelectedPort) && !IsConnected);

        DisconnectCommand = new AsyncCommand(
            execute: DisconnectAsync,
            canExecute: () => IsConnected);

        RefreshPortsCommand = new Command(RefreshPorts);

        RefreshPorts();
    }

    // CLI calls this directly
    public async Task<bool> ConnectAsync(string port)
    {
        var result = await _connectionService.ConnectAsync(port);
        if (result)
        {
            SelectedPort = port;
            IsConnected = true;
        }
        return result;
    }

    // GUI binds to ConnectCommand, which calls this
    private async Task ConnectAsync()
    {
        await ConnectAsync(SelectedPort);
    }

    private async Task DisconnectAsync()
    {
        await _connectionService.DisconnectAsync();
        IsConnected = false;
    }

    private void RefreshPorts()
    {
        AvailablePorts.Clear();
        foreach (var port in _portDetector.GetAvailablePorts())
        {
            AvailablePorts.Add(port);
        }
    }
}
```

### CLI Usage (Thin Wrapper)

```csharp
// ZWaveCLI/Commands/ConnectCommand.cs
public class ConnectCommand : Command
{
    private readonly ConnectionViewModel _viewModel;

    public ConnectCommand(ConnectionViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public async Task<int> ExecuteAsync(string port)
    {
        Console.WriteLine($"Connecting to {port}...");

        var result = await _viewModel.ConnectAsync(port);

        if (result)
        {
            Console.WriteLine($"✓ {_viewModel.StatusText}");
            return 0;
        }
        else
        {
            Console.WriteLine("✗ Connection failed");
            return 1;
        }
    }
}
```

### GUI Usage (Data Binding)

```xml
<!-- ZWaveGUI/Views/ConnectionView.axaml -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

  <StackPanel Spacing="10">

    <!-- Status -->
    <TextBlock Text="{Binding StatusText}"
               FontWeight="Bold"/>

    <!-- Port Selection -->
    <ComboBox ItemsSource="{Binding AvailablePorts}"
              SelectedItem="{Binding SelectedPort}"
              DisplayMemberBinding="{Binding DisplayName}"/>

    <!-- Actions -->
    <StackPanel Orientation="Horizontal" Spacing="10">
      <Button Command="{Binding ConnectCommand}"
              Content="Connect"/>
      <Button Command="{Binding DisconnectCommand}"
              Content="Disconnect"/>
      <Button Command="{Binding RefreshPortsCommand}"
              Content="Refresh"/>
    </StackPanel>

  </StackPanel>

</UserControl>
```

```csharp
// ZWaveGUI/Views/ConnectionView.axaml.cs
public partial class ConnectionView : UserControl
{
    public ConnectionView()
    {
        InitializeComponent();
    }
}
```

```csharp
// ZWaveGUI/ViewLocator.cs (Avalonia convention)
public class ViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        var name = data.GetType().FullName!
            .Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }
}
```

### Project Structure (Phase 1 + Phase 2)

```
z-wave-pc-controller/
├── ZWaveController/                  (Backend library - SHARED)
│   ├── Services/
│   ├── Models/
│   └── ZWaveController_netcore.csproj
│
├── ZWaveController.ViewModels/       (Presentation logic - SHARED)
│   ├── ConnectionViewModel.cs
│   ├── NodeManagementViewModel.cs
│   ├── ConfigurationViewModel.cs
│   ├── SecurityViewModel.cs
│   └── ZWaveController.ViewModels.csproj
│
├── ZWaveCLI/                         (CLI application - Phase 1)
│   ├── Commands/
│   │   ├── ConnectCommand.cs
│   │   ├── ListNodesCommand.cs
│   │   └── ...
│   ├── Program.cs
│   └── ZWaveCLI.csproj
│
├── ZWaveGUI/                         (GUI application - Phase 2)
│   ├── Views/
│   │   ├── ConnectionView.axaml
│   │   ├── NodeManagementView.axaml
│   │   └── ...
│   ├── App.axaml
│   ├── Program.cs
│   └── ZWaveGUI.csproj
│
├── ZWaveCLI.Tests/
│   └── ... (CLI-specific tests)
│
└── ZWaveGUI.Tests/
    └── ... (GUI-specific tests)
```

### Code Reuse Breakdown

| Component | CLI | GUI | Shared |
|-----------|-----|-----|--------|
| **Backend (ZWaveController)** | ✓ | ✓ | 100% |
| **ViewModels** | ✓ | ✓ | 100% |
| **Commands (ICommand)** | ✓ | ✓ | 100% |
| **Presentation Logic** | - | - | 0% (different) |
| **CLI Commands** | ✓ | - | 0% |
| **XAML Views** | - | ✓ | 0% |

**Overall Code Reuse:** ~90%

### GUI Session Example: Connection UI (Iteration GUI-1)

#### Session GUI-1.1: Connection View (40 min)

**Test First:**
```csharp
[Fact]
public void ConnectionView_ShouldBindToViewModel()
{
    // Arrange
    var viewModel = new ConnectionViewModel(
        Mock.Of<IConnectionService>(),
        Mock.Of<ISerialPortDetector>());

    // Act
    var view = new ConnectionView { DataContext = viewModel };

    // Assert
    view.DataContext.Should().Be(viewModel);
}

[Fact]
public async Task ConnectButton_WhenClicked_ShouldCallViewModel()
{
    // Arrange (UI testing with Avalonia)
    var mockService = new Mock<IConnectionService>();
    mockService.Setup(s => s.ConnectAsync(It.IsAny<string>()))
        .ReturnsAsync(true);

    var viewModel = new ConnectionViewModel(
        mockService.Object,
        Mock.Of<ISerialPortDetector>());

    var view = new ConnectionView { DataContext = viewModel };

    // Act
    viewModel.SelectedPort = "/dev/ttyUSB0";
    await viewModel.ConnectCommand.ExecuteAsync();

    // Assert
    mockService.Verify(s => s.ConnectAsync("/dev/ttyUSB0"), Times.Once);
    viewModel.IsConnected.Should().BeTrue();
}
```

**Implementation:**
1. Create `ConnectionView.axaml` with XAML
2. Set up data binding
3. Wire up commands
4. Test in app

**Commit:**
```
test: add connection view tests
feat: implement connection view UI
```

### Benefits of This Approach

| Benefit | Description |
|---------|-------------|
| **Proven Backend** | Backend is fully tested before GUI development |
| **Faster GUI Development** | 60-70% less work than full GUI rewrite |
| **Consistent Behavior** | Same ViewModels ensure CLI and GUI behave identically |
| **Independent Evolution** | CLI and GUI can be updated independently |
| **Lower Risk** | GUI bugs don't affect CLI, backend is isolated |
| **Better Testing** | Backend tests don't need UI automation |

### Timeline Summary

```
Week 1-2:  Iteration 0 (MVP CLI)           → Usable CLI
Week 3-4:  Iterations 1-3 (Core CLI)       → Feature-rich CLI
Week 5-6:  Iterations 4-7 (Advanced CLI)   → Complete CLI
Week 7-8:  GUI Iterations 0-3              → Basic GUI
Week 9-10: GUI Iterations 4-7              → Complete GUI

Total: 10 weeks part-time (4-5 hours/week)
       5 weeks full-time (8 hours/day)
```

### User Choice

```bash
# Power users / automation
$ zwavecli add-node --timeout 30
$ zwavecli list-nodes --format json > nodes.json
$ zwavecli config-set 2 1 10

# General users
$ zwavegui  # Opens GUI application
# Click "Add Node" button
# Visual feedback, wizards, drag-drop
```

### Parallel Maintenance

Once both are complete:

- Bug fix in backend → Both CLI and GUI benefit
- New feature → Implement in backend/ViewModel once, expose in both UIs
- CLI-specific improvement → Only touch CLI layer
- GUI-specific improvement → Only touch GUI layer

---

## Next Steps

1. **Set up project** (Session 0)
   ```bash
   cd /path/to/z-wave-pc-controller
   mkdir ZWaveControllerCLI
   cd ZWaveControllerCLI
   # Follow Session 0 setup
   ```

2. **Start Iteration 0** (MVP)
   - Session 0.1: Serial port detection
   - Session 0.2: Connect command
   - Session 0.3: Info command
   - Session 0.4: List ports command
   - Session 0.5: Integration testing

3. **Track progress**
   - Update progress dashboard after each session
   - Commit after each session
   - Run tests before committing

4. **Iterate**
   - Complete one iteration before starting next
   - Ensure all tests pass
   - Manual testing with hardware

---

## Success Criteria

### Iteration 0 (MVP) Success
- [ ] CLI runs on Linux without errors
- [ ] Can detect USB Z-Wave controllers
- [ ] Connects to controller successfully
- [ ] Displays controller version and info
- [ ] All tests pass (>80% coverage)
- [ ] No memory leaks
- [ ] Documented in README

### Phase 1 Complete (CLI - All 7 Iterations)
- [ ] CLI runs on Linux and Windows
- [ ] Feature parity with core operations
- [ ] 125-165 passing tests
- [ ] 80%+ code coverage
- [ ] Works on Ubuntu 22.04, 24.04
- [ ] .deb package available
- [ ] Comprehensive documentation
- [ ] CI/CD pipeline green

### Phase 2 Complete (GUI - Future)
- [ ] GUI runs on Linux and Windows
- [ ] Avalonia UI application
- [ ] Reuses 90%+ of backend code
- [ ] Rich visualizations (topology maps, graphs)
- [ ] Wizard-based workflows
- [ ] Unified installer (CLI + GUI bundle)

### Both Applications (End Goal)
- [ ] Share ZWaveController library
- [ ] Share ViewModels layer
- [ ] Consistent behavior across interfaces
- [ ] Same configuration format
- [ ] Interoperable (CLI scripts GUI workflows)
- [ ] User testimonial: "I use both depending on the task!"

---

**Document Status:** ✅ Ready for Implementation
**Last Updated:** 2025-11-08 (v1.1 - Added dual CLI+GUI architecture)
**Next Review:** After Iteration 0 completion

---

## Current Status

**Date:** 2025-11-09
**Phase:** Session 0 - Environment Setup
**Status:** ✅ **COMPLETE**

### Completed Tasks
- ✅ Backend upgraded to .NET 8 (`ZWaveController_netcore.csproj`)
- ✅ CLI project structure created (`ZWaveCLI/`)
- ✅ Test project structure created (`ZWaveCLI.Tests/`)
- ✅ Solution file updated with new projects
- ✅ Project files configured with dependencies
- ✅ Basic Program.cs with DI setup
- ✅ Test framework configured (xUnit, FluentAssertions, Moq, Verify)
- ✅ CLI README documentation created

### Project Structure Created
```
z-wave-pc-controller/
├── ZWaveCLI/
│   ├── Program.cs              # Entry point with DI
│   ├── Commands/               # Command implementations (empty)
│   ├── Services/               # CLI services (empty)
│   ├── Models/                 # CLI models (empty)
│   ├── README.md               # CLI documentation
│   └── ZWaveCLI.csproj        # .NET 8 project
├── ZWaveCLI.Tests/
│   ├── Commands/               # Command tests (empty)
│   ├── Services/               # Service tests (empty)
│   ├── Integration/            # Integration tests (empty)
│   ├── Helpers/
│   │   └── TestDataBuilder.cs # Test data builder template
│   ├── ProgramTests.cs         # Basic test file
│   └── ZWaveCLI.Tests.csproj  # .NET 8 test project
├── ZWaveController/
│   └── ZWaveController_netcore.csproj  # ✅ Upgraded to .NET 8
└── ZWaveController.sln         # ✅ Updated with CLI projects
```

### Next Steps (Iteration 0 - MVP)

**⚠️ PREREQUISITE**: Install .NET 8 SDK

Once .NET 8 SDK is available, verify the setup:
```bash
# Verify build
dotnet build ZWaveController.sln

# Run tests
dotnet test ZWaveCLI.Tests/ZWaveCLI.Tests.csproj

# Run CLI
dotnet run --project ZWaveCLI/ZWaveCLI.csproj -- --help
```

**Then proceed with Iteration 0:**
- Session 0.1: Serial Port Detection (30 min)
- Session 0.2: Connection Command (35 min)
- Session 0.3: Version Information (30 min)
- Session 0.4: List Ports Command (25 min)
- Session 0.5: Integration & Manual Testing (40 min)

See the [Project Setup](#project-setup) section for detailed instructions.

---

## Quick Start

Ready to begin? Here's your first hour:

**Session 0: Environment Setup** ✅ **COMPLETE**
- Project structure created
- Dependencies configured
- Solution file updated

**Session 0.1: First Test (30 min)** ⭕ **NEXT**
```csharp
// Write your first failing test
[Fact]
public void SerialPortDetector_ShouldDetectPorts()
{
    var detector = new SerialPortDetector();
    var ports = detector.GetAvailablePorts();
    ports.Should().NotBeNull();
}
// Make it pass! 🎉
```

Let's build this iteratively - **CLI first, then GUI**! 🚀
