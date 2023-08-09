using Raylib_cs;

namespace ChessChallenge.Application
{
    public static class BotBrainCapacityUI
    {
        static readonly Color Green = new(17, 212, 73, 255);
        static readonly Color Yellow = new(219, 161, 24, 255);
        static readonly Color Orange = new(219, 96, 24, 255);
        static readonly Color Red = new(219, 9, 9, 255);
        static readonly Color Background = new(40, 40, 40, 255);

        public static void Draw(string name1, string name2, int totalTokenCount1, int debugTokenCount1, int totalTokenCount2, int debugTokenCount2, int tokenLimit)
        {
            int activeTokenCount1 = totalTokenCount1 - debugTokenCount1;
            int activeTokenCount2 = totalTokenCount2 - debugTokenCount2;

            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            int height = UIHelper.ScaleInt(48);
            int fontSize = UIHelper.ScaleInt(30);

            int startX = UIHelper.ScaleInt(1200);
            // Bg
            Raylib.DrawRectangle(startX, 0, screenWidth - startX, height, Background);
            Raylib.DrawRectangle(startX, screenHeight - height, screenWidth - startX, height, Background);
            // Bar
            double t1 = (double)activeTokenCount1 / tokenLimit;
            double t2 = (double)activeTokenCount2 / tokenLimit;

            Color col1 = getColor(t1);
            Color col2 = getColor(t2);

            static Color getColor(double val) {
                if (val <= 0.7)
                    return Green;
                else if (val <= 0.85)
                    return Yellow;
                else if (val <= 1)
                    return Orange;
                else
                    return Red;
            }

            Raylib.DrawRectangle(startX, 0, (int)((screenWidth - startX) * t1), height, col1);
            Raylib.DrawRectangle(startX, screenHeight - height, (int)((screenWidth - startX) * t2), height, col2);

            var textPos1 = new System.Numerics.Vector2(startX + (screenWidth - startX) / 2, height / 2);
            var textPos2 = new System.Numerics.Vector2(startX + (screenWidth - startX) / 2, screenHeight - height / 2);
            string text1 = name1 + $"Bot Brain Capacity: {activeTokenCount1}/{tokenLimit}";
            string text2 = name2 + $"Bot Brain Capacity: {activeTokenCount2}/{tokenLimit}";
            if (activeTokenCount1 > tokenLimit)
                text1 += " [LIMIT EXCEEDED]";
            else if (debugTokenCount1 != 0)
                text1 += $"    ({totalTokenCount1} with Debugs included)";
            
            if (activeTokenCount2 > tokenLimit)
                text2 += " [LIMIT EXCEEDED]";
            else if (debugTokenCount1 != 0)
                text2 += $"    ({totalTokenCount2} with Debugs included)";

            UIHelper.DrawText(text1, textPos1, fontSize, 1, Color.WHITE, UIHelper.AlignH.Centre);
            UIHelper.DrawText(text2, textPos2, fontSize, 1, Color.WHITE, UIHelper.AlignH.Centre);
        }
    }
}