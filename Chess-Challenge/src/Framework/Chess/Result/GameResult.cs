namespace ChessChallenge.Chess
{
    public enum GameResult
    {
        NotStarted,
        VoidResult,
        InProgress,
        WhiteIsMated,
        BlackIsMated,
        Stalemate,
        Repetition,
        FiftyMoveRule,
        InsufficientMaterial,
        DrawByArbiter,
        WhiteTimeout,
        BlackTimeout,
        WhiteIllegalMove,
        BlackIllegalMove
    }
}