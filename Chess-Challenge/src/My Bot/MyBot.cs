//#define PRINT

using ChessChallenge.API;
using System;
using System.Linq;

/*
    MyBot V3.1  ~(1008 Brain Power SMH)

    Features
    Negamax Alpha Beta Pruning
    Score board based off piece locations and phases (?dunno what this is tbh) with emphasis on the stage of the game
    Transposition table
    Time management (fail safe by preventing unfinished depth searches from affecting results)
    Quiescense searching (only applies to moves that result in a capture)
    Move ordering (basic)

    Todo
    History Heuristic (move ordering)
    Null move pruning
    check extensions
    RFP
    MVV-LVA
    delta pruning

    NOTES
    Bot fails at end game, potentially some problems with TT tables
    
*/

/*
    TODO
    Go back and correct MyBotV2_1's negamax/eval functions because i think they are flawed and is the reason it blundered pieces

    add more features to board evaluation
        - pawn advancement
        - piece mobility
        - piece threats
        - piece protection
    Null Move Heuristic (find eval of opponent moving two times in a row to get the minimum alpha value)
    OPTIMIZE CODE
    Move Pruning
    Late Move Reductions, History Reductions
    
*/

public class MyBot : IChessBot {

    //save tokens by storing references here
    Timer timer; Board board;

    //is this a lambda function??
    bool shouldStop => timer.MillisecondsElapsedThisTurn > timePerMove;

    //save who knows how many tokens (like 1 or 2 maybe)
    static Move nullMove = Move.NullMove;

    //best move from the current depth, best move for the search as a whole
    Move bestMove, bestRootMove;
    int bestEval, bestRootEval, timePerMove;

    //TTable (also Thanks @Selenaut for the extremely compact version)
    (ulong zobristKey, int depth, int eval, int flag, Move Move)[] TTable = new (ulong zobristKey, int depth, int eval, int flag, Move Move)[TranspositionTableLength];
    private const int MIN_VALUE = -1_000_000,  MAX_VALUE = 1_000_000, TranspositionTableLength = 2097152;

    //piece eval tables
    int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };
    private readonly decimal[] PackedPestoTables = {
        63746705523041458768562654720m, 71818693703096985528394040064m, 75532537544690978830456252672m, 75536154932036771593352371712m, 76774085526445040292133284352m, 3110608541636285947269332480m, 936945638387574698250991104m, 75531285965747665584902616832m,
        77047302762000299964198997571m, 3730792265775293618620982364m, 3121489077029470166123295018m, 3747712412930601838683035969m, 3763381335243474116535455791m, 8067176012614548496052660822m, 4977175895537975520060507415m, 2475894077091727551177487608m,
        2458978764687427073924784380m, 3718684080556872886692423941m, 4959037324412353051075877138m, 3135972447545098299460234261m, 4371494653131335197311645996m, 9624249097030609585804826662m, 9301461106541282841985626641m, 2793818196182115168911564530m,
        77683174186957799541255830262m, 4660418590176711545920359433m, 4971145620211324499469864196m, 5608211711321183125202150414m, 5617883191736004891949734160m, 7150801075091790966455611144m, 5619082524459738931006868492m, 649197923531967450704711664m,
        75809334407291469990832437230m, 78322691297526401047122740223m, 4348529951871323093202439165m, 4990460191572192980035045640m, 5597312470813537077508379404m, 4980755617409140165251173636m, 1890741055734852330174483975m, 76772801025035254361275759599m,
        75502243563200070682362835182m, 78896921543467230670583692029m, 2489164206166677455700101373m, 4338830174078735659125311481m, 4960199192571758553533648130m, 3420013420025511569771334658m, 1557077491473974933188251927m, 77376040767919248347203368440m,
        73949978050619586491881614568m, 77043619187199676893167803647m, 1212557245150259869494540530m, 3081561358716686153294085872m, 3392217589357453836837847030m, 1219782446916489227407330320m, 78580145051212187267589731866m, 75798434925965430405537592305m,
        68369566912511282590874449920m, 72396532057599326246617936384m, 75186737388538008131054524416m, 77027917484951889231108827392m, 73655004947793353634062267392m, 76417372019396591550492896512m, 74568981255592060493492515584m, 70529879645288096380279255040m,
    };
    private readonly int[][] UnpackedPestoTables;
    private readonly short[] pieceEval = {  82, 337, 365, 477, 1025, 20000, // Middlegame
                                            94, 281, 297, 512, 936, 20000}; //Endgame

    public Move Think(Board cBoard, Timer cTimer) {
        #if PRINT
        negamaxNodesCount = 0; tTableCacheCount = 0; tTableExpiredCacheCount = 0; boardEvalCount = 0;
        #endif

        board = cBoard;
        timer = cTimer;
        timePerMove = timer.MillisecondsRemaining / 40;
        
        //reset best moves
        Move[] moves = cBoard.GetLegalMoves();
        bestMove = bestRootMove = moves[0];

        #if PRINT
        Console.WriteLine("eval " + Evaluate(0));
        #endif
        
        //iterative deepening, while there is still time left
        for (int depth = 1; depth <= 50 && !shouldStop; depth++) {

            //dont need value technically, just for PRINT purposes
            int val = Negamax(depth, 0, MIN_VALUE, MAX_VALUE);
            
            //if the search was not canceled because of running out of time
            if (!shouldStop) {
                bestRootMove = bestMove;

                #if PRINT
                bestRootEval = val;
                #endif
            }

            #if PRINT
            Console.WriteLine("depth " + depth + " " + bestMove + " (" + val +") | " + "(" + timer.MillisecondsElapsedThisTurn + "ms)");
            #endif
        }

        //best root move is default move, so set it to the best move ever found
        //should only ever occur if the bot runs out of time at the first depth search which means this is useless then
        //check to see if we can just set it to a random move
        if (bestRootMove == moves[0]) {
            bestRootMove = bestMove;

            #if PRINT
            bestRootEval = bestEval;
            #endif
        }

        #if PRINT
        Console.WriteLine($"{bestRootMove} ({bestRootEval}) | {cTimer.MillisecondsElapsedThisTurn}ms");
        Console.WriteLine($"{negamaxNodesCount} Negamax\t{boardEvalCount} boardEvals\t TTable ({(tTableCacheCount - tTableExpiredCacheCount) / (float)negamaxNodesCount}))");
        Console.WriteLine($"{1000 * negamaxNodesCount / (float) timer.MillisecondsElapsedThisTurn} Nodes/s");
        Console.WriteLine($"{tTableExpiredCacheCount} TtableExpired");
        #endif
        return bestRootMove;
    }

    #if PRINT
    int negamaxNodesCount = 0, tTableCacheCount = 0, tTableExpiredCacheCount = 0, boardEvalCount = 0;
    #endif

    //quiesence searching till a quiet position is reached
    public int Quiesence(int depth, int alpha, int beta) {
        #if PRINT
        negamaxNodesCount++;
        #endif

        int stand_pat = Evaluate(depth);
        if (stand_pat >= beta)
            return beta;
        if (alpha < stand_pat)
            alpha = stand_pat;

        Span<Move> moves = stackalloc Move[128];
        board.GetLegalMovesNonAlloc(ref moves, true);
        GetOrderedMoves(ref moves, Move.NullMove, depth);

        //search ordered moves
        foreach (Move move in moves) {
            board.MakeMove(move);
            int score = -Quiesence(depth + 1, -beta, -alpha);
            board.UndoMove(move);

            if (score >= beta) return beta;
            if (score > alpha) alpha = score;
        }

        return alpha;
    }

    //negamax with alpha beta pruning
    public int Negamax(int depthLeft, int depth, int alpha, int beta) {
        if (depthLeft < 1) //full search is completed, now search for a quiet position
            return Quiesence(depth, alpha, beta);

        //dont want to reach these states
        if (board.IsDraw()) return 0;
        if (board.IsInCheckmate()) return  MIN_VALUE + depth;
        
        Move prevBestMove = nullMove;

        //check caches
        ref var transpositionTableEntry = ref TTable[board.ZobristKey % TranspositionTableLength];
        if (transpositionTableEntry.zobristKey == board.ZobristKey) {
            //todo do pv on current best move cached on this board position
            prevBestMove = transpositionTableEntry.Move;

            #if PRINT
            tTableCacheCount++;
            #endif

            if (depth != 0 && transpositionTableEntry.depth >= depth && (transpositionTableEntry.flag == 1 ||
                transpositionTableEntry.flag == 0 && transpositionTableEntry.eval <= alpha ||
                transpositionTableEntry.flag == 2 && transpositionTableEntry.eval >= beta))
                    return transpositionTableEntry.eval;

            #if PRINT
            tTableExpiredCacheCount++;
            #endif
        }

        #if PRINT
        negamaxNodesCount++;
        #endif

        
        Span<Move> moves = stackalloc Move[128];
        board.GetLegalMovesNonAlloc(ref moves);
        GetOrderedMoves(ref moves, prevBestMove, depth);

        //no possible moves (which means checkmate/stalemate/draw)
        if (moves.Length == 0)
            return Evaluate(depth);

        //find best move possible from all subtrees
        int highestEval = MIN_VALUE;
        Move highestMove = Move.NullMove;
        foreach (Move move in moves) {
            if (shouldStop)
                return 0;

            board.MakeMove(move);
            int eval = -Negamax(depthLeft - 1, depth+1, -beta, -alpha) + (move.IsPromotion ? 20 * (int) move.PromotionPieceType : 0);
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

        //mark and cache
        TTable[board.ZobristKey % TranspositionTableLength] = (board.ZobristKey, depth, highestEval, highestEval >= beta ? 2 : highestEval > alpha ? 1 : 0, highestMove);
        return highestEval;
    }

    //evaluates a position based on how desirable it is for the current player to play the next move
    public int Evaluate(int depth) {
        #if PRINT
        boardEvalCount++;
        #endif

        int eval = 0;
        //bonus points for having the right to castle
        if (board.HasKingsideCastleRight(board.IsWhiteToMove)) eval += 10;
        if (board.HasQueensideCastleRight(board.IsWhiteToMove)) eval += 5;

        //add up score for pieces and their locations at current game stage
        int mg = 0, eg = 0, phase = 0;
        // Iterate through both players
        foreach (bool stm in new[] { true, false })
        {
            // Iterate through all piece types
            for (int piece = -1; ++piece < 6;)
            {
                // Get piece bitboard
                ulong bb = board.GetPieceBitboard((PieceType)(piece + 1), stm);

                // Iterate through each individual piece
                while (bb != 0)
                {
                    // Get square index for pst based on color
                    int sq = BitboardHelper.ClearAndGetIndexOfLSB(ref bb) ^ (stm ? 56 : 0);
                    // Increment mg and eg score
                    mg += UnpackedPestoTables[sq][piece];
                    eg += UnpackedPestoTables[sq][piece + 6];
                    // Updating position phase
                    phase += piecePhase[piece];
                }
            }
            mg = -mg;
            eg = -eg;
        }

        // In case of premature promotion
        phase = Math.Min(phase, 24);

        // Tapered evaluation
        return (mg * phase + eg * (24 - phase)) / 24 * (board.IsWhiteToMove ? 1 : -1) + eval;
    }

    //todo add move ordering
    public void GetOrderedMoves(ref Span<Move> moves, Move prevBestMove, int depth) {
        //use pv (principal variation) as first move in array
        //use stack alloc to store array (order by captures then non captures)
        Span<int> priorities = stackalloc int[moves.Length];
        for (int i = 0; i < moves.Length; i++) {
            var move = moves[i];
            //this causes immense lag, figure out if we can hash a board's eval
            //board.MakeMove(move);
            //priorities[i] = -evaluate(depth);
            //board.UndoMove(move);

            //prioritize lower eval pieces moving
            int priority = - 10 * (int) move.MovePieceType;
            if (move == prevBestMove) priority += MAX_VALUE;

            //bonuses for capture, promotion, enpassant, castles
            if (move.IsCapture) priority += 1000 * (int) move.CapturePieceType - (int) move.MovePieceType;
            if (move.IsPromotion) priority += 100 * (int) move.PromotionPieceType;
            if (move.IsEnPassant || move.IsCastles) priority += 20;

            priorities[i] = -priority;
        }
        //sort
        priorities.Sort(moves);
    }

    //expands the pesto tables from their compressed versions
    public MyBot() {
        UnpackedPestoTables = new int[64][];
        UnpackedPestoTables = PackedPestoTables.Select(packedTable =>
        {
            int pieceType = 0;
            return decimal.GetBits(packedTable).Take(3)
                .SelectMany(c => BitConverter.GetBytes(c)
                    .Select((byte square) => (int)((sbyte)square * 1.461) + pieceEval[pieceType++]))
                .ToArray();
        }).ToArray();
    }
}