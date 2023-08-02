﻿using ChessChallenge.Chess;
using ChessChallenge.Example;
using Raylib_cs;
using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ChessChallenge.Application.Settings;
using static ChessChallenge.Application.ConsoleHelper;

namespace ChessChallenge.Application
{
    public class ChallengeController
    {
        
        public enum PlayerType
        {
            Human,
            V1__MyBotV1, V1__MyBotV1NoDebug,
            V1__MyBotV1_1, V1__MyBotV1_2, V1__MyBotV1_3, V1__MyBotV1_4,
            V2__MyBotV2, V2__MyBotV2_1, V2__MyBotV2_2,
            MyBotV3,
            EvilBot, 
            Enemy__NNBot, Enemy__EloBot0,
            Enemy__EloBot1, Enemy__EloBot2,
            Enemy__HumanBot, Enemy__SelenautBot,
            Enemy__LiteBlueEngine__LiteBlueEngine1, Enemy__LiteBlueEngine__LiteBlueEngine2, Enemy__LiteBlueEngine__LiteBlueEngine3, Enemy__LiteBlueEngine__LiteBlueEngine4, Enemy__LiteBlueEngine__LiteBlueEngine5,
        }

        public static PlayerType[] ActivePlayers = {
                PlayerType.Human,           PlayerType.V2__MyBotV2_2,
                PlayerType.Enemy__NNBot,    PlayerType.MyBotV3,
                PlayerType.Enemy__EloBot0,  PlayerType.Enemy__SelenautBot,
                PlayerType.Enemy__EloBot1,  PlayerType.Enemy__HumanBot, 
                PlayerType.Enemy__EloBot2,  PlayerType.EvilBot,
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine1, PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine4,
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine2, PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine5,
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine3,
        };

        ChessPlayer CreatePlayer(PlayerType type)
        {
            return type switch
            {
                PlayerType.V1__MyBotV1 => new ChessPlayer(new MyBotV1(), type, GameDurationMilliseconds),
                PlayerType.V1__MyBotV1NoDebug => new ChessPlayer(new MyBotV1NoDebug(), type, GameDurationMilliseconds),
                PlayerType.V1__MyBotV1_1 => new ChessPlayer(new MyBotV1_1(), type, GameDurationMilliseconds),
                PlayerType.V1__MyBotV1_2 => new ChessPlayer(new MyBotV1_2(), type, GameDurationMilliseconds),
                PlayerType.V1__MyBotV1_3 => new ChessPlayer(new MyBotV1_3(), type, GameDurationMilliseconds),
                PlayerType.V1__MyBotV1_4 => new ChessPlayer(new MyBotV1_4(), type, GameDurationMilliseconds),

                PlayerType.V2__MyBotV2 => new ChessPlayer(new MyBotV2(), type, GameDurationMilliseconds),
                PlayerType.V2__MyBotV2_1 => new ChessPlayer(new MyBotV2_1(), type, GameDurationMilliseconds),
                PlayerType.V2__MyBotV2_2 => new ChessPlayer(new MyBotV2_2(), type, GameDurationMilliseconds),

                PlayerType.MyBotV3 => new ChessPlayer(new MyBotV3(), type, GameDurationMilliseconds),

                PlayerType.EvilBot => new ChessPlayer(new EvilBot(), type, GameDurationMilliseconds),

                PlayerType.Enemy__NNBot => new ChessPlayer(new NNBot(), type, GameDurationMilliseconds),

                PlayerType.Enemy__EloBot0 => new ChessPlayer(new EloBot0(), type, GameDurationMilliseconds),
                PlayerType.Enemy__EloBot1 => new ChessPlayer(new EloBot1(), type, GameDurationMilliseconds),
                PlayerType.Enemy__EloBot2 => new ChessPlayer(new EloBot2(), type, GameDurationMilliseconds),

                PlayerType.Enemy__HumanBot => new ChessPlayer(new HumanBot(), type, GameDurationMilliseconds),
                PlayerType.Enemy__SelenautBot => new ChessPlayer(new SelenautBot(), type, GameDurationMilliseconds),

                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine1 => new ChessPlayer(new LiteBlueEngine1(), type, GameDurationMilliseconds),
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine2 => new ChessPlayer(new LiteBlueEngine2(), type, GameDurationMilliseconds),
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine3 => new ChessPlayer(new LiteBlueEngine3(), type, GameDurationMilliseconds),
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine4 => new ChessPlayer(new LiteBlueEngine4(), type, GameDurationMilliseconds),
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine5 => new ChessPlayer(new LiteBlueEngine5(), type, GameDurationMilliseconds),
                
                _ => new ChessPlayer(new HumanPlayer(boardUI), type)
            };
        }

        public static PlayerType player1Type = PlayerType.Human;
        public static PlayerType player2Type = PlayerType.Human;
        public static PlayerType botToTest1 = PlayerType.MyBotV3;
        public static PlayerType botToTest2 = PlayerType.Enemy__EloBot1;

        // Game state
        readonly Random rng;
        int gameID;
        bool isPlaying;
        Board board;
        public ChessPlayer PlayerWhite { get; private set; }
        public ChessPlayer PlayerBlack {get;private set;}

        float lastMoveMadeTime;
        bool isWaitingToPlayMove;
        Move moveToPlay;
        float playMoveTime;
        public bool HumanWasWhiteLastGame { get; private set; }

        // Bot match state
        readonly string[] botMatchStartFens;
        int botMatchGameIndex;
        public BotMatchStats BotStatsA { get; private set; }
        public BotMatchStats BotStatsB {get;private set;}
        bool botAPlaysWhite;


        // Bot task
        AutoResetEvent botTaskWaitHandle;
        bool hasBotTaskException;
        ExceptionDispatchInfo botExInfo;

        // Other
        readonly BoardUI boardUI;
        readonly MoveGenerator moveGenerator;
        int tokenCount1;
        int debugTokenCount1;
        int tokenCount2;
        int debugTokenCount2;
        
        readonly StringBuilder pgns;

        public ChallengeController()
        {
            Log($"Launching Chess-Challenge version {Settings.Version}");
            (tokenCount1, debugTokenCount1) = GetTokenCount(botToTest1);
            (tokenCount2, debugTokenCount2) = GetTokenCount(botToTest2);
            Warmer.Warm();

            rng = new Random();
            moveGenerator = new();
            boardUI = new BoardUI();
            board = new Board();
            pgns = new();

            BotStatsA = new BotMatchStats(botToTest1.ToString());
            BotStatsB = new BotMatchStats(botToTest2.ToString());
            botMatchStartFens = FileHelper.ReadResourceFile("Fens.txt").Split('\n').Where(fen => fen.Length > 0).ToArray();
            botTaskWaitHandle = new AutoResetEvent(false);

            StartNewGame(PlayerType.Human, botToTest1);
        }


        public static PlayerType[] tournament =  new PlayerType[] {

        };
        /*
        0 = winCount
        1 = drawCount
        2 = loseCount
        3 = illegalMoveCount
        4 = timeoutCount
        5 = 
        6 = gamesPlayed
        */
        public static int[,] tournamentScores = new int[tournament.Length,5];
        public int tournamentMatchesPerMatchUp = 1;
        
        //TODO
        public void StartTournament() {
            tournamentScores = new int[tournament.Length,5];
        }

        public void EndGame() {
            EndGame(GameResult.DrawByArbiter, log: false, autoStartNextBotMatch: false);
            gameID = rng.Next();

            // Stop prev task and create a new one
            if (RunBotsOnSeparateThread) {
                // Allow task to terminate
                botTaskWaitHandle.Set();
            }
        }

        public void StartNewGame(PlayerType whiteType, PlayerType blackType)
        {
            // End any ongoing game
            EndGame(GameResult.DrawByArbiter, log: false, autoStartNextBotMatch: false);
            gameID = rng.Next();

            player1Type = whiteType;
            player2Type = blackType;

            // Stop prev task and create a new one
            if (RunBotsOnSeparateThread)
            {
                // Allow task to terminate
                botTaskWaitHandle.Set();
                // Create new task
                botTaskWaitHandle = new AutoResetEvent(false);
                Task.Factory.StartNew(BotThinkerThread, TaskCreationOptions.LongRunning);
            }
            // Board Setup
            board = new Board();
            bool isGameWithHuman = whiteType is PlayerType.Human || blackType is PlayerType.Human;
            int fenIndex = isGameWithHuman ? 0 : botMatchGameIndex / 2;
            board.LoadPosition(botMatchStartFens[fenIndex]);

            // Player Setup
            PlayerWhite = CreatePlayer(whiteType);
            PlayerBlack = CreatePlayer(blackType);
            PlayerWhite.SubscribeToMoveChosenEventIfHuman(OnMoveChosen);
            PlayerBlack.SubscribeToMoveChosenEventIfHuman(OnMoveChosen);

            // UI Setup
            boardUI.UpdatePosition(board);
            boardUI.ResetSquareColours();
            (tokenCount1, debugTokenCount1) = GetTokenCount(player1Type);
            (tokenCount2, debugTokenCount2) = GetTokenCount(player2Type);
            SetBoardPerspective();

            // Start
            isPlaying = true;
            NotifyTurnToMove();
        }

        void BotThinkerThread()
        {
            int threadID = gameID;
            //Console.WriteLine("Starting thread: " + threadID);

            while (true)
            {
                // Sleep thread until notified
                botTaskWaitHandle.WaitOne();
                // Get bot move
                if (threadID == gameID)
                {
                    var move = GetBotMove();

                    if (threadID == gameID)
                    {
                        OnMoveChosen(move);
                    }
                }
                // Terminate if no longer playing this game
                if (threadID != gameID)
                {
                    break;
                }
            }
            //Console.WriteLine("Exitting thread: " + threadID);
        }

        Move GetBotMove()
        {
            API.Board botBoard = new(board);
            try
            {
                API.Timer timer = new(PlayerToMove.TimeRemainingMs, PlayerNotOnMove.TimeRemainingMs, GameDurationMilliseconds, IncrementMilliseconds);
                API.Move move = PlayerToMove.Bot.Think(botBoard, timer);
                return new Move(move.RawValue);
            }
            catch (Exception e)
            {
                Log("An error occurred while bot was thinking.\n" + e.ToString(), true, ConsoleColor.Red);
                hasBotTaskException = true;
                botExInfo = ExceptionDispatchInfo.Capture(e);
            }
            return Move.NullMove;
        }



        void NotifyTurnToMove()
        {
            //playerToMove.NotifyTurnToMove(board);
            if (PlayerToMove.IsHuman)
            {
                PlayerToMove.Human.SetPosition(FenUtility.CurrentFen(board));
                PlayerToMove.Human.NotifyTurnToMove();
            }
            else
            {
                if (RunBotsOnSeparateThread)
                {
                    botTaskWaitHandle.Set();
                }
                else
                {
                    double startThinkTime = Raylib.GetTime();
                    var move = GetBotMove();
                    double thinkDuration = Raylib.GetTime() - startThinkTime;
                    PlayerToMove.UpdateClock(thinkDuration);
                    OnMoveChosen(move);
                }
            }
        }

        void SetBoardPerspective()
        {
            // Board perspective
            if (PlayerWhite.IsHuman || PlayerBlack.IsHuman)
            {
                boardUI.SetPerspective(PlayerWhite.IsHuman);
                HumanWasWhiteLastGame = PlayerWhite.IsHuman;
            }
            else if (PlayerWhite.Bot is not EvilBot && PlayerWhite.Bot is not HumanPlayer && PlayerBlack.Bot is not EvilBot && PlayerBlack.Bot is not HumanPlayer)
            {
                boardUI.SetPerspective(true);
            }
            else
            {
                boardUI.SetPerspective(PlayerWhite.Bot is not EvilBot && PlayerWhite.Bot is not HumanPlayer);
            }
        }

        static (int totalTokenCount, int debugTokenCount) GetTokenCount(PlayerType botType)
        {   
            if (botType == PlayerType.Human) return (0,0);

            string path = Path.Combine(Directory.GetCurrentDirectory(), "src", "My Bot");
            foreach (string t in (botType + "").Split("__"))
                path = Path.Combine(path, t);
            path = path + ".cs";

            if (botType == PlayerType.EvilBot) path = Path.Combine(Directory.GetCurrentDirectory(), "src", "Evil Bot", "EvilBot.cs");

            using StreamReader reader = new(path);
            string txt = reader.ReadToEnd();
            return TokenCounter.CountTokens(txt);
        }

        void OnMoveChosen(Move chosenMove)
        {
            if (IsLegal(chosenMove))
            {
                PlayerToMove.AddIncrement(IncrementMilliseconds);
                if (PlayerToMove.IsBot)
                {
                    moveToPlay = chosenMove;
                    isWaitingToPlayMove = true;
                    playMoveTime = lastMoveMadeTime + MinMoveDelay;
                }
                else
                {
                    PlayMove(chosenMove);
                }
            }
            else
            {
                string moveName = MoveUtility.GetMoveNameUCI(chosenMove);
                string log = $"Illegal move: {moveName} in position: {FenUtility.CurrentFen(board)}";
                Log(log, true, ConsoleColor.Red);
                GameResult result = PlayerToMove == PlayerWhite ? GameResult.WhiteIllegalMove : GameResult.BlackIllegalMove;
                EndGame(result);
            }
        }

        void PlayMove(Move move)
        {
            if (isPlaying)
            {
                bool animate = PlayerToMove.IsBot;
                lastMoveMadeTime = (float)Raylib.GetTime();

                board.MakeMove(move, false);
                boardUI.UpdatePosition(board, move, animate);

                GameResult result = Arbiter.GetGameState(board);
                if (result == GameResult.InProgress)
                {
                    NotifyTurnToMove();
                }
                else
                {
                    EndGame(result);
                }
            }
        }

        void EndGame(GameResult result, bool log = true, bool autoStartNextBotMatch = true)
        {
            if (isPlaying)
            {
                isPlaying = false;
                isWaitingToPlayMove = false;
                gameID = -1;

                if (log)
                {
                    Log("Game Over: " + result, false, ConsoleColor.Blue);
                }

                string pgn = PGNCreator.CreatePGN(board, result, GetPlayerName(PlayerWhite), GetPlayerName(PlayerBlack));
                pgns.AppendLine(pgn);

                // If 2 bots playing each other, start next game automatically.
                if (PlayerWhite.IsBot && PlayerBlack.IsBot)
                {
                    UpdateBotMatchStats(result);
                    botMatchGameIndex++;
                    int numGamesToPlay = botMatchStartFens.Length * 2;

                    if (botMatchGameIndex < numGamesToPlay && autoStartNextBotMatch)
                    {
                        botAPlaysWhite = !botAPlaysWhite;
                        const int startNextGameDelayMs = 600;
                        System.Timers.Timer autoNextTimer = new(startNextGameDelayMs);
                        int originalGameID = gameID;
                        autoNextTimer.Elapsed += (s, e) => AutoStartNextBotMatchGame(originalGameID, autoNextTimer);
                        autoNextTimer.AutoReset = false;
                        autoNextTimer.Start();

                    }
                    else if (autoStartNextBotMatch)
                    {
                        Log("Match finished", false, ConsoleColor.Blue);
                    }
                }
            }
        }

        private void AutoStartNextBotMatchGame(int originalGameID, System.Timers.Timer timer)
        {
            if (originalGameID == gameID)
            {
                StartNewGame(PlayerBlack.PlayerType, PlayerWhite.PlayerType);
            }
            timer.Close();
        }


        void UpdateBotMatchStats(GameResult result)
        {
            UpdateStats(BotStatsA, botAPlaysWhite);
            UpdateStats(BotStatsB, !botAPlaysWhite);

            void UpdateStats(BotMatchStats stats, bool isWhiteStats)
            {
                // Draw
                if (Arbiter.IsDrawResult(result))
                {
                    stats.NumDraws++;
                }
                // Win
                else if (Arbiter.IsWhiteWinsResult(result) == isWhiteStats)
                {
                    stats.NumWins++;
                }
                // Loss
                else
                {
                    stats.NumLosses++;
                    stats.NumTimeouts += (result is GameResult.WhiteTimeout or GameResult.BlackTimeout) ? 1 : 0;
                    stats.NumIllegalMoves += (result is GameResult.WhiteIllegalMove or GameResult.BlackIllegalMove) ? 1 : 0;
                }
            }
        }

        public void Update()
        {
            if (isPlaying)
            {
                PlayerWhite.Update();
                PlayerBlack.Update();

                PlayerToMove.UpdateClock(Raylib.GetFrameTime());
                if (PlayerToMove.TimeRemainingMs <= 0)
                {
                    EndGame(PlayerToMove == PlayerWhite ? GameResult.WhiteTimeout : GameResult.BlackTimeout);
                }
                else
                {
                    if (isWaitingToPlayMove && Raylib.GetTime() > playMoveTime)
                    {
                        isWaitingToPlayMove = false;
                        PlayMove(moveToPlay);
                    }
                }
            }

            if (hasBotTaskException)
            {
                hasBotTaskException = false;
                botExInfo.Throw();
            }
        }

        public void Draw()
        {
            boardUI.Draw();
            string nameW = GetPlayerName(PlayerWhite);
            string nameB = GetPlayerName(PlayerBlack);
            boardUI.DrawPlayerNames(nameW, nameB, PlayerWhite.TimeRemainingMs, PlayerBlack.TimeRemainingMs, isPlaying);
        }

        public void DrawOverlay()
        {
            if (PlayerBlack.IsHuman)
                BotBrainCapacityUI.Draw(MenuUI.getShortName((int) player1Type), MenuUI.getShortName((int) player2Type),tokenCount1, debugTokenCount1, tokenCount2, debugTokenCount2, MaxTokenCount);
            else {
                BotBrainCapacityUI.Draw(MenuUI.getShortName((int) player2Type), MenuUI.getShortName((int) player1Type),tokenCount2, debugTokenCount2, tokenCount1, debugTokenCount1, MaxTokenCount);
            }
            MenuUI.DrawButtons(this);
            MatchStatsUI.DrawMatchStats(this);
        }

        static string GetPlayerName(ChessPlayer player) => GetPlayerName(player.PlayerType);
        static string GetPlayerName(PlayerType type) => type.ToString();

        public void StartNewBotMatch(PlayerType botTypeA, PlayerType botTypeB)
        {
            EndGame(GameResult.DrawByArbiter, log: false, autoStartNextBotMatch: false);
            botMatchGameIndex = 0;
            string nameA = GetPlayerName(botTypeA);
            string nameB = GetPlayerName(botTypeB);
            if (nameA == nameB)
            {
                nameA += " (A)";
                nameB += " (B)";
            }
            BotStatsA = new BotMatchStats(nameA);
            BotStatsB = new BotMatchStats(nameB);
            botAPlaysWhite = true;
            Log($"Starting new match: {nameA} vs {nameB}", false, ConsoleColor.Blue);
            StartNewGame(botTypeA, botTypeB);
        }


        ChessPlayer PlayerToMove => board.IsWhiteToMove ? PlayerWhite : PlayerBlack;
        ChessPlayer PlayerNotOnMove => board.IsWhiteToMove ? PlayerBlack : PlayerWhite;

        public int TotalGameCount => botMatchStartFens.Length * 2;
        public int CurrGameNumber => Math.Min(TotalGameCount, botMatchGameIndex + 1);
        public string AllPGNs => pgns.ToString();


        bool IsLegal(Move givenMove)
        {
            var moves = moveGenerator.GenerateMoves(board);
            foreach (var legalMove in moves)
            {
                if (givenMove.Value == legalMove.Value)
                {
                    return true;
                }
            }

            return false;
        }

        public class BotMatchStats
        {
            public string BotName;
            public int NumWins;
            public int NumLosses;
            public int NumDraws;
            public int NumTimeouts;
            public int NumIllegalMoves;

            public BotMatchStats(string name) => BotName = name;
        }

        public void Release()
        {
            boardUI.Release();
        }
    }
}
