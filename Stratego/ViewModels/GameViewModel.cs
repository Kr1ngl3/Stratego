using Avalonia.Media.Imaging;
using Stratego.Models;
using System.Collections.Generic;
using System.IO;
using Avalonia.Collections;
using System;
using Avalonia.Controls.Mixins;
using Avalonia.Controls;
using ReactiveUI;
using System.Threading.Tasks;
using Avalonia.Metadata;
using System.Numerics;
using Avalonia.Rendering;
using Avalonia.Threading;
using System.Linq;
using DynamicData.Kernel;
using LinkedBaseAndWrapperList;
using System.Collections.Specialized;
using System.Reflection;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

namespace Stratego.ViewModels
{

    class GameViewModel : ViewModelBase
    {
        #region private readonly fields
        // board that runs the game
        private readonly Board _board;
        #endregion

        #region private fields
        // lists for each place where pieces can be stored
        private ObservableWrapperList<ITileable, ITileableViewModel> _field;
        private ObservableWrapperList<ITileable, ITileableViewModel> _enemyDeadPieces;
        private ObservableWrapperList<ITileable, ITileableViewModel> _deadPieces;
        
        // determines if the player is ready to send its field
        private bool _readyActive => _board.DeadPiecesCount == 0;
        #endregion

        #region public properties
        // getters for each list of pieces
        public IEnumerable<ITileableViewModel> Field => _field.List;
        public IEnumerable<ITileableViewModel> EnemyDeadPieces => _enemyDeadPieces.List;
        public IEnumerable<ITileableViewModel> DeadPieces => _deadPieces.List;
        
        // getter for backgroudn bitmap
        public Bitmap Background => s_background;

        // determines if the game is in prep phase and buttons used in prep phase should be shown
        // game state only updates when the game starts and therefore also checks your turn which is set to false when the field has been send
        public bool GameInPrep => _board.State == Board.GameState.Prep && _board.YourTurn;
        #endregion

        #region static variables
        // bitmap for background image
        static Bitmap s_background = new Bitmap(Directory.GetCurrentDirectory() + "/Assets/plateau.png");
        // int for size of piece, updated when window is resized
        public static float s_pieceSize = 0;
        #endregion

        #region public methods
        /// <summary>
        /// constructor also subscribes to boards events and initializes lists
        /// </summary>
        /// <param name="color"> color to be passed to board </param>
        /// <param name="client"> client to be passed to board </param>
        /// <param name="playerNumber"> player number to be passed to board </param>
        public GameViewModel(Piece.Color color, Client client, int playerNumber)
        {
            BaseList<ITileable, ITileableViewModel> field = new BaseList<ITileable, ITileableViewModel>();
            BaseList<ITileable, ITileableViewModel> dead = new BaseList<ITileable, ITileableViewModel>();
            BaseList<ITileable, ITileableViewModel> enemyDead = new BaseList<ITileable, ITileableViewModel>();
            _board = new Board(color, client, playerNumber, field, dead, enemyDead);

            _field = new ObservableWrapperList<ITileable, ITileableViewModel>(field);
            _deadPieces = new ObservableWrapperList<ITileable, ITileableViewModel>(dead);
            _enemyDeadPieces = new ObservableWrapperList<ITileable, ITileableViewModel>(enemyDead);

            _board.AnimatePiece += AnimatePiece;

            //
            _board.PiecesChanged += PropertyChangedOnPieces;

            // calls start condition updated, when pieces are moved in prep phase, when ready button is hit and when the board is cleared
            _board.StartConditionUpdated += StartConditionUpdated;
        }

        private async void AnimatePiece(object? sender, AnimationEventArgs e)
        {
            if (sender is not Piece)
                return;
            Piece p = (Piece)sender;



            PieceViewModel? pv = FindPieceViewModel(p);
            if (pv is null)
                return;
            
            pv.Appear = e.IsAppearing;
            if (e.IsAttacking)
                pv.Attack = e.IsAttacking;
            pv.Vanish = e.IsVanishing;
            if (e.Target is not null)
                pv.Target = e.Target;

            if (e.IsAppearing)
                await Task.Run(async () =>
                {
                    // wait till after animation begins
                    await Task.Delay(10);
                    pv.IsVisible = true;
                });
            await Task.Delay(e.AnimationTime);
            
            pv.Appear = false;
            pv.Vanish = false;
            
            p.IsAnimating = false;
            if (e.IsVanishing)
                pv.Target = null;
            if (e.IsAttacking)
                pv.Attack = false;
            if (!e.IsAttacking)
                pv.Target = null;


            if ((e.Target is not null || e.IsVanishing) && !e.IsAttacking)
                pv.IsVisible = false;
        }

        private PieceViewModel? FindPieceViewModel(Piece p)
        {
            PieceViewModel? pv = (PieceViewModel)_field.Find(p)!;
            if (pv is not null)
                return pv;
            pv = (PieceViewModel)_deadPieces.Find(p)!;
            if (pv is not null) 
                return pv;
            pv = (PieceViewModel)_enemyDeadPieces.Find(p)!;
            return pv;
        }

        public void Chaos()
        {
            Clear();
            _board.Chaos();
        }

        public void Clear()
        {
            _board.InitializeBoard();
        }

        public void Ready()
        {
            _board.Ready();
        }

        [DependsOn(nameof(_readyActive))]
        bool CanReady(object _)
        {
            return _readyActive;
        }
        #endregion

        //private async void PieceAttack(object? indexFrom, int indexTo)
        //{
        //    (_field[(int)indexFrom!] as PieceViewModel)!.Target = new((indexTo % 10 - (int)indexFrom % 10) * s_pieceSize, (indexTo / 10 - (int)indexFrom / 10) * s_pieceSize);
        //    (_field[(int)indexFrom!] as PieceViewModel)!.Attack = true;
        //    await AnimatePiece(_field, (int)indexFrom, 1000);
        //    (_field[(int)indexFrom!] as PieceViewModel)!.Attack = false;

        //}

        //private async void KillPiece(object? pieceIndex, EventArgs e)
        //{
        //    PieceViewModel pieceToKill = (_field[(int)pieceIndex!] as PieceViewModel)!;
        //    pieceToKill.Vanish = true;
        //    await AnimatePiece(_field, (int)pieceIndex, 500);
        //    _field[(int)pieceIndex!] = new EmptyTileViewModel((EmptyTile)_board.Field.AsList()[(int)pieceIndex]);
        //    pieceToKill.Vanish = false;

        //    if (pieceToKill.IsOtherColor)
        //    {
        //        _enemyDeadPieces.Add(new PieceViewModel((Piece)_board.EnemyDeadPieces.Last()));
        //        (_enemyDeadPieces.Last() as PieceViewModel)!.Appear = true;
        //        await AnimatePiece(_enemyDeadPieces, _enemyDeadPieces.Count - 1, 500);
        //        (_enemyDeadPieces.Last() as PieceViewModel)!.Appear = false;

        //    }
        //    else
        //    {
        //        _deadPieces.Add(new PieceViewModel((Piece)_board.DeadPieces.Last()));
        //        (_deadPieces.Last() as PieceViewModel)!.Appear = true;
        //        await AnimatePiece(_deadPieces, _deadPieces.Count - 1, 500);
        //        (_deadPieces.Last() as PieceViewModel)!.Appear = false;

        //    }

        //}

        //private async void PiecesMoved(object? indexFromAndWhere, int indexTo)
        //{
        //    int indexFrom = (indexFromAndWhere as Tuple<int, bool>)!.Item1;
        //    bool dead = (indexFromAndWhere as Tuple<int, bool>)!.Item2;

        //    PieceViewModel selectedPiece;

        //    if (dead)
        //    {
        //        selectedPiece = (_deadPieces[indexFrom] as PieceViewModel)!;
        //        selectedPiece.Vanish = true;
        //        await AnimatePiece(_deadPieces, indexFrom, 500);
        //        selectedPiece.Vanish = false;

        //        _deadPieces[indexFrom] = _field[indexTo];
        //    }
        //    else
        //    {
        //        selectedPiece = (_field[indexFrom] as PieceViewModel)!;
        //        selectedPiece.Target = new((indexTo % 10 - indexFrom % 10) * s_pieceSize, (indexTo / 10 - indexFrom / 10) * s_pieceSize);
        //        await AnimatePiece(_field, indexFrom, 1000);
        //        selectedPiece.Target = null;

        //        _field[indexFrom] = _field[indexTo];
        //    }

        //    _field[indexTo] = selectedPiece;
        //    if (dead)
        //    {
        //        selectedPiece.Appear = true;
        //        await AnimatePiece(_field, indexTo, 500);
        //        selectedPiece.Appear = false;
        //    }
        //}

        private async Task WaitAnimatePiece(AvaloniaList<ITileableViewModel> list, int index, int animeationTime)
        {
            PieceViewModel temp = (PieceViewModel)list[index];
            list[index] = new EmptyTileViewModel(new EmptyTile(false, _board));
            list[index] = temp;
            await Task.Delay(animeationTime - 20);
        }

        private void StartConditionUpdated()
        {
            this.RaisePropertyChanged(nameof(_readyActive));
            this.RaisePropertyChanged(nameof(GameInPrep));
        }

        private void PropertyChangedOnPieces()
        {
            if (_board.State == Board.GameState.Prep)
            {
                foreach (ITileableViewModel t in DeadPieces)
                    if (t is PieceViewModel)
                        t.SelectedChanged();
            }
            
            foreach (ITileableViewModel t in Field)
            {
                if (t is PieceViewModel)
                    (t as PieceViewModel)!.HighLightedChanged();
                t.SelectedChanged();
            }
            
        }


    }
}
