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
using Team4;
using BoundaryClass;

namespace CameraClass
{
    public class Camera
    {
        public enum CameraType
        {
            FLY,
            WALK,
            CHASE
        }

        //class data members
        private Vector3 position;
        private Vector3 up;
        private Vector3 right;
        private Vector3 look;
        private Vector3 target;
        protected CameraType cameraType;
        private Matrix viewMatrix;
        private static int count = 0;
        private bool reset = false;

        float speed = 5;


        //boundary
        private int cam = 0;
        private float PlayerRadius = 0.3f;
        private Boundary boundary = new Boundary();
        /// <summary>
        /// ////////////
        /// </summary>

        private bool atY = false, atZ = false;

        private Game1 game;

        //constructor
        public Camera(Game1 game)
        {
            cameraType = CameraType.WALK;
            InitializeVectors();
            this.game = game;
        }

        //second constructor
        public Camera(CameraType CT, Game1 game)
        {
            cameraType = CT;
            InitializeVectors();
            this.game = game;
        }

        //initialize for vectors
        private void InitializeVectors()
        {
            position = new Vector3();
            target = new Vector3();
            up = new Vector3();
            look = new Vector3();
            right = new Vector3();
        }

        public void SetViewMatrix(Vector3 pos, Vector3 tar, Vector3 u) 
        { 
             position = pos; 
             target = tar; 
             up = u; 
             //Determine unit vectors look and right and normalize up 
             look = target - position; 
             if (look.Length() != 1.0f) 
             look.Normalize(); 
             if (up.Length() != 1.0f) 
             up.Normalize(); 
             right = Vector3.Cross(look, up); 
             if (right.Length() != 1.0f)
                 right.Normalize();
             //Create viewMatrix 
             boundary.makeSphere(cam, position, PlayerRadius);
             viewMatrix = Matrix.CreateLookAt(position, target, up);
        }

        public void Move(float units)
        {
            if (cameraType == CameraType.WALK)
                position += new Vector3(look.X, 0.0f, look.Z) * units;
            if (cameraType == CameraType.FLY)
                position += look * units;
            target = position + look;

            SetViewMatrix(position, target, up);
        }

        public void Strafe(float units)
        {
            if (cameraType == CameraType.WALK)
                position += new Vector3(right.X, 0.0f, right.Z) * units;
            if (cameraType == CameraType.FLY)
                position += right * units;
            target = position + look;
            SetViewMatrix(position, target, up);
        }


        public void Lift(float units)
        {
            position += up * units;
            target = position + look;
            SetViewMatrix(position, target, up);
        }

        public void Pitch(float angle)
        {
            Matrix T = Matrix.CreateFromAxisAngle(right, angle);
            if (cameraType == CameraType.FLY)
                up = Vector3.Transform(up, T);
            look = Vector3.Transform(look, T);
            target = position + look;
            SetViewMatrix(position, target, up);
        }

        public void Yaw(float angle)
        {
            Matrix T;
            if (cameraType == CameraType.WALK)
                T = Matrix.CreateRotationY(angle);
            else
                T = Matrix.CreateFromAxisAngle(up, angle);
            right = Vector3.Transform(right, T);
            look = Vector3.Transform(look, T);
            target = position + look;
            SetViewMatrix(position, target, up);
        }

        public void Roll(float angle)
        {
            if (cameraType == CameraType.FLY)
            {
                Matrix T = Matrix.CreateFromAxisAngle(look, angle);
                right = Vector3.Transform(right, T);
                right.Normalize();
                up = Vector3.Transform(up, T);
                up.Normalize();
                SetViewMatrix(position, target, up);
            }
        }

        public Vector3 Position
        {
            set { position = value; }
            get { return position; }
        }

        public Vector3 Look
        {
            get { return look; }
        }

        public Matrix ViewMatrix
        {
            get { return this.viewMatrix; }
        }

        

        public CameraType CamType
        {
            set { cameraType = value; }
            get { return cameraType; }
        }

        public virtual void Update(GameTime gameTime)
        {
           

            KeyboardState kbs = Keyboard.GetState();
            Vector3 fwd;
            MouseState mss = Mouse.GetState();
            Vector2 mousePos = new Vector2(game.Window.ClientBounds.Width / 2, game.Window.ClientBounds.Height / 2);


            if (game.IsActive)
            {
                Mouse.SetPosition((int)mousePos.X, (int)mousePos.Y);

                float dX = mss.X - mousePos.X;
                float dY = mss.Y - mousePos.Y;


                Quaternion rX = Quaternion.CreateFromAxisAngle(Vector3.Up, -dX * gameTime.ElapsedGameTime.Milliseconds / 5000);
                Quaternion rY = Quaternion.CreateFromAxisAngle(Vector3.Cross(look, Vector3.Up), -dY * gameTime.ElapsedGameTime.Milliseconds / 5000);


                Vector3 prevLook = look;

                look = Vector3.Transform(look, rY * rX);

                if (look.Y > .99)
                {
                    look = prevLook;
                }

                if (look.Y < -.99)
                {
                    look = prevLook;
                }
                look.Normalize();
                fwd = new Vector3(look.X, 0, look.Z);
                fwd.Normalize();

                //if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                    //speed = 50;
                //else
                    speed = 5;
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (kbs.IsKeyDown(Keys.W))
                {
                    position += speed * fwd / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    boundary.makeSphere(cam, position, PlayerRadius);
                    if (checkIfOutOfBounds() || boundary.checkTreeCollision())
                        position -= speed * fwd / (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                }
                if (kbs.IsKeyDown(Keys.A))
                {
                    position -= speed * Vector3.Normalize(Vector3.Cross(fwd, Vector3.Up)) / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    boundary.makeSphere(cam, position, PlayerRadius);
                    if (checkIfOutOfBounds() || boundary.checkTreeCollision())
                        position += speed * Vector3.Normalize(Vector3.Cross(fwd, Vector3.Up)) / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
                if (kbs.IsKeyDown(Keys.S))
                {
                    position -= speed * fwd / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    boundary.makeSphere(cam, position, PlayerRadius);
                    if (checkIfOutOfBounds() || boundary.checkTreeCollision())
                        position += speed * fwd / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
                if (kbs.IsKeyDown(Keys.D))
                {
                    position += speed * Vector3.Normalize(Vector3.Cross(fwd, Vector3.Up)) / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    boundary.makeSphere(cam, position, PlayerRadius);

                    if (checkIfOutOfBounds() || boundary.checkTreeCollision())
                        position -= speed * Vector3.Normalize(Vector3.Cross(fwd, Vector3.Up)) / (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            boundary.makeSphere(cam, position, PlayerRadius);
            viewMatrix = Matrix.CreateLookAt(position, position + look, Vector3.Up);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        /// <summary>
        /// new update method for panning the camera around the model and then zooming into it.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="pos">this is the position of the model</param>
        public virtual bool Update(GameTime gameTime, Vector3 pos)
        {
            int milliseconds = gameTime.ElapsedGameTime.Milliseconds;
            float unit = (float)milliseconds * .008f;
            float angle = (float)milliseconds * .001f;

            //spins the camera around the model
            if (count < 788)
            {
                Yaw(angle / 2);
                Strafe(unit);
            }
            else
            {
                //zooms the camera into the model
                if (position.Y >= pos.Y + 3)
                {
                    Lift(-unit / 4);
                }
                else
                    atY = true;

                if (position.Z >= pos.Z - 3)
                {
                    Move(unit * 1.4f);
                }
                else
                    atZ = true;
                
                
            }
            count++;
            if (atY && atZ)
                return true;

            return false;
        }

        /// <summary>
        /// Method to enable mouse panning for camera1
        /// </summary>
        /// <param name="mouseState"></param>
        /// <param name="aspect"></param>
        public void Pan(MouseState mouseState, Vector2 aspect)
        {
            if (cameraType == CameraType.WALK)
            {
                Quaternion quaternion, quatConj, qLook;
                float angleX, angleY;
                float middleX = aspect.X / 2.0f;
                float middleY = aspect.Y / 2.0f;
                float deltaX = mouseState.X - middleX;
                float deltaY = mouseState.Y - middleY;
                if (deltaX == 0 && deltaY == 0)
                    return;
                angleX = 0.0f;
                angleY = 0.0f;
                if (mouseState.X != middleX)
                    angleX = -deltaX / aspect.X;
                if (mouseState.Y != middleY)
                    angleY = deltaY / aspect.Y;
                if (angleX == 0 || angleY == 0)
                    return;
                Mouse.SetPosition((int)middleX, (int)middleY);
                //rotate look about right by angleY 
                float sin = (float)Math.Sin(angleY / 2.0f);
                float cos = (float)Math.Cos(angleY / 2.0f);
                quaternion = new Quaternion(sin * right, cos);
                quatConj = quaternion;
                quatConj.Conjugate(); //create conjugate of quaternion 
                qLook = new Quaternion(look, 0); //create a quaternion of the look vector 
                qLook = quaternion * qLook * quatConj;
                look = new Vector3(qLook.X, qLook.Y, qLook.Z);
                target = position + look;
                SetViewMatrix(position, target, up);
                //rotate look bout up by angleX 
                sin = (float)Math.Sin(angleX / 2.0f);
                cos = (float)Math.Cos(angleX / 2.0f);
                quaternion = new Quaternion(sin * up, cos);
                quatConj = quaternion;
                quatConj.Conjugate();
                qLook = new Quaternion(look, 0);
                qLook = quaternion * qLook * quatConj;
                look = new Vector3(qLook.X, qLook.Y, qLook.Z);
                target = position + look;
                boundary.makeSphere(cam, position, PlayerRadius);
                SetViewMatrix(position, target, up);
            }
        }
    
        public void shoot()
        {
            
            game.Components.Add(new Egg(game, position, Vector3.Normalize(look)));

        }

        public Vector3 Up
        {
            get { return this.up; }
            set { this.up = value; }
        } //Up

        public float GetViewerAngle()
        {
            // use camera look direction to get
            // rotation angle about Y
            float x = -Look.X;
            float z = -Look.Z;
            return (float)Math.Atan2(x, z);
        }

        public bool checkIfOutOfBounds()
        {
            if (Math.Abs(Position.X) > 750 || Math.Abs(Position.Z) > 750)
                return true;
            else
                return false;
        }

        public virtual void UpdateEnd(GameTime gameTime)
        {
            int milliseconds = gameTime.ElapsedGameTime.Milliseconds;
            float unit = (float)milliseconds * .008f;
            float angle = (float)milliseconds * .001f;

            Yaw(angle / 2);
            Strafe(unit);
        }

        public virtual bool UpdateStart(GameTime gameTime, Vector3 pos, bool start)
        {
            int milliseconds = gameTime.ElapsedGameTime.Milliseconds;
            float unit = (float)milliseconds * .008f;
            float angle = (float)milliseconds * .001f;

            //spins the camera around the model
            if (!start)
            {
                Yaw(angle / 2);
                Strafe(unit);
            }
            else
            {
                if (!reset)
                {
                    SetViewMatrix(new Vector3(0.0f, 15.0f, 18.0f), new Vector3(0.0f, 3.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
                    reset = true;
                }
                //zooms the camera into the model
                if (position.Y >= pos.Y + 3)
                    Lift(-unit / 4);
                else
                    atY = true;

                if (position.Z >= pos.Z - 3)
                    Move(unit * 1.4f);
                else
                    atZ = true;


            }
            if (atY && atZ)
            {
                reset = false;
                return true;
            }

            return false;
        }



    }





    /// <summary>
    /// class definition for chase camera
    /// </summary>
    //public class ChaseCamera : Camera
    //{
    //    Vector2 adjust;
    //    Vector3 offset;
    //    Matrix world;
    //    //public ChaseCamera(Vector2 adjust, Matrix world)
    //    //{
    //    //    this.adjust = adjust;
    //    //    this.world = world;
    //    //    cameraType = CameraType.CHASE;
    //    //    offset = world.Translation - adjust.X * world.Forward + adjust.Y * world.Up;
    //    //    SetViewMatrix(offset, world.Translation, world.Up);
    //    //}
    //    public override void Update(GameTime gameTime)
    //    {
    //        KeyboardState keyState = Keyboard.GetState();
    //        float unit = (float)gameTime.ElapsedGameTime.Milliseconds * .005f;

    //        if (keyState.IsKeyDown(Keys.Up))
    //            adjust.Y += unit;
    //        if (keyState.IsKeyDown(Keys.Down))
    //            adjust.Y -= unit;
    //        if (keyState.IsKeyDown(Keys.Right))
    //            adjust.X += unit;
    //        if (keyState.IsKeyDown(Keys.Left))
    //            adjust.X -= unit;
    //        offset = world.Translation - adjust.X * world.Forward + adjust.Y * world.Up;
    //        SetViewMatrix(offset, world.Translation, world.Up);
    //    }
    //    public Matrix World
    //    {
    //        set { world = value; }
    //    }
    //    public Vector2 Adjust
    //    {
    //        set { adjust = value; }
    //    }

    //} 
}
