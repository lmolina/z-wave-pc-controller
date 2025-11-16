using Moq;
using ZWaveCLI.Models;
using ZWaveCLI.Services;

namespace ZWaveCLI.Tests.Helpers;

/// <summary>
/// Test data builder for creating common test objects and mocks.
/// This provides a consistent way to set up test data across all tests.
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a sample PortInfo for testing.
    /// </summary>
    public static PortInfo CreatePortInfo(
        string name = "/dev/ttyUSB0",
        string description = "USB Serial Device",
        string? vendorId = "10C4",
        string? productId = "EA60",
        string? manufacturer = "Silicon Labs")
    {
        return new PortInfo
        {
            Name = name,
            Description = description,
            VendorId = vendorId,
            ProductId = productId,
            Manufacturer = manufacturer
        };
    }

    /// <summary>
    /// Creates a list of sample PortInfo objects for testing.
    /// </summary>
    public static List<PortInfo> CreatePortInfoList()
    {
        return new List<PortInfo>
        {
            CreatePortInfo("/dev/ttyUSB0", "Silicon Labs CP210x UART Bridge", "10C4", "EA60", "Silicon Labs"),
            CreatePortInfo("/dev/ttyUSB1", "FTDI USB Serial Device", "0403", "6001", "FTDI"),
            CreatePortInfo("/dev/ttyACM0", "USB ACM Device", "2341", "0043", "Arduino")
        };
    }

    /// <summary>
    /// Creates a mock ISerialPortDetector for testing.
    /// </summary>
    public static Mock<ISerialPortDetector> CreateMockPortDetector(List<PortInfo>? ports = null)
    {
        var mock = new Mock<ISerialPortDetector>();
        var portList = ports ?? CreatePortInfoList();

        mock.Setup(d => d.GetAvailablePorts())
            .Returns(portList);

        mock.Setup(d => d.GetPortInfo(It.IsAny<string>()))
            .Returns<string>(name => portList.FirstOrDefault(p => p.Name == name));

        mock.Setup(d => d.IsValidPortName(It.IsAny<string>()))
            .Returns<string>(name => !string.IsNullOrWhiteSpace(name) &&
                                    (name.StartsWith("/dev/tty") || name.StartsWith("COM", StringComparison.OrdinalIgnoreCase)));

        return mock;
    }

    // TODO: Add more builders as we implement features
    // public static IDataSource CreateSerialPort(string port = "/dev/ttyUSB0")
    //     => new SerialPortDataSource(port, BaudRates.Rate_115200);

    // public static Mock<IControllerSession> CreateMockController()
    // {
    //     var mock = new Mock<IControllerSession>();
    //     mock.Setup(c => c.Controller.Version).Returns("Z-Wave 7.19.1");
    //     return mock;
    // }
}
