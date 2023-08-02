using ChessChallenge.API;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Math;


public class Selenaut : IChessBot
{
    private const int TRANSPOSITION_TABLE_COUNT = 4_194_304;

    private const int QSEARCH_DEPTH = 6;

    private const byte TRANSPOSITION_EXACT = 0b11;
    private const byte TRANSPOSITION_LOWER_BOUND = 0b01;
    private const byte TRANSPOSITION_UPPER_BOUND = 0b10;

    private const int MIN_VALUE = -1_000_000;
    private const int MAX_VALUE = +1_000_000;

    private readonly int[] pieceValues = { 0, 0, 1, 1, 2, 4, 0 };
    private readonly int[] pieceEvalValues = { 0, 100, 310, 330, 500, 901, 20000 };
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

    Board board;
    Timer timer;
    int millisecondsRemaining;
    (ulong Hash, int Depth, int Value, byte Flag, Move Move, bool IsValid)[] transpositionTableEntries = new (ulong Hash, int Depth, int Value, byte Flag, Move Move, bool IsValid)[TRANSPOSITION_TABLE_COUNT];
    Move[] m_killerMoves = new Move[1024];
    int nodesSearched = 0;
    int ttHits = 0;
    int ttReturns = 0;

    bool ShouldStop => timer.MillisecondsElapsedThisTurn > millisecondsRemaining / 30 - 10;

    public Move Think(Board b, Timer t)
    {
        Console.WriteLine(b.GetFenString());
        board = b;
        timer = t;
        //transpositionTableEntries = new TranspositionTableEntry[TRANSPOSITION_TABLE_COUNT];
        millisecondsRemaining = t.MillisecondsRemaining;
        nodesSearched = 0;
        ttHits = 0;
        ttReturns = 0;

        var moves = board.GetLegalMoves();
        var bestMove = moves[0];
        int currentDepth = 1;
        for (; !ShouldStop; currentDepth++)
        {
            var ratedMoves = moves.Select(m =>
            {
                nodesSearched++;
                board.MakeMove(m);
                var value = -NegaMax(currentDepth, 0, MIN_VALUE, MAX_VALUE, board.IsWhiteToMove ? 1 : -1);
                board.UndoMove(m);
                return (move: m, eval: value);
            }).ToList()
            .OrderByDescending(m => m.eval)
            .ToList();
            if (!ShouldStop)
            {
                var bestRatedMove = ratedMoves[0];
                bestMove = bestRatedMove.move;
                Console.WriteLine($"depth {currentDepth}: {bestRatedMove.move}: {bestRatedMove.eval / 100.0 * (board.IsWhiteToMove ? 1 : -1):N1}");
            }
        }
        Console.WriteLine($"{nodesSearched / (timer.MillisecondsElapsedThisTurn + 1.0) * 1000:N1} nodes/s");
        Console.WriteLine($"{ttHits} TT Hits");
        Console.WriteLine($"{ttReturns} TT Returns");
        var tableFull = transpositionTableEntries.Count(t => t.IsValid);
        Console.WriteLine($"{tableFull}/{transpositionTableEntries.Length} ({tableFull * 1.0 / transpositionTableEntries.Length:p2})");
        Console.WriteLine(b.GetFenString());
        Console.Write("PV:");
        PrintPV(20);
        Console.WriteLine();
        return bestMove;
    }

    private void PrintPV(int depth)
    {
        ulong zHash = board.ZobristKey;
        var tp = transpositionTableEntries[zHash % TRANSPOSITION_TABLE_COUNT];
        if (tp.Flag != 0 && tp.Hash == zHash && depth >= 0)
        {
            Console.Write("{0} | ", tp.Move);
            board.MakeMove(tp.Move);
            PrintPV(depth - 1);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    int NegaMax(int depth, int ply, int alpha, int beta, int color)
    {
        nodesSearched++;
        int alphaOriginal = alpha;
        bool qSearch = depth <= 0;
        bool pvNode = beta > alpha + 1;
        Move bestMove = Move.NullMove;
        ref var entry = ref transpositionTableEntries[board.ZobristKey % TRANSPOSITION_TABLE_COUNT];
        if (entry.IsValid && entry.Hash == board.ZobristKey)
        {
            ttHits++;
            bestMove = entry.Move;
            if (!pvNode && entry.Depth >= depth)
            {
                if (entry.Flag == TRANSPOSITION_EXACT)
                {
                    ttReturns++;
                    return entry.Value;
                }
                if (entry.Flag == TRANSPOSITION_LOWER_BOUND && entry.Value >= beta)
                {
                    ttReturns++;
                    return entry.Value;
                }
                if (entry.Flag == TRANSPOSITION_UPPER_BOUND && entry.Value <= alpha)
                {
                    ttReturns++;
                    return entry.Value;
                }
            }
        }
        Span<Move> moves = stackalloc Move[128];
        board.GetLegalMovesNonAlloc(ref moves, qSearch && !board.IsInCheck());
        var standingPat = color * Eval(ply);
        if (board.IsDraw() || board.IsInCheckmate())
        {
            return standingPat;
        }
        if (moves.Length == 0) return standingPat;
        if (qSearch)
        {
            if (standingPat >= beta)
            {
                return standingPat;
            }
            if (standingPat > alpha)
            {
                alpha = standingPat;
            }
        }

        //RFP implementation
        if (!pvNode && !qSearch && depth <= 2 && !board.IsInCheck() && standingPat >= beta + 125 * depth)
        {
            return standingPat;
        }

        OrderMoves(ref moves, bestMove, depth);
        int bestEval = MIN_VALUE;
        for (int i = 0; i < moves.Length; i++)
        {
            if (ShouldStop)
            {
                return 0;
            }
            var move = moves[i];
            board.MakeMove(move);
            // NWS
            int eval = -NegaMax(depth - 1, ply + 1, (qSearch || i == 0) ? -beta : -alpha - 1, -alpha, -color);
            if (!qSearch && i != 0 && eval > alpha && eval < beta)
            {
                eval = -NegaMax(depth - 1, ply + 1, -beta, -alpha, -color);
            }
            board.UndoMove(move);
            alpha = Max(alpha, eval);
            if (eval > bestEval)
            {
                bestEval = eval;
                bestMove = move;
            }
            if (alpha >= beta)
            {
                break;
            }
        }
        if (!qSearch)
        {
            if (bestEval <= alphaOriginal)
            {
                entry.Flag = TRANSPOSITION_UPPER_BOUND;
            }
            else if (bestEval >= beta)
            {
                entry.Flag = TRANSPOSITION_LOWER_BOUND;
                if (!bestMove.IsCapture)
                {
                    m_killerMoves[depth + 30] = bestMove;
                }
            }
            else
            {

                entry.Flag = TRANSPOSITION_EXACT;
            }
            entry.Hash = board.ZobristKey;
            entry.IsValid = true;
            entry.Move = bestMove;
            entry.Value = bestEval;
            entry.Depth = depth;
        }
        return bestEval;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OrderMoves(ref Span<Move> moves, Move bestMove, int depth)
    {
        Span<int> priorities = stackalloc int[moves.Length];
        for (int i = 0; i < moves.Length; i++)
        {
            var move = moves[i];
            int value = 0;
            if (move == bestMove)
            {
                value += 100000;
            }
            if (move.IsCapture)
            {
                value += 1000 + 10 * (int)move.CapturePieceType - (int)move.MovePieceType;
            }
            if (depth >= 0 && move == m_killerMoves[depth + 30])
                value += 1;
            priorities[i] = -value;
        }
        priorities.Sort(moves);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private int Eval(int ply)
    {
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? MIN_VALUE + ply : MAX_VALUE - ply;
        if (board.IsDraw())
            return -1;
        //Pulled from JW's example bot's implementation of compressed PeSTO (ComPresSTO?)

        int mg = 0, eg = 0, phase = 0;

        foreach (bool stm in new[] { true, false })
        {
            for (var p = PieceType.Pawn; p <= PieceType.King; p++)
            {
                int piece = (int)p, ind;
                ulong mask = board.GetPieceBitboard(p, stm);
                while (mask != 0)
                {
                    phase += piecePhase[piece];
                    ind = 128 * (piece - 1) + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
                    mg += getPstVal(ind) + pieceEvalValues[piece];
                    eg += getPstVal(ind + 64) + pieceEvalValues[piece];
                }
            }
            mg = -mg;
            eg = -eg;
        }
        return (mg * phase + eg * (24 - phase)) / 24;
    }

    int getPstVal(int psq)
    {
        return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
    }

    public void GameOver() {
    }
}