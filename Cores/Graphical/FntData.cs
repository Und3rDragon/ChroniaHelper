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
            if (!Md.Session.cachedFntData.ContainsKey(path) || overwrite)
            {
                path.CreateFntFontTextures(out textures, out offsets);
            }

            if (!Md.Session.cachedFntData.ContainsKey(path))
            {
                Md.Session.cachedFntData.Add(path, this);
            }

            if (overwrite)
            {
                Md.Session.cachedFntData.Enter(path, this);
            }
        }
    }
}
