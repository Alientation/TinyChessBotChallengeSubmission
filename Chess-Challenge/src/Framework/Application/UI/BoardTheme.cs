using Raylib_cs;

namespace ChessChallenge.Application
{
    public class BoardTheme
    {
        public Color LightCol = new(238, 216, 192, 255);
        public Color DarkCol = new(171, 121, 101, 255);

        public Color selectedLight = new(236, 197, 123, 255);
        public Color selectedDark = new(200, 158, 80, 255);

        public Color MoveFromLight = new(207, 172, 106, 255);
        public Color MoveFromDark = new(197, 158, 54, 255);

        public Color MoveToLight = new(221, 208, 124, 255);
        public Color MoveToDark = new(197, 173, 96, 255);

        public Color LegalLight = new(89, 171, 221, 255);
        public Color LegalDark = new(62, 144, 195, 255);

        public Color CheckLight = new(234, 74, 74, 255);
        public Color CheckDark = new(207, 39, 39, 255);

        public Color BorderCol = new(44, 44, 44, 255);

        public Color LightCoordCol = new(255, 240, 220, 255);
        public Color DarkCoordCol = new(140, 100, 80, 255);

        public Color PremoveLight = new(255, 142, 105, 255);
        public Color PremoveDark = new(255, 142, 105, 255);
    }
}

