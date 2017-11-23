# WpfLocalization

This project provides a simple, yet powerful and feature-rich way to localize a WPF application. It is focused on 
simplicity and easy-of-use and is an alternative to the WPF localization method provided by Microsoft.

The project is a replacement (a complete rewrite and redesign) of an earlier localization solution posted on 
CodeProject in 2009 - [Advanced WPF Localization](https://www.codeproject.com/Articles/249369/Advanced-WPF-Localization).


## Basic usage

*Please refer to the [Wiki](https://github.com/JechoJekov/WpfLocalization/wiki/) for complete feature list and examples. 
This section describes basic usage only.*

1. Install the WpfLocaliation package from [NuGet](https://www.nuget.org/packages/WpfLocalization)
2. Create a default resource file in your project (Solution Explorer - Project -> Properties -> Resources)
3. Use the `Loc` XAML extension in your application.

```XAML
<Window ...>
...
  <TextBlock Text="{Loc Text_HelloWorld}"/>
...
</Window>
```

4. Add localized values to the resource file.

Name | Value
---- | -----
... | ...
Text_HelloWorld | Welcome to WPF localization!
... | ...

5. Add culture-specific resource files (e.g. `Resources.de.resx`, `Resources.fr-FR.resx`) and localize the values

*Resources.de.resx*

Name | Value
---- | -----
... | ...
Text_HelloWorld | Willkommen bei der WPF localization!
... | ...

*Resources.fr-FR.resx*

Name | Value
---- | -----
... | ...
Text_HelloWorld | Bienvenue dans la WPF localization!
... | ...


## Overall list of features

*Please refer to the [Wiki](https://github.com/JechoJekov/WpfLocalization/wiki/) for a complete feature list and examples.*

* Both dependency and non-dependency properties
* Any data type for which there is a `TypeConverter` (including images, icons, fonts, colors, enumerations, margins, etc.)
* Bindings
* Templates
* Styles (including setters)
* User controls
* Custom controls (both in themes and in code-behind)
* Code-behind
* Multiple resource files located in any project (assembly)
* Mix multiple languages inside the same control, window or different windows
* Multiple UI threads
* High-performance (important for applications that utilize data grids or other demanding data controls)


## Documentation

[Wiki](https://github.com/JechoJekov/WpfLocalization/wiki/)  
[Code Project](https://www.codeproject.com/Articles/xxxxxx/Ultimate-WPF-Localization)


## Sample project

[C# - WpfLocalization.Demo](https://github.com/JechoJekov/WpfLocalization/tree/master/Project/WpfLocalization.Demo)  
[VB.NET - WpfLocalization.VBDemo](https://github.com/JechoJekov/WpfLocalization/tree/master/Project/WpfLocalization.VBDemo)

## License

Public Domain - [Unlicense](http://unlicense.org/)


## NuGet package

https://www.nuget.org/packages/WpfLocalization