using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Stratego.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratego.ViewModels
{
    class TileViewModel : ViewModelBase, ITileableViewModel
    {
        private readonly Tile _tile;

        public virtual Bitmap Image => _tile.Image;
        public virtual ISolidColorBrush Background => _tile.IsSelectable ? Brushes.GreenYellow : Brushes.Transparent;

        public TileViewModel(Tile tile)
        {
            _tile = tile;
        }


        public void Click()
        {
            _tile.Move();
        }

        public void SelectedChanged()
        {
            this.RaisePropertyChanged(nameof(Background));
        }
    }
}
