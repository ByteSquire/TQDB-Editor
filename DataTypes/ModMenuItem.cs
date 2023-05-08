using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace TQDBEditor.DataTypes
{
    public partial class ModMenuItem : ObservableObject
    {
        public string Name { get; init; }

        public ICommand Command { get; init; }

        public object CommandParameter { get; init; }

        [ObservableProperty]
        private bool _isActive;

        public ModMenuItem(string name, bool isActive, ICommand command)
        { Name = name; Command = command; CommandParameter = name; IsActive = isActive; }

        public override bool Equals(object? obj)
        {
            if (obj is ModMenuItem other && other.Name == Name)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
