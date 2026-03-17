# Laincord

A Discord client built to look and feel like Windows Live Messenger 2009, forked from [Aerochat](https://github.com/not-nullptr/Aerochat).

Laincord picks up where Aerochat left off — fixing bugs, filling in missing functionality, and working toward a complete WLM 2009 experience. Along the way, it brings in Discord-native features like reactions, emoji pickers, and full server support that fit naturally into the classic Messenger interface.

<!-- If you have a screenshot, add it here: -->
<!-- ![Laincord](screenshot.png) -->

## Features

- Windows Live Messenger 2009 look and feel
- Direct messages and group DMs
- Server and channel support
- Reactions
- Emoji picker with search, categories, and server emoji
- Emoji autocomplete
- Animated GIF embeds
- Custom scenes and ads
- Desktop notifications
- Resizable server sidebar

## Download

To download Laincord, please click the links in the "Releases" section on the right of this page, or visit the following link:

### [View releases](https://github.com/lkenna/Laincord/releases)

## Frequently-asked questions

Please see our dedicated page for frequently-asked questions and help.

### [View frequently-asked questions](https://github.com/lkenna/Laincord/wiki/Frequently%E2%80%90asked-questions)

## Building

In order to build Laincord from source, you will need:

- [Visual Studio 2022 (with the .NET Desktop Development workload)](https://visualstudio.microsoft.com)

Laincord cannot be built as `AnyCPU` due to depending on native code. You must set the build architecture to `x64` before you can build.

## Credits

- [Aerochat](https://github.com/not-nullptr/Aerochat) by nullptr — the original project this is forked from
- [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) — Discord API library

## License

Laincord is licensed under the [Mozilla Public License 2.0](LICENSE). DSharpPlus is licensed under the [MIT License](DSP/LICENSE).
