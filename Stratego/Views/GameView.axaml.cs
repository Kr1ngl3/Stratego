using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Stratego.Models;
using Stratego.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;

namespace Stratego.Views
{
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            GameViewModel.s_pieceSize = (float)field.DesiredSize.Width / 10;
        }

    }
}
