using System;
using ChessChallenge.API;

public class MagnusCarlBot : IChessBot
{
    private int maxDepth = 5;
    private Move bestMove;
    private Board board;
    int positionsEvaluated = 0;
    Timer timer;
    // Point values for each piece type for evaluation
    int[] pointValues = {100, 320, 330, 500, 900, 99999};
    public struct Transposition
    {
        public ulong zobristHash;
        public Move move;
        public int evaluation;
        public sbyte depth;
        public byte flag;
    };
    Transposition[] m_TPTable;

    public MagnusCarlBot()
    {
        m_TPTable = new Transposition[0x800000];
    }
        // Big table packed with data from premade piece square tables
    
    private readonly ulong[,] PackedEvaluationTables = {
        { 58233348458073600, 61037146059233280, 63851895826342400, 66655671952007680 },
        { 63862891026503730, 66665589183147058, 69480338950193202, 226499563094066 },
        { 63862895153701386, 69480338782421002, 5867015520979476,  8670770172137246 },
        { 63862916628537861, 69480338782749957, 8681765288087306,  11485519939245081 },
        { 63872833708024320, 69491333898698752, 8692760404692736,  11496515055522836 },
        { 63884885386256901, 69502350490469883, 5889005753862902,  8703755520970496 },
        { 63636395758376965, 63635334969551882, 21474836490,       1516 },
        { 58006849062751744, 63647386663573504, 63625396431020544, 63614422789579264 }
    };

    public int GetSquareBonus(PieceType type, bool isWhite, int file, int rank)
    {
        // Because arrays are only 4 squares wide, mirror across files
        if (file > 3)
            file = 7 - file;

        // Mirror vertically for white pieces, since piece arrays are flipped vertically
        if (isWhite)
            rank = 7 - rank;

        // First, shift the data so that the correct byte is sitting in the least significant position
        // Then, mask it out
        // Use unchecked to preserve the sign in case of an overflow
        sbyte unpackedData = unchecked((sbyte)((PackedEvaluationTables[rank, file] >> 8 * ((int)type - 1)) & 0xFF));

        // Invert eval scores for black pieces
        return isWhite ? unpackedData : -unpackedData;
    }
    
    // Negamax algorithm with alpha-beta pruning
    int Search(int depth, int alpha, int beta, int color)
    {
        // If the search reaches the desired depth or the end of the game, evaluate the position and return its value
        if (depth == 0 || board.IsDraw() || board.IsInCheckmate())
        {
            if (board.IsDraw()) return 0;
            
            if (board.IsInCheckmate()) return -10000 + (maxDepth - depth);
            
            return EvaluateBoard();
        }

        Move[] legalMoves = board.GetLegalMoves();
        int bestEval = -99999;
        int eval;
        int startingAlpha = alpha;
        ref Transposition transposition = ref m_TPTable[board.ZobristKey & 0x7FFFFF];
        if(transposition.zobristHash == board.ZobristKey && transposition.depth >= depth)
        {
            //Console.WriteLine("Transpositon Shit :)");
            //If we have an "exact" score (a < score < beta) just use that
            if(transposition.flag == 1) return transposition.evaluation;
            //If we have a lower bound better than beta, use that
            if(transposition.flag == 2 && transposition.evaluation >= beta)  return transposition.evaluation;
            //If we have an upper bound worse than alpha, use that
            if(transposition.flag == 3 && transposition.evaluation <= alpha) return transposition.evaluation;
        }
        int[] scores = new int[legalMoves.Length];
        for(int i = 0; i < legalMoves.Length; i++) {
            Move move = legalMoves[i];

            if(move.IsCapture) scores[i] = (int)move.CapturePieceType - (int)move.MovePieceType;
        }
        for (int i = 0; legalMoves.Length > i; i++)
        {
            if(timer.MillisecondsElapsedThisTurn >= 1000 ){  Console.WriteLine("MoveTimeout");return 50000 * -color;}
            // Incrementally sort moves
            for(int j = i + 1; j < legalMoves.Length; j++) {
                if(scores[j] > scores[i])
                    (scores[i], scores[j], legalMoves[i], legalMoves[j]) = (scores[j], scores[i], legalMoves[j], legalMoves[i]);
            }
            Move move = legalMoves[i];
            // Make the move on a temporary board and call search recursively
            board.MakeMove(move);
            positionsEvaluated += 1;
            eval = -Search(depth -1, -beta, -alpha, -color);
            board.UndoMove(move);

            // Update the best move and prune if necessary
            if (eval > bestEval)   
            {
                bestEval = eval;
                if (depth == maxDepth) bestMove = move;
                
                // Improve alpha
                alpha = Math.Max(alpha, eval);
                
                if (alpha >= beta) break;
            }
            transposition.evaluation = bestEval;
            transposition.zobristHash = board.ZobristKey;
            transposition.move = bestMove;
            if(bestEval < startingAlpha) transposition.flag = 3;
            else if(bestEval >= beta) transposition.flag = 2;
            else transposition.flag = 1;
            transposition.depth = (sbyte)depth;

        
        }
        return bestEval;
    }
    public int EvaluateBoard()
    {
        int materialValue = 0;
        int mobilityValue = board.GetLegalMoves().Length;
        PieceList[] pieceLists = board.GetAllPieceLists();
        int color = board.IsWhiteToMove ? 1 : -1;
        int pieceCount = 0;
        // Loop through each piece type and add the difference in material value to the total
        int squereBonus = 0;
        foreach(PieceList pList in pieceLists)
        {
            pieceCount += pList.Count;
        }
        foreach(PieceList pList in pieceLists)
        {
            foreach(Piece piece in pList)
            {
                squereBonus += GetSquareBonus(piece.PieceType,piece.IsWhite,piece.Square.File, piece.Square.Rank);
            }
        }
        for(int i = 0;i < 5; i++){
            materialValue += (pieceLists[i].Count - pieceLists[i + 6].Count) * pointValues[i];
        }
        return materialValue * color + mobilityValue * color + squereBonus;
    }
    public Move Think(Board boardInput, Timer timerInput)
    {
        this.board = boardInput;
        timer = timerInput;
        positionsEvaluated = 0;
        for(int depth = 1; depth <= 50; depth++) {
            int score = Search(depth, -99999, 99999, board.IsWhiteToMove ? 1 : -1);
            
            if (timer.MillisecondsElapsedThisTurn >=  timer.MillisecondsRemaining / 60)
            {
                Console.WriteLine("Depth :" + depth.ToString() + " Time :" + timer.MillisecondsElapsedThisTurn.ToString());
                break;
            }
        }
        // Call the Minimax algorithm to find the best move
        
        Console.WriteLine(positionsEvaluated/* + " " + bestMove*/);
        return bestMove;
    }

}