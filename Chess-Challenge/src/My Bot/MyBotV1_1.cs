using ChessChallenge.API;
using System;
using System.Collections.Generic; 

/*
    MyBot V1.1  ~(637 Brain Power SMH)

    Features
    Min Max Algorithm
    Alpha Beta Pruning
*/
/*  STATS depth3 = {
        Move 1 Metric
        ThinkTime: 966 (966)
        MinimaxTime: 966 (24.15)
        evalBoardTime: 828 (0)
        evalPieceTime: 122 (0)
        getPossibleMovesTime: 31 (0)

        Moves: 298158
        ThinkCount: 1
        ThinkCount: 298198 |  cache (0) | depthCache (0)
        ThinkCount: 15143 | cache (0.11998943)
        ThinkCount: 283058 | cache (0.3053226)
        ThinkCount: 5801194 | cache (0)


        Move 2 Metric
        ThinkTime: 1005 (502.5)
        MinimaxTime: 1005 (22.84)
        evalBoardTime: 863 (0)
        evalPieceTime: 124 (0)
        getPossibleMovesTime: 33 (0)

        Moves: 309775
        ThinkCount: 2
        ThinkCount: 309819 |  cache (0) | depthCache (0)
        ThinkCount: 15921 | cache (0.114754096)
        ThinkCount: 293904 | cache (0.30061856)
        ThinkCount: 6051415 | cache (0)


        Move 5 Metric
        ThinkTime: 1802 (360.4)
        MinimaxTime: 1802 (14.08)
        evalBoardTime: 1530 (0)
        evalPieceTime: 228 (0)
        getPossibleMovesTime: 71 (0)

        Moves: 568964
        ThinkCount: 5
        ThinkCount: 569092 |  cache (0) | depthCache (0)
        ThinkCount: 43308 | cache (0.16336474)
        ThinkCount: 525799 | cache (0.28246915)
        ThinkCount: 10515830 | cache (0)



        Move 10 Metric
        ThinkTime: 1987 (198.7)
        MinimaxTime: 1987 (12.66)
        evalBoardTime: 1684 (0)
        evalPieceTime: 250 (0)
        getPossibleMovesTime: 80 (0)

        Moves: 636264
        ThinkCount: 10
        ThinkCount: 636421 |  cache (0) | depthCache (0)
        ThinkCount: 50554 | cache (0.16204454)
        ThinkCount: 585897 | cache (0.28076267)
        ThinkCount: 11518135 | cache (0)


        Move 20 Metric
        ThinkTime: 2578 (128.9)
        MinimaxTime: 2578 (11.06)
        evalBoardTime: 2155 (0)
        evalPieceTime: 312 (0)
        getPossibleMovesTime: 112 (0)

        Moves: 885272
        ThinkCount: 20
        ThinkCount: 885505 |  cache (0) | depthCache (0)
        ThinkCount: 75929 | cache (0.1706726)
        ThinkCount: 809636 | cache (0.30279532)
        ThinkCount: 14598729 | cache (0)


        Move 30 Metric
        ThinkTime: 3044 (101.47)
        MinimaxTime: 3044 (9.31)
        evalBoardTime: 2507 (0)
        evalPieceTime: 338 (0)
        getPossibleMovesTime: 138 (0)

        Moves: 1133878
        ThinkCount: 30
        ThinkCount: 1134205 |  cache (0) | depthCache (0)
        ThinkCount: 110398 | cache (0.18947807)
        ThinkCount: 1023897 | cache (0.3342729)
        ThinkCount: 16593107 | cache (0)


        Move 40 Metric
        ThinkTime: 3156 (78.9)
        MinimaxTime: 3156 (8.6)
        evalBoardTime: 2585 (0)
        evalPieceTime: 346 (0)
        getPossibleMovesTime: 146 (0)

        Moves: 1197967
        ThinkCount: 40
        ThinkCount: 1198334 |  cache (0) | depthCache (0)
        ThinkCount: 120050 | cache (0.18948771)
        ThinkCount: 1078404 | cache (0.34265545)
        ThinkCount: 16913636 | cache (0)


        Game Summary
        Total time for ThinkTime: 3197
        Total time for MinimaxTime: 3197
        Total time for evalBoardTime: 2610
        Total time for evalPieceTime: 347
        Total time for getPossibleMovesTime: 152
        Total moves: 1236847
        Total count for ThinkCount: 48
        Total count for minimaxCount: 1237291
        Total count for minimaxCacheCount: 0
        Total count for minimaxDepthCacheCount: 0
        Total count for getPossibleMovesCount: 129371
        Total count for getPossibleMovesCacheCount: 26817
        Total count for evalBoardCount: 1108064
        Total count for evalBoardCacheCount: 386801
        Total count for evalPieceCount: 17027130
        Total count for evalPieceCacheCount: 0

    }*/

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
    Dont use dictionary, use hash + bit mask and an array (much faster)


    Make tournament system
    Make elo system
    file system for bots (bot training)
    add dropdown menu for bot matches
*/
public class MyBotV1_1 : IChessBot
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


    int defaultSearchDepth = 4;
    int autoMoveThreshold = 10;

    int moveCount = 0;

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
        if (timer.MillisecondsRemaining < 30000) defaultSearchDepth = 3;
        if (timer.MillisecondsRemaining < 15000) defaultSearchDepth = 2;
        if (timer.MillisecondsRemaining < 3000) defaultSearchDepth = 1;
        moveCount++;
        cTimer = timer;
        onWhiteSide = board.IsWhiteToMove;
        logCount(LogCountType.ThinkCount, 1);
        
        int startThinkingTime = timer.MillisecondsElapsedThisTurn;
        int boardEvalInitial = evaluate(board, onWhiteSide);

        Move[] moves = getPossibleMoves(board);
        
        int maxEval = int.MinValue;
        Move bestMove = moves[0];
        foreach (Move move in getPossibleMoves(board)) {
            int startMinimaxTime = timer.MillisecondsElapsedThisTurn;

            board.MakeMove(move);
            int eval = scoreMove(move);
            if (moveCache.Contains(move)) eval -= 1;
            eval = minimax(board, 0, float.NegativeInfinity, float.PositiveInfinity, false, defaultSearchDepth, maxEval);
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
    public int minimax(Board board, int depth, float alpha, float beta, bool isMax, int maxDepth, int bestPrevEval) {
        /*if (minimaxCache.ContainsKey(board.ZobristKey)) {
            logCount(LogCountType.minimaxDepthCacheCount, maxDepth - depth);
            return minimaxCache[board.ZobristKey];
        } */
        logCount(LogCountType.minimaxCount, 1);

        if (depth >= maxDepth) return evaluate(board, onWhiteSide);

        if (isMax) {
            int maxEval = int.MinValue;
            foreach (Move move in getPossibleMoves(board)) {
                board.MakeMove(move);
                int eval = evaluateBoard(board);
                /*if (minimaxCache.ContainsKey(board.ZobristKey)) {
                    logCount(LogCountType.minimaxCacheCount, 1);
                    eval = minimaxCache[board.ZobristKey];
                } else*/ 
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
    public int evaluate(Board board, Boolean evaluatingForWhite) {
        logCount(LogCountType.evalBoardCount, 1);
        int timeEvalBoardStart = cTimer.MillisecondsElapsedThisTurn;

        int eval = 0;
        if (boardEvalCache.ContainsKey(board.ZobristKey)) {
            logCount(LogCountType.evalBoardCacheCount, 1);
            eval = boardEvalCache[board.ZobristKey];
        } else {
            evaluateBoard(board);

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

    public int evaluateBoard(Board board) {
        int eval =0;
        if (board.IsInCheck()) eval += 3;
        if (board.IsInCheckmate()) eval += 1000000;
        if (board.IsInStalemate() || board.IsInsufficientMaterial() || board.IsRepeatedPosition()) eval -= 100000;
        if (board.IsDraw() || board.IsFiftyMoveDraw()) eval -= 500;
        return eval;
    }

    int[] pieceValue = new int[] {
        0,1,3,3,5,9,100
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
            return Math.Max(pieceValue[(int) move.CapturePieceType] - pieceValue[(int) move.MovePieceType], 0);
        if (move.IsEnPassant)
            return 1;
        if (move.IsPromotion)
            return Math.Max(pieceValue[(int) move.MovePieceType] - pieceValue[(int) move.PromotionPieceType], 0);
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