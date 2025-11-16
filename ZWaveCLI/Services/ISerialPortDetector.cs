using ZWaveCLI.Models;

namespace ZWaveCLI.Services;

/// <summary>
/// Interface for detecting and querying serial ports.
/// </summary>
public interface ISerialPortDetector
{
    /// <summary>
    /// Gets a list of all available serial ports on the system.
    /// </summary>
    /// <returns>A collection of port information objects.</returns>
    IEnumerable<PortInfo> GetAvailablePorts();

    /// <summary>
    /// Gets detailed information about a specific port.
    /// </summary>
    /// <param name="portName">The name of the port (e.g., /dev/ttyUSB0, COM3).</param>
    /// <returns>Port information if found; otherwise, null.</returns>
    PortInfo? GetPortInfo(string portName);

    /// <summary>
    /// Validates whether a port name has a valid format.
    /// </summary>
    /// <param name="portName">The port name to validate.</param>
    /// <returns>True if the port name format is valid; otherwise, false.</returns>
    bool IsValidPortName(string portName);
}
