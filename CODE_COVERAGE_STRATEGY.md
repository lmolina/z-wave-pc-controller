# Code Coverage Strategy for Z-Wave CLI Migration

**Version:** 1.0
**Date:** 2025-11-09
**Target:** 80%+ coverage (mid-long term)
**Approach:** Incremental, test-driven, AI-agent optimized

---

## Executive Summary

### Current State Analysis

**Backend (ZWaveController):**
- **Lines of Code:** ~24,712 (290 files)
- **Existing Tests:** ~5,323 lines (NUnit, .NET Framework 4.8)
- **Estimated Coverage:** 40-50% (based on test/code ratio and test file inspection)
- **Test Framework:** NUnit 3.13 + NSubstitute (Windows-focused)

**Coverage by Area:**
| Area | Files Tested | Status | Notes |
|------|--------------|--------|-------|
| Network Management | ✅ | Good | `NetworkManagementTests.cs` (54K lines) |
| Security (S0/S2) | ✅ | Good | `SecuritySettingsTests.cs` (38K lines) |
| Command Classes | ✅ | Good | `CommandClassesTests.cs` (37K lines) |
| Connection/Sessions | ✅ | Good | `ConnectTests.cs` (13K lines) |
| Smart Start | ✅ | Good | `SmartStartTests.cs` (11K lines) |
| Configuration | ⚠️ | Minimal | `ConfigurationParametersTests.cs` (2.6K lines) |
| Services | ⚠️ | Partial | Only Polling, ERTT covered |
| Platform Abstraction | ❌ | None | No tests for SerialPortMonitor, SourcesInfoService |
| Models | ❌ | None | Session models not directly tested |

**Key Gaps:**
1. **Platform-specific code** (serial port detection, USB monitoring) - ❌ Not tested
2. **Services layer** (7 of 11 services untested)
3. **Error handling** (exception paths, timeouts)
4. **Linux-specific paths** (new code for migration)

---

## CLI Migration Coverage Strategy

### Philosophy

**Thin Wrapper = Focused Testing**

Since the CLI is a thin wrapper around the backend:
- ✅ **Don't retest backend logic** (already covered by existing tests)
- ✅ **Test CLI-specific code** (command parsing, argument validation, output formatting)
- ✅ **Test platform abstractions** (Linux serial port detection, etc.)
- ✅ **Integration tests** (end-to-end workflows on Linux)

### Coverage Targets by Iteration

| Iteration | New Code (LOC) | Test LOC | Target Coverage | Priority |
|-----------|----------------|----------|-----------------|----------|
| **0: MVP** | ~500 | 150-200 | 85%+ | **CRITICAL** |
| **1: Nodes** | ~300 | 100-150 | 80%+ | HIGH |
| **2: Commands** | ~400 | 150-200 | 80%+ | HIGH |
| **3: Network Mgmt** | ~600 | 200-250 | 75%+ | MEDIUM |
| **4: Configuration** | ~350 | 120-180 | 75%+ | MEDIUM |
| **5: Security** | ~450 | 150-200 | 75%+ | MEDIUM |
| **6: Advanced** | ~800 | 250-300 | 70%+ | LOW |
| **7: Polish** | ~200 | 100-150 | 80%+ | HIGH |
| **TOTAL** | ~3,600 | 1,220-1,630 | **80%+** | - |

**Overall CLI Coverage Goal:** 80%+ (achieved incrementally)

---

## High-Impact Coverage Targets

### Tier 1: CRITICAL (Must be ≥85% coverage)

These are the foundation - bugs here break everything.

#### 1. **Serial Port Detection** (Iteration 0.1)
**Why Critical:** Linux-specific, new code, platform abstraction layer.

**Files to Test:**
- `ZWaveCLI/Services/SerialPortDetector.cs` (new)
- `ZWaveCLI/Services/LinuxSerialPortEnumerator.cs` (new)

**Test Coverage Requirements:**
```
✅ GetAvailablePorts() - basic enumeration
✅ GetAvailablePorts() - parse /sys/class/tty/ for metadata
✅ GetPortInfo() - VID/PID extraction
✅ GetPortInfo() - fallback when /sys unavailable
✅ IsValidPort() - port name validation
✅ Edge cases: no ports, permission denied, symlinks
```

**Target:** 90%+ coverage (10-12 tests)

---

#### 2. **Connection Service** (Iteration 0.2)
**Why Critical:** Core functionality, every command depends on it.

**Files to Test:**
- `ZWaveCLI/Services/ConnectionService.cs` (new wrapper)
- Integration with `BasicControllerSession` (backend)

**Test Coverage Requirements:**
```
✅ ConnectAsync() - successful connection
✅ ConnectAsync() - timeout handling
✅ ConnectAsync() - invalid port
✅ ConnectAsync() - controller not responding
✅ DisconnectAsync() - clean disconnect
✅ DisconnectAsync() - already disconnected
✅ GetConnectionStatus() - various states
✅ Error message formatting
```

**Target:** 85%+ coverage (12-15 tests)

---

#### 3. **CLI Command Parsing** (Iteration 0.2-0.4)
**Why Critical:** User-facing, argument validation prevents crashes.

**Files to Test:**
- `ZWaveCLI/Commands/ConnectCommand.cs`
- `ZWaveCLI/Commands/ListPortsCommand.cs`
- `ZWaveCLI/Commands/InfoCommand.cs`

**Test Coverage Requirements:**
```
✅ Command registration in System.CommandLine
✅ Argument parsing (valid inputs)
✅ Argument validation (invalid inputs)
✅ Option flags (--verbose, --timeout, etc.)
✅ Exit codes (0 = success, 1 = error)
✅ Help text display
✅ Error message formatting
```

**Target:** 90%+ coverage (8-10 tests per command)

---

### Tier 2: HIGH (Should be ≥80% coverage)

Important functionality, but less foundational.

#### 4. **Command Execution** (Iterations 1-2)
**Files to Test:**
- `ZWaveCLI/Commands/ListNodesCommand.cs`
- `ZWaveCLI/Commands/NodeInfoCommand.cs`
- `ZWaveCLI/Commands/SendCommand.cs`
- `ZWaveCLI/Commands/BasicSetCommand.cs`

**Test Coverage Requirements:**
```
✅ Command execution flow
✅ Backend service interaction (mocked)
✅ Output formatting (tables, lists)
✅ Error handling (node not found, timeout)
✅ Payload parsing (hex strings)
```

**Target:** 80%+ coverage (6-8 tests per command)

---

#### 5. **Output Formatting** (All iterations)
**Files to Test:**
- `ZWaveCLI/Services/OutputFormatter.cs` (new)
- Spectre.Console integration

**Test Coverage Requirements:**
```
✅ Table rendering (ASCII)
✅ JSON output (--format json)
✅ Compact vs verbose modes
✅ Color coding (success/error/warning)
✅ Progress indicators
```

**Target:** 85%+ coverage (10-12 tests)

---

### Tier 3: MEDIUM (Should be ≥75% coverage)

Complex features, but backend handles most logic.

#### 6. **Network Management Commands** (Iteration 3)
**Files:** Add/remove node, reset, learn mode commands

**Test Coverage Requirements:**
```
✅ Command parsing
✅ Callback handling (node added, failed)
✅ Timeout management
✅ Cancellation (Ctrl+C)
✅ Status display
```

**Target:** 75%+ coverage (5-7 tests per command)

---

#### 7. **Configuration Commands** (Iteration 4)
**Files:** Config get/set/list commands

**Test Coverage Requirements:**
```
✅ Parameter parsing (number, value, size)
✅ XML configuration loading
✅ Parameter description lookup
✅ Value range validation
```

**Target:** 75%+ coverage (6-8 tests per command)

---

### Tier 4: LOW (Can be ≥70% coverage)

Advanced features, less frequently used.

#### 8. **Advanced Features** (Iteration 6)
**Files:** Topology, firmware update, provisioning

**Test Coverage Requirements:**
```
✅ Basic command execution
✅ Major error paths
✅ Output formatting
```

**Target:** 70%+ coverage (4-6 tests per command)

---

## Integration Testing Strategy

### End-to-End Test Scenarios

**Coverage Goal:** 10-15 E2E scenarios (manual + automated)

#### Automated Integration Tests (Mocked Hardware)
```csharp
// Example structure
[Collection("Integration")]
public class ConnectionFlowTests
{
    [Fact]
    public async Task FullFlow_ListPorts_Connect_GetInfo_Disconnect()
    {
        // Arrange: Mock controller session
        var mockSession = CreateMockController();

        // Act: Execute CLI commands in sequence
        var exitCode1 = await CLI.RunAsync(["list-ports"]);
        var exitCode2 = await CLI.RunAsync(["connect", "/dev/ttyUSB0"]);
        var exitCode3 = await CLI.RunAsync(["info"]);
        var exitCode4 = await CLI.RunAsync(["disconnect"]);

        // Assert: All succeed
        exitCode1.Should().Be(0);
        exitCode2.Should().Be(0);
        exitCode3.Should().Be(0);
        exitCode4.Should().Be(0);
    }
}
```

**Scenarios to Cover:**
1. ✅ List ports → Connect → Get info → Disconnect
2. ✅ Connect → List nodes → Get node info → Disconnect
3. ✅ Connect → Add node (success) → List nodes → Disconnect
4. ✅ Connect → Add node (timeout) → Cancel → Disconnect
5. ✅ Connect → Remove node → List nodes → Disconnect
6. ✅ Connect → Configure parameter → Verify → Disconnect
7. ✅ Connect → Send command → Watch response → Disconnect
8. ✅ Connection failure → Error handling → Retry
9. ✅ Invalid port → Error message → Exit
10. ✅ Controller disconnect during operation → Recovery

**Target:** 30-40 integration tests total

---

#### Manual Testing Checklist (Real Hardware)

**Execute once per iteration with real Z-Wave controller:**

**Iteration 0 (MVP):**
- [ ] List ports shows USB controller
- [ ] Connect to /dev/ttyUSB0 succeeds
- [ ] Info displays correct version
- [ ] Disconnect works cleanly
- [ ] Error messages are helpful

**Iteration 1-7:**
- [ ] Add test for each new feature
- [ ] Verify on Ubuntu 22.04 and 24.04
- [ ] Test with multiple controller types (700/800 series)

---

## Coverage Measurement Setup

### Tools Configuration

**For .NET 8 projects, use:**
- **Coverlet** (cross-platform coverage collector)
- **ReportGenerator** (HTML reports)

### Setup Instructions

```bash
# Add Coverlet to test project
cd ZWaveCLI.Tests
dotnet add package coverlet.collector
dotnet add package coverlet.msbuild

# Install ReportGenerator globally
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Running Coverage Reports

```bash
# Generate coverage data
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report
reportgenerator \
  -reports:"ZWaveCLI.Tests/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:"Html;Badges"

# Open report
xdg-open coverage-report/index.html  # Linux
# or
open coverage-report/index.html      # macOS
```

### CI/CD Integration

**Add to GitHub Actions:**

```yaml
# .github/workflows/test.yml
name: Tests with Coverage

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Run tests with coverage
        run: |
          dotnet test --collect:"XPlat Code Coverage"

      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: '**/coverage.cobertura.xml'
          fail_ci_if_error: true

      - name: Coverage threshold check
        run: |
          # Fail if coverage < 80%
          dotnet test /p:Threshold=80 /p:ThresholdType=line
```

---

## Per-Iteration Coverage Plan

### Iteration 0: MVP (Target: 85%+)

**New Code:** ~500 LOC
**Test Code:** 150-200 LOC
**Estimated Tests:** 15-20 tests

**Coverage Breakdown:**
```
Services/SerialPortDetector.cs       [90%] 10-12 tests
Services/ConnectionService.cs        [85%] 12-15 tests
Commands/ListPortsCommand.cs         [90%]  8-10 tests
Commands/ConnectCommand.cs           [90%]  8-10 tests
Commands/InfoCommand.cs              [90%]  8-10 tests
Program.cs (DI setup)                [70%]  3-5 tests
Models/PortInfo.cs                   [95%]  5-7 tests
```

**Excluded from coverage (documented):**
- Platform-specific P/Invoke declarations (not testable)
- Main() entry point (integration tested)

**Definition of Done:**
- [ ] All unit tests green
- [ ] Coverage report shows ≥85%
- [ ] Manual test on Linux succeeds
- [ ] Zero critical bugs

---

### Iteration 1: Node Discovery (Target: 80%+)

**New Code:** ~300 LOC
**Test Code:** 100-150 LOC
**Estimated Tests:** 10-15 tests

**Coverage Breakdown:**
```
Commands/ListNodesCommand.cs         [85%]  6-8 tests
Commands/NodeInfoCommand.cs          [85%]  6-8 tests
Commands/RequestNodeInfoCommand.cs   [80%]  5-7 tests
Services/NodeFormatter.cs            [90%]  8-10 tests
```

---

### Iteration 2: Basic Commands (Target: 80%+)

**New Code:** ~400 LOC
**Test Code:** 150-200 LOC
**Estimated Tests:** 15-20 tests

**Coverage Breakdown:**
```
Commands/SendCommand.cs              [85%]  8-10 tests
Commands/BasicSetCommand.cs          [90%]  6-8 tests
Commands/BasicGetCommand.cs          [90%]  6-8 tests
Commands/WatchCommand.cs             [75%]  5-7 tests
Services/CommandLogger.cs            [85%]  8-10 tests
Services/HexParser.cs                [95%] 10-12 tests
```

---

### Iterations 3-7: Incremental Coverage

**Follow same pattern:**
1. Write tests first (TDD)
2. Measure coverage after each session
3. Target ≥75% per iteration
4. Cumulative coverage trends toward 80%

---

## Coverage Monitoring Dashboard

### Per-Session Tracking

After each session, update this table:

| Session | New Code LOC | Test LOC | Coverage % | Tests Added | Status |
|---------|--------------|----------|------------|-------------|--------|
| 0.1     | 120          | 45       | 92%        | 12          | ✅ |
| 0.2     | 180          | 65       | 88%        | 15          | ✅ |
| 0.3     | 100          | 35       | 86%        | 10          | ✅ |
| 0.4     | 80           | 30       | 90%        | 8           | ✅ |
| 0.5     | 20           | 25       | 95%        | 5           | ✅ |
| **Iter 0 Total** | **500** | **200** | **88%** | **50** | ✅ |

**AI Agent Instruction:** Update this table after each commit with coverage numbers.

---

### Cumulative Coverage Tracking

| Iteration | Cumulative LOC | Cumulative Tests | Coverage % | Trend |
|-----------|----------------|------------------|------------|-------|
| 0         | 500            | 200              | 88%        | ⬆️ |
| 1         | 800            | 320              | 85%        | ➡️ |
| 2         | 1,200          | 520              | 83%        | ➡️ |
| 3         | 1,800          | 770              | 81%        | ➡️ |
| 4         | 2,150          | 950              | 80%        | ➡️ |
| 5         | 2,600          | 1,150            | 80%        | ➡️ |
| 6         | 3,400          | 1,400            | 78%        | ⬇️ |
| 7         | 3,600          | 1,550            | 81%        | ⬆️ |
| **FINAL** | **3,600**      | **1,550**        | **81%+**   | **✅** |

**Target:** Maintain ≥80% throughout.

---

## Effort vs Impact Matrix

### High Impact, Low Effort (Do First) 🚀

1. **Serial Port Detection** - New code, must test (10-12 tests, 2 hours)
2. **CLI Command Parsing** - Thin layer, easy to test (8-10 tests per command, 1 hour each)
3. **Output Formatting** - Pure logic, no I/O (10-12 tests, 1.5 hours)

### High Impact, High Effort (Do Second) ⚡

4. **Connection Service** - Complex interaction, but critical (12-15 tests, 3 hours)
5. **Integration Tests** - E2E flows, setup overhead (30-40 tests, 8 hours total)

### Medium Impact, Low Effort (Fill Gaps) 🎯

6. **Model Validation** - Simple property tests (5-7 tests per model, 30 min each)
7. **Error Message Formatting** - String formatting tests (6-8 tests, 1 hour)

### Low Impact, High Effort (Defer or Skip) ⏸️

8. **UI-specific code** (none in CLI)
9. **Platform P/Invoke** (mark as excluded)

---

## Quality Metrics (AI Agent KPIs)

### Per-Iteration Goals

After each iteration, verify:

✅ **Coverage:** ≥80% line coverage
✅ **Test/Code Ratio:** ≥0.4 (40 test lines per 100 code lines)
✅ **Test Execution:** All tests pass in <10 seconds
✅ **Zero Flaky Tests:** All tests deterministic
✅ **Documentation:** All public APIs have XML comments

### Red Flags (Require Immediate Action)

🚨 **Coverage drops below 75%** → Add tests before proceeding
🚨 **Test execution >10 seconds** → Optimize or parallelize
🚨 **Flaky test appears** → Fix immediately, don't merge
🚨 **Manual test fails** → Block iteration completion

---

## Exclusions and Rationale

### Code Excluded from Coverage

**Documented exclusions (don't count against 80% target):**

1. **Platform P/Invoke declarations**
   ```csharp
   [DllImport("libc")]
   private static extern int ioctl(...);
   ```
   **Reason:** Not testable without native library mocking

2. **Main() entry point**
   ```csharp
   static async Task<int> Main(string[] args)
   ```
   **Reason:** Tested via integration tests, not unit tests

3. **Logging statements**
   ```csharp
   _logger.LogDebug("Connected to {Port}", port);
   ```
   **Reason:** Testing logs is low value

4. **Auto-generated code**
   **Reason:** Not our code

**Mark exclusions with:**
```csharp
[ExcludeFromCodeCoverage]
public class PlatformInterop { ... }
```

---

## Backend Test Migration (Optional Enhancement)

**Current backend tests:** NUnit 3.13 (.NET Framework 4.8)
**Future option:** Migrate to xUnit (.NET 8) for consistency

**Effort:** ~40 hours
**Impact:** Medium (nice-to-have, not critical)

**Recommendation:** Defer until CLI complete. Backend tests work on .NET Framework.

---

## Success Criteria Summary

### Iteration 0 Complete When:
- [ ] ≥85% line coverage for new code
- [ ] All unit tests green (15-20 tests)
- [ ] 3-5 integration tests green
- [ ] Manual test on Linux succeeds
- [ ] Coverage report generated and reviewed
- [ ] Zero high-priority bugs

### Phase 1 Complete When (All 7 Iterations):
- [ ] ≥80% overall line coverage (3,600 LOC)
- [ ] 125-165 unit tests (per migration plan)
- [ ] 30-40 integration tests
- [ ] All manual tests pass on Ubuntu 22.04 and 24.04
- [ ] Coverage dashboard shows upward or stable trend
- [ ] CI/CD pipeline enforces coverage threshold

---

## AI Agent Instructions

### After Each Commit:

1. **Run coverage:**
   ```bash
   dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
   ```

2. **Check metrics:**
   - Coverage % (must be ≥target for iteration)
   - Test count (compare to estimate)
   - Execution time (<10 seconds)

3. **Update tracking tables:**
   - Per-Session Tracking
   - Cumulative Coverage Tracking

4. **Flag issues:**
   - Coverage below threshold → Add tests
   - Slow tests → Optimize
   - Flaky tests → Fix immediately

5. **Commit coverage report:**
   ```bash
   git add coverage.cobertura.xml
   git commit -m "test: coverage report for Session X.Y (Z%)"
   ```

### Before Proceeding to Next Iteration:

✅ Verify all checkboxes in "Success Criteria" are complete
✅ Review coverage report HTML
✅ Document any deviations from plan
✅ Update CLI_MIGRATION_PLAN.md with actuals vs. estimates

---

**Document Status:** ✅ Ready for Iteration 0
**Last Updated:** 2025-11-09
**Next Review:** After Iteration 0 completion
