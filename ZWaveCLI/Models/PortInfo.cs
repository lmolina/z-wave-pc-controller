namespace ZWaveCLI.Models;

/// <summary>
/// Represents information about a serial port.
/// </summary>
public class PortInfo
{
    /// <summary>
    /// Gets or sets the port name (e.g., /dev/ttyUSB0, COM3).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable description of the port.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vendor ID (VID) if available.
    /// </summary>
    public string? VendorId { get; set; }

    /// <summary>
    /// Gets or sets the product ID (PID) if available.
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the manufacturer name if available.
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Gets a display-friendly name combining port name and description.
    /// </summary>
    public string DisplayName => string.IsNullOrEmpty(Description)
        ? Name
        : $"{Name} - {Description}";

    public override string ToString() => DisplayName;
}
