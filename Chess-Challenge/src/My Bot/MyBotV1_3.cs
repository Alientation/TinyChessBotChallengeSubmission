using ChessChallenge.API;
using System;
using System.Collections.Generic;

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
        Move bestMove = negamax(board, 2, -30000, 30000, 1).Item1;
        return bestMove;
    }

    public (Move, int) negamax(Board board, int depthLeft, int alpha, int beta, int color) {
        if (depthLeft == 0) {
            return (Move.NullMove, color * evaluate(board));
        }

        Move bestMove = Move.NullMove;
        Move[] moves = getOrderedMoves(board);
        if (moves.Length == 0)
            return (bestMove, color * evaluate(board));

        int bestScore = -30000;
        foreach (Move move in moves) {
            board.MakeMove(move);
            int score = -negamax(board, depthLeft - 1, -beta, -alpha, -color).Item2;
            board.UndoMove(move);

            if (score > bestScore) {
                bestScore = score;
                bestMove = move;
            }

            alpha = Math.Max(alpha, score);
            if (alpha >= beta) {
                break;
            }
        }

        return (bestMove, bestScore);
    }

    

    public int evaluate(Board board) {
        int eval = 0;
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
            eval += piecesValueFunctions[i % 5](pieceList[i]) * (i < 6 == isBotWhite ? 1 : -1);
        }
        return eval;
    }

    public Move[] getOrderedMoves(Board board) {
        return board.GetLegalMoves();
    }


    delegate int PiecesValueFunction(ulong pieceBitboard);
    public static int Pawns(ulong bitboardPawns) {
        return 100;
    }
    public static int Bishops(ulong bitboardBishops) {
        return 300;
    }
    public static int Knights(ulong bitboardKnights) {
        return 300;
    }
    public static int Rooks(ulong bitboardRooks) {
        return 500;
    }
    public static int Queens(ulong bitboardQueens) {
        return 900;
    }
    public static int Kings(ulong bitboardKings) {
        return 0;
    }
    PiecesValueFunction[] piecesValueFunctions = new PiecesValueFunction[] {
        Pawns,
        Bishops,
        Knights,
        Rooks,
        Queens,
        Kings
    };

    public void GameOver() {
        
    }
}