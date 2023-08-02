using ChessChallenge.API;
using System;
using System.Linq;
using System.Numerics;

/*
    MyBot V1.0  ~(725 Brain Power SMH)

    Features
    Negamax Alpha Beta Pruning
    Score board based off piece locations
     Added Transposition table (remove move cache)

    NOTES
    When its end game, it incorrectly values moving the king as winning the game instead of promoting pawns and checkmating with queens.. this does not make sense. might be a problem with the Ttables


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

    Go back and correct MyBotV2_1's negamax/eval functions because i think they are flawed and is the reason it blundered pieces

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
public class MyBotV3 : IChessBot {
    Timer timer; Board board;
    bool shouldStop => timer.MillisecondsElapsedThisTurn > timePerMove;
    static Move nullMove = Move.NullMove;
    Move bestMove = nullMove, bestRootMove;
    int bestEval, bestRootEval, timePerMove = 500;
    (ulong zobristKey, int depthSearchedAfter, int eval, byte flag, Move Move)[] TTable = new (ulong zobristKey, int depthSearchedAfter, int eval, byte flag, Move Move)[TranspositionTableLength];
    private const int MIN_VALUE = -1_000_000,  MAX_VALUE = 1_000_000, TranspositionTableLength = 1<<22;

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
        #if DEBUG
        negamaxNodesCount = 0; tTableCacheCount = 0; tTableExpiredCacheCount = 0; boardEvalCount = 0;
        #endif
        board = cBoard;
        timer = cTimer;

        #if DEBUG
        Console.WriteLine("eval " + evaluate(0));
        #endif
        
        for (int depth = 1; depth <= 50 && !shouldStop; depth++) {
            int val = negamax(depth, 0, MIN_VALUE, MAX_VALUE);
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

        timePerMove = Math.Max(timer.MillisecondsRemaining >> 7 , 50);

        #if DEBUG
        Console.WriteLine($"{bestRootMove} ({bestRootEval}) | {cTimer.MillisecondsElapsedThisTurn}ms");
        Console.WriteLine($"{negamaxNodesCount} Negamax\t{boardEvalCount} boardEvals\t TTable ({(tTableCacheCount - tTableExpiredCacheCount) / (float)negamaxNodesCount}))");
        Console.WriteLine($"{1000 * negamaxNodesCount / (float) timer.MillisecondsElapsedThisTurn} Nodes/s");
        Console.WriteLine($"{tTableExpiredCacheCount} TtableExpired");
        #endif
        return bestRootMove;
    }

    #if DEBUG
    int negamaxNodesCount = 0, tTableCacheCount = 0, tTableExpiredCacheCount = 0, boardEvalCount = 0;
    #endif


    public int quiesence(int depth, int alpha, int beta) {
        #if DEBUG
        negamaxNodesCount++;
        #endif

        int stand_pat = evaluate(depth);
        if (stand_pat >= beta)
            return beta;
        if (alpha < stand_pat)
            alpha = stand_pat;

        Span<Move> moves = stackalloc Move[128];
        board.GetLegalMovesNonAlloc(ref moves, true);
        getOrderedMoves(ref moves, Move.NullMove, depth);

        foreach (Move move in moves) {
            board.MakeMove(move);
            int score = -quiesence(depth + 1, -beta, -alpha);
            board.UndoMove(move);

            if (score >= beta) return beta;
            if (score > alpha) alpha = score;
        }

        return alpha;
    }

    //negamax with alpha beta pruning
    public int negamax(int depthLeft, int depth, int alpha, int beta) {
        #if DEBUG
        negamaxNodesCount++;
        #endif

        if (depthLeft <= 0)
            return quiesence(depth, alpha, beta);
        
        Move prevBestMove = Move.NullMove;

        ref var transpositionTableEntry = ref TTable[board.ZobristKey % TranspositionTableLength];
        if (transpositionTableEntry.zobristKey == board.ZobristKey) {
            //todo do pv on current best move cached on this board position
            prevBestMove = transpositionTableEntry.Move;

            #if DEBUG
            tTableCacheCount++;
            #endif

            if (depth != 0 && transpositionTableEntry.depthSearchedAfter >= depthLeft) {
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
        
        Span<Move> moves = stackalloc Move[128];
        board.GetLegalMovesNonAlloc(ref moves);
        getOrderedMoves(ref moves, prevBestMove, depth);

        if (moves.Length == 0)
            return evaluate(depth);

        int highestEval = MIN_VALUE;
        Move highestMove = Move.NullMove;
        foreach (Move move in moves) {
            if (shouldStop)
                return 0;

            board.MakeMove(move);
            int eval = -negamax(depthLeft - 1, depth+1, -beta, -alpha) + (move.IsPromotion ? 20 * (int) move.PromotionPieceType : 0);
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

        byte flag = 0;
        if (highestEval <= beta) flag = 1;
        if (highestEval >= alpha) flag = 2;

        TTable[board.ZobristKey % TranspositionTableLength] = (board.ZobristKey, depth, highestEval, flag, highestMove);
        return highestEval;
    }

    //evaluates a position based on how desirable it is for the current player to play the next move
    public int evaluate(int depth) {
        #if DEBUG
        boardEvalCount++;
        #endif

        //dont want to reach these states
        if (board.IsDraw()) return 0;
        if (board.IsInCheckmate()) return  -30000 + depth;

        int eval = 0;
        //bonus points for having the right to castle
        if (board.HasKingsideCastleRight(board.IsWhiteToMove)) eval += 10;
        if (board.HasQueensideCastleRight(board.IsWhiteToMove)) eval += 5;

        //add up score for pieces and their locations at current game stage
        int mg = 0, eg = 0, phase = 0;

        foreach (bool stm in new[] { true, false }) {
            for (var p = PieceType.Pawn; p <= PieceType.King; p++) {
                int piece = (int)p, ind;
                ulong mask = board.GetPieceBitboard(p, stm);
                while (mask != 0)
                {
                    phase += piecePhase[piece];
                    ind = 128 * piece - 128 + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
                    mg += getPstVal(ind) + pieceEval[piece];
                    eg += getPstVal(ind + 64) + pieceEval[piece];
                }
            }
            mg = -mg;
            eg = -eg;
        }
        return (mg * phase + eg * (24 - phase)) / 24 * (board.IsWhiteToMove ? 1 : -1) + eval;
    }

    //todo add move ordering
    public void getOrderedMoves(ref Span<Move> moves, Move bestMove, int depth) {
        //use pv (principal variation) as first move in array
        //use stack alloc to store array (order by captures then non captures)
        Span<int> priorities = stackalloc int[moves.Length];
        for (int i = 0; i < moves.Length; i++) {
            var move = moves[i];
            int value = 0;
            if (move == bestMove)
                value += 100000;
            if (move.IsCapture)
                value += 1000 + 20 * (int)move.CapturePieceType - (int)move.MovePieceType;
            if (move.IsCastles)
                value += 100;
            if (move.IsEnPassant)
                value += 50;
            if (move.IsPromotion)
                value += 100 * (int) move.PromotionPieceType;
            priorities[i] = -value;
        }
        priorities.Sort(moves);
    }

    int getPstVal(int psq) {
        return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
    }
}