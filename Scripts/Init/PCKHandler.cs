using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using TQDB_Parser;

namespace TQDBEditor
{
    public partial class PCKHandler : Node
    {
        private Dictionary<string, List<PackedScene>> registeredFileEditors;
        private Dictionary<(string vName, string vClass, string vType), List<PackedScene>> registeredEntryEditors;

        private ILogger logger;

        public override void _Ready()
        {
            registeredFileEditors = new();
            registeredEntryEditors = new();

            logger = this.GetConsoleLogger();

            ReadPcks();
        }

        private void ReadPcks()
        {
            var editors = Directory.EnumerateDirectories("editors", "*", SearchOption.TopDirectoryOnly);

            foreach (var fileEditor in editors)
            {
                var files = Directory.EnumerateFiles(fileEditor);
                try
                {
                    var fileEditorDll = files.Single(x => x.EndsWith(".dll"));
                    Assembly.LoadFile(fileEditorDll);
                }
                catch (InvalidOperationException)
                {
                    // dll is optional, only required if using C#
                }
                try
                {
                    var fileEditorPck = files.Single(x => x.EndsWith(".pck"));
                    var success = ProjectSettings.LoadResourcePack(fileEditorPck);
                    if (!success)
                        logger.LogError("Failed to load editor pack: {pack}", fileEditorPck);
                }
                catch (InvalidOperationException)
                {
                    logger.LogError("Failed to load editor: {pack}, must contain exactly one pck file", fileEditor);
                    continue;
                }
            }

            using var resDA = DirAccess.Open("res://Editors");
            var resEditors = resDA.GetDirectories();
            GD.Print(resEditors);
            foreach (var resEditor in resEditors)
            {
                using var da = DirAccess.Open("res://Editors/" + resEditor);
                if (da.FileExists("info.json"))
                {
                    using var infoFile = Godot.FileAccess.Open("res://Editors/" + resEditor + "/info.json",
                        Godot.FileAccess.ModeFlags.Read);

                    try
                    {
                        var node = JsonNode.Parse(infoFile.GetAsText());

                        foreach (var infoObj in node.AsArray().AsEnumerable().Select(x => x.AsObject()))
                        {
                            if (infoObj.TryGetPropertyValue("name", out var nameNode))
                            {
                                var scenePath = "res://Editors/" + resEditor + '/' + nameNode + ".tscn";
                                if (!Godot.FileAccess.FileExists(scenePath))
                                {
                                    logger.LogError("Editor {editor} referenced in {info} not found", scenePath, infoFile.GetPath());
                                    continue;
                                }
                                if (infoObj.TryGetPropertyValue("templateName", out var templateNode))
                                {
                                    RegisterFileEditor(ResourceLoader.Load<PackedScene>(scenePath), (string?)templateNode);
                                }
                                else if (infoObj.TryGetPropertyValue("variable", out var variableNode))
                                {
                                    try
                                    {
                                        var variableObj = variableNode.AsObject();

                                        if (variableObj.TryGetPropertyValue("name", out var vNameNode))
                                        {
                                            if (variableObj.TryGetPropertyValue("class", out var vClassNode))
                                            {
                                                if (variableObj.TryGetPropertyValue("type", out var vTypeNode))
                                                {
                                                    RegisterEntryEditor(ResourceLoader.Load<PackedScene>(scenePath), (string?)vNameNode, (string?)vClassNode, (string?)vTypeNode);
                                                }
                                                else
                                                {
                                                    logger.LogError("File {file}: The variable must have a type", infoFile);
                                                }
                                            }
                                            else
                                            {
                                                logger.LogError("File {file}: The variable must have a class", infoFile);
                                            }
                                        }
                                        else
                                        {
                                            logger.LogError("File {file}: The variable must have a name", infoFile);
                                        }
                                    }
                                    catch (InvalidOperationException)
                                    {
                                        logger.LogError("File {file}: The variable property must be an object", infoFile);
                                    }
                                }
                                else
                                {
                                    logger.LogError("File {file}: Need to specify templateName or variable property", infoFile);
                                }
                            }
                        }
                    }
                    catch (JsonException e)
                    {
                        logger.LogError(e, "File: {file}", infoFile);
                    }
                    catch (InvalidOperationException e)
                    {
                        logger.LogError(e, "File: {file}", infoFile);
                    }
                }
                else
                {
                    logger.LogError("Editor: {editor} is missing an info.json",
                        "res://Editors/" + resEditor);
                }
            }
        }

        public void RegisterFileEditor(PackedScene editor, string templateName = null)
        {
            if (templateName is null)
            {
                var key = "any";
                if (registeredFileEditors.ContainsKey(key))
                    registeredFileEditors[key].Add(editor);
                else
                    registeredFileEditors.Add(key, new List<PackedScene>() { editor });

                logger.LogInformation("Loaded file editor {editor}", editor.ResourcePath);
                return;
            }

            if (templateName.Contains(Path.DirectorySeparatorChar) ||
                templateName.Contains(Path.AltDirectorySeparatorChar))
                throw new ArgumentException(
                        $"The provided name must be the filename without any directories",
                        nameof(templateName));

            if (!templateName.EndsWith(".tpl"))
            {
                if (!templateName.Contains('.'))
                    templateName += ".tpl";
                else
                    throw new ArgumentException($"The provided name must be the filename" +
                        $" or the filename with the .tpl extension, other extensions are invalid",
                        nameof(templateName));
            }

            if (registeredFileEditors.ContainsKey(templateName))
                registeredFileEditors[templateName].Add(editor);
            else
                registeredFileEditors.Add(templateName, new List<PackedScene>() { editor });

            logger.LogInformation("Loaded file editor {editor}", editor.ResourcePath);
        }

        public IReadOnlyList<PackedScene> GetFileEditors(string templateName)
        {
            if (registeredFileEditors.ContainsKey(templateName))
                return registeredFileEditors[templateName];
            else if (registeredFileEditors.TryGetValue("any", out var list))
                return list;

            logger.LogError("Missing generic file editor!");
            return null;
        }

        public void RegisterEntryEditor(PackedScene editor,
            string variableName = null,
            string variableClass = null,
            string variableType = null)
        {
            //if (variableName is null && variableClass is null && variableType is null)
            //    throw new ArgumentException("At least one variable constraint must be set!");

            if (variableClass != null && !Enum.TryParse<VariableClass>(variableClass, true, out var _))
            {
                logger.LogError("Unknown variable class: {class}", variableClass);
                return;
            }
            if (variableType != null && !Enum.TryParse<VariableType>(variableType, true, out var _))
            {
                logger.LogError("Unknown variable type: {type}", variableType);
                return;
            }

            var key = (
                vName: variableName ?? "any",
                vClass: variableClass ?? "any",
                vType: variableType ?? "any"
            );

            if (registeredEntryEditors.ContainsKey(key))
                registeredEntryEditors[key].Add(editor);
            else
                registeredEntryEditors.Add(key, new List<PackedScene>() { editor });

            logger.LogInformation("Loaded entry editor {editor}", editor.ResourcePath);
        }

        public IReadOnlyList<PackedScene> GetEntryEditors(
            string variableName,
            string variableClass,
            string variableType)
        {
            if (!Enum.TryParse<VariableClass>(variableClass, out var _))
                logger.LogWarning("Using unknown variable class {class}", variableClass);
            if (!Enum.TryParse<VariableType>(variableType, out var _))
                logger.LogWarning("Using unknown variable type {type}", variableType);

            var key = (
                vName: variableName,
                vClass: variableClass,
                vType: variableType
            );

            return GetEntryEditors(key);
        }

        private IReadOnlyList<PackedScene> GetEntryEditors((string vName, string vClass, string vType) key)
        {
            if (registeredEntryEditors.ContainsKey(key))
                return registeredEntryEditors[key];
            else
            {
                if (key.vName != "any")
                {
                    key.vName = "any";
                    return GetEntryEditors(key);
                }
                if (key.vClass != "any")
                {
                    key.vClass = "any";
                    return GetEntryEditors(key);
                }
                if (key.vType != "any")
                {
                    key.vType = "any";
                    return GetEntryEditors(key);
                }

                logger.LogError("Missing generic entry editor!");
                return null;
            }
        }
    }
}