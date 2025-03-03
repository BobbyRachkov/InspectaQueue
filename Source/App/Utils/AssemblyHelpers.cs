using System.Reflection;

namespace Rachkov.InspectaQueue.Common.Utils
{
    public static class AssemblyHelpers
    {
        /// <summary>
        /// Loads assembly from file without locking the file. File can be deleted while the app is running.
        /// </summary>
        /// <param name="path">The full path of the dll</param>
        /// <returns></returns>
        public static Assembly LoadAssemblyLoose(string path)
        {
            var assemblyBytes = File.ReadAllBytes(path);
            return Assembly.Load(assemblyBytes);
        }

        /// <summary>
        /// Loads assembly from file without locking the file. File can be deleted while the app is running.
        /// </summary>
        /// <param name="path">The full path of the dll</param>
        /// <returns></returns>
        public static Assembly? TryLoadAssemblyLoose(string path)
        {
            try
            {
                var assemblyBytes = File.ReadAllBytes(path);
                return Assembly.Load(assemblyBytes);
            }
            catch
            {
                return null;
            }
        }
    }
}
