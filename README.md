# Auto Clicker

Auto Clicker is a Windows desktop app for automating mouse clicks. Configure the click interval and behavior, choose where clicks happen, then start or stop the sequence from the app or with a global hotkey.

## Features

- Set the delay between clicks in hours, minutes, seconds, and milliseconds.
- Add an optional random timing offset.
- Choose the left, right, or middle mouse button and single or double clicks.
- Repeat a specified number of times or continue until stopped.
- Click at the current cursor position or select a fixed screen location.
- Start and stop clicking with a configurable global function key (F1–F12); F6 is the default.

## Requirements

- Windows 10 version 1809 (build 17763) or later.
- The .NET SDK that supports the target framework in `AutoClicker/AutoClicker.csproj`.

## Build and run

From the repository root, build the solution:

```powershell
dotnet build .\AutoClicker.slnx
```

Run the app with:

```powershell
dotnet run --project .\AutoClicker\AutoClicker.csproj
```

## Usage

1. Set the interval and click options.
2. Choose **Current location**, or select the fixed-location option and click **Pick location**. Click the desired point on screen to select it; press **Esc** to cancel.
3. Choose a fixed repeat count or **Repeat until stopped**.
4. Select **Start** or press the configured hotkey (F6 by default) to begin. Use the same control to stop.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).
