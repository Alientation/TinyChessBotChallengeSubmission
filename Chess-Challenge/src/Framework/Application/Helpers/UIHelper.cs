using Raylib_cs;
using System;
using System.Numerics;
using System.Text.RegularExpressions;
using static ChessChallenge.Application.FileHelper;

namespace ChessChallenge.Application {
    public static class UIHelper {
        static readonly bool SDF_Enabled = true;
        const string fontName = "OPENSANS-SEMIBOLD.TTF";
        const int referenceResolution = 1920;

        //how fast the blinker blinks (times per second)
        const int textInputBlinkingSpeed = 3;

        //stats for how fast backspace should act
        public static double lastTimeBackspace = 0;
        public static double firstTimeBackspace = 0;
        public const double initalBackspaceDelay = .5;
        public const double backspaceDelay = .02;

        //text input get cursor position on text
        public static Vector2 lastMousePressedPos = new(-1, -1);
        public static int cursorAtTextLocationFromEnd = 0;

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


        public static void NumberInput(ref string existingText, ref bool isActive, ref bool isMouseOver, Vector2 centre, Vector2 size, int fontSize, string textHint = "input text", int maxInputLength = 1000, int borderThickness = 5, AlignH alignH = AlignH.Centre, AlignV alignV = AlignV.Centre) {
            TextInput(ref existingText, ref isActive, ref isMouseOver, centre, size, fontSize, textHint, maxInputLength, borderThickness, alignH, alignV);
            existingText = Regex.Replace(existingText, "[^0-9]", "");
        }

        //draws text input box and returns the text inside the box after user input is processed, if it is open (active) and 
        //if mouse is hovering over it (for mouse cursor effect)
        //
        //supports pasting in text, backspace, enter (to escape the text), clicking on the text to move the cursor, and of course typing in text
        public static void TextInput(ref string existingText, ref bool isActive, ref bool isMouseOver, Vector2 centre, Vector2 size, int fontSize, string textHint = "input text", int maxInputLength = 1000, int borderThickness = 5, AlignH alignH = AlignH.Centre, AlignV alignV = AlignV.Centre) {
            //inner and outer rectangles for input
            Rectangle recInside = GetRectangle(centre, size);

            size.X += ScaleInt(borderThickness);
            size.Y += ScaleInt(borderThickness);
            Rectangle recOutside = GetRectangle(centre, size);

            //colors for various states
            Color normalCol = new(40, 40, 40, 255);
            Color hoverCol = new(3, 173, 252, 255);
            Color pressCol = new(2, 119, 173, 255);
            Color insideCol = new(87, 83, 83, 255);
            Color insideHoverCol = new(134, 211, 247, 255);
            Color insidePressCol = new(61, 141, 179, 255);

            //check for mouse inputs
            bool mouseOver = MouseInRect(recOutside);
            isMouseOver = mouseOver || isMouseOver;
            bool pressed = mouseOver && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT);
            if (pressed) lastMousePressedPos = Raylib.GetMousePosition();

            //mouse just pressed on text while it is open so move cursor to approriate location
            if (isActive && pressed && PositionInRectangle(lastMousePressedPos, recOutside)) {
                //resset cursor position to end of text just in case
                cursorAtTextLocationFromEnd = existingText.Length;

                //figure out the starting location for text
                Vector2 boundSize = Raylib.MeasureTextEx(font, existingText, fontSize, 1);
                float offsetX = alignH == AlignH.Left ? 0 : (alignH == AlignH.Centre ? -boundSize.X / 2 : -boundSize.X);
                float textEndX = centre.X + offsetX; //end of current text

                //process text to find where the cursor should be
                for (int i = 0; i < existingText.Length; i++) {
                    //moves the cursor between 2 characters if it is between the halfway points of each of the 2 characters
                    if (i > 0) textEndX += .5f * Raylib.MeasureTextEx(font, existingText.Substring(i - 1, 1), fontSize, 1).X;
                    textEndX += .5f * Raylib.MeasureTextEx(font, existingText.Substring(i, 1), fontSize, 1).X;

                    //while cursor is to the left of the end of text update cursor position
                    if (Raylib.GetMousePosition().X < textEndX) break;
                    cursorAtTextLocationFromEnd = existingText.Length - i - 1;
                }
            }

            //mouse pressed outside of text input box, so is no longer active
            if (isActive && !mouseOver && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                isActive = false;
            else if (!isActive && pressed) { //not active but it was pressed so isActive is true now, reset cursor location to end of text
                cursorAtTextLocationFromEnd = 0;
                isActive = true;
            }

            //draw the text input box
            Color col1 = isActive ? (pressed ? pressCol : hoverCol) : normalCol;
            Color col2 = isActive ? (pressed ? insidePressCol : insideHoverCol) : insideCol;

            Raylib.DrawRectangleRec(recOutside, col1);
            Raylib.DrawRectangleRec(recInside, col2);

            Color textCol = isActive ? Color.WHITE : new Color(180, 180, 180, 255);

            //returns current text and closes the text input
            if (!isActive) {
                DrawText(existingText.Length == 0 ? textHint : existingText, centre, fontSize, 1, textCol, alignH, alignV);
                return;
            } else { //blinking cursor when active and at the cursor location
                bool doBlink = Math.Round(Raylib.GetTime() * textInputBlinkingSpeed) % 2 == 0;
                string text = existingText;

                DrawText(text, centre, fontSize, 1, textCol, alignH, alignV);

                if (doBlink) {
                    Vector2 boundSize = Raylib.MeasureTextEx(font, existingText, fontSize, 1);
                    float offsetX = alignH == AlignH.Left ? 0 : (alignH == AlignH.Centre ? -boundSize.X / 2 : -boundSize.X);

                    Raylib.DrawRectangleRec(new Rectangle(offsetX + centre.X + Raylib.MeasureTextEx(font, text[..(existingText.Length - cursorAtTextLocationFromEnd)], fontSize, 1).X, centre.Y - fontSize / 2, ScaleInt(2), fontSize), textCol);
                }
            }            
            
            //get keys pressed since last frame
            int key = Raylib.GetKeyPressed();
            while (key != 0) {
                if (key == (int)KeyboardKey.KEY_BACKSPACE) { //initial backspace, reset previous backspace sequences timings
                    if (cursorAtTextLocationFromEnd < existingText.Length && existingText.Length > 0) {
                        existingText = existingText.Remove(existingText.Length - cursorAtTextLocationFromEnd - 1,1);

                        firstTimeBackspace = Raylib.GetTime();
                        lastTimeBackspace = firstTimeBackspace;
                    }
                } else if (key == (int)KeyboardKey.KEY_ENTER) { //entered, so return what we have and close the window
                    while (Raylib.GetKeyPressed() != 0) { //get rid of all keys in queue
                    }

                    //reset cursor icon
                    Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
                    isActive = false;
                    return;
                } else if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL)) { //left control
                    if (key == (int) KeyboardKey.KEY_V) //check for pasting and add to text at cursor location
                        existingText = existingText[..(existingText.Length - cursorAtTextLocationFromEnd)] + Raylib.GetClipboardText_() + existingText[(existingText.Length - cursorAtTextLocationFromEnd)..];
                } else { //normal key pressed, add to text at cursor location
                    existingText = existingText[..(existingText.Length - cursorAtTextLocationFromEnd)] + (char) key + existingText[(existingText.Length - cursorAtTextLocationFromEnd)..];
                }

                //trim to max length
                if (existingText.Length > maxInputLength) existingText = existingText[..maxInputLength];
                
                //get next key in queue
                key = Raylib.GetKeyPressed();
            }

            //check for backspace sequence (held down for long enough)
            if (Raylib.GetTime() - firstTimeBackspace > initalBackspaceDelay && 
                Raylib.GetTime() - lastTimeBackspace > backspaceDelay && 
                Raylib.IsKeyDown(KeyboardKey.KEY_BACKSPACE)) {
                if (cursorAtTextLocationFromEnd < existingText.Length && existingText.Length > 0) {
                    existingText = existingText.Remove(existingText.Length - cursorAtTextLocationFromEnd - 1,1);
                    lastTimeBackspace = Raylib.GetTime();
                }
            }

            //keep it open if not escaped
            return;
        }

        public static Rectangle GetRectangle(Vector2 centre, Vector2 size) {
            return new(centre.X - size.X / 2, centre.Y - size.Y / 2, size.X, size.Y);
        }

        public static bool Button(string text, Vector2 centre, Vector2 size, int fontSize, bool isSelected = false) {
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

            DrawText(text, centre, fontSize, 1, textCol, AlignH.Centre);

            return pressedThisFrame;
        }

        public static bool MouseInRect(Rectangle rec) {
            Vector2 mousePos = Raylib.GetMousePosition();
            return mousePos.X >= rec.x && mousePos.Y >= rec.y && mousePos.X <= rec.x + rec.width && mousePos.Y <= rec.y + rec.height;
        }

        public static bool PositionInRectangle(Vector2 pos, Rectangle rec) {
            return pos.X >= rec.x && pos.Y >= rec.y && pos.X <= rec.x + rec.width && pos.Y <= rec.y + rec.height;
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
