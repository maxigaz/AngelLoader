<h1 align="center">
AngelLoader
</h1>
<p align="center"><img src="http://fenphoenix.com/github/AngelLoader/main_window_cover_3_600.png" /></p>

## Description
AngelLoader is a modern, standalone fan mission loader for Thief 1, Thief 2, Thief 3, and System Shock 2. Current loaders for those games (FMSel, NewDarkLoader) must be attached to each game individually, necessitating multiple installs, multiple setting of config options, the inability to manage all your missions in one place, etc. AngelLoader is a one-stop shop for all your missions: every FM can be viewed, played, edited, installed, and uninstalled from one place.

The list of fan missions is filterable by game and many other criteria, and provides the option to either organize games by tab or to treat them as ordinary filters.

The interface is inspired by DarkLoader (by Björn Henke and Tom N. Harris) and NewDarkLoader (by Robin Collier). AngelLoader emulates the classic DarkLoader/NewDarkLoader UI design, with its simple "everything at your fingertips" layout making for a quick and intuitive experience. It also incorporates features from NewDarkLoader and FMSel, such as tags, filtering, rating, optional audio file conversion, etc.

FM loaders have traditionally had FM scanning functionality, and AngelLoader's scanner is second to none, detecting titles and authors from the trickiest of fan missions with a speed and accuracy rate not seen from any loader before. It also detects NewDark game types accurately, in contrast to DarkLoader which requires manual editing of its .ini file in order for NewDark Thief 1 missions to work.

In short, AngelLoader aims to be a complete successor to DarkLoader, being an all-in-one loader and manager with an intuitive interface, high performance, and many features both classic and modern.

## Building
- Download a 32-bit build of [FFmpeg](https://ffmpeg.zeranoe.com/builds/) (**must be 32-bit**) or use this [custom minimal build](https://www.dropbox.com/s/hguxwku13kf16zc/ffmpeg_minimal_AngelLoader.zip)
    - For the regular build:
        - Create a folder named "ffmpeg" in the solution base dir.
        - Extract the ffmpeg archive. It should have a bin folder in it. Copy all files from the bin folder to the ffmpeg folder you just created.
    - For the minimal build:
        - Just extract the ffmpeg folder to the solution base dir.
    - My custom build should be a NuGet package, but I can't figure out how to make a NuGet package consisting only of binaries, as no matter what I try it just gleefully vomits out an empty .dll along with the actual stuff

## License
AngelLoader's code is released under the MIT license, except portions which are otherwise specified.
AngelLoader contains portions of code from the following:
- Ookii Dialogs by [Sven Groot](http://www.ookii.org/software/dialogs/), modified by [Caio Proiete](https://github.com/caioproiete/ookii-dialogs-winforms), and further modified and slimmed down by myself. This code is released under the BSD 3-clause license.

FMScanner, which has now been forked to be part of AngelLoader, uses portions of code from the following:
- [SimpleHelpers.Net](https://github.com/khalidsalomao/SimpleHelpers.Net)
- Modified portions of the [.NET Core](https://github.com/dotnet/corefx) System.IO.Compression code (tuned for scanning performance)
