using FluentAssertions;
using Xunit;
using ZWaveCLI.Models;
using ZWaveCLI.Services;

namespace ZWaveCLI.Tests.Services;

/// <summary>
/// Tests for SerialPortDetector service.
/// Following TDD approach: Write tests first, then implement.
/// </summary>
public class SerialPortDetectorTests
{
    [Fact]
    public void GetAvailablePorts_ShouldReturnNotNull()
    {
        // Arrange
        var detector = new SerialPortDetector();

        // Act
        var ports = detector.GetAvailablePorts();

        // Assert
        ports.Should().NotBeNull();
    }

    [Fact]
    public void GetAvailablePorts_ShouldReturnList()
    {
        // Arrange
        var detector = new SerialPortDetector();

        // Act
        var ports = detector.GetAvailablePorts();

        // Assert
        ports.Should().BeAssignableTo<IEnumerable<PortInfo>>();
    }

    [Fact]
    public void GetAvailablePorts_EachPortShouldHaveName()
    {
        // Arrange
        var detector = new SerialPortDetector();

        // Act
        var ports = detector.GetAvailablePorts();

        // Assert
        ports.Should().AllSatisfy(p => p.Name.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void GetAvailablePorts_OnLinux_ShouldDetectTtyDevices()
    {
        // Arrange
        var detector = new SerialPortDetector();

        // Act
        var ports = detector.GetAvailablePorts();

        // Assert
        // On Linux, if there are any serial devices, they should start with /dev/
        if (OperatingSystem.IsLinux() && ports.Any())
        {
            ports.Should().AllSatisfy(p => p.Name.Should().StartWith("/dev/"));
        }
    }

    [Theory]
    [InlineData("/dev/ttyUSB0", true)]
    [InlineData("/dev/ttyACM0", true)]
    [InlineData("/dev/ttyS0", true)]
    [InlineData("COM1", true)]
    [InlineData("", false)]
    [InlineData("/dev/invalid", true)] // Validation is just for format, not existence
    public void IsValidPortName_ShouldValidateFormat(string portName, bool expected)
    {
        // Arrange
        var detector = new SerialPortDetector();

        // Act
        var result = detector.IsValidPortName(portName);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetPortInfo_WithValidPort_ShouldReturnInfo()
    {
        // Arrange
        var detector = new SerialPortDetector();
        var ports = detector.GetAvailablePorts();

        // Skip if no ports available
        if (!ports.Any())
        {
            return;
        }

        var firstPort = ports.First();

        // Act
        var portInfo = detector.GetPortInfo(firstPort.Name);

        // Assert
        portInfo.Should().NotBeNull();
        portInfo!.Name.Should().Be(firstPort.Name);
    }

    [Fact]
    public void GetPortInfo_WithInvalidPort_ShouldReturnNull()
    {
        // Arrange
        var detector = new SerialPortDetector();

        // Act
        var portInfo = detector.GetPortInfo("/dev/this-port-definitely-does-not-exist-12345");

        // Assert
        portInfo.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        // Act
        var act = () => new SerialPortDetector();

        // Assert
        act.Should().NotThrow();
    }
}
