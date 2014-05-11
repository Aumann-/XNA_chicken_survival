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
using TerrainRuntime;

namespace Team4
{
    public class DrawableTerrain:DrawableGameComponent
    {
        private IndexBuffer indexBuffer;
        private VertexBuffer vertexBuffer;
        private Game game;
        private Terrain terrain;
        private Texture2D image;
        private string terrainAsset;
        private string imageAsset;
        private int rows, columns;
        private float worldWidth, cellWidth, height;

        public DrawableTerrain(Game game, string terrainAsset, string imageAsset)
            : base(game)
        {
            this.game = game;
           
            this.terrainAsset = terrainAsset;
            this.imageAsset = imageAsset;
        }

        public override void Initialize()
        {

            terrain = game.Content.Load<Terrain>(terrainAsset);
            rows = terrain.ROWS;
            columns = terrain.COLS;
            worldWidth = terrain.worldWidth;
            cellWidth = terrain.cellWidth;
            InitializeGrid();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            image = game.Content.Load<Texture2D>(imageAsset);
            
            base.LoadContent();
        }

        public void InitializeGrid()
        {
            
            VertexPositionNormalTexture[] grid = new VertexPositionNormalTexture[rows * columns];
            int k = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    grid[k].Position = terrain.position[j + i * columns];
                    grid[k].TextureCoordinate = new Vector2((float)j * 10.0f / (float)(columns - 1), (float)i * 10.0f / (float)(rows - 1));
                    grid[k].Normal = terrain.normal[j + i * columns];
                    k++;
                }
            }

            short[] indices = new short[2 * columns];
            k = 0;

            for (short j = 0; j < columns; j++)
            {
                indices[k++] = j;
                indices[k++] = (short)(columns + j);
            }

            indexBuffer = new IndexBuffer(game.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);

            vertexBuffer = new VertexBuffer(game.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, grid.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(grid);
            indexBuffer.SetData<short>(indices);
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix world = Matrix.Identity;
            BasicEffect effect = new BasicEffect(game.GraphicsDevice);
            effect.World = world;
            effect.View = Game1.ViewMatrix;
            effect.Projection = Game1.ProjectionMatrix;
            effect.Texture = image;
            effect.TextureEnabled = true;
            effect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            effect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            effect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.SpecularPower = 5.0f;
            effect.Alpha = 1.0f;
            effect.FogEnabled = true;
            effect.FogColor = Color.Black.ToVector3(); // For best results, ake this color whatever your background is.
            effect.FogStart = 15f;
            effect.FogEnd = 200f;

            RasterizerState rasterState = new RasterizerState();
            rasterState.CullMode = CullMode.None;

            game.GraphicsDevice.RasterizerState = rasterState;
            game.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            game.GraphicsDevice.Indices = indexBuffer;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                for (int Z = 0; Z < rows - 1; Z++)
                {
                    game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, Z * columns, 0, 2 * columns, 0, 2 * (columns - 1));
                }
            }

            base.Draw(gameTime);
        }


        public float getHeight(Vector3 location, Vector3 direction)
        {
            Vector3[] points = new Vector3[4];
            points[0] = location;

            for (int i = 1; i < 3; i++)
                points[i] = points[i - 1] + direction;

            for (int i = 0; i < 3; i++)
            {
                int col = (int)((points[i].X + worldWidth) / cellWidth);
                int row = (int)((points[i].Z + worldWidth) / cellWidth);

                if (col < 0)
                    col = 0;
                if (row < 0)
                    row = 0;
                if (col > columns - 1)
                    col = columns - 1;
                if (row > rows - 1)
                    row = rows - 1;

                points[i].Y = terrain.position[col + row * columns].Y;
            }

            height = MathHelper.CatmullRom(points[0].Y, points[1].Y, points[2].Y, points[3].Y, .125f);
            return height;
        }


    }
}
