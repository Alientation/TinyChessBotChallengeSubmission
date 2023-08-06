
using System.Text;
using ChessChallenge.Application;

/*
Todo, make it save games respective to each unique challenge controller
*/
namespace ChessChallenge.Chess {

    public static class PGNCreator {

        public static string CreatePGN(Move[] moves, ChallengeController controller) {
            return CreatePGN(controller, moves, GameResult.InProgress, FenUtility.StartPositionFEN);
        }

        public static string CreatePGN(ChallengeController controller, Board board, GameResult result, string whiteName = "", string blackName = "") {
            return CreatePGN(controller, board.AllGameMoves.ToArray(), result, board.GameStartFen, whiteName, blackName);
        }

        private static int numGames = 0;
        private static int numMatches = 0;
        private static int lastMatchID = -1;
        public static string CreatePGN(ChallengeController controller, Move[] moves, GameResult result, string startFen, string whiteName = "", string blackName = "") {
            startFen = startFen.Replace("\n", "").Replace("\r", "");

            StringBuilder pgn = new();
            Board board = new();
            board.LoadPosition(startFen);
            numGames++;
            if (controller.GetMatchID() != lastMatchID) {
                numMatches++;
                numGames = 1;
                lastMatchID = controller.GetMatchID();
            }

            pgn.AppendLine($"[Match #{numMatches} Game #{numGames}]");

            // Headers
            if (result is GameResult.WhiteIsMated or GameResult.BlackIsMated)
                pgn.AppendLine($"[\"{(result == GameResult.WhiteIsMated ? whiteName : blackName)}\" is mated]");
            if (result is GameResult.WhiteIllegalMove or GameResult.BlackIllegalMove)
                pgn.AppendLine($"[\"{(result == GameResult.WhiteIsMated ? whiteName : blackName)}\" made an illegal Move]");
            if (result is GameResult.WhiteTimeout or GameResult.BlackTimeout)
                pgn.AppendLine($"[\"{(result == GameResult.WhiteIsMated ? whiteName : blackName)}\" had timeout]");

            if (!string.IsNullOrEmpty(whiteName))
                pgn.AppendLine($"[White \"{whiteName}\"]");
            if (!string.IsNullOrEmpty(blackName))
                pgn.AppendLine($"[Black \"{blackName}\"]");
            
            if (startFen != FenUtility.StartPositionFEN)
                pgn.AppendLine($"[FEN \"{startFen}\"]");
            if (result is not GameResult.NotStarted or GameResult.InProgress)
                pgn.AppendLine($"[Result \"{result}\"]");

            for (int plyCount = 0; plyCount < moves.Length; plyCount++) {
                string moveString = MoveUtility.GetMoveNameSAN(moves[plyCount], board);
                board.MakeMove(moves[plyCount]);

                if (plyCount % 2 == 0)
                    pgn.Append((plyCount / 2 + 1) + ". ");
                pgn.Append(moveString + " ");
            }

            return pgn.ToString();
        }
    }
}