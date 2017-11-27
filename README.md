# WpfLocalization

[![Join the chat at https://gitter.im/WpfLocalization/Lobby](https://badges.gitter.im/WpfLocalization/Lobby.svg)](https://gitter.im/WpfLocalization/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This project provides a simple, yet powerful and feature-rich way to localize a WPF application. It is focused on 
simplicity and easy-of-use and is an alternative to the WPF localization method provided by Microsoft.

The project is a replacement (a complete rewrite and redesign) of an earlier localization solution posted on 
CodeProject in 2009 - [Advanced WPF Localization](https://www.codeproject.com/Articles/249369/Advanced-WPF-Localization).

## Basic usage

*Please refer to the [Wiki](https://github.com/JechoJekov/WpfLocalization/wiki/) for complete feature list and examples. 
This section describes basic usage only.*

1. Install the WpfLocaliation package from [NuGet](https://www.nuget.org/packages/WpfLocalization)
2. Use the `Loc` markup extension in your application

```XAML
<Window ...>
  ...
  <TextBlock Text="{Loc Text_HelloWorld}"/>
  ...
  <!-- Localize bindings -->
  <TextBlock Text="{Loc Text_HelloUserName, Binding={Binding UserName}}"/>
  ...
  <!-- Localize multi-bindings -->
  <TextBlock>
    <TextBlock.Text>
      <Loc Key="Text_HelloPersonalName">
        <Binding Path="FirstName"/>
        <Binding Path="LastName"/>
      </Loc>
    </TextBlock.Text>
  </TextBlock>
  ...
  <!-- Localize date/time, number and currency formatting even if there is no localized resource -->
  <TextBlock Text="{Loc StringFormat='{}{0:d}', Binding={Binding BirthDate}}"/>
  ...
  <!-- Localize date/time, number and currency formatting inside localized text -->
  <TextBlock Text="{Loc Text_BirthDate, Binding={Binding BirthDate}}"/>
  ...
</Window>
```

3. Add localized values to the default resource file created by Visual Studio
  (go to *Solution Explorer - Project -> Properties -> Resources* or open 
  `Properties/Resources.resx` (C#), `My Project/Resources.resx` (VB.NET))

Name | Value
---- | -----
... | ...
Text_HelloWorld | Welcome to WPF localization!
Text_HelloUserName | Hello {0}!
Text_HelloPersonalName | Hello {0} {1}!
Text_BirthDate | Your birth date is {0:d}.
... | ...

4. Add culture-specific resource files (e.g. `Resources.de.resx`, `Resources.fr-FR.resx`) and localize the values

*Resources.de.resx*

Name | Value
---- | -----
... | ...
Text_HelloWorld | Willkommen bei der WPF localization!
Text_HelloUserName | Hallo {0}!
Text_HelloPersonalName | Hallo {0} {1}!
Text_BirthDate | Dein Geburtsdatum ist {0:d}.
... | ...

*Resources.fr-FR.resx*

Name | Value
---- | -----
... | ...
Text_HelloWorld | Bienvenue dans la WPF localization!
Text_HelloUserName | Bonjour {0}!
Text_HelloPersonalName | Bonjour {0} {1}!
Text_BirthDate | Votre date de naissance est le {0:d}.
... | ...


## Overall list of features

*Please refer to the [Wiki](https://github.com/JechoJekov/WpfLocalization/wiki/) for a complete feature list and examples.*

* Supports both dependency and non-dependency properties
* Changing the culture at runtime automatically updates all localized properties
  * Design-time preview of different languages
* Supports text, images and any data type for which there is a `TypeConverter` (including fonts, colors, enumerations, margins, etc.)
* Bindings
* Templates
* Styles (including setters)
* User controls
* Custom controls (both in themes and in code-behind)
* Code-behind
* Multiple resource files located in any assembly
* Mix multiple languages inside the same control, window or different windows
* Multiple UI threads
* High-performance (important for applications that utilize data grids or other demanding data controls)
* Supports WPF designer in Visual Studio 2013/2015/2017 including previewing different languages at design time


## Documentation

[Wiki](https://github.com/JechoJekov/WpfLocalization/wiki/)


## Demo project

[C# - WpfLocalization.Demo](https://github.com/JechoJekov/WpfLocalization/tree/master/Project/WpfLocalization.Demo)  
[VB.NET - WpfLocalization.VBDemo](https://github.com/JechoJekov/WpfLocalization/tree/master/Project/WpfLocalization.VBDemo)


## License

Public Domain - [Unlicense](http://unlicense.org/)

*Note:* Even though the project is public domain I would appreciate if you let me know in case you use the library in an open source
or a commercial project. I would also appreciate any suggestions and bug reports.


## NuGet package

https://www.nuget.org/packages/WpfLocalization
