using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendOfMyria
{
    class Player
    {
        //determine if this object is active or not
        public bool Active;

        //Lars is facing right
        public bool facingRight;

        //used for keeping track of frame animation
        float delay;
        float elapsed;

        //how high Lars can jump, measured by pixels/frames per second
        public float jumpSpeed;

        //Lars' movement speed
        public float moveSpeed;
        
        //the current frame of the animation
        int frames;

        //Amount of hit points Lars has
        int Health;

        public int terminalVelocity;

        //the height and width of Lars
        public int Height;
        public int Width;

        public Platform currentlyTouching;

        //the rectangle that contains Lars' animation
        Rectangle animationRect;

        //current stat of the player
        string playerState;

        //Lars' jumping graphic
        Texture2D jumpingRight;
        Texture2D jumpingLeft;

        //Texture of Lars standing
        Texture2D standingRight;
        Texture2D standingLeft;

        //Lars' walking animation
        Texture2D walkingRight;
        Texture2D walkingLeft;

        //Position of Lars relative to the upper left side of the screen
        public Vector2 Position;

        //Lars' velocity will be contained in a vector
        public Vector2 velocity;

        //starting position of Lars on the map
        public Vector2 startPos = new Vector2(100, 200);

        public int jumpedFrames;

        public int released;

        //initializing Lars' variables that make him tick
        public void Initialize(int fps)
        {
            Active = true;

            playerState = "falling";
            facingRight = true;

            delay = 90f;
            jumpSpeed = 300 / (fps/4);
            moveSpeed = 8.0f;
            

            frames = 0;
            Health = 100;
            terminalVelocity = 50;

            Width = 75;
            Height = 152;

            velocity = new Vector2(0, 0);

            jumpedFrames = 0;

            released = 0;
        }

        //this is called to update the player information every frame
        public void Update(GameTime gameTime)
        {
            /**
             * This updates the animation of the player
             **/
            if (playerState == "walking")
            {
                elapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (elapsed >= delay)
                {
                    if (facingRight)
                    {
                        if (frames >= 7)
                        {
                            frames = 0;
                        }
                        else
                        {
                            frames++;
                        }
                    }
                    else
                    {
                        if (frames <= 0)
                        {
                            frames = 7;
                        }
                        else
                        {
                            frames--;
                        }
                    }
                    elapsed = 0;
                }

                animationRect = new Rectangle(79 * frames, 0, Width, Height);                
            }
            else
            {
                elapsed = 0;
                if (facingRight)
                {
                    frames = 0;
                }
                else
                {
                    frames = 7;
                }
            }

            /*
             * When the player stop his upward movement during the jump he will be considered
             * falling and we must update the player's state
             **/
            if (playerState == "jumping" && velocity.Y >= 0)
            {
                playerState = "falling";
            }
        }

        //drawing Lars's graphic to the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle playerRect = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);

            if (playerState == "walking")
            {
                if (facingRight)
                {
                    spriteBatch.Draw(walkingRight, Position, animationRect, Color.White);
                }
                else
                {
                    spriteBatch.Draw(walkingLeft, Position, animationRect, Color.White);
                }
            }
            else if (playerState == "jumping" || playerState == "falling")
            {
                if (facingRight)
                {
                    spriteBatch.Draw(jumpingRight, playerRect, Color.White);
                }
                else
                {
                    spriteBatch.Draw(jumpingLeft, playerRect, Color.White);
                }
            }
            else
            {
                if (facingRight)
                {
                    spriteBatch.Draw(standingRight, playerRect, Color.White);
                }
                else
                {
                    /**
                     * for some odd reason when we apply the Height and Width variables
                     * it streches out the image for the player.  The variables are not
                     * changing in value, the image is smaller which XNA streches the 
                     * image to fit the rectangle.  Quick fix is use the Position vector
                     **/
                    spriteBatch.Draw(standingLeft, playerRect, Color.White);
                }
            }
        }

        public float getFeet()
        {
            return this.Position.Y + this.Height;
        }

        public float getRightSide()
        {
            return this.Position.X + this.Width;
        }

        public string getState()
        {
            return playerState;
        }

        public Platform getTouching()
        {
            return currentlyTouching;
        }

        public float getWaist()
        {
            return this.Position.Y + (this.Height / 2);
        }        

        public void respawn()
        {
            Position = startPos;
        }

        public void setState(string state)
        {
            playerState = state;
        }

        public void setTexture(string sTexture, Texture2D texture)
        {
            switch (sTexture)
            {
                case "sl":
                    standingLeft = texture;
                    break;

                case "sr":
                    standingRight = texture;
                    break;

                case "wl":
                    walkingLeft = texture;
                    break;

                case "wr":
                    walkingRight = texture;
                    break;

                case "jl":
                    jumpingLeft = texture;
                    break;

                case "jr":
                    jumpingRight = texture;
                    break;
            }
        }

        public void setTouching(Platform platform)
        {
            currentlyTouching = platform;
        }
    }
}
