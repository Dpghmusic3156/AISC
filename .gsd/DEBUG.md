---
status: resolved
trigger: "không thể mở app, nó chạy rồi tự tắt ngay lập tức"
created: 2026-03-11T15:02
updated: 2026-03-11T15:08
---

## Symptoms
expected: App starts and shows tray icon, runs in background
actual: App launches and immediately closes
errors: XamlParseException at App.xaml — Cannot locate resource

## Resolution
root_cause: App.xaml used incorrect MaterialDesignColors resource paths (`Recommended/Accent/` doesn't exist) and was missing `Defaults.xaml`. These are v5 breaking changes from earlier MaterialDesign versions.
fix: Replaced manual resource dictionary loading with the `BundledTheme` approach — the correct API for MaterialDesignThemes v5.x.
verification: App launches successfully, no crash, tray icon appears.

## Evidence
- checked: Full exception stack trace via `dotnet run 2>&1`
  found: `System.IO.IOException: Cannot locate resource 'themes/recommended/accent/materialdesigncolor.lime.xaml'`
  implication: The `Accent` folder doesn't exist in v5 — it was renamed to `Secondary`

- checked: Context7 MaterialDesignInXAML docs for correct App.xaml setup
  found: v5 uses `<materialDesign:BundledTheme>` element + `MaterialDesign3.Defaults.xaml`
  implication: Original App.xaml was using an outdated/incorrect configuration pattern

## Files Changed
- `src/AIScreenCapture.UI/App.xaml` — Replaced 4 manual ResourceDictionary entries with `BundledTheme` + `MaterialDesign3.Defaults.xaml`
