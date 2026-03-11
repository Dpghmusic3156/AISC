# DECISIONS.md

## Log of Architectural Decisions

### 1. Technology Stack Selection
- **Context**: Needed a Windows app that runs efficiently in the background, supports global hotkeys, and can overlay a region selection UI.
- **Options Considered**: Electron, Tauri, C# WPF, Python (PyQt).
- **Decision**: C# WPF (.NET 8+)
- **Rationale**: Best integration with Windows APIs (System Tray, Global Hotkeys, Layered/Transparent Windows). Low background memory footprint compared to web-based alternatives.
- **Date**: 2026-03-11

## Phase 1 Decisions

**Date:** 2026-03-11

### Scope
- Cấu hình UI: Sử dụng \MaterialDesignInXAML\ cho ứng dụng WPF để giao diện hiện đại, sạch sẽ.
- Cơ chế lưu trữ: Sử dụng file \config.json\ để lưu trữ API keys và settings (phù hợp nhu cầu cá nhân).
- Preset mặc định: Tạo sẵn preset có tên "Giải thích nội dung trong ảnh này" sử dụng AI provider Google Gemini với model \gemini-3-flash\.

### Approach
- Chose: WPF with MaterialDesignInXAML, JSON serialization for settings.
- Reason: User preferred modern UI via MaterialDesignInXAML and simpler configuration storage via JSON.

## Phase 2 Decisions

**Date:** 2026-03-11

### Scope
- Phím tắt mặc định: **Ctrl + Middle Mouse Button**
- Multi-monitor: Tạm thời chưa cần hỗ trợ
- Sau khi capture: Thêm toggle trong Settings để chọn "gửi thẳng AI" hoặc "preview trước"

### Approach
- **Global Hotkey**: Win32 P/Invoke — nhưng vì Ctrl+Middle Mouse cần kết hợp keyboard + mouse hook (RegisterHotKey chỉ hỗ trợ keyboard), sẽ dùng low-level hooks (WH_KEYBOARD_LL + WH_MOUSE_LL)
- **Screen Region Selection**: Fullscreen transparent WPF overlay — screenshot làm background, user vẽ rectangle
- **DPI**: Xử lý DPI-aware cho capture chính xác
- **UAC**: Tự xử lý (try/catch, graceful fallback)

### Constraints
- Ctrl + Middle Mouse yêu cầu low-level mouse hook thay vì RegisterHotKey (chỉ hỗ trợ keyboard keys)

## Phase 3 Decisions

**Date:** 2026-03-11

### Scope
- Preset selection: "Active Preset" — user chọn 1 preset mặc định trong Settings, capture tự dùng
- Kết quả AI: Tạm show MessageBox (Phase 4 sẽ lo Result Popup)
- Custom baseUrl: Hỗ trợ để dùng OpenAI-compatible APIs

### Approach
- **API Clients**: HttpClient trực tiếp (REST API calls), không dùng SDK — hỗ trợ custom baseUrl
- **Rate Limiting**: Xử lý 429 errors (retry with backoff)
- **Timeout**: 60 giây
- **API Key Security**: Plain text trong config.json — không cần encrypt


