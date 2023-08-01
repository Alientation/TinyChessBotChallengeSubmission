using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Numerics;

/*
    MyBot V1.0  ~(637 Brain Power SMH)

    Features
    Min Max Algorithm
    Alpha Beta Pruning
*/

/*
    TODO

    Score board based off piece locations
    sort moves when getting possible moves
    add more features to board evaluation
        - pawn advancement
        - piece mobility
        - piece threats
        - piece protection

    add more features to move scoring
    add game stage
    Null Move Heuristic (find eval of opponent moving two times in a row to get the minimum alpha value)
    Quiesence Searching (only applies to moves that result in a capture)
    Optimized Searching (go down only promising paths)
    OPTIMIZE CODE
    Move Pruning
    Late Move Reductions, History Reductions
    Use NegaMax??
    Store minimax traversals (edges that haven't been searched from a particular board state)
    
*/
public class MyBotV1_3 : IChessBot {

    bool isBotWhite;
    int gameStage = 1;
    

    public Move Think(Board board, Timer timer) {
        isBotWhite = board.IsWhiteToMove;
        Move bestMove = negamax(board, 6, -30000, 30000, 1).Item1;
        return bestMove;
    }

    public (Move, int) negamax(Board board, int depthLeft, int alpha, int beta, int color) {
        if (depthLeft == 0) {
            return (Move.NullMove, color * evaluate(board));
        }

        if (board.IsDraw()) {
            return (Move.NullMove, 0);
        }
        if (board.IsInCheckmate()) {
            return (Move.NullMove, -30000 * color);
        }

        Move bestMove = Move.NullMove;
        Move[] moves = getOrderedMoves(board);
        if (moves.Length == 0)
            return (bestMove, color * evaluate(board));

        int bestEval = -30000;
        foreach (Move move in moves) {
            board.MakeMove(move);
            int eval = -negamax(board, depthLeft - 1, -beta, -alpha, -color).Item2;
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


    public int evaluate(Board board) {
        int eval = board.IsInCheck() ^ (isBotWhite == board.IsWhiteToMove) ? 0 : -100;
        var pieceList = new ulong[] {
            board.GetPieceBitboard(PieceType.Pawn, true),
            board.GetPieceBitboard(PieceType.Bishop, true),
            board.GetPieceBitboard(PieceType.Knight, true),
            board.GetPieceBitboard(PieceType.Rook, true),
            board.GetPieceBitboard(PieceType.Queen, true),
            board.GetPieceBitboard(PieceType.King, true),
            board.GetPieceBitboard(PieceType.Pawn, false),
            board.GetPieceBitboard(PieceType.Bishop, false),
            board.GetPieceBitboard(PieceType.Knight, false),
            board.GetPieceBitboard(PieceType.Rook, false),
            board.GetPieceBitboard(PieceType.Queen, false),
            board.GetPieceBitboard(PieceType.King, false),
        };
        for (int i = 0; i < pieceList.Length; i++) {
            //eval += piecesValueFunctions[i % 5](pieceList[i]) * (i < 6 == isBotWhite ? 1 : -1);
            int piecesEval = 0;
            while (pieceList[i] != 0) {
                piecesEval += pieceValue[i % 5];
                int index = BitOperations.TrailingZeroCount(pieceList[i]);
                pieceList[i] &= pieceList[i] - 1;
            }
            eval += piecesEval * (i < 6 == isBotWhite ? 1 : -1);
        }
        return eval;
    }

    public Move[] getOrderedMoves(Board board) {
        return board.GetLegalMoves();
    }


    /*delegate int PiecesValueFunction(ulong pieceBitboard);
    public static int Pawns(ulong bitboardPawns) {
        int score = 0;
        while (bitboardPawns != 0) {
            score += 100;
            int index = BitOperations.TrailingZeroCount(bitboardPawns);
            bitboardPawns &= bitboardPawns - 1;
        }
        return score;
    }
    public static int Bishops(ulong bitboardBishops) {
        int score = 0;
        while (bitboardBishops != 0) {
            score += 300;
            int index = BitOperations.TrailingZeroCount(bitboardBishops);
            bitboardBishops &= bitboardBishops - 1;
        }
        return score;
    }
    public static int Knights(ulong bitboardKnights) {
        int score = 0;
        while (bitboardKnights != 0) {
            score += 300;
            int index = BitOperations.TrailingZeroCount(bitboardKnights);
            bitboardKnights &= bitboardKnights - 1;
        }
        return score;
    }
    public static int Rooks(ulong bitboardRooks) {
        int score = 0;
        while (bitboardRooks != 0) {
            score += 500;
            int index = BitOperations.TrailingZeroCount(bitboardRooks);
            bitboardRooks &= bitboardRooks - 1;
        }
        return score;
    }
    public static int Queens(ulong bitboardQueens) {
        int score = 0;
        while (bitboardQueens != 0) {
            score += 900;
            int index = BitOperations.TrailingZeroCount(bitboardQueens);
            bitboardQueens &= bitboardQueens - 1;
        }
        return score;
    }
    public static int Kings(ulong bitboardKings) {
        int score = 0;
        while (bitboardKings != 0) {
            int index = BitOperations.TrailingZeroCount(bitboardKings);
            bitboardKings &= bitboardKings - 1;
        }
        return score;
    }
    PiecesValueFunction[] piecesValueFunctions = new PiecesValueFunction[] {
        Pawns,
        Bishops,
        Knights,
        Rooks,
        Queens,
        Kings
    };*/

    public void GameOver() {
        
    }
}