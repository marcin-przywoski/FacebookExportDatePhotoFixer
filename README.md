# Facebook Export Date Fixer

[![CI](https://github.com/marcin-przywoski/FacebookExportDatePhotoFixer/actions/workflows/CI.yml/badge.svg)](https://github.com/marcin-przywoski/FacebookExportDatePhotoFixer/actions/workflows/CI.yml)
[![Release](https://img.shields.io/github/release/marcin-przywoski/FacebookExportDatePhotoFixer.svg)](https://github.com/marcin-przywoski/FacebookExportDatePhotoFixer/releases)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/marcin-przywoski/FacebookExportDatePhotoFixer)
[![Downloads](https://img.shields.io/github/downloads/marcin-przywoski/FacebookExportDatePhotoFixer/total)](https://github.com/marcin-przywoski/FacebookExportDatePhotoFixer/releases)

This application is used to embed dates (Creation Time, Last Access Time, Last Write Time) back into the media contained in the Facebook Export as by default these are stripped. Currently it works for both HTML and JSON exports and selected media formats (GIF / JPG / MP4 / PNG).
I've created it as there was no tool available for easy use that allowed to instantly embed dates from Messenger HTML files back into saved media and copy them to another location to keep original files in place.  
In case of any issues just post a new issue and I'll fix it - it will be a great opportunity for learning. Same goes for improvements!

## Installation

Download the repository and compile it by yourself in VS Code or VS or download the compiled package from the Packages section
I've added pre-compiled self contained packages if you don't have .NET core runtime installed

## Requirements

- .NET 5 runtime, .NET 6 runtime or .NET 3.1 runtime if you want to compile it for yourself

## Road map

- Make program compatible with different versions of exports including older ones (facebook changes some things over time).

- ~~Allow program to work with JSON exports~~

- ~~Add wider support for different .NET versions~~

- ~~Improve UI~~ and clarity of the program

- Add more customization options

- Add an option to choose only selected media types

- Add an option to choose only selected conversations (both via a direct select of desired folder as well as after inital scanning of export folder)

- Add ability to embed date into EXIF meta-data

- ~~Add ability to change filenames to date of being sent for ease of search~~

- Add an option to put everything in one folder instead of creating replication of original folder structure

- Add ability to overwrite existing files in source directory

- ~~Improve performance of the application with the usage of parallelism~~

- ~~Fix bottleneck on updating UI by batching them~~

- Make program working on MacOS and Linux

- (Possibly in the future) Rewrite application to a full fledged application for viewing exported Messenger conversations with added functionality

## Acknowledgments

- [AngleSharp)](https://github.com/AngleSharp/AngleSharp)
- [AngleSharp.XPath)](https://github.com/AngleSharp/AngleSharp.XPath)
- [Ookii.Dialogs.Wpf](https://github.com/ookii-dialogs/ookii-dialogs-wpf)
- [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)
- [System.Reactive](https://github.com/dotnet/reactive)

## Lessons Learned

This is my first published project on GitHub and I've learned how to use threads to update UI from other threads, how to use async / await as well as how to use external libraries in order to not re-invent the wheel :)
