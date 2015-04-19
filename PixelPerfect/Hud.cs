using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect
{
    class Hud
    {
        String levelName = "";
        List<Tile> collectibleTiles = new List<Tile>();
        List<Tile> collectedTiles = new List<Tile>();

        public bool enabled = true;

        private Texture2D background = Util.GetGradientTexture(Config.SCREEN_WIDTH_SCALED, Config.Hud.HUD_HEIGHT, Color.MidnightBlue, Color.DarkSlateBlue, Util.GradientType.Horizontal);
        public Hud() {}

        public void Init(string levelName, int collectiblesCount)
        {
            this.levelName = levelName;

            collectibleTiles.Clear();
            collectedTiles.Clear();

            for (int i = 0; i < collectiblesCount; i++)
            {
                collectibleTiles.Add(TileFactory.CreateTile((int)Config.TileType.KEY, new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + Config.Hud.COLLECTIBLES_SPACE + i * Config.Hud.COLLECTIBLES_SPACE, Config.Hud.COLLECTIBLES_Y)));
                ((CollectibleTile)collectibleTiles[i]).Deactivate();
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (Tile tile in collectibleTiles)
                tile.Update(gameTime);

            foreach (Tile tile in collectedTiles)
                tile.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!enabled)
                return;

            if (background != null)
                spriteBatch.Draw(background, new Vector2(0, Config.SCREEN_HEIGHT_SCALED - Config.Hud.HUD_HEIGHT), Color.White);

            //spriteBatch.Draw(Globals.pixelTexture, new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - Config.Hud.HUD_HEIGHT, Config.SCREEN_WIDTH_SCALED + 20, Config.SCREEN_HEIGHT_SCALED), Color.Black);

            Util.DrawStringAligned(spriteBatch, "Time: " + Globals.CurrentLevelState.levelTime.ToString("mm\\:ss\\.f"), Globals.silkscreenFont, Color.White,
                        new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 22, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED),
                        new Vector2(4, Config.Hud.TEXT_POSITION_Y), Util.Align.Left);
             
            Util.DrawStringAligned(spriteBatch, levelName, Globals.silkscreenFont, Color.White,
                                   new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 22, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED),
                                   new Vector2(0, Config.Hud.TEXT_POSITION_Y), Util.Align.Center);

            Util.DrawStringAligned(spriteBatch, "DEATHS: " + Globals.CurrentLevelState.deathCount, Globals.silkscreenFont, Color.White,
                        new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 22, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED),
                        new Vector2(0, Config.Hud.TEXT_POSITION_Y), Util.Align.Right);
             
            foreach (Tile tile in collectibleTiles)
                tile.Draw(spriteBatch, Vector2.Zero);

            foreach (Tile tile in collectedTiles)
                tile.Draw(spriteBatch, Vector2.Zero);
        }

        public void Collect()
        {
            collectibleTiles.RemoveAt(collectibleTiles.Count - 1);
            collectedTiles.Add(TileFactory.CreateTile((int)Config.TileType.KEY,
                                                      new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - (collectedTiles.Count + 2) * (Config.Hud.COLLECTIBLES_SPACE), Config.Hud.COLLECTIBLES_Y)));
        }

        public void SetText(string text)
        {
            levelName = text;
        }
    }
}
