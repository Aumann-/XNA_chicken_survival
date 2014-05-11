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

namespace TerrainRuntime
{
    public class Terrain
    {
        public byte[] height;
        public Vector3[] position;
        public Vector3[] normal;
        public int ROWS, COLS;
        public float worldWidth, worldHeight, heightScale;
        public float cellWidth, cellHeight;
        
        public float GetY(int index)
        {
            return position[index].Y;
        }

        public float WorldWidth
        {
            get { return WorldWidth; }
        }

        public float CellWidth
        {
            get { return cellWidth; }
        }

        internal Terrain(ContentReader cr)
        {
            ROWS = cr.ReadInt32();
            COLS = cr.ReadInt32();
            worldWidth = cr.ReadSingle();
            worldHeight = cr.ReadSingle();


            cellWidth = cr.ReadSingle();
            cellHeight = cr.ReadSingle();

            heightScale = 0.1f;

            //declare position and normal vector arrays
            position = new Vector3[ROWS * COLS];
            normal = new Vector3[ROWS * COLS];

            //read in position and normal data to generate height map
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    position[col + row * COLS] = cr.ReadVector3();
                    position[col + row * COLS].Y *= heightScale;
                    normal[col + row * COLS] = cr.ReadVector3();
                }
            }
        }
    }

    public class TerrainReader : ContentTypeReader<Terrain>
    {
        protected override Terrain Read(ContentReader input, Terrain existingInstance)
        {
            return new Terrain(input);
        }
    }

}
