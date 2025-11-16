using System.IO.Ports;
using System.Text.RegularExpressions;
using ZWaveCLI.Models;

namespace ZWaveCLI.Services;

/// <summary>
/// Service for detecting and querying serial ports on the system.
/// Supports both Windows (COM ports) and Linux (/dev/tty* devices).
/// </summary>
public class SerialPortDetector : ISerialPortDetector
{
    private const string LinuxSysClassTtyPath = "/sys/class/tty";

    /// <summary>
    /// Gets a list of all available serial ports on the system.
    /// </summary>
    public IEnumerable<PortInfo> GetAvailablePorts()
    {
        var ports = new List<PortInfo>();

        try
        {
            // Get port names from System.IO.Ports
            var portNames = SerialPort.GetPortNames();

            foreach (var portName in portNames)
            {
                var portInfo = GetPortInfo(portName);
                if (portInfo != null)
                {
                    ports.Add(portInfo);
                }
            }

            // On Linux, also check /sys/class/tty for USB devices
            if (OperatingSystem.IsLinux())
            {
                var linuxPorts = GetLinuxUsbSerialPorts();
                foreach (var port in linuxPorts)
                {
                    if (!ports.Any(p => p.Name == port.Name))
                    {
                        ports.Add(port);
                    }
                }
            }
        }
        catch (Exception)
        {
            // Return empty list on error (e.g., permission issues)
            // In production, we'd log this error
        }

        return ports;
    }

    /// <summary>
    /// Gets detailed information about a specific port.
    /// </summary>
    public PortInfo? GetPortInfo(string portName)
    {
        if (string.IsNullOrWhiteSpace(portName))
        {
            return null;
        }

        try
        {
            // Check if port exists
            if (!DoesPortExist(portName))
            {
                return null;
            }

            var portInfo = new PortInfo
            {
                Name = portName,
                Description = GetPortDescription(portName)
            };

            // On Linux, try to get additional USB device info
            if (OperatingSystem.IsLinux())
            {
                EnrichWithLinuxUsbInfo(portInfo);
            }

            return portInfo;
        }
        catch (Exception)
        {
            // Return null on error
            return null;
        }
    }

    /// <summary>
    /// Validates whether a port name has a valid format.
    /// </summary>
    public bool IsValidPortName(string portName)
    {
        if (string.IsNullOrWhiteSpace(portName))
        {
            return false;
        }

        // Windows: COM1, COM2, etc.
        if (Regex.IsMatch(portName, @"^COM\d+$", RegexOptions.IgnoreCase))
        {
            return true;
        }

        // Linux: /dev/ttyUSB0, /dev/ttyACM0, /dev/ttyS0, etc.
        if (portName.StartsWith("/dev/tty", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a port exists in the system.
    /// </summary>
    private bool DoesPortExist(string portName)
    {
        try
        {
            // On Linux, check if the device file exists
            if (OperatingSystem.IsLinux())
            {
                return File.Exists(portName);
            }

            // On Windows, check if it's in the available ports list
            var portNames = SerialPort.GetPortNames();
            return portNames.Contains(portName, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a basic description for a port.
    /// </summary>
    private string GetPortDescription(string portName)
    {
        // Default description based on port type
        if (portName.Contains("ttyUSB", StringComparison.OrdinalIgnoreCase))
        {
            return "USB Serial Device";
        }
        else if (portName.Contains("ttyACM", StringComparison.OrdinalIgnoreCase))
        {
            return "USB ACM Device";
        }
        else if (portName.Contains("ttyS", StringComparison.OrdinalIgnoreCase))
        {
            return "Serial Port";
        }
        else if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
        {
            return "Serial Port";
        }

        return "Serial Device";
    }

    /// <summary>
    /// Gets USB serial ports from Linux sysfs.
    /// </summary>
    private IEnumerable<PortInfo> GetLinuxUsbSerialPorts()
    {
        var ports = new List<PortInfo>();

        if (!Directory.Exists(LinuxSysClassTtyPath))
        {
            return ports;
        }

        try
        {
            // Look for USB-to-serial adapters
            var ttyDirs = Directory.GetDirectories(LinuxSysClassTtyPath, "tty*");

            foreach (var ttyDir in ttyDirs)
            {
                var deviceName = Path.GetFileName(ttyDir);
                var devicePath = $"/dev/{deviceName}";

                // Check if it's a USB device
                var deviceLink = Path.Combine(ttyDir, "device");
                if (Directory.Exists(deviceLink))
                {
                    var realPath = GetRealPath(deviceLink);
                    if (realPath.Contains("/usb", StringComparison.OrdinalIgnoreCase))
                    {
                        if (File.Exists(devicePath))
                        {
                            var portInfo = GetPortInfo(devicePath);
                            if (portInfo != null)
                            {
                                ports.Add(portInfo);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // Ignore errors (e.g., permission issues)
        }

        return ports;
    }

    /// <summary>
    /// Enriches port info with Linux USB device information.
    /// </summary>
    private void EnrichWithLinuxUsbInfo(PortInfo portInfo)
    {
        try
        {
            var deviceName = Path.GetFileName(portInfo.Name);
            var sysPath = Path.Combine(LinuxSysClassTtyPath, deviceName, "device");

            if (!Directory.Exists(sysPath))
            {
                return;
            }

            // Walk up to find the USB device directory
            var usbDevicePath = FindUsbDeviceParent(sysPath);
            if (usbDevicePath == null)
            {
                return;
            }

            // Read VID/PID
            var vidPath = Path.Combine(usbDevicePath, "idVendor");
            var pidPath = Path.Combine(usbDevicePath, "idProduct");
            var manufacturerPath = Path.Combine(usbDevicePath, "manufacturer");
            var productPath = Path.Combine(usbDevicePath, "product");

            if (File.Exists(vidPath))
            {
                portInfo.VendorId = File.ReadAllText(vidPath).Trim().ToUpperInvariant();
            }

            if (File.Exists(pidPath))
            {
                portInfo.ProductId = File.ReadAllText(pidPath).Trim().ToUpperInvariant();
            }

            if (File.Exists(manufacturerPath))
            {
                portInfo.Manufacturer = File.ReadAllText(manufacturerPath).Trim();
            }

            if (File.Exists(productPath))
            {
                var product = File.ReadAllText(productPath).Trim();
                if (!string.IsNullOrEmpty(product))
                {
                    portInfo.Description = product;
                }
            }

            // Build richer description if we have VID/PID
            if (!string.IsNullOrEmpty(portInfo.VendorId) && !string.IsNullOrEmpty(portInfo.ProductId))
            {
                var desc = portInfo.Description;
                if (!string.IsNullOrEmpty(portInfo.Manufacturer))
                {
                    desc = $"{portInfo.Manufacturer} {desc}";
                }
                portInfo.Description = $"{desc} (VID:{portInfo.VendorId} PID:{portInfo.ProductId})";
            }
        }
        catch (Exception)
        {
            // Ignore errors reading USB info
        }
    }

    /// <summary>
    /// Finds the USB device parent directory in sysfs.
    /// </summary>
    private string? FindUsbDeviceParent(string path)
    {
        try
        {
            var current = GetRealPath(path);

            // Walk up the directory tree looking for idVendor and idProduct files
            while (!string.IsNullOrEmpty(current) && current != "/")
            {
                if (File.Exists(Path.Combine(current, "idVendor")) &&
                    File.Exists(Path.Combine(current, "idProduct")))
                {
                    return current;
                }

                current = Path.GetDirectoryName(current);
            }
        }
        catch (Exception)
        {
            // Ignore errors
        }

        return null;
    }

    /// <summary>
    /// Gets the real path by following symlinks.
    /// </summary>
    private string GetRealPath(string path)
    {
        try
        {
            // On Linux, resolve symlinks using the target
            if (OperatingSystem.IsLinux())
            {
                var info = new FileInfo(path);
                if (info.Exists && info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    return info.LinkTarget ?? path;
                }
            }

            return Path.GetFullPath(path);
        }
        catch
        {
            return path;
        }
    }
}
