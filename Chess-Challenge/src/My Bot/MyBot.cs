using ChessChallenge.API;
using System;
using System.Linq;

/*
    MyBot V3.6  ~(780 Brain Power)

    Features (dif from previous version 3.5)
    Improved Move Ordering performance

    Todo
    History Heuristic
    Null move pruning
    check extensions
    RFP
    Futility pruning

    NOTES
    

    30.6 +/- 18.3 compared to MyBotV3_5
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
    Late Move Reductions, History Reductions
*/

public class MyBot : IChessBot {

    //save tokens by storing references here
    Timer timer; Board board;

    //search info
    bool shouldStop => timer.MillisecondsElapsedThisTurn > timePerMove;
    Move bestRootMove;
    int timePerMove;

    //TTable
    record struct TTableEntry(ulong zobristKey, int depth, int eval, int flag, Move Move);
    TTableEntry[] TTable = new TTableEntry[0x400000];

    private const int MIN_VALUE = -100000,  MAX_VALUE = 100000;

    //PeSTO evaluation
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
                                            94, 281, 297, 512, 936, 20000}; // Endgame


    #if DEBUG
    int nodesWithoutQuiesence = 0, nodesWithQuiesence = 0, terminalNodesWithoutQuiesence = 0, terminalNodesWithQuiesence = 0;
    #endif


    public Move Think(Board cBoard, Timer cTimer) {
        board = cBoard;
        timer = cTimer;
        timePerMove = timer.MillisecondsRemaining / 40;

        //prevent illegal moves
        bestRootMove = board.GetLegalMoves()[0];

        //iterative deepening, while there is still time left
        for (int depth = 1; !shouldStop && depth < 50; ) {
            #if DEBUG
            Console.WriteLine("d" + (depth-1) + " " + cTimer.MillisecondsElapsedThisTurn + "ms");
            #endif

            if (Negamax(++depth, 0, MIN_VALUE, MAX_VALUE) > 50000) break;
        }

        return bestRootMove;
    }


    //negamax with alpha beta pruning
    public int Negamax(int depth, int ply, int alpha, int beta) {


        bool isInCheck = board.IsInCheck(), root = ply == 0;
        int highestEval = MIN_VALUE;
        Move highestMove = Move.NullMove;

        // Check for draw by repetition
        if (!root && board.IsRepeatedPosition()) return 0;
        if (isInCheck) depth++;
        bool quiesence = depth < 1;

        //check caches
        TTableEntry TTEntry = TTable[board.ZobristKey & 0x3FFFFF];
        if (TTEntry.zobristKey == board.ZobristKey && depth != 0 && TTEntry.depth >= depth && !root && 
            (TTEntry.flag == 1 || TTEntry.flag == 0 && TTEntry.eval <= alpha || TTEntry.flag == 2 && TTEntry.eval >= beta))
            return TTEntry.eval;

        //check for cutoff in quiesence search
        if (quiesence) {
            highestEval = Evaluate();
            if (highestEval >= beta)
                return beta;
            alpha = Math.Max(alpha, highestEval);
        }

        //faster sort than inline
        Move[] moves = board.GetLegalMoves(quiesence && !isInCheck);
        int[] movesScore = new int[moves.Length];
        for (int i = -1; ++i < moves.Length; ) {
            Move move = moves[i];
            movesScore[i] -= move == TTEntry.Move ? 1000000 :
                            move.IsCapture ? 1000 * (int)move.CapturePieceType - (int)move.MovePieceType :
                            0;
        }
        Array.Sort(movesScore, moves);

        //no possible moves (which means checkmate/stalemate/draw)
        if (!quiesence && moves.Length == 0)
            return isInCheck ? MIN_VALUE + depth : 0;

        //find best move possible from all subtrees
        foreach (Move move in moves) {
            if (shouldStop)
                return MAX_VALUE;

            board.MakeMove(move);
            int eval = -Negamax(depth - 1, ply + 1, -beta, -alpha);
            board.UndoMove(move);

            //check for cutoffs and update best moves
            if (eval > highestEval) {
                highestEval = eval;
                highestMove = move;

                if (root)
                    bestRootMove = move;

                alpha = Math.Max(alpha, eval);

                if (alpha >= beta)
                    break;
            }
        }

        //mark and cache search result
        TTable[board.ZobristKey & 0x3FFFFF] = new TTableEntry(board.ZobristKey, depth, highestEval, highestEval >= beta ? 2 : highestEval > alpha ? 1 : 0, highestMove);

        return highestEval;
    }

    //evaluates a position based on how desirable it is for the current player to play the next move
    public int Evaluate() {
        //add up score for pieces and their locations at current game stage
        int mg = 0, eg = 0, phase = 0;
        // Iterate through both players
        foreach (bool stm in new[] { true, false }) {
            // Iterate through all piece types
            for (int piece = -1; ++piece < 6;) {
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
        return (mg * phase + eg * (24 - phase)) / 24 * (board.IsWhiteToMove ? 1 : -1); 
        // + (board.HasKingsideCastleRight(board.IsWhiteToMove) ? 15 : board.HasQueensideCastleRight(board.IsWhiteToMove) ? 5 : 0)
    }

    //expands the pesto tables from their compressed versions
    public MyBot() {
        UnpackedPestoTables = new int[64][];
        UnpackedPestoTables = PackedPestoTables.Select(packedTable => {
            int pieceType = 0;
            return decimal.GetBits(packedTable).Take(3)
                .SelectMany(c => BitConverter.GetBytes(c)
                    .Select((byte square) => (int)((sbyte)square * 1.461) + pieceEval[pieceType++]))
                .ToArray();
        }).ToArray();
    }
}