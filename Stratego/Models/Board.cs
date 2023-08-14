using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using DynamicData;
using MessageBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stratego.Models
{
    class Board
    {
        public enum GameState
        {
            Prep,
            Game
        }
        private readonly Client _client;
        private readonly int _playerNumber;
        private GameState _state = GameState.Prep;
        private List<ITileable> _field = new List<ITileable>(BOARD_SIZE);
        private List<ITileable> _deadPieces = new List<ITileable>(PIECE_COUNT);
        private List<ITileable> _enemyDeadPieces = new List<ITileable>(PIECE_COUNT);
        private Piece.Color _color;
        private byte[]? _enemyPieceData = null;

        private Piece? _selectedPiece = null;
        private Piece? _highlightedPiece = null;

        public GameState State => _state;
        public IEnumerable<ITileable> Field => _field;
        public IEnumerable<ITileable> DeadPieces => _deadPieces;
        public IEnumerable<ITileable> EnemyDeadPieces => _enemyDeadPieces;
        public int DeadPiecesCount => _deadPieces.Where(t => t is Piece).Count();
        public bool EnemyPieceDataRecieved => _enemyPieceData is not null;

        public bool YourTurn { get; private set; } = true;

        public event Action PiecesChanged = null!;
        public event Action ListsChanged = null!;
        public event Action StartConditionUpdated = null!;
        public event EventHandler<int> PieceAttack = null!;
        public event EventHandler PieceDie = null!;
        public event EventHandler<int> PiecesMoved = null!;
        
        const int PIECE_COUNT = 40;
        const int BOARD_SIZE = 100;
        static Vector2[] s_neighborVectors =
        {
            new Vector2(-1, 0),
            new Vector2(0, -1),
            new Vector2(1, 0),
            new Vector2(0, 1)
        };
        static Piece.Type[] s_pieces = new Piece.Type[]
        {
            Piece.Type.Flag,
            Piece.Type.Spy,
            Piece.Type.Scout,
            Piece.Type.Scout,
            Piece.Type.Scout,
            Piece.Type.Scout,
            Piece.Type.Scout,
            Piece.Type.Scout,
            Piece.Type.Scout,
            Piece.Type.Scout,
            Piece.Type.Miner,
            Piece.Type.Miner,
            Piece.Type.Miner,
            Piece.Type.Miner,
            Piece.Type.Miner,
            Piece.Type.Sergeant,
            Piece.Type.Sergeant,
            Piece.Type.Sergeant,
            Piece.Type.Sergeant,
            Piece.Type.Lieutenant,
            Piece.Type.Lieutenant,
            Piece.Type.Lieutenant,
            Piece.Type.Lieutenant,
            Piece.Type.Captain,
            Piece.Type.Captain,
            Piece.Type.Captain,
            Piece.Type.Captain,
            Piece.Type.Major,
            Piece.Type.Major,
            Piece.Type.Major,
            Piece.Type.Colonel,
            Piece.Type.Colonel,
            Piece.Type.General,
            Piece.Type.Marshal,
            Piece.Type.Bomb,
            Piece.Type.Bomb,
            Piece.Type.Bomb,
            Piece.Type.Bomb,
            Piece.Type.Bomb,
            Piece.Type.Bomb
        };
        static int[] s_excludedTiles =
        {
            52, 
            53, 
            56, 
            57,
            42,
            43,
            46,
            47
        };

        public Board(Piece.Color color, Client client, int playerNumber)
        {
            _playerNumber = playerNumber;
            _color = color;
            _client = client;
            InitializeBoard(true);
        }



        /// <summary>
        /// Figures out where the selected piece can move to, and highlights those tiles
        /// </summary>
        /// <param name="piece"> The piece that was selected </param>
        public void Select(Piece piece)
        {
            UnselectPrev();
            _selectedPiece = piece;
            // first part, for when game is in prep phase
            if (_state == GameState.Prep)
            {
                // loops over your half
                for (int i = 50; i < 100; i++)
                {
                    // skips if coord is not a tile
                    if (_field[i] is not EmptyTile)
                        continue;

                    // gets the coordinate for easier use in the ifs
                    Vector2 coordinates = GetCoordinates(_field[i]);
                    // skips if we are over the water
                    if (coordinates.Y == 5 && (coordinates.X == 2 || coordinates.X == 3 || coordinates.X == 6 || coordinates.X == 7))
                        continue;
                    // skips if you have chosen bombs and the tiles are the first two rows of your half
                    if (piece.IsBomb && coordinates.Y <= 6)
                        continue;

                    // sets the tile as selectable
                    (_field[i] as EmptyTile)!.IsSelectable = true;

                }
            }
            // for when the game has started
            else
            {

                Vector2 coord = GetCoordinates(piece);

                // Scouts move differently from everything else
                if ((Piece.Type)piece.GetType == Piece.Type.Scout)
                {
                    foreach (Vector2 v in s_neighborVectors)
                    {
                        for (Vector2 coordTemp = coord + v; TileOk(coordTemp); coordTemp += v)
                        {
                            if (!CheckAndChangeColor(coordTemp))
                                break;
                        }
                    }
                }
                else
                    foreach (Vector2 n in GetNeighbors(coord))
                        CheckAndChangeColor(n);
            }
            PiecesChanged.Invoke();
        }

        /// <summary>
        /// Arranges all your pieces randomly, currently only works when no pieces have been moved
        /// </summary>
        public void Chaos()
        {
            if (DeadPiecesCount != 40)
                return;

            List<int> exclusive = s_excludedTiles.ToList();
            Random random = new();
            Queue<int> queue = new();



            IEnumerable<int> temp = Enumerable.Range(70, 30);
            foreach (int i in temp.OrderBy(item => random.Next()))
                queue.Enqueue(i);

            for (int i = 39; i >= 34; i--)
            {
                int index = queue.Dequeue();
                exclusive.Add(index);
                (_deadPieces[i] as Piece)!.Select();
                (_field[index] as EmptyTile)!.Move();
            }

            queue.Clear();
            temp = Enumerable.Range(50, 50);
            foreach (int i in temp.OrderBy(item => random.Next()))
                queue.Enqueue(i);

            for (int i = 0; i < 34; i++)
            {
                int index;
                while (true)
                {
                    index = queue.Dequeue();
                    if (exclusive.Contains(index))
                        continue;
                    break;
                }
                (_deadPieces[i] as Piece)!.Select();
                (_field[index] as EmptyTile)!.Move();
            }
        }

        public void Move(EmptyTile t)
        {
            if (_selectedPiece is null)
                return;
            if (_state == GameState.Prep)
                StartConditionUpdated.Invoke();
            Move(_selectedPiece, t);
        }
        /// <summary>
        /// The function that moves a piece to a tile
        /// only happens when a piece is selected and a selectable tile was clicked
        /// </summary>
        /// <param name="t"> the tile the selected piece is moving to </param>
        public void Move(Piece p, EmptyTile t)
        {
            UnselectPrev();
            int indexTo = _field.IndexOf(t);

            int indexFrom;

            if (p.Dead)
            {
                indexFrom = _deadPieces.IndexOf(p);
                _deadPieces[indexFrom] = _field[indexTo];
            }
            else
            {
                indexFrom = _field.IndexOf(p);
                _field[indexFrom] = _field[indexTo];
            }

            _field[indexTo] = p;

            Tuple<int, bool> indexFromAndWhere = new(indexFrom, p.Dead);
            p.Dead = false;


            PiecesChanged.Invoke();
            PiecesMoved.Invoke(indexFromAndWhere, indexTo);
            if (_state == GameState.Game && YourTurn)
                SendMove(new byte[] { (byte)indexFrom, (byte)indexTo});
        }

        public async void OponentMove(byte[] data)
        {
            UnHighlightEnemy();
            int indexFrom = 99 - data[0];
            int indexTo = 99 - data[1];

            if (_field[indexTo] is EmptyTile)
                Move((_field[indexFrom] as Piece)!, (_field[indexTo] as EmptyTile)!);
            else
                await Attacked((_field[indexFrom] as Piece)!, (_field[indexTo] as Piece)!);
            YourTurn = true;
        }

        public async Task Attacked(Piece defender)
        {
            if (_selectedPiece is null)
                return;
            await Attacked(_selectedPiece, defender);
        }
        public async Task Attacked(Piece attacker, Piece defender)
        {
            UnselectPrev();
            PiecesChanged.Invoke();
            if (YourTurn)
                SendMove(new byte[] { (byte)_field.IndexOf(attacker), (byte)_field.IndexOf(defender) });

            PieceAttack.Invoke(_field.IndexOf(attacker), (byte)_field.IndexOf(defender));
            // wait for attack/move animation + ekstra buffer
            await Task.Delay(1200);
            
            if (((Piece.Type)attacker.GetType == Piece.Type.Miner && (Piece.Type)defender.GetType == Piece.Type.Bomb)
                || ((Piece.Type)attacker.GetType == Piece.Type.Spy && (Piece.Type)defender.GetType == Piece.Type.Marshal)
                || (attacker.GetType > defender.GetType))
            {
                if (!YourTurn)
                    HighlightEnemy(attacker);
                await KillAndMove(attacker, defender);
            }
            else if (attacker.GetType == defender.GetType)
            {
                await Kill(defender);
                await Kill(attacker);
            }
            else
            {
                HighlightEnemy(defender);
                PiecesChanged.Invoke();
                await Kill(attacker);
            }

        }

        /// <summary>
        /// clears the lists generates them anew
        /// </summary>
        public void InitializeBoard(bool firstTime = false)
        {
            _field.Clear();
            _deadPieces.Clear();
            for (int i = 0; i < PIECE_COUNT; i++)
            {
                _deadPieces.Add(new Piece(s_pieces[i], _color, this, _color));
            }
            for (int i = 0; i < BOARD_SIZE; i++)
                _field.Add(new EmptyTile(false, this));

            if (firstTime)
                return;

            ListsChanged.Invoke();
            StartConditionUpdated.Invoke();
        }

        /// <summary>
        /// Function that starts the game and creates enemy field from data
        /// </summary>
        public void StartGame()
        {
            if (_enemyPieceData is null)
                return;
            if (_playerNumber == 0)
                YourTurn = true;
            else
                ThreadPool.QueueUserWorkItem(GetMove, null);

            _deadPieces.Clear();
            ConstructEnemyField(_enemyPieceData);
            _state = GameState.Game;
            ListsChanged.Invoke();
        }

        public void Ready()
        {
            YourTurn = false;
            StartConditionUpdated.Invoke();
            byte[] temp = SerializeField();
            _client.SendField(temp);

            ThreadPool.QueueUserWorkItem(GetEnemyField, null);
        }

        private async Task<int> Kill(Piece p)
        {
            p.Dead = true;
            int index = _field.IndexOf(p);

            if (p.IsOtherColor)
            {
                p.CanBeTargeted = false;
                p.Highlighted = true;
                _enemyDeadPieces.Add(p);
            }
            else
                _deadPieces.Add(p);

            _field[index] = new EmptyTile(false, this);

            PieceDie.Invoke(index, null!);
            await Task.Delay(1200);
            return index;
        }

        private async Task KillAndMove(Piece attacker, Piece defender)
        {
            int index = await Kill(defender);
            Move(attacker, (_field[index] as EmptyTile)!);
        }

        /// <summary>
        /// unselects the previously selected piece, happens every time a piece is clicked or moved
        /// when a piece is unselected, the highlighted tiles should also stop being highlighted
        /// </summary>
        private void UnselectPrev()
        {
            if (_selectedPiece is null)
                return;
            _selectedPiece.Unselect();
            _selectedPiece = null;

            UnhighlightTiles();
        }

        /// <summary>
        /// unhighlights tiles, also happens every time a piece is clicked
        /// </summary>
        private void UnhighlightTiles()
        {
            foreach (ITileable t in _field)
                if (t is EmptyTile)
                    (t as EmptyTile)!.IsSelectable = false;
                else
                    (t as Piece)!.CanBeTargeted = false;
        }

        private void UnHighlightEnemy()
        {
            if (_highlightedPiece is null)
                return;
            _highlightedPiece.Highlighted = false;
            _highlightedPiece = null;
        }

        private void HighlightEnemy(Piece piece)
        {
            _highlightedPiece = piece;
            piece.Highlighted = true;
        }

        /// <summary>
        /// Gets all the pieces location in the form of a byte array, where each byte represents the type of a piece.
        /// </summary>
        /// <returns> returns the byte array data </returns>
        private byte[] SerializeField()
        {
            byte[] data = new byte[50]; 

            for (int i = 50; i < 100; i++)
                data[i - 50] = _field[i].GetType;

            return data;
        }


        /// <summary>
        /// Creates the enemy field from a byte array
        /// </summary>
        /// <param name="data"> byte array containing data on piece placement </param>
        private void ConstructEnemyField(byte[]? data)
        {
            if (data is null)
                return;

            for (int i = 49; i >= 0; i--)
            {
                byte type = data[49 - i];
                if (type == 12)
                    _field[i] = new EmptyTile(false, this);
                else
                    _field[i] = new Piece((Piece.Type)type, Piece.OtherColor(_color), this, _color) { Dead = false };
            }
        }

        /// <summary>
        /// gets x- and y-coordinates and arranges them in a Vector2
        /// </summary>
        /// <param name="t"> the tile to get coordinates from </param>
        private Vector2 GetCoordinates(ITileable t)
        {
            int index = _field.IndexOf(t);
            return new(index % 10, index / 10);
        }

        /// <summary>
        /// function that converts a vector2 coord to index of field
        /// </summary>
        /// <param name="coord"> the coord </param>
        /// <returns></returns>
        private int CoordToIndex(Vector2 coord)
        {
            return (int)coord.Y * 10 + (int)coord.X;
        }

        /// <summary>
        /// Function to find the neighbors if the exist
        /// </summary>
        /// <param name="coord"> point to find neighbors of </param>
        /// <returns></returns>
        private IEnumerable<Vector2> GetNeighbors(Vector2 coord)
        {
            List<Vector2> neighbors = new List<Vector2>();

            for (int i = 0; i < 4; i++)
            {
                Vector2 tempN = s_neighborVectors[i] + coord;
                if (TileOk(tempN))
                    neighbors.Add(tempN);
            }
            return neighbors;
        }

        private bool TileOk(Vector2 coordTemp)
        {
            return coordTemp.X >= 0 && coordTemp.Y >= 0 && coordTemp.X < 10 && coordTemp.Y < 10 && !s_excludedTiles.ToList().Contains(CoordToIndex(coordTemp));
        }

        private bool CheckAndChangeColor(Vector2 v)
        {
            if (_field[CoordToIndex(v)] is Piece)
            {
                if ((_field[CoordToIndex(v)] as Piece)!.IsOtherColor)
                    (_field[CoordToIndex(v)] as Piece)!.CanBeTargeted = true;
                return false;
            }
            else if (_field[CoordToIndex(v)] is EmptyTile)
                (_field[CoordToIndex(v)] as EmptyTile)!.IsSelectable = true;
            return true;
        }

        private async void GetEnemyField(object? obj)
        {
            _enemyPieceData = await _client.GetField();
            Dispatcher.UIThread.Post(() => StartGame());
        }

        private async void GetMove(object? obj)
        {
            byte[] temp = await _client.GetMove();
            Dispatcher.UIThread.Post(() => OponentMove(temp));
        }

        private void SendMove(byte[] move)
        {
            _client.SendMove(move);
            YourTurn = false;
            ThreadPool.QueueUserWorkItem(GetMove, null);
        }

    }
}