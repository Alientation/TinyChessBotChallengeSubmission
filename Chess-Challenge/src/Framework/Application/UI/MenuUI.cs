using Raylib_cs;
using System.Numerics;
using System;
using System.IO;

namespace ChessChallenge.Application
{
    public static class MenuUI
    {
        public static void DrawButtons(ChallengeController controller)
        {
            int initX = 235, initY = 150, initWidth = 350, initHeight = 35;

            Vector2 buttonPos = UIHelper.Scale(new Vector2(initX, initY));
            Vector2 buttonSize = UIHelper.Scale(new Vector2(initWidth, initHeight));
            float spacingY = buttonSize.Y * 1.2f;
            float spacingX = buttonSize.X * 1.4f;
            float breakSpacing = spacingX * 0.6f;

            if (NextButtonInRow("Human vs " + ChallengeController.botToTest1, ref buttonPos, spacingY, buttonSize)) {
                controller.StartNewGame(ChallengeController.PlayerType.Human, ChallengeController.botToTest1);
            }

            if (NextButtonInRow("Human vs " + ChallengeController.botToTest2, ref buttonPos, spacingY, buttonSize)) {
                controller.StartNewGame(ChallengeController.PlayerType.Human, ChallengeController.botToTest2);
            }

            if (NextButtonInRow("Human vs " + ChallengeController.botToTest3, ref buttonPos, spacingY, buttonSize)) {
                controller.StartNewGame(ChallengeController.PlayerType.Human, ChallengeController.botToTest3);
            }

            if (NextButtonInRow(ChallengeController.botToTest1 + " vs " + ChallengeController.botToTest2, ref buttonPos, spacingY, buttonSize)) {
                controller.StartNewBotMatch(ChallengeController.botToTest1, ChallengeController.botToTest2);
            }

            if (NextButtonInRow(ChallengeController.botToTest2 + " vs " + ChallengeController.botToTest3, ref buttonPos, spacingY, buttonSize)) {
                controller.StartNewBotMatch(ChallengeController.botToTest2, ChallengeController.botToTest3);
            }

            if (NextButtonInRow(ChallengeController.botToTest3 + " vs " + ChallengeController.botToTest1, ref buttonPos, spacingY, buttonSize)) {
                controller.StartNewBotMatch(ChallengeController.botToTest3, ChallengeController.botToTest1);
            }

            if (NextButtonInRow(ChallengeController.botToTest1 + " vs EvilBot", ref buttonPos, spacingY, buttonSize)) {
                controller.StartNewBotMatch(ChallengeController.PlayerType.EvilBot, ChallengeController.botToTest1);
            }

            if (NextButtonInRow(ChallengeController.botToTest2 + " vs EvilBot", ref buttonPos, spacingY, buttonSize)) {
                controller.StartNewBotMatch(ChallengeController.PlayerType.EvilBot, ChallengeController.botToTest2);
            }

            if (NextButtonInRow(ChallengeController.botToTest3 + " vs EvilBot", ref buttonPos, spacingY, buttonSize)) {
                controller.StartNewBotMatch(ChallengeController.PlayerType.EvilBot, ChallengeController.botToTest3);
            }


            if (NextButtonInRow("Tournament", ref buttonPos, spacingY, buttonSize)) {
                controller.StartTournament();
            }



            // Page buttons
            buttonPos.Y += breakSpacing;

            if (NextButtonInRow("Save Games", ref buttonPos, spacingY, buttonSize))
            {
                string pgns = controller.AllPGNs;
                string directoryPath = Path.Combine(FileHelper.AppDataPath, "Games");
                Directory.CreateDirectory(directoryPath);
                string fileName = FileHelper.GetUniqueFileName(directoryPath, "games", ".txt");
                string fullPath = Path.Combine(directoryPath, fileName);
                File.WriteAllText(fullPath, pgns);
                ConsoleHelper.Log("Saved games to " + fullPath, false, ConsoleColor.Blue);
            }
            if (NextButtonInRow("Rules & Help", ref buttonPos, spacingY, buttonSize))
            {
                FileHelper.OpenUrl("https://github.com/SebLague/Chess-Challenge");
            }
            if (NextButtonInRow("Documentation", ref buttonPos, spacingY, buttonSize))
            {
                FileHelper.OpenUrl("https://seblague.github.io/chess-coding-challenge/documentation/");
            }
            if (NextButtonInRow("Submission Page", ref buttonPos, spacingY, buttonSize))
            {
                FileHelper.OpenUrl("https://forms.gle/6jjj8jxNQ5Ln53ie6");
            }

            // Window and quit buttons
            buttonPos.Y += breakSpacing;

            bool isBigWindow = Raylib.GetScreenWidth() > Settings.ScreenSizeSmall.X;
            string windowButtonName = isBigWindow ? "Smaller Window" : "Bigger Window";
            if (NextButtonInRow(windowButtonName, ref buttonPos, spacingY, buttonSize))
            {
                Program.SetWindowSize(isBigWindow ? Settings.ScreenSizeSmall : Settings.ScreenSizeBig);
            }
            if (NextButtonInRow("Exit (ESC)", ref buttonPos, spacingY, buttonSize))
            {
                Environment.Exit(0);
            }

            bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size)
            {
                bool pressed = UIHelper.Button(name, pos, size);
                pos.Y += spacingY;
                return pressed;
            }
        }
    }
}