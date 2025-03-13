using MoonTweaker.Utilities.Configuration;
using MoonTweaker.Utilities.Tweaks;

namespace MoonTweaker.Core.ViewModel
{
    internal class InterfaceVM : ViewModelBase
    {
        public bool IsBlockForWin10 => SystemDiagnostics.IsWindowsVersion[11];
        public bool IsBlockWithoutLicense => WindowsLicense.IsWindowsActivated;
    }
}
