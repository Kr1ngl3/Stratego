using Avalonia.Media;
using Avalonia.Media.Imaging;
using LinkedBaseAndWrapperList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratego.ViewModels
{
    interface ITileableViewModel : IWrapper
    {
        public Bitmap Image { get; }
        public ISolidColorBrush Background { get; }

        public void Click();
        public void SelectedChanged();
    }
}
