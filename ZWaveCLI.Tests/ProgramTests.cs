using FluentAssertions;
using Xunit;

namespace ZWaveCLI.Tests;

/// <summary>
/// Tests for the main Program entry point.
/// These tests verify basic CLI functionality and setup.
/// </summary>
public class ProgramTests
{
    [Fact]
    public void RootCommand_Should_HaveDescription()
    {
        // This is a placeholder test to establish the testing pattern.
        // When we implement the actual CLI, this will be replaced with real tests.
        true.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_ReturnZero_WhenNoErrors()
    {
        // TODO: Test that successful execution returns exit code 0
        // This will be implemented in Session 0.2: Connection Command
        true.Should().BeTrue();
    }
}
