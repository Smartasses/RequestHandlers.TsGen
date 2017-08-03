using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if NET45
namespace RequestHandlers.TsGen
{
    public class LoadAssembliesHelper
    {
        public static Assembly[] Load(List<string> inputPaths)
        {
            return inputPaths.Select(x => Assembly.LoadFrom(x)).ToArray();
        }
    }
}
#else
using System.Runtime.Loader;

namespace RequestHandlers.TsGen
{
    public class LoadAssembliesHelper
    {
        public static Assembly[] Load(List<string> inputPaths)
        {
            return inputPaths.Select(x => AssemblyLoadContext.Default.LoadFromAssemblyPath(x)).ToArray();
        }
    }
}
#endif