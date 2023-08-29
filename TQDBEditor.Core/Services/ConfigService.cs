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
    public static class ConfigService
    {
        public const string CONFIG_FILE_PATH = "TQDBEditor.config.json";

        public static void RegisterConfigService(IContainerRegistry containerRegistry)
        {
            IConfigurationRoot? config = null;
            if (!File.Exists(CONFIG_FILE_PATH))
                config = LoadArtManagerConfig();
            else
                config = LoadConfig();

            config ??= new ConfigurationBuilder().AddInMemoryCollection().Build();

            var notifyingConfiguration = new NotifyingConfiguration(config);

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
                lifetime.ShutdownRequested += (_, _) => Lifetime_ShutdownRequested(notifyingConfiguration);

            containerRegistry.RegisterInstance<IObservableConfiguration>(notifyingConfiguration);
            containerRegistry.RegisterInstance<IConfiguration>(notifyingConfiguration);
        }

        private static IConfigurationRoot? LoadConfig()
        {
            try
            {
                return new ConfigurationBuilder().AddJsonFile(CONFIG_FILE_PATH).Build();
            }
            catch (Exception) { }
            return null;
        }

        private static IConfigurationRoot? LoadArtManagerConfig()
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
                        return null; // maybe continue fallback chain, depending on where else the Tools.ini can be
                }
                var amConfig = new ConfigurationBuilder().AddIniFile(tqToolsConfig).Build();

                return new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {DirectoryConfigurationExtensions.WORKING_DIR, amConfig["Login:localdir"]},
                        {DirectoryConfigurationExtensions.BUILD_DIR,  amConfig["Login:builddir"]},
                        {DirectoryConfigurationExtensions.TOOLS_DIR, amConfig["Login:toolsdir"]},
                        {DirectoryConfigurationExtensions.MOD_DIR, amConfig["Login:moddir"]},

                        {DBRViewConfigurationExtensions.NAME_WIDTH, amConfig["Database:nameColumnWidth"]},
                        {DBRViewConfigurationExtensions.CLASS_WIDTH, amConfig["Database:classColumnWidth"]},
                        {DBRViewConfigurationExtensions.TYPE_WIDTH, amConfig["Database:typeColumnWidth"]},
                        {DBRViewConfigurationExtensions.DESC_WIDTH, amConfig["Database:descriptionColumnWidth"]},
                    })
                    .Build();
            }
            catch (Exception) { }
            return null;
        }

        private static void Lifetime_ShutdownRequested(IConfiguration config)
        {
            try
            {
                // save config
                var json = new Dictionary<string, object>();
                foreach (var child in config.GetChildren())
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

        private static (string, object) ToJsonObject(IConfigurationSection section)
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

    public interface IObservableConfiguration : IConfiguration
    {
        public delegate void ConfigurationChangedEventHandler(IConfiguration sender, string key);
        public event ConfigurationChangedEventHandler? ConfigurationChanged;

        public void AddWellKnownChangeHandler(string key, Action<string?> handler);
    }

    public class NotifyingConfiguration : ConfigurationRoot, IObservableConfiguration
    {
        private readonly Dictionary<string, ICollection<Action<string?>>> _wellKnownHandlers;
        public NotifyingConfiguration(IConfigurationRoot configuration) : base(configuration.Providers.ToList())
        {
            _wellKnownHandlers = new();
        }

        public event IObservableConfiguration.ConfigurationChangedEventHandler? ConfigurationChanged;

        public new string? this[string key]
        {
            get => base[key];
            set
            {
                if (value != base[key])
                {
                    base[key] = value;
                    if (_wellKnownHandlers.TryGetValue(key, out var handlers))
                        foreach (var handler in handlers)
                            handler(value);
                    ConfigurationChanged?.Invoke(this, key);
                }
            }
        }

        string? IConfiguration.this[string key]
        {
            get => base[key];
            set => this[key] = value;
        }

        public void AddWellKnownChangeHandler(string key, Action<string?> handler)
        {
            if (!_wellKnownHandlers.TryGetValue(key, out ICollection<Action<string?>>? collection))
            {
                collection = new List<Action<string?>>();
                _wellKnownHandlers.Add(key, collection);
            }
            collection.Add(handler);
        }
    }

    public static class DBRViewConfigurationExtensions
    {
        public const string SECTION_KEY = "dbr";
        public const string NAME_WIDTH = "dbr:namewidth";
        public const string CLASS_WIDTH = "dbr:classwidth";
        public const string TYPE_WIDTH = "dbr:typewidth";
        public const string DESC_WIDTH = "dbr:descwidth";
        public const string VALUE_WIDTH = "dbr:valuewidth";

        public static string? GetNameWidth(this IConfiguration configuration) => configuration[NAME_WIDTH];
        public static void SetNameWidth(this IConfiguration configuration, string? value) => configuration[NAME_WIDTH] = value;
        public static void AddNameWidthChangeListener(this IObservableConfiguration configuration, Action<string?> listener) => configuration.AddWellKnownChangeHandler(NAME_WIDTH, listener);

        public static string? GetClassWidth(this IConfiguration configuration) => configuration[CLASS_WIDTH];
        public static void SetClassWidth(this IConfiguration configuration, string? value) => configuration[CLASS_WIDTH] = value;
        public static void AddClassWidthChangeListener(this IObservableConfiguration configuration, Action<string?> listener) => configuration.AddWellKnownChangeHandler(CLASS_WIDTH, listener);

        public static string? GetTypeWidth(this IConfiguration configuration) => configuration[TYPE_WIDTH];
        public static void SetTypeWidth(this IConfiguration configuration, string? value) => configuration[TYPE_WIDTH] = value;
        public static void AddTypeWidthChangeListener(this IObservableConfiguration configuration, Action<string?> listener) => configuration.AddWellKnownChangeHandler(TYPE_WIDTH, listener);

        public static string? GetDescWidth(this IConfiguration configuration) => configuration[DESC_WIDTH];
        public static void SetDescWidth(this IConfiguration configuration, string? value) => configuration[DESC_WIDTH] = value;
        public static void AddDescWidthChangeListener(this IObservableConfiguration configuration, Action<string?> listener) => configuration.AddWellKnownChangeHandler(DESC_WIDTH, listener);

        public static string? GetValueWidth(this IConfiguration configuration) => configuration[VALUE_WIDTH];
        public static void SetValueWidth(this IConfiguration configuration, string? value) => configuration[VALUE_WIDTH] = value;
        public static void AddValueWidthChangeListener(this IObservableConfiguration configuration, Action<string?> listener) => configuration.AddWellKnownChangeHandler(VALUE_WIDTH, listener);
    }

    public static class DirectoryConfigurationExtensions
    {
        public const string SECTION_KEY = "directories";
        public const string WORKING_DIR = "directories:workingdir";
        public const string BUILD_DIR = "directories:builddir";
        public const string TOOLS_DIR = "directories:toolsdir";
        public const string MOD_DIR = "directories:moddir";

        public static string? GetWorkingDir(this IConfiguration configuration) => configuration[WORKING_DIR];
        public static void SetWorkingDir(this IConfiguration configuration, string? value) => configuration[WORKING_DIR] = value;
        public static void AddWorkingDirChangeListener(this IObservableConfiguration configuration, Action<string?> listener) => configuration.AddWellKnownChangeHandler(WORKING_DIR, listener);

        public static string? GetBuildDir(this IConfiguration configuration) => configuration[BUILD_DIR];
        public static void SetBuildDir(this IConfiguration configuration, string? value) => configuration[BUILD_DIR] = value;
        public static void AddBuildDirChangeListener(this IObservableConfiguration configuration, Action<string?> listener) => configuration.AddWellKnownChangeHandler(BUILD_DIR, listener);

        public static string? GetToolsDir(this IConfiguration configuration) => configuration[TOOLS_DIR];
        public static void SetToolsDir(this IConfiguration configuration, string? value) => configuration[TOOLS_DIR] = value;
        public static void AddToolsDirChangeListener(this IObservableConfiguration configuration, Action<string?> listener) => configuration.AddWellKnownChangeHandler(TOOLS_DIR, listener);

        public static string? GetModName(this IConfiguration configuration) => configuration[MOD_DIR];
        public static void SetModName(this IConfiguration configuration, string? value) => configuration[MOD_DIR] = value;
        public static void AddModNameChangeListener(this IObservableConfiguration configuration, Action<string?> listener) => configuration.AddWellKnownChangeHandler(MOD_DIR, listener);

        public static string? GetModDir(this IConfiguration configuration) => (configuration[WORKING_DIR] == null || configuration[MOD_DIR] == null) ? null : Path.Combine(configuration[WORKING_DIR]!, "CustomMaps", configuration[MOD_DIR]!);
        public static void AddModDirChangeListener(this IObservableConfiguration configuration, Action<string?> listener)
        {
            configuration.AddModNameChangeListener(x => listener(GetModDir(configuration)));
            configuration.AddWorkingDirChangeListener(x => listener(GetModDir(configuration)));
        }
    }
}