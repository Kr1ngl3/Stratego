using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Stratego.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratego.ViewModels
{
    class PieceViewModel : ViewModelBase, ITileableViewModel
    {
        private readonly Piece _piece;

        public Bitmap Image => _piece.Image;
        public ISolidColorBrush Background { get 
            {
                if (_piece.IsSelected)
                    return new SolidColorBrush(new Color(255, 235, 216, 52), .8);
                else if (_piece.CanBeTargeted)
                    return new SolidColorBrush(new Color(255, 255, 0, 0), .8);
                else
                    return Brushes.Transparent;
            } }
        public Bitmap BackPiece => _piece.BackPiece;

        public PieceViewModel(Piece piece)
        {
            _piece = piece;
        }

        public void Click()
        {
            if (_piece.CanBeTargeted)
            {
                _piece.Attacked();
            }
            else
            {
                _piece.Select();
                SelectedChanged();
            }
        }

        public void HighLightedChanged()
        {
            this.RaisePropertyChanged(nameof(Image));
        }

        public void SelectedChanged()
        {
            this.RaisePropertyChanged(nameof(Background));
        }
    }
}
