using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nua.Types;

namespace Nua.Stdlib
{

    public class TableOperations : NuaNativeTable
    {
        private TableOperations() { }

        public static TableOperations Create()
        {
            return new TableOperations()
            {
                Storage =
                {

                }
            };
        }
    }
}
