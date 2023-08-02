using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Numerics;

/*
    MyBot V1.0  ~(725 Brain Power SMH)

    Features
    Negamax Alpha Beta Pruning
    Score board based off piece locations

    NOTES


    Against NNBot, it makes unintelligent trades that result in large eval drops. why does the search do this?
    

    Against the EloBot1, it fails at early game and late game but it is descent at random game positions
    - 202 +/- 58 Elo difference
    - 119 / 17 / 31 (win rate 71.25%)


    Add Transposition table (remove move cache)
    Add Quiesence
    Add move ordering
    possibly do killer moves and null move pruning
Ggame stage
*/

/*
    TODO

    sort moves when getting possible moves
    add more features to board evaluation
        - pawn advancement
        - piece mobility
        - piece threats
        - piece protection
    Null Move Heuristic (find eval of opponent moving two times in a row to get the minimum alpha value)
    Quiesence Searching (only applies to moves that result in a capture)
    Optimized Searching (go down only promising paths)
    OPTIMIZE CODE
    Move Pruning
    Late Move Reductions, History Reductions
    Store minimax traversals (edges that haven't been searched from a particular board state)
    
*/
public class MyBotV2_1 : IChessBot {
    bool isBotWhite, breakBecauseTime = false;
    int timePerMove = 500, gameStage = 0;
    Timer cTimer;
    Board cBoard;
    static Move nullMove = Move.NullMove;
    Move bestMove = nullMove, bestRootMove;
    int bestEval, bestRootEval;
    (ulong zobristKey, short depth, int eval, byte flag, Move Move, short ancient)[] TTable = new (ulong zobristKey, short depth, int eval, byte flag, Move Move, short ancient)[1<<21];
    int maxDepthDifferenceFromCurrentAllowedToBeUsedFromTTable = 2;
    short currentAncientValue = 0, maxAncientDifferenceAllowed = 6;
    

    public Move Think(Board board, Timer timer) {
        if (cTimer == null) {
            isBotWhite = board.IsWhiteToMove;
            if (!isBotWhite)
                for (int i = 0; i < pieceValueLocation.Length; i++)
                    Array.Reverse(pieceValueLocation[i]);
        }
        cBoard = board;
        cTimer = timer;

        //Game stage
        int pieceCount = BitOperations.PopCount(board.AllPiecesBitboard);
        int score = 32 - pieceCount + (board.PlyCount);
        if (score < 10) gameStage = 0;
        else if (score < 20) gameStage = 1;
        else gameStage = 2;

        #if DEBUG
        Console.WriteLine("eval " + evaluate(board) + " stage=" + gameStage);
        #endif
        
        for (int depth = 1; depth <= 3 * gameStage + 5 && timer.MillisecondsElapsedThisTurn < timePerMove; depth++) {
            int val = negamax(board, depth, 0, -300000000, 300000000);
            if (!breakBecauseTime) {
                bestRootEval = val;
                bestRootMove = bestMove;
            }
            breakBecauseTime = false;
            #if DEBUG
            Console.WriteLine("depth " + depth + " " + bestMove + " (" + val +") | " + "(" + timer.MillisecondsElapsedThisTurn + "ms)");
            #endif
        }

        if (bestRootMove == nullMove) {
            bestRootMove = bestMove;
            bestRootEval = bestEval;
        }

        if (timer.MillisecondsRemaining < 50000) timePerMove = 400;
        if (timer.MillisecondsRemaining < 35000) timePerMove = 300;
        if (timer.MillisecondsRemaining < 25000) timePerMove = 200;
        if (timer.MillisecondsRemaining < 15000) timePerMove = 100;
        if (timer.MillisecondsRemaining < 05000) timePerMove = 50;

        #if DEBUG
        Console.WriteLine("Chosen Move " + bestRootMove + " (" + bestRootEval + ") -> [" + "nodes=" + negamaxNodesCount + ", eval=" + boardEvalCount + " (" + 
        (boardEvalCacheCount / (float) boardEvalCount) + "), moveEval=" + moveEvalCount + ", findMove=" + possibleMoveCount + " (" + (possibleMoveCacheCount / (float) possibleMoveCount) + ")"); 
        #endif
        return bestRootMove;
    }

    #if DEBUG
    int negamaxNodesCount = 0, boardEvalCount = 0, boardEvalCacheCount = 0, moveEvalCount = 0, possibleMoveCount = 0, possibleMoveCacheCount = 0;
    #endif

    //negamax with alpha beta pruning
    public int negamax(Board board, int depthLeft, int depth, int alpha, int beta) {
        if (cTimer.MillisecondsElapsedThisTurn > timePerMove) {
            breakBecauseTime = true;
            return 0;
        }

        #if DEBUG
        negamaxNodesCount++;
        #endif
        if (board.IsDraw()) return -100;
        if (board.IsInCheckmate()) return board.PlyCount - 100000000;

        if (depthLeft == 0 || board.IsInCheckmate() || board.IsInStalemate() || board.IsFiftyMoveDraw() || board.IsInsufficientMaterial())
            return evaluate(board);

        Move[] moves = getOrderedMoves(board, depth);

        int highestEval = -200000;
        foreach (Move move in moves) {
            board.MakeMove(move);
            int eval = evaluateMove(move);

            eval -= negamax(board, depthLeft - 1, depth+1, -beta, -alpha);
            
            board.UndoMove(move);

            if (eval > highestEval) {
                highestEval = eval;
                if (depth == 0) {
                    bestMove = move;
                    bestEval = highestEval;
                }
            }
            alpha = Math.Max(alpha, eval);
            if (alpha >= beta)
                break;
        }

        return highestEval;
    }

    int[] pieceValue = { 0, 110, 300, 320, 520, 910, 10000 };
    Dictionary<ulong,int> boardEvalCache = new Dictionary<ulong,int>(1000000);

    //evaluates a position based on how desirable it is for the current player to play the next move
    public int evaluate(Board board) {
        #if DEBUG
        boardEvalCount++;
        #endif

        //board eval cache
        if (boardEvalCache.ContainsKey(board.ZobristKey)) {
            #if DEBUG
            boardEvalCacheCount++;
            #endif
            return boardEvalCache[board.ZobristKey];
        }

        //dont want to reach these states
        if (board.IsDraw()) return -100;
        if (board.IsInCheckmate()) return  board.IsWhiteToMove == isBotWhite ? -100000 : 100000;

        //debuff future moves (anything that can be achieved earlier is prioritized)
        int eval = 0;//40 - board.PlyCount;

        //only give score for checking if not in end game (end game focus on piece structure/pawn promotion)
        //if (board.IsInCheck() && gameStage < 2) eval -= 50;

        //add up score for pieces and their locations at current game stage
        foreach (bool flag in new[] {true, false}) {
            int piecesEval = 0;
            for (var p = PieceType.Pawn; p <= PieceType.King; p++) {
                ulong mask = board.GetPieceBitboard(p, flag);
                while (mask != 0) {
                    int index = BitboardHelper.ClearAndGetIndexOfLSB(ref mask);
                    //piecesEval += pieceValue[(int)p] + pieceValueLocation[((int)p - 1) * 3 + gameStage][index];
                    piecesEval += pieceValue[(int)p];
                }
                eval += piecesEval * (flag == board.IsWhiteToMove ? 1 : -1);
            }
        }
        boardEvalCache[board.ZobristKey] = eval;
        return eval;
    }

    //evaluates a move based on what it accomplishes
    public int evaluateMove(Move move) {
        #if DEBUG
        moveEvalCount++;
        #endif
        //if (move.IsCapture) return move.CapturePieceType - move.MovePieceType;
        //if (move.IsCastles) return 160;
        //if (move.IsEnPassant) return 60;
        //if (move.IsPromotion) return pieceValue[(int)move.PromotionPieceType];
        return 0;
    }

    //todo add move ordering
    Dictionary<ulong,Move[]> orderMovesCache = new Dictionary<ulong,Move[]>(100000);
    public Move[] getOrderedMoves(Board board, int depth) {
        #if DEBUG
        possibleMoveCount++;
        #endif

        //use pv (principal variation) as first move in array
        //use stack alloc to store array (order by captures then non captures)

        if (orderMovesCache.ContainsKey(board.ZobristKey)) {
            #if DEBUG
            possibleMoveCacheCount++;
            #endif
            return orderMovesCache[board.ZobristKey];
        }

        Move[] moves = board.GetLegalMoves();
        orderMovesCache[board.ZobristKey] = moves;

        return moves;
    }
    #if DEBUG
    int[][] pieceValueLocation = new int[][] {
        new int[] { //pawn
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            05,05,05,06,06,05,05,05,
            20,21,45,60,60,45,21,20,
            10,11,15,20,20,15,11,10,
            00,00,00,00,00,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            05,05,15,16,16,15,15,15,
            05,05,20,20,20,25,21,20,
            20,21,45,60,60,45,21,20,
            05,05,20,20,20,25,21,20,
            09,07,15,10,10,15,11,15,
            30,30,10,05,05,10,10,10,
        },
        new int[] {
            60,60,35,30,30,35,60,60,
            40,40,35,25,25,35,40,40,
            10,15,25,15,15,25,15,10,
            30,30,10,05,05,10,30,30,
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
            20,00,10,00,00,10,00,20,
            00,30,00,00,00,00,30,00,
            00,00,10,00,00,10,00,00,
        },
        new int[] {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-40,-10,-10,-40,-10,-20,
        },
        new int[] {
            -10,-05,-05,-05,-05,-05,-05,-10,
            -05,  0,  0,  0,  0,  0,  0,-05,
            -05,  0,  5, 10, 10,  5,  0,-05,
            -05,  5,  5, 10, 10,  5,  5,-05,
            -05,  0, 10, 10, 10, 10,  0,-05,
            -05, 10, 10, 10, 10, 10, 10,-05,
            -05,  5,  0,  0,  0,  0,  5,-05,
            -10,-05,-20,-05,-05,-20,-05,-10,
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
            15,15,15,20,20,15,15,15,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,10,10,10,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            15,15,15,20,20,15,15,15,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,10,10,10,00,00,
        },


        new int[] { //queen
            00,00,00,00,00,00,00,00,
            -8,-6,-5,-3,-3,-5,-6,-8,
            -8,-8,-6,-5,-5,-6,-8,-8,
            00,00,00,-3,-3,00,00,00,
            01,00,00,00,00,00,00,01,
            00,05,00,00,00,00,05,00,
            00,00,10,00,00,10,00,00,
            00,00,00,20,20,00,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            10,10,10,15,15,10,10,10,
            15,15,15,20,20,15,15,15,
            10,10,10,15,15,10,10,10,
            00,00,00,00,00,00,00,00,
            00,00,00,20,20,20,00,00,
        },
        new int[] {
            00,00,00,00,00,00,00,00,
            15,15,15,20,20,15,15,15,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,00,00,00,00,00,
            00,00,00,10,10,10,00,00,
        },


        new int[] { //king
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -20, -30, -30, -40, -40, -30, -30, -20,
            -10, -20, -20, -20, -20, -20, -20, -10, 
             20,  20,   0,   0,   0,   0,  20,  20,
             20,  30,  10,   0,   0,  10,  30,  20
        },
        new int[] {
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -20, -30, -30, -40, -40, -30, -30, -20,
            -10, -20, -20, -20, -20, -20, -20, -10, 
             20,  20,   0,   0,   0,   0,  20,  20,
             20,  30,  10,   0,   0,  10,  30,  20
        },
        new int[] {
            -50,-40,-30,-20,-20,-30,-40,-50,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -50,-30,-30,-30,-30,-30,-30,-50
        },
    };

    public void GameOver() {
        
    }
    #endif
}