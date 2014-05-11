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

namespace Camera
{
    public class Camera
    {
        public enum CameraType
        {
            FLY,
            WALK
        }

        //class data members
        private Vector3 position;
        private Vector3 up;
        private Vector3 right;
        private Vector3 look;
        private Vector3 target;
        private CameraType cameraType;
        private Matrix viewMatrix;


        //constructor
        public Camera()
        {
            cameraType = CameraType.WALK;
            InitializeVectors();
        }

        //second constructor
        public Camera(CameraType CT)
        {
            cameraType = CT;
            InitializeVectors();
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
        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
        }

        public CameraType CamType
        {
            set { cameraType = value; }
            get { return cameraType; }
        }

        public virtual void Update(GameTime gameTime)
        {
            int milliseconds = gameTime.ElapsedGameTime.Milliseconds;
            float unit = (float)milliseconds * .008f;
            float angle = (float)milliseconds * .001f;
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            if (keyState.IsKeyDown(Keys.LeftAlt))
                Roll(angle);
            if (keyState.IsKeyDown(Keys.LeftControl))
                Roll(-angle);
            if (keyState.IsKeyDown(Keys.Left))
                Strafe(-unit);
            if (keyState.IsKeyDown(Keys.Right))
                Strafe(unit);
            if (keyState.IsKeyDown(Keys.Up) || mouseState.LeftButton == ButtonState.Pressed)
                Move(unit);
            if (keyState.IsKeyDown(Keys.Down) || mouseState.RightButton == ButtonState.Pressed)
                Move(-unit);
            if (keyState.IsKeyDown(Keys.A))
                Yaw(angle);
            if (keyState.IsKeyDown(Keys.D))
                Yaw(-angle);
            if (keyState.IsKeyDown(Keys.W))
                Pitch(angle);
            if (keyState.IsKeyDown(Keys.S))
                Pitch(-angle);
            if (keyState.IsKeyDown(Keys.PageUp))
                Lift(unit);
            if (keyState.IsKeyDown(Keys.PageDown))
                Lift(-unit);
        } 




    }
}
