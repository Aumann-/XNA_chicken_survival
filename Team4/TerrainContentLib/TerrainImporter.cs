using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;

namespace TerrainContentLib
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    /// 
    /// This should be part of a Content Pipeline Extension Library project.
    /// 
    /// TODO: change the ContentImporter attribute to specify the correct file
    /// extension, display name, and default processor for this importer.
    /// </summary>
    [ContentImporter(".raw", DisplayName = "Terrain Importer", DefaultProcessor = "TerrainProcessor")]
    public class TerrainImporter : ContentImporter<TerrainContent>
    {
        public override TerrainContent Import(string filename, ContentImporterContext context)
        {
            // TODO: read the specified file into an instance of the imported type.
            byte[] bytes = File.ReadAllBytes(filename);
            TerrainContent terrain = new TerrainContent(bytes);
            return terrain;
        }
    }
}
