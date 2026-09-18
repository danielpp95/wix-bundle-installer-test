// Entry point for the out-of-process bootstrapper application (WiX v5 Burn model).
// Must stay minimal: it only connects to the parent Burn process as quickly as possible.
namespace InstallerUI
{
    using WixToolset.BootstrapperApplicationApi;

    internal class Program
    {
        private static int Main()
        {
            var application = new InstallerBootstrapperApplication();

            ManagedBootstrapperApplication.Run(application);

            return 0;
        }
    }
}
