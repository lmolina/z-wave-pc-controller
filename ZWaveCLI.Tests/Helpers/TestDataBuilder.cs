using Moq;

namespace ZWaveCLI.Tests.Helpers;

/// <summary>
/// Test data builder for creating common test objects and mocks.
/// This provides a consistent way to set up test data across all tests.
/// </summary>
public static class TestDataBuilder
{
    // TODO: Add builders as we implement features
    // Example from the plan:
    // public static IDataSource CreateSerialPort(string port = "/dev/ttyUSB0")
    //     => new SerialPortDataSource(port, BaudRates.Rate_115200);

    // public static Mock<IControllerSession> CreateMockController()
    // {
    //     var mock = new Mock<IControllerSession>();
    //     mock.Setup(c => c.Controller.Version).Returns("Z-Wave 7.19.1");
    //     return mock;
    // }
}
