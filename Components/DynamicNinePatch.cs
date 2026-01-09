using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Imports;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Cores;

public class DynamicNinePatch : Component
{
    public List<MTexture> textures = new();
    public Dictionary<NinePatchLabel, MTexture> subTextures = new();
    public NinePatchLabel index = new() {
        cordX = 0, 
        cordY = 0, 
        frameCount = 0 
    };
    public NinePatchConfig config = NinePatchConfig.Normal;
    
    public bool playing = true;

    public struct NinePatchConfig
    {
        public int sizeX;
        public int sizeY;
        public int unitSizeX;
        public int unitSizeY;
        public float animationInterval;

        public NinePatchConfig(int sizeX = 3, int sizeY = 3, int unitX = 8, int unitY = 8, float interval = 0.1f)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            unitSizeX = unitX;
            unitSizeY = unitY;
            animationInterval = interval;
        }
        
        public static NinePatchConfig Normal;
        static NinePatchConfig()
        {
            Normal = new() { 
                sizeX = 3, sizeY = 3, 
                unitSizeX = 8, unitSizeY = 8, 
                animationInterval = 0.1f 
            };
        }

        public void Validate()
        {
            sizeX.MakeAbs();
            sizeY.MakeAbs();
            unitSizeX.MakeAbs();
            unitSizeY.MakeAbs();
            animationInterval.MakeAbs();
        }
    }

    public struct NinePatchLabel
    {
        public int frameCount;
        public int cordX;
        public int cordY;

        public NinePatchLabel(int x = 0, int y = 0, int n = 0)
        {
            cordX = x;
            cordY = y;
            frameCount = n;
        }

        public void Validate()
        {
            frameCount.MakeAbs();
            cordX.MakeAbs();
            cordY.MakeAbs();
        }
    }

    public DynamicNinePatch(string path, NinePatchConfig config, bool active = true, bool visible = true)
        : base(active, visible)
    {
        this.config = config;
        this.config.Validate();

        textures = GFX.Game.GetAtlasSubtextures(path);

        for(int n = 0; n < textures.Count; n++)
        {
            for(int cx = 0; cx < config.sizeX; cx++)
            {
                for(int cy = 0; cy < config.sizeY; cy++)
                {
                    MTexture texture = textures[n].GetSubtexture(
                        cx * config.unitSizeX, cy * config.unitSizeY, 
                        config.unitSizeX, config.unitSizeY);
                    NinePatchLabel label = new(cx, cy, n);
                    subTextures.Enter(label, texture);
                }
            }
        }
    }

    public override void Render()
    {
        for(int m = 0; (float)m < Entity.Width / 8f; m++)
        {
            for(int n = 0; (float)n < Entity.Height / 8f; n++)
            {
                int labelX = ((m != 0) ? (((float)m != Entity.Width / 8f - 1f) ? 1 : 2) : 0);
                int labelY = ((n != 0) ? (((float)n != Entity.Height / 8f - 1f) ? 1 : 2) : 0);

                AnalyzeTexture(labelX, labelY);
                index.Validate();

                subTextures[index].Draw(new Vector2(Entity.X + (float)(m * 8), Entity.Y + (float)(n * 8)));
            }
        }
    }

    public override void Update()
    {
        base.Update();

        if (playing && Scene.OnInterval(config.animationInterval.GetAbs()))
        {
            index.frameCount++;

            if(index.frameCount >= textures.Count)
            {
                index.frameCount = 0;
            }
        }
    }

    public void AnalyzeTexture(int x, int y)
    {
        index.cordX = SetIndex(x, config.sizeX);
        index.cordY = SetIndex(x, config.sizeY);
    }

    public int SetIndex(int label, int size)
    {
        if(label == 0)
        {
            return 0;
        }
        else if(label == 2)
        {
            return size - 1;
        }
        else if(label == 1)
        {
            if(size == 1)
            {
                return 0;
            }
            else if(size == 2)
            {
                return 1;
            }
            else if(size == 3)
            {
                return 2;
            }
            else if(size >= 4)
            {
                return Calc.Random.Range(1, size - 1);
            }
        }

        return 0;
    }
}
