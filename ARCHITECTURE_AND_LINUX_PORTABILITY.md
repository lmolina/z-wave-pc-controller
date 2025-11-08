# Z-Wave PC Controller: Architecture Analysis and Linux Portability Assessment

**Document Version:** 1.0
**Date:** 2025-11-08
**Target:** Linux Port Feasibility Study

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Overview](#project-overview)
3. [Architecture Analysis](#architecture-analysis)
4. [Dependency Analysis](#dependency-analysis)
5. [Platform-Specific Components](#platform-specific-components)
6. [Linux Portability Assessment](#linux-portability-assessment)
7. [Recommended Migration Strategy](#recommended-migration-strategy)
8. [Effort Estimation](#effort-estimation)
9. [References](#references)

---

## Executive Summary

The Z-Wave PC Controller is a Windows desktop application built on **.NET Framework 4.8** using **WPF (Windows Presentation Foundation)** for the UI layer. The application provides a comprehensive GUI tool for setting up and managing Z-Wave networks through Serial API controllers (USB or IP).

### Key Findings

**Current State:**
- Primary UI: WPF (Windows-only)
- Target Framework: .NET Framework 4.8
- Platform Dependencies: Windows Forms, WPF, Native DLLs (WebCam capture)
- Communication: Serial port, TCP/IP, ZIP protocol (Z-Wave over IP)

**Portability Status:**
- ✅ **High**: Backend library (ZWaveController) - mostly portable
- ⚠️ **Medium**: Serial communication - requires platform abstraction
- ❌ **Low**: UI layer (WPF) - complete rewrite needed
- ❌ **Low**: WebCam capture - native DLL replacement required

**Recommended Approach:**
1. Migrate to **.NET 6/8** (cross-platform)
2. Replace WPF with **Avalonia UI** or **MAUI** (cross-platform UI frameworks)
3. Replace native WebCam DLLs with cross-platform alternatives (V4L2 on Linux)
4. Leverage existing .NET Core project variants where available

---

## Project Overview

### Purpose
Z-Wave PC Controller is a GUI tool for:
- Z-Wave network setup and management
- Node discovery, configuration, and control
- Firmware updates (OTA and local)
- Network topology visualization and analysis
- Security management (S0/S2 encryption)
- Smart Start provisioning
- Performance testing and monitoring

### Target Hardware
- Z-Wave Serial API controllers via:
  - USB/Serial (COM ports)
  - TCP/IP (J-Link devices)
  - ZIP protocol (Z-Wave over IP with DTLS encryption)

### Build System
- **IDE:** Visual Studio 2022
- **Build Tool:** MSBuild
- **Solution File:** `ZWaveController.sln`
- **Projects:**
  - `ZWaveControllerUI` - WPF desktop application
  - `ZWaveController` - Backend library
  - `Tests` - Unit tests
  - `IntegrationTests` - Integration tests

---

## Architecture Analysis

### High-Level Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                 ZWaveControllerUI (WPF App)                   │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ Presentation Layer (MVVM Pattern)                      │  │
│  │ ├─ Views (41 XAML files)                               │  │
│  │ ├─ ViewModels (50+ classes)                            │  │
│  │ ├─ Commands (100+ command classes)                     │  │
│  │ └─ Data Binding & Converters                           │  │
│  └────────────────────────────────────────────────────────┘  │
└─────────────────────┬────────────────────────────────────────┘
                      │
                      ▼
┌──────────────────────────────────────────────────────────────┐
│             ZWaveController (Backend Library)                 │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ Business Logic Layer                                   │  │
│  │ ├─ Controller Sessions (Basic, ZIP)                    │  │
│  │ ├─ Services (Polling, ERTT, NodeInfo, etc.)           │  │
│  │ ├─ Models (Session, Network, Security)                │  │
│  │ └─ Configuration Management                            │  │
│  └────────────────────────────────────────────────────────┘  │
└─────────────────────┬────────────────────────────────────────┘
                      │
     ┌────────────────┼────────────────┬─────────────────┐
     │                │                │                 │
     ▼                ▼                ▼                 ▼
┌─────────┐    ┌──────────┐    ┌──────────┐    ┌───────────────┐
│  Serial │    │ TCP/IP   │    │   ZIP    │    │  WebCam (DLL) │
│ (COM)   │    │ (Socket) │    │ (DTLS)   │    │  (Platform)   │
└─────────┘    └──────────┘    └──────────┘    └───────────────┘
     │                │                │                 │
     └────────────────┴────────────────┴─────────────────┘
                      │
                      ▼
              Z-Wave Controllers
            (USB/Serial/IP/Wireless)
```

### Component Breakdown

#### 1. **ZWaveControllerUI (Presentation Layer)**

**Technology:** WPF (Windows Presentation Foundation)
**Pattern:** MVVM (Model-View-ViewModel)
**Target Framework:** .NET Framework 4.8

**Key Components:**

| Component | Count | Purpose |
|-----------|-------|---------|
| XAML Views | 41 | UI definitions (windows, dialogs, controls) |
| ViewModels | 50+ | Presentation logic and data binding |
| Commands | 100+ | User actions and operations |
| Converters | 50+ | Data transformation for binding |
| Custom Controls | 15+ | Specialized UI components |

**Main Views:**
- `MainWindow.xaml` - Primary application container
- `NetworkManagementView.xaml` - Node add/remove operations
- `ConfigurationView.xaml` - Parameter configuration
- `TopologyMapView.xaml` - Network visualization
- `SmartStartView.xaml` - Provisioning management
- `SecuritySettingsWindow.xaml` - S0/S2 security setup

**UI Framework Dependencies:**
```csharp
// WPF Core
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PresentationCore;
using PresentationFramework;
using WindowsBase;

// Windows Forms (for dialogs)
using System.Windows.Forms; // FolderBrowserDialog
using System.Drawing; // Graphics, color conversions
```

**NuGet Packages:**
- `MaterialDesignColors` (v2.0.4) - Material design color palettes
- `MaterialDesignThemes` (v4.3.0) - Material design components
- `ZXing.Net` (v0.16.9) - QR code generation/scanning

---

#### 2. **ZWaveController (Backend Library)**

**Technology:** Class Library
**Target Frameworks:** .NET Framework 4.8, .NET Core 6.0 (dual variants)
**Architecture:** Layered, service-oriented

**Key Services:**

| Service | File | Purpose | Platform-Specific |
|---------|------|---------|-------------------|
| SourcesInfoService | `Services/SourcesInfoService.cs` | Device discovery (Serial/TCP/ZIP) | ⚠️ Partial (Serial) |
| PollingService | `Services/PollingService.cs` | Periodic device polling | ✅ Portable |
| ERTTService | `Services/ERTTService.cs` | Extended Range Test Tool | ✅ Portable |
| NodeInformationService | `Services/NodeInformationService.cs` | Node metadata management | ✅ Portable |
| JammingDetectionService | `Services/JammingDetectionService.cs` | RF jamming detection | ✅ Portable |
| WebCamCaptureService | `Services/WebCamCaptureService.cs` | QR code scanning via webcam | ❌ Native DLL |
| PredefinedPayloadsService | `Services/PredefinedPayloadsService.cs` | Command templates | ✅ Portable |
| WakeUpNodesService | `Services/WakeUpNodesService.cs` | Wake-up management | ✅ Portable |

**Core Models:**

| Model | Purpose | Complexity |
|-------|---------|------------|
| BasicControllerSession | Serial/TCP session management | 2800+ lines |
| ZipControllerSession | ZIP protocol handler | Complex |
| ConnectModel | Connection configuration | Simple |
| SerialPortMonitor | USB device insertion/removal | Platform-specific |
| TraceCaptureModel | Protocol trace recording | Simple |
| IMAFullNetwork | Network topology | Complex |

**Session Architecture:**

```csharp
// Session creation based on data source type
public IControllerSession CreateControllerSession(
    IDataSource selectedDataSource,
    IApplicationModel applicationModel)
{
    if (selectedDataSource is SocketDataSource socketSource &&
        socketSource.Type == SoketSourceTypes.ZIP)
    {
        return new ZipControllerSession(applicationModel);
    }
    else
    {
        return new BasicControllerSession(applicationModel);
    }
}
```

**Communication Stack:**

```
ApplicationLayer (ZWave.BasicApplication)
    ↓
SessionLayer (ZWave.Layers.Session)
    ↓
FrameLayer (BasicFrame or XModem)
    ↓
TransportLayer
    ├─ SerialPortTransportLayer (COM ports)
    ├─ TcpClientTransportLayer (TCP sockets)
    └─ DtlsTransportLayer (ZIP with encryption)
    ↓
Physical Layer
```

---

## Dependency Analysis

### Internal Dependencies (z-wave-tools-core)

These are external projects that must be built alongside the PC Controller:

| Library | Purpose | APIs Used | Linux Compatibility |
|---------|---------|-----------|---------------------|
| **BasicApplication** | Serial protocol implementation | `BasicApplicationLayer`, `Device`, `ApplicationClient`, `Operations` | ✅ .NET Core variant exists |
| **Utils** | Utility classes and UI infrastructure | `IDataSource`, `ILogService`, `ComputerSystemHardwareHelper` | ⚠️ Partial (Win32 device info) |
| **ZipApplication** | Z-Wave over IP protocol | `ZipControllerDiscoverService`, `DtlsTransportLayer` | ✅ Network-based, portable |
| **ZWave** | Core Z-Wave protocol | `Device`, `Layers`, `Security`, `CommandClasses` | ✅ .NET Core variant exists |
| **ZWaveXml** | XML configuration parsing | `ZWave.Xml.Application` | ✅ Standard XML APIs |

#### BasicApplication APIs

**Key Classes:**
```csharp
namespace ZWave.BasicApplication
{
    // Device abstraction
    public class Device : IDevice
    {
        CommunicationStatuses Connect(IDataSource dataSource);
        bool GetVersion(bool waitResponse, int timeout);
        int SendData(byte nodeId, byte[] frame, ...);
    }

    // Application layer
    public class BasicApplicationLayer
    {
        BasicApplicationLayer(ISessionLayer, IFrameLayer, ITransportLayer);
        ApplicationClient CreateEndDevice(bool isListening);
    }

    // Operations
    namespace Operations
    {
        public class AddNodeToNetworkOperation;
        public class RemoveNodeFromNetworkOperation;
        public class SetLearnModeOperation;
    }
}
```

**Usage in PC Controller:**
- Device connection and initialization
- Version negotiation
- Command queue management
- Network management operations

---

#### Utils APIs

**Key Classes:**
```csharp
namespace Utils
{
    // Data sources
    public interface IDataSource
    {
        string SourceName { get; }
        string Description { get; }
        Guid SourceId { get; }
    }

    public class SerialPortDataSource : IDataSource
    {
        SerialPortDataSource(string portName, BaudRates baudRate);
    }

    public class SocketDataSource : IDataSource
    {
        SocketDataSource(string ipAddress, int port);
        SocketDataSource(string ipAddress, int port, byte[] psk); // ZIP
        SoketSourceTypes Type { get; } // TCP or ZIP
    }

    // Platform-specific helper (Windows only)
    public static class ComputerSystemHardwareHelper
    {
        List<Win32PnPEntityClass> GetWin32PnPEntityClassSerialPortDevices();
        List<string> GetDeviceNames(); // Fallback to basic enumeration
    }
}

namespace Utils.UI
{
    public interface ILogService
    {
        void Log(string message, ...);
    }
}
```

**Platform-Specific Code:**
- `ComputerSystemHardwareHelper` uses **System.Management** (WMI queries) on Windows
- Enumerates COM ports with detailed device information (VID/PID, description)
- **Conditional compilation** for .NET Core: uses `SerialPort.GetPortNames()` instead

**Linux Alternative:**
- Use `System.IO.Ports.SerialPort.GetPortNames()` (cross-platform in .NET 6+)
- Parse `/sys/class/tty/` for device details on Linux
- Use `udev` rules for USB device metadata

---

#### ZipApplication APIs

**Key Classes:**
```csharp
namespace ZWave.ZipApplication
{
    // ZIP discovery
    public class ZipControllerDiscoverService
    {
        ZipControllerDiscoverService(
            string ipv6BroadcastAddr,
            string ipv4BroadcastAddr,
            int udpPort);

        List<string> Discover(); // Returns IP addresses
    }

    // DTLS transport
    public class DtlsTransportLayer : ITransportLayer
    {
        // Pre-Shared Key (PSK) authentication
        // DTLS 1.2 encryption
    }

    // Constants
    public static class Constants
    {
        const string Ipv6DiscoverBroadcastAddress = "ff02::2";
        const string Ipv4DiscoverBroadcastAddress = "255.255.255.255";
        const int UdpPortNo = 4123;
        const int DtlsPortNo = 41230;
        const byte[] DefaultPsk = { ... };
    }
}
```

**Usage:**
- UDP broadcast for ZIP gateway discovery (IPv4/IPv6)
- DTLS encrypted tunneling for Z-Wave over IP
- Cross-platform (uses standard .NET sockets)

**Linux Compatibility:** ✅ Full - uses standard networking APIs

---

#### ZWave APIs (Core Protocol)

**Key Namespaces:**
```csharp
namespace ZWave
{
    // Device abstraction
    public interface IDevice
    {
        string Version { get; }
        byte[] DSK { get; } // Device-Specific Key
        ChipTypes ChipType { get; }
    }
}

namespace ZWave.Layers
{
    // Protocol layers
    public interface ISessionLayer { }
    public interface IFrameLayer { }
    public interface ITransportLayer
    {
        int WriteData(byte[] data);
        byte[] ReadData(int timeout);
    }

    // Implementations
    public class BasicFrameLayer : IFrameLayer;
    public class XModemFrameLayer : IFrameLayer;
    public class SerialPortTransportLayer : ITransportLayer;
    public class TcpClientTransportLayer : ITransportLayer;
}

namespace ZWave.CommandClasses
{
    public class COMMAND_CLASS_BASIC;
    public class COMMAND_CLASS_VERSION;
    public class COMMAND_CLASS_SECURITY;
    public class COMMAND_CLASS_SECURITY_2;
    // 100+ command classes
}

namespace ZWave.Security
{
    public class SecurityManager
    {
        // S0/S2 key management
    }

    namespace S2
    {
        public class KEXSetData; // Key Exchange
        public class DSKManager; // Device-Specific Key
    }
}
```

**Linux Compatibility:** ✅ Full - protocol logic is platform-agnostic

---

### External NuGet Packages

| Package | Version | Purpose | Linux Compatibility |
|---------|---------|---------|---------------------|
| **AutoMapper** | 10.0.0 / 13.0.1 | Object-to-object mapping | ✅ Full |
| **Newtonsoft.Json** | 13.0.3 | JSON serialization | ✅ Full |
| **MaterialDesignColors** | 2.0.4 | WPF color themes | ❌ WPF-specific |
| **MaterialDesignThemes** | 4.3.0 | WPF controls | ❌ WPF-specific |
| **ZXing.Net** | 0.16.9 | QR code generation | ✅ Core library portable |

**Notes:**
- AutoMapper and Newtonsoft.Json are fully cross-platform
- Material Design packages are WPF-specific (replaced in cross-platform UI)
- ZXing.Net core is portable; WPF-specific bindings need replacement

---

### System Dependencies

| Namespace | Purpose | Linux Alternative |
|-----------|---------|-------------------|
| **System.Windows.*** | WPF UI framework | ❌ Avalonia UI / .NET MAUI |
| **System.Windows.Forms** | Folder/file dialogs | ❌ Use cross-platform dialog libraries |
| **System.Drawing** | Graphics, image manipulation | ⚠️ System.Drawing.Common (deprecated on Linux) → SkiaSharp |
| **System.IO.Ports** | Serial port communication | ✅ Available in .NET 6+ |
| **System.Net.Sockets** | TCP/UDP networking | ✅ Full cross-platform support |
| **System.Net.NetworkInformation** | Network interface enumeration | ✅ Full cross-platform support |
| **System.Management** | WMI queries (Windows) | ❌ Linux: Parse /sys, /proc, use udev |

---

### Native Dependencies

#### WebCam Capture DLLs

**Files:**
- `wcvcap64.dll` (64-bit Windows)
- `wcvcap32.dll` (32-bit Windows)
- Location: `z-wave-blobs/z-wave-pc-controller/WebCamCapture/win/`

**Usage:**
```csharp
// ZWaveController/Services/WebCamCaptureService.cs
[DllImport("wcvcap64", EntryPoint = "OpenWebCam", CallingConvention = CallingConvention.Cdecl)]
private extern static bool OpenWebCam64(
    uint deviceNo,
    OnFrameInputDelegate onFrameInput,
    OnErrorDelegate onError);

[DllImport("wcvcap64", EntryPoint = "CloseWebCam", CallingConvention = CallingConvention.Cdecl)]
private extern static void CloseWebCam64(uint deviceNo);
```

**Purpose:**
- Capture frames from webcam for QR code scanning
- Used in Smart Start provisioning (DSK scanning)

**API Surface:**
```
OpenWebCam(deviceNo, frameCallback, errorCallback) -> bool
CloseWebCam(deviceNo) -> void

Callbacks:
- OnFrameInputDelegate(width, height, stride, pixelFormat, data)
- OnErrorDelegate(errorMessage)
```

**Linux Alternatives:**

| Library | API | Pros | Cons |
|---------|-----|------|------|
| **V4L2 (Video4Linux2)** | `/dev/video*` devices | Native Linux API | Requires P/Invoke or wrapper |
| **OpenCV (EmguCV)** | `VideoCapture` class | Cross-platform, .NET bindings | Large dependency |
| **AForge.NET** | `VideoCaptureDevice` | Simple API | Windows-focused |
| **DirectShow.NET** | - | - | Windows-only |
| **FFmpeg + wrapper** | `libavcodec`, `libavformat` | Powerful, cross-platform | Complex integration |

**Recommended:** Use **OpenCV (Emgu.CV NuGet)** for cross-platform support
```csharp
using Emgu.CV;
using Emgu.CV.Structure;

var capture = new VideoCapture(0); // Device 0
Mat frame = new Mat();
capture.Read(frame);
```

---

## Platform-Specific Components

### 1. Conditional Compilation (#if !NETCOREAPP)

**SerialPortMonitor.cs** (Lines 18-48)
```csharp
public void Open()
{
#if !NETCOREAPP
    // Windows-specific: USB device insertion/removal events
    SerialPortMontiorHelper.PortsChanged += SerialPortMontiorHelper_PortsChanged;
#endif
}

#if !NETCOREAPP
private void SerialPortMontiorHelper_PortsChanged(object sender, PortsChangedArgs e)
{
    if (e.EventType == EventType.Insertion)
    {
        // Auto-reconnect on device insertion
        _controllerSession.Connect(_controllerSession.DataSource);
    }
    else
    {
        // Test connection and disconnect if failed
        _controllerSession.Disconnect();
    }
}
#endif
```

**Purpose:** Monitor USB COM port insertion/removal for automatic reconnection

**Linux Alternative:**
- Use **udev** rules and monitor `/dev/` changes
- Implement file system watcher for `/dev/ttyUSB*`, `/dev/ttyACM*`
- Or disable feature for initial port

---

**SourcesInfoService.cs** (Lines 165-204)
```csharp
private void AppendSerialPorts(List<IDataSource> dataSources)
{
#if NETCOREAPP
    // .NET Core: Simple port enumeration
    var serialPorts = SerialPortTransportClient.GetPortNames();
    dataSources.AddRange(serialPorts.Select(portName =>
        new SerialPortDataSource(portName, BaudRates.Rate_115200)));
#else
    // Windows: Rich device information via WMI
    FillWithWin32DeviceInfo(dataSources,
        ComputerSystemHardwareHelper.GetWin32PnPEntityClassSerialPortDevices);
#endif
}

#if !NETCOREAPP
private void FillWithWin32DeviceInfo<T>(List<IDataSource> dataSources,
    Func<List<T>> devicesListProvider) where T : Win32PnPEntityClass
{
    List<T> win32DevicesList = devicesListProvider.Invoke();
    // Extract device description, VID/PID, etc.
}
#endif
```

**Purpose:** Enumerate serial ports with device metadata

**Linux Alternative:**
```csharp
// .NET 6+ System.IO.Ports
var ports = SerialPort.GetPortNames();

// Rich metadata: Parse /sys/class/tty/*/device/
foreach (var port in ports)
{
    var sysPath = $"/sys/class/tty/{Path.GetFileName(port)}/device/";
    if (Directory.Exists(sysPath))
    {
        var vid = File.ReadAllText($"{sysPath}/idVendor").Trim();
        var pid = File.ReadAllText($"{sysPath}/idProduct").Trim();
        var description = File.ReadAllText($"{sysPath}/product").Trim();
    }
}
```

---

### 2. Windows Forms Integration

**Usage in WPF Views:**
```csharp
using System.Windows.Forms; // FolderBrowserDialog

// Example from FolderBrowserDialogViewModel
var dialog = new FolderBrowserDialog();
if (dialog.ShowDialog() == DialogResult.OK)
{
    SelectedPath = dialog.SelectedPath;
}
```

**Files:**
- `ZWaveControllerUI/Models/Dialogs/FolderBrowserDialogViewModel.cs`
- `ZWaveControllerUI/Models/Dialogs/OpenFileDialogViewModel.cs`
- `ZWaveControllerUI/Models/Dialogs/SaveFileDialogViewModel.cs`

**Linux Alternative:**
- **Avalonia UI:** Built-in `OpenFolderDialog`, `OpenFileDialog`
- **.NET MAUI:** `FolderPicker`, `FilePicker` (cross-platform)
- **GTK#:** Native Linux file dialogs

---

### 3. System.Drawing Dependencies

**Usage:**
```csharp
using System.Drawing; // Graphics, Color, Bitmap
using System.Drawing.Imaging; // PixelFormat

// Example: TopologyMapView network visualization
Graphics graphics = Graphics.FromImage(bitmap);
graphics.DrawLine(pen, point1, point2);
```

**Issue:** `System.Drawing.Common` is deprecated on Linux (requires libgdiplus)

**Alternative:** **SkiaSharp** (cross-platform 2D graphics)
```csharp
using SkiaSharp;

var surface = SKSurface.Create(width, height, SKColorType.Rgba8888);
var canvas = surface.Canvas;
canvas.DrawLine(x0, y0, x1, y1, paint);
```

---

### 4. WPF Framework Dependencies

**References in ZWaveControllerUI.csproj:**
```xml
<Reference Include="WindowsBase" />
<Reference Include="PresentationCore" />
<Reference Include="PresentationFramework" />
<Reference Include="System.Xaml" />
<Reference Include="UIAutomationProvider" />
```

**XAML Features Used:**
- Data binding (OneWay, TwoWay)
- Value converters
- Dependency properties
- Routed events
- Styles and templates
- Triggers and animations

**Migration Path:**
- **Avalonia UI:** Near 1:1 XAML compatibility
- **.NET MAUI:** Different XAML dialect, requires adaptation
- **Uno Platform:** Pixel-perfect WPF compatibility

---

## Linux Portability Assessment

### Component-by-Component Analysis

| Component | Portability | Effort | Notes |
|-----------|-------------|--------|-------|
| **ZWaveController (Backend)** | ✅ High | Low | Mostly portable; conditional compilation already exists |
| **Serial Communication** | ✅ High | Low | .NET 6+ System.IO.Ports works on Linux |
| **TCP/ZIP Networking** | ✅ Full | None | Standard sockets API |
| **WebCam Capture** | ❌ Low | Medium | Native DLL replacement needed |
| **Serial Port Monitor** | ⚠️ Medium | Medium | USB hotplug via udev |
| **Device Enumeration** | ⚠️ Medium | Low | Parse /sys/class/tty/ |
| **WPF UI Layer** | ❌ None | High | Complete UI rewrite |
| **Windows Forms Dialogs** | ❌ None | Low | Replace with cross-platform dialogs |
| **System.Drawing** | ⚠️ Medium | Medium | Replace with SkiaSharp |
| **External Dependencies** | ✅ High | Low | z-wave-tools-core has .NET Core variants |

---

### Critical Path Items

#### 1. UI Framework Migration (High Effort)

**Current:** WPF on .NET Framework 4.8
**Target:** Cross-platform UI framework

**Option A: Avalonia UI** ⭐ Recommended
- **Pros:**
  - XAML-based (similar to WPF)
  - Excellent Linux support
  - Material Design themes available
  - Active development
  - Can reuse MVVM ViewModels
- **Cons:**
  - Some WPF APIs not supported
  - Custom controls may need rewrite
- **Migration Effort:** 60-80% of XAML can be reused

**Option B: .NET MAUI**
- **Pros:**
  - Official Microsoft cross-platform framework
  - Mobile support (iOS/Android)
- **Cons:**
  - Different XAML dialect
  - Desktop Linux support still maturing
  - More mobile-focused
- **Migration Effort:** ~50% XAML reuse

**Option C: Uno Platform**
- **Pros:**
  - Pixel-perfect WPF/UWP compatibility
  - WebAssembly support
- **Cons:**
  - Larger runtime
  - More complex tooling
- **Migration Effort:** 80-90% XAML reuse

**Recommendation:** **Avalonia UI** for best Linux experience

---

#### 2. WebCam Capture Replacement (Medium Effort)

**Current:** Native Windows DLLs (wcvcap32/64.dll)

**Option A: Emgu.CV (OpenCV wrapper)** ⭐ Recommended
```csharp
// Cross-platform, mature, well-documented
using Emgu.CV;

var capture = new VideoCapture(0);
capture.ImageGrabbed += (sender, e) => {
    Mat frame = new Mat();
    capture.Retrieve(frame);
    // Process frame for QR code detection
};
capture.Start();
```

**Option B: DirectShow.NET + V4L2 wrapper**
- Platform-specific implementations
- More control but higher complexity

**Option C: FFmpeg with wrapper**
- Overkill for this use case
- Licensing considerations (LGPL/GPL)

**Recommendation:** **Emgu.CV NuGet package**

---

#### 3. Serial Port Monitoring (Medium Effort)

**Current:** Windows WMI events for USB insertion/removal

**Linux Implementation:**
```csharp
// Option A: FileSystemWatcher on /dev/
var watcher = new FileSystemWatcher("/dev/");
watcher.Created += (s, e) => {
    if (e.Name.StartsWith("ttyUSB") || e.Name.StartsWith("ttyACM"))
    {
        // Device inserted
    }
};

// Option B: udev monitoring (via P/Invoke)
// Requires libudev wrapper or native interop

// Option C: Disable feature for MVP
// Manual refresh button only
```

**Recommendation:** Start with **Option C** (disable), add **Option A** later

---

#### 4. Device Enumeration (Low Effort)

**Current:** WMI queries for device metadata

**Linux Implementation:**
```csharp
public static List<SerialPortInfo> GetLinuxSerialPorts()
{
    var ports = new List<SerialPortInfo>();
    var portNames = SerialPort.GetPortNames();

    foreach (var portName in portNames)
    {
        var info = new SerialPortInfo { PortName = portName };

        // Parse sysfs for metadata
        var devName = Path.GetFileName(portName);
        var sysPath = $"/sys/class/tty/{devName}/device/";

        if (Directory.Exists(sysPath))
        {
            try
            {
                info.VendorId = File.ReadAllText($"{sysPath}../idVendor").Trim();
                info.ProductId = File.ReadAllText($"{sysPath}../idProduct").Trim();
                info.Description = File.ReadAllText($"{sysPath}../product").Trim();
            }
            catch { /* Fallback to port name only */ }
        }

        ports.Add(info);
    }

    return ports;
}
```

---

### External Dependency Requirements

#### z-wave-tools-core Repositories

These must be available and built:

| Repository | Status | Notes |
|------------|--------|-------|
| **z-wave-tools-core** | ⚠️ Check for .NET Core support | Contains BasicApplication, ZWave, Utils, etc. |
| **z-wave-blobs** | ❌ Windows-only binaries | WebCam DLLs not needed after replacement |

**Verification Steps:**
1. Clone z-wave-tools-core alongside z-wave-pc-controller
2. Check for `*_netcore.csproj` variants
3. Build with .NET 6+ SDK
4. Run tests on Linux

**Conditional Build:**
```bash
# Build for .NET 6
dotnet build ZWaveController_netcore.csproj -c Release

# Build dependencies
cd ../z-wave-tools-core/ZWave
dotnet build ZWave_netcore.csproj -c Release
```

---

## Recommended Migration Strategy

### Phase 1: Backend Preparation (2-3 weeks)

**Goal:** Ensure backend library runs on .NET 6/8 on Linux

1. **Set up Linux build environment**
   - Install .NET 6/8 SDK on Ubuntu/Debian
   - Clone z-wave-tools-core and z-wave-blobs
   - Build ZWaveController_netcore.csproj

2. **Stub out platform-specific code**
   ```csharp
   // ZWaveController/Services/WebCamCaptureService.cs
   #if LINUX
   public bool Start() => false; // Stub for Phase 2
   #endif
   ```

3. **Test serial communication**
   - Connect USB Z-Wave controller
   - Verify `/dev/ttyUSB0` (or similar) access
   - Test `SerialPort.GetPortNames()` on Linux
   - Run basic connection test

4. **Test networking**
   - Verify TCP discovery
   - Test ZIP protocol on local network
   - Validate DTLS encryption

**Success Criteria:**
- Backend library builds without errors on Linux
- Serial communication works
- TCP/ZIP protocols functional
- Unit tests pass

---

### Phase 2: UI Migration (4-6 weeks)

**Goal:** Reimplement UI using Avalonia UI

1. **Set up Avalonia project**
   ```bash
   dotnet new avalonia.mvvm -n ZWaveControllerAvalonia
   ```

2. **Port ViewModels** (mostly code reuse)
   - Copy `ZWaveControllerUI/Models/*.cs` → Avalonia project
   - Update namespace references
   - Test data binding

3. **Convert XAML views** (semi-automated)
   - Start with `MainWindow.xaml`
   - Convert WPF-specific controls to Avalonia equivalents
   - Priority order:
     1. MainWindow + MainMenuView
     2. NetworkManagementView (core functionality)
     3. CommandClassesView
     4. ConfigurationView
     5. Remaining views

4. **Replace dialogs**
   ```csharp
   // Avalonia equivalent
   var dialog = new OpenFolderDialog();
   var result = await dialog.ShowAsync(parentWindow);
   ```

5. **Implement custom controls**
   - NumericUpDown
   - ListViewEx
   - TopologyMapMatrixControl (using SkiaSharp)

**Success Criteria:**
- Main window renders correctly
- Core workflows functional (connect, manage nodes)
- Data binding works
- Commands execute properly

---

### Phase 3: Platform-Specific Features (2-3 weeks)

**Goal:** Implement Linux equivalents for Windows-only features

1. **WebCam capture replacement**
   ```bash
   dotnet add package Emgu.CV
   dotnet add package Emgu.CV.runtime.ubuntu-x64
   ```
   - Implement `IWebCamCaptureService` interface
   - Factory pattern for platform selection
   - Test QR code scanning

2. **Device enumeration enhancement**
   - Implement sysfs parsing
   - Extract VID/PID/description
   - Test with multiple USB devices

3. **Serial port monitoring (optional)**
   - FileSystemWatcher on `/dev/`
   - Filter for `ttyUSB*`, `ttyACM*`
   - Reconnect logic

**Success Criteria:**
- QR code scanning works on Linux
- Device list shows descriptive names
- USB hotplug detection (if implemented)

---

### Phase 4: Testing & Packaging (2-3 weeks)

**Goal:** Production-ready Linux application

1. **Integration testing**
   - Run full test suite on Linux
   - Test all major workflows
   - Performance profiling

2. **Packaging**
   ```bash
   # Self-contained deployment
   dotnet publish -c Release -r linux-x64 --self-contained true

   # Framework-dependent (requires .NET runtime)
   dotnet publish -c Release -r linux-x64 --self-contained false
   ```

3. **Distribution formats**
   - **AppImage** (portable, no installation)
   - **.deb package** (Ubuntu/Debian)
   - **.rpm package** (RHEL/Fedora)
   - **Flatpak** (sandboxed, cross-distro)
   - **Snap** (Canonical's format)

4. **Documentation**
   - Update README with Linux instructions
   - Document USB permissions setup
   - Provide udev rules example

**Example udev rule:**
```bash
# /etc/udev/rules.d/99-zwave-controller.rules
# Silicon Labs USB to UART
SUBSYSTEM=="tty", ATTRS{idVendor}=="10c4", ATTRS{idProduct}=="ea60", MODE="0666", GROUP="dialout"
```

---

### Phase 5: Maintenance & Optimization (Ongoing)

1. **CI/CD setup**
   - GitHub Actions for Linux builds
   - Automated testing on Ubuntu
   - Nightly builds

2. **Performance optimization**
   - Profile startup time
   - Optimize rendering (SkiaSharp)
   - Memory leak detection

3. **Feature parity**
   - Compare Windows vs Linux functionality
   - Address platform-specific bugs
   - User feedback integration

---

## Effort Estimation

### Summary

| Phase | Duration | Complexity | Risk |
|-------|----------|------------|------|
| Backend Preparation | 2-3 weeks | Low | Low |
| UI Migration | 4-6 weeks | High | Medium |
| Platform Features | 2-3 weeks | Medium | Medium |
| Testing & Packaging | 2-3 weeks | Medium | Low |
| **Total** | **10-15 weeks** | | |

### Team Composition

**Option A: Single Developer** (15 weeks full-time)
- Requires expertise in: .NET, WPF/Avalonia, Linux, Z-Wave protocol
- Timeline: 3-4 months

**Option B: Two Developers** (8-10 weeks)
- Developer 1: Backend + Serial communication
- Developer 2: UI migration + Platform features
- Timeline: 2-2.5 months

**Option C: Full Team** (6-8 weeks)
- Developer 1: Backend
- Developer 2: UI (views)
- Developer 3: UI (controls/styles)
- QA: Testing
- Timeline: 1.5-2 months

---

### Risk Assessment

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| z-wave-tools-core incompatibility | High | Low | Verify .NET Core support early |
| Avalonia XAML incompatibilities | Medium | Medium | Prototype critical views first |
| USB permissions on Linux | Low | Medium | Document setup, provide scripts |
| Performance issues | Medium | Low | Profile early, optimize rendering |
| Missing functionality in Avalonia | Medium | Low | Evaluate custom control needs upfront |

---

## Technical Recommendations

### 1. Target Framework

**Recommendation:** **.NET 8** (LTS until 2026)
- Cross-platform by default
- Improved performance vs .NET 6
- Long-term support

### 2. UI Framework

**Recommendation:** **Avalonia UI 11.x**
- Best Linux compatibility
- Active community
- Material Design support via Material.Avalonia
- XAML similarity to WPF

### 3. Graphics Library

**Recommendation:** **SkiaSharp**
- Used internally by Avalonia
- High performance
- Cross-platform
- Good .NET integration

### 4. Serial Communication

**Recommendation:** **System.IO.Ports** (.NET 8)
- Standard library, no dependencies
- Cross-platform support
- Adequate for Z-Wave communication (115200 baud)

### 5. WebCam Library

**Recommendation:** **Emgu.CV 4.x**
- Mature, stable
- Cross-platform
- Good documentation
- NuGet packages for Ubuntu

### 6. Build System

**Recommendation:** Keep **MSBuild/.csproj** format
- Compatible with .NET SDK
- Works on Linux via `dotnet` CLI
- Integrates with CI/CD (GitHub Actions, GitLab CI)

### 7. Testing

**Recommendations:**
- **xUnit** for unit testing (already used)
- **Verify** for snapshot testing (UI regression)
- Run tests on both Windows and Linux in CI

---

## Implementation Checklist

### Backend (ZWaveController)

- [ ] Migrate to .NET 8 (update `TargetFramework`)
- [ ] Build and test on Linux
- [ ] Verify z-wave-tools-core dependencies build on Linux
- [ ] Test serial port enumeration (`SerialPort.GetPortNames()`)
- [ ] Test TCP/ZIP discovery on Linux
- [ ] Stub out `WebCamCaptureService` for Linux
- [ ] Implement Linux serial device metadata (sysfs parsing)
- [ ] Conditional compilation for `SerialPortMonitor`
- [ ] Run unit tests on Linux
- [ ] Update NuGet packages (AutoMapper, Newtonsoft.Json)

### UI (ZWaveControllerUI → ZWaveControllerAvalonia)

- [ ] Create new Avalonia MVVM project
- [ ] Set up project structure (Views, ViewModels, Models)
- [ ] Copy ViewModels from WPF project (update namespaces)
- [ ] Convert `MainWindow.xaml` to Avalonia XAML
- [ ] Port Material Design styles to Material.Avalonia
- [ ] Convert value converters
- [ ] Implement custom controls:
  - [ ] NumericUpDown
  - [ ] ListViewEx
  - [ ] TopologyMapMatrixControl (SkiaSharp)
  - [ ] Others as needed
- [ ] Replace Windows Forms dialogs (FolderBrowserDialog, etc.)
- [ ] Port all 41 XAML views (priority-based)
- [ ] Replace `System.Drawing` with SkiaSharp
- [ ] Update ZXing.Net integration (remove WPF-specific parts)
- [ ] Test data binding
- [ ] Test commands
- [ ] Test navigation

### Platform Features

- [ ] Install Emgu.CV NuGet package
- [ ] Implement cross-platform `IWebCamCaptureService`
- [ ] Factory for platform-specific implementations
- [ ] Test QR code scanning on Linux
- [ ] Implement FileSystemWatcher for `/dev/` (optional)
- [ ] Test USB hotplug detection
- [ ] Document USB permissions setup
- [ ] Create udev rules template

### Testing

- [ ] Run unit tests on Linux
- [ ] Run integration tests on Linux
- [ ] Manual testing of all major workflows
- [ ] Performance profiling
- [ ] Memory leak detection
- [ ] Cross-platform compatibility testing

### Packaging

- [ ] Self-contained publish for Linux
- [ ] Create AppImage
- [ ] Create .deb package
- [ ] Create .rpm package (optional)
- [ ] Create Flatpak manifest
- [ ] Set up CI/CD (GitHub Actions)
- [ ] Automated builds for Linux

### Documentation

- [ ] Update README with Linux build instructions
- [ ] Document dependencies (z-wave-tools-core setup)
- [ ] Document USB permissions setup
- [ ] Create Linux installation guide
- [ ] Update architecture diagrams
- [ ] Document known limitations
- [ ] Create troubleshooting guide

---

## Appendix A: File Structure

### Current Project Structure
```
z-wave-pc-controller/
├── ZWaveControllerUI/         (WPF Application - Windows only)
│   ├── Views/                 (41 XAML views)
│   ├── Models/                (50+ ViewModels)
│   ├── Commands/
│   ├── Converters/
│   ├── Controls/              (Custom WPF controls)
│   └── ZWaveControllerUI.csproj
├── ZWaveController/           (Backend Library - Portable)
│   ├── Services/
│   ├── Models/
│   ├── Commands/              (100+ command classes)
│   ├── Interfaces/
│   └── ZWaveController.csproj / ZWaveController_netcore.csproj
├── Tests/
├── IntegrationTests/
└── ZWaveController.sln
```

### Proposed Linux Port Structure
```
z-wave-pc-controller/
├── ZWaveControllerAvalonia/   (NEW - Cross-platform UI)
│   ├── Views/
│   ├── ViewModels/            (Reused from WPF with minimal changes)
│   ├── Controls/              (Avalonia equivalents)
│   ├── Assets/
│   └── ZWaveControllerAvalonia.csproj
├── ZWaveController/           (Backend - update to .NET 8)
│   └── ... (same structure, updated)
├── ZWaveController.Platform/  (NEW - Platform-specific code)
│   ├── Linux/
│   │   ├── SerialPortEnumerator.cs
│   │   ├── WebCamCaptureService.cs
│   │   └── UsbMonitor.cs
│   └── Windows/
│       └── ... (existing Windows code)
├── Tests/
├── IntegrationTests/
└── ZWaveController.sln
```

---

## Appendix B: Key APIs to Replace

### WPF → Avalonia Mapping

| WPF API | Avalonia Equivalent | Notes |
|---------|---------------------|-------|
| `Window` | `Window` | Same |
| `UserControl` | `UserControl` | Same |
| `Button` | `Button` | Same |
| `TextBox` | `TextBox` | Same |
| `ListView` | `ListBox` or `DataGrid` | Different styling |
| `TreeView` | `TreeView` | Same |
| `ComboBox` | `ComboBox` | Same |
| `Dispatcher.Invoke()` | `Dispatcher.UIThread.Post()` | Different API |
| `DependencyProperty` | `StyledProperty` | Different registration |
| `Binding` | `Binding` | Same syntax |
| `INotifyPropertyChanged` | `INotifyPropertyChanged` | Same |
| `RoutedEvent` | `RoutedEvent` | Similar |
| `Style` | `Style` | Same |
| `DataTemplate` | `DataTemplate` | Same |
| `FolderBrowserDialog` | `OpenFolderDialog` | Async API |
| `OpenFileDialog` | `OpenFileDialog` | Async API |
| `SaveFileDialog` | `SaveFileDialog` | Async API |

---

## Appendix C: External References

### Documentation
- **.NET on Linux:** https://learn.microsoft.com/en-us/dotnet/core/install/linux
- **Avalonia UI:** https://docs.avaloniaui.net/
- **Emgu.CV:** http://www.emgu.com/wiki/index.php/Main_Page
- **System.IO.Ports:** https://learn.microsoft.com/en-us/dotnet/api/system.io.ports
- **Z-Wave Alliance:** https://z-wavealliance.org/

### Tools
- **.NET SDK:** https://dotnet.microsoft.com/download
- **Visual Studio Code:** https://code.visualstudio.com/
- **Rider (JetBrains):** https://www.jetbrains.com/rider/

### Libraries
- **Avalonia UI:** https://github.com/AvaloniaUI/Avalonia
- **Material.Avalonia:** https://github.com/AvaloniaCommunity/Material.Avalonia
- **Emgu.CV:** https://github.com/emgucv/emgucv
- **SkiaSharp:** https://github.com/mono/SkiaSharp

---

## Conclusion

The Z-Wave PC Controller can be successfully ported to Linux with a moderate development effort of **10-15 weeks** (2.5-4 months). The primary challenges are:

1. **UI Framework Migration** (largest effort): WPF → Avalonia UI
2. **Native DLL Replacement** (medium effort): WebCam capture using Emgu.CV
3. **Platform-Specific Code** (low effort): Serial port enumeration and monitoring

The backend library (`ZWaveController`) is largely portable, with .NET Core project variants already available. The z-wave-tools-core dependencies should be verified for Linux compatibility, but are expected to work with minimal changes.

**Key Success Factors:**
- Use **.NET 8** for long-term support
- Choose **Avalonia UI** for best Linux compatibility
- Leverage existing MVVM ViewModels (70-80% reuse)
- Replace native DLLs with cross-platform alternatives (Emgu.CV)
- Implement platform-specific code behind abstractions

With proper planning and execution, the Linux port can achieve full feature parity with the Windows version while maintaining a shared codebase for future development.

---

**Document Status:** ✅ Complete
**Last Updated:** 2025-11-08
**Next Review:** After Phase 1 completion
