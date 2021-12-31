using System;

namespace NukeTaskRunner;

using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell;

[ PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true),
  InstalledProductRegistration(
      "#110",
      "#112",
      Vsix.Version,
      IconResourceID = 400
  ), Guid(PackageGuids.guidVSPackageString), ProvideMenuResource("Menus.ctmenu", 1), ]
// ReSharper disable once InconsistentNaming
public sealed class VSPackage : AsyncPackage
{
}
