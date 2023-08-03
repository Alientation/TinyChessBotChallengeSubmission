using Raylib_cs;
using System.Numerics;
using System;
using System.IO;

namespace ChessChallenge.Application
{
    public static class MenuUI
    {
        public static int selectedPlayer1 = -1;
        public static bool is1Open = false;
        public static int selectedPlayer2 = -1;
        public static bool is2Open = false;
        public static string pattern = @"(?<=\w__)";

        public static string getShortName(int type) {
            ChallengeController.PlayerType playerType = (ChallengeController.PlayerType) type;
            return ((playerType + "").Split("__")[(playerType + "").Split("__").Length-1]);
        }

        public static void DrawButtons(ChallengeController controller)
        {
            int initX = 250, initY = 45, initWidth = 420, initHeight = 35;

            Vector2 buttonPos = UIHelper.Scale(new Vector2(initX, initY));
            Vector2 buttonSize = UIHelper.Scale(new Vector2(initWidth, initHeight));
            float spacingY = buttonSize.Y * 1.3f;
            float spacingX = buttonSize.X * 1.4f;
            float breakSpacing = UIHelper.ScaleInt(50);


            if (NextButtonInRow("Tournament", ref buttonPos, spacingY, buttonSize)) {
                controller.StartTournament();
            }

            buttonPos.Y = UIHelper.ScaleInt(450);

            if (selectedPlayer1 >= 0 && selectedPlayer2 >= 0) {
                if (NextButtonInRow("Play " + getShortName(selectedPlayer1) + " vs " + getShortName(selectedPlayer2), ref buttonPos, spacingY, buttonSize)) {
                    if ((ChallengeController.PlayerType) selectedPlayer1 == ChallengeController.PlayerType.Human || 
                        (ChallengeController.PlayerType) selectedPlayer2 == ChallengeController.PlayerType.Human)
                        controller.StartNewGame((ChallengeController.PlayerType) selectedPlayer1, (ChallengeController.PlayerType) selectedPlayer2);
                    else
                        controller.StartNewBotMatch((ChallengeController.PlayerType) selectedPlayer1, (ChallengeController.PlayerType) selectedPlayer2);
                }
            }

            if (NextButtonInRow("End Game", ref buttonPos, spacingY, buttonSize)) {
                controller.EndGame(false);
            }

            buttonPos.Y = UIHelper.ScaleInt(110) + UIHelper.ScaleInt(initY);

            int temp1 = DropdownList(selectedPlayer1 < 0 ? "Choose" : getShortName(selectedPlayer1), ChallengeController.ActivePlayers, is1Open, new Vector2(130, 160), new Vector2(240,35));
            is1Open = temp1 == -2;
            if (temp1 >= 0) selectedPlayer1 = temp1;


            int temp2 = DropdownList(selectedPlayer2 < 0 ? "Choose" : getShortName(selectedPlayer2), ChallengeController.ActivePlayers, is2Open, new Vector2(400, 160), new Vector2(240,35));
            is2Open = temp2 == -2;
            if (temp2 >= 0) selectedPlayer2 = temp2;


            // Page buttons
            buttonPos.Y = UIHelper.ScaleInt(600);

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
                bool pressed = UIHelper.Button(name.Replace("__",""), pos, size);
                pos.Y += spacingY;
                return pressed;
            }

            int DropdownList(string text, ChallengeController.PlayerType[] options, bool isOpen, Vector2 pos, Vector2 size) {
                pos = UIHelper.Scale(pos);
                size = UIHelper.Scale(size);

                bool pressed = UIHelper.Button(text.Replace("__",""), pos, size);
                if (pressed) isOpen = !isOpen;

                int item = -1;
                if (isOpen) {
                    is1Open = false;
                    is2Open = false;
                    Vector2 itemPos = new Vector2(pos.X, pos.Y + UIHelper.ScaleInt(45));
                    float initX = itemPos.X;
                    Vector2 itemSize = UIHelper.Scale(new Vector2(250, 50));
                    bool toggle = false;
                    foreach (ChallengeController.PlayerType option in options) {
                        bool itemPressed = UIHelper.Button(getShortName((int) option), itemPos, itemSize);

                        if (itemPressed)
                            item = (int) option;
                        
                        if (toggle) {
                            itemPos.Y += UIHelper.ScaleInt(45);
                            itemPos.X = initX;
                        } else {
                            itemPos.X += UIHelper.ScaleInt(250);
                        }
                        toggle = !toggle;
                    }
                    if (item == -1) return -2;
                }
                return item;
            }
        }
    }
}