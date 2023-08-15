using Avalonia.Media.Imaging;
using LinkedBaseAndWrapperList;

namespace Stratego.Models
{
    interface ITileable : IModel
    {
        public byte GetType { get; }
        public bool Dead { get; }
        public Bitmap Image { get; }
    }
}
