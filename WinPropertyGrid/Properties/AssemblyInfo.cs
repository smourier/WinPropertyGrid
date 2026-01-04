using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyTitle("WinPropertyGrid")]
[assembly: AssemblyDescription("WinUI3 Property Grid")]
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("WinPropertyGrid")]
[assembly: AssemblyCopyright("Copyright (C) 2023-2026 Simon Mourier. All rights reserved.")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("cb341e8b-f413-4267-aa52-4df1f4b36d62")]
[assembly: SupportedOSPlatform("windows10.0.22621.0")]
