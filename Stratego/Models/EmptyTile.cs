using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Stratego.Models
{
    class EmptyTile : ITileable
    {

        // mostly to tell what collection it is in
        private bool _dead;
        private Board _board;

        // for when a piece can be moved to the tile
        public bool IsSelectable { get; set; }

        public new byte GetType => 12;
        public bool Dead => _dead;
        public Bitmap Image => s_image;

        public EmptyTile(bool dead, Board board)
        {
            _board = board;
            _dead = dead;
        }

        public bool Move()
        {
            if (!IsSelectable)
                return false;
            
            _board.Move(this);
            return true;
        }

        static Bitmap s_image = new Bitmap(Directory.GetCurrentDirectory() + "/Assets/empty.png");
    }
}
