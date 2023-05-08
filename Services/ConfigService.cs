using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Prism.Services.Dialogs;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;

namespace TQDBEditor.Services
{
    public interface IConfigService
    {
        public delegate void WorkingDirChangedEventHandler();
        public event WorkingDirChangedEventHandler WorkingDirChanged;
        public delegate void ModNameChangedEventHandler();
        public event ModNameChangedEventHandler ModNameChanged;
        public string WorkingDir { get; set; }
        public string BuildDir { get; set; }
        public string ToolsDir { get; set; }
        public string[] AdditionalDirs { get; set; }
        public string ModDir { get; }
        public string ModsDir { get; }
        public string ModName { get; set; }
        public void OnExitApplication();
    }

    public class ConfigService : IConfigService
    {
        private Configuration config;
        private ILogger logger;
        private Window mainWindow;
        private IDialogService dialogService;

        private string _workingDir;
        public string WorkingDir
        {
            get => _workingDir;
            set
            {
                if (_workingDir.Equals(value))
                    return;

                _workingDir = value;
                WorkingDirChanged?.Invoke();
            }
        }
        public string BuildDir { get; set; }
        public string ToolsDir { get; set; }
        public string[] AdditionalDirs { get; set; }
        public string ModDir => Path.Combine(ModsDir, modSubDir);
        public string ModsDir => Path.Combine(WorkingDir, "CustomMaps");
        private string modSubDir;
        public string ModName
        {
            get => modSubDir;
            set
            {
                if (modSubDir.Equals(value))
                    return;

                modSubDir = value;
                ModNameChanged?.Invoke();
            }
        }

        public event IConfigService.WorkingDirChangedEventHandler WorkingDirChanged;
        public event IConfigService.ModNameChangedEventHandler ModNameChanged;

        public (int, int) WindowSize { get; set; }
        public (int, int) WindowPos { get; set; }

        public ConfigService(Window mainWindow, IDialogService dialogService)
        {
            this.mainWindow = mainWindow;
            this.dialogService = dialogService;

            Ready();
        }

        public void Ready()
        {
            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var myGamesFolder = Path.Combine(documentsFolder, "My Games");
            var tqITPath = Path.Combine(myGamesFolder, "Titan Quest - Immortal Throne");
            BuildDir = tqITPath;
            ToolsDir = string.Empty;
            _workingDir = Path.Combine(tqITPath, "Working");
            modSubDir = string.Empty;

            LoadConfig();
            ApplyValues();


            PixelRect maxSize = new();
            foreach (var screen in mainWindow.Screens.All)
                maxSize = new(maxSize.TopLeft + screen.Bounds.TopLeft, maxSize.BottomRight + screen.Bounds.BottomRight);

            if (WindowSize.Item1 > 0 && WindowSize.Item2 > 0)
            {
                if (WindowSize.Item1 >= maxSize.Width || WindowSize.Item2 >= maxSize.Height)
                    mainWindow.WindowState = WindowState.Maximized;

                mainWindow.Width = WindowSize.Item1;
                mainWindow.Height = WindowSize.Item2;
            }
            if (WindowPos.Item1 > 0 && WindowPos.Item2 > 0)
            {
                if (WindowPos.Item1 < maxSize.Width && WindowPos.Item2 < maxSize.Height)
                    mainWindow.Position = new PixelPoint(WindowPos.Item1, WindowPos.Item2);
            }

            if (!ValidateConfig())
            {
                dialogService.ShowDialog("SetupDialog", result =>
                {
                    if (result.Result != ButtonResult.OK)
                    {
                        Exit();
                        return;
                    }
                    SetupConfirmed();
                });
            }
            else
                SetupConfirmed();
        }

        public bool ValidateConfig()
        {
            if (string.IsNullOrEmpty(WorkingDir) || string.IsNullOrEmpty(ToolsDir))
            {
                //GD.Print("Invalid config:");
                //GD.Print("Working: " + WorkingDir);
                //GD.Print("Tools: " + ToolsDir);
                return false;
            }
            if (!Directory.Exists(WorkingDir) || !Directory.Exists(ToolsDir))
                return false;
            return true;
        }

        private void SetupConfirmed()
        {
            if (string.IsNullOrEmpty(modSubDir))
            {
                Directory.CreateDirectory(ModsDir);
                var mods = Directory.EnumerateDirectories(ModsDir, "*", SearchOption.TopDirectoryOnly);
                if (mods.Any())
                {
                    ModName = Path.GetRelativePath(ModsDir, mods.First());
                }
                else
                {
                    var existingMods = Directory.EnumerateDirectories(ModsDir, "*", SearchOption.TopDirectoryOnly);

                    var dialogParams = new DialogParameters
                    {
                        { "existingMods", existingMods }
                    };
                    dialogService.ShowDialog("NewMod", dialogParams, result =>
                    {
                        if (result.Result != ButtonResult.OK || !result.Parameters.TryGetValue("name", out string modName))
                        {
                            Exit();
                            return;
                        }

                        Directory.CreateDirectory(Path.Combine(ModsDir, modName));
                        ModName = modName;
                    });
                }
            }
        }

        public string GetModPath()
        {
            return ModDir;
        }

        private static void Exit()
        {
            switch (Application.Current!.ApplicationLifetime)
            {
                case IClassicDesktopStyleApplicationLifetime desktop:
                    desktop.Shutdown(-1);
                    return;
                case IControlledApplicationLifetime controlled:
                    controlled.Shutdown(-1);
                    return;
            }
        }

        public void OnExitApplication()
        {
            SaveConfig();
        }

        public void SaveConfig()
        {
            WindowSize = ((int)mainWindow.Width, (int)mainWindow.Height);
            WindowPos = (mainWindow.Position.X, mainWindow.Position.Y);
            //if (!ValidateConfig())
            //    return;
            ApplyValues();

            //Save it to a file (overwrite if already exists).
            config.Save(ConfigurationSaveMode.Full);
        }

        public void ApplyValues()
        {
            // Create the custom section entry  
            // in <configSections> group and the 
            // related target section in <configuration>.
            if (config.Sections[EditorSettings.SECTION_NAME] is not EditorSettings editorSettings)
            {
                editorSettings = new();
                config.Sections.Add(EditorSettings.SECTION_NAME, editorSettings);
            }

            // Set directory values
            var dirSection = editorSettings.Directories!;
            dirSection.WorkingDir = WorkingDir;
            dirSection.BuildDir = BuildDir;
            dirSection.ToolsDir = ToolsDir;
            dirSection.AdditionalDirs = AdditionalDirs;
            dirSection.ModDir = modSubDir;

            //Set editor values
            var editorSection = editorSettings.Editor!;

            // Set window values
            var windowSection = editorSettings.Window!;
            windowSection.WindowSize = WindowSize;
            windowSection.WindowPos = WindowPos;

            editorSettings.SectionInformation.ForceSave = true;
        }

        public void LoadConfig()
        {
            try
            {
                // Get the application configuration file.
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);

                if (config.GetSection(EditorSettings.SECTION_NAME) is not EditorSettings editorSettings)
                    throw new ConfigurationErrorsException();

                // Load directory values
                var dirSection = editorSettings.Directories ?? new();
                _workingDir = dirSection.WorkingDir;
                BuildDir = dirSection.BuildDir;
                ToolsDir = dirSection.ToolsDir;
                AdditionalDirs = dirSection.AdditionalDirs;
                modSubDir = dirSection.ModDir;

                // Load editor values
                var editorSection = editorSettings.Editor ?? new();

                // Load window values
                var windowSection = editorSettings.Window ?? new();
                WindowSize = windowSection.WindowSize;
                WindowPos = windowSection.WindowPos;

                logger?.LogInformation("Loaded editor config");
            }
            catch (ConfigurationErrorsException)
            {
                LoadArtManagerOptions();
                return;
            }
        }

        private void LoadArtManagerOptions()
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

            var lines = File.ReadAllLines(tqToolsConfig);

            var currentSection = "[None]";
            foreach (var line in lines)
            {
                if (line.StartsWith('['))
                {
                    currentSection = line;
                    continue;
                }
                // Load directory values
                if (currentSection == "[Login]")
                {
                    var split = line.Split('=', 2);
                    var key = split[0];
                    var value = split[1];

                    switch (key)
                    {
                        case "localdir":
                            _workingDir = value;
                            break;
                        case "builddir":
                            BuildDir = value;
                            break;
                        case "toolsdir":
                            ToolsDir = value;
                            break;
                        case "additionalbuilddirs":
                            AdditionalDirs = value.Split(',');
                            break;
                        case "moddir":
                            modSubDir = value;
                            break;
                        default:
                            break;
                    }
                    continue;
                }
                // Load editor values
                //if (currentSection == "[Database]")
                //{
                //    var split = line.Split('=', 2);
                //    var key = split[0];
                //    var value = split[1];

                //    switch (key)
                //    {
                //        case "nameColumnWidth":
                //            NameColumnWidth = int.Parse(value);
                //            break;
                //        case "classColumnWidth":
                //            ClassColumnWidth = int.Parse(value);
                //            break;
                //        case "typeColumnWidth":
                //            TypeColumnWidth = int.Parse(value);
                //            break;
                //        case "defaultValueColumnWidth":
                //            DefaultValueColumnWidth = int.Parse(value);
                //            break;
                //        case "descriptionColumnWidth":
                //            DescriptionColumnWidth = int.Parse(value);
                //            break;
                //        default:
                //            break;
                //    }
                //    continue;
                //}
            }
            logger?.LogInformation("Loaded ArtManager options from: [i]Documents[/i]{separator}{ArtManager-ToolsPath}",
                Path.DirectorySeparatorChar, Path.GetRelativePath(documentsFolder, tqToolsConfig));
        }
    }

    public sealed class EditorSettings : ConfigurationSection
    {
        public const string SECTION_NAME = "CustomApplicationConfig";

        public EditorSettings() : base()
        {
            Directories = new();
            Editor = new();
            Window = new();
        }

        [ConfigurationProperty(nameof(Directories))]
        public DirectoriesConfigElement? Directories { get => base[nameof(Directories)] as DirectoriesConfigElement; set => base[nameof(Directories)] = value; }

        [ConfigurationProperty(nameof(Editor))]
        public EditorConfigElement? Editor { get => base[nameof(Editor)] as EditorConfigElement; set => base[nameof(Editor)] = value; }

        [ConfigurationProperty(nameof(Window))]
        public WindowConfigElement? Window { get => base[nameof(Window)] as WindowConfigElement; set => base[nameof(Window)] = value; }

        public class DirectoriesConfigElement : ConfigurationElement
        {
            [ConfigurationProperty(nameof(WorkingDir))]
            public string WorkingDir { get => base[nameof(WorkingDir)] as string ?? string.Empty; set => base[nameof(WorkingDir)] = value; }

            [ConfigurationProperty(nameof(BuildDir))]
            public string BuildDir { get => base[nameof(BuildDir)] as string ?? string.Empty; set => base[nameof(BuildDir)] = value; }

            [ConfigurationProperty(nameof(ToolsDir))]
            public string ToolsDir { get => base[nameof(ToolsDir)] as string ?? string.Empty; set => base[nameof(ToolsDir)] = value; }

            [ConfigurationProperty(nameof(ModDir))]
            public string ModDir { get => base[nameof(ModDir)] as string ?? string.Empty; set => base[nameof(ModDir)] = value; }

            [ConfigurationProperty(nameof(AdditionalDirs))]
            public string[] AdditionalDirs { get => base[nameof(AdditionalDirs)] as string[] ?? Array.Empty<string>(); set => base[nameof(AdditionalDirs)] = value; }
        }

        public class EditorConfigElement : ConfigurationElement
        {

        }

        public class WindowConfigElement : ConfigurationElement
        {
            [ConfigurationProperty(nameof(WindowSize))]
            public (int, int) WindowSize { get => base[nameof(WindowSize)] as (int, int)? ?? (0, 0); set => base[nameof(WindowSize)] = value; }

            [ConfigurationProperty(nameof(WindowPos))]
            public (int, int) WindowPos { get => base[nameof(WindowPos)] as (int, int)? ?? (0, 0); set => base[nameof(WindowPos)] = value; }
        }
    }
}