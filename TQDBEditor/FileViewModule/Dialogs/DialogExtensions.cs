using Prism.Services.Dialogs;
using System;
using TQDB_Parser.Blocks;

namespace TQDBEditor.FileViewModule.Dialogs
{
    public static class IDialogParametersExtensions
    {
        public const string selectedEntry = nameof(selectedEntry);
        public const string changedValue = nameof(changedValue);
        public const string templateGroup = nameof(templateGroup);

        public static void AddVariable(this IDialogParameters dialogParameters, IVariableProvider entry)
        {
            dialogParameters.Add(selectedEntry, entry);
        }

        public static IVariableProvider GetVariable(this IDialogParameters dialogParameters)
        {
            return dialogParameters.GetValue<IVariableProvider>(selectedEntry);
        }

        public static void AddTemplateGroup(this IDialogParameters dialogParameters, GroupBlock template)
        {
            dialogParameters.Add(templateGroup, template);
        }

        public static GroupBlock GetTemplateGroup(this IDialogParameters dialogParameters)
        {
            return dialogParameters.GetValue<GroupBlock>(templateGroup);
        }

        public static void AddChangedValue(this IDialogParameters dialogParameters, string value)
        {
            dialogParameters.Add(changedValue, value);
        }

        public static string GetChangedValue(this IDialogParameters dialogParameters)
        {
            return dialogParameters.GetValue<string>(changedValue);
        }
    }

    public static class IDialogServiceExtensions
    {
        public const string databaseFilePicker = nameof(databaseFilePicker);
        public const string arrayEdit = nameof(arrayEdit);
        public const string equationEdit = nameof(equationEdit);

        public static void ShowDBFilePicker(this IDialogService dialogService, Action<string> callback, IVariableProvider input)
        {
            var dialogParams = new DialogParameters();
            dialogParams.AddVariable(input);
            dialogService.Show(databaseFilePicker, dialogParams, Callback, windowName: TQDBEditor.Dialogs.IDialogServiceExtensions.confirmationDialogWindow);

            void Callback(IDialogResult result)
            {
                if (result.Result == ButtonResult.OK)
                    callback(result.Parameters.GetChangedValue());
            }
        }

        public static void ShowArrayEdit(this IDialogService dialogService, Action<string> callback, IVariableProvider input, GroupBlock template)
        {
            var dialogParams = new DialogParameters();
            dialogParams.AddVariable(input);
            dialogParams.AddTemplateGroup(template);
            dialogService.Show(arrayEdit, dialogParams, Callback, windowName: TQDBEditor.Dialogs.IDialogServiceExtensions.confirmationDialogWindow);

            void Callback(IDialogResult result)
            {
                if (result.Result == ButtonResult.OK)
                    callback(result.Parameters.GetChangedValue());
            }
        }

        public static void ShowEquationEdit(this IDialogService dialogService, Action<string> callback, IVariableProvider input, GroupBlock template)
        {
            var dialogParams = new DialogParameters();
            dialogParams.AddVariable(input);
            dialogParams.AddTemplateGroup(template);
            dialogService.Show(equationEdit, dialogParams, Callback, windowName: TQDBEditor.Dialogs.IDialogServiceExtensions.confirmationDialogWindow);

            void Callback(IDialogResult result)
            {
                if (result.Result == ButtonResult.OK)
                    callback(result.Parameters.GetChangedValue());
            }
        }
    }
}
