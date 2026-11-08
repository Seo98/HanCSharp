using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace HanCSharp.VisualStudio
{
    internal static class ContentTypes
    {
        public const string Name = "HanCSharp";
#pragma warning disable CS0649
        [Export(typeof(ContentTypeDefinition)), Name(Name), BaseDefinition("code")]
        internal static ContentTypeDefinition HanContent;
        [Export(typeof(FileExtensionToContentTypeDefinition)), ContentType(Name), FileExtension(".han")]
        internal static FileExtensionToContentTypeDefinition HanFile;
#pragma warning restore CS0649
    }
}
