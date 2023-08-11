using ChessChallenge.API;
using System;
using System.Collections.Generic; 

/*
    MyBot V1.0  ~(637 Brain Power)

    Features
    Min Max Algorithm
    Alpha Beta Pruning
*/

public class MyBotV1 : IChessBot
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


    int defaultSearchDepth = 2;
    int autoMoveThreshold = 16;

    ///parses compressed piece values
    public MyBotV1() {
        timeLog = new List<int>[] {
            new List<int>(),
            new List<int>(),
            new List<int>(),
            new List<int>(),
            new List<int>(),
        };
        countLogs = new int[10];
        moveLog = new List<Move>();
    }

    /// Make move
    public Move Think(Board board, Timer timer)
    {
        cTimer = timer;
        onWhiteSide = board.IsWhiteToMove;
        logCount(LogCountType.ThinkCount, 1);
        
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

            int endMinimaxTime = timer.MillisecondsElapsedThisTurn;
            Console.WriteLine("Minimax time: " + (endMinimaxTime-startMinimaxTime) + " resulting in a board eval of " + eval + " dif from max (" + (eval-maxEval) + ")");
            logTime(LogTimeType.MinimaxTime, endMinimaxTime-startMinimaxTime);

            if (maxEval > autoMoveThreshold)
                break;
        }

        int endThinkingTime = timer.MillisecondsElapsedThisTurn;
        Console.WriteLine("Thinking time: " + (endThinkingTime-startThinkingTime) + " resulting in a board eval of " + maxEval + " + (" + (maxEval-boardEvalInitial) + ")");
        logTime(LogTimeType.ThinkTime, endThinkingTime-startThinkingTime);

        compileLogs();
        Console.WriteLine("\n--------------------\n\n");
        return bestMove;
    }


    /// algorithm to find best move
    public int minimax(Board board, int depth, float alpha, float beta, bool isMax, int maxDepth, int bestPrevEval) {
        /*if (minimaxCache.ContainsKey(board.ZobristKey)) {
            logCount(LogCountType.minimaxDepthCacheCount, maxDepth - depth);
            return minimaxCache[board.ZobristKey];
        } */
        logCount(LogCountType.minimaxCount, 1);

        if (depth >= maxDepth) return evaluateBoard(board, onWhiteSide);

        if (isMax) {
            int maxEval = int.MinValue;
            foreach (Move move in getPossibleMoves(board)) {
                board.MakeMove(move);
                int eval;
                /*if (minimaxCache.ContainsKey(board.ZobristKey)) {
                    logCount(LogCountType.minimaxCacheCount, 1);
                    eval = minimaxCache[board.ZobristKey];
                } else*/ 
                    eval = minimax(board, depth+1, alpha, beta, false, maxDepth, bestPrevEval);
                board.UndoMove(move);

                logMove(move);

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break;
                //if (maxEval < 0 && maxEval < bestPrevEval) return -10000;
            }
            //minimaxCache[board.ZobristKey] = maxEval;
            return maxEval;
        } else {
            int minEval = int.MaxValue;
            foreach (Move move in getPossibleMoves(board)) {
                board.MakeMove(move);
                int eval;
                /*if (minimaxCache.ContainsKey(board.ZobristKey)) {
                    logCount(LogCountType.minimaxCacheCount, 1);
                    eval = minimaxCache[board.ZobristKey];
                } else*/
                    eval = minimax(board, depth+1, alpha, beta, true, maxDepth, bestPrevEval);
                
                board.UndoMove(move);

                logMove(move);

                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break;
            }
            //minimaxCache[board.ZobristKey] = minEval;
            return minEval;
        }
    }


    /// Checks for existing calculations and computes if not found
    public Move[] getPossibleMoves(Board board) {
        logCount(LogCountType.getPossibleMovesCount, 1);
        int timeGetMovesStart = cTimer.MillisecondsElapsedThisTurn;

        Move[] moves;

        if (movesFromBoardCache.ContainsKey(board.ZobristKey)) {
            logCount(LogCountType.getPossibleMovesCacheCount, 1);
            moves = movesFromBoardCache[board.ZobristKey];
        } else {
            moves = board.GetLegalMoves();


            movesFromBoardCache[board.ZobristKey] = moves;
        }

        logTime(LogTimeType.getPossibleMovesTime, cTimer.MillisecondsElapsedThisTurn-timeGetMovesStart);
        return moves;
    }


    /// Checks for existing calculations and computes if not found
    public int evaluateBoard(Board board, Boolean evaluatingForWhite) {
        logCount(LogCountType.evalBoardCount, 1);
        int timeEvalBoardStart = cTimer.MillisecondsElapsedThisTurn;

        int eval = 0;
        if (boardEvalCache.ContainsKey(board.ZobristKey)) {
            logCount(LogCountType.evalBoardCacheCount, 1);
            eval = boardEvalCache[board.ZobristKey];
        } else {
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
        }
        logTime(LogTimeType.evalBoardTime, cTimer.MillisecondsElapsedThisTurn-timeEvalBoardStart);
        return eval;
    }


    int[] pieceValue = new int[] {
        0,1,3,3,5,9,-100000
    };

    Dictionary<Piece,int> pieceEvalCache = new Dictionary<Piece, int>();

    /// Gets the value of a piece based on its position on the board and other characteristics
    public int getPieceValue(Board board, Piece piece) {
        logCount(LogCountType.evalPieceCount, 1);
        int timeEvalPieceValueStart = cTimer.MillisecondsElapsedThisTurn;
        int val;
        if (pieceEvalCache.ContainsKey(piece)) {
            logCount(LogCountType.evalPieceCacheCount, 1);
            val = pieceEvalCache[piece];
        } else {
            val = pieceValue[(int) piece.PieceType];
            pieceEvalCache[piece] = val;
        }

        logTime(LogTimeType.evalPieceTime, cTimer.MillisecondsElapsedThisTurn-timeEvalPieceValueStart);
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


    public enum LogTimeType : int {
        ThinkTime = 0,
        MinimaxTime = 1,
        evalBoardTime = 2,
        evalPieceTime = 3,
        getPossibleMovesTime = 4,
    }

    public enum LogCountType : int {
        ThinkCount = 0,
        minimaxCount = 1,
        minimaxCacheCount = 2,
        minimaxDepthCacheCount = 3,
        getPossibleMovesCount = 4,
        getPossibleMovesCacheCount = 5,
        evalBoardCount = 6,
        evalBoardCacheCount = 7,
        evalPieceCount = 8,
        evalPieceCacheCount = 9,
    }


    public Timer cTimer;
    List<int>[] timeLog;

    int[] countLogs;

    List<Move> moveLog;

    public void logTime(LogTimeType type, int time) {
        timeLog[(int) type].Add(time);
    }

    public void logCount(LogCountType type, int count) {
        countLogs[(int) type]+=count;
    }

    public void logMove(Move move) {
        moveLog.Add(move);
    }


    public void compileLogs() {
        for (int i = 0; i < timeLog.Length; i++) {
            int sum = 0;
            foreach (int time in timeLog[i]) {
                sum += time;
            }
            if (timeLog[i].Count > 0)
                Console.WriteLine("Time for " + (LogTimeType) i + ": " + (sum) + " (" + Math.Round(sum/ (float)timeLog[i].Count,2) + ")");
            else
                Console.WriteLine("Time for " + (LogTimeType) i + ": 0");
        }

        Console.WriteLine("Total moves: " + moveLog.Count);
        
        for (int i = 0; i < countLogs.Length; i++) {
            Console.WriteLine("Total count for " + (LogCountType) i + ": " + countLogs[i]);
        }
    }
    public void GameOver() {
        Console.WriteLine("Game over\n--------------------\n\n");
        for (int i = 0; i < timeLog.Length; i++) {
            int sum = 0;
            foreach (int time in timeLog[i]) {
                sum += time;
            }
            Console.WriteLine("Total time for " + (LogTimeType) i + ": " + (sum));
        }

        Console.WriteLine("Total moves: " + moveLog.Count);
        
        for (int i = 0; i < countLogs.Length; i++) {
            Console.WriteLine("Total count for " + (LogCountType) i + ": " + countLogs[i]);
        }
        Console.WriteLine("--------------------\n");
    }
}