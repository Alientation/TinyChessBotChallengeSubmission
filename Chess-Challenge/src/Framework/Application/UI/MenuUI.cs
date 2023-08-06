using Raylib_cs;
using System.Numerics;
using System;

namespace ChessChallenge.Application {
    public static class MenuUI {
        public const int MAX_INPUT_LENGTH = 8;

        public static int selectedPlayer1 = 0, selectedPlayer2 = 0;
        public static bool isPlayer1SelectionOpen = false, isPlayer2SelectionOpen = false;

        public static bool isUCICommandUIOpen = false;
        
        public static string timeControl1Input = "", timeControl2Input = "";
        public static string timeIncrement1Input = "", timeIncrement2Input = "";
        public static string timeBetweenGameInput = "";
        public static bool isTimeControl1InputActive = false, isTimeControlInput2Active = false;
        public static bool isTimeIncrement1InputActive = false, isTimeIncrement2InputActive = false;
        public static bool isTimeBetweenGameInputActive = false;

        public static int initX = 265, initY = 45, initWidth = 450, initHeight = 35;
        public static Vector2 ButtonSize => UIHelper.Scale(new Vector2(initWidth, initHeight));
        public static Vector2 ButtonSizeSmall => UIHelper.Scale(new Vector2(240,35));
        public static Vector2 ButtonSizeXSmall => UIHelper.Scale(new Vector2(110,35));
        public static int ButtonSizeNormalPositionX => UIHelper.ScaleInt(initX);
        public static int ButtonSizeSmallPositionX1 => UIHelper.ScaleInt(130);
        public static int ButtonSizeSmallPositionX2 => UIHelper.ScaleInt(400);
        public static float SpacingY => ButtonSize.Y * 1.3f;

        public static string GetShortName(ChallengeController.PlayerType playerType, int sizeLimit = 100) {
            string shortName = (playerType + "").Split("__")[^1];

            if (shortName.Length > sizeLimit) {
                int beginLength = sizeLimit / 2;
                int endLength = (sizeLimit + 1) / 2;
                return shortName[..beginLength] + ".." + shortName.Substring(shortName.Length - endLength, endLength);
            }

            return shortName;
        }

        public static void DrawButtons(ChallengeController controller) {
            Vector2 buttonSize = ButtonSize;
            Vector2 buttonSizeSmall = ButtonSizeSmall;
            Vector2 buttonSizeXSmall = ButtonSizeXSmall;

            int buttonSizeNormalPositionX = ButtonSizeNormalPositionX;
            int buttonSizeSmallPositionX1 = ButtonSizeSmallPositionX1;
            int buttonSizeSmallPositionX2 = ButtonSizeSmallPositionX2;

            int fontSizeNormal = UIHelper.ScaleInt(32);
            int fontSizeSmall = UIHelper.ScaleInt(26);
            int fontSizeXSmall = UIHelper.ScaleInt(18);

            float spacingY = SpacingY;
            Vector2 buttonPos = new(buttonSizeNormalPositionX, UIHelper.ScaleInt(initY));
            bool isMouseOverTextInput = false;


            // Top buttons
            if (NextButtonInRow("Tournament", ref buttonPos, spacingY, buttonSize, fontSizeNormal))
                controller.StartTournament();

            
            
            //Middle Buttons
            buttonPos.X = buttonSizeSmallPositionX1;
            buttonPos.Y = UIHelper.ScaleInt(370);

            if (NextButtonInRow("UCI Cmd Gen", ref buttonPos, 0, buttonSizeSmall, fontSizeNormal, selected: isUCICommandUIOpen))
                isUCICommandUIOpen = !isUCICommandUIOpen;
            if (isPlayer1SelectionOpen || isPlayer2SelectionOpen)
                isUCICommandUIOpen = false;

            if (isUCICommandUIOpen) {
                Rectangle rec = UIHelper.GetRectangle(ScaleVector(initX,568), ScaleVector(500, 500));
                Raylib.DrawRectangleRec(rec, new Color(20,26,36,255));

                buttonPos.X = UIHelper.ScaleInt(75);
                buttonPos.Y = UIHelper.ScaleInt(345);
                if (NextButtonInRow("Create Engine", ref buttonPos, 0, buttonSizeXSmall, fontSizeXSmall)) {

                }



            }
            
            /*
            -engine
            -each
                conf=NAME
                cmd=COMMAND
                dir=DIR
                arg=ARG
                stderr=FILE
                restart=MODE {"auto","on","off"}
                trust
                tc=TIMECONTROL {move/time+increment} {inf}
                st=N
                timemargin=N
                whitepov
                
            -concurrency 
                N
            -maxmoves 
                N
            -tournament 
                TYPE {"guantlet","round-robin","knockout","pyramid"}
            -games 
                N
            -rounds 
                N
            -sprt 
                elo0=ELO0 
                elo1=ELO1 
                alpha=ALPHA 
                beta=BETA
            -ratingsinterval 
                N
            -outcomeinterval 
                N
            -debug
            -openings 
                file=FILE 
                format=FORMAT {"epd","pgn"} 
                order=ORDER {"random","sequential"} 
                plies=N 
                start=N 
                policy=POLICY {"encounter","round","default"}
            -pgnout 
                FILE [min][fi]
            -epdout 
                FILE
            -recover
            -repeat 
                [N]
            -noswap
            -wait 
                N
            -resultformat 
                FORMAT
            */


            // Quick links/random buttons
            buttonPos.X = buttonSizeSmallPositionX1;
            buttonPos.Y = UIHelper.ScaleInt(900);

            if (NextButtonInRow("Save Games", ref buttonPos, 0, buttonSizeSmall, fontSizeNormal))
                controller.SaveGames();
            buttonPos.X = buttonSizeSmallPositionX2;
            if (NextButtonInRow("Rules & Help", ref buttonPos, spacingY, buttonSizeSmall, fontSizeNormal))
                FileHelper.OpenUrl("https://github.com/SebLague/Chess-Challenge");
            buttonPos.X = buttonSizeSmallPositionX1;
            if (NextButtonInRow("Documentation", ref buttonPos, 0, buttonSizeSmall, fontSizeNormal))
                FileHelper.OpenUrl("https://seblague.github.io/chess-coding-challenge/documentation/");
            buttonPos.X = buttonSizeSmallPositionX2;
            if (NextButtonInRow("Submission Page", ref buttonPos, spacingY, buttonSizeSmall, fontSizeNormal))
                FileHelper.OpenUrl("https://forms.gle/6jjj8jxNQ5Ln53ie6");

            // Window and quit buttons
            buttonPos.X = buttonSizeSmallPositionX1;
            buttonPos.Y = UIHelper.ScaleInt(1030);

            bool isBigWindow = Raylib.GetScreenWidth() > Settings.ScreenSizeSmall.X;
            string windowButtonName = isBigWindow ? "Smaller Window" : "Bigger Window";
            if (NextButtonInRow(windowButtonName, ref buttonPos, 0, buttonSizeSmall, fontSizeNormal))
                Program.SetWindowSize(isBigWindow ? Settings.ScreenSizeSmall : Settings.ScreenSizeBig);
            buttonPos.X = buttonSizeSmallPositionX2;
            if (NextButtonInRow("Exit (ESC)", ref buttonPos, spacingY, buttonSizeSmall, fontSizeNormal))
                Environment.Exit(0);

            
            // Game Control

            //Swap players
            buttonPos = ScaleVector(265, 100);
            if (NextButtonInRow("<>", ref buttonPos, 0, UIHelper.Scale(new Vector2(25,25)), fontSizeSmall)) {
                (selectedPlayer2, selectedPlayer1) = (selectedPlayer1, selectedPlayer2);
                (timeControl2Input, timeControl1Input) = (timeControl1Input, timeControl2Input);
                (timeIncrement2Input, timeIncrement1Input) = (timeIncrement1Input, timeIncrement2Input);
            }

            //Play game
            buttonPos = ScaleVector(66, 100 + initY);
            if (NextButtonInRow("Play", ref buttonPos, 0, buttonSizeXSmall, fontSizeNormal, controller.IsGameInProgress())) {
                int timeControl1 = timeControl1Input == "" ? Settings.MAX_TIME : int.Parse(timeControl1Input);
                if (timeControl1 == 0) timeControl1 = Settings.MAX_TIME;
                int timeControl2 = timeControl2Input == "" ? Settings.MAX_TIME : int.Parse(timeControl2Input);
                if (timeControl2 == 0) timeControl2 = Settings.MAX_TIME;

                int timeIncrement1 = timeIncrement1Input == "" ? Settings.DefaultIncrementMilliseconds : int.Parse(timeIncrement1Input);
                int timeIncrement2 = timeIncrement2Input == "" ? Settings.DefaultIncrementMilliseconds : int.Parse(timeIncrement2Input);

                int timeBetweenGames = timeBetweenGameInput == "" ? Settings.DefaultTimeBetweenGames : int.Parse(timeBetweenGameInput);
                controller.startNextGameDelayMs = timeBetweenGames;

                ChallengeController.PlayerType player1 = ChallengeController.ActivePlayers[selectedPlayer1];
                ChallengeController.PlayerType player2 = ChallengeController.ActivePlayers[selectedPlayer2];

                controller.StartNewGamesMatch(player1, player2, timeControl1, timeIncrement1, timeControl2, timeIncrement2);
            }
            
            //End game
            buttonPos.X = UIHelper.ScaleInt(195);
            if (NextButtonInRow("End", ref buttonPos, 0, buttonSizeXSmall, fontSizeNormal))
                controller.EndGame(false);

            //Toggle Pause game
            buttonPos.X = UIHelper.ScaleInt(336);
            if (NextButtonInRow("Paused", ref buttonPos, 0, buttonSizeXSmall, fontSizeNormal, controller.IsPaused())) {
                if (controller.IsPaused())
                    controller.ResumeGame();
                else
                    controller.PauseGame();
            }

            //Toggle fast forward
            buttonPos.X = UIHelper.ScaleInt(465);
            if (NextButtonInRow(">>", ref buttonPos, spacingY, buttonSizeXSmall, fontSizeNormal, controller.fastForward))
                controller.fastForward = !controller.fastForward;
            

            //time control input
            buttonPos.X = UIHelper.ScaleInt(90);
            UIHelper.DrawText("P1 Time: ", buttonPos, UIHelper.ScaleInt(24), 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(90);
            UIHelper.TextInput(ref timeControl1Input, ref isTimeControl1InputActive, ref isMouseOverTextInput, buttonPos, ScaleVector(140,35), UIHelper.ScaleInt(32), "infinity", MAX_INPUT_LENGTH);

            buttonPos.X = UIHelper.ScaleInt(350);
            UIHelper.DrawText("P2 Time: ", buttonPos, UIHelper.ScaleInt(24), 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(90);
            UIHelper.TextInput(ref timeControl2Input, ref isTimeControlInput2Active, ref isMouseOverTextInput, buttonPos, ScaleVector(140,35), UIHelper.ScaleInt(32), "infinity", MAX_INPUT_LENGTH);

            buttonPos.Y += UIHelper.ScaleInt(45);
            buttonPos.X = UIHelper.ScaleInt(110);
            UIHelper.DrawText("P1 t/move: ", buttonPos, UIHelper.ScaleInt(24), 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(70);
            UIHelper.TextInput(ref timeIncrement1Input, ref isTimeIncrement1InputActive, ref isMouseOverTextInput, buttonPos, ScaleVector(140,35), UIHelper.ScaleInt(32), "0", MAX_INPUT_LENGTH);

            buttonPos.X = UIHelper.ScaleInt(370);
            UIHelper.DrawText("P2 t/move: ", buttonPos, UIHelper.ScaleInt(24), 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(70);
            UIHelper.TextInput(ref timeIncrement2Input, ref isTimeIncrement2InputActive, ref isMouseOverTextInput, buttonPos, ScaleVector(140,35), UIHelper.ScaleInt(32), "0", MAX_INPUT_LENGTH);

            buttonPos.X = UIHelper.ScaleInt(370);
            buttonPos.Y += UIHelper.ScaleInt(45);
            UIHelper.DrawText("Wait t/game: ", buttonPos, UIHelper.ScaleInt(22), 0, Color.WHITE, UIHelper.AlignH.Right, UIHelper.AlignV.Centre);
            buttonPos.X += UIHelper.ScaleInt(70);
            UIHelper.TextInput(ref timeBetweenGameInput, ref isTimeBetweenGameInputActive, ref isMouseOverTextInput, buttonPos, ScaleVector(140,35), UIHelper.ScaleInt(32), "" + Settings.DefaultTimeBetweenGames, MAX_INPUT_LENGTH);

            
            buttonPos.X = buttonSizeSmallPositionX2;
            buttonPos.Y = UIHelper.ScaleInt(370);
            if (NextButtonInRow("Auto Perspective", ref buttonPos, 0, buttonSizeSmall, UIHelper.ScaleInt(28), controller.doSwitchPerspective))
                controller.doSwitchPerspective = !controller.doSwitchPerspective;
            
            //player selection
            DropdownListSelectPlayersHelper(ChallengeController.ActivePlayers, ref isPlayer1SelectionOpen, ScaleVector(130, 100), ScaleVector(240,35), ref selectedPlayer1);
            if (isPlayer1SelectionOpen) isPlayer2SelectionOpen = false;

            DropdownListSelectPlayersHelper(ChallengeController.ActivePlayers, ref isPlayer2SelectionOpen, ScaleVector(400, 100), ScaleVector(240,35), ref selectedPlayer2);
            if (isPlayer2SelectionOpen) isPlayer1SelectionOpen = false;

            //update cursor
            Raylib.SetMouseCursor(isMouseOverTextInput ? MouseCursor.MOUSE_CURSOR_IBEAM : MouseCursor.MOUSE_CURSOR_DEFAULT);
        }

        public static Vector2 ScaleVector(float x, float y) {
            return UIHelper.Scale(new Vector2(x, y));
        }

        //helper methods
        public static void DropdownListSelectPlayersHelper(ChallengeController.PlayerType[] playerChoices, ref bool isOpen, Vector2 pos, Vector2 size, ref int selectedPlayer) {
            string[] options = new string[playerChoices.Length];
            for (int i = 0; i < playerChoices.Length; i++)
                options[i] = GetShortName(playerChoices[i],20);

            DropdownList(selectedPlayer < 0 ? "Choose" : options[selectedPlayer], options, ref isOpen, pos, size, UIHelper.ScaleInt(28), UIHelper.ScaleInt(26), ref selectedPlayer);


        }
        
        //universal button
        public static bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size, int fontSize, bool selected = false) {
            bool pressed = UIHelper.Button(name, pos, size, fontSize, selected);
            pos.Y += spacingY;
            return pressed;
        }

        //universal dropdown list
        public static void DropdownList(string text, string[] options, ref bool isOpen, Vector2 pos, Vector2 size, int fontSize, int itemFontSize, ref int selectedOption) {
            //create button for dropdown list
            bool pressed = UIHelper.Button(text, pos, size, fontSize, isOpen);
            if (pressed) isOpen = !isOpen;

            //if it is not open, return
            if (!isOpen) return;

            //create list beneath button
            Vector2 itemPos = new(pos.X, pos.Y + UIHelper.ScaleInt(45));
            Vector2 itemSize = ScaleVector(250, 35);

            float initX = itemPos.X;
            bool toggle = false; //two columns of buttons
            for (int i = 0; i < options.Length; i++) {
                bool itemPressed = UIHelper.Button(options[i], itemPos, itemSize, itemFontSize, i == selectedOption);

                //item is pressed, return the item and close the list
                if (itemPressed) {
                    selectedOption = i;
                    isOpen = false;
                    return;
                }
                
                //build row
                if (toggle) {
                    itemPos.Y += UIHelper.ScaleInt(35);
                    itemPos.X = initX;
                } else
                    itemPos.X += UIHelper.ScaleInt(250);
                
                toggle = !toggle;
            }

            //detect if mouse has been pressed in some other frame
            bool mouseOver = UIHelper.MouseInRect(UIHelper.GetRectangle(pos, size));
            bool pressedNotThisFrame = !mouseOver && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);

            isOpen = !pressedNotThisFrame;
        }
    }
}