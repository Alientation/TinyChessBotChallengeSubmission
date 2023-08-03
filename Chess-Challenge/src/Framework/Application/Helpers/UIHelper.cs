using Raylib_cs;
using System;
using System.Numerics;
using static ChessChallenge.Application.FileHelper;

namespace ChessChallenge.Application {
    public static class UIHelper {
        static readonly bool SDF_Enabled = true;
        const string fontName = "OPENSANS-SEMIBOLD.TTF";
        const int referenceResolution = 1920;
        const int textInputBlinkingSpeed = 3;

        static Font font;
        static Font fontSdf;
        static Shader shader;

        public enum AlignH {
            Left,
            Centre,
            Right
        }
        public enum AlignV {
            Top,
            Centre,
            Bottom
        }

        static UIHelper() {
            if (SDF_Enabled) {
                unsafe {
                    const int baseSize = 64;
                    uint fileSize = 0;
                    var fileData = Raylib.LoadFileData(GetResourcePath("Fonts", fontName), ref fileSize);
                    Font fontSdf = default;
                    fontSdf.baseSize = baseSize;
                    fontSdf.glyphCount = 95;
                    fontSdf.glyphs = Raylib.LoadFontData(fileData, (int)fileSize, baseSize, null, 0, FontType.FONT_SDF);

                    Image atlas = Raylib.GenImageFontAtlas(fontSdf.glyphs, &fontSdf.recs, 95, baseSize, 0, 1);
                    fontSdf.texture = Raylib.LoadTextureFromImage(atlas);
                    Raylib.UnloadImage(atlas);
                    Raylib.UnloadFileData(fileData);

                    Raylib.SetTextureFilter(fontSdf.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
                    UIHelper.fontSdf = fontSdf;

                }
                shader = Raylib.LoadShader("", GetResourcePath("Fonts", "sdf.fs"));
            }
            font = Raylib.LoadFontEx(GetResourcePath("Fonts", fontName), 128, null, 0);
        }

        public static void DrawText(string text, Vector2 pos, int size, int spacing, Color col, AlignH alignH = AlignH.Left, AlignV alignV = AlignV.Centre) {
            Vector2 boundSize = Raylib.MeasureTextEx(font, text, size, spacing);
            float offsetX = alignH == AlignH.Left ? 0 : (alignH == AlignH.Centre ? -boundSize.X / 2 : -boundSize.X);
            float offsetY = alignV == AlignV.Top ? 0 : (alignV == AlignV.Centre ? -boundSize.Y / 2 : -boundSize.Y);
            Vector2 offset = new(offsetX, offsetY);

            if (SDF_Enabled) {
                Raylib.BeginShaderMode(shader);
                Raylib.DrawTextEx(fontSdf, text, pos + offset, size, spacing, col);
                Raylib.EndShaderMode();
            } else {
                Raylib.DrawTextEx(font, text, pos + offset, size, spacing, col);
            }
        }

        public static (string, bool) TextInput(string existingText, bool isActive, Vector2 centre, Vector2 size, string textHint = "input text") {
            //inner and outer rectangles for input
            Rectangle recInside = GetRectangle(centre, size);

            size.X += UIHelper.ScaleInt(5);
            size.Y += UIHelper.ScaleInt(5);
            Rectangle recOutside = GetRectangle(centre, size);

            Color normalCol = new(40, 40, 40, 255);
            Color hoverCol = new(3, 173, 252, 255);
            Color pressCol = new(2, 119, 173, 255);
            Color insideCol = new(87, 83, 83, 255);
            Color insideHoverCol = new(134, 211, 247, 255);
            Color insidePressCol = new(61, 141, 179, 255);

            bool mouseOver = MouseInRect(recOutside);
            bool pressed = mouseOver && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);

            if (isActive && !mouseOver && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) {
                isActive = false;
                Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
            } else if (!isActive && pressed) {
                isActive = true;
                Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_IBEAM);
            }

            Color col1 = isActive ? (pressed ? pressCol : hoverCol) : normalCol;
            Color col2 = isActive ? (pressed ? insidePressCol : insideHoverCol) : insideCol;

            Raylib.DrawRectangleRec(recOutside, col1);
            Raylib.DrawRectangleRec(recInside, col2);

            Color textCol = isActive ? Color.WHITE : new Color(180, 180, 180, 255);
            int fontSize = ScaleInt(32);

            //returns current text and closes the text input
            if (!isActive) {
                DrawText(existingText.Length == 0 ? textHint : existingText, centre, fontSize, 1, textCol, AlignH.Centre);
                return (existingText, false);
            } else { //blinking cursor thing
                bool doBlink = Math.Round(Raylib.GetTime() * textInputBlinkingSpeed) % 2 == 0;
                string text = existingText;

                if (doBlink)
                    text = " " + existingText + (doBlink ? "|" : "");
                DrawText(text, centre, fontSize, 1, textCol, AlignH.Centre);
            }            
            
            //get keys pressed since last frame
            int key = Raylib.GetKeyPressed();
            while (key != 0) {
                if (key == (int)KeyboardKey.KEY_BACKSPACE) {
                    if (existingText.Length > 0)
                        existingText = existingText.Substring(0, existingText.Length - 1);
                } else {
                
                    //entered, so return what we have and close the window
                    if (key == (int)KeyboardKey.KEY_ENTER) {
                        while (Raylib.GetKeyPressed() != 0) {
                            //get rid of all keys in queue
                        }

                        Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
                        return (existingText, false);
                    }

                    existingText += (char) key;
                }
                key = Raylib.GetKeyPressed();
            }

            //keep it open if not escaped
            return (existingText, true);
        }

        public static Rectangle GetRectangle(Vector2 centre, Vector2 size) {
            return new(centre.X - size.X / 2, centre.Y - size.Y / 2, size.X, size.Y);
        }

        public static bool Button(string text, Vector2 centre, Vector2 size, bool isSelected = false) {
            Rectangle rec = GetRectangle(centre, size);

            Color normalCol = new(40, 40, 40, 255);
            Color hoverCol = new(3, 173, 252, 255);
            Color pressCol = new(2, 119, 173, 255);

            bool mouseOver = MouseInRect(rec);
            bool pressed = mouseOver && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);
            bool pressedThisFrame = pressed && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT);
            Color col = mouseOver ? (pressed ? pressCol : hoverCol) : normalCol;

            if (isSelected) col = hoverCol;

            Raylib.DrawRectangleRec(rec, col);
            Color textCol = (mouseOver || isSelected) ? Color.WHITE : new Color(180, 180, 180, 255);
            int fontSize = ScaleInt(32);

            DrawText(text, centre, fontSize, 1, textCol, AlignH.Centre);

            return pressedThisFrame;
        }

        public static bool MouseInRect(Rectangle rec) {
            Vector2 mousePos = Raylib.GetMousePosition();
            return mousePos.X >= rec.x && mousePos.Y >= rec.y && mousePos.X <= rec.x + rec.width && mousePos.Y <= rec.y + rec.height;
        }

        public static float Scale(float val, int referenceResolution = referenceResolution) {
            return Raylib.GetScreenWidth() / (float)referenceResolution * val;
        }

        public static int ScaleInt(int val, int referenceResolution = referenceResolution) {
            return (int)Math.Round(Raylib.GetScreenWidth() / (float)referenceResolution * val);
        }

        public static Vector2 Scale(Vector2 vec, int referenceResolution = referenceResolution) {
            float x = Scale(vec.X, referenceResolution);
            float y = Scale(vec.Y, referenceResolution);
            return new Vector2(x, y);
        }

        public static void Release() {
            Raylib.UnloadFont(font);
            if (SDF_Enabled) {
                Raylib.UnloadFont(fontSdf);
                Raylib.UnloadShader(shader);
            }
        }
    }
}
