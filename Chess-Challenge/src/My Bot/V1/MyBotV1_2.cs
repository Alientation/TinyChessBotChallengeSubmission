using ChessChallenge.API;
using System;
using System.Collections.Generic; 

/*
    MyBot V1.2  ~(69420 Brain Power)

    Features (dif from previous version 1.1)
    
*/

public class MyBotV1_2 : IChessBot {

    //dont reset after each game to get a slight advantage (lol)
    Dictionary<ulong,Move[]> movesFromBoardCache = new Dictionary<ulong,Move[]>();
    Dictionary<ulong,int> boardEvalCache = new Dictionary<ulong,int>();
    Dictionary<ulong,int> minimaxCache = new Dictionary<ulong, int>();


    int defaultSearchDepth = 4;
    int autoMoveThreshold = 200;

    int moveCount = 0;
    bool botIsWhite = true;

    ///parses compressed piece values
    public MyBotV1_2() {
        moveCount = 0;
        timeLog = new List<int>[] {
            new List<int>(),
            new List<int>(),
            new List<int>(),
            new List<int>(),
            new List<int>(),
        };
        countLogs = new int[logCountSize];
        moveLog = new List<Move>();
    }

    /// Make move
    HashSet<Move> moveCache = new HashSet<Move>();
    public Move Think(Board board, Timer timer) {   
        botIsWhite = board.IsWhiteToMove;
        Console.WriteLine("Thinking...");
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
            eval = negamax(board, defaultSearchDepth, -30000, 30000);
            if (maxEval < eval) {
                maxEval = eval;
                bestMove = move;
            }
            board.UndoMove(move);

            int endMinimaxTime = timer.MillisecondsElapsedThisTurn;
            Console.WriteLine(move + " -> " + eval + " + (" + (eval-maxEval) + ")");
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

    /// Negamax Algorithm
    public int negamax(Board board, int depth, int alpha, int beta) {
        logCount(LogCountType.negamaxCount, 1);
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw()) return evaluate(board);

        int bestEval = -30000;
        foreach (Move move in getPossibleMoves(board)) {
            board.MakeMove(move);
            int eval = -negamax(board, depth-1, -beta, -alpha);
            board.UndoMove(move);

            if (eval >= beta) return beta;
            if (eval > bestEval) {
                bestEval = eval;
                if (eval > alpha) alpha = eval;
            }
        }
        return alpha;
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
                    eval += getPieceValue(board, piece) * (piece.IsWhite == board.IsWhiteToMove ? -1 : 1);
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
        if (board.IsInCheck()) eval += 5;
        if (board.IsInCheckmate()) eval += 1000000;
        if (board.IsInStalemate() || board.IsInsufficientMaterial() || board.IsRepeatedPosition()) eval -= 5000;
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
            return 1 + (Math.Max(pieceValue[(int) move.CapturePieceType] - pieceValue[(int) move.MovePieceType], 0) >> 3);
        if (move.IsEnPassant)
            return 5;
        if (move.IsPromotion)
            return 1 + (Math.Max(pieceValue[(int) move.MovePieceType] - pieceValue[(int) move.PromotionPieceType], 0) >> 3);
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

    int logCountSize = 8;

    public enum LogCountType : int {
        ThinkCount = 0,
        negamaxCount = 1,
        getPossibleMovesCount = 2,
        getPossibleMovesCacheCount = 3,
        evalBoardCount = 4,
        evalBoardCacheCount = 5,
        evalPieceCount = 6,
        evalPieceCacheCount = 7,
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
        
        Console.WriteLine((LogCountType) 0 + ": " + countLogs[(int)LogCountType.ThinkCount]);
        Console.WriteLine((LogCountType) 1 + ": " + countLogs[(int)LogCountType.negamaxCount]);
        Console.WriteLine((LogCountType) 4 + ": " + countLogs[(int)LogCountType.getPossibleMovesCount] + " | cache (" + (countLogs[(int)LogCountType.getPossibleMovesCacheCount] / (float) countLogs[(int)LogCountType.getPossibleMovesCount]) + ")");
        Console.WriteLine((LogCountType) 6 + ": " + countLogs[(int) LogCountType.evalBoardCount] + " | cache (" + (countLogs[(int) LogCountType.evalBoardCacheCount] / (float) countLogs[(int) LogCountType.evalBoardCount]) + ")");
        Console.WriteLine((LogCountType) 8 + ": " + countLogs[(int) LogCountType.evalPieceCount] + " | cache (" + (countLogs[(int) LogCountType.evalPieceCacheCount] / (float)countLogs[(int) LogCountType.evalPieceCount]) + ")");
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