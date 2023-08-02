using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Numerics;

/*
    MyBot V1.0  ~(725 Brain Power SMH)

    Features
    Negamax Alpha Beta Pruning
    Score board based off piece locations
     Added Transposition table (remove move cache)

    NOTES

    Massive help from selenaut bot to fix issues with my negamax/eval functions trying to trade a queen for a knight and losing so many pieces without reason


    Against NNBot
    

    Against the EloBot1
    - 202 +/- 58 Elo difference
    - 119 / 17 / 31 (win rate 71.25%)


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
    OPTIMIZE CODE
    Move Pruning
    Late Move Reductions, History Reductions
    
*/
public class MyBotV2_1 : IChessBot {
    int timePerMove = 500;
    Timer timer;
    Board board;
    bool shouldStop => timer.MillisecondsElapsedThisTurn > timePerMove;
    static Move nullMove = Move.NullMove;
    Move bestMove = nullMove, bestRootMove;
    int bestEval, bestRootEval;
    private const int TranspositionTableLength = 1<<22;
    (ulong zobristKey, int depthSearchedAfter, int eval, byte flag, Move Move)[] TTable = new (ulong zobristKey, int depthSearchedAfter, int eval, byte flag, Move Move)[TranspositionTableLength];
    private const int MIN_VALUE = -1_000_000;
    private const int MAX_VALUE = 1_000_000;

    int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };
    ulong[] psts = {
    657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086,
    364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588,
    421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452,
    162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453,
    347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514,
    329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460,
    257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958,
    384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824,
    365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484,
    329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047,
    347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452,
    384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716,
    366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428,
    329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844,
    329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863,
    419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224,
    366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995,
    365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612,
    401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596,
    67159620133902};
    private readonly int[] pieceEval = { 0, 100, 310, 330, 500, 901, 20000};

    public Move Think(Board cBoard, Timer cTimer) {
        board = cBoard;
        timer = cTimer;

        //Game stage
        int pieceCount = BitOperations.PopCount(board.AllPiecesBitboard);

        #if DEBUG
        Console.WriteLine("eval " + evaluate(0));
        #endif
        
        for (int depth = 1; depth <= 50 && !shouldStop; depth++) {
            int val = negamax(depth, 0, -100000, 100000, board.IsWhiteToMove ? 1 : -1);
            if (!shouldStop) {
                bestRootEval = val;
                bestRootMove = bestMove;
            }
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
        Console.WriteLine($"{bestRootMove} ({bestRootEval}) | {cTimer.MillisecondsElapsedThisTurn}ms");
        Console.WriteLine($"{negamaxNodesCount} Negamax\t{boardEvalCount} boardEvals\t{tTableCount} tTable ({(tTableCacheCount - tTableExpiredCacheCount) / (float)negamaxNodesCount}))");
        Console.WriteLine($"{tTableExpiredCacheCount} TtableExpired");
        #endif
        return bestRootMove;
    }

    #if DEBUG
    int negamaxNodesCount = 0, tTableCount = 0, tTableCacheCount = 0, tTableExpiredCacheCount = 0, boardEvalCount = 0;
    #endif

    //negamax with alpha beta pruning
    public int negamax(int depthLeft, int depth, int alpha, int beta, int color) {
        #if DEBUG
        negamaxNodesCount++;
        #endif
        Move prevBestMove;

        ref var transpositionTableEntry = ref TTable[board.ZobristKey % TranspositionTableLength];
        if (transpositionTableEntry.zobristKey == board.ZobristKey) {
        
            //todo do pv on current best move cached on this board position
            prevBestMove = transpositionTableEntry.Move;

            #if DEBUG
            tTableCacheCount++;
            #endif

            if (transpositionTableEntry.depthSearchedAfter >= depthLeft) {
                if (transpositionTableEntry.flag == 0)
                    return transpositionTableEntry.eval;
                else if (transpositionTableEntry.flag == 1 && transpositionTableEntry.eval >= beta)
                    return transpositionTableEntry.eval;
                else if (transpositionTableEntry.flag == 2 && transpositionTableEntry.eval <= alpha)
                    return transpositionTableEntry.eval;
            }

            #if DEBUG
            tTableExpiredCacheCount++;
            #endif
        }

        if (depthLeft == 0 || board.IsInCheckmate() || board.IsDraw())
            return color * evaluate(depth);

        Move[] moves = getOrderedMoves(depth);

        int highestEval = MIN_VALUE;
        Move highestMove = Move.NullMove;
        foreach (Move move in moves) {
            if (shouldStop) {
                return 0;
            }

            board.MakeMove(move);
            int eval = -negamax(depthLeft - 1, depth+1, -beta, -alpha, -color);
            board.UndoMove(move);

            if (eval > highestEval) {
                highestEval = eval;
                highestMove = move;

                if (depth == 0) {
                    bestMove = move;
                    bestEval = highestEval;
                }
            }
            alpha = Math.Max(alpha, eval);
            if (alpha >= beta)
                break;
        }


        #if DEBUG
        tTableCount++;
        #endif

        transpositionTableEntry.depthSearchedAfter = depthLeft;
        transpositionTableEntry.eval = highestEval;
        transpositionTableEntry.Move = highestMove;
        transpositionTableEntry.flag = 0;
        if (highestEval <= beta) transpositionTableEntry.flag = 1;
        if (highestEval >= alpha) transpositionTableEntry.flag = 2;
        transpositionTableEntry.zobristKey = board.ZobristKey;

        return highestEval;
    }

    //evaluates a position based on how desirable it is for the current player to play the next move
    public int evaluate(int depth) {
        #if DEBUG
        boardEvalCount++;
        #endif

        //dont want to reach these states
        if (board.IsDraw()) return -1;
        if (board.IsInCheckmate()) return  board.IsWhiteToMove ? MIN_VALUE + depth * 10 : MAX_VALUE - depth * 10;

        //add up score for pieces and their locations at current game stage
        int mg = 0, eg = 0, phase = 0;

        foreach (bool stm in new[] { true, false }) {
            for (var p = PieceType.Pawn; p <= PieceType.King; p++) {
                int piece = (int)p, ind;
                ulong mask = board.GetPieceBitboard(p, stm);
                while (mask != 0)
                {
                    phase += piecePhase[piece];
                    ind = 128 * (piece - 1) + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
                    mg += getPstVal(ind) + pieceEval[piece];
                    eg += getPstVal(ind + 64) + pieceEval[piece];
                }
            }
            mg = -mg;
            eg = -eg;
        }
        return (mg * phase + eg * (24 - phase)) / 24;
    }

    //todo add move ordering
    public Move[] getOrderedMoves(int depth) {
        //use pv (principal variation) as first move in array
        //use stack alloc to store array (order by captures then non captures)


        Move[] moves = board.GetLegalMoves();
        return moves;
    }

    int getPstVal(int psq) {
        return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
    }

    public void GameOver() {
        
    }
}