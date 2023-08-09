using ChessChallenge.Chess;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.IO;
using static ChessChallenge.Application.UIHelper;
using ChessChallenge.Application.APIHelpers;

namespace ChessChallenge.Application {
    public class BoardUI {
      
        // Board settings
        const int squareSize = 100;
        const double moveAnimDuration = 0.15;
        bool whitePerspective = true;

        // Colour state
        Color topTextCol;
        Color bottomTextCol;

        // Drag state
        bool isDraggingPiece;
        int dragSquare;
        Vector2 dragPos;

        static readonly int[] pieceImageOrder = { 5, 3, 2, 4, 1, 0 };
        Texture2D piecesTexture;
        Dictionary<int, Color> squareColOverrides;
        Board board;
        Move lastMove;

        // Animate move state
        Board animateMoveTargetBoardState;
        Move moveToAnimate;
        double moveAnimStartTime;
        bool isAnimatingMove;


        public enum HighlightType {
            MoveFrom,
            MoveTo,
            LegalMove,
            Check,
            Premove,
        }


        public BoardUI() {
            LoadPieceTexture();

            board = new Board();
            board.LoadStartPosition();
            squareColOverrides = new Dictionary<int, Color>();
            topTextCol = Theme.InactivePlayerTextColor;
            bottomTextCol = Theme.InactivePlayerTextColor;
        }

        int[][] pieceCount;

        public void SetPerspective(bool whitePerspective) {
            this.whitePerspective = whitePerspective;
        }

        public void UpdatePosition(Board board, bool newGame = false) {
            if (newGame) {
                pieceCount = new int[][] {
                    new int[] {
                        board.pawns[0].Count,
                        board.knights[0].Count,
                        board.bishops[0].Count,
                        board.rooks[0].Count,
                        board.queens[0].Count,
                    },
                    new int[]  {
                        board.pawns[1].Count,
                        board.knights[1].Count,
                        board.bishops[1].Count,
                        board.rooks[1].Count,
                        board.queens[1].Count,
                    }
                };
            }

            isAnimatingMove = false;

            // Update
            this.board = new(board);
            lastMove = Move.NullMove;
            if (board.IsInCheck())
                OverrideSquareColour(board.KingSquare[board.MoveColourIndex], HighlightType.Check);
        }

        public void UpdatePosition(Board board, Move moveMade, bool animate = false) {
            if (moveMade.IsPromotion)
                if (this.board.IsWhiteToMove)
                    pieceCount[this.board.IsWhiteToMove ? 0 : 1][moveMade.PromotionPieceType - 1]++;

            // Interrupt prev animation
            if (isAnimatingMove) {
                UpdatePosition(animateMoveTargetBoardState);
                isAnimatingMove = false;
            }

            ResetSquareColours();
            if (animate) {
                OverrideSquareColour(moveMade.StartSquareIndex, HighlightType.MoveFrom);
                animateMoveTargetBoardState = new Board(board);
                moveToAnimate = moveMade;
                moveAnimStartTime = Raylib.GetTime();
                isAnimatingMove = true;
            } else {
                UpdatePosition(board);

                if (!moveMade.IsNull) {
                    HighlightMove(moveMade);
                    lastMove = moveMade;
                }
            }
        }

        void HighlightMove(Move move) {
            OverrideSquareColour(move.StartSquareIndex, HighlightType.MoveFrom);
            OverrideSquareColour(move.TargetSquareIndex, HighlightType.MoveTo);
        }

        public void DragPiece(int square, Vector2 worldPos) {
            isDraggingPiece = true;
            dragSquare = square;
            dragPos = worldPos;
        }

        public bool TryGetSquareAtPoint(Vector2 worldPos, out int squareIndex) {
            Vector2 boardStartPosWorld = new Vector2(squareSize, squareSize) * -4;
            Vector2 endPosWorld = boardStartPosWorld + new Vector2(8, 8) * squareSize;

            float tx = (worldPos.X - boardStartPosWorld.X) / (endPosWorld.X - boardStartPosWorld.X);
            float ty = (worldPos.Y - boardStartPosWorld.Y) / (endPosWorld.Y - boardStartPosWorld.Y);

            if (tx >= 0 && tx <= 1 && ty >= 0 && ty <= 1) {
                if (!whitePerspective) {
                    tx = 1 - tx;
                    ty = 1 - ty;
                }
                squareIndex = new Coord((int)(tx * 8), 7 - (int)(ty * 8)).SquareIndex;
                return true;
            }

            squareIndex = -1;
            return false;
        }

        public void OverrideSquareColour(int square, HighlightType hightlightType) {
            bool isLight = new Coord(square).IsLightSquare();

            Color col = hightlightType switch {
                HighlightType.MoveFrom => isLight ? Theme.MoveFromLightColor : Theme.MoveFromDarkColor,
                HighlightType.MoveTo => isLight ? Theme.MoveToLightColor : Theme.MoveToDarkColor,
                HighlightType.LegalMove => isLight ? Theme.LegalLightColor : Theme.LegalDarkColor,
                HighlightType.Check => isLight ? Theme.CheckLightColor : Theme.CheckDarkColor,
                HighlightType.Premove => isLight ? Theme.PremoveLightColor : Theme.PremoveDarkColor,
                _ => Color.PINK
            };

            if (squareColOverrides.ContainsKey(square))
                squareColOverrides[square] = col;
            else
                squareColOverrides.Add(square, col);
        }

        public void HighlightLegalMoves(Board board, int square) {
            MoveGenerator moveGenerator = new();
            var moves = moveGenerator.GenerateMoves(board);
            foreach (var move in moves)
                if (move.StartSquareIndex == square)
                    OverrideSquareColour(move.TargetSquareIndex, HighlightType.LegalMove);
        }

        public void Draw() {
            double animT = (Raylib.GetTime() - moveAnimStartTime) / moveAnimDuration;

            if (isAnimatingMove && animT >= 1) {
                isAnimatingMove = false;
                UpdatePosition(animateMoveTargetBoardState, moveToAnimate, false);
            }

            DrawBorder();
            ForEachSquare(DrawSquare);
            
            if (isAnimatingMove)
                UpdateMoveAnimation(animT);

            if (BitboardDebugState.BitboardDebugVisualizationRequested)
                ForEachSquare(DrawBitboardDebugOverlaySquare);

            if (isDraggingPiece)
                DrawPiece(board.Square[dragSquare], dragPos - new Vector2(squareSize * 0.5f, squareSize * 0.5f));
            else if (board.IsInCheck())
                OverrideSquareColour(board.KingSquare[board.MoveColourIndex], HighlightType.Check);


            // Reset state
            isDraggingPiece = false;
        }

        static void ForEachSquare(Action<int, int> action) {
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    action(x, y);
        }

        void UpdateMoveAnimation(double animT) {
            Coord startCoord = new Coord(moveToAnimate.StartSquareIndex);
            Coord targetCoord = new Coord(moveToAnimate.TargetSquareIndex);
            Vector2 startPos = GetSquarePos(startCoord.fileIndex, startCoord.rankIndex, whitePerspective);
            Vector2 targetPos = GetSquarePos(targetCoord.fileIndex, targetCoord.rankIndex, whitePerspective);

            Vector2 animPos = Vector2.Lerp(startPos, targetPos, (float)animT);
            DrawPiece(board.Square[moveToAnimate.StartSquareIndex], animPos);
        }

        public void DrawPlayerNames(string nameWhite, string nameBlack, int timeWhite, int timeBlack, bool isPlaying) {
            string nameBottom = whitePerspective ? nameWhite : nameBlack;
            string nameTop = !whitePerspective ? nameWhite : nameBlack;
            int timeBottom = whitePerspective ? timeWhite : timeBlack;
            int timeTop = !whitePerspective ? timeWhite : timeBlack;
            bool bottomTurnToMove = whitePerspective == board.IsWhiteToMove && isPlaying;
            bool topTurnToMove = whitePerspective != board.IsWhiteToMove && isPlaying;

            string colNameBottom = whitePerspective ? "White" : "Black";
            string colNameTop = !whitePerspective ? "White" : "Black";

            int boardStartX = -squareSize * 4;
            int boardStartY = -squareSize * 4;
            const int spaceY = 35;


            Color textTopTargetCol = topTurnToMove ? Theme.ActivePlayerTextColor : Theme.InactivePlayerTextColor;
            Color textBottomTargetCol = bottomTurnToMove ? Theme.ActivePlayerTextColor : Theme.InactivePlayerTextColor;

            float colLerpSpeed = 16;
            topTextCol = LerpColour(topTextCol, textTopTargetCol, Raylib.GetFrameTime() * colLerpSpeed);
            bottomTextCol = LerpColour(bottomTextCol, textBottomTargetCol, Raylib.GetFrameTime() * colLerpSpeed);

            //Color textColTop = topTurnToMove ? activeTextCol : inactiveTextCol;

            Draw(boardStartY + squareSize * 8 + spaceY, colNameBottom, nameBottom, timeBottom, bottomTextCol);
            Draw(boardStartY - spaceY, colNameTop, nameTop, timeTop, topTextCol);

            // Draw pieces value
            int top = getPiecesValue(!whitePerspective);
            int bottom = getPiecesValue(whitePerspective);

            int boardTopY = boardStartY + squareSize * 8 + spaceY + UIHelper.ScaleInt(50);
            int boardBottomY = boardStartY - spaceY - UIHelper.ScaleInt(50);

            UIHelper.DrawText(format(bottom, top), new Vector2(boardStartX, boardTopY), 36, 1, Theme.ActivePlayerTextColor, UIHelper.AlignH.Left);
            UIHelper.DrawText(format(top, bottom), new Vector2(boardStartX, boardBottomY), 36, 1, Theme.ActivePlayerTextColor, UIHelper.AlignH.Left);

            // Draw missing pieces
            int[][] missingPieces = new int[][]{
                getMissingPieces(!whitePerspective ? 1 : 0),
                getMissingPieces(whitePerspective ? 1 : 0)
            };

            int pieceSize = UIHelper.ScaleInt(75);
            int spacingXSamePiece = UIHelper.ScaleInt(20);
            int spacingXDifPiece = UIHelper.ScaleInt(50);

            for (int color = 0; color < 2; color++) {
                int missingPieceX = boardStartX + UIHelper.ScaleInt(70);

                for (int piece = 0; piece < missingPieces[color].Length; piece++) {
                    for (int pieceIndex = 0; pieceIndex < missingPieces[color][piece]; pieceIndex++) {
                        int type = PieceHelper.PieceType(piece + 1);
                        Rectangle srcRect = GetPieceTextureRect(type, color == 0);
                        Rectangle targRect = new(missingPieceX, (color == 0 ^ whitePerspective ? boardTopY : boardBottomY) - pieceSize/2, pieceSize, pieceSize);

                        Color tint = new(255, 255, 255, 255);
                        Raylib.DrawTexturePro(piecesTexture, srcRect, targRect, new Vector2(0, 0), 0, tint);

                        missingPieceX += spacingXSamePiece;
                    }

                    if (missingPieces[color][piece] != 0)
                        missingPieceX += spacingXDifPiece;
                }
            }

            string format(int val1, int val2) {
                if (val1 < val2) return "" + (val1 - val2);
                else return "+" + (val1 - val2);
            }

            int getPiecesValue(bool isWhite) {
                int value = 0;
                value += board.pawns[isWhite ? 0 : 1].Count * 1;
                value += board.bishops[isWhite ? 0 : 1].Count * 3;
                value += board.knights[isWhite ? 0 : 1].Count * 3;
                value += board.rooks[isWhite ? 0 : 1].Count * 5;
                value += board.queens[isWhite ? 0 : 1].Count * 9;
                return value;
            }

            int[] getMissingPieces(int isWhite) {
                int[] missing = new int[5];
                missing[0] = pieceCount[isWhite][0] - board.pawns[isWhite].Count;
                missing[1] = pieceCount[isWhite][1] - board.knights[isWhite].Count;
                missing[2] = pieceCount[isWhite][2] - board.bishops[isWhite].Count;
                missing[3] = pieceCount[isWhite][3] - board.rooks[isWhite].Count;
                missing[4] = pieceCount[isWhite][4] - board.queens[isWhite].Count;
                return missing;
            }

            void Draw(float y, string colName, string name, int timeMs, Color textCol) {
                const int fontSize = 36;
                const int fontSpacing = 1;
                var namePos = new Vector2(boardStartX, y);

                UIHelper.DrawText($"{colName}: {name}", namePos, fontSize, fontSpacing, Theme.PlayerNameColor);
                var timePos = new Vector2(boardStartX + squareSize * 8, y);
                string timeText;
                if (timeMs == Settings.MAX_TIME)
                    timeText = "Time: Unlimited";
                else {
                    double secondsRemaining = timeMs / 1000.0;
                    int numMinutes = (int)(secondsRemaining / 60);
                    int numSeconds = (int)(secondsRemaining - numMinutes * 60);
                    int dec = (int)((secondsRemaining - numMinutes * 60 - numSeconds) * 10);

                    timeText = $"Time: {numMinutes:00}:{numSeconds:00}.{dec}";
                }
                UIHelper.DrawText(timeText, timePos, fontSize, fontSpacing, textCol, UIHelper.AlignH.Right);
            }
        }

        public void ResetSquareColours(bool keepPrevMoveHighlight = false) {
            squareColOverrides.Clear();
            if (keepPrevMoveHighlight && !lastMove.IsNull)
                HighlightMove(lastMove);
        }


        void DrawBorder() {
            int boardStartX = -squareSize * 4;
            int boardStartY = -squareSize * 4;
            int w = 12;
            Raylib.DrawRectangle(boardStartX - w, boardStartY - w, 8 * squareSize + w * 2, 8 * squareSize + w * 2, Theme.BorderColor);
        }

        void DrawSquare(int file, int rank) {

            Coord coord = new Coord(file, rank);
            Color col = coord.IsLightSquare() ? Theme.LightColor : Theme.DarkColor;
            if (squareColOverrides.TryGetValue(coord.SquareIndex, out Color overrideCol))
                col = overrideCol;

            // top left
            Vector2 pos = GetSquarePos(file, rank, whitePerspective);
            Raylib.DrawRectangle((int)pos.X, (int)pos.Y, squareSize, squareSize, col);
            int piece = board.Square[coord.SquareIndex];
            float alpha = isDraggingPiece && dragSquare == coord.SquareIndex ? 0.3f : 1;
            if (!isAnimatingMove || coord.SquareIndex != moveToAnimate.StartSquareIndex)
                DrawPiece(piece, new Vector2((int)pos.X, (int)pos.Y), alpha);

            if (Settings.DisplayBoardCoordinates) {
                int textSize = 25;
                float xpadding = 5f;
                float ypadding = 2f;
                Color coordNameCol = coord.IsLightSquare() ? Theme.DarkCoordColor : Theme.LightCoordColor;

                if (rank == (whitePerspective ? 0 : 7))  {
                    string fileName = BoardHelper.fileNames[file] + "";
                    Vector2 drawPos = pos + new Vector2(xpadding, squareSize - ypadding);
                    DrawText(fileName, drawPos, textSize, 0, coordNameCol, AlignH.Left, AlignV.Bottom);
                }

                if (file == (whitePerspective ? 7 : 0)) {
                    string rankName = (rank + 1) + "";
                    Vector2 drawPos = pos + new Vector2(squareSize - xpadding, ypadding);
                    DrawText(rankName, drawPos, textSize, 0, coordNameCol, AlignH.Right, AlignV.Top);
                }
            }
        }

        void DrawBitboardDebugOverlaySquare(int file, int rank) {
            ulong bitboard = BitboardDebugState.BitboardToVisualize;
            bool isSet = BitBoardUtility.ContainsSquare(bitboard, new Coord(file,rank).SquareIndex);
            Color col = isSet ? Theme.BitboardColorONE : Theme.BitboardColorZERO;

            Vector2 squarePos = GetSquarePos(file, rank, whitePerspective);
            Raylib.DrawRectangle((int)squarePos.X, (int)squarePos.Y, squareSize, squareSize, col);
            Vector2 textPos = squarePos + new Vector2(squareSize, squareSize) / 2;
            DrawText(isSet ? "1" : "0", textPos, 50, 0, Color.WHITE, AlignH.Centre);
        }

        static Vector2 GetSquarePos(int file, int rank, bool whitePerspective) {
            const int boardStartX = -squareSize * 4;
            const int boardStartY = -squareSize * 4;

            if (!whitePerspective) {
                file = 7 - file;
                rank = 7 - rank;
            }

            int posX = boardStartX + file * squareSize;
            int posY = boardStartY + (7 - rank) * squareSize;
            return new Vector2(posX, posY);
        }

        void DrawPiece(int piece, Vector2 posTopLeft, float alpha = 1) {
            if (piece != PieceHelper.None) {
                int type = PieceHelper.PieceType(piece);
                bool white = PieceHelper.IsWhite(piece);
                Rectangle srcRect = GetPieceTextureRect(type, white);
                Rectangle targRect = new Rectangle((int)posTopLeft.X, (int)posTopLeft.Y, squareSize, squareSize);

                Color tint = new Color(255, 255, 255, (int)MathF.Round(255 * alpha));
                Raylib.DrawTexturePro(piecesTexture, srcRect, targRect, new Vector2(0, 0), 0, tint);
            }
        }


        static Color LerpColour(Color a, Color b, float t) {
            int newR = (int)(Math.Round(Lerp(a.r, b.r, t)));
            int newG = (int)(Math.Round(Lerp(a.g, b.g, t)));
            int newB = (int)(Math.Round(Lerp(a.b, b.b, t)));
            int newA = (int)(Math.Round(Lerp(a.a, b.a, t)));
            return new Color(newR, newG, newB, newA);

            float Lerp(float a, float b, float t) {
                t = Math.Min(1, Math.Max(t, 0));
                return a + (b - a) * t;
            }
        }

        void LoadPieceTexture() {
            // Workaround for Raylib.LoadTexture() not working when path contains non-ascii chars
            byte[] pieceImgBytes = File.ReadAllBytes(FileHelper.GetResourcePath("Pieces.png"));
            Image pieceImg = Raylib.LoadImageFromMemory(".png", pieceImgBytes);
            piecesTexture = Raylib.LoadTextureFromImage(pieceImg);
            Raylib.UnloadImage(pieceImg);

            Raylib.GenTextureMipmaps(ref piecesTexture);
            Raylib.SetTextureWrap(piecesTexture, TextureWrap.TEXTURE_WRAP_CLAMP);
            Raylib.SetTextureFilter(piecesTexture, TextureFilter.TEXTURE_FILTER_BILINEAR);
        }

        public void Release() {
            Raylib.UnloadTexture(piecesTexture);
        }

        static Rectangle GetPieceTextureRect(int pieceType, bool isWhite) {
            const int size = 333;
            return new Rectangle(size * pieceImageOrder[pieceType - 1], isWhite ? 0 : size, size, size);
        }
    }
}