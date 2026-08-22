using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Utils;

namespace ChroniaHelper.Cores.Graphical;

public class FntData
{
    public Dictionary<int, MTexture> textures;
    public Dictionary<int, Vc2> offsets;

    public FntData() { }
    
    public FntData(string path)
    {
        path.CreateFntFontTextures(out textures, out offsets);
    }
    
    public class SessionData : FntData
    {
        public SessionData(string path, bool overwrite = false)
        {
            // 命中缓存且不强制覆盖时，直接复用已解析的字体数据（避免重复解析 FNT XML）
            if (!overwrite && Md.Session.cachedFntData.TryGetValue(path, out FntData cached))
            {
                textures = cached.textures;
                offsets = cached.offsets;
                return;
            }

            path.CreateFntFontTextures(out textures, out offsets);

            if (overwrite)
            {
                Md.Session.cachedFntData[path] = this;
            }
            else if (!Md.Session.cachedFntData.ContainsKey(path))
            {
                Md.Session.cachedFntData.Add(path, this);
            }
        }
    }
}
