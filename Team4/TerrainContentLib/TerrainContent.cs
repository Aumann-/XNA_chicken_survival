using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TerrainContentLib
{
    public class TerrainContent
    {
        public byte[] height;
        public Vector3[] position;
        public Vector3[] normal;
        public float cellWidth, cellHeight;
        public int ROWS = 257;
        public int COLS = 257;
        public float worldWidth = 1000.0f;
        public float worldHeight = 1000.0f;

        //constructor
        public TerrainContent(byte[] bytes)
        {
            height = bytes;
            setCellDimensions();
            generatePositions();
            generateNormals();
        }

        //method to calcualte cellWidth and Height
        public void setCellDimensions()
        {
            cellWidth = 2.0f * worldWidth / (COLS - 1);
            cellHeight = 2.0f * worldHeight / (ROWS - 1);
        }

        //initialize positions array
        private void generatePositions()
        {
            position = new Vector3[ROWS * COLS];
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    float X = -worldWidth + col * cellWidth;
                    float Y = height[row * COLS + col];
                    float Z = -worldHeight + row * cellHeight;
                    position[col + row * COLS] = new Vector3(X, Y, Z);
                }
            }
        }

        //method to create normal vectors for each vertex
        private void generateNormals()
        {
            Vector3 tail, right, down, cross;

            normal = new Vector3[ROWS * COLS];
            //normal is cross product of two vectors joined at tail

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    tail = position[col + row * COLS];
                    right = position[col + row * COLS] - tail;
                    down = position[col + row * COLS] - tail;
                    cross = Vector3.Cross(down, right);
                    cross.Normalize();
                    normal[col + row * COLS] = cross;
                }
            }
        }



    }
}
