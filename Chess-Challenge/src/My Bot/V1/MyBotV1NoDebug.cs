using ChessChallenge.API;
using System;
using System.Collections.Generic; 

/*
    MyBot V1.0  ~(637 Brain Power SMH)

    Features
    Min Max Algorithm
    Alpha Beta Pruning
*/
/*  STATS = {
        1st Move Metric
        Thinking time: 692 resulting in a board eval of 0 + (0)
        Time for ThinkTime: 692 (692)
        Time for MinimaxTime: 673 (33.65)
        Time for evalBoardTime: 682 (0.05)
        Time for evalPieceTime: 624 (0)
        Time for getPossibleMovesTime: 0 (0)
        Total moves: 15942
        Total count for ThinkCount: 1
        Total count for minimaxCount: 15962
        Total count for minimaxCacheCount: 0
        Total count for minimaxDepthCacheCount: 0
        Total count for getPossibleMovesCount: 1618
        Total count for getPossibleMovesCacheCount: 66
        Total count for evalBoardCount: 14347
        Total count for evalBoardCacheCount: 1508
        Total count for evalPieceCount: 410448
        Total count for evalPieceCacheCount: 410272

        2nd Move Metric



        5th Move Metric



        10th Move Metric



        20th Move Metric




        30th Move Metric


        40th Move Metric


        50th Move Metric

    }*/

public class MyBotV1NoDebug : IChessBot
{
    /*

    ///3 stages, early, mid, late game
    ///each stage contains 8 longs representing the value at each position
    ///each position contains 8 bits representing the value at said position
    ///value is from -128 to 127
    ulong[][] tempPieceValueForSquare = new ulong[][] {
        new ulong[] {}, ///Pawn
        new ulong[] {}, ///Knight
        new ulong[] {}, ///Bishop
        new ulong[] {}, ///Rook
        new ulong[] {}, ///Queen
        new ulong[] {}, ///King
    };
    
    int[][] pieceValueForSquare = new int[][] {
        new int[] { ///Pawn
            ///Early game, generally, pawns should stay close to friendly side because it will help keep them safe
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,1,2,2,1,0,0,
            4,4,1,2,2,1,4,4,
            0,0,0,0,0,0,0,0,

            ///Mid game, pawns stay close to the middle i think
            0,0,0,0,0,0,0,0,
            0,0,0,1,1,0,0,0,
            1,1,1,2,2,1,1,1,
            2,2,2,3,3,2,2,2,
            4,4,3,2,2,3,4,4,
            3,3,2,1,1,2,3,3,
            2,2,1,0,0,1,2,2,
            0,0,0,0,0,0,0,0,

            ///END game, pawns ATTACK
            7,7,7,7,7,7,7,7,
            6,5,4,3,3,4,5,6,
            5,3,2,2,2,2,3,5,
            3,2,1,1,1,1,2,3,
            4,4,3,2,2,3,4,4,
            3,3,2,1,1,2,3,3,
            2,2,1,0,0,1,2,2,
            0,0,0,0,0,0,0,0,
        },
        new int[] { ///Knight

            ///avoid edges, go forward but not too far
            -1,-2,-1,-1,-1,-1,-2,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,
            -2,-1,1,0,0,1,-1,-2,
            -2,-1,0,0,0,0,-1,-2,
            -3,-2,0,0,0,0,-2,-3,
            -3,-2,1,0,0,1,-2,-3,
            -3,-2,-2,-2,-2,-2,-2,-3,
            -3,-3,-3,-3,-3,-3,-3,-3,

            ///little more forward
            -1,-1,-1,-1,-1,-1,-1,-1,
            -1,0,1,1,1,1,0,-1,
            -1,0,1,1,1,1,0,-1,
            -1,0,1,1,1,1,0,-1,
            -2,-1,0,0,0,0,-1,-2,
            -4,-3,-2,-1,-1,-2,-3,-4,
            -6,-6,-4,-4,-4,-4,-6,-6,
            -7,-7,-6,-6,-6,-6,-7,-7,

            ///just stay in center
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,1,1,1,1,0,0,
            0,0,1,2,2,1,0,0,
            0,0,1,2,2,1,0,0,
            0,0,1,1,1,1,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
        },
        new int[] { ///Bishop

            ///push out
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,1,1,0,0,0,
            1,1,1,2,2,1,1,1,
            3,1,2,3,3,2,1,3,
            1,2,1,2,2,1,2,1,
            0,0,1,0,0,1,0,0,

            ///little more forward but try to get good diagonal lines
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,1,1,0,0,0,
            1,1,1,2,2,1,1,1,
            1,1,1,2,2,1,1,1,
            3,1,2,3,3,2,1,3,
            1,2,1,2,2,1,2,1,
            0,0,1,0,0,1,0,0,

            ///just stay in center
            0,0,0,0,0,0,0,0,
            0,1,1,1,1,1,1,0,
            0,1,2,2,2,2,1,0,
            0,1,2,3,3,2,1,0,
            0,1,2,3,3,2,1,0,
            0,1,2,2,2,2,1,0,
            0,1,1,1,1,1,1,0,
            0,0,0,0,0,0,0,0,
        },
        new int[] { ///Rook
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,

            0,0,0,0,0,0,0,0,
            2,2,2,2,2,2,2,2,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,

            0,0,0,0,0,0,0,0,
            4,4,4,4,4,4,4,4,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
        },
        new int[] { ///Queen
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            3,3,3,3,3,3,3,3,
            5,5,5,5,5,5,5,5,

            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            3,3,3,3,3,3,3,3,
            5,5,5,5,5,5,5,5,
            3,3,3,3,3,3,3,3,
            5,5,5,5,5,5,5,5,

            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
        },
        new int[] { ///King
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            5,5,4,3,3,4,5,5,

            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            5,5,4,3,3,4,5,5,

            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
        },
    };*/
    Boolean onWhiteSide;

    //dont reset after each game to get a slight advantage (lol)
    Dictionary<ulong,Move[]> movesFromBoardCache = new Dictionary<ulong,Move[]>();
    Dictionary<ulong,int> boardEvalCache = new Dictionary<ulong,int>();
    Dictionary<ulong,int> minimaxCache = new Dictionary<ulong, int>();


    int defaultSearchDepth = 3;
    int autoMoveThreshold = 16;

    ///parses compressed piece values
    public MyBotV1NoDebug() {

    }

    /// Make move
    public Move Think(Board board, Timer timer)
    {
        onWhiteSide = board.IsWhiteToMove;
        
        int startThinkingTime = timer.MillisecondsElapsedThisTurn;
        int boardEvalInitial = evaluateBoard(board, onWhiteSide);

        Move[] moves = getPossibleMoves(board);
        
        int maxEval = int.MinValue;
        Move bestMove = moves[0];
        foreach (Move move in getPossibleMoves(board)) {
            int startMinimaxTime = timer.MillisecondsElapsedThisTurn;

            board.MakeMove(move);
            int eval = scoreMove(move);
            eval = minimax(board, 0, float.NegativeInfinity, float.PositiveInfinity, false, defaultSearchDepth, maxEval);
            if (maxEval < eval) {
                maxEval = eval;
                bestMove = move;
            }
            board.UndoMove(move);

            if (maxEval > autoMoveThreshold)
                break;
        }
        return bestMove;
    }


    /// algorithm to find best move
    public int minimax(Board board, int depth, float alpha, float beta, bool isMax, int maxDepth, int bestPrevEval) {
        if (depth >= maxDepth) return evaluateBoard(board, onWhiteSide);

        if (isMax) {
            int maxEval = int.MinValue;
            foreach (Move move in getPossibleMoves(board)) {
                board.MakeMove(move);
                int eval = minimax(board, depth+1, alpha, beta, false, maxDepth, bestPrevEval);
                board.UndoMove(move);

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break;
            }
            return maxEval;
        } else {
            int minEval = int.MaxValue;
            foreach (Move move in getPossibleMoves(board)) {
                board.MakeMove(move);
                int eval = minimax(board, depth+1, alpha, beta, true, maxDepth, bestPrevEval);
                board.UndoMove(move);

                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break;
            }
            return minEval;
        }
    }


    /// Checks for existing calculations and computes if not found
    public Move[] getPossibleMoves(Board board) {
        if (movesFromBoardCache.ContainsKey(board.ZobristKey)) {
            return movesFromBoardCache[board.ZobristKey];
        }
            
        Move[] moves = board.GetLegalMoves();
        movesFromBoardCache[board.ZobristKey] = moves;

        return moves;
    }


    /// Checks for existing calculations and computes if not found
    public int evaluateBoard(Board board, Boolean evaluatingForWhite) {
        if (boardEvalCache.ContainsKey(board.ZobristKey)) {
            return boardEvalCache[board.ZobristKey];
        }

        int eval = 0;
        if (board.IsInCheck()) eval += 3;
        if (board.IsInCheckmate()) eval += 1000000;
        if (board.IsInStalemate() || board.IsInsufficientMaterial() || board.IsRepeatedPosition()) eval -= 100000;
        if (board.IsDraw() || board.IsFiftyMoveDraw()) eval -= 10;

        foreach (PieceList pieces in board.GetAllPieceLists()) {
            foreach (Piece piece in pieces) {
                eval += getPieceValue(board, piece) * (piece.IsWhite ^ !evaluatingForWhite ? 1 : -1);
            }
        }

        boardEvalCache[board.ZobristKey] = eval;
        return eval;
    }


    int[] pieceValue = new int[] {
        0,1,3,3,5,9,-100000
    };

    Dictionary<Piece,int> pieceEvalCache = new Dictionary<Piece, int>();

    /// Gets the value of a piece based on its position on the board and other characteristics
    public int getPieceValue(Board board, Piece piece) {
        if (pieceEvalCache.ContainsKey(piece)) {
            return pieceEvalCache[piece];
        }

        int val = pieceValue[(int) piece.PieceType];
        pieceEvalCache[piece] = val;
        return val;
    }

    
    /// Gets the value of a move  based on what it achieves
    public int scoreMove(Move move) {
        if (move.IsCapture)
            return pieceValue[(int) move.CapturePieceType];
        if (move.IsEnPassant)
            return -1;
        if (move.IsPromotion)
            return pieceValue[(int) move.PromotionPieceType];
        if (move.IsCastles)
            return 1;
        
        return 0;
    }

  public void GameOver() {
    
  }
}