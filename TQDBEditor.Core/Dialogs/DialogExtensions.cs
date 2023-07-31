using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Chrome;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using TQDB_Parser.DBR;

namespace TQDBEditor.Dialogs
{
    public static class IDialogParametersExtensions
    {
        public const string modName = nameof(modName);
        public const string existingMods = nameof(existingMods);
        public const string infoTitle = nameof(infoTitle);
        public const string infoText = nameof(infoText);
        public const string selectedEntry = nameof(selectedEntry);

        public static string GetModName(this IDialogParameters dialogParameters)
        {
            return dialogParameters.GetValue<string>(modName);
        }

        public static void AddModName(this IDialogParameters dialogParameters, string modName)
        {
            dialogParameters.Add(IDialogParametersExtensions.modName, modName);
        }

        public static IEnumerable<string> GetExistingMods(this IDialogParameters dialogParameters)
        {
            return dialogParameters.GetValue<IEnumerable<string>>(existingMods);
        }

        public static void AddExistingMods(this IDialogParameters dialogParameters, IEnumerable<string> existingMods)
        {
            dialogParameters.Add(IDialogParametersExtensions.existingMods, existingMods);
        }

        public static void AddInfoTitle(this IDialogParameters dialogParameters, string? title)
        {
            if (title != null)
                dialogParameters.Add(infoTitle, title);
        }

        public static string? GetInfoTitle(this IDialogParameters dialogParameters)
        {
            if (dialogParameters.TryGetValue<string>(infoTitle, out var title))
                return title;
            return null;
        }

        public static void AddInfoText(this IDialogParameters dialogParameters, string text)
        {
            dialogParameters.Add(infoText, text);
        }

        public static string GetInfoText(this IDialogParameters dialogParameters)
        {
            return dialogParameters.GetValue<string>(infoText);
        }

        public static void AddSelectedEntry(this IDialogParameters dialogParameters, DBREntry text)
        {
            dialogParameters.Add(selectedEntry, text);
        }

        public static DBREntry GetSelectedEntry(this IDialogParameters dialogParameters)
        {
            return dialogParameters.GetValue<DBREntry>(selectedEntry);
        }
    }

    public static class IDialogServiceExtensions
    {
        public const string baseDialogWindow = nameof(baseDialogWindow);
        public const string newMod = nameof(newMod);
        public const string confirmationDialogWindow = nameof(confirmationDialogWindow);
        public const string infoDialog = nameof(infoDialog);
        public const string informationDialogWindow = nameof(informationDialogWindow);
        public const string databaseFilePicker = nameof(databaseFilePicker);
        public const string arrayEdit = nameof(arrayEdit);
        public const string equationEdit = nameof(equationEdit);

        public static void ShowNewModDialog(this IDialogService dialogService, IEnumerable<string> existingMods, Action<string> callback)
        {
            var dialogParams = new DialogParameters();
            dialogParams.AddExistingMods(existingMods);
            dialogService.ShowDialog(newMod, dialogParams, Callback, confirmationDialogWindow);

            void Callback(IDialogResult dialogResult)
            {
                if (dialogResult.Result == ButtonResult.OK)
                    callback(dialogResult.Parameters.GetModName());
            }
        }

        public static void ShowInfoDialog(this IDialogService dialogService, string title, string info)
        {
            var dialogParams = new DialogParameters();
            dialogParams.AddInfoTitle(title);
            dialogParams.AddInfoText(info);
            dialogService.ShowDialog(infoDialog, dialogParams, windowName: informationDialogWindow);
        }

        public static void ShowDBFilePicker(this IDialogService dialogService, DBREntry input)
        {
            var dialogParams = new DialogParameters();
            dialogParams.AddSelectedEntry(input);
            dialogService.ShowDialog(databaseFilePicker, dialogParams, windowName: confirmationDialogWindow);
        }

        public static void ShowArrayEdit(this IDialogService dialogService, DBREntry input)
        {
            var dialogParams = new DialogParameters();
            dialogParams.AddSelectedEntry(input);
            dialogService.ShowDialog(arrayEdit, dialogParams, windowName: confirmationDialogWindow);
        }

        public static void ShowEquationEdit(this IDialogService dialogService, DBREntry input)
        {
            var dialogParams = new DialogParameters();
            dialogParams.AddSelectedEntry(input);
            dialogService.ShowDialog(equationEdit, dialogParams, windowName: confirmationDialogWindow);
        }
    }

    public class NewModDialogResult : DialogResult
    {
        public NewModDialogResult(string modName) : base(ButtonResult.OK, InitParams(modName)) { }

        private static IDialogParameters InitParams(string modName)
        {
            var dialogParameters = new DialogParameters();
            dialogParameters.AddModName(modName);
            return dialogParameters;
        }
    }
}
