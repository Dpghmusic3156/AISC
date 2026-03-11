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
