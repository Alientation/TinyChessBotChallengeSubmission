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
            Vector2 buttonPos = UIHelper.Scale(new Vector2(220, 150));
            Vector2 buttonSize = UIHelper.Scale(new Vector2(400, 55));
            float spacing = buttonSize.Y * 1.2f;
            float breakSpacing = spacing * 0.6f;

            if (NextButtonInRow("Human vs " + ChallengeController.botToTest1, ref buttonPos, spacing, buttonSize)) {
                controller.StartNewGame(ChallengeController.PlayerType.Human, ChallengeController.botToTest1);
            }

            if (NextButtonInRow("Human vs " + ChallengeController.botToTest2, ref buttonPos, spacing, buttonSize)) {
                controller.StartNewGame(ChallengeController.PlayerType.Human, ChallengeController.botToTest2);
            }

            if (NextButtonInRow(ChallengeController.botToTest1 + " vs " + ChallengeController.botToTest2, ref buttonPos, spacing, buttonSize)) {
                controller.StartNewBotMatch(ChallengeController.botToTest1, ChallengeController.botToTest2);
            }

            if (NextButtonInRow(ChallengeController.botToTest1 + " vs EvilBot", ref buttonPos, spacing, buttonSize)) {
                controller.StartNewBotMatch(ChallengeController.PlayerType.EvilBot, ChallengeController.botToTest1);
            }

            if (NextButtonInRow(ChallengeController.botToTest2 + " vs EvilBot", ref buttonPos, spacing, buttonSize)) {
                controller.StartNewBotMatch(ChallengeController.PlayerType.EvilBot, ChallengeController.botToTest2);
            }


            if (NextButtonInRow("Tournament", ref buttonPos, spacing, buttonSize)) {
                controller.StartTournament();
            }



            // Page buttons
            buttonPos.Y += breakSpacing;

            if (NextButtonInRow("Save Games", ref buttonPos, spacing, buttonSize))
            {
                string pgns = controller.AllPGNs;
                string directoryPath = Path.Combine(FileHelper.AppDataPath, "Games");
                Directory.CreateDirectory(directoryPath);
                string fileName = FileHelper.GetUniqueFileName(directoryPath, "games", ".txt");
                string fullPath = Path.Combine(directoryPath, fileName);
                File.WriteAllText(fullPath, pgns);
                ConsoleHelper.Log("Saved games to " + fullPath, false, ConsoleColor.Blue);
            }
            if (NextButtonInRow("Rules & Help", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://github.com/SebLague/Chess-Challenge");
            }
            if (NextButtonInRow("Documentation", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://seblague.github.io/chess-coding-challenge/documentation/");
            }
            if (NextButtonInRow("Submission Page", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://forms.gle/6jjj8jxNQ5Ln53ie6");
            }

            // Window and quit buttons
            buttonPos.Y += breakSpacing;

            bool isBigWindow = Raylib.GetScreenWidth() > Settings.ScreenSizeSmall.X;
            string windowButtonName = isBigWindow ? "Smaller Window" : "Bigger Window";
            if (NextButtonInRow(windowButtonName, ref buttonPos, spacing, buttonSize))
            {
                Program.SetWindowSize(isBigWindow ? Settings.ScreenSizeSmall : Settings.ScreenSizeBig);
            }
            if (NextButtonInRow("Exit (ESC)", ref buttonPos, spacing, buttonSize))
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