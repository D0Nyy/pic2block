using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.ImageSharp;

namespace pic2block
{
    public class Structure
    {
        public Size size;
        public List<Block> blocks { get; }
        
        public List<String> stateList { get; set; }

        public Structure(Size size)
        {
            this.size = size;
            blocks = new List<Block>();
            stateList = new List<String>();
        }

        public void Add(Block block)
        {
            block.position = new Vector2(blocks.Count - (size.Width * (int)(blocks.Count/size.Width)) ,(int)(blocks.Count/size.Height));
            blocks.Add(block);
        }
    }
}