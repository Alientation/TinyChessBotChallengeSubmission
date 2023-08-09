using Raylib_cs;

namespace ChessChallenge.Application {
    public class Theme {
        //Window Colors
        public static Color WindowBackgroundColor = new(36, 31, 31, 255);

        
        //Button Colors
        public static Color ButtonBackgroundColor = new(48, 44, 44, 255);
        public static Color ButtonBackgroundHoverColor = new(3, 173, 252, 255);
        public static Color ButtonBackgroundPressedColor = new(2, 119, 173, 255);

        public static Color ButtonTextColor = new (180, 180, 180, 255);
        public static Color ButtonTextHoverColor = Color.WHITE;


        //Text Input Colors
        public static Color TextInputBackgroundColor = new(40, 40, 40, 255);
        public static Color TextInputInsideColor = new(87, 83, 83, 255);
        
        public static Color TextInputBackgroundHoverColor = new(3, 173, 252, 255);
        public static Color TextInputInsideHoverColor = new(134, 211, 247, 255);

        public static Color TextInputBackgroundPressedColor = new(2, 119, 173, 255);
        public static Color TextInputInsidePressedColor = new(61, 141, 179, 255);

        public static Color TextInputTextColor = new(180, 180, 180, 255);
        public static Color TextInputTextHoverColor = Color.WHITE;


        //BoardUI Colors
        public static Color LightCol = new(238, 216, 192, 255);
        public static Color DarkCol = new(171, 121, 101, 255);

        public static Color SelectedLight = new(236, 197, 123, 255);
        public static Color SelectedDark = new(200, 158, 80, 255);

        public static Color MoveFromLight = new(207, 172, 106, 255);
        public static Color MoveFromDark = new(197, 158, 54, 255);

        public static Color MoveToLight = new(221, 208, 124, 255);
        public static Color MoveToDark = new(197, 173, 96, 255);

        public static Color LegalLight = new(89, 171, 221, 255);
        public static Color LegalDark = new(62, 144, 195, 255);

        public static Color CheckLight = new(234, 74, 74, 255);
        public static Color CheckDark = new(207, 39, 39, 255);

        public static Color BorderCol = new(44, 44, 44, 255);

        public static Color LightCoordCol = new(255, 240, 220, 255);
        public static Color DarkCoordCol = new(140, 100, 80, 255);

        public static Color PremoveLight = new(255, 142, 105, 255);
        public static Color PremoveDark = new(255, 142, 105, 255);
    }
}

