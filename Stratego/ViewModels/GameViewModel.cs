﻿using Avalonia.Media.Imaging;
using Stratego.Models;
using System.Collections.Generic;
using System.IO;
using Avalonia.Collections;
using System;
using Avalonia.Controls.Mixins;
using Avalonia.Controls;
using ReactiveUI;

namespace Stratego.ViewModels
{
    class GameViewModel : ViewModelBase
    {

        private readonly Board _board;
        private readonly Client _client = new Client();

        private AvaloniaList<ITileableViewModel> _field = new();
        private AvaloniaList<ITileableViewModel> _enemyDeadPieces = new();
        private AvaloniaList<ITileableViewModel> _deadPieces = new();

        public Bitmap Background => s_background;
        public bool GameInPrep => _board.State == Board.GameState.Prep;
        public IEnumerable<ITileableViewModel> Field => _field;
        public IEnumerable<ITileableViewModel> EnemyDeadPieces => _enemyDeadPieces;
        public IEnumerable<ITileableViewModel> DeadPieces => _deadPieces;

        public GameViewModel()
        {
            InitializeLists();
            _board.PiecesChanged += PropertyChangedOnPieces;
            _board.PiecesMoved += PiecesMoved;
            _board.ListsChanged += ReloadAll;
        }

        public void PiecesMoved(object? indexFromAndWhere, int indexTo)
        {
            int indexFrom = (indexFromAndWhere as Tuple<int, bool>)!.Item1;
            bool dead = (indexFromAndWhere as Tuple<int, bool>)!.Item2;

            PieceViewModel selectedPiece;

            if (dead)
            {
                selectedPiece = (_deadPieces[indexFrom] as PieceViewModel)!;
                _deadPieces[indexFrom] = new TileViewModel(new Tile(true, _board));
            }
            else
            {
                selectedPiece = (_field[indexFrom] as PieceViewModel)!;
                _field[indexFrom] = _field[indexTo];
            }

            _field[indexTo] = selectedPiece;
        }

        public void EnemyMove()
        {
            _board.OponentMove(new byte[]{49, 59});
        }

        public void Chaos()
        {
            _board.Chaos();
        }

        public void Enemy()
        {
            _board.EnemyChaos();
        }

        public void Clear()
        {
            _board.InitializeBoard();
        }

        public void Start()
        {
            _board.StartGame();
            this.RaisePropertyChanged(nameof(GameInPrep));
            ReloadField();
        }

        private void ReloadAll()
        {
            ReloadField();
            ReloadDead();
        }

        private void ReloadDead()
        {
            _deadPieces.Clear();
            _enemyDeadPieces.Clear();

            foreach (ITileable t in _board.DeadPieces)
                if (t is not Piece)
                    _deadPieces.Add(new TileViewModel((t as Tile)!));
                else
                    _deadPieces.Add(new PieceViewModel((t as Piece)!));

            foreach (ITileable t in _board.EnemyDeadPieces)
                if (t is not Piece)
                    _enemyDeadPieces.Add(new TileViewModel((t as Tile)!));
                else
                    _enemyDeadPieces.Add(new PieceViewModel((t as Piece)!));
        }

        private void ReloadField()
        {
            _field.Clear();

            foreach (ITileable t in _board.Field)
                if (t is not Piece)
                    _field.Add(new TileViewModel((t as Tile)!));
                else
                    _field.Add(new PieceViewModel((t as Piece)!));
        }

        private void InitializeLists()
        {
            foreach (ITileable t in _board.Field)
                if (t is not Piece)
                    _field.Add(new TileViewModel((t as Tile)!));
                else
                    _field.Add(new PieceViewModel((t as Piece)!));

            foreach (ITileable t in _board.DeadPieces)
                if (t is not Piece)
                    _deadPieces.Add(new TileViewModel((t as Tile)!));
                else
                    _deadPieces.Add(new PieceViewModel((t as Piece)!));
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

        static Bitmap s_background = new Bitmap(Directory.GetCurrentDirectory() + "/Assets/plateau.png");

    }
}