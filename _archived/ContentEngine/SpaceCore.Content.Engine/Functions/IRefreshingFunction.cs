using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Content.Functions;
public interface IRefreshingFunction // This is mainly for use with the Patch stuff in SpaceCore SDV
{
    public bool WouldChangeFromRefresh(FuncCall fcall, ContentEngine ce);
}
