﻿using System.Numerics;

namespace ChessChallenge.Application {
    public static class Settings {
        public const string Version = "1.20";

        // Game settings
        public const int DefaultGameDurationMilliseconds = 360 * 1000;
        public const int DefaultIncrementMilliseconds = 0;
        public const int DefaultTimeBetweenGames = 2500;
        public const int DefaultGamesPerMatch = 1;
        public const int MAX_TIME = 60 * 60 * 1000;
        public const float MinMoveDelay = 0;
        public static readonly bool RunBotsOnSeparateThread = true;

        // Display settings
        public const bool DisplayBoardCoordinates = true;
        public static readonly Vector2 ScreenSizeSmall = new(1280, 720);
        public static readonly Vector2 ScreenSizeBig = new(1440, 810);
        //public static readonly Vector2 ScreenSizeBig = new(1920, 1080);

        // Other settings
        public const int MaxTokenCount = 1024;
        public const LogType MessagesToLog = LogType.All;

        public enum LogType {
            None,
            ErrorOnly,
            All
        }
    }
}
