using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratego.Models
{
    interface ITileable
    {
        public byte GetType {get;}
        public bool Dead { get; }
        public Bitmap Image { get; }
    }
}
