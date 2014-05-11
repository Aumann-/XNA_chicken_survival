using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

// TODO: replace this with the type you want to write out.
using TWrite = System.String;

namespace TerrainContentLib
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class TerrainWriter : ContentTypeWriter<TerrainContent>
    {
        protected override void Write(ContentWriter cw, TerrainContent terrain)
        {
            cw.Write(terrain.ROWS);
            cw.Write(terrain.COLS);
            cw.Write(terrain.worldWidth);
            cw.Write(terrain.worldHeight);
            cw.Write(terrain.cellWidth);
            cw.Write(terrain.cellHeight);

            for (int row = 0; row < terrain.ROWS; row++)
            {
                for (int col = 0; col < terrain.COLS; col++)
                {
                    cw.Write(terrain.position[col + row * terrain.COLS]);
                    cw.Write(terrain.normal[col + row * terrain.COLS]);
                }
            }
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "TerrainRuntime.Terrain, TerrainRuntime, Version=1.0.0.0, Culture=neutral";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // TODO: change this to the name of your ContentTypeReader
            // class which will be used to load this data.
            return "TerrainRuntime.TerrainReader, TerrainRuntime, Version=1.0.0.0, Culture=neutral";
        }
    }
}
