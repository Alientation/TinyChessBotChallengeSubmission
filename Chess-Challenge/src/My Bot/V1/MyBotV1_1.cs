using ChessChallenge.API;
using System;
using System.Collections.Generic; 

/*
    MyBot V1.1  ~(637 Brain Power)

    Features (dif from previous version 1.0)
    
*/

public class MyBotV1_1 : IChessBot {
    //dont reset after each game to get a slight advantage (lol)
    Dictionary<ulong,Move[]> movesFromBoardCache = new Dictionary<ulong,Move[]>();
    Dictionary<ulong,int> boardEvalCache = new Dictionary<ulong,int>();
    Dictionary<ulong,int> minimaxCache = new Dictionary<ulong, int>();


    int defaultSearchDepth = 4;
    int autoMoveThreshold = 10;
    int maxEvalCutoff = 500;
    int minEvalCutoff = -500;

    int moveCount = 0;
    bool botIsWhite = true;

    ///parses compressed piece values
    public MyBotV1_1() {
        moveCount = 0;
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
    HashSet<Move> moveCache = new HashSet<Move>();
    public Move Think(Board board, Timer timer)
    {
        botIsWhite = board.IsWhiteToMove;
        if (timer.MillisecondsRemaining < 30000) defaultSearchDepth = 3;
        if (timer.MillisecondsRemaining < 15000) defaultSearchDepth = 2;
        if (timer.MillisecondsRemaining < 3000) defaultSearchDepth = 1;
        moveCount++;
        cTimer = timer;
        logCount(LogCountType.ThinkCount, 1);
        
        int startThinkingTime = timer.MillisecondsElapsedThisTurn;
        int boardEvalInitial = evaluate(board);

        Move[] moves = getPossibleMoves(board);
        
        int maxEval = -30000;
        Move bestMove = moves[0];
        foreach (Move move in getPossibleMoves(board)) {
            int startMinimaxTime = timer.MillisecondsElapsedThisTurn;

            board.MakeMove(move);
            int eval = scoreMove(move);
            if (moveCache.Contains(move)) eval -= 1;
            eval = minimax(board, 0, -30000, 30000, false, defaultSearchDepth, maxEval);
            if (maxEval < eval) {
                maxEval = eval;
                bestMove = move;
            }
            board.UndoMove(move);

            int endMinimaxTime = timer.MillisecondsElapsedThisTurn;
            //Console.WriteLine(move + " -> " + eval + " + (" + (eval-maxEval) + ")");
            logTime(LogTimeType.MinimaxTime, endMinimaxTime-startMinimaxTime);

            if (maxEval > autoMoveThreshold)
                break;
        }

        int endThinkingTime = timer.MillisecondsElapsedThisTurn;
        Console.WriteLine("[" + bestMove + "] -> " + maxEval + " + (" + (maxEval-boardEvalInitial) + ")");
        logTime(LogTimeType.ThinkTime, endThinkingTime-startThinkingTime);

        if (moveCount == 1 || moveCount == 2 || moveCount == 5 || moveCount == 10 || moveCount == 20 || moveCount == 30 || moveCount == 40 || moveCount == 50) {
            Console.WriteLine("\nMove " + moveCount + " Metric");
            compileLogs();
            Console.WriteLine("--------------------");
        }
        moveCache.Add(bestMove);
        return bestMove;
    }


    /// algorithm to find best move
    public int minimax(Board board, int depth, int alpha, int beta, bool isMax, int maxDepth, int bestPrevEval) {
        /*if (minimaxCache.ContainsKey(board.ZobristKey)) {
            logCount(LogCountType.minimaxDepthCacheCount, maxDepth - depth);
            return minimaxCache[board.ZobristKey];
        } */
        logCount(LogCountType.minimaxCount, 1);

        if (depth >= maxDepth) return evaluate(board);

        if (isMax) {
            int maxEval = -30000;
            foreach (Move move in getPossibleMoves(board)) {
                board.MakeMove(move);
                //int eval = evaluateBoard(board);
                int eval = 0;
                /*if (minimaxCache.ContainsKey(board.ZobristKey)) {
                    logCount(LogCountType.minimaxCacheCount, 1);
                    eval = minimaxCache[board.ZobristKey];
                } else*/ 
                if (eval > maxEvalCutoff) {
                    board.UndoMove(move);
                    return eval;
                }

                eval += minimax(board, depth+1, alpha, beta, false, maxDepth, bestPrevEval);
                
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
            int minEval = 30000;
            foreach (Move move in getPossibleMoves(board)) {
                board.MakeMove(move);
                //int eval = evaluateBoard(board);
                int eval = 0;
                /*if (minimaxCache.ContainsKey(board.ZobristKey)) {
                    logCount(LogCountType.minimaxCacheCount, 1);
                    eval = minimaxCache[board.ZobristKey];
                } else*/

                if (eval < minEvalCutoff) {
                    board.UndoMove(move);
                    return eval;
                }

                eval += minimax(board, depth+1, alpha, beta, true, maxDepth, bestPrevEval);
                
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
    public int evaluate(Board board) {
        logCount(LogCountType.evalBoardCount, 1);
        int timeEvalBoardStart = cTimer.MillisecondsElapsedThisTurn;

        int eval = 0;
        if (boardEvalCache.ContainsKey(board.ZobristKey)) {
            logCount(LogCountType.evalBoardCacheCount, 1);
            eval = boardEvalCache[board.ZobristKey];
        } else {
            //eval = evaluateBoard(board);

            foreach (PieceList pieces in board.GetAllPieceLists()) {
                foreach (Piece piece in pieces) {
                    eval += getPieceValue(board, piece) * (piece.IsWhite == botIsWhite ? 1 : -1);
                }
            }

            boardEvalCache[board.ZobristKey] = eval;
        }
        logTime(LogTimeType.evalBoardTime, cTimer.MillisecondsElapsedThisTurn-timeEvalBoardStart);
        return eval;
    }
    /*
    public int evaluateBoard(Board board) {
        int eval =0;
        if (board.IsInCheck()) eval += 1;
        if (board.IsInCheckmate()) eval += 1000000;
        if (board.IsInStalemate() || board.IsInsufficientMaterial() || board.IsRepeatedPosition()) eval -= 100000;
        if (board.IsDraw() || board.IsFiftyMoveDraw()) eval -= 500;
        return eval;
    }*/

    int[] pieceValue = new int[] {
        0,10,30,30,50,90,0
    };

    /// Gets the value of a piece based on its position on the board and other characteristics
    public int getPieceValue(Board board, Piece piece) {
        logCount(LogCountType.evalPieceCount, 1);
        int timeEvalPieceValueStart = cTimer.MillisecondsElapsedThisTurn;
        int val = pieceValue[(int) piece.PieceType];
        /*if (pieceEvalCache.ContainsKey(piece)) {
            logCount(LogCountType.evalPieceCacheCount, 1);
            val = pieceEvalCache[piece];
        } else {
            val = pieceValue[(int) piece.PieceType];
            pieceEvalCache[piece] = val;
        }*/
        
        logTime(LogTimeType.evalPieceTime, cTimer.MillisecondsElapsedThisTurn-timeEvalPieceValueStart);
        return val;
    }

    
    /// Gets the value of a move  based on what it achieves
    public int scoreMove(Move move) {
        if (move.IsCapture)
            return Math.Max(pieceValue[(int) move.CapturePieceType] - pieceValue[(int) move.MovePieceType], 0) >> 1;
        if (move.IsEnPassant)
            return 5;
        if (move.IsPromotion)
            return Math.Max(pieceValue[(int) move.MovePieceType] - pieceValue[(int) move.PromotionPieceType], 0) >> 1;
        if (move.IsCastles)
            return 40;
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


    private Timer cTimer;
    private List<int>[] timeLog;

    private int[] countLogs;

    private List<Move> moveLog;

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
            for (int j = 0; j < timeLog[i].Count; j++) {
                sum += timeLog[i][j];
            }
            if (timeLog[i].Count > 0)
                Console.WriteLine((LogTimeType) i + ": " + (sum) + " (" + Math.Round(sum/ (float)timeLog[i].Count,2) + ")");
            else
                Console.WriteLine((LogTimeType) i + ": 0");
        }
        
        Console.WriteLine("\nMoves: " + moveLog.Count);
        
        Console.WriteLine((LogCountType) 0 + ": " + countLogs[0]);
        Console.WriteLine((LogCountType) 1 + ": " + countLogs[1] + " |  cache (" + (countLogs[2] / (float) countLogs[1]) + ") | depthCache (" + (countLogs[3] / (float) countLogs[1]) + ")");
        Console.WriteLine((LogCountType) 4 + ": " + countLogs[4] + " | cache (" + (countLogs[5] / (float) countLogs[4]) + ")");
        Console.WriteLine((LogCountType) 6 + ": " + countLogs[6] + " | cache (" + (countLogs[7] / (float) countLogs[6]) + ")");
        Console.WriteLine((LogCountType) 8 + ": " + countLogs[8] + " | cache (" + (countLogs[9] / (float)countLogs[8]) + ")");
    }
    public void GameOver() {
        Console.WriteLine("Game over\n--------------------\n\n");
        for (int i = 0; i < timeLog.Length; i++) {
            int sum = 0;
            for (int j = 0; j < timeLog[i].Count; j++) {
                sum += timeLog[i][j];
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