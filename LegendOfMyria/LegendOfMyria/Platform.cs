using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendOfMyria
{
    class Platform
    {
        //determine if this object is active or not
        public bool Active;

        //used for keeping track of frame animation
        float delay;
        float elapsed;

        //the current frame of the animation
        int frames;
        int Health;

        public int Height;
        public int Width;

        //This will tell us if the left or right side of the platform is a cliff and the player should fall off
        string cliffSide;

        //texture of the platform
        Texture2D platform;
        Texture2D touched;

        //x, y coords of the platform in relation to the upper left corner
        public Vector2 Position;

        //initializing platform variables
        public void Initialize(Texture2D texture, int width, int height, Vector2 startPos, string cliff)
        {
            Active = false;

            delay = 90f;

            frames = 0;
            Health = 100;

            Width = width;
            Height = height;

            cliffSide = cliff;

            platform = texture;

            Position = startPos;
        }

        //this is called to update the platform every frame
        public void Update(GameTime gameTime)
        {

        }

        //drawing the platform graphic to the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle platformRect = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);
            if(Active)
            {
                spriteBatch.Draw(touched, platformRect, Color.White);
            }
            else
            {
                spriteBatch.Draw(platform, platformRect, Color.White);
            }
        }

        public float getBottom()
        {
            return this.Position.Y + this.Height;
        }

        public float getRightSide()
        {
            return this.Position.X + this.Width;
        }

        public string getCliffSide()
        {
            return cliffSide;
        }

        public void setTexture(Texture2D texture)
        {
            touched = texture;
        }
    }
}
