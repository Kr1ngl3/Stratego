using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratego.Models
{
    interface ITileableViewModel
    {
        public Bitmap Image { get; }
        public ISolidColorBrush Background { get; }

        public void Click();
        public void SelectedChanged();
    }
}
