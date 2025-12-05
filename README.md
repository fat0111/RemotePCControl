# 🖥️ Remote PC Control - HCMUS Socket Programming Project

## 📋 Mô tả dự án

Ứng dụng điều khiển máy tính từ xa qua mạng LAN sử dụng Socket Programming với C\#. Hỗ trợ streaming webcam real-time với tốc độ 15 FPS.

### ✨ Tính năng chính

1.  **📱 Quản lý Applications** - List/Start/Stop ứng dụng
2.  **⚙️ Quản lý Processes** - List/Start/Stop tiến trình trong Task Manager
3.  **📸 Screenshot** - Chụp màn hình từ xa
4.  **⌨️ Keylogger** - Ghi lại phím bấm (cho mục đích học tập)
5.  **📹 Webcam Streaming** - Live video streaming real-time 15 FPS + Capture snapshots
6.  **🔋 System Control** - Shutdown/Restart máy tính

-----

## 🏗️ Kiến trúc hệ thống

```plaintext
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│  Web Interface  │◄───────►│   Server (C#)   │◄───────►│ ClientControlled│
│ (ASP.NET Core)  │         │   Console App   │         │  (WinForms C#)  │
│  + SignalR      │         │   Port 8888     │         │  + Webcam       │
└─────────────────┘         └─────────────────┘         └─────────────────┘
      Browser                 Forwarding Server            Controlled PC
```

### Protocol giao tiếp

**Length-Prefix Protocol:**

```plaintext
[4 bytes: length] + [N bytes: UTF-8 message]
```

**Message formats:**

```plaintext
REGISTER_CONTROLLED|<IP>|<PASSWORD>
LOGIN|<IP>|<PASSWORD>
COMMAND|<TARGET_IP>|<COMMAND_NAME>|<PARAMS>
RESPONSE|<SOURCE_IP>|<RESPONSE_TYPE>|<DATA>
```

-----

## 📦 Cài đặt

### Yêu cầu hệ thống

  * **OS:** Windows 10/11
  * **dotNET:** .NET 8.0 SDK
  * **IDE:** Visual Studio 2022 (hoặc VS Code)
  * **Webcam:** Camera thật (không phải camera ảo)

### Bước 1: Clone hoặc tải project

```bash
git clone <repository-url>
cd RemotePCControl
```

### Bước 2: Cài đặt dependencies

**ClientControlled project:**

```bash
cd ClientControlled
dotnet add package AForge.Video --version 2.2.5
dotnet add package AForge.Video.DirectShow --version 2.2.5
```

### Bước 3: Sử dụng BuildProject

Để build, public và tạo các shortcut để chạy project:

```bash
# Build
cd BuildProject
dotnet run
```

### Bước 3a: Build nhanh cho máy bị điều khiển

```bash
cd BuildProjectClient
dotnet run
```

> **Lưu ý:**
>
>   * Không thiết lập biến môi trường server.
>   * Tự lấy `server-info.json` (được sinh khi chạy BuildProject trên máy server) để ghi sẵn `clientsettings.json` và đóng gói file `client-controlled.zip` cho Web tải về.

-----

## 🚀 Chạy ứng dụng

### Step 1: Khởi động Server (bắt buộc)

```bash
cd Server
dotnet run
```

*Hoặc chạy file `Server.exe` trong `bin/Debug/net8.0/`*
*Hoặc chạy shortcut được tạo ra ở `Desktop` hoặc trong folder `Shortcuts`*

**Kết quả:**

```plaintext
╔══════════════════════════════════════════════════════════╗
║          REMOTE PC CONTROL SERVER v2.0                   ║
║          HCMUS - Socket Programming Project              ║
║          ✓ Webcam Streaming Support                      ║
╚══════════════════════════════════════════════════════════╝

[SERVER] Started on port 8888
[SERVER] Waiting for connections...
```

### Step 2: Khởi động ClientControlled (PC bị điều khiển)

```bash
cd ClientControlled
dotnet run
```

*Hoặc chạy file `ClientControlled.exe`*
*Hoặc chạy shortcut được tạo ra ở `Desktop` hoặc trong folder `Shortcuts`*

**Kết quả:**

```plaintext
[CLIENT] Connected to server
[INFO] IP: 192.168.1.100
[INFO] Password: 834521
```

> **📝 Ghi chú lại IP và Password này để đăng nhập từ Web\!**

### Step 3: Khởi động Web Interface

```bash
cd WebInterface
dotnet run
```

*Hoặc chạy shortcut được tạo ra ở `Desktop` hoặc trong folder `Shortcuts`*

**Kết quả:**

```plaintext
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
      Now listening on: http://localhost:5000
```

### Step 4: Truy cập Web và điều khiển

1.  Mở browser: `https://localhost:7001` (ip này có hiển thị trên cửa sổ console của WebInterface)
2.  Chọn **"Người điều khiển"**
3.  Nhập **IP** và **Password** từ ClientControlled
4.  Click **"Kết nối"**
5.  Sử dụng các tính năng\!

-----

## 🎯 Hướng dẫn sử dụng Webcam Streaming

### Bật webcam và streaming

1.  Vào tab **📹 Webcam**
2.  Click **"Bật Webcam"** → Camera sẽ tự động chọn camera tốt nhất
3.  Click **"Live Stream"** → Video hiển thị real-time với FPS counter
4.  Click **"Chụp ảnh"** khi đang stream → Ảnh xuất hiện bên dưới
5.  Click **"Dừng Stream"** → Video tạm dừng, camera vẫn bật
6.  Click **"Tắt Webcam"** → Tắt hoàn toàn camera

### Tính năng Smart Camera Selection

Camera sẽ được chấm điểm và chọn theo thứ tự:

  * ✅ Bỏ qua camera ảo: OBS, Snap Camera, DroidCam, ManyCam...
  * ✅ Bỏ qua camera IR/Windows Hello
  * ✅ Ưu tiên camera có resolution cao (FHD/HD)
  * ✅ Ưu tiên frame rate cao (60fps/30fps)
  * ✅ Ưu tiên thương hiệu uy tín: Logitech, Microsoft, HP...

**Console logs:**

```plaintext
[WEBCAM] Scanning: Integrated Camera
[WEBCAM] -> Score: 50
[WEBCAM] ✓ SELECTED: Integrated Camera (Score: 50)
[WEBCAM] Resolution: 1280x720 @ 30fps
[WEBCAM] Streaming started
[WEBCAM] Streaming: 75 frames sent, 15.2 fps
```

-----

## ⚙️ Tùy chỉnh hiệu suất

### Điều chỉnh Frame Rate

**File:** `ClientControlled/ClientControlled.cs`

```csharp
private void StreamWebcamFrames()
{
    // ...
    Thread.Sleep(66);   // 15 FPS (mặc định)
    // Thread.Sleep(33);   // 30 FPS (mượt hơn, nặng hơn)
    // Thread.Sleep(100);  // 10 FPS (nhẹ hơn)
}
```

### Điều chỉnh chất lượng JPEG

```csharp
private string GetWebcamFrame()
{
    // ...
    encoderParams.Param[0] = new EncoderParameter(
        Encoder.Quality, 60L);  // 60% (mặc định)
        // 80L);  // Rõ hơn, nặng hơn ~30%
        // 40L);  // Mờ hơn, nhẹ hơn ~30%
}
```

### Bảng hiệu suất

| Config | Bandwidth | CPU (Client) | FPS | Quality |
| :--- | :--- | :--- | :--- | :--- |
| **15fps, Q60, 720p** | 600 KB/s | 8% | 15 | Good ⭐⭐⭐⭐ |
| **15fps, Q60, 1080p** | 900 KB/s | 12% | 15 | Great ⭐⭐⭐⭐⭐ |
| **10fps, Q40, 720p** | 200 KB/s | 5% | 10 | OK ⭐⭐⭐ |
| **30fps, Q80, 1080p** | 3 MB/s | 18% | 30 | Excellent ⭐⭐⭐⭐⭐ |

> **💡 Khuyến nghị:** 15fps, Q60, 720p (cân bằng tốt nhất cho LAN)

-----

## 🐛 Xử lý lỗi thường gặp

### ❌ Lỗi: "No webcam found"

**Nguyên nhân:** Không phát hiện camera
**Giải pháp:**

1.  Kiểm tra Device Manager → Camera có hoạt động?
2.  Cài lại driver camera
3.  Restart máy

### ❌ Lỗi: "No suitable camera found"

**Nguyên nhân:** Tất cả camera bị blacklist
**Giải pháp:**

1.  Xem console logs → Tìm tên camera thực của bạn
2.  Mở `ClientControlled.cs` → Tìm hàm `FindBestCamera()`
3.  Xóa tên camera khỏi `blacklist` array

### ❌ Lỗi: "Camera in use by another application"

**Giải pháp:**

1.  Đóng tất cả app khác đang dùng camera (Zoom, Teams, Skype...)
2.  Mở Task Manager → Tìm process đang giữ camera
3.  Restart ClientControlled app

### ❌ Lỗi: Stream lag/chậm

**Giải pháp:**

  * Giảm FPS xuống 10 (`Thread.Sleep(100)`)
  * Giảm quality xuống 40 (`Encoder.Quality, 40L`)
  * Giảm resolution bằng cách giới hạn trong `FindBestCamera()`

### ❌ Lỗi: "Cannot connect to server"

**Giải pháp:**

1.  Kiểm tra Server có đang chạy không?
2.  Kiểm tra Windows Firewall → Allow port 8888
3.  Kiểm tra IP address đúng không (127.0.0.1 cho localhost)

-----

## 📊 Testing Checklist

### ✅ Server

  - [ ] Server khởi động thành công
  - [ ] Console hiển thị "Started on port 8888"
  - [ ] Accept được connections

### ✅ ClientControlled

  - [ ] Kết nối được Server
  - [ ] Hiển thị IP và Password
  - [ ] Camera được detect và chọn đúng

### ✅ Web Interface

  - [ ] Đăng nhập thành công
  - [ ] Tất cả 6 tabs hoạt động
  - [ ] List Apps/Processes thành công
  - [ ] Screenshot hoạt động
  - [ ] Keylogger hoạt động
  - [ ] Webcam streaming hoạt động
  - [ ] FPS counter hiển thị
  - [ ] Capture snapshot hoạt động
  - [ ] System commands hoạt động

### ✅ Webcam Streaming

  - [ ] Camera khởi động thành công
  - [ ] Video stream mượt mà
  - [ ] FPS \~15 (±2)
  - [ ] LIVE indicator hoạt động
  - [ ] Capture snapshot thành công
  - [ ] Snapshots hiển thị dưới video
  - [ ] Stop/Start stream hoạt động
  - [ ] Tắt webcam cleanup đúng

-----

## 📁 Cấu trúc Project

```plaintext
RemotePCControl/
├── Server/
│   ├── Server.cs                 # Main server forwarding logic
│   └── Server.csproj
│
├── ClientControlled/
│   ├── ClientControlled.cs       # Main client service với webcam
│   ├── Form1.cs                  # WinForms UI
│   ├── Program.cs
│   └── ClientControlled.csproj
│
└── WebInterface/
    ├── wwwroot/
    │   └── index.html            # Web UI với streaming support
    ├── Hubs/
    │   └── ControlHub.cs         # SignalR Hub
    ├── Services/
    │   └── ConnectionService.cs  # Service kết nối Server
    ├── Program.cs
    └── WebInterface.csproj
```

-----

## 🔒 Lưu ý bảo mật

**⚠️ QUAN TRỌNG:** Đây là project học tập về Socket Programming.

  * ✅ Chỉ sử dụng trong môi trường LAN an toàn
  * ✅ Không expose ra Internet
  * ✅ Keylogger chỉ cho mục đích demo
  * ✅ Không sử dụng cho mục đích xấu

-----

## 📝 License

Project này được tạo cho mục đích học tập tại HCMUS.

-----

## 👥 Contributors

**1. Họ tên: Huỳnh Tuấn Kiệt**

  * **MSSV:** 24120356
  * **Lớp:** 24CTT5
  * **Môn:** Mạng máy tính
  * **Giảng viên:** Đỗ Hoàng Cường

**2. Họ tên: Võ Nhật Liệu**

  * **MSSV:** 24120368
  * **Lớp:** 24CTT5
  * **Môn:** Mạng máy tính
  * **Giảng viên:** Đỗ Hoàng Cường

**3.  Họ tên: Đinh Tiến Phát**

  * **MSSV:** 24120405
  * **Lớp:** 24CTT5
  * **Môn:** Mạng máy tính
  * **Giảng viên:** Đỗ Hoàng Cường

-----

## 📞 Support

Nếu gặp vấn đề, hãy:

1.  Xem phần **Xử lý lỗi** ở trên
2.  Check console logs (F12 trên browser)
3.  Check Server console và ClientControlled console
4.  Liên hệ 24120356@student.hcmus.edu.vn để được tư vấn giải quyết

-----

## 🎉 Demo Video

[Link video demo]

<br>

-----

# Giải thích các mô hình cơ bản trong đồ án

## 🖥️ Server

### Tổng quan

Server đóng vai trò là trung gian (Relay Server), quản lý kết nối giữa Web Interface và các máy ClientControlled. Được phát triển bằng C\# .NET 8.0, hỗ trợ đa luồng và xử lý đồng thời nhiều kết nối.

### 🛠 Công nghệ chính

  * **.NET 8.0**: Nền tảng chính
  * **TCP/IP Socket**: Giao tiếp mạng
  * **Đa luồng (Multithreading)**: Xử lý đa nhiệm
  * **JSON**: Định dạng dữ liệu trao đổi

### 📊 Các hàm chính

| Tên Hàm | Thông số | Mô tả chi tiết |
| :--- | :--- | :--- |
| **`Start`** | `int port` | - Khởi tạo `TcpListener`<br>- Bắt đầu lắng nghe kết nối<br>- Khởi động các luồng xử lý |
| **`AcceptClients`** | - | - Chấp nhận kết nối mới<br>- Tạo luồng xử lý riêng cho mỗi client |
| **`HandleClient`** | `TcpClient tcpClient` | - Xử lý giao tiếp với từng client<br>- Đọc gói tin theo giao thức Length-Prefix |
| **`ProcessMessage`** | `client`, `message`, `stream` | - Phân tích và định tuyến tin nhắn<br>- Xử lý đăng ký, đăng nhập, điều hướng lệnh: <br> + `REGISTER_CONTROLLED`: Đăng ký máy bị điều khiển vào Dictionary `sessions`.<br> + `LOGIN`: Xác thực máy điều khiển kết nối vào phiên.<br> + `COMMAND`: Chuyển tiếp lệnh (Shutdown, Keylog...) sang máy đích.<br> + `RESPONSE/WEBCAM`: Chuyển tiếp dữ liệu/hình ảnh về máy điều khiển. |
| **`ForwardCommand`** | `targetClient`, `command` | - Đóng gói lệnh theo giao thức **Length-Prefix**.<br>- Gửi lệnh điều khiển đến máy bị điều khiển thông qua `NetworkStream`. |
| **`DisplayStats`** | *Không* | - Luồng giám sát chạy mỗi 5 giây.<br>- Hiển thị bảng thống kê FPS và lưu lượng truyền tải của Webcam Streaming lên màn hình Console Server. |

### Các cấu trúc dữ liệu quản lý (State Management)

Để quản lý trạng thái của nhiều máy cùng lúc, Server sử dụng các cấu trúc dữ liệu sau:

1.  **`List<ConnectedClient> clients`**:

      * Lưu trữ danh sách tất cả các kết nối TCP đang active.
      * Dùng để quản lý vòng đời kết nối (đóng/mở).

2.  **`Dictionary<string, ClientSession> sessions`**:

      * **Key:** Địa chỉ IP của máy bị điều khiển.
      * **Value:** Object `ClientSession` chứa thông tin cặp đôi (Controller - Controlled) và Password.
      * *Công dụng:* Giúp Server biết phải chuyển gói tin từ máy nào sang máy nào.

3.  **`Dictionary<string, StreamStats> streamStats`**:

      * Lưu trữ trạng thái Streaming (FPS, Frame count, Start Time).
      * Dùng để tính toán và hiển thị hiệu suất truyền hình ảnh thời gian thực.

-----

## 💻 ClientControlled (Máy bị điều khiển)

### 🎯 Tổng quan

Ứng dụng chạy trên máy tính cần được điều khiển từ xa, kết nối tới Server trung tâm và thực thi các lệnh nhận được. Được phát triển bằng C\# WinForms với .NET 8.0.

### ✨ Tính năng chính

  * **Điều khiển từ xa** thông qua lệnh từ Server
  * **Quản lý ứng dụng & tiến trình**
  * **Chụp màn hình** và gửi về Server
  * **Quay phim màn hình** thời gian thực
  * **Điều khiển hệ thống** (tắt/mở lại máy, v.v.)
  * **Keylogger** (cho mục đích học tập)
  * **Hỗ trợ đa màn hình**

### 🛠 Công nghệ sử dụng

| Công nghệ | Mục đích sử dụng |
| :--- | :--- |
| **.NET 8.0** | Nền tảng chính |
| **Windows Forms** | Giao diện người dùng |
| **System.Net.Sockets** | Kết nối mạng |
| **System.Drawing** | Xử lý hình ảnh |
| **AForge.Video** | Xử lý webcam |
| **user32.dll** | Tương tác hệ thống |

### 📋 Các lệnh hỗ trợ

| Lệnh | Mô tả |
| :--- | :--- |
| `LIST_APPS` | Liệt kê ứng dụng đang chạy |
| `LIST_PROCESSES` | Liệt kê tiến trình hệ thống |
| `START_APP` | Khởi chạy ứng dụng |
| `STOP_PROCESS` | Dừng tiến trình |
| `SCREENSHOT` | Chụp màn hình |
| `WEBCAM_START` | Bật webcam |
| `WEBCAM_STOP` | Tắt webcam |
| `SHUTDOWN` | Tắt máy |
| `RESTART` | Khởi động lại máy |
| `KEYLOG` | Xem log bàn phím |

### 🚀 Cách sử dụng

1.  **Cấu hình**

      * Chỉnh sửa file `clientsettings.json`
      * Đặt địa chỉ IP của Server

    <!-- end list -->

    ```json
    {
      "ServerIP": "127.0.0.1",
      "Port": 8888
    }
    ```

2.  **Khởi động**

      * Chạy file `ClientControlled.exe`
      * Ứng dụng sẽ hiển thị mật khẩu ngẫu nhiên
      * Cung cấp mật khẩu này cho người điều khiển

3.  **Kiểm tra kết nối**

      * Giao diện hiển thị trạng thái kết nối
      * Thông báo khi có kết nối mới

### ⚠️ Lưu ý bảo mật

  * Chỉ chạy ứng dụng khi thực sự cần thiết
  * Không chia sẻ mật khẩu với người không tin cậy
  * Tắt ứng dụng khi không sử dụng
  * Cập nhật phiên bản mới nhất để đảm bảo an toàn

-----

## 🌐 WebInterface

### 🎯 Tổng quan

Giao diện web cho phép điều khiển máy tính từ xa thông qua trình duyệt. Được phát triển bằng ASP.NET Core với SignalR để hỗ trợ real-time.

### ✨ Tính năng chính

  * **Điều khiển từ xa** qua giao diện web
  * **Xem màn hình** máy điều khiển
  * **Điều khiển chuột và bàn phím**
  * **Quản lý file** từ xa
  * **Chat trực tuyến** với người dùng
  * **Hỗ trợ đa nền tảng**

### 🛠 Công nghệ sử dụng

| Công nghệ | Mục đích sử dụng |
| :--- | :--- |
| **ASP.NET Core** | Backend server |
| **SignalR** | Giao tiếp real-time |
| **HTML5/CSS3** | Giao diện người dùng |
| **JavaScript** | Xử lý phía client |
| **Bootstrap** | Thiết kế responsive |

### 📋 Hướng dẫn cài đặt

1.  **Yêu cầu hệ thống**

      * .NET 8.0 SDK
      * Node.js (cho frontend)
      * Trình duyệt web hiện đại

2.  **Cài đặt**

    ```bash
    cd WebInterface
    dotnet restore
    cd ClientApp
    npm install
    ```

3.  **Cấu hình**
    Chỉnh sửa file `appsettings.json`:

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=RemotePC;Trusted_Connection=True;"
      },
      "Jwt": {
        "Key": "YOUR_SECRET_KEY",
        "Issuer": "https://localhost:5001"
      }
    }
    ```

4.  **Chạy ứng dụng**

    ```bash
    dotnet run
    ```

    Truy cập: `https://localhost:5001`

### 🚀 Các tính năng chính

#### 1\. Đăng nhập & Bảo mật

  * Xác thực người dùng
  * Phân quyền truy cập
  * Mã hóa dữ liệu đầu cuối

#### 2\. Dashboard

  * Tổng quan hệ thống
  * Danh sách máy đang kết nối
  * Thống kê hoạt động

#### 3\. Điều khiển từ xa

  * Xem màn hình thời gian thực
  * Điều khiển chuột và bàn phím
  * Chia sẻ file
  * Trò chuyện trực tuyến

### 🔧 API Endpoints

| Method | Endpoint | Mô tả |
| :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Đăng nhập |
| `POST` | `/api/auth/register` | Đăng ký |
| `GET` | `/api/machines` | Danh sách máy |
| `POST` | `/api/connect` | Kết nối tới máy |
| `POST` | `/api/command` | Gửi lệnh điều khiển |
| `GET` | `/api/chat` | Kết nối chat |

### 📱 Hỗ trợ đa nền tảng

  * **Web**: Chrome, Firefox, Edge, Safari
  * **Di động**: Hỗ trợ responsive cho điện thoại và máy tính bảng
  * **Hệ điều hành**: Windows, macOS, Linux, Android, iOS

-----

Đây là tính năng nổi bật giúp tự động chọn camera tốt nhất trên thiết bị.

1.  **Thuật toán chọn Camera (`FindBestCamera`)**:

      * **Blacklist:** Tự động loại bỏ các camera ảo, camera hồng ngoại (IR), hoặc phần mềm bên thứ 3 (OBS, DroidCam, ManyCam...) dựa trên tên thiết bị.
      * **Whitelist:** Cộng điểm ưu tiên cho các từ khóa uy tín (Logitech, Microsoft, HD, FHD, Built-in).
      * **Scoring System (Hệ thống tính điểm):**
          * Resolution $\ge$ 1080p: +50 điểm.
          * Resolution $\ge$ 720p: +30 điểm.
          * FPS $\ge$ 60: +20 điểm.
          * FPS $\ge$ 30: +10 điểm.
      * \-\> **Kết quả:** Chọn camera có điểm số cao nhất để stream.

2.  **Streaming (`StreamWebcamFrames`)**:

      * Chạy trên luồng riêng (Background Thread).
      * Capture khung hình -\> Nén JPEG với chất lượng 60% (để tối ưu băng thông mạng LAN).
      * Gửi dữ liệu dạng `WEBCAM_FRAME|Base64...`.
      * Sử dụng `Thread.Sleep(66)` để duy trì tốc độ khoảng **15 FPS**, cân bằng giữa độ mượt và hiệu năng CPU.

### Cấu hình & Keylogger

  * **Keylogger (`KeyLogger` class):**

      * Sử dụng vòng lặp vô tận kiểm tra trạng thái 255 phím ảo mỗi 10ms.
      * Dùng hàm API Windows `GetAsyncKeyState` để phát hiện phím nhấn ngay cả khi ứng dụng không focus.
      * Lưu log vào `StringBuilder` trong bộ nhớ.

  * **Cấu hình (`ClientSettings` class):**

      * Tự động tải file `clientsettings.json`.
      * Hỗ trợ Override bằng biến môi trường `REMOTEPC_SERVER_IP` (hữu ích khi deploy số lượng lớn hoặc chạy qua script).