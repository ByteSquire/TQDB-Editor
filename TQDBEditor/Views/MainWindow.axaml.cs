using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using DryIoc;
using ImTools;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Diagnostics.CodeAnalysis;
using TQDBEditor.Services;
using TQDBEditor.ViewModels;

namespace TQDBEditor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            // Find CaptionButtons (Fullscreen, Minimize, Maximize, Close) to limit the menu
            if (this.TryFindVisualChild(x => x is CaptionButtons, out var capButtons))
                _captionWidth = capButtons.Bounds.Width;
            MinWidth = _captionWidth;
            // Find TitleBar (Basically just the CaptionButtons) and limit it to the right so it doesn't break focus on the menu
            if (this.TryFindVisualChild(x => x is TitleBar, out var titleBar))
                (titleBar as TitleBar)!.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

            // Offset the ToolbarWidth every time the window changes width
            SizeChanged += (s, x) => { if (x.WidthChanged) ToolbarWidth = x.NewSize.Width; };
            ToolbarWidth = Width;
        }

        public static readonly DirectProperty<MainWindow, double> ToolbarWidthProperty =
            AvaloniaProperty.RegisterDirect<MainWindow, double>(
            nameof(ToolbarWidth),
            o => o.ToolbarWidth);

        private double _toolbarWidth = 0;
        private double _captionWidth = 0;

        public double ToolbarWidth
        {
            get { return _toolbarWidth; }
            private set { if (value > _captionWidth) SetAndRaise(ToolbarWidthProperty, ref _toolbarWidth, value - _captionWidth); }
        }
    }

    public static class VisualListExtensions
    {
        /// <summary>
        /// Recursively tries to find a Visual Child matching the <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="me"></param>
        /// <param name="result"></param>
        /// <returns>Whether a matching child was found</returns>
        public static bool TryFindVisualChild(this Visual me, Func<Visual, bool> predicate, [MaybeNullWhen(false)] out Visual result)
        {
            result = default;
            var children = me.GetVisualChildren();
            if ((result = children.FindFirst(predicate)) != default)
                return true;

            foreach (var v in children)
            {
                if (v.TryFindVisualChild(predicate, out result))
                    return true;
            }

            return false;
        }
    }
}
