using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Stratego.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Stratego.ViewModels
{
    class PieceViewModel : ViewModelBase, ITileableViewModel
    {
        private readonly Piece _piece;
        private Vector2? _target = null;
        private bool _vanish = false;
        private bool _appear = false;
        private bool _attack = false;
        private bool _isVisible;
        private Vector2? _animationStart = null;

        public bool IsOtherColor => _piece.IsOtherColor;
        public double Size => GameViewModel.s_pieceSize;
        public Bitmap Image => _piece.Image;
        public Vector2? Target { get => _target; 
            set {
                if (value is null)
                    _animationStart = null;
                this.RaiseAndSetIfChanged(ref _target, value, nameof(Target));
            } }
        public bool Vanish { get => _vanish; set => this.RaiseAndSetIfChanged(ref _vanish, value, nameof(Vanish)); }
        public bool Appear { get => _appear; set => this.RaiseAndSetIfChanged(ref _appear, value, nameof(Appear)); }
        public bool IsVisible { get => _isVisible; set => this.RaiseAndSetIfChanged(ref _isVisible, value, nameof(IsVisible)); }
        public bool Attack { get => _attack; 
            set {
                if (!value)
                    _animationStart = new Vector2((float)X, (float)Y);
                this.RaiseAndSetIfChanged(ref _attack, value, nameof(Attack));
            } }

        public double X {
            get {
                if (Attack)
                    return (Target ?? Vector2.Zero).Y == 0 ? ReduceLength((Target ?? Vector2.Zero).X, Size * .5) : (Target ?? Vector2.Zero).X;
                else
                    return (Target ?? Vector2.Zero).X;
            } }
        public double Y {
            get {
                if (Attack)
                    return (Target ?? Vector2.Zero).X == 0 ? ReduceLength((Target ?? Vector2.Zero).Y, Size * .5) : (Target ?? Vector2.Zero).Y;
                else
                    return (Target ?? Vector2.Zero).Y;
            } }

        public double StartX => (_animationStart ?? Vector2.Zero).X;
        public double StartY => (_animationStart ?? Vector2.Zero).Y;

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
            _isVisible = piece.IsVisible;
        }

        public async void Click()
        {
            if (_piece.CanBeTargeted)
            {
                await _piece.Attacked();
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

        private double ReduceLength(double length, double reduction)
        {
            return Math.CopySign(Math.Abs(length) - reduction,length);
        }
    }
}
