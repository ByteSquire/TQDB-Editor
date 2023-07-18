//using Avalonia;
//using Avalonia.Controls;
//using Avalonia.Controls.ApplicationLifetimes;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.Ini;
//using Microsoft.Extensions.Configuration.Json;
//using Prism.Services.Dialogs;
//using System;
//using System.IO;
//using System.Linq;

//namespace TQDBEditor.Services
//{
//    public interface IConfigService
//    {
//        public delegate void WorkingDirChangedEventHandler();
//        public event WorkingDirChangedEventHandler WorkingDirChanged;
//        public delegate void ModNameChangedEventHandler();
//        public event ModNameChangedEventHandler ModNameChanged;
//        public string WorkingDir { get; set; }
//        public string BuildDir { get; set; }
//        public string ToolsDir { get; set; }
//        public string[] AdditionalDirs { get; set; }
//        public string ModDir { get; }
//        public string ModsDir { get; }
//        public string ModName { get; set; }
//        public void OnExitApplication();
//    }

//    public class ConfigService : IConfigService
//    {
//        private IConfigurationRoot config;
//        private ILogger logger;
//        private Window mainWindow;
//        private IDialogService dialogService;

//        private string _workingDir;
//        public string WorkingDir
//        {
//            get => _workingDir;
//            set
//            {
//                if (_workingDir.Equals(value))
//                    return;

//                _workingDir = value;
//                WorkingDirChanged?.Invoke();
//            }
//        }
//        public string BuildDir { get; set; }
//        public string ToolsDir { get; set; }
//        public string[] AdditionalDirs { get; set; }
//        public string ModDir => Path.Combine(ModsDir, modSubDir);
//        public string ModsDir => Path.Combine(WorkingDir, "CustomMaps");
//        private string modSubDir;
//        public string ModName
//        {
//            get => modSubDir;
//            set
//            {
//                if (modSubDir.Equals(value))
//                    return;

//                modSubDir = value;
//                ModNameChanged?.Invoke();
//            }
//        }

//        public event IConfigService.WorkingDirChangedEventHandler WorkingDirChanged;
//        public event IConfigService.ModNameChangedEventHandler ModNameChanged;

//        public (int, int) WindowSize { get; set; }
//        public (int, int) WindowPos { get; set; }

//        public ConfigService(Window mainWindow, IDialogService dialogService)
//        {
//            this.mainWindow = mainWindow;
//            this.dialogService = dialogService;

//            Ready();
//        }

//        public void Ready()
//        {
//            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//            var myGamesFolder = Path.Combine(documentsFolder, "My Games");
//            var tqITPath = Path.Combine(myGamesFolder, "Titan Quest - Immortal Throne");
//            BuildDir = tqITPath;
//            ToolsDir = string.Empty;
//            _workingDir = Path.Combine(tqITPath, "Working");
//            modSubDir = string.Empty;

//            LoadConfig();
//            ApplyValues();


//            PixelRect maxSize = new();
//            foreach (var screen in mainWindow.Screens.All)
//                maxSize = new(maxSize.TopLeft + screen.Bounds.TopLeft, maxSize.BottomRight + screen.Bounds.BottomRight);

//            if (WindowSize.Item1 > 0 && WindowSize.Item2 > 0)
//            {
//                if (WindowSize.Item1 >= maxSize.Width || WindowSize.Item2 >= maxSize.Height)
//                    mainWindow.WindowState = WindowState.Maximized;

//                mainWindow.Width = WindowSize.Item1;
//                mainWindow.Height = WindowSize.Item2;
//            }
//            if (WindowPos.Item1 > 0 && WindowPos.Item2 > 0)
//            {
//                if (WindowPos.Item1 < maxSize.Width && WindowPos.Item2 < maxSize.Height)
//                    mainWindow.Position = new PixelPoint(WindowPos.Item1, WindowPos.Item2);
//            }

//            if (!ValidateConfig())
//            {
//                dialogService.ShowDialog("SetupDialog", result =>
//                {
//                    if (result.Result != ButtonResult.OK)
//                    {
//                        Exit();
//                        return;
//                    }
//                    SetupConfirmed();
//                });
//            }
//            else
//                SetupConfirmed();
//        }

//        public bool ValidateConfig()
//        {
//            if (string.IsNullOrEmpty(WorkingDir) || string.IsNullOrEmpty(ToolsDir))
//            {
//                //GD.Print("Invalid config:");
//                //GD.Print("Working: " + WorkingDir);
//                //GD.Print("Tools: " + ToolsDir);
//                return false;
//            }
//            if (!Directory.Exists(WorkingDir) || !Directory.Exists(ToolsDir))
//                return false;
//            return true;
//        }

//        private void SetupConfirmed()
//        {
//            if (string.IsNullOrEmpty(modSubDir))
//            {
//                Directory.CreateDirectory(ModsDir);
//                var mods = Directory.EnumerateDirectories(ModsDir, "*", SearchOption.TopDirectoryOnly);
//                if (mods.Any())
//                {
//                    ModName = Path.GetRelativePath(ModsDir, mods.First());
//                }
//                else
//                {
//                    var existingMods = Directory.EnumerateDirectories(ModsDir, "*", SearchOption.TopDirectoryOnly);

//                    var dialogParams = new DialogParameters
//                    {
//                        { "existingMods", existingMods }
//                    };
//                    dialogService.ShowDialog("NewMod", dialogParams, result =>
//                    {
//                        if (result.Result != ButtonResult.OK || !result.Parameters.TryGetValue("name", out string modName))
//                        {
//                            Exit();
//                            return;
//                        }

//                        Directory.CreateDirectory(Path.Combine(ModsDir, modName));
//                        ModName = modName;
//                    });
//                }
//            }
//        }

//        public string GetModPath()
//        {
//            return ModDir;
//        }

//        private static void Exit()
//        {
//            switch (Application.Current!.ApplicationLifetime)
//            {
//                case IClassicDesktopStyleApplicationLifetime desktop:
//                    desktop.Shutdown(-1);
//                    return;
//                case IControlledApplicationLifetime controlled:
//                    controlled.Shutdown(-1);
//                    return;
//            }
//        }

//        public void OnExitApplication()
//        {
//            SaveConfig();
//        }

//        public void SaveConfig()
//        {
//            WindowSize = ((int)mainWindow.Width, (int)mainWindow.Height);
//            WindowPos = (mainWindow.Position.X, mainWindow.Position.Y);
//            //if (!ValidateConfig())
//            //    return;
//            ApplyValues();

//            //Save it to a file (overwrite if already exists).
//            config.Save(ConfigurationSaveMode.Full);
//        }

//        public void ApplyValues()
//        {
//            // Create the custom section entry  
//            // in <configSections> group and the 
//            // related target section in <configuration>.
//            if (config.GetSection(EditorSettings.SECTION_NAME) is not EditorSettings editorSettings)
//            {
//                editorSettings = new();
//                config.Sections.Add(EditorSettings.SECTION_NAME, editorSettings);
//            }

//            // Set directory values
//            var dirSection = editorSettings.Directories!;
//            dirSection.WorkingDir = WorkingDir;
//            dirSection.BuildDir = BuildDir;
//            dirSection.ToolsDir = ToolsDir;
//            dirSection.AdditionalDirs = AdditionalDirs;
//            dirSection.ModDir = modSubDir;

//            //Set editor values
//            var editorSection = editorSettings.Editor!;

//            // Set window values
//            var windowSection = editorSettings.Window!;
//            windowSection.WindowSize = EditorSettings.WindowConfigElement.AsString(WindowSize);
//            windowSection.WindowPos = EditorSettings.WindowConfigElement.AsString(WindowPos);
//        }

//        public void LoadConfig()
//        {
//            // Get the application configuration file.
//            config = new ConfigurationBuilder().AddJsonFile("config.json", true, true).AddIniFile("Tools.ini", true, true).Build();

//            var manager = new ConfigurationManager();
//            manager.AddIniFile("Tools.ini", true, true).AddJsonFile("config.json", true, true);

//            var editorSettings = manager.GetSection(EditorSettings.SECTION_NAME).Get<EditorSettings>();
//            //if (config.GetSection(EditorSettings.SECTION_NAME) is not EditorSettings editorSettings)

//            // Load directory values
//            var dirSection = editorSettings?.Directories ?? new();
//            _workingDir = dirSection.WorkingDir;
//            BuildDir = dirSection.BuildDir;
//            ToolsDir = dirSection.ToolsDir;
//            AdditionalDirs = dirSection.AdditionalDirs;
//            modSubDir = dirSection.ModDir;

//            // Load editor values
//            var editorSection = editorSettings?.Editor ?? new();

//            // Load window values
//            var windowSection = editorSettings?.Window ?? new();
//            WindowSize = EditorSettings.WindowConfigElement.AsInt(windowSection.WindowSize);
//            WindowPos = EditorSettings.WindowConfigElement.AsInt(windowSection.WindowPos);

//            logger?.LogInformation("Loaded editor config");
//        }

//        private void LoadArtManagerOptions()
//        {
//            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//            var myGamesFolder = Path.Combine(documentsFolder, "My Games");
//            var tqToolsConfig = Path.Combine(myGamesFolder, "Titan Quest - Immortal Throne", "Tools.ini");
//            if (!File.Exists(tqToolsConfig))
//            {
//                tqToolsConfig = Path.Combine(myGamesFolder, "Titan Quest", "Tools.ini");
//                if (!File.Exists(tqToolsConfig))
//                    return; // maybe continue fallback chain, depending on where else the Tools.ini can be
//            }

//            var lines = File.ReadAllLines(tqToolsConfig);

//            var currentSection = "[None]";
//            foreach (var line in lines)
//            {
//                if (line.StartsWith('['))
//                {
//                    currentSection = line;
//                    continue;
//                }
//                // Load directory values
//                if (currentSection == "[Login]")
//                {
//                    var split = line.Split('=', 2);
//                    var key = split[0];
//                    var value = split[1];

//                    switch (key)
//                    {
//                        case "localdir":
//                            _workingDir = value;
//                            break;
//                        case "builddir":
//                            BuildDir = value;
//                            break;
//                        case "toolsdir":
//                            ToolsDir = value;
//                            break;
//                        case "additionalbuilddirs":
//                            AdditionalDirs = value.Split(',');
//                            break;
//                        case "moddir":
//                            modSubDir = value;
//                            break;
//                        default:
//                            break;
//                    }
//                    continue;
//                }
//                // Load editor values
//                //if (currentSection == "[Database]")
//                //{
//                //    var split = line.Split('=', 2);
//                //    var key = split[0];
//                //    var value = split[1];

//                //    switch (key)
//                //    {
//                //        case "nameColumnWidth":
//                //            NameColumnWidth = int.Parse(value);
//                //            break;
//                //        case "classColumnWidth":
//                //            ClassColumnWidth = int.Parse(value);
//                //            break;
//                //        case "typeColumnWidth":
//                //            TypeColumnWidth = int.Parse(value);
//                //            break;
//                //        case "defaultValueColumnWidth":
//                //            DefaultValueColumnWidth = int.Parse(value);
//                //            break;
//                //        case "descriptionColumnWidth":
//                //            DescriptionColumnWidth = int.Parse(value);
//                //            break;
//                //        default:
//                //            break;
//                //    }
//                //    continue;
//                //}
//            }
//            logger?.LogInformation("Loaded ArtManager options from: [i]Documents[/i]{separator}{ArtManager-ToolsPath}",
//                Path.DirectorySeparatorChar, Path.GetRelativePath(documentsFolder, tqToolsConfig));
//        }
//    }

//    public sealed class EditorSettings
//    {
//        public const string SECTION_NAME = "CustomApplicationConfig";

//        [ConfigurationKeyName(nameof(Directories))]
//        public DirectoriesConfigSection? Directories { get; set; } = new();

//        [ConfigurationKeyName(nameof(Editor))]
//        public EditorConfigElement? Editor { get; set; } = new();

//        [ConfigurationKeyName(nameof(Window))]
//        public WindowConfigElement? Window { get; set; } = new();

//        public class DirectoriesConfigSection
//        {
//            [ConfigurationKeyName(nameof(WorkingDir))]
//            public string WorkingDir { get; set; } = string.Empty;

//            [ConfigurationKeyName(nameof(BuildDir))]
//            public string BuildDir { get; set; } = string.Empty;

//            [ConfigurationKeyName(nameof(ToolsDir))]
//            public string ToolsDir { get; set; } = string.Empty;

//            [ConfigurationKeyName(nameof(ModDir))]
//            public string ModDir { get; set; } = string.Empty;

//            [ConfigurationKeyName(nameof(AdditionalDirs))]
//            public string[] AdditionalDirs { get; set; } = Array.Empty<string>();
//        }

//        public class EditorConfigElement
//        {
//        }

//        public class WindowConfigElement
//        {
//            [ConfigurationKeyName(nameof(WindowSize))]
//            public string WindowSize { get; set; } = string.Empty;

//            [ConfigurationKeyName(nameof(WindowPos))]
//            public string WindowPos { get; set; } = string.Empty;

//            public static (int, int) AsInt(string input)
//            {
//                var split = input.Split(';');
//                return (int.Parse(split[0]), int.Parse(split[1]));
//            }
//            public static string AsString((int, int) input)
//            {
//                return input.Item1.ToString() + ';' + input.Item2.ToString();
//            }
//        }
//    }
//}
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Configuration;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TQDBEditor.Services
{
    public class ConfigService
    {
        public const string CONFIG_FILE_PATH = "TQDBEditor.config.json";

        private IConfigurationRoot _config;

        public ConfigService(IContainerRegistry containerRegistry)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
                lifetime.ShutdownRequested += Lifetime_ShutdownRequested;

            if (!File.Exists(CONFIG_FILE_PATH))
                LoadArtManagerConfig();
            else
                LoadConfig();

            var config = _config ?? new ConfigurationBuilder().AddInMemoryCollection().Build();

            var notifyingConfiguration = new NotifyingConfiguration(config);
            _config = notifyingConfiguration;

            containerRegistry.RegisterInstance(notifyingConfiguration);
            containerRegistry.RegisterInstance<IConfiguration>(notifyingConfiguration);
        }

        private void LoadConfig()
        {
            try
            {
                _config = new ConfigurationBuilder().AddJsonFile(CONFIG_FILE_PATH).Build();
            }
            catch (Exception) { }
        }

        private void LoadArtManagerConfig()
        {
            try
            {
                var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var myGamesFolder = Path.Combine(documentsFolder, "My Games");
                var tqToolsConfig = Path.Combine(myGamesFolder, "Titan Quest - Immortal Throne", "Tools.ini");
                if (!File.Exists(tqToolsConfig))
                {
                    tqToolsConfig = Path.Combine(myGamesFolder, "Titan Quest", "Tools.ini");
                    if (!File.Exists(tqToolsConfig))
                        return; // maybe continue fallback chain, depending on where else the Tools.ini can be
                }
                var amConfig = new ConfigurationBuilder().AddIniFile(tqToolsConfig).Build();

                _config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {DirectoryConfigurationExtensions.WORKING_DIR, amConfig["Login:localdir"]},
                        {DirectoryConfigurationExtensions.BUILD_DIR,  amConfig["Login:builddir"]},
                        {DirectoryConfigurationExtensions.TOOLS_DIR, amConfig["Login:toolsdir"]},
                        {DirectoryConfigurationExtensions.MOD_DIR, amConfig["Login:moddir"]},
                    })
                    .Build();
            }
            catch (Exception) { }
        }

        private void Lifetime_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            try
            {
                // save config
                var json = new Dictionary<string, object>();
                foreach (var child in _config.GetChildren())
                {
                    var pair = ToJsonObject(child);
                    json.Add(pair.Item1, pair.Item2);
                }

                File.WriteAllText(CONFIG_FILE_PATH, JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                File.WriteAllText(CONFIG_FILE_PATH, "{}");
            }
        }

        private (string, object) ToJsonObject(IConfigurationSection section)
        {
            if (section.Value != null)
                return (section.Key, section.Value);
            else
            {
                var json = new Dictionary<string, object>();
                foreach (var child in section.GetChildren())
                {
                    var pair = ToJsonObject(child);
                    json.Add(pair.Item1, pair.Item2);
                }
                return (section.Key, json);
            }
        }
    }

    public class NotifyingConfiguration : ConfigurationRoot, IConfiguration
    {
        public NotifyingConfiguration(IConfigurationRoot configuration) : base(configuration.Providers.ToList()) { }

        public delegate void ConfigurationChangedEventHandler(IConfiguration sender, string key);
        public event ConfigurationChangedEventHandler? ConfigurationChanged;

        public new string? this[string key]
        {
            get => base[key];
            set
            {
                if (value != base[key])
                {
                    base[key] = value;
                    ConfigurationChanged?.Invoke(this, key);
                }
            }
        }

        string? IConfiguration.this[string key]
        {
            get => base[key];
            set => this[key] = value;
        }
    }

    public static class DirectoryConfigurationExtensions
    {
        public const string SECTION_KEY = "directories";
        public const string WORKING_DIR = "directories:workingdir";
        public const string BUILD_DIR = "directories:builddir";
        public const string TOOLS_DIR = "directories:toolsdir";
        public const string MOD_DIR = "directories:moddir";

        public static string? GetWorkingDir(this IConfiguration configuration) => configuration[WORKING_DIR];
        public static void SetWorkingDir(this IConfiguration configuration, string? workingDir) => configuration[WORKING_DIR] = workingDir;

        public static string? GetBuildDir(this IConfiguration configuration) => configuration[BUILD_DIR];
        public static void SetBuildDir(this IConfiguration configuration, string? workingDir) => configuration[BUILD_DIR] = workingDir;

        public static string? GetToolsDir(this IConfiguration configuration) => configuration[TOOLS_DIR];
        public static void SetToolsDir(this IConfiguration configuration, string? workingDir) => configuration[TOOLS_DIR] = workingDir;

        public static string? GetModDir(this IConfiguration configuration) => configuration[MOD_DIR];
        public static void SetModDir(this IConfiguration configuration, string? workingDir) => configuration[MOD_DIR] = workingDir;
    }
}