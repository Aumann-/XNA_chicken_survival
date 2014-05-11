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
using BoundaryClass;

namespace Team4
{
    class Egg : DrawableGameComponent
    {
        protected Model eggModel;
        protected Game1 game;
        protected Effect shader;
        protected bool projectile;
        protected Vector3 pos, vel, acc, up, fwd;
        protected Matrix shrink;
        protected float rotation;
        float eggRadius= 0.5f;

        // Boundary
        private Boundary boundary = new Boundary();
        int egg = 1;

        public Egg(Game1 game)
            : base(game)
        {

        }

        public Egg(Game1 game, Vector3 pos, Vector3 dir) 
            :base(game)
        {
            this.game = game;
            this.pos = pos;
            this.vel = new Vector3(dir.X, dir.Y+.03f, dir.Z) * 3;
            this.acc = Vector3.Down /50;
            this.fwd = Vector3.Down;
            this.up = Vector3.Forward;
            projectile = true;
            rotation = 0;
            shrink = Matrix.CreateScale(.25f);
            Initialize();
        }


        public override void Initialize()
        {
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            eggModel = game.Content.Load<Model>("Models/egg");
            shader = game.Content.Load<Effect>("Shaders/eggShader");

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            ///////////////////////////////////////////////////////////////////////////////////
            boundary.makeSphere(egg,pos,eggRadius);  
            ////////////////////////////////////////////////////////////////////////////////
            if (projectile)
            {
                vel += acc;
                pos = pos + vel / 4;
                rotation += MathHelper.Pi /(float) gameTime.ElapsedGameTime.TotalMilliseconds;
            }


            if (boundary.checkTreeCollision() || game.Terrain.getHeight(pos, vel) > pos.Y || boundary.BillboardCollision() )
            {// here for now

                 if (boundary.BillboardCollision())
                {
                    game.ResetColonelPos(game.Camera.Position);
                }

                if (!(this is EggGib))
                {
                    DestroyEgg();
                }
            }
            if (this is EggGib && boundary.checkTreeCollision())
            {
                vel = new Vector3(-vel.X, vel.Y, -vel.Z);
                pos += vel;
                vel /= 2;
                return;
            }

            if (this is EggGib && game.Terrain.getHeight(pos, vel) > pos.Y)
            {
                game.Components.Remove(this);
                return;
            }
           
            base.Update(gameTime);
        }

        
        public override void Draw(GameTime gameTime)
        {

            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.GraphicsDevice.BlendState = BlendState.Opaque;
            Matrix[] transforms = new Matrix[eggModel.Bones.Count];

            Matrix World = Matrix.CreateWorld(pos, fwd, up);
            eggModel.CopyAbsoluteBoneTransformsTo(transforms);

            shader.Parameters["lightPos"].SetValue(new Vector4(pos+Vector3.Normalize(Vector3.One),1));
            shader.Parameters["View"].SetValue(game.Camera.ViewMatrix);
            shader.Parameters["Projection"].SetValue(game.ProjMatrix);
 
            foreach (ModelMesh mesh in eggModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = shader;
                }

                foreach (Effect eff in mesh.Effects)
                {
                    shader.Parameters["World"].SetValue(shrink * Matrix.CreateRotationX(rotation) * Matrix.CreateTranslation(pos));
                }
            }

            foreach (ModelMesh mesh in eggModel.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    mesh.Draw();
                }
            }

            this.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            base.Update(gameTime);
        }

        public void DestroyEgg()
        {
            //game.addEgg();
            game.Components.Remove(this);
            
            for (int i = 0; i < 75; i++)
            {
                game.Components.Add(new EggGib(game, pos, vel));
            }
        }
    }

    class EggGib : Egg
    {
        private static Random r = new Random();


        public EggGib(Game1 game, Vector3 pos, Vector3 vel)
            : base(game)
        {
            this.game = game;
            this.pos = pos;
            this.vel = new Vector3(vel.X + (float)r.NextDouble() - .5f, (float)r.NextDouble() + .5f, vel.Z + (float)r.NextDouble() - .5f);
            if (this.pos.Y < game.Terrain.getHeight(this.pos, this.vel))
            {
                this.pos.Y = game.Terrain.getHeight(this.pos, this.vel) + .01f;
            }
            this.acc = Vector3.Down / 50;
            this.fwd = Vector3.Down;
            this.up = Vector3.Forward;
            projectile = true;
            rotation = 0;
            Initialize();
            this.shrink = Matrix.CreateScale(.1f);
        }
    }
}
