using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nua.Types;

namespace Nua
{
    public static class NuaUtilities
    {
        public static bool ConditionTest(NuaValue? value)
        {
            if (value == null)
                return false;
            if (value is not NuaBoolean boolean)
                return true;
            else
                return boolean.Value;
        }
    }
}
