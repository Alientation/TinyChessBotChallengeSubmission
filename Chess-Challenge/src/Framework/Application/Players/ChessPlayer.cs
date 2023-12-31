﻿using ChessChallenge.API;
using System;

namespace ChessChallenge.Application {
    public class ChessPlayer {
        public readonly ChallengeController.PlayerType PlayerType;
        public readonly IChessBot? Bot;
        public readonly HumanPlayer? Human;

        double secondsElapsed;
        int incrementAddedMs;
        int baseTimeMs;

        public ChessPlayer(object instance, ChallengeController.PlayerType type, int baseTimeMs = Settings.MAX_TIME) {
            this.PlayerType = type;
            Bot = instance as IChessBot;
            Human = instance as HumanPlayer;
            this.baseTimeMs = baseTimeMs;

        }

        public bool IsHuman => Human != null;
        public bool IsBot => Bot != null;

        public void Update() {
            Human?.Update();
        }

        public void UpdateClock(double dt) {
            secondsElapsed += dt;
        }

        public void AddIncrement(int incrementMs) {
            if (baseTimeMs == Settings.MAX_TIME)
                return;

            incrementAddedMs += incrementMs;
        }

        public int TimeRemainingMs {
            get {
                if (baseTimeMs == Settings.MAX_TIME)
                    return baseTimeMs;
                return (int)Math.Ceiling(Math.Max(0, baseTimeMs - secondsElapsed * 1000.0 + incrementAddedMs));
            }
        }

        public void SubscribeToMoveChosenEventIfHuman(Action<Chess.Move> action) {
            if (Human != null)
                Human.MoveChosen += action;
        }
    }
}
