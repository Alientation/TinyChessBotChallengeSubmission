
using ChessChallenge.Chess;

namespace ChessChallenge.API
{
    public interface IChessBot
    {
        Move Think(Board board, Timer timer);
        void GameOver();
    }
}
