﻿using Raylib_cs;
using System.Numerics;
using System;
using System.Text.RegularExpressions;

namespace ChessChallenge.Application {
    public static class MenuUI {
        public static int selectedPlayer1 = (int) ChallengeController.player1Type, selectedPlayer2 = (int) ChallengeController.player2Type;
        public static bool is1Open = false, is2Open = false;
        
        public static string timeControl1Input = "", timeControl2Input = "";
        public static string timeIncrement1 = "", timeIncrement2 = "";
        public static bool isTimeControl1InputActive = false, isTimeControl2Active;
        public static bool isTimeIncrement1Active = false, isTimeIncrement2Active;

        public static string getShortName(int type) {
            ChallengeController.PlayerType playerType = (ChallengeController.PlayerType) type;
            return ((playerType + "").Split("__")[(playerType + "").Split("__").Length-1]);
        }

        public static void DrawButtons(ChallengeController controller) {
            int initX = 260, initY = 45, initWidth = 450, initHeight = 35;

            Vector2 buttonPos = UIHelper.Scale(new Vector2(initX, initY));
            Vector2 buttonSize = UIHelper.Scale(new Vector2(initWidth, initHeight));
            Vector2 buttonSizeSmall = UIHelper.Scale(new Vector2(240,35));
            float spacingY = buttonSize.Y * 1.3f;
            float spacingX = buttonSize.X * 1.4f;
            float breakSpacing = UIHelper.ScaleInt(50);


            if (NextButtonInRow("Tournament", ref buttonPos, spacingY, buttonSize))
                controller.StartTournament();

            buttonPos.Y = UIHelper.ScaleInt(550);

            // Page buttons
            buttonPos.Y = UIHelper.ScaleInt(600);

            if (NextButtonInRow("Save Games", ref buttonPos, spacingY, buttonSize))
                controller.saveGames();
            if (NextButtonInRow("Rules & Help", ref buttonPos, spacingY, buttonSize))
                FileHelper.OpenUrl("https://github.com/SebLague/Chess-Challenge");
            if (NextButtonInRow("Documentation", ref buttonPos, spacingY, buttonSize))
                FileHelper.OpenUrl("https://seblague.github.io/chess-coding-challenge/documentation/");
            if (NextButtonInRow("Submission Page", ref buttonPos, spacingY, buttonSize))
                FileHelper.OpenUrl("https://forms.gle/6jjj8jxNQ5Ln53ie6");

            // Window and quit buttons
            buttonPos.Y += breakSpacing;

            bool isBigWindow = Raylib.GetScreenWidth() > Settings.ScreenSizeSmall.X;
            string windowButtonName = isBigWindow ? "Smaller Window" : "Bigger Window";
            if (NextButtonInRow(windowButtonName, ref buttonPos, spacingY, buttonSize))
                Program.SetWindowSize(isBigWindow ? Settings.ScreenSizeSmall : Settings.ScreenSizeBig);
            if (NextButtonInRow("Exit (ESC)", ref buttonPos, spacingY, buttonSize))
                Environment.Exit(0);

            // Game Set up
            buttonPos.X = UIHelper.ScaleInt(130);
            buttonPos.Y = UIHelper.ScaleInt(100) + UIHelper.ScaleInt(initY);

            if (NextButtonInRow("Play", ref buttonPos, spacingY, buttonSizeSmall)) {
                int timeControl1 = timeControl1Input == "" ? Settings.MAX_TIME : int.Parse(timeControl1Input);
                int timeControl2 = timeControl2Input == "" ? Settings.MAX_TIME : int.Parse(timeControl2Input);
                int timeIncrement1 = MenuUI.timeIncrement1 == "" ? Settings.MAX_TIME : int.Parse(MenuUI.timeIncrement1);
                int timeIncrement2 = MenuUI.timeIncrement2 == "" ? Settings.MAX_TIME : int.Parse(MenuUI.timeIncrement2);

                if ((ChallengeController.PlayerType) selectedPlayer1 == ChallengeController.PlayerType.Human || 
                    (ChallengeController.PlayerType) selectedPlayer2 == ChallengeController.PlayerType.Human)
                    controller.StartNewGame((ChallengeController.PlayerType) selectedPlayer1, (ChallengeController.PlayerType) selectedPlayer2, timeControl1, timeControl2, timeIncrement1, timeIncrement2);
                else
                    controller.StartNewBotMatch((ChallengeController.PlayerType) selectedPlayer1, (ChallengeController.PlayerType) selectedPlayer2, timeControl1, timeControl2, timeIncrement1, timeIncrement2);
            }

            buttonPos.X = UIHelper.ScaleInt(400);
            buttonPos.Y = UIHelper.ScaleInt(100) + UIHelper.ScaleInt(initY);
            if (NextButtonInRow("Fast Forward", ref buttonPos, spacingY, buttonSizeSmall, controller.fastForward))
                controller.fastForward = !controller.fastForward;
            

            //time control input
            buttonPos.X = UIHelper.ScaleInt(90);
            UIHelper.DrawText("P1 Time: ", buttonPos, 16, 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(90);
            var textInput1 = UIHelper.TextInput(timeControl1Input, isTimeControl1InputActive, buttonPos, UIHelper.Scale(new Vector2(140,35)), "infinity");

            buttonPos.X = UIHelper.ScaleInt(350);
            UIHelper.DrawText("P2 Time: ", buttonPos, 16, 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(90);
            var textInput2 = UIHelper.TextInput(timeControl2Input, isTimeControl2Active, buttonPos, UIHelper.Scale(new Vector2(140,35)), "infinity");

            buttonPos.Y += UIHelper.ScaleInt(45);
            buttonPos.X = UIHelper.ScaleInt(110);
            UIHelper.DrawText("P1 s/move: ", buttonPos, 16, 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(70);
            var textInput11 = UIHelper.TextInput(timeIncrement1, isTimeIncrement1Active, buttonPos, UIHelper.Scale(new Vector2(140,35)), "infinity");

            buttonPos.X = UIHelper.ScaleInt(370);
            UIHelper.DrawText("P2 s/move: ", buttonPos, 16, 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(70);
            var textInput21 = UIHelper.TextInput(timeIncrement2, isTimeIncrement2Active, buttonPos, UIHelper.Scale(new Vector2(140,35)), "infinity");

            //parse time control input.. remove any non digits
            timeControl1Input =  Regex.Replace(textInput1.Item1, "[^0-9]", "");
            timeControl2Input =  Regex.Replace(textInput2.Item1, "[^0-9]", "");
            timeIncrement1 =  Regex.Replace(textInput11.Item1, "[^0-9]", "");
            timeIncrement2 =  Regex.Replace(textInput21.Item1, "[^0-9]", "");
            isTimeControl1InputActive = textInput1.Item2;
            isTimeControl2Active = textInput2.Item2;
            isTimeIncrement1Active = textInput11.Item2;
            isTimeIncrement2Active = textInput21.Item2;

            //update cursor (IK THIS IS BAD CODE ^^^^^)
            Raylib.SetMouseCursor((textInput1.Item3 || textInput2.Item3 || textInput11.Item3 || textInput21.Item3) ? MouseCursor.MOUSE_CURSOR_IBEAM : MouseCursor.MOUSE_CURSOR_DEFAULT);

            
            buttonPos.Y += UIHelper.ScaleInt(200);
            buttonPos.X = UIHelper.ScaleInt(initX);
            if (NextButtonInRow("End Game", ref buttonPos, spacingY, buttonSize))
                controller.EndGame(false);


            buttonPos.Y = UIHelper.ScaleInt(110) + UIHelper.ScaleInt(initY);

            int temp1 = DropdownList(selectedPlayer1 < 0 ? "Choose" : getShortName(selectedPlayer1), ChallengeController.ActivePlayers, is1Open, new Vector2(130, 100), new Vector2(240,35), selectedPlayer1);
            is1Open = temp1 == -2;
            if (temp1 >= 0) selectedPlayer1 = temp1;


            int temp2 = DropdownList(selectedPlayer2 < 0 ? "Choose" : getShortName(selectedPlayer2), ChallengeController.ActivePlayers, is2Open, new Vector2(400, 100), new Vector2(240,35), selectedPlayer2);
            is2Open = temp2 == -2;
            if (temp2 >= 0) selectedPlayer2 = temp2;
            

            bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size, bool selected = false) {
                bool pressed = UIHelper.Button(name.Replace("__",""), pos, size, selected);
                pos.Y += spacingY;
                return pressed;
            }

            int DropdownList(string text, ChallengeController.PlayerType[] options, bool isOpen, Vector2 pos, Vector2 size, int selectedOption = -1) {
                pos = UIHelper.Scale(pos);
                size = UIHelper.Scale(size);

                bool pressed = UIHelper.Button(text.Replace("__",""), pos, size);
                if (pressed) isOpen = !isOpen;

                if (!isOpen) return -1;

                is1Open = false;
                is2Open = false;
                Vector2 itemPos = new Vector2(pos.X, pos.Y + UIHelper.ScaleInt(45));
                float initX = itemPos.X;
                Vector2 itemSize = UIHelper.Scale(new Vector2(250, 50));
                bool toggle = false;
                foreach (ChallengeController.PlayerType option in options) {
                    bool itemPressed = UIHelper.Button(getShortName((int) option), itemPos, itemSize, (int) option == selectedOption);

                    if (itemPressed)
                        return (int) option;
                    
                    if (toggle) {
                        itemPos.Y += UIHelper.ScaleInt(45);
                        itemPos.X = initX;
                    } else
                        itemPos.X += UIHelper.ScaleInt(250);
                    
                    toggle = !toggle;
                }

                bool mouseOver = UIHelper.MouseInRect(UIHelper.GetRectangle(pos, size));
                bool pressedNotThisFrame = !mouseOver && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);
                if (pressedNotThisFrame) return -1;

                return -2;
            }
        }
    }
}