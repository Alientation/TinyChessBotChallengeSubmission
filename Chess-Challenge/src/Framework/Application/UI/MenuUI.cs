using Raylib_cs;
using System.Numerics;
using System;
using System.Text.RegularExpressions;

namespace ChessChallenge.Application {
    public static class MenuUI {
        public const int MAX_INPUT_LENGTH = 8;

        public static int selectedPlayer1 = 0, selectedPlayer2 = 0;
        public static bool isPlayer1SelectionOpen = false, isPlayer2SelectionOpen = false;
        
        public static string timeControl1Input = "", timeControl2Input = "";
        public static string timeIncrement1Input = "", timeIncrement2Input = "";
        public static bool isTimeControl1InputActive = false, isTimeControlInput2Active;
        public static bool isTimeIncrement1InputActive = false, isTimeIncrement2InputActive;

        public static int initX = 260, initY = 45, initWidth = 450, initHeight = 35;
        public static Vector2 buttonSize = UIHelper.Scale(new Vector2(initWidth, initHeight)); 
        public static Vector2 buttonSizeSmall = UIHelper.Scale(new Vector2(240,35));
        public static Vector2 buttonSizeXSmall = UIHelper.Scale(new Vector2(110,35));

        public static float spacingY = buttonSize.Y * 1.3f, breakSpacing = UIHelper.ScaleInt(50);

        public static string GetShortName(ChallengeController.PlayerType playerType) {
            return (playerType + "").Split("__")[^1];
        }

        public static void DrawButtons(ChallengeController controller) {
            Vector2 buttonPos = UIHelper.Scale(new Vector2(initX, initY));

            if (NextButtonInRow("Tournament", ref buttonPos, spacingY, buttonSize))
                controller.StartTournament();

            buttonPos.Y = UIHelper.ScaleInt(550);

            // Page buttons
            buttonPos.Y = UIHelper.ScaleInt(600);

            if (NextButtonInRow("Save Games", ref buttonPos, spacingY, buttonSize))
                controller.SaveGames();
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
            buttonPos.X = UIHelper.ScaleInt(66);
            buttonPos.Y = UIHelper.ScaleInt(100) + UIHelper.ScaleInt(initY);

            if (NextButtonInRow("Play", ref buttonPos, spacingY, buttonSizeXSmall, controller.IsGameInProgress())) {
                int timeControl1 = timeControl1Input == "" ? Settings.MAX_TIME : int.Parse(timeControl1Input);
                if (timeControl1 == 0) timeControl1 = Settings.MAX_TIME;
                int timeControl2 = timeControl2Input == "" ? Settings.MAX_TIME : int.Parse(timeControl2Input);
                if (timeControl2 == 0) timeControl2 = Settings.MAX_TIME;
                int timeIncrement1 = timeIncrement1Input == "" ? 0 : int.Parse(timeIncrement1Input);
                int timeIncrement2 = timeIncrement2Input == "" ? 0 : int.Parse(timeIncrement2Input);

                ChallengeController.PlayerType player1 = ChallengeController.ActivePlayers[selectedPlayer1];
                ChallengeController.PlayerType player2 = ChallengeController.ActivePlayers[selectedPlayer2];

                ChallengeController.PlayerType Human = ChallengeController.PlayerType.Human;

                if (player1 == Human || player2 == Human)
                    controller.StartNewGame(player1, player2, timeControl1, timeIncrement1, timeControl2, timeIncrement2);
                else
                    controller.StartNewBotMatch(player1, player2, timeControl1, timeIncrement1, timeControl2, timeIncrement2);
            }

            buttonPos.X = UIHelper.ScaleInt(195);
            buttonPos.Y = UIHelper.ScaleInt(100) + UIHelper.ScaleInt(initY);

            if (NextButtonInRow("End", ref buttonPos, spacingY, buttonSizeXSmall))
                controller.EndGame(false);

            buttonPos.X = UIHelper.ScaleInt(336);
            buttonPos.Y = UIHelper.ScaleInt(100) + UIHelper.ScaleInt(initY);
            if (NextButtonInRow("Paused", ref buttonPos, spacingY, buttonSizeXSmall, controller.IsPaused())) {
                if (controller.IsPaused())
                    controller.ResumeGame();
                else
                    controller.PauseGame();
            }

            buttonPos.X = UIHelper.ScaleInt(465);
            buttonPos.Y = UIHelper.ScaleInt(100) + UIHelper.ScaleInt(initY);
            if (NextButtonInRow(">>", ref buttonPos, spacingY, buttonSizeXSmall, controller.fastForward))
                controller.fastForward = !controller.fastForward;
            

            //time control input
            buttonPos.X = UIHelper.ScaleInt(90);
            UIHelper.DrawText("P1 Time: ", buttonPos, UIHelper.ScaleInt(24), 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(90);
            var textInput1 = UIHelper.TextInput(timeControl1Input, isTimeControl1InputActive, buttonPos, UIHelper.Scale(new Vector2(140,35)), "infinity");

            buttonPos.X = UIHelper.ScaleInt(350);
            UIHelper.DrawText("P2 Time: ", buttonPos, UIHelper.ScaleInt(24), 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(90);
            var textInput2 = UIHelper.TextInput(timeControl2Input, isTimeControlInput2Active, buttonPos, UIHelper.Scale(new Vector2(140,35)), "infinity");

            buttonPos.Y += UIHelper.ScaleInt(45);
            buttonPos.X = UIHelper.ScaleInt(100);
            UIHelper.DrawText("P1 t/move: ", buttonPos, UIHelper.ScaleInt(24), 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(80);
            var textInput11 = UIHelper.TextInput(timeIncrement1Input, isTimeIncrement1InputActive, buttonPos, UIHelper.Scale(new Vector2(140,35)), "0");

            buttonPos.X = UIHelper.ScaleInt(370);
            UIHelper.DrawText("P2 t/move: ", buttonPos, UIHelper.ScaleInt(24), 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(70);
            var textInput21 = UIHelper.TextInput(timeIncrement2Input, isTimeIncrement2InputActive, buttonPos, UIHelper.Scale(new Vector2(140,35)), "0");

            //parse time control input.. remove any non digits
            timeControl1Input =  Regex.Replace(textInput1.Item1, "[^0-9]", "");
            timeControl2Input =  Regex.Replace(textInput2.Item1, "[^0-9]", "");
            timeIncrement1Input =  Regex.Replace(textInput11.Item1, "[^0-9]", "");
            timeIncrement2Input =  Regex.Replace(textInput21.Item1, "[^0-9]", "");

            if (timeControl1Input.Length > MAX_INPUT_LENGTH) timeControl1Input = timeControl1Input[..MAX_INPUT_LENGTH];
            if (timeControl2Input.Length > MAX_INPUT_LENGTH) timeControl2Input = timeControl2Input[..MAX_INPUT_LENGTH];
            if (timeIncrement1Input.Length > MAX_INPUT_LENGTH) timeIncrement1Input = timeIncrement1Input[..MAX_INPUT_LENGTH];
            if (timeIncrement2Input.Length > MAX_INPUT_LENGTH) timeIncrement2Input = timeIncrement2Input[..MAX_INPUT_LENGTH];

            isTimeControl1InputActive = textInput1.Item2;
            isTimeControlInput2Active = textInput2.Item2;
            isTimeIncrement1InputActive = textInput11.Item2;
            isTimeIncrement2InputActive = textInput21.Item2;

            //update cursor (IK THIS IS BAD CODE ^^^^^)
            Raylib.SetMouseCursor((textInput1.Item3 || textInput2.Item3 || textInput11.Item3 || textInput21.Item3) ? MouseCursor.MOUSE_CURSOR_IBEAM : MouseCursor.MOUSE_CURSOR_DEFAULT);

            
            /*buttonPos.Y += UIHelper.ScaleInt(200);
            buttonPos.X = UIHelper.ScaleInt(initX);
            if (NextButtonInRow("End Game", ref buttonPos, spacingY, buttonSize))
                controller.EndGame(false);*/


            buttonPos.Y = UIHelper.ScaleInt(110) + UIHelper.ScaleInt(initY);


            //player selection
            var player1Selection = DropdownListSelectPlayersHelper(ChallengeController.ActivePlayers, isPlayer1SelectionOpen, UIHelper.Scale(new Vector2(130, 100)), UIHelper.Scale(new Vector2(240,35)), selectedPlayer1);
            isPlayer1SelectionOpen = player1Selection.Item2;
            if (isPlayer1SelectionOpen) isPlayer2SelectionOpen = false;
            if (player1Selection.Item1 >= 0) selectedPlayer1 = player1Selection.Item1;

            var player2Selection = DropdownListSelectPlayersHelper(ChallengeController.ActivePlayers, isPlayer2SelectionOpen, UIHelper.Scale(new Vector2(400, 100)), UIHelper.Scale(new Vector2(240,35)), selectedPlayer2);
            isPlayer2SelectionOpen = player2Selection.Item2;
            if (isPlayer2SelectionOpen) isPlayer1SelectionOpen = false;
            if (player2Selection.Item1 >= 0) selectedPlayer2 = player2Selection.Item1;


            (int, bool) DropdownListSelectPlayersHelper(ChallengeController.PlayerType[] playerChoices, bool isOpen, Vector2 pos, Vector2 size, int selectedPlayer = -1) {
                string[] options = new string[playerChoices.Length];
                for (int i = 0; i < playerChoices.Length; i++)
                    options[i] = GetShortName(playerChoices[i]);

                return DropdownList(selectedPlayer < 0 ? "Choose" : GetShortName(playerChoices[selectedPlayer]), options, isOpen, pos, size, selectedPlayer);
            }
            
            //universal button
            bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size, bool selected = false) {
                bool pressed = UIHelper.Button(name, pos, size, selected);
                pos.Y += spacingY;
                return pressed;
            }

            //universal dropdown list
            (int, bool) DropdownList(string text, string[] options, bool isOpen, Vector2 pos, Vector2 size, int selectedOption = -1) {
                //create button for dropdown list
                bool pressed = UIHelper.Button(text, pos, size);
                if (pressed) isOpen = !isOpen;

                //if it is not open, return
                if (!isOpen) return (-1, false);

                //create list beneath button
                Vector2 itemPos = new(pos.X, pos.Y + UIHelper.ScaleInt(45));
                float initX = itemPos.X;
                Vector2 itemSize = UIHelper.Scale(new Vector2(250, 50));
                bool toggle = false; //two columns of buttons
                for (int i = 0; i < options.Length; i++) {
                    bool itemPressed = UIHelper.Button(options[i], itemPos, itemSize, i == selectedOption);

                    //item is pressed, return the item and close the list
                    if (itemPressed) return (i, false);
                    
                    //build row
                    if (toggle) {
                        itemPos.Y += UIHelper.ScaleInt(45);
                        itemPos.X = initX;
                    } else
                        itemPos.X += UIHelper.ScaleInt(250);
                    
                    toggle = !toggle;
                }

                //detect if mouse has been pressed in some other frame
                bool mouseOver = UIHelper.MouseInRect(UIHelper.GetRectangle(pos, size));
                bool pressedNotThisFrame = !mouseOver && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);

                return (selectedOption, !pressedNotThisFrame);
            }
        }
    }
}