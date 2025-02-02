using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Editor_IFC;

namespace RZDP_IFC_Viewer.Infracrucrure
{
    class PropertySetComparer : IEqualityComparer<BasePropertySetDefinition>
    {
        public bool Equals(BasePropertySetDefinition? prop1, BasePropertySetDefinition? prop2)
        {
            if (prop1.IFCPropertySetDefinition.Equals(prop2.IFCPropertySetDefinition))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode([DisallowNull] BasePropertySetDefinition obj)
        {

            return obj.IFCPropertySetDefinition.GetHashCode();
        }
    }
}
