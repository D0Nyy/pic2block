using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;

namespace pic2block
{
    public class Block
    {
        public String BlockID { get; set; }
        public Color AverageColor { get; set; }
        public Vector2 position { get; set; }
        
        // I have to make this so i can copy a block
        public Block(Block block)
        {
            this.BlockID = block.BlockID;
            this.AverageColor = block.AverageColor;
            this.position = block.position;
        }
        public Block(String blockid, Color color):this()
        {
            this.BlockID = blockid;
            this.AverageColor = color;
        }

        public Block()
        {
            
        }
    }
}