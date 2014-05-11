using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using CameraClass;
using BoundaryClass;


namespace Team4
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private SpriteFont arial; //font arial - size 16
        private SpriteFont andy;  //font arial - bold- size 20
        string scoreString = "Score:";
        double scoreGame = 00;
        int delta = 1;
        Rectangle textbox;
        int textPosition = 500;
        

        float Speed = 2.0f; //speed of colonel sanders at start
       
        
        private static Camera camera1; //this is the main walking camera for the character
        private static Camera camera2; //this is the cinematic camera for when the game starts
        private static Matrix projectionMatrix;

        private static BoundingFrustum cameraFrustum;

        //mouse
        MouseState mousestate;
        MouseState lastmousestate;

        //audioengine
        AudioEngine audioengine;
        WaveBank wavebank;
        SoundBank soundbank;
        Cue chickencluck;
        Cue chickendance;
        Cue footsteps;
        //Cue ambience;
        Cue heartbeat1;
        Cue heartbeat2;
        Cue heartbeat3;
        Cue heartbeat4;
        Cue scarynoise;
        Cue jumpscare;

        bool shouldplayJumpScare;


        //Billboards
        Billboard ColSanders;
        Billboard[] fence;

        //flag to only add one egg instead of 60
        private bool addedEgg = false;

        // Boundary
        private Boundary boundary = new Boundary();

        private Model chicken;
        private Vector3 chickenPos; //position of the chicken model
        private float chickHeight;
        private bool done = false; //varaible for if cinematic camera is done
        private bool start = false; //flag for if player has started game
        private bool end = false; //flag for if player has lost
        private bool firstScare = true;

        private Model[] tree; //array to hold the tree models
        private int numTrees = 100; //amount of desired tree

        private Model roasted; //model for roasted chicken (death model)

        Random r;
        float[] x; //array for tree x positions
        float[] z; //array for tree z positions
        float temp; //used as a tempory holder when generating tree locations

        int eggsRemaining; //number of eggs remaining

        private DrawableTerrain terrain;
        enum CameraView { CAMERA1, CAMERA2 }; //use to set active camera
        private static CameraView activeCamera; //current active camera for scene


        public Matrix ProjMatrix
        {
            get { return projectionMatrix; }
        }    

        public DrawableTerrain Terrain
        {
            get { return terrain; }
        }

        public Camera Camera { get { return camera1; } }

        public BoundingFrustum CameraFrustum { get { return cameraFrustum; } }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            eggsRemaining = 1;
            this.graphics.IsFullScreen = false; //start game in window mode
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Window.Title = "Colonel Slender: Chicken Chaser";
            //initialize the textbox - where the text will be placed into
            textbox = new Rectangle(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2, 300, 300);

            done = false;
            end = false;
            start = false;
            Speed = 2.0f;
            eggsRemaining = 1;
            shouldplayJumpScare = false;
            firstScare = true;
          //  finalScore = (int)scoreGame;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi / 4.0f,
            (float)Window.ClientBounds.Width / (float)Window.ClientBounds.Height, 0.005f, 200.0f);
            //Create View matrix for camera1
            camera1 = new Camera(Camera.CameraType.WALK, this);
            camera1.SetViewMatrix(new Vector3(0.0f, 7.1f, 0.0f), new Vector3(0.0f, 7.1f, -1.0f),
            new Vector3(0.0f, 1.0f, 0.0f));
            
            //initalize the terrain object
            terrain = new DrawableTerrain(this, "Images/ter", "Images/ground");
            //add the terrain object so it will be drawn
            //this.Components.Add(terrain);
            terrain.Initialize();

            //position of the chicken model
            //must be recalculated if terrain is changed
            chickenPos = new Vector3(0.0f, 3.1f, 0.0f);
            chickHeight = terrain.getHeight(chickenPos, camera1.Look);
            chickenPos.Y = chickHeight + 3.1f;

            //create view matrix for camera2
            camera2 = new Camera(Camera.CameraType.WALK, this);
            camera2.SetViewMatrix(new Vector3(0.0f, 15.0f, 18.0f), chickenPos,
            new Vector3(0.0f, 1.0f, 0.0f));

            //start game with camera2(cinematic)
            activeCamera = CameraView.CAMERA2;

            tree = new Model[numTrees]; //array to hold each tree model
            x = new float[numTrees]; //array to hold the x position of each tree
            z = new float[numTrees]; //array to hold the z position of each tree

            r = new Random();
            
            //Generate a random position for each tree
            for (int i = 0; i < numTrees; i++)
            {
                temp = (float)r.NextDouble() * 1500 - 750;
                //check if there is already a tree at that x coordinate
                while (x.Contains(temp) || x.Contains((temp - 100) * -1))
                {
                    temp = (float)r.NextDouble() * 1500 - 750;
                }
                x[i] = temp;


                temp = (float)r.NextDouble() * 1500 - 750;
                //check if there is already a tree at that z coordinate
                while (z.Contains(temp) || z.Contains((temp - 100) * -1))
                {
                    temp = (float)r.NextDouble() * 1500 - 750;
                }
                z[i] = temp;

            }

            //update camera frustum
            cameraFrustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);
            //////////
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //load the fonts
            arial = Content.Load<SpriteFont>("arial");
            andy = Content.Load<SpriteFont>("andy");

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //load the chicken model
            chicken = Content.Load<Model>("Models/xchicken2");

            //load the tree models
            for (int i = 0; i < numTrees; i++)
                tree[i] = Content.Load<Model>("Models/testTree");

            //load the death model
            roasted = Content.Load<Model>("Models/roasted");

            //load audio engine
            audioengine = new AudioEngine("Content/Music/finalproject.xgs");
            wavebank = new WaveBank(audioengine, "Content/Music/Wave Bank.xwb");
            soundbank = new SoundBank(audioengine, "Content/Music/Sound Bank.xsb");
            chickendance = soundbank.GetCue("chickendance");
            chickencluck = soundbank.GetCue("cluck2");
            footsteps = soundbank.GetCue("walking");
            //ambience = soundbank.GetCue("ambience");
            heartbeat1 = soundbank.GetCue("heartbeat1");
            heartbeat2 = soundbank.GetCue("heartbeat2");
            heartbeat3 = soundbank.GetCue("heartbeat3");
            heartbeat4 = soundbank.GetCue("heartbeat4");
            scarynoise = soundbank.GetCue("scarynoise");
            jumpscare = soundbank.GetCue("jumpscare");

            //load Colonel Sanders
            ColSanders = new Billboard(this, 10.0f, "Images/colsanders", false, true);
            ColSanders.Initialize();
            ColSanders.LoadContent();
            ResetColonelPos(camera1.Position);

            fence = new Billboard[300];

            //add fences around egg of play area
            for (int i = 0; i < fence.Length; i++)
            {
                int fenceStart = -750;
                int offset = 10;

                if (i < 75)
                {
                    fence[i] = new Billboard(this, 10.0f, "Images/chicken-wire", true, false);
                    fence[i].Position = new Vector3(-750.0f, 10.0f, fenceStart + (i * 20.0f) + offset);
                }
                else if (i >= 75 && i < 150)
                {
                    fence[i] = new Billboard(this, 10.0f, "Images/chicken-wire", true, false);
                    fence[i].Position = new Vector3(750.0f, 10.0f, fenceStart + ((i - 75) * 20.0f) + offset);
                }
                else if (i >= 150 && i < 225)
                {
                    fence[i] = new Billboard(this, 10.0f, "Images/chicken-wire", false, false);
                    fence[i].Position = new Vector3(fenceStart + ((i - 150) * 20.0f) + offset, 10.0f, -750);
                }
                else
                {
                    fence[i] = new Billboard(this, 10.0f, "Images/chicken-wire", false, false);
                    fence[i].Position = new Vector3(fenceStart + ((i - 225) * 20.0f) + offset, 10.0f, 750);
                }
                fence[i].Initialize();
                fence[i].LoadContent();
            }
            
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
        /// method to add one egg to player total
        /// </summary>
        public void addEgg()
        {
            eggsRemaining++;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            // TODO: Add your update logic here
            KeyboardState keyState = Keyboard.GetState();

            //start playing intro music
            //if (!chickendance.IsPlaying)
            //{
            //    chickendance.Play();
            //}

            //exit if escape is pressed
            if (keyState.IsKeyDown(Keys.Escape))
                this.Exit();
            //press space to start game
            if (keyState.IsKeyDown(Keys.Space))
                 start = true;
            //toggle between fullscreen and windowed mode
            if (keyState.IsKeyDown(Keys.F10))
                this.graphics.ToggleFullScreen();

            ////end game immediately (for debugging purposes)
            // if (keyState.IsKeyDown(Keys.R))
            //{
            //    end = true;
            //    camera2.SetViewMatrix(new Vector3(0.0f, 10.0f, 18.0f), new Vector3(0.0f, 3.0f, 0.0f),
            //        new Vector3(0.0f, 1.0f, 0.0f));
            //    activeCamera = CameraView.CAMERA2;
                
            ////  camera2.UpdateEnd(gameTime);
            //}
            

             
            //check which function key is pressed to switch camera
            //this will need to be removed before release
            //if (keyState.IsKeyDown(Keys.F1))
                //activeCamera = CameraView.CAMERA1;
            //if (keyState.IsKeyDown(Keys.F2))
                 //activeCamera = CameraView.CAMERA2;

            //update the active camera
            if (activeCamera == CameraView.CAMERA1)
            {
                camera1.Update(gameTime);
            }

            //change height of camera to adjust for terrain
            camera1.Position = new Vector3(camera1.Position.X, terrain.getHeight(camera1.Position, Vector3.Up) + 10.0f, camera1.Position.Z);

            //update camera2 and check if it has finished
            if (activeCamera == CameraView.CAMERA2 && !end)
            {
                //ambience.Stop(AudioStopOptions.AsAuthored);

                if (!chickendance.IsPlaying)
                {
                    chickendance = soundbank.GetCue("chickendance");
                    chickendance.Play();
                }

                done = camera2.UpdateStart(gameTime, chickenPos, start);
                
            }

            if (end)
            {
                //ambience.Stop(AudioStopOptions.AsAuthored);
                shouldplayJumpScare = false;

                //if (!chickendance.IsPlaying)
                //{
                //    chickendance = soundbank.GetCue("chickendance");
                //    chickendance.Play();
                //}

                camera2.UpdateEnd(gameTime);
            }

            //update camera frustum
            cameraFrustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);

            //if player is looking at sanders, reduce player score
            if (cameraFrustum.Intersects(boundary.box) )
            {
                if (!end)
                {
                    scoreGame -= 0.6;
                    Speed += 0.005f;
                }

                if (!jumpscare.IsPlaying && shouldplayJumpScare && firstScare)
                {
                    jumpscare = soundbank.GetCue("jumpscare");
                    jumpscare.Play();
                    shouldplayJumpScare = false;
                    firstScare = false;
                }
            }

            else
            {
                if (start) //don't let sanders move until player has started game
                {
                    if (firstScare)
                        shouldplayJumpScare = true;
                    ColSanders.Position += CalcColonelVector(camera1.Position, ColSanders.Position);
                    ColSanders.Position = new Vector3(ColSanders.Position.X, terrain.getHeight(ColSanders.Position, Vector3.Forward) + 10.0f, ColSanders.Position.Z);
                }
            }  
                Vector3 min = new Vector3(ColSanders.Position.X - 2.0f, ColSanders.Position.Y - 10.0f, ColSanders.Position.Z - 1.0f);
                Vector3 max = new Vector3(ColSanders.Position.X + 2.0f, ColSanders.Position.Y + 10.0f, ColSanders.Position.Z + 1.0f);
                boundary.makeBox(min, max);
            

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
               

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //if cinematic camera is done, switch to camera1 (1st person camera)
            if (done && !end)
            {
                chickendance.Stop(AudioStopOptions.AsAuthored);

                /*if (!ambience.IsPlaying)
                {
                    ambience = soundbank.GetCue("ambience");
                    ambience.Play();
                }*/

                HandleHeartBeat(ColSanders.Position, camera1.Position);

                if (!footsteps.IsPlaying && (keyState.IsKeyDown(Keys.W) || keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.D)))
                {
                    footsteps = soundbank.GetCue("walking");
                    footsteps.Play();
                }


                if (!(keyState.IsKeyDown(Keys.W) || keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.D)))
                    footsteps.Stop(AudioStopOptions.AsAuthored);

                activeCamera = CameraView.CAMERA1;
                scoreGame += .2;
                chickendance.Stop(AudioStopOptions.AsAuthored);
            }

            //if Sanders caught (touched) player, game ends
            if (isColliding(ColSanders.Position, camera1.Position))
            {
                footsteps.Stop(AudioStopOptions.AsAuthored);
                end = true;
                camera2.SetViewMatrix(new Vector3(0.0f, 10.0f, 18.0f), new Vector3(0.0f, 3.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
                activeCamera = CameraView.CAMERA2;
                ColSanders.Position = new Vector3(0.0f, 100.0f, 0.0f);
            }
            
            //reset game if player presses space after losing
            if (end)
            {
                if (!chickendance.IsPlaying)
                {
                    chickendance = soundbank.GetCue("chickendance");
                    chickendance.Play();
                }

                scarynoise.Stop(AudioStopOptions.AsAuthored);
                heartbeat1.Stop(AudioStopOptions.AsAuthored);
                heartbeat2.Stop(AudioStopOptions.AsAuthored);
                heartbeat3.Stop(AudioStopOptions.AsAuthored);
                heartbeat4.Stop(AudioStopOptions.AsAuthored);

                if (keyState.IsKeyDown(Keys.Space))
                {
                    this.Initialize();
                    scoreGame = 00;
                    textPosition = 500;
                    end = false;
                    ResetColonelPos(camera1.Position);
                    
                }
            }
           
            
            //handling shooting
            mousestate = Mouse.GetState();

            if (mousestate.LeftButton == ButtonState.Pressed && lastmousestate.LeftButton == ButtonState.Released && eggsRemaining > 0)
            {
                eggsRemaining--;
                camera1.shoot();
                chickencluck = soundbank.GetCue("cluck2");
                chickencluck.Play();
                if(boundary.checkTreeCollision() == true )
                {

                }
            }

            lastmousestate = mousestate;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            //code to add eggs at certains intervals of time (currently every 30 seconds)
            //uses flag to only add 1 egg
            if ((gameTime.TotalGameTime.Seconds == 29 || gameTime.TotalGameTime.Seconds == 59) 
                && !addedEgg && !end && start)
            {
                addEgg();
                scoreGame += 500;
                addedEgg = true;
            }

            //reset the flag for adding an egg
            if ((gameTime.TotalGameTime.Seconds == 0 || gameTime.TotalGameTime.Seconds == 30) && addedEgg)
            {
                addedEgg = false;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //draw all the tree models
            for (int i = 0; i < numTrees; i++)
            {   
                float height = terrain.getHeight(new Vector3(x[i], 0.0f, z[i]), Vector3.Forward);
                height += 20.0f;

                DrawModel(tree[i], new Vector3((float)x[i], height, (float)z[i]), 10.0f, i);
            }

            //draw terrain (used instead of components to control draw order
            terrain.Draw(gameTime);
            // TODO: Add your drawing code here
            
            //draw all fences around area
            foreach (Billboard bill in fence)
            {
                bill.Draw(camera1, this);
            }

            //only draw chicken model if cinematic camera has not finished
            if (!done)
            {
                DrawModel(chicken, chickenPos, 1.0f, 0);
                
                if (!start)
               {//instructions to screen
                    DrawMenu(delta);
                    ResetGraphics();
               }

            }

            //draw death model once game has ended
            if (end)
            {
                chickenPos.Y = chickHeight + 0.5f; //make roasted model not float above ground
                DrawModel(roasted, chickenPos, 1.0f, 0);
                //draw the final score to screen
               if(textPosition >= 200)
                    textPosition -= 2;

                DrawEndText(delta);
                DrawCredits(delta, textPosition);
                ResetGraphics();
            }
           
            
            //start drawing Sanders once game starts
            if (done && !end)
            {
                ColSanders.Draw(camera1, this);
                //draw the score to the screen
                DrawString2(delta, gameTime);
                ResetGraphics();
            }
            //spriteBatch.Begin();
            //spriteBatch.DrawString(arial, "Position: X: " + (int)camera1.Position.X + " Y: " + (int)camera1.Position.Y + " Z: " + (int)camera1.Position.Z, new Vector2(0, 200), Color.White);
            //spriteBatch.End();
            ResetGraphics();

           
            base.Draw(gameTime);
        }

        /// <summary>
        /// Method to draw a model
        /// </summary>
        /// <param name="model">this is the model object to be drawn</param>
        /// <param name="trans">this is the position of the model</param>
        /// <param name="color">this is a flag to determine if diffuse color should be used</param>
        void DrawModel(Model model, Vector3 trans, float scale, int count)
        {
            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.GraphicsDevice.BlendState = BlendState.Opaque;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)

                {
                    //rotate puts model right-side up, translate places model at desired location
                    effect.World =  Matrix.CreateScale(scale) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(trans);
                    effect.Projection = projectionMatrix;
                    effect.View = ViewMatrix;
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
                    effect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
                    effect.SpecularPower = 5.0f;
                    effect.Alpha = 1.0f;
                    effect.FogEnabled = true;
                    effect.FogColor = Color.Black.ToVector3();
                    effect.FogStart = 15f;
                    effect.FogEnd = 200f;
                   
                }
                boundary.maketreeSphere(count, model, trans, scale);
                
                mesh.Draw();
            }
        }
        



        //property to return the viewMatrix of the active camera
        public static Matrix ViewMatrix
        {
            get
            {
                if (activeCamera == CameraView.CAMERA1)
                    return camera1.ViewMatrix;
                else
                    return camera2.ViewMatrix;
            }
        }

        //property to return the Projection Matrix
        public static Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
        }
   
        public void DrawString2( int delta,GameTime gameTime)
        {  
            scoreString = "Score: ";
            string strEgg = " Eggs: "+ eggsRemaining.ToString();
            string stringOut = String.Format("{0}{1}", scoreString, (int)scoreGame);
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.DrawString(arial, stringOut, new Vector2(20.0f, 40.0f + delta), Color.BlanchedAlmond);
            spriteBatch.DrawString(arial, strEgg, new Vector2(20.0f, 60.0f), Color.BlanchedAlmond);
            spriteBatch.End();
        }
        public void DrawCredits(int delta, int textPosition)
        {

            string textCredits = "Team4\nGame Credits";
            string creditList = "Angelica Avila \nBlake Bennet \nAaron Christie \nTaylor Postoak \nMathew Schrader \nRoy Smith";
            string specialThanks = "\n\nSpecial Thanks To:\nalecfara\nsneakyVertex";
            // string stringEndOut = String.Format("{0}{1}",text, finalScore);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.DrawString(andy, textCredits, new Vector2(textbox.X-390, textPosition-60), Color.Azure);
            spriteBatch.DrawString(arial, creditList, new Vector2(textbox.X-370, textPosition), Color.BlanchedAlmond);
            spriteBatch.DrawString(arial, specialThanks, new Vector2(textPosition + 150, textbox.Y + 80), Color.AliceBlue);
            spriteBatch.End();

        }
        public void DrawEndText( int delta)
        {
            //int score=(int)scoreGame;
          
            string text = "Game Over";
            string finalScore = "Final Score: " + scoreGame.ToString("n0");
          // string stringEndOut = String.Format("{0}{1}",text, finalScore);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.DrawString(andy, text, new Vector2(textbox.X, textbox.Y), Color.BlanchedAlmond);
            spriteBatch.DrawString(andy, finalScore,  new Vector2(textbox.X, textbox.Y+40), Color.BlanchedAlmond);
            spriteBatch.End();
        }

        public void DrawMenu(int delta)
        {
            //middle alignment
            string menu = "How to Play:";
            string comments = "      Colonel Sanders is out to make you into his original recipe!\nShoot him with your eggs, the longer you stay alive the higher your score. \n                                        GOOD LUCK!";
            string submenu = "Press spacebar to start";
            //right alignment
            string movement = " A : LEFT \nW : FORWARD \n D : RIGHT\n S : BACKWARDS";
            string press = "PRESS THE KEYS TO MOVE" + "\n" + movement;
            //left alignment
            string shoot = "TO SHOOT:\n Use the left mouse button";
            string toLook = shoot + "\n\nTO LOOK AROUND: \nMove the mouse";

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            //right of the screen    
            spriteBatch.DrawString(arial, press, new Vector2(textbox.X + 110, textbox.Y - 90), Color.BlanchedAlmond);
            //left of the screen
            spriteBatch.DrawString(arial, toLook, new Vector2(textbox.X - 390, textbox.Y - 90), Color.BlanchedAlmond);

            //middle of the screen
            spriteBatch.DrawString(andy, menu, new Vector2(textbox.X - 150, textbox.Y - 190), Color.White);
            spriteBatch.DrawString(arial, comments, new Vector2(textbox.X - 340, textbox.Y + 80), Color.BlanchedAlmond);
            spriteBatch.DrawString(andy, submenu, new Vector2(textbox.X - 150, textbox.Y + 170), Color.Black);

            spriteBatch.End();
        }
        public void ResetGraphics()
        {
            
            //Create RasterizerState object and set graphics device RasterizerState
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //Set alpha blending to opaque
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //Creaete a SamplerState object and reset texturing of graphics device
            SamplerState samplerState = new SamplerState();
            samplerState.AddressU = TextureAddressMode.Wrap;
            samplerState.AddressV = TextureAddressMode.Wrap;
            GraphicsDevice.SamplerStates[0] = samplerState;
        }

        public Vector3 CalcColonelVector(Vector3 camPos, Vector3 ColPos)
        {
            Vector3 veloc = ColPos - camPos;
            veloc.Normalize();
            return -(veloc / 5) * Speed;

        }

        public void ResetColonelPos(Vector3 camPos)
        {
            Random rand = new Random();
            float x = rand.Next(-750, 750);
            float y = terrain.getHeight(ColSanders.Position, Vector3.Forward) + 10.0f;
            float z = rand.Next(-750, 750);

            Vector3 newPos = new Vector3(x, y, z);

            Vector3 distance = newPos - camPos;

            float mag = distance.Length();

            if (mag < 50.0f)
                ResetColonelPos(camPos);

            Speed *= 1.5f;

            this.ColSanders.Position = newPos;
        } //ResetColonelPos

        public bool isColliding(Vector3 colPos, Vector3 camPos)
        {
            Vector3 distance = colPos - camPos;
            float mag = distance.Length();

            if (mag < 2.0f)
                return true;
            else
                return false;
        } //isColliding


        public void HandleHeartBeat(Vector3 ColPos, Vector3 CamPos)
        {
            Vector3 distance = ColPos - CamPos;
            float mag = distance.Length();

            if (mag > 1000.0f)
            {
                heartbeat3.Stop(AudioStopOptions.AsAuthored);
                heartbeat4.Stop(AudioStopOptions.AsAuthored);

                scarynoise.Stop(AudioStopOptions.AsAuthored);

                if (!heartbeat2.IsPlaying)
                {
                    heartbeat2 = soundbank.GetCue("heartbeat2");
                    heartbeat2.Play();
                }
            }
            else if (mag < 1000.0f && mag > 500.0f)
            {
                heartbeat2.Stop(AudioStopOptions.AsAuthored);
                heartbeat4.Stop(AudioStopOptions.AsAuthored);

                if (!heartbeat3.IsPlaying)
                {
                    heartbeat3 = soundbank.GetCue("heartbeat3");
                    heartbeat3.Play();
                }

                if (!scarynoise.IsPlaying)
                {
                    scarynoise = soundbank.GetCue("scarynoise");
                    scarynoise.Play();
                }
            }
            else
            {
                heartbeat2.Stop(AudioStopOptions.AsAuthored);
                heartbeat3.Stop(AudioStopOptions.AsAuthored);

                if (!heartbeat4.IsPlaying)
                {
                    heartbeat4 = soundbank.GetCue("heartbeat4");
                    heartbeat4.Play();
                }

                if (!scarynoise.IsPlaying)
                {
                    scarynoise = soundbank.GetCue("scarynoise");
                    scarynoise.Play();
                }
            }
        }// HandleHeartBeat 
        
        
    }
}
