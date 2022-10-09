using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class Config : Node
{
    //Create new ConfigFile object.
    private ConfigFile config;

    public string WorkingDir { get; set; }
    public string BuildDir { get; set; }
    public string ToolsDir { get; set; }
    public Array<string> AdditionalDirs { get; set; }
    public string ModDir => Path.Combine(WorkingDir, "CustomMaps", modSubDir);
    public string ModName => modSubDir;
    private string modSubDir;


    public bool ViewDescriptions { get; set; }
    public int NameColumnWidth { get; set; }
    public int ClassColumnWidth { get; set; }
    public int TypeColumnWidth { get; set; }
    public int DefaultValueColumnWidth { get; set; }
    public int DescriptionColumnWidth { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        config = new ConfigFile();
        AdditionalDirs = new Array<string>();
        LoadConfig();
        ApplyValues();

        GD.Print(WorkingDir);
        GD.Print(BuildDir);
        GD.Print(ToolsDir);
        GD.Print(AdditionalDirs);
        GD.Print(ModDir);

        GD.Print(ViewDescriptions);
        GD.Print(NameColumnWidth);
        GD.Print(ClassColumnWidth);
        GD.Print(TypeColumnWidth);
        GD.Print(DefaultValueColumnWidth);
        GD.Print(DescriptionColumnWidth);
    }

    public string GetModPath()
    {
        return ModDir;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    //public override void _Process(double delta)
    //{
    //}

    public void SaveConfig()
    {
        ApplyValues();

        //Save it to a file (overwrite if already exists).
        config.Save("user://config.cfg");
    }

    public void ApplyValues()
    {
        // Set directory values
        config.SetValue("Directories", "workingDir", WorkingDir);
        config.SetValue("Directories", "buildDir", BuildDir);
        config.SetValue("Directories", "toolsDir", ToolsDir);
        config.SetValue("Directories", "additionalDirs", AdditionalDirs/*string.Join(",", AdditionalDirs)*/);
        config.SetValue("Directories", "modDir", modSubDir);

        //Set editor values
        config.SetValue("Editor", "viewDescriptions", ViewDescriptions);
        config.SetValue("Editor", "nameColumnWidth", NameColumnWidth);
        config.SetValue("Editor", "classColumnWidth", ClassColumnWidth);
        config.SetValue("Editor", "typeColumnWidth", TypeColumnWidth);
        config.SetValue("Editor", "defaultValueColumnWidth", DefaultValueColumnWidth);
        config.SetValue("Editor", "descriptionColumnWidth", DescriptionColumnWidth);
    }

    public void LoadConfig()
    {
        var dirSection = "Directories";
        var editorSection = "Editor";
        if (config.Load("user://config.cfg") != Error.Ok)
        {
            var documentsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var myGamesFolder = Path.Combine(documentsFolder, "My Games");
            var tqToolsConfig = Path.Combine(myGamesFolder, "Titan Quest - Immortal Throne", "Tools.ini");
            GD.Print(tqToolsConfig);
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
                            WorkingDir = value;
                            break;
                        case "builddir":
                            BuildDir = value;
                            break;
                        case "toolsdir":
                            ToolsDir = value;
                            break;
                        case "additionalbuilddirs":
                            foreach (var additionalDir in value.Split(","))
                                AdditionalDirs.Add(additionalDir);
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
                if (currentSection == "[Database]")
                {
                    var split = line.Split('=', 2);
                    var key = split[0];
                    var value = split[1];

                    switch (key)
                    {
                        case "viewDescriptions":
                            ViewDescriptions = int.Parse(value) != 0;
                            break;
                        case "nameColumnWidth":
                            NameColumnWidth = int.Parse(value);
                            break;
                        case "classColumnWidth":
                            ClassColumnWidth = int.Parse(value);
                            break;
                        case "typeColumnWidth":
                            TypeColumnWidth = int.Parse(value);
                            break;
                        case "defaultValueColumnWidth":
                            DefaultValueColumnWidth = int.Parse(value);
                            break;
                        case "descriptionColumnWidth":
                            DescriptionColumnWidth = int.Parse(value);
                            break;
                        default:
                            break;
                    }
                    continue;
                }
            }
            GD.Print("Loaded ArtManager options!");
            return;
        }

        // Load directory values
        WorkingDir = (string)config.GetValue(dirSection, "localDir", WorkingDir);
        BuildDir = (string)config.GetValue(dirSection, "buildDir", BuildDir);
        ToolsDir = (string)config.GetValue(dirSection, "toolsDir", ToolsDir);
        AdditionalDirs = new Array<string>(config.GetValue(dirSection, "additionalbuildDirs", AdditionalDirs).AsStringArray());
        modSubDir = (string)config.GetValue(dirSection, "modDir", modSubDir);

        // Load editor values
        ViewDescriptions = (bool)config.GetValue(editorSection, "viewDescriptions", true);
        NameColumnWidth = (int)config.GetValue(editorSection, "nameColumnWidth", -1);
        ClassColumnWidth = (int)config.GetValue(editorSection, "classColumnWidth", -1);
        TypeColumnWidth = (int)config.GetValue(editorSection, "typeColumnWidth", -1);
        DefaultValueColumnWidth = (int)config.GetValue(editorSection, "defaultValueColumnWidth", -1);
        DescriptionColumnWidth = (int)config.GetValue(editorSection, "descriptionColumnWidth", -1);
    }
}
