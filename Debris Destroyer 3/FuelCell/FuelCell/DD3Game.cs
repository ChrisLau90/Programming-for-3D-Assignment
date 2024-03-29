// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

/// <summary>
/// This is the main class of the game and is mostly modified from the FuelCellGame.
/// REFERENCES:
/// [0] FuelCell game code http://msdn.microsoft.com/en-us/library/dd940288.aspx
/// date accessed 18th April 2012
/// [1] How to pause: http://msdn.microsoft.com/en-us/library/bb195026(v=xnagamestudio.31).aspx
/// date accessed: 22nd April 2012
/// </summary>
namespace FuelCell
{
    public enum GameState {Start, Instruction, Controls, Running, Won, Lost }
    
    /// <summary>
    /// This is the main class of the game and is mostly modified from the FuelCellGame.
    /// Reference: http://msdn.microsoft.com/en-us/library/dd940288.aspx
    /// date accessed: 22nd April 2012
    /// </summary>
    public class DD3Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        KeyboardState lastKeyboardState = new KeyboardState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        GamePadState currentGamePadState = new GamePadState();

        MouseState mouseState;

        int maxFuelAmount;
        int fuelRemaining; //<<<<<<<<<<<<<<<<<<<
        int score;
        int level;

        Random random;
        SpriteBatch spriteBatch;
        SpriteFont statsFont;
        GameState currentGameState = GameState.Start;

        GameObject ground;
        Camera gameCamera;

        GameObject boundingSphere;

        Ship fuelCarrier;
        FuelCell[] fuelCells;
        Debris[] barriers;
        List<Bullet> bullets; //<<<<<<<<<<<<<<<<<<<<

        Texture2D startScreen;
        Texture2D instructionScreen;
        Texture2D controlsScreen;

        Texture2D healthbar1;
        Texture2D healthbar2;

        Texture2D crosshair;

        int bulletTimer;

        SoundEffect laserSound;
        SoundEffect laserSound2;
        SoundEffect refuel;
        SoundEffect explosion;

        private bool paused = false;            //[1]
        private bool pauseKeyDown = false;      //[1]
        private bool pausedForGuide = false;    //[1]

        public DD3Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 853;
            graphics.PreferredBackBufferHeight = 480;

            Content.RootDirectory = "Content";

            //roundTime = GameConstants.RoundTime;
            random = new Random();

            maxFuelAmount = 2500;
            fuelRemaining = maxFuelAmount;
            level = 1;
            score = 0;
            
            bulletTimer = 10;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            ground = new GameObject();
            gameCamera = new Camera();
            boundingSphere = new GameObject();
            bullets = new List<Bullet>();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            startScreen = Content.Load<Texture2D>("Images/Startup");
            instructionScreen = Content.Load<Texture2D>("Images/Instructions");
            controlsScreen = Content.Load<Texture2D>("Images/Controls");

            healthbar1 = Content.Load<Texture2D>("Images/healthbar");
            healthbar2 = Content.Load<Texture2D>("Images/healthbar2");

            crosshair = Content.Load<Texture2D>("Images/crosshair");

            ground.Model = Content.Load<Model>("Models/ground");
            boundingSphere.Model = Content.Load<Model>("Models/sphere1uR");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            statsFont = Content.Load<SpriteFont>("Fonts/StatsFont");

            //Initialize fuel cells
            fuelCells = new FuelCell[GameConstants.NumFuelCells];
            for (int index = 0; index < fuelCells.Length; index++)
            {
                fuelCells[index] = new FuelCell();
                fuelCells[index].LoadContent(Content, "Models/fuelcell");
            }

            //Initialize barriers
            barriers = new Debris[GameConstants.NumBarriers];
            int randomBarrier = random.Next(3);
            string barrierName = null;

            for (int index = 0; index < barriers.Length; index++)
            {

                switch (randomBarrier)
                {
                    case 0:
                        barrierName = "Models/cube10uR";
                        break;
                    case 1:
                        barrierName = "Models/cylinder10uR";
                        break;
                    case 2:
                        barrierName = "Models/pyramid10uR";
                        break;
                }
                barriers[index] = new Debris();
                barriers[index].LoadContent(Content, barrierName);
                randomBarrier = random.Next(3);
            }
            PlaceFuelCellsAndDebris();

            //Initialize fuel carrier
            fuelCarrier = new Ship();
            fuelCarrier.LoadContent(Content, "Models/fuelcarrier");

            //int min = GameConstants.MinDistance;
            //int max = GameConstants.MaxDistance;
            
            //Load sound effects
            laserSound = Content.Load<SoundEffect>("Sounds/laserSound");
            laserSound2 = Content.Load<SoundEffect>("Sounds/laserSound2");
            refuel = Content.Load<SoundEffect>("Sounds/powerUp");
            explosion = Content.Load<SoundEffect>("Sounds/explosion");
        }

        private void PlaceFuelCellsAndDebris()
        {
            int min = GameConstants.MinDistance;
            int max = GameConstants.MaxDistance;
            Vector3 tempCenter;

            //place fuel cells
            foreach (FuelCell cell in fuelCells)
            {
                cell.Position = GenerateRandomPosition(min, max);
                tempCenter = cell.BoundingSphere.Center;
                tempCenter.X = cell.Position.X;
                tempCenter.Y = 0;
                tempCenter.Z = cell.Position.Z;
                cell.BoundingSphere = 
                    new BoundingSphere(tempCenter, cell.BoundingSphere.Radius);
                cell.Retrieved = false;
            }

            //place barriers
            foreach (Debris barrier in barriers)
            {
                barrier.Position = GenerateRandomPosition2(min, max);
                tempCenter = barrier.BoundingSphere.Center;
                tempCenter.X = barrier.Position.X;
                tempCenter.Y = barrier.Position.Y + 2;
                tempCenter.Z = barrier.Position.Z;
                barrier.BoundingSphere = new BoundingSphere(tempCenter, 
                    barrier.BoundingSphere.Radius);
            }
        }



        private Vector3 GenerateRandomPosition(int min, int max)
        {
            int xValue, zValue;
            do
            {
                xValue = random.Next(min, max);
                zValue = random.Next(min, max);
                if (random.Next(100) % 2 == 0)
                    xValue *= -1;
                if (random.Next(100) % 2 == 0)
                    zValue *= -1;

            } while (IsOccupied(xValue, zValue));

            return new Vector3(xValue, 0, zValue);
        }

        private Vector3 GenerateRandomPosition2(int min, int max)
        {
            int xValue, yValue, zValue;
            do
            {
                xValue = random.Next(min, max);
                yValue = random.Next(0, 150);
                zValue = random.Next(min, max);
                if (random.Next(100) % 2 == 0)
                    xValue *= -1;
                if (random.Next(100) % 2 == 0)
                    zValue *= -1;

            } while (IsOccupied(xValue, zValue));

            return new Vector3(xValue, yValue, zValue);
        }

        private bool IsOccupied(int xValue, int zValue)
        {
            foreach (GameObject currentObj in fuelCells)
            {
                if (((int)(MathHelper.Distance(xValue, currentObj.Position.X)) < 15) &&
                    ((int)(MathHelper.Distance(zValue, currentObj.Position.Z)) < 15))
                    return true;
            }

            foreach (GameObject currentObj in barriers)
            {
                if (((int)(MathHelper.Distance(xValue, currentObj.Position.X)) < 15) &&
                    ((int)(MathHelper.Distance(zValue, currentObj.Position.Z)) < 15))
                    return true;
            }
            return false;
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
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            lastGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            
            mouseState = Mouse.GetState();//<<<<<<<<<<


            // Allows the game to exit
            if ((currentKeyboardState.IsKeyDown(Keys.Escape)) || 
                (currentGamePadState.Buttons.Back == ButtonState.Pressed))
                this.Exit();
            if (currentGameState == GameState.Start)
            {
                if ((lastKeyboardState.IsKeyUp(Keys.Enter) && 
                    (currentKeyboardState.IsKeyDown(Keys.Enter)))||
                    currentGamePadState.Buttons.Start == ButtonState.Pressed)
                {
                    laserSound2.Play();
                    currentGameState = GameState.Instruction;
                }
                lastKeyboardState = currentKeyboardState;
            }
            if (currentGameState == GameState.Instruction)
            {
                if ((lastKeyboardState.IsKeyUp(Keys.Enter) &&
                    (currentKeyboardState.IsKeyDown(Keys.Enter))) ||
                    currentGamePadState.Buttons.Start == ButtonState.Pressed)
                {
                    laserSound2.Play();
                    currentGameState = GameState.Controls;
                }
                lastKeyboardState = currentKeyboardState;
            }
            if (currentGameState == GameState.Controls)
            {
                if ((lastKeyboardState.IsKeyUp(Keys.Enter) &&
                    (currentKeyboardState.IsKeyDown(Keys.Enter))) ||
                    currentGamePadState.Buttons.Start == ButtonState.Pressed)
                {
                    laserSound2.Play();
                    currentGameState = GameState.Running;
                }
                lastKeyboardState = currentKeyboardState;
            }

            if ((currentGameState == GameState.Running))
            {
                checkPauseKey(currentKeyboardState); //[i]
                if (!paused)
                {
                    fuelCarrier.Update(currentGamePadState,
                        currentKeyboardState, barriers);

                    gameCamera.Update(fuelCarrier.ForwardDirection, fuelCarrier.AimDirection,
                        fuelCarrier.Position, aspectRatio);
                    //UPDATED CAMERA.UPDATE PARAMETER


                    if (fuelCarrier.CheckForFuelCollision(fuelCells))
                    {
                        refuel.Play();
                        if ((fuelRemaining + 500) > maxFuelAmount)
                        {
                            fuelRemaining = 2500;
                        }
                        else
                        {
                            fuelRemaining += 500;
                        }
                    }

                    //DECREASE THE FUEL
                    fuelRemaining -= level;

                    if (fuelRemaining > 0 && isAllDestroyed())
                    {
                        currentGameState = GameState.Won;
                    }
                    else if ((fuelRemaining <= 0) &&
                        (!isAllDestroyed()))
                    {
                        currentGameState = GameState.Lost;
                    }

                    if (bulletTimer != 0)
                    {
                        bulletTimer--;
                    }
                    if (bulletTimer == 0)
                    {
                        shoot();
                        bulletTimer = 10;
                    }

                    if (bullets.Count != 0)
                    {
                        for (int i = 0; i < bullets.Count; i++)
                        {
                            bullets[i].Update(barriers);

                            if (bullets[i].CheckForCollision(bullets[i].BoundingSphere, barriers))
                            {
                                explosion.Play();
                                score += 50;
                            }
                            if (bullets[i].timer == 25)
                            {
                                bullets.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }

            if (currentGameState == GameState.Won)
            {
                // Reset the world for a new game
                if ((lastKeyboardState.IsKeyDown(Keys.Enter) &&
                    (currentKeyboardState.IsKeyUp(Keys.Enter))))
                {
                    ResetGame(gameTime, aspectRatio);
                    level++;
                }
            }
            else if (currentGameState == GameState.Lost)
            {
                // Reset the world for a new game
                if ((lastKeyboardState.IsKeyDown(Keys.Enter) &&
                    (currentKeyboardState.IsKeyUp(Keys.Enter))))
                {
                    ResetGame(gameTime, aspectRatio);
                    level = 1;
                    score = 0;
                }
            }

            base.Update(gameTime);
        }

        private void shoot()
        {
            //SHOOT
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Bullet bullet = new Bullet(Content, fuelCarrier);
                bullets.Add(bullet);
                laserSound.Play();
            }
        }

        private void ResetGame(GameTime gameTime, float aspectRatio)
        {
            fuelCarrier.Reset();
            bullets = new List<Bullet>();
            gameCamera.Update(fuelCarrier.ForwardDirection, fuelCarrier.AimDirection,
                fuelCarrier.Position, aspectRatio);
            InitializeGameField();

            fuelRemaining = maxFuelAmount;

            currentGameState = GameState.Running;

        }

        private void InitializeGameField()
        {
            //Initialize barriers
            barriers = new Debris[GameConstants.NumBarriers];
            int randomBarrier = random.Next(3);
            string barrierName = null;

            for (int index = 0; index < GameConstants.NumBarriers; index++)
            {
                switch (randomBarrier)
                {
                    case 0:
                        barrierName = "Models/cube10uR";
                        break;
                    case 1:
                        barrierName = "Models/cylinder10uR";
                        break;
                    case 2:
                        barrierName = "Models/pyramid10uR";
                        break;
                }
                barriers[index] = new Debris();
                barriers[index].LoadContent(Content, barrierName);
                randomBarrier = random.Next(3);
            }
            PlaceFuelCellsAndDebris();
        }

        /// <summary>
        /// This is called when the game should f itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            switch (currentGameState)
            {
                case GameState.Start:
                    DrawStartScreen();
                    break;
                case GameState.Instruction:
                    DrawInstructionScreen();
                    break;
                case GameState.Controls:
                    DrawControlsScreen();
                    break;
                case GameState.Running:
                    DrawGameplayScreen();
                    break;
                case GameState.Won:
                    DrawWinScreen();
                    break;
                case GameState.Lost:
                    DrawLossScreen();
                    break;
            };

            base.Draw(gameTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model">Model representing the game playing field.</param>
        private void DrawTerrain(Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = Matrix.Identity;

                    // Use the matrices provided by the game camera
                    effect.View = gameCamera.ViewMatrix;
                    effect.Projection = gameCamera.ProjectionMatrix;
                }
                mesh.Draw();
            }
        }

        private void DrawStartScreen()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(startScreen, new Rectangle(0, 0, 853, 480), Color.White);
            spriteBatch.End();
        }

        private void DrawInstructionScreen()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(instructionScreen, new Rectangle(0, 0, 853, 480), Color.White);
            spriteBatch.End();
        }

        private void DrawControlsScreen()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(controlsScreen, new Rectangle(0, 0, 853, 480), Color.White);
            spriteBatch.End();
        }

        private void DrawWinScreen()
        {
            string gameResult = "Level " + level.ToString() + " complete!";

            string strScore = score.ToString();
            string wages = "Today's wages so far: $";
            wages += strScore;
            
            float xOffsetText, yOffsetText;
            Vector2 viewportSize = new Vector2(GraphicsDevice.Viewport.Width, 
                GraphicsDevice.Viewport.Height);
            Vector2 strCenter;

            xOffsetText = yOffsetText = 0;
            Vector2 strResult = statsFont.MeasureString(gameResult);
            Vector2 strPlayAgainSize = 
                statsFont.MeasureString(GameConstants.StrContinue);
            Vector2 strPosition;
            strCenter = new Vector2(strResult.X / 2, strResult.Y / 2);

            Vector2 strScoreSize = statsFont.MeasureString(wages);

            yOffsetText = (viewportSize.Y / 2 - strCenter.Y);
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);

            spriteBatch.Begin();
            spriteBatch.DrawString(statsFont, gameResult, 
                strPosition, Color.Green);

            strCenter = new Vector2(strScoreSize.X / 2, strScoreSize.Y / 2);
            yOffsetText = (viewportSize.Y / 2 - strCenter.Y) +
                (float)statsFont.LineSpacing;
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);
            spriteBatch.DrawString(statsFont, wages,
                strPosition, Color.AntiqueWhite);

            strCenter = 
                new Vector2(strPlayAgainSize.X / 2, strPlayAgainSize.Y / 2);
            yOffsetText = (viewportSize.Y / 2 - (strCenter.Y -23)) + 
                (float)statsFont.LineSpacing;
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);
            spriteBatch.DrawString(statsFont, GameConstants.StrContinue, 
                strPosition, Color.AntiqueWhite);

            spriteBatch.End();

            //re-enable depth buffer after sprite batch disablement
            
            //GraphicsDevice.DepthStencilState.DepthBufferEnable = true;
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;
            
            //GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //GraphicsDevice.RenderState.AlphaTestEnable = false;
            
            //GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }

        private void DrawLossScreen()
        {
            string gameResult = "Level " + level.ToString() + " failed!";

            string strScore = score.ToString();
            string wages = "Today's total wages: $";
            wages += strScore;
            
            float xOffsetText, yOffsetText;

            Vector2 viewportSize = new Vector2(GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            Vector2 strCenter;

            xOffsetText = yOffsetText = 0;
            Vector2 strResult = statsFont.MeasureString(gameResult);

            Vector2 strScoreSize = statsFont.MeasureString(wages);

            Vector2 strPlayAgainSize =
                statsFont.MeasureString(GameConstants.StrPlayAgain);
            
            Vector2 strPosition;
            strCenter = new Vector2(strResult.X / 2, strResult.Y / 2);

            yOffsetText = (viewportSize.Y / 2 - strCenter.Y);
            xOffsetText = (viewportSize.X / 2 - strCenter.X);

            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);

            spriteBatch.Begin();
            spriteBatch.DrawString(statsFont, gameResult,
                strPosition, Color.Red);

            strCenter = new Vector2(strScoreSize.X / 2, strScoreSize.Y / 2);
            yOffsetText = (viewportSize.Y / 2 - strCenter.Y) +
                (float)statsFont.LineSpacing;
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);
            spriteBatch.DrawString(statsFont, wages,
                strPosition, Color.AntiqueWhite);

            strCenter =
                new Vector2(strPlayAgainSize.X / 2, strPlayAgainSize.Y / 2);
            yOffsetText = (viewportSize.Y / 2 - (strCenter.Y - 23) +
                (float)statsFont.LineSpacing);
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);
            spriteBatch.DrawString(statsFont, GameConstants.StrPlayAgain,
                strPosition, Color.AntiqueWhite);

            spriteBatch.End();

            //re-enable depth buffer after sprite batch disablement

            //GraphicsDevice.DepthStencilState.DepthBufferEnable = true;
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;

            //GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //GraphicsDevice.RenderState.AlphaTestEnable = false;

            //GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }

        private void DrawGameplayScreen()
        {
            //spriteBatch.Begin();
            //background.Draw(spriteBatch);
            //spriteBatch.End();

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            DrawTerrain(ground.Model);
            foreach (FuelCell fuelCell in fuelCells)
            {
                if (!fuelCell.Retrieved)
                {
                    fuelCell.Draw(gameCamera.ViewMatrix, 
                        gameCamera.ProjectionMatrix);
                }
            }
            foreach (Debris barrier in barriers)
            {
                if (!barrier.Destroyed)
                {
                    barrier.Draw(gameCamera.ViewMatrix,
                        gameCamera.ProjectionMatrix);
                    /*
                    RasterizerState rs = new RasterizerState();
                    rs.FillMode = FillMode.WireFrame;
                    GraphicsDevice.RasterizerState = rs;
                    barrier.DrawBoundingSphere(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix, boundingSphere);
                    rs = new RasterizerState();
                    rs.FillMode = FillMode.Solid;
                    GraphicsDevice.RasterizerState = rs;
                    */
                 }
            }
            foreach (Bullet bullet in bullets)
            {
                bullet.Draw(gameCamera.ViewMatrix,
                    gameCamera.ProjectionMatrix);
                /*
                RasterizerState rs = new RasterizerState();
                rs.FillMode = FillMode.WireFrame;
                GraphicsDevice.RasterizerState = rs;
                bullet.DrawBoundingSphere(gameCamera.ViewMatrix, gameCamera.ProjectionMatrix, boundingSphere);
                rs = new RasterizerState();
                rs.FillMode = FillMode.Solid;
                GraphicsDevice.RasterizerState = rs;
                */
            }

            fuelCarrier.Draw(gameCamera.ViewMatrix, 
                gameCamera.ProjectionMatrix);

            DrawStats();
            
        }

        private void DrawStats()
        {
            float xOffsetText, yOffsetText;
            string str1 = "Fuel Remaining: ";
            string str2 = "Pay: $";

            str2 += score.ToString();

            Rectangle rectSafeArea;
                
            //Calculate str1 position
            rectSafeArea = GraphicsDevice.Viewport.TitleSafeArea;

            xOffsetText = rectSafeArea.X;
            yOffsetText = rectSafeArea.Y;

            Vector2 strSize = statsFont.MeasureString(str1);
            Vector2 strPosition = 
                new Vector2((int)xOffsetText + 13, (int)yOffsetText + 15);

            spriteBatch.Begin();
            spriteBatch.DrawString(statsFont, str1, strPosition, Color.White);
            strPosition.X += 668;
            spriteBatch.DrawString(statsFont, str2, strPosition, Color.White);
            spriteBatch.Draw(healthbar1, new Rectangle(161, 10, 500, 30), Color.White);
            spriteBatch.Draw(healthbar2, new Rectangle(165, 13, fuelRemaining/5, 24), Color.White);
            spriteBatch.Draw(crosshair, new Rectangle(408, 240, 40, 40), Color.White);
            spriteBatch.End();

            //re-enable depth buffer after sprite batch disablement
            
            //GraphicsDevice.DepthStencilState.DepthBufferEnable = true;
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;
            
            //GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //GraphicsDevice.RenderState.AlphaTestEnable = false;
            
            //GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            //GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;s
        }

        /// <summary>
        /// This next three methods are used for the pausing function.
        /// Check reference [1] at the bottom of the page
        /// date accessed: 22nd April 2012
        /// </summary>

        private void BeginPause(bool UserInitiated)
        {
            paused = true;
            pausedForGuide = !UserInitiated;
            //TODO: Pause audio playback
            //TODO: Pause controller vibration
        }


        /// <summary>
        /// Check reference [1]
        /// date accessed: 22nd April 2012
        /// </summary>
        private void EndPause()
        {
            //TODO: Resume audio
            //TODO: Resume controller vibration
            pausedForGuide = false;
            paused = false;
        }

        private void checkPauseKey(KeyboardState keyboardState)
        {
            bool pauseKeyDownThisFrame = (keyboardState.IsKeyDown(Keys.R));
            // If key was not down before, but is down now, we toggle the
            // pause setting
            if (!pauseKeyDown && pauseKeyDownThisFrame)
            {
                if (!paused)
                    BeginPause(true);
                else
                    EndPause();
            }
            pauseKeyDown = pauseKeyDownThisFrame;
        }

        private bool isAllDestroyed()
        {
            foreach (Debris barrier in barriers)
            {
                if (!barrier.Destroyed)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
/// <summary>
/// This is the main class of the game and is mostly modified from the FuelCellGame.
/// REFERENCES:
/// [0] FuelCell game code http://msdn.microsoft.com/en-us/library/dd940288.aspx
/// date accessed 18th April 2012
/// [1] How to pause: http://msdn.microsoft.com/en-us/library/bb195026(v=xnagamestudio.31).aspx
/// date accessed: 22nd April 2012
/// </summary>

