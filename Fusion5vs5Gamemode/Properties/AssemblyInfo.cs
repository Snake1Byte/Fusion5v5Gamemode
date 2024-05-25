using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Fusion5vs5Gamemode;
using Fusion5vs5Gamemode.Client;
using LabFusion.SDK.Modules;
using MelonLoader;
using Main = Fusion5vs5Gamemode.Main;

[assembly: MelonInfo(typeof(Main), Main.NAME, Main.VERSION, Main.AUTHOR)]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(Main.NAME)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct(Main.NAME)]
[assembly: AssemblyCopyright("Copyright ©  2024 " + Main.AUTHOR)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d6c0fb27-e32d-457b-beaf-63549dbaa9b8")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(Main.VERSION)]
[assembly: AssemblyFileVersion(Main.VERSION)]

[assembly: ModuleInfo(typeof(FusionModule), Main.NAME, Main.VERSION, Main.AUTHOR, null, true, ConsoleColor.Cyan)]