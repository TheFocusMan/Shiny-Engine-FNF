using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]
[assembly:InternalsVisibleTo("Shiny Engine FNF")]
[assembly: XmlnsDefinitionAttribute("http://shinyenginefnf.fridaynightfunkin.com/controls", "Shiny_Engine_FNF.Code")]
[assembly: XmlnsPrefix("http://shinyenginefnf.fridaynightfunkin.com/controls", "fnf")] 