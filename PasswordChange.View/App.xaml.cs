using System.IO.Compression;
using System.Reflection;
using System.Windows;

namespace PasswordChange.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            System.AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = "PasswordChange.View.Resources.R.gz";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                string dllName = string.Format("{0}.dll", new AssemblyName(args.Name).Name);
                                if (entry.Name.Equals(dllName, System.StringComparison.OrdinalIgnoreCase))
                                {
                                    using (var entryStream = entry.Open())
                                    using (var outStream = new System.IO.MemoryStream())
                                    {
                                        entryStream.CopyTo(outStream);
                                        return Assembly.Load(outStream.ToArray());
                                    }
                                }
                            }
                        }
                    }
                }

                //string resourceName = string.Format("PasswordChange.View.Resources.{0}.dll", new AssemblyName(args.Name).Name);
                //using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                //{
                //    if (stream != null)
                //    {
                //        var assemblyData = new byte[stream.Length];
                //        stream.Read(assemblyData, 0, assemblyData.Length);
                //        return Assembly.Load(assemblyData);
                //    }
                //}

                return null;
            };
        }
    }
}
