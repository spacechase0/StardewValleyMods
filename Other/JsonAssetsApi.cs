using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreRings.Other
{
    public interface JsonAssetsApi
    {
        void LoadAssets(string path);

        int GetObjectId(string name);
    }
}
