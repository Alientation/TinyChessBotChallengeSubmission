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
public class MyBotV1_4 : IChessBot {
    bool isBotWhite;
    int timePerMove = 500;
    int gameStage = 0;
    Timer cTimer;
    Board cBoard;
    (Move, int) bestMove = (Move.NullMove, 0);

    public Move Think(Board board, Timer timer) {
        if (cTimer == null) {
            isBotWhite = board.IsWhiteToMove;
            if (!isBotWhite) {
                for (int i = 0; i < pieceValueLocation.Length; i++) {
                    Array.Reverse(pieceValueLocation[i]);
                }
            }
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
            negamax(board, depth, 0, -300000000, 300000000);
            #if DEBUG
            Console.WriteLine("depth " + depth + " " + bestMove.Item1 + " (" + bestMove.Item2 +") | " + "(" + timer.MillisecondsElapsedThisTurn + "ms)");
            #endif
        }

        if (timer.MillisecondsRemaining < 30000) timePerMove = 300;
        if (timer.MillisecondsRemaining < 15000) timePerMove = 150;

        #if DEBUG
        Console.WriteLine("Chosen Move " + bestMove.Item1 + " (" + bestMove.Item2 + ") -> [" + "nodes=" + negamaxNodesCount + ", eval=" + boardEvalCount + " (" + 
        (boardEvalCacheCount / (float) boardEvalCount) + "), moveEval=" + moveEvalCount + ", findMove=" + possibleMoveCount + " (" + (possibleMoveCacheCount / (float) possibleMoveCount) + ")"); 
        #endif
        return bestMove.Item1;
    }

    #if DEBUG
    int negamaxNodesCount = 0;
    int boardEvalCount = 0;
    int boardEvalCacheCount = 0;
    int moveEvalCount = 0;
    int possibleMoveCount = 0;
    int possibleMoveCacheCount = 0;
    #endif

    public (Move, int) negamax(Board board, int depthLeft, int depth, int alpha, int beta) {
        #if DEBUG
        negamaxNodesCount++;
        #endif
        if (board.IsDraw()) return (Move.NullMove, -100);
        if (board.IsInCheckmate()) return (Move.NullMove, board.PlyCount - 100000000);

        if (depthLeft == 0 || board.IsInCheckmate() || board.IsInStalemate() || board.IsFiftyMoveDraw() || board.IsInsufficientMaterial()) {
            return (Move.NullMove, evaluate(board));
        }

        Move[] moves = getOrderedMoves(board);

        var best = (Move.NullMove, -200000);
        foreach (Move move in moves) {
            board.MakeMove(move);
            int eval = evaluateMove(move);

            eval -= negamax(board, depthLeft - 1, depth+1, -beta, -alpha).Item2;
            board.UndoMove(move);

            if (eval > best.Item2) {
                best = (move, eval);
                if (depth == 0) bestMove = best;
            }

            alpha = Math.Max(alpha, eval);
            if (alpha >= beta) {
                break;
            }
        }
        return (best);
    }

    int[] pieceValue = { 0, 100, 300, 320, 500, 900, 10000 };

    Dictionary<ulong,int> boardEvalCache = new Dictionary<ulong,int>(1000000);
    public int evaluate(Board board) {
        #if DEBUG
        boardEvalCount++;
        #endif
        if (boardEvalCache.ContainsKey(board.ZobristKey)) {
            #if DEBUG
            boardEvalCacheCount++;
            #endif
            return boardEvalCache[board.ZobristKey];
        }

        if (board.IsDraw()) return -100;
        if (board.IsInCheckmate()) return  board.IsWhiteToMove == isBotWhite ? -100000 : 100000;

        int eval = 40 - board.PlyCount;
        if (board.IsInCheck()) eval -= 50;

        foreach (bool flag in new[] {true, false}) {
            int piecesEval = 0;
            for (var p = PieceType.Pawn; p <= PieceType.King; p++) {
                ulong mask = board.GetPieceBitboard(p, flag);
                while (mask != 0) {
                    int index = BitboardHelper.ClearAndGetIndexOfLSB(ref mask);
                    piecesEval += pieceValue[(int)p] + pieceValueLocation[((int)p - 1) * 3 + gameStage][index];
                }
                eval += piecesEval * (flag == board.IsWhiteToMove ? 1 : -1);
            }
        }
        boardEvalCache[board.ZobristKey] = eval;
        return eval;
    }

    public int evaluateMove(Move move) {
        #if DEBUG
        moveEvalCount++;
        #endif
        if (move.IsCapture) return ((int)move.CapturePieceType - (int)move.MovePieceType) * 20;
        if (move.IsCastles) return 80;
        if (move.IsEnPassant) return 60;
        if (move.IsPromotion) return 140;
        return 0;
    }

    Dictionary<ulong,Move[]> orderMovesCache = new Dictionary<ulong,Move[]>(100000);
    public Move[] getOrderedMoves(Board board) {
        #if DEBUG
        possibleMoveCount++;
        #endif
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