# RESEARCH.md — Architecture & Technology Options

## Objective
Determine the best technology stack to build a Windows-only desktop application that:
1. Runs in the system tray.
2. Registers global hotkeys.
3. Provides a screen region selection UI (transparent overlay).
4. Provides a main configuration window (UI).
5. Displays a draggable, lightweight popup for AI responses.
6. Makes HTTP requests to AI providers (OpenAI, Gemini, Claude).

## Option 1: Electron (JavaScript/TypeScript + HTML/CSS)
**Pros:**
- Very easy to build beautiful, modern UIs (React, Vue, or Vanilla CSS).
- Huge ecosystem of NPM packages for API integrations, global shortcuts (`globalShortcut`), and tray management.
- Transparent windows and frameless framing are well supported.

**Cons:**
- High memory usage (Chromium + Node.js overhead) especially for a background app.
- Large bundle size.

## Option 2: Tauri (Rust + Web Frontend)
**Pros:**
- Very low memory usage and small binary size compared to Electron.
- Can still use familiar web technologies (React, Vue, solid) for the UI.
- Native APIs for tray, global shortcuts, and windows are available.

**Cons:**
- Requires writing logic in Rust (steeper learning curve for some).
- Screen capture specifically might require native Rust crates (like `xcap` or `scrap`) which can be complex to integrate cross-platform, though Windows support is usually decent.

## Option 3: C# / .NET (WPF or WinUI 3)
**Pros:**
- Native Windows performance and integration.
- First-class support for global hotkeys (via P/Invoke), System Tray (NotifyIcon), and transparent/layered windows (crucial for region selection overlays).
- Excellent built-in libraries for image processing, HttpClient, and JSON.
- Visual Studio provides superb tooling.

**Cons:**
- UI building (XAML) can be slightly more verbose than web HTML/CSS, though very powerful.
- Windows-only (which aligns perfectly with the SPEC, so not a real con here).

## Option 4: Python (PyQt / PySide / Tkinter)
**Pros:**
- Very fast to prototype.
- Great libraries for API requests and JSON handling.
- `keyboard` or `pynput` for global hotkeys; desktop magic with PIL/mss for screen capture.

**Cons:**
- UI can feel dated unless heavily customized.
- Distribution (pyinstaller) can sometimes trigger false positives in Windows Defender.
- Larger footprint than native C# for a simple utility.

## Recommendation
Given the requirement for **Windows-only**, **system tray presence**, **global hotkeys**, and **transparent region selection overlays**, **C# with WPF (or WinForms for simplicity, WPF preferred for modern look)** is the strongest candidate. It offers native performance, deep OS integration, and doesn't suffer from the memory overhead of Electron for an app that needs to run constantly in the background.

Alternatively, **Tauri** is a very strong modern choice if a web-based UI is highly desired for the configuration screens and popup, while maintaining low resource usage.

**Pending Decision:** User preference on tech stack based on their familiarity.
