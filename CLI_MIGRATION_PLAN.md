# Z-Wave PC Controller: CLI Migration Plan (TDD, Lean, Iterative)

**Version:** 1.0
**Date:** 2025-11-08
**Approach:** Test-Driven Development, Lean Iterations, 25-40 min sessions
**Target:** Linux-compatible CLI application with progressive feature addition

---

## Table of Contents

1. [Strategy Overview](#strategy-overview)
2. [MVP Definition](#mvp-definition)
3. [Project Setup](#project-setup)
4. [Iteration Plan](#iteration-plan)
5. [Session Structure](#session-structure)
6. [Testing Strategy](#testing-strategy)
7. [Implementation Roadmap](#implementation-roadmap)
8. [Session Breakdown by Iteration](#session-breakdown-by-iteration)

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

### Final Success (All Iterations)
- [ ] Feature parity with GUI app (core features)
- [ ] 100+ passing tests
- [ ] 80%+ code coverage
- [ ] Works on Ubuntu 22.04, 24.04
- [ ] .deb package available
- [ ] Comprehensive documentation
- [ ] CI/CD pipeline green
- [ ] User testimonial: "It just works!"

---

**Document Status:** ✅ Ready for Implementation
**Last Updated:** 2025-11-08
**Next Review:** After Iteration 0 completion

Let's build this iteratively! 🚀
