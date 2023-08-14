using Avalonia.Controls;
using System;

namespace Stratego.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// gets a action that can be invoked to close the window
        /// </summary>
        /// <returns> Action that closes the window </returns>
        public Action GetCloseAction()
        {
            return () => Close();
        }
    }
}