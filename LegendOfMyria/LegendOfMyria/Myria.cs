using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LegendOfMyria
{
    /// <summary>
    /// This is the main type for your game.  The game runs at 62 frames per second
    /// </summary>
    public class Myria : Microsoft.Xna.Framework.Game
    {
        //the map of the level represented by a 2d grid of characters
        char[,] levelMap;

        //gravity constant that affects all objects
        float gravity;

        //saving user input from xbone, current and previous
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        //the window the game will be presented on
        GraphicsDeviceManager graphics;

        //game's frames per seconds
        int framesPerSecond;

        //the Height and Width of the window screen
        int windowHalf;

        //Number that holds the player score
        //int score;

        //saving user input from a keyboard, current and previous
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        //platforms throughout the level that are higher than the ground level
        List<Platform> thePlatforms;

        //this will store the input of the player per frame from the start of the game
        List<string> playerInput;

        //this will store the state of the player per frame from the start of the game
        List<string> playerState;

        //this will store the x, y location of the player per frame from the start of the game
        List<Vector2> playerPath;

        //The player character Lars
        Player player;

        //the ground platform.  Lars cannot fall below this levels lest he dies
        Platform dummy;

        //A random number generator
        Random random;

        // The font used to display UI elements
        SpriteFont font;

        SpriteBatch spriteBatch;

        string levelData;

        //Image used to display the static background
        //Texture2D mainBackground;

        //this lets us know where the end point of the level is, used for window movement
        Vector2 maxLevelDistance;

        //This tracks the distance the player has moved from the starting point of the level
        Vector2 windowMovement;



        public Myria()
        {
            //setting the screen variables
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            windowHalf = graphics.PreferredBackBufferWidth / 2;
            Content.RootDirectory = "Content";

            //setting the fps to 64
            framesPerSecond = 64;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / (float)framesPerSecond);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Set player's score to zero
            //score = 0;

            //Initialize the player class
            player = new Player();
            player.Initialize(framesPerSecond);

            //creating environment
            thePlatforms = new List<Platform>();

            levelMap = new char[20, 4];

            //creating the variables to record data 
            playerInput = new List<string>();

            playerState = new List<string>();

            playerPath = new List<Vector2>();

            //a null place holder for the player's variable determining what platform it is current touching
            dummy = new Platform();
            dummy.Initialize(null, 0,0, new Vector2(-1, -1), "none");
            player.setTouching(dummy);

            //creating the gravity constant, which is based on 4% of the player's jump speed
            gravity = player.jumpSpeed * 0.04f;

            // Initialize our random number generator
            random = new Random();

            windowMovement = new Vector2(0, 0);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            /**
             * We determine how the level will be constructed based on a text file
             **/

            //this is the variable that will hold the text buffer information
            //the buffer only reads one line at a time
            levelData = null;

            //this is reading the text file to the text buffer
            using (var stream = TitleContainer.OpenStream("Levels/tutorial.txt"))
            {
                using (var reader = new StreamReader(stream))
                {
                    //for each row in the level map
                    for (int i = 0; i < 4; i++)
                    {
                        //retrieve the row from the file
                        levelData = reader.ReadLine();
                        
                        //for each column or letter per row
                        for (int j = 0; j < levelData.Length; j++)
                        {
                            levelMap[j, i] = levelData[j];
                            if (levelData[j] == 'G')
                            {
                                Platform temp = new Platform();
                                Vector2 vector = new Vector2(200 * j, 200 * i);
                                string cliff;

                                //if the first column should be ground
                                if (j == 0)
                                {
                                    if (levelData[j + 1] != 'G')
                                    {
                                        cliff = "right";
                                    }
                                    else
                                    {
                                        cliff = "none";
                                    }
                                }
                                //if the last column should be ground
                                else if(j == 19)
                                {
                                    if (levelData[j - 1] != 'G')
                                    {
                                        cliff = "left";
                                    }
                                    else
                                    {
                                        cliff = "none";
                                    }
                                }
                                //everything else inside the buffer and we can check both sides of the current position
                                else
                                {
                                    //if both sides of the platform are a cliff
                                    if (levelData[j - 1] != 'G' && levelData[j + 1] != 'G')
                                    {
                                        cliff = "both";
                                    }
                                    //if the left side of the platform is a cliff
                                    else if (levelData[j - 1] != 'G' && levelData[j + 1] == 'G')
                                    {
                                        cliff = "left";
                                    }
                                    //if the right side of the platform is a cliff
                                    else if (levelData[j - 1] == 'G' && levelData[j + 1] != 'G')
                                    {
                                        cliff = "right";
                                    }
                                    //if neither side of the platform is a cliff
                                    else
                                    {
                                        cliff = "none";
                                    }
                                }
                                //creating the platform and adding it to the array of platforms
                                temp.Initialize(Content.Load<Texture2D>("dirtPlatform"), 200, 200, vector, cliff);
                                temp.setTexture(Content.Load<Texture2D>("ground"));
                                thePlatforms.Add(temp);
                            }
                        }
                    }
                }
            }

            maxLevelDistance.X = (levelData.Length * 200)- graphics.PreferredBackBufferWidth;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //setting Lars' textures
            player.setTexture("sl", Content.Load<Texture2D>("StandingLeft"));
            player.setTexture("sr", Content.Load<Texture2D>("StandingRight"));
            player.setTexture("wl", Content.Load<Texture2D>("WalkingAnimationLeft"));
            player.setTexture("wr", Content.Load<Texture2D>("WalkingAnimationRight"));
            player.setTexture("jl", Content.Load<Texture2D>("JumpingLeft"));
            player.setTexture("jr", Content.Load<Texture2D>("JumpingRight"));

            //creating the ground

            
            // Load the score font
            font = Content.Load<SpriteFont>("gameFont");

             //load background image
             //mainBackground = Content.Load<Texture2D>("mainbackground");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }
            if(currentKeyboardState.IsKeyDown(Keys.Escape))
            {
                //creating an array of strings to store all the information that we collected during the game
                string[,] header = new string[playerPath.Count + 1, playerState.Count + 1];
                int count = 1;

                //these are the headers for each column which will tell us what info we are looking at
                header[0, 0] = "Position";
                header[1, 0] = "State";
                header[2, 0] = "Input";

                //we must add all the values for the position, state, and input from the player
                foreach (Vector2 position in playerPath)
                {
                    header[0, count] = ""+position;
                    count++;
                }

                count = 1;

                foreach (string state in playerState)
                {
                    header[1, count] = state;
                    count++;
                }

                count = 1;

                foreach (string input in playerInput)
                {
                    header[2, count] = input;
                    count++;
                }

                //now that we have all the info in the proper order, we shall write to a text file that will be saved on the desktop
                System.IO.StreamWriter file = new System.IO.StreamWriter("C:\\Users\\Anthony\\Desktop\\debug.txt");
                for (int i = 0; i < header.GetLength(0); i++)
                {
                    file.WriteLine(header[0, i] + "\t" + header[1, i] + "\t"+ header[2, i]);
                }
                file.Close();
                //closing out the game
                this.Exit();
            }

            //we shall store the position, state, and input from the player at the beginning of every frame
            playerState.Add(player.getState());
            playerPath.Add(player.Position);

            //We need to determine what keys are being pressed.  So far the only keys we are registering are left, right, and jump
            if (currentKeyboardState.IsKeyDown(Keys.A) &&
                currentKeyboardState.IsKeyDown(Keys.D) &&
                currentKeyboardState.IsKeyDown(Keys.Space))
            {
                playerInput.Add("left, right, and jump");
            }
            else if (currentKeyboardState.IsKeyDown(Keys.A) &&
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                playerInput.Add("left and right");
            }
            else if (currentKeyboardState.IsKeyDown(Keys.A) &&
                currentKeyboardState.IsKeyDown(Keys.Space))
            {
                playerInput.Add("left and jump");
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D) &&
                currentKeyboardState.IsKeyDown(Keys.Space))
            {
                playerInput.Add("right and jump");
            }
            else if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                playerInput.Add("left");
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                playerInput.Add("right");
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Space))
            {
                playerInput.Add("space");
            }
            else
            {
                playerInput.Add("none");
            }

            // Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            //Update the player
            UpdatePlayer(gameTime);

            ApplyGravity();

            //detect collisions
            CollisionDetection();
            
            //Apply gravity to objects
            ApplyMovement();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Start drawing
            spriteBatch.Begin();

            //spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

            // Draw the Player
            player.Draw(spriteBatch);

            //Draw the ground
            foreach (Platform element in thePlatforms)
            {
                if (player.currentlyTouching == element)
                {
                    element.Active = true;
                }
                else
                {
                    element.Active = false;
                }
                element.Draw(spriteBatch);
            }

            //display for testing variables
            spriteBatch.DrawString(font, player.getState(), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
            //spriteBatch.DrawString(font, "v: " + player.velocity.X, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);


            //Stop drawing
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void ApplyGravity()
        {
            //applying gravity to the player
            if (player.getState() == "falling" || player.getState() == "jumping")
            {
                if (player.velocity.Y < player.terminalVelocity)
                {
                    player.velocity.Y += gravity;
                }
                else
                {
                    player.velocity.Y = player.terminalVelocity;
                }
            }
            else if (player.getState() == "climbing")
            {
                player.velocity.Y = player.jumpSpeed * 0.1f;
            }
            else if (player.getState() == "wallJump")
            {
                if (player.facingRight)
                {
                    player.velocity = new Vector2(player.velocity.X - 1, player.velocity.Y + gravity);
                }
                else
                {
                    player.velocity = new Vector2(player.velocity.X + 1, player.velocity.Y + gravity);
                }

                if (player.velocity.X == 0)
                {
                    player.setState("falling");
                }
            }
        }

        private void ApplyMovement()
        {
            /**
             * This function will move all the objects in the game.
             **/

            //moving the player
            if (player.Position.X >= windowHalf && player.velocity.X > 0 && windowMovement.X < maxLevelDistance.X)
            {

                if (maxLevelDistance.X - windowMovement.X < player.velocity.X)
                {
                    MoveScreen(maxLevelDistance.X - windowMovement.X);
                    player.Position.Y += player.velocity.Y;
                }
                else
                {
                    MoveScreen(player.velocity.X);
                    player.Position.Y += player.velocity.Y;
                }
            }
            else if (player.Position.X <= windowHalf && player.velocity.X < 0 && windowMovement.X > 0)
            {
                if (windowMovement.X < Math.Abs(player.velocity.X))
                {
                    MoveScreen(-windowMovement.X);
                    player.Position.Y += player.velocity.Y;
                }
                else
                {
                    MoveScreen(player.velocity.X);
                    player.Position.Y += player.velocity.Y;
                }
            }
            else
            {
                player.Position += player.velocity;
            }
        }

        private void MoveScreen(float movement)
        {
            /**
             * This is going to move the screen to keep the player in the middle.  If the player is going to move
             * past the middle of the screen we shall move the environment instead.  If we reach the end of the 
             * map the player needs to move around without moving the window.
             **/
            //moving the environment instead of the player
            foreach (Platform element in thePlatforms)
            {
                //it must move at the speed of the player
                element.Position.X -= movement;
            }

            //this stores how far we have moved the camera from the starting point
            windowMovement.X += movement;
        }

        private void CollisionDetection()
        {
            /**
             * This will determine how objects will interact with each other based the player's input.
             * It will compare the current frame with the next frame and will adjust the objects'
             * velocity to emulate the collision
             **/

            //These are the rectangles that contain the player and the environment
            Rectangle playerRect;
            Rectangle platformRect;

            //This is the player's position in the future
            Vector2 futurePlayerPosition = player.Position + player.velocity;

            //This is the rectangle containing the player this current frame
            playerRect = new Rectangle((int)futurePlayerPosition.X,
                (int)futurePlayerPosition.Y,
                player.Width,
                player.Height);

            //Here we will loop through all the of the platforms and determine if the player will collide with any of them next frame
            foreach(Platform element in thePlatforms)
            {
                //this is the rectangle containing the current platform in the array
                platformRect = new Rectangle((int)element.Position.X,
                    (int)element.Position.Y,
                    element.Width,
                    element.Height);

                //this will check if the playerRect and the current platformRect are intersecting
                if (playerRect.Intersects(platformRect))
                {
                    //the rectangle conatining the intersection between player and platform
                    Rectangle intersectRect = Rectangle.Intersect(playerRect, platformRect);                    
                    int platformBottom = platformRect.Y + platformRect.Height;

                    //check if the player is below the platform
                    if ((int)player.Position.Y >= platformBottom)
                    {
                        //the player's head hit the bottom of the platform so we stop upward movement and set them to falling
                        player.setState("falling");
                        player.Position = new Vector2(player.Position.X + player.velocity.X, platformBottom);
                        player.velocity = new Vector2(0, 0);
                    }
                    //check if the player is on the side of the platform
                    else if ((int)player.Position.Y < platformBottom && (int)player.getFeet() > platformRect.Y)
                    {
                        //we need to determine the current state of the player
                        if (player.getState() == "walking")
                        {
                            //We do not want to switch what platform the player is touching but update the state
                            player.setState("standing");
                        }
                        else if (player.getState() == "jumping")
                        {
                            //We do not want to affect the player's upward movement but which platform he's touching
                            player.setTouching(element);
                        }
                        else if (player.getState() == "falling")
                        {
                            //once the player begins to fall is when he'll be able to climb on the wall
                            player.setState("hitWall");
                            player.setTouching(element);
                        }

                        //the player must not pass through the platform
                        if (player.velocity.X > 0)
                        {
                            player.velocity.X -= intersectRect.Width;
                        }
                        else if (player.velocity.X < 0)
                        {
                            player.velocity.X += intersectRect.Width;
                        }                        
                    }
                    //check to see if the player is above the platform
                    else if ((int)player.getFeet() <= platformRect.Y)
                    {
                        player.setState("standing");
                        player.setTouching(element);
                        player.Position = new Vector2(player.Position.X + player.velocity.X, platformRect.Y - playerRect.Height);
                        player.velocity = new Vector2(0, 0);
                    }
                }
            }

            //checks to see if the player walks off any of the platforms
            if (player.currentlyTouching != dummy)
            {
                bool found = false;
                //if the platform has a cliff on the left side
                if (player.currentlyTouching.getCliffSide() == "left")
                {
                    //we only need to check to see if they will fall off that side of the platform
                    if (player.getRightSide() < player.currentlyTouching.Position.X)
                    {
                        player.setState("falling");
                        player.setTouching(dummy);
                    }
                    //if the player walks off the other side of the platform we need to figure out what platform the player is 
                    //walking onto
                    else if(player.Position.X > player.currentlyTouching.getRightSide())
                    {
                        foreach (Platform element in thePlatforms)
                        {
                            //if the current platform is the platform next to the one the player is currently touching
                            if (element.Position.X == player.currentlyTouching.getRightSide() && element.Position.Y == player.currentlyTouching.Position.Y && found == false)
                            {
                                player.setTouching(element);
                                found = true;
                            }
                        }
                    }

                }
                //if the platform has a cliff on the right side
                else if (player.currentlyTouching.getCliffSide() == "right")
                {
                    if (player.Position.X > player.currentlyTouching.getRightSide() && player.getState() != "standing")
                    {
                        player.setState("falling");
                        player.setTouching(dummy);
                    }
                    else if (player.getRightSide() < player.currentlyTouching.Position.X)
                    {
                        foreach (Platform element in thePlatforms)
                        {
                            if (element.getRightSide() == player.currentlyTouching.Position.X && element.Position.Y == player.currentlyTouching.Position.Y && found == false)
                            {
                                player.setTouching(element);
                                found = true;
                            }
                        }
                    }
                }
                //if the platform has cliffs on both sides
                else if (player.currentlyTouching.getCliffSide() == "both")
                {
                    //we need to check if the player walks off either sides of the platform
                    if (player.getRightSide() < player.currentlyTouching.Position.X || player.Position.X > player.currentlyTouching.getRightSide())
                    {
                        player.setState("falling");
                        player.setTouching(dummy);
                    }
                }
                //this platform has no cliffs and we just need to figure out what platform the player is walking onto
                else
                {
                    //check to see if the player walked off the left side of the platform
                    if (player.getRightSide() < player.currentlyTouching.Position.X)
                    {
                        foreach(Platform element in thePlatforms)
                        {
                            if (element.getRightSide() == player.currentlyTouching.Position.X && found == false)
                            {
                                player.setTouching(element);
                                found = true;
                            }
                        }
                    }
                    //check to see if the player walked off the right side of the platform
                    else if(player.Position.X > player.currentlyTouching.getRightSide())
                    {
                        foreach (Platform element in thePlatforms)
                        {
                            if (element.Position.X == player.currentlyTouching.getRightSide() && element.Position.Y == player.currentlyTouching.Position.Y && found == false)
                            {
                                player.setTouching(element);
                                found = true;
                            }
                        }
                    }
                }
            }
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            /**
             * This will update the player's state during the game.  The velocity of the player will be updated based on state,
             * but will not applied in this function
             **/
            // Get Thumbstick Controls
            player.Position.X += currentGamePadState.ThumbSticks.Left.X * player.moveSpeed;
            player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * player.moveSpeed;

            /*
             *Use the Keyboard / Dpad
             **/
            //if the player wants to move left
            if (currentKeyboardState.IsKeyDown(Keys.A) ||
                currentKeyboardState.IsKeyDown(Keys.Left) ||
            currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                //we need to determine what actions will happen based on the player's current state
                if (player.getState() == "standing" || player.getState() == "walking")
                {
                    //check to see if right arrow is not being held down
                    if (previousKeyboardState.IsKeyUp(Keys.D) && previousKeyboardState.IsKeyUp(Keys.Right))
                    {
                        //set player variables for walking left
                        player.setState("walking");
                        player.facingRight = false;
                        player.velocity.X = -player.moveSpeed;
                    }
                    else
                    {
                        //the player was holding down the other direction key and should be standing in that direction
                        player.setState("standing");
                        player.facingRight = true;
                        player.velocity.X = 0;
                    }
                }
                else if (player.getState() == "jumping" || player.getState() == "falling")
                {
                    if (previousKeyboardState.IsKeyUp(Keys.D) && previousKeyboardState.IsKeyUp(Keys.Right))
                    {
                        //the player can still move when they are mid jump/fall
                        player.facingRight = false;
                        player.velocity.X = -player.moveSpeed;
                    }
                    else
                    {
                        player.facingRight = true;
                        player.velocity.X = 0;
                    }
                }
                else if (player.getState() == "hitWall" || player.getState() == "climbing")
                {
                    //the player's position next frame
                    float playerFutureX = player.Position.X - player.moveSpeed;
                    float playerFutureRight = player.getRightSide() - player.moveSpeed;

                    //if the player is going to move into empty space they are considered falling again
                    if (playerFutureRight < player.currentlyTouching.Position.X || playerFutureX > player.currentlyTouching.getRightSide())
                    {
                        player.setState("falling");                        
                        player.facingRight = false;
                        player.velocity.X = -player.moveSpeed;
                        player.setTouching(dummy);
                    }
                    else if (player.Position.Y >= player.currentlyTouching.getBottom())
                    {
                        player.setState("falling");
                        player.setTouching(dummy);
                    }
                    //otherwise they should be climbing on the side of the wall
                    else
                    {
                        player.setState("climbing");
                        player.facingRight = true;
                    }
                }
            }
            
            //if the player wants to move right
            if (currentKeyboardState.IsKeyDown(Keys.D) ||
                currentKeyboardState.IsKeyDown(Keys.Right) ||
            currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                if (player.getState() == "standing" || player.getState() == "walking")
                {
                    //check to see if left is not being held down
                    if (previousKeyboardState.IsKeyUp(Keys.A) && previousKeyboardState.IsKeyUp(Keys.Left))
                    {
                        //set variables for walking right
                        player.setState("walking");
                        player.facingRight = true;
                        player.velocity.X = player.moveSpeed;
                    }
                    else
                    {
                        //the player is holding down left and should be standing and facing left
                        player.setState("standing");
                        player.facingRight = false;
                        player.velocity.X = 0;
                    }
                }
                else if (player.getState() == "jumping" || player.getState() == "falling")
                {
                    if (previousKeyboardState.IsKeyUp(Keys.A) && previousKeyboardState.IsKeyUp(Keys.Left))
                    {
                        //the player can still move when they are mid jump/fall
                        player.facingRight = true;
                        player.velocity.X = player.moveSpeed;
                    }
                    else
                    {
                        player.facingRight = false;
                        player.velocity.X = 0;
                    }
                }
                else if (player.getState() == "hitWall" || player.getState() == "climbing")
                {
                    //the player's position next frame
                    float playerFutureX = player.Position.X + player.moveSpeed;
                    float playerFutureRight = player.getRightSide() + player.moveSpeed;

                    //if the player is going to move into empty space they are considered falling again
                    if (playerFutureRight < player.currentlyTouching.Position.X || playerFutureX > player.currentlyTouching.getRightSide())
                    {
                        player.setState("falling");
                        player.facingRight = true;
                        player.velocity.X = player.moveSpeed;
                        player.setTouching(dummy);
                    }
                    else if (player.Position.Y > player.currentlyTouching.getBottom())
                    {
                        player.setState("falling");
                        player.facingRight = true;
                        player.setTouching(dummy);
                    }
                    //otherwise they should be climbing on the side of the wall
                    else
                    {
                        player.setState("climbing");
                        player.facingRight = false;
                    }
                }
            }
            
            //the player is pressing jump
            if (currentKeyboardState.IsKeyDown(Keys.Space) ||
                currentGamePadState.Buttons.A == ButtonState.Pressed)
            {
                if (previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    if (player.getState() == "standing" || player.getState() == "walking")
                    {
                        player.setState("jumping");                        
                        player.velocity.Y = -player.jumpSpeed;
                        player.setTouching(dummy);
                    }
                    else if (player.getState() == "climbing")
                    {
                        player.setState("wallJump");
                        float tempXSpeed = 17;
                        if (player.facingRight)
                        {
                            player.velocity = new Vector2(tempXSpeed, -player.jumpSpeed);
                        }
                        else
                        {
                            player.velocity = new Vector2(-tempXSpeed, -player.jumpSpeed);
                        }
                        player.setTouching(dummy);
                    }
                }
            }

            //if the player is not pressing left or right, the player is not moving and we should set it to standing
            if (currentKeyboardState.IsKeyUp(Keys.A) &&
            currentKeyboardState.IsKeyUp(Keys.D) && 
            currentKeyboardState.IsKeyUp(Keys.Left) &&
            currentKeyboardState.IsKeyUp(Keys.Right))
            {
                if (player.getState() == "walking")
                {
                    player.setState("standing");
                    player.velocity.X = 0;
                }
                else if (player.getState() == "jumping" || player.getState() == "falling")
                {
                    player.velocity.X = 0;
                }
                else if (player.getState() == "climbing")
                {
                    player.setState("falling");
                    player.setTouching(dummy);
                }
            }

            // Make sure that the player does not go out of bounds
            player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
            
            if (player.Position.Y > GraphicsDevice.Viewport.Height)
            {
                player.respawn();
            }
            //player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

            player.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
    }
}
