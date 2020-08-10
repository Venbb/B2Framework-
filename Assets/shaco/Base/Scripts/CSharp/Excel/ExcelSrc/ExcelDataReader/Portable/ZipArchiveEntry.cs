using System.IO;
using shaco.ICSharpCode.SharpZipLib.Zip;

namespace shaco.ExcelDataReader.Core
{
    internal sealed class ZipArchiveEntry
    {
        private readonly ZipFile _handle;
        private readonly shaco.ICSharpCode.SharpZipLib.Zip.ZipEntry _entry;

        internal ZipArchiveEntry(ZipFile handle, shaco.ICSharpCode.SharpZipLib.Zip.ZipEntry entry)
        {
            _handle = handle;
            _entry = entry;
        }

        public string FullName {get{return _entry.Name;}}

        public Stream Open()
        {
            return _handle.GetInputStream(_entry);
        }
    }
}