using ChessChallenge.Chess;
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

/*
    Todo
    
    create tournament mode
    - round robin 2 * ## games per match up (games are a random set ## of FEN starting positions)
    - calculate elo differentiation between bots, rank the bots
    - store results/elo in a tournament file

    each game played with a bot will store its respective results into a file for that specific bot
    stores the game PGN and the enemy bot name in the file
    stores the results of the game (win/loss/draw/illegal move/timeout) + time left for each bot
    hashes the bot file and stores the hash in the file under the respective game

    let window be resizable

    create a list view under the dropdown list so more bots can be shown
    let user input time control per side and time increment per move and rounds to play
    Allow for multiple games to be played at once (for better speed in tournament mode)


*/

namespace ChessChallenge.Application {
    public class ChallengeController {
        
        public enum PlayerType {
            Human,
            V1__MyBotV1, V1__MyBotV1NoDebug,
            V1__MyBotV1_1, V1__MyBotV1_2, V1__MyBotV1_3, V1__MyBotV1_4,
            V2__MyBotV2, V2__MyBotV2_1, V2__MyBotV2_2,
            MyBotV3, MyBotV3_1, MyBotV3_2, MyBotV3_3,
            EvilBot, 
            Enemy__NNBot, Enemy__EloBot0,
            Enemy__EloBot1, Enemy__EloBot2,
            Enemy__HumanBot, Enemy__SelenautBot,
            Enemy__LiteBlueEngine__LiteBlueEngine1, Enemy__LiteBlueEngine__LiteBlueEngine2, Enemy__LiteBlueEngine__LiteBlueEngine3, Enemy__LiteBlueEngine__LiteBlueEngine4, Enemy__LiteBlueEngine__LiteBlueEngine5,
            Enemy__MagnusCarlBot, Enemy__StormwindV2,
        }

        public static PlayerType[] ActivePlayers = {
                PlayerType.Human,           PlayerType.MyBotV3_1,
                PlayerType.V2__MyBotV2_2,    PlayerType.MyBotV3_3,
                PlayerType.V1__MyBotV1,    PlayerType.V1__MyBotV1_4,
                PlayerType.Enemy__EloBot0,  PlayerType.Enemy__SelenautBot,
                PlayerType.Enemy__EloBot1,  PlayerType.Enemy__HumanBot, 
                PlayerType.Enemy__EloBot2,  PlayerType.EvilBot,
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine1, PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine4,
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine2, PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine5,
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine3, PlayerType.Enemy__MagnusCarlBot,
                PlayerType.Enemy__StormwindV2, PlayerType.Enemy__NNBot,
        };

        ChessPlayer CreatePlayer(PlayerType type, int gameDurationMilliseconds = DefaultGameDurationMilliseconds) {
            return type switch {
                PlayerType.V1__MyBotV1 => new ChessPlayer(new MyBotV1(), type, gameDurationMilliseconds),
                PlayerType.V1__MyBotV1NoDebug => new ChessPlayer(new MyBotV1NoDebug(), type, gameDurationMilliseconds),
                PlayerType.V1__MyBotV1_1 => new ChessPlayer(new MyBotV1_1(), type, gameDurationMilliseconds),
                PlayerType.V1__MyBotV1_2 => new ChessPlayer(new MyBotV1_2(), type, gameDurationMilliseconds),
                PlayerType.V1__MyBotV1_3 => new ChessPlayer(new MyBotV1_3(), type, gameDurationMilliseconds),
                PlayerType.V1__MyBotV1_4 => new ChessPlayer(new MyBotV1_4(), type, gameDurationMilliseconds),

                PlayerType.V2__MyBotV2 => new ChessPlayer(new MyBotV2(), type, gameDurationMilliseconds),
                PlayerType.V2__MyBotV2_1 => new ChessPlayer(new MyBotV2_1(), type, gameDurationMilliseconds),
                PlayerType.V2__MyBotV2_2 => new ChessPlayer(new MyBotV2_2(), type, gameDurationMilliseconds),

                PlayerType.MyBotV3 => new ChessPlayer(new MyBotV3(), type, gameDurationMilliseconds),
                PlayerType.MyBotV3_1 => new ChessPlayer(new MyBotV3_1(), type, gameDurationMilliseconds),
                PlayerType.MyBotV3_2 => new ChessPlayer(new MyBotV3_2(), type, gameDurationMilliseconds),
                PlayerType.MyBotV3_3 => new ChessPlayer(new MyBotV3_3(), type, gameDurationMilliseconds),

                PlayerType.EvilBot => new ChessPlayer(new EvilBot(), type, gameDurationMilliseconds),

                PlayerType.Enemy__NNBot => new ChessPlayer(new NNBot(), type, gameDurationMilliseconds),

                PlayerType.Enemy__EloBot0 => new ChessPlayer(new EloBot0(), type, gameDurationMilliseconds),
                PlayerType.Enemy__EloBot1 => new ChessPlayer(new EloBot1(), type, gameDurationMilliseconds),
                PlayerType.Enemy__EloBot2 => new ChessPlayer(new EloBot2(), type, gameDurationMilliseconds),

                PlayerType.Enemy__HumanBot => new ChessPlayer(new HumanBot(), type, gameDurationMilliseconds),
                PlayerType.Enemy__SelenautBot => new ChessPlayer(new SelenautBot(), type, gameDurationMilliseconds),

                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine1 => new ChessPlayer(new LiteBlueEngine1(), type, gameDurationMilliseconds),
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine2 => new ChessPlayer(new LiteBlueEngine2(), type, gameDurationMilliseconds),
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine3 => new ChessPlayer(new LiteBlueEngine3(), type, gameDurationMilliseconds),
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine4 => new ChessPlayer(new LiteBlueEngine4(), type, gameDurationMilliseconds),
                PlayerType.Enemy__LiteBlueEngine__LiteBlueEngine5 => new ChessPlayer(new LiteBlueEngine5(), type, gameDurationMilliseconds),

                PlayerType.Enemy__MagnusCarlBot => new ChessPlayer(new MagnusCarlBot(), type, gameDurationMilliseconds),
                
                PlayerType.Enemy__StormwindV2 => new ChessPlayer(new StormwindV2(), type, gameDurationMilliseconds),
                
                _ => new ChessPlayer(new HumanPlayer(boardUI), type)
            };
        }

        public static PlayerType player1Type = PlayerType.Human, player2Type = PlayerType.Human;
        public static PlayerType botToTest1 = PlayerType.MyBotV3_1, botToTest2 = PlayerType.Enemy__EloBot1;

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
        int botMatchStartFensIndex;
        readonly string[][] botMatchStartFens;

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
        int tokenCount1, debugTokenCount1, tokenCount2, debugTokenCount2;
        public bool fastForward;
        readonly StringBuilder pgns;
        int gameDuration1Milliseconds = Settings.DefaultGameDurationMilliseconds, gameDuration2Milliseconds = Settings.DefaultGameDurationMilliseconds;
        int increment1Milliseconds = Settings.DefaultIncrementMilliseconds, increment2Milliseconds = Settings.DefaultIncrementMilliseconds;

        public ChallengeController() {
            Log($"Launching Chess-Challenge version {Settings.Version}");
            (tokenCount1, debugTokenCount1) = GetTokenCount(botToTest1);
            (tokenCount2, debugTokenCount2) = GetTokenCount(botToTest2);
            Warmer.Warm();

            rng = new Random();
            moveGenerator = new();
            boardUI = new BoardUI();
            board = new Board();
            pgns = new();
            fastForward = false;

            BotStatsA = new BotMatchStats(botToTest1.ToString());
            BotStatsB = new BotMatchStats(botToTest2.ToString());

            //read in all fens

            DirectoryInfo d = new DirectoryInfo(FileHelper.GetResourcePath("Fens")); //Assuming Test is your Folder

            FileInfo[] files = d.GetFiles("*.txt"); //Getting Text files
            botMatchStartFens = new string[files.Length][];
            botMatchStartFensIndex = 0;

            for (int i = 0; i < files.Length; i++)
                botMatchStartFens[i] = FileHelper.ReadResourceFile("Fens\\" + files[i].Name).Split('\n').Where(fen => fen.Length > 0).ToArray();
            

            //botMatchStartFens = FileHelper.ReadResourceFile("Fens.txt").Split('\n').Where(fen => fen.Length > 0).ToArray();

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

        public void EndGame(bool keepResults) {
            EndGame(keepResults ? GameResult.DrawByArbiter : GameResult.VoidResult, log: false, autoStartNextBotMatch: false);
            gameID = rng.Next();

            // Stop prev task and create a new one
            if (RunBotsOnSeparateThread) {
                // Allow task to terminate
                botTaskWaitHandle.Set();
            }
        }

        public void StartNewGame(PlayerType whiteType, PlayerType blackType, 
            int timeControl1 = DefaultGameDurationMilliseconds, int timeIncrement1 = DefaultIncrementMilliseconds,
            int timeControl2 = DefaultGameDurationMilliseconds, int timeIncrement2 = DefaultIncrementMilliseconds,
            int currentBotMatchStartFensIndex = -1) {
            // End any ongoing game
            EndGame(GameResult.DrawByArbiter, log: false, autoStartNextBotMatch: false);
            gameID = rng.Next();

            player1Type = whiteType;
            player2Type = blackType;

            increment1Milliseconds = timeIncrement1;
            increment2Milliseconds = timeIncrement2;

            // Stop prev task and create a new one
            if (RunBotsOnSeparateThread) {
                // Allow task to terminate
                botTaskWaitHandle.Set();
                // Create new task
                botTaskWaitHandle = new AutoResetEvent(false);
                Task.Factory.StartNew(BotThinkerThread, TaskCreationOptions.LongRunning);
            }
            // Board Setup
            board = new Board();
            bool isGameWithHuman = whiteType is PlayerType.Human || blackType is PlayerType.Human;

            if (isGameWithHuman)
                board.LoadPosition(FenUtility.StartPositionFEN);
            else {
                int fenIndex = botMatchGameIndex / 2;
                currentBotMatchStartFensIndex = currentBotMatchStartFensIndex == -1 ? botMatchStartFensIndex : currentBotMatchStartFensIndex; 
                board.LoadPosition(botMatchStartFens[currentBotMatchStartFensIndex][fenIndex]);
            }

            // Player Setup
            PlayerWhite = CreatePlayer(whiteType,timeControl1);
            PlayerBlack = CreatePlayer(blackType,timeControl2);
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

        void BotThinkerThread() {
            int threadID = gameID;
            //Console.WriteLine("Starting thread: " + threadID);

            while (true) {
                // Sleep thread until notified
                botTaskWaitHandle.WaitOne();
                // Get bot move
                if (threadID == gameID) {
                    var move = GetBotMove();

                    if (threadID == gameID)
                        OnMoveChosen(move);
                }
                // Terminate if no longer playing this game
                if (threadID != gameID)
                    break;
            }
            //Console.WriteLine("Exitting thread: " + threadID);
        }

        Move GetBotMove() {
            API.Board botBoard = new(board);
            try {
                API.Timer timer = new(PlayerToMove.TimeRemainingMs, PlayerNotOnMove.TimeRemainingMs, PlayerToMove.PlayerType == player1Type ? gameDuration1Milliseconds : gameDuration2Milliseconds, PlayerToMove.PlayerType == player1Type ? increment1Milliseconds : increment2Milliseconds);
                API.Move move = PlayerToMove.Bot.Think(botBoard, timer);
                return new Move(move.RawValue);
            } catch (Exception e) {
                Log("An error occurred while bot was thinking.\n" + e.ToString(), true, ConsoleColor.Red);
                hasBotTaskException = true;
                botExInfo = ExceptionDispatchInfo.Capture(e);
            }

            return Move.NullMove;
        }



        void NotifyTurnToMove() {
            if (PlayerToMove.IsHuman) {
                PlayerToMove.Human.SetPosition(FenUtility.CurrentFen(board));
                PlayerToMove.Human.NotifyTurnToMove();
                return;
            }

            if (RunBotsOnSeparateThread) {
                botTaskWaitHandle.Set();
            } else {
                double startThinkTime = Raylib.GetTime();
                var move = GetBotMove();
                double thinkDuration = Raylib.GetTime() - startThinkTime;
                PlayerToMove.UpdateClock(thinkDuration);
                OnMoveChosen(move);
            }
        }

        void SetBoardPerspective() {
            // Board perspective
            if (PlayerWhite.IsHuman || PlayerBlack.IsHuman) {
                boardUI.SetPerspective(PlayerWhite.IsHuman);
                HumanWasWhiteLastGame = PlayerWhite.IsHuman;
            } else if (PlayerWhite.Bot is not EvilBot && PlayerWhite.Bot is not HumanPlayer && PlayerBlack.Bot is not EvilBot && PlayerBlack.Bot is not HumanPlayer)
                boardUI.SetPerspective(true);
            else
                boardUI.SetPerspective(PlayerWhite.Bot is not EvilBot && PlayerWhite.Bot is not HumanPlayer);
        }

        static (int totalTokenCount, int debugTokenCount) GetTokenCount(PlayerType botType) {   
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

        void OnMoveChosen(Move chosenMove) {
            if (IsLegal(chosenMove)) {
                PlayerToMove.AddIncrement(PlayerToMove.PlayerType == player1Type ? increment1Milliseconds : increment2Milliseconds);

                if (PlayerToMove.IsBot) {
                    moveToPlay = chosenMove;
                    isWaitingToPlayMove = true;
                    playMoveTime = lastMoveMadeTime + MinMoveDelay;
                    return;
                }
                
                PlayMove(chosenMove);
            } else {
                string moveName = MoveUtility.GetMoveNameUCI(chosenMove);
                string log = $"Illegal move: {moveName} in position: {FenUtility.CurrentFen(board)}";
                Log(log, true, ConsoleColor.Red);
                GameResult result = PlayerToMove == PlayerWhite ? GameResult.WhiteIllegalMove : GameResult.BlackIllegalMove;
                EndGame(result);
            }
        }

        void PlayMove(Move move) {
            if (!isPlaying) return;

            bool animate = PlayerToMove.IsBot;
            lastMoveMadeTime = (float)Raylib.GetTime();

            board.MakeMove(move, false);
            boardUI.UpdatePosition(board, move, animate);

            GameResult result = Arbiter.GetGameState(board);
            if (result == GameResult.InProgress)
                NotifyTurnToMove();
            else
                EndGame(result);
        }

        void EndGame(GameResult result, bool log = true, bool autoStartNextBotMatch = true) {
            if (!isPlaying) return;

            isPlaying = false;
            isWaitingToPlayMove = false;
            gameID = -1;

            if (log) 
                Log("Game Over: " + result, false, ConsoleColor.Blue);
            
            if (result != GameResult.VoidResult) {
                string pgn = PGNCreator.CreatePGN(board, result, GetPlayerName(PlayerWhite), GetPlayerName(PlayerBlack));
                pgns.AppendLine(pgn);
            }

            // If 2 bots playing each other, start next game automatically.
            if (!PlayerWhite.IsBot && !PlayerBlack.IsBot) return;

            if (log && result != GameResult.VoidResult) 
                UpdateBotMatchStats(result);

            botMatchGameIndex++;
            int numGamesToPlay = botMatchStartFens[botMatchStartFensIndex].Length * 2;

            if (botMatchGameIndex < numGamesToPlay && autoStartNextBotMatch) {
                botAPlaysWhite = !botAPlaysWhite;
                
                if (fastForward) {
                    StartNewGame(PlayerBlack.PlayerType, PlayerWhite.PlayerType);
                    return;
                }

                const int startNextGameDelayMs = 600;
                System.Timers.Timer autoNextTimer = new(startNextGameDelayMs);
                int originalGameID = gameID;
                autoNextTimer.Elapsed += (s, e) => AutoStartNextBotMatchGame(originalGameID, autoNextTimer);
                autoNextTimer.AutoReset = false;
                autoNextTimer.Start();

            } else if (autoStartNextBotMatch) {
                fastForward = false;
                Log("Match finished", false, ConsoleColor.Blue);
            }
            
        }
        
        public void saveGames() {
            string pgns = AllPGNs;
            string directoryPath = Path.Combine(FileHelper.AppDataPath, "Games");
            Directory.CreateDirectory(directoryPath);
            string fileName = FileHelper.GetUniqueFileName(directoryPath, "games", ".txt");
            string fullPath = Path.Combine(directoryPath, fileName);
            File.WriteAllText(fullPath, pgns);
            ConsoleHelper.Log("Saved games to " + fullPath, false, ConsoleColor.Blue);
        }

        private void AutoStartNextBotMatchGame(int originalGameID, System.Timers.Timer timer) {
            if (originalGameID == gameID)
                StartNewGame(PlayerBlack.PlayerType, PlayerWhite.PlayerType);
            timer.Close();
        }


        void UpdateBotMatchStats(GameResult result) {
            UpdateStats(BotStatsA, botAPlaysWhite);
            UpdateStats(BotStatsB, !botAPlaysWhite);

            void UpdateStats(BotMatchStats stats, bool isWhiteStats) {
                if (Arbiter.IsDrawResult(result)) {
                    //Draws
                    stats.NumDraws++;
                } else if (Arbiter.IsWhiteWinsResult(result) == isWhiteStats) {
                    //Wins
                    stats.NumWins++;
                } else {
                    //Losses
                    stats.NumLosses++;
                    stats.NumTimeouts += (result is GameResult.WhiteTimeout or GameResult.BlackTimeout) ? 1 : 0;
                    stats.NumIllegalMoves += (result is GameResult.WhiteIllegalMove or GameResult.BlackIllegalMove) ? 1 : 0;
                }
            }
        }

        public void Update() {
            if (isPlaying) {
                PlayerWhite.Update();
                PlayerBlack.Update();

                PlayerToMove.UpdateClock(Raylib.GetFrameTime());
                if (PlayerToMove.TimeRemainingMs <= 0) {
                    EndGame(PlayerToMove == PlayerWhite ? GameResult.WhiteTimeout : GameResult.BlackTimeout);
                } else if (isWaitingToPlayMove && Raylib.GetTime() > playMoveTime) {
                    isWaitingToPlayMove = false;
                    PlayMove(moveToPlay);
                }
            }

            if (hasBotTaskException) {
                hasBotTaskException = false;
                botExInfo.Throw();
            }
        }

        public void Draw() {
            boardUI.Draw();
            string nameW = GetPlayerName(PlayerWhite);
            string nameB = GetPlayerName(PlayerBlack);
            boardUI.DrawPlayerNames(nameW, nameB, PlayerWhite.TimeRemainingMs, PlayerBlack.TimeRemainingMs, isPlaying);
        }

        public void DrawOverlay() {
            if (PlayerBlack.IsHuman)
                BotBrainCapacityUI.Draw(MenuUI.getShortName((int) player1Type), MenuUI.getShortName((int) player2Type),tokenCount1, debugTokenCount1, tokenCount2, debugTokenCount2, MaxTokenCount);
            else
                BotBrainCapacityUI.Draw(MenuUI.getShortName((int) player2Type), MenuUI.getShortName((int) player1Type),tokenCount2, debugTokenCount2, tokenCount1, debugTokenCount1, MaxTokenCount);
            
            MenuUI.DrawButtons(this);
            MatchStatsUI.DrawMatchStats(this);
        }

        static string GetPlayerName(ChessPlayer player) => GetPlayerName(player.PlayerType);
        static string GetPlayerName(PlayerType type) => (type.ToString()).Split("__")[(type.ToString()).Split("__").Length-1];

        public void StartNewBotMatch(PlayerType botTypeA, PlayerType botTypeB) {
            EndGame(GameResult.DrawByArbiter, log: false, autoStartNextBotMatch: false);
            botMatchGameIndex = 0;
            string nameA = GetPlayerName(botTypeA);
            string nameB = GetPlayerName(botTypeB);
            if (nameA == nameB) {
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


        bool IsLegal(Move givenMove) {
            var moves = moveGenerator.GenerateMoves(board);
            foreach (var legalMove in moves)
                if (givenMove.Value == legalMove.Value)
                    return true;

            return false;
        }

        public class BotMatchStats {
            public string BotName;
            public int NumWins, NumLosses, NumDraws, NumTimeouts, NumIllegalMoves;

            public BotMatchStats(string name) => BotName = name;
        }

        public void Release() {
            boardUI.Release();
        }
    }
}
