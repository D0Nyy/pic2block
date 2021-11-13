using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;  
using Color = System.Drawing.Color;
namespace pic2block
{
    class Program
    {
        private static readonly int BLOCKRESOLUTION = 16 * 16;
        
        private static Structure structure;

        private static readonly List<Block> Blocks = new List<Block>();

        private static int choise;

        static void Main(string[] args)
        {
            GetBlockColor();
            Console.Write("Give a picture: ");
            var image = Console.ReadLine();
            
            Console.Write("Give width: ");
            var width = Int32.Parse(Console.ReadLine());
            Console.Write("Give height: ");
            var height = Int32.Parse(Console.ReadLine());
            
            Console.Write("Press 1 for Wall(MAX:255x255) or 2 for Ground(NO LIMIT): ");
            choise = Int32.Parse(Console.ReadLine());
            
            if (choise == 1 || choise == 2)
            {
                Console.WriteLine("Converting image...");
                // Set canvas size and run
                structure = new Structure(new Size(width, height));
                TranslatePicture(image);
            }
            else
            {
                Console.WriteLine("ERROR. Not a valid option.");
                return;
            }
        }

        /// <summary>
        /// This function get the textures from the Textures directory and matches them to a color.
        /// </summary>
        static void GetBlockColor()
        {
            // Go threw all Blocks in the Textures folder
            foreach (var fileName in Directory.GetFiles("Textures"))
            {
                // Get texture image
                var texture = Image.Load<Rgb24>($"{fileName}");

                // Get sum of the color of the texture
                var colorSumR = 0;
                var colorSumG = 0;
                var colorSumB = 0;

                for (int i = 0; i < texture.Height; i++)
                {
                    var pixelRowSpan = texture.GetPixelRowSpan(i);

                    for (int j = 0; j < texture.Width; j++)
                    {
                        var pixel = pixelRowSpan[j];

                        colorSumR += pixel.R;
                        colorSumG += pixel.G;
                        colorSumB += pixel.B;
                    }
                }
                
                // Find the average color of the block
                var blockColor = Color.FromArgb(colorSumR / BLOCKRESOLUTION, colorSumG / BLOCKRESOLUTION,
                    colorSumB / BLOCKRESOLUTION);
                
                // Get block id(must be right files)
                var blockid = $"minecraft:{fileName.Split("\\")[1].Split('.')[0]}";
                
                Blocks.Add(new Block(blockid, blockColor));
            }

            // Save block info to a json
            File.WriteAllText("palette.json",JsonConvert.SerializeObject(Blocks,Formatting.Indented));
        }
        
        static void TranslatePicture(String path)
        {
            Image<Rgb24> image = Image.Load<Rgb24>(path);

            // Resize image
            if (image.Height > structure.size.Height || image.Width > structure.size.Width)
            {
                image.Mutate(x => x.Resize(structure.size.Width,structure.size.Height,KnownResamplers.NearestNeighbor)); // Removing NearestNeighbor makes it blurry
                //image.Mutate(x => x.Resize(structure.size.Width,structure.size.Height)); // Maybe this is better for bigger images
            }
            else
            {
                structure.size.Height = image.Height;
                structure.size.Width = image.Width;
            }

            // this loop runs every pixel row of the image
            for (int i = 0; i < image.Height; i++)
            {
                // Get all row pixels
                var pixelRowSpan = image.GetPixelRowSpan(i);

                // Iterate threw each pixel
                for (int j = 0; j < image.Width; j++)
                {
                    // Set bedrock as default color
                    Block blockPicked = Blocks.First(x=>x.BlockID.Equals("minecraft:bedrock"));
                    
                    // Determine the closest color
                    var r = (int)pixelRowSpan[j].R;
                    var g = (int)pixelRowSpan[j].G;
                    var b = (int)pixelRowSpan[j].B;

                    int df = 10000;
                    foreach (Block block in Blocks)
                    {
                        var similarity = ColorDifference(block.AverageColor, Color.FromArgb(r, g, b));
                        if (similarity !=0 && similarity < df)
                        {
                            df = similarity;
                            blockPicked = block;
                        }
                    }

                    pixelRowSpan[j] = SixLabors.ImageSharp.Color.FromRgb(blockPicked.AverageColor.R, blockPicked.AverageColor.G, blockPicked.AverageColor.B);
                    
                    // Create a copy of the block and give it to the structure
                    structure.Add(new Block(blockPicked));
                }
            }
            
            // save image
            image.Save($"Pictures/Generated.png");
            CreateDataPack();
        }

        private static void CreateDataPack()
        {
            var fileName = "Datapack\\pic2block\\data\\p2b\\functions\\generatepicture.mcfunction";
            List<string> commands = new List<string>();

            foreach (var block in structure.blocks)
            {
                if (choise == 1)
                {
                    // WALL (MAX: 255X255)
                    commands.Add($"setblock ~{structure.size.Width - block.position.X} ~{structure.size.Height - block.position.Y} ~{-1} {block.BlockID}");
                }
                else if (choise == 2)
                {
                    // GROUND (Technically Unlimited. Be careful)
                    commands.Add($"setblock ~{structure.size.Width - block.position.X} ~{-1} ~{structure.size.Height - block.position.Y} {block.BlockID}");
                }
            }
            
            File.WriteAllLines(fileName,commands);
            Console.WriteLine("Finished!");
        }

        /// <summary>
        /// This function returns how similar are the colors given.
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        private static int ColorDifference(Color color1, Color color2)
        {
            return (int) Math.Sqrt((color1.R - color2.R) * (color1.R - color2.R) 
                                     + (color1.G - color2.G) * (color1.G - color2.G)
                                     + (color1.B - color2.B) * (color1.B - color2.B)); 
        }
    }
}
