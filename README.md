# 📸 AI Screen Capture (AISC)

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Desktop-blue?style=for-the-badge&logo=windows)
![OpenAI](https://img.shields.io/badge/ChatGPT-74aa9c?style=for-the-badge&logo=openai&logoColor=white)
![Gemini](https://img.shields.io/badge/Gemini-8E75B2?style=for-the-badge&logo=googlebard&logoColor=white)
![Claude](https://img.shields.io/badge/Claude-D97757?style=for-the-badge&logo=anthropic&logoColor=white)

**AI Screen Capture** là một tiện ích nhỏ gọn trên Windows giúp bạn chụp màn hình và chuyển ngay hình ảnh đó cho AI (ChatGPT, Claude, Gemini) để phân tích, giải toán, dịch thuật, hoặc giải thích code trực tiếp trên màn hình của bạn. 

👉 Mọi thứ hiển thị qua một popup tối giản giống hệt trải nghiệm của các Web Extension hiện đại (như *schoolCheat-AI*).

---

## ✨ Tính năng nổi bật

- 🎯 **Chụp Xong Hỏi Luôn (Zero-click prompt):** Chỉ cần bôi đen vùng màn hình, app sẽ tự động gửi ảnh cho AI theo cấu hình có sẵn của bạn. Không cần gõ lại prompt.
- ⚡ **Hệ thống Hotkey Kép:**
  - **Chế độ 1 (Chuột):** Bấm `Ctrl + Chuột Giữa` để chụp, bấm `Chuột Giữa` để ẩn/hiện popup.
  - **Chế độ 2 (Bàn phím):** Tự do tuỳ chỉnh (Vd: `Ctrl + Shift + C`) không lo trùng lặp.
- 🤖 **Đa nền tảng AI:** Hỗ trợ nhập API trực tiếp cho **OpenAI (GPT-4o, GPT-4-vision)**, **Anthropic (Claude 3.5 Sonnet)**, và **Google Gemini (Gemini 1.5 Pro)**.
- 🎨 **Giao diện Minimalist:** Cửa sổ kết quả trong suốt, gọn gàng, có thể kéo thả bất cứ đâu. Đi kèm hiệu ứng loading "Dấu tick xanh nhấp nháy" cực kỳ mượt mà.
- ⚙️ **Tuỳ biến cao:** Hỗ trợ tuỳ chỉnh độ làm mờ màn hình (Dimness), phím tắt, kích cỡ cửa sổ, cỡ chữ, theme (Sáng/Tối).

---

## 🚀 Hướng dẫn Cài đặt & Sử dụng

### 1. Yêu cầu hệ thống
- Hệ điều hành: Windows 10 / Windows 11
- Môi trường: [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### 2. Cách chạy ứng dụng
1. Tải toàn bộ Source Code về hoặc dùng lệnh:
   ```bash
   git clone https://github.com/dpghmusic3156/AISC.git
   ```
2. Mở thư mục `src` bằng **Visual Studio 2022** hoặc **JetBrains Rider**.
3. Bấm **Run** dự án `AIScreenCapture.UI`.
4. Khi chạy thành công, app sẽ nằm ẩn dưới **Khay hệ thống (System Tray)** (góc phải dưới màn hình).

### 3. Cách thiết lập AI (Lần đầu tiên)
- **Bước 1:** Chuột phải vào Icon App dưới System Tray -> Chọn **Settings**.
- **Bước 2 (API Keys):** Nhập API Key của nền tảng bạn muốn dùng (OpenAI / Claude / Gemini).
- **Bước 3 (Presets):** Chuyển sang Tab **Presets** -> Bấm **Add Preset**.
  - **Name:** Tên gợi nhớ (Vd: *Giải Toán*, *Giải thích Code*).
  - **Provider:** Chọn nền tảng (OpenAI/Claude/Gemini).
  - **Model:** Chọn Model (bấm nút 🔄 để tải danh sách Model mới nhất từ API).
  - **System Prompt:** Nhập câu lệnh mặc định (Vd: *"Hãy giải thích đoạn code trong ảnh này một cách ngắn gọn"*).
- **Bước 4 (Shortcuts):** Chọn cách bạn muốn gọi App (Bằng Chuột hay Bàn Phím).
- **Bước 5:** Bấm **Save & Close**.

---

## 🎮 Thao tác nhanh
1. Nhấn phím tắt bạn đã cài (Mặc định là `Ctrl + Chuột Giữa`).
2. Màn hình sẽ tối đi -> Kéo chuột tạo vùng chữ nhật quét qua nội dung cần hỏi.
3. Đợi biểu tượng tick xanh nhấp nháy load.
4. Kết quả sẽ tự động bật lên ở dạng ô cửa sổ tối giản.
5. Xem xong? Bấm phím tắt (*Mặc định: Chuột Giữa*) để ẩn cửa sổ đi.

---

## 🛠 Công nghệ sử dụng
- **C# / WPF** (Thư viện UI Windows).
- Khung UI Material Design: **MaterialDesignInXamlToolkit**.
- Quản lý Khay hệ thống: **Hardcodet.NotifyIcon.Wpf**.
- Quản lý phím tắt: **NHotkey.Wpf** và **Win32 Hooks**.
- Giao tiếp AI API HTTP Requests mượt mà kết hợp Async/Await.

---
*Made with ❤️ by ghuy.*
