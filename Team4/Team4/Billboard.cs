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

    struct VertexPositionTexture : IVertexType
    {
        private Vector3 position;
        private Vector2 textureCoordinate;

        //private Single speed;
        //private Single fade;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

        public VertexPositionTexture(Vector3 position, Vector2 textureCoordinate)
        {
            this.position = position;

            this.textureCoordinate = textureCoordinate;

        }
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        public Vector3 Position
        {
            set { position = value; }
            get { return position; }
        }
        public Vector2 TextureCoordinate
        {
            set { textureCoordinate = value; }
            get { return textureCoordinate; }
        }
    }

    class Billboard
    {

        private float size;
        private static Game game;
        private string imageAsset;
        private Texture2D image;
        private static VertexBuffer vertexBuffer;
        private static IndexBuffer indexBuffer;
        private BoundingBox boundingBox;
        Effect effect;
        EffectParameter wvpMatEffect;
        EffectParameter textureEffect;
        EffectParameter alphaTest;
        EffectParameter alphaDirection;
        bool shouldRotate;
        bool shouldfollow;

        Vector3 pos;
        public Billboard(Game gm, float size, string imageAsset, bool shouldRotate, bool shouldfollow)
        {
            game = gm;
            this.size = size;
            this.imageAsset = imageAsset;
            this.shouldRotate = shouldRotate;
            this.shouldfollow = shouldfollow;
        }

        public Vector3 Position { set; get; }

        public void Initialize()
        {
            VertexPositionTexture[] corners;
            short[] indices;
            Vector2 uv = new Vector2(0.0f, 0.0f);
            pos = new Vector3(0.0f, 0.0f, 0.0f);
            corners = new VertexPositionTexture[4];
            indices = new short[4] { 0, 1, 2, 3 };
            // bottom left
            uv.X = 0.0f; uv.Y = 1.0f; pos.X = -size; pos.Y = -size; pos.Z = 0.0f;
            corners[0] = new VertexPositionTexture(pos, uv);
            // top left
            uv.X = 0.0f; uv.Y = 0.0f; pos.X = -size; pos.Y = size; pos.Z = 0.0f;
            corners[1] = new VertexPositionTexture(pos, uv);
            // bottm right
            uv.X = 1.0f; uv.Y = 1.0f; pos.X = size; pos.Y = -size; pos.Z = 0.0f;
            corners[2] = new VertexPositionTexture(pos, uv);
            // top right
            uv.X = 1.0f; uv.Y = 0.0f; pos.X = size; pos.Y = size; pos.Z = 0.0f;
            corners[3] = new VertexPositionTexture(pos, uv);
            vertexBuffer = new VertexBuffer(game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, corners.Length, BufferUsage.WriteOnly);
            indexBuffer = new IndexBuffer(game.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionTexture>(corners);
            indexBuffer.SetData<short>(indices);
            boundingBox = new BoundingBox(Position + corners[0].Position, Position + corners[3].Position);
        }

        public void LoadContent()
        {
            image = game.Content.Load<Texture2D>(imageAsset);
            effect = game.Content.Load<Effect>("Shaders/BillboardTexture");
            wvpMatEffect = effect.Parameters["WVPMatrix"];
            textureEffect = effect.Parameters["textureImage"];
            alphaTest = effect.Parameters["AlphaTest"];
            alphaDirection = effect.Parameters["AlphaTestDirection"];
        }

        public void Update(Vector3 position)
        {
            this.pos = position;
        }

        public void Draw(CameraClass.Camera cam, Game1 game)
        {
            DepthStencilState prevDepthState = game.GraphicsDevice.DepthStencilState;
            BlendState prevBlendState = game.GraphicsDevice.BlendState;
            RasterizerState prevRasterState = game.GraphicsDevice.RasterizerState;
            Matrix world;
            Matrix translation = Matrix.CreateTranslation(Position);

            if (shouldfollow)
            {
                Matrix billboard = Matrix.CreateFromAxisAngle(Vector3.Up, cam.GetViewerAngle());
                world = billboard * translation;
            }
            else if (shouldRotate)
            {
                Matrix billboard = Matrix.CreateFromAxisAngle(Vector3.Up, (float)Math.PI / 2);
                world = billboard * translation;
            }
            else
            {
                world = translation;
            }

            //define world matrix and set wvpmatrix in effect

            wvpMatEffect.SetValue(world * cam.ViewMatrix * Game1.ProjectionMatrix);
            //set the vertex and index buffers
            game.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            game.GraphicsDevice.Indices = indexBuffer;
            //draw opaque pixels first
            textureEffect.SetValue(image);
            alphaDirection.SetValue(1.0f);
            game.GraphicsDevice.BlendState = BlendState.Opaque;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {   
                pass.Apply();
               // if (game.CameraFrustum.Intersects(boundingBox))
                    game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 4, 0, 2);

            }
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            alphaDirection.SetValue(-1.0f);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                if (game.CameraFrustum.Intersects(boundingBox))
                    game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 4, 0, 2);
            }
            game.GraphicsDevice.DepthStencilState = prevDepthState;
            game.GraphicsDevice.BlendState = prevBlendState;
            game.GraphicsDevice.RasterizerState = prevRasterState;
            game.GraphicsDevice.SetVertexBuffer(null);
            game.GraphicsDevice.Indices = null;
            
        }

        public void SetGraphicsDeviceState()
        {
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        }
    }
}
