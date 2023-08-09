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


        //Text Colors
        public static Color LightTextColor = new(180, 180, 180, 255);


        //BoardUI Colors
        public static Color ActivePlayerTextColor = new(200, 200, 200, 255);
        public static Color InactivePlayerTextColor = new(100, 100, 100, 255);
        public static Color PlayerNameColor = new(67, 204, 101, 255);

        public static Color BitboardColorZERO = new(61, 121, 217, 200);
        public static Color BitboardColorONE = new(252, 43, 92, 200);


        public static Color LightColor = new(238, 216, 192, 255);
        public static Color DarkColor = new(171, 121, 101, 255);

        public static Color SelectedLightColor = new(236, 197, 123, 255);
        public static Color SelectedDarkColor = new(200, 158, 80, 255);

        public static Color MoveFromLightColor = new(207, 172, 106, 255);
        public static Color MoveFromDarkColor = new(197, 158, 54, 255);

        public static Color MoveToLightColor = new(221, 208, 124, 255);
        public static Color MoveToDarkColor = new(197, 173, 96, 255);

        public static Color LegalLightColor = new(89, 171, 221, 255);
        public static Color LegalDarkColor = new(62, 144, 195, 255);

        public static Color CheckLightColor = new(234, 74, 74, 255);
        public static Color CheckDarkColor = new(207, 39, 39, 255);

        public static Color BorderColor = new(44, 44, 44, 255);

        public static Color LightCoordColor = new(255, 240, 220, 255);
        public static Color DarkCoordColor = new(140, 100, 80, 255);

        public static Color PremoveLightColor = new(255, 142, 105, 255);
        public static Color PremoveDarkColor = new(255, 142, 105, 255);
    }
}

