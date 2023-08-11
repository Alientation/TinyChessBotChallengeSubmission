using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Numerics;

/*
    MyBot V1.3  ~(637 Brain Power)

    Features (dif from previous version 1.2)
*/

public class MyBotV1_3 : IChessBot {
    bool isBotWhite;
    int timePerMove = 500;
    Timer cTimer;

    public Move Think(Board board, Timer timer) {
        if (cTimer == null) {
            isBotWhite = board.IsWhiteToMove;
            if (!isBotWhite) {
                for (int i = 0; i < pieceValueLocation.Length; i++) {
                    Array.Reverse(pieceValueLocation[i]);
                }
            }
        }
        cTimer = timer;
        var bestMove = (Move.NullMove,0);

        #if DEBUG
        Console.WriteLine("eval " + evaluate(board, 0));
        #endif
        
        for (int depth = 1; depth <= 50 && timer.MillisecondsElapsedThisTurn < timePerMove; depth++) {
            var move = negamax(board, depth, 0, -300000, 300000, 1);
            if (move.Item1 != Move.NullMove) bestMove = move;
            #if DEBUG
            Console.WriteLine(move.Item1 + " " + move.Item2);
            Console.WriteLine("d" + depth + " -> " + move.Item1 + " " + move.Item2 + "  (" + timer.MillisecondsElapsedThisTurn + " ms)");
            #endif
        }

        #if DEBUG
        Console.WriteLine("Chosen Move " + bestMove.Item1 + " (" + bestMove.Item2 + ")"); 
        #endif
        return bestMove.Item1;
    }

    public (Move, int) negamax(Board board, int depthLeft, int depth, int alpha, int beta, int color) {
        if (depthLeft == 0 || board.IsInCheckmate() || board.IsInStalemate() || board.IsFiftyMoveDraw() || board.IsInsufficientMaterial() || cTimer.MillisecondsElapsedThisTurn > timePerMove) {
            return (Move.NullMove, evaluate(board, depth));
        }

        Move bestMove = Move.NullMove;
        Move[] moves = getOrderedMoves(board);

        int bestEval = -200000;
        foreach (Move move in moves) {
            board.MakeMove(move);
            int eval = evaluateMove(move);

            eval -= negamax(board, depthLeft - 1, depth+1, -beta, -alpha, -color).Item2;
            board.UndoMove(move);

            if (eval > bestEval) {
                bestEval = eval;
                bestMove = move;
            }

            alpha = Math.Max(alpha, eval);
            if (alpha >= beta) {
                break;
            }
        }
        return (bestMove, bestEval);
    }

    int[] pieceValue = new int[] {
        0,100,300,300,500,900,0
    };

    public int evaluate(Board board, int depth) {
        if (board.IsDraw()) return -100;
        if (board.IsInCheckmate()) return  board.IsWhiteToMove == isBotWhite ? -100000 : 100000;

        int eval = 20 - ((depth >> 1) << 4);
        if (board.IsInCheck()) eval -= 5;
        bool flag = true;
        for (int i = 2; i < 13; i++) {
            int piecesEval = 0;
            ulong piecesBitboard = board.GetPieceBitboard((PieceType) (i >> 1), flag);
            while (piecesBitboard != 0) {
                int index = BitboardHelper.ClearAndGetIndexOfLSB(ref piecesBitboard);
                piecesEval += pieceValue[(i >> 1)] + pieceValueLocation[((i >> 1) - 1) * 3 + getGameStage(board)][index];
            }
            eval += piecesEval * (flag == board.IsWhiteToMove ? 1 : -1);
            flag = !flag;
        }
        return eval;
    }

    public int evaluateMove(Move move) {
        if (move.IsCapture) return ((int)move.CapturePieceType - (int)move.MovePieceType) * 10;
        if (move.IsCastles) return 40;
        if (move.IsEnPassant) return 20;
        if (move.IsPromotion) return 20;
        return 0;
    }

    public Move[] getOrderedMoves(Board board) {
        return board.GetLegalMoves();
    }

    public int getGameStage(Board board) {
        int pieceCount = BitOperations.PopCount(board.AllPiecesBitboard);
        int score = pieceCount + board.PlyCount >> 2;
        if (score < 20) return 2;
        if (score < 40) return 1;
        return 0;
    }

    int[][] pieceValueLocation = new int[][] {
        new int[] { //pawn
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            05,05,05,06,06,05,05,05,
            20,21,25,30,30,25,21,20,
            10,11,15,20,20,15,11,10,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            05,05,15,16,16,15,15,15,
            05,05,20,20,20,25,21,20,
            09,07,15,10,10,15,11,15,
            30,30,10,05,05,10,10,10,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            40,41,35,30,30,35,41,40,
            20,21,25,20,20,25,21,20,
            10,11,15,10,10,15,11,10,
            30,30,10,05,05,10,10,10,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },


        new int[] { //knight
            00,00,00,00,00,00,00,00,
            00,05,05,05,05,05,05,00,
            00,05,07,07,07,07,05,00,
            00,05,09,09,09,09,05,00,
            00,05,10,12,12,10,05,00,
            00,05,15,10,10,15,05,00,
            00,05,05,05,05,05,05,00,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,05,05,05,05,05,05,00,
            00,05,15,10,10,15,05,00,
            00,05,15,15,15,15,05,00,
            00,05,15,15,15,15,05,00,
            00,05,15,10,10,15,05,00,
            00,05,05,05,05,05,05,00,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,05,05,05,05,00,00,
            00,05,10,10,10,10,05,00,
            05,10,15,15,15,15,10,05,
            00,10,15,15,15,15,10,00,
            00,10,15,15,15,15,10,00,
            00,10,15,15,15,15,10,00,
            00,05,05,05,05,05,05,00,
            00,00,00,00,00,00,00,00,
        },


        new int[] { //bishop
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },


        new int[] { //rook
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },


        new int[] { //queen
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },


        new int[] { //king
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
        },
    };

    public void GameOver() {
        
    }
}