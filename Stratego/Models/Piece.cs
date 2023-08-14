using Avalonia.Media.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Stratego.Models
{
    class Piece : ITileable
    {
        public enum Type
        {
            Flag,
            Spy,
            Scout,
            Miner,
            Sergeant,
            Lieutenant,
            Captain,
            Major,
            Colonel,
            General,
            Marshal,
            Bomb,
        }
        public enum Color
        {
            Red,
            Blue
        }

        private readonly Board _board;
        private readonly Type _type;
        private readonly Color _playerColor;

        public new byte GetType => (byte)_type;
        private readonly Color _color;
        private bool _isSelectable { 
            get {
                if (!_board.YourTurn 
                    || _color != _playerColor)
                    return false;

                if (_board.State == Board.GameState.Prep)
                    return true;
                else
                    return _type != Type.Flag && _type != Type.Bomb && !Dead;
            } }

        private bool _isSelected = false;

        public Bitmap Image { 
            get {
                if (!IsOtherColor || Highlighted)
                    return s_images[(int)_type];
                else
                    return s_empty;
            } }

        public bool Dead { get; set; } = true;
        public Bitmap BackPiece => _color == Color.Red ? s_redBackPiece : s_blueBackPiece;
        public bool IsSelected => _isSelected;
        public bool CanBeTargeted { get; set; } = false;
        public bool IsBomb => _type == Type.Bomb;
        public bool IsOtherColor => _color != _playerColor;
        public bool Highlighted { get; set; } = false;


        public Piece(Type type, Color color, Board board, Color playerColor)
        {
            _board = board;
            _type = type;
            _color = color;
            _playerColor = playerColor;
        }

        public void Unselect()
        {
            _isSelected = false;
        }

        public void Select()
        {
            if (_isSelected)
                return;

            if (_isSelectable)
            {
                _isSelected = true;
                _board.Select(this);
            }
        }

        public async Task Attacked()
        {
            await _board.Attacked(this);
        }

        public static Color OtherColor(Color c)
        {
            return (Color)(1 & ~(int)c);
        }


        static Bitmap s_redBackPiece = new Bitmap(Directory.GetCurrentDirectory() + "/Assets/red.png");
        static Bitmap s_blueBackPiece = new Bitmap(Directory.GetCurrentDirectory() + "/Assets/blue.png");
        static Bitmap s_empty = new Bitmap(Directory.GetCurrentDirectory() + "/Assets/empty.png");
        static Bitmap[] s_images = {
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/flag.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/spy.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/scout.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/miner.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/sergeant.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/lieutenant.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/captain.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/major.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/colonel.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/general.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/marshal.png"),
            new Bitmap(Directory.GetCurrentDirectory() + "/Assets/bomb.png")
        };
    }
}
