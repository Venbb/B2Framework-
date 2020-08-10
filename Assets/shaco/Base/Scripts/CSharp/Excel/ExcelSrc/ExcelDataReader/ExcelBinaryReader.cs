using System.IO;
using System.Text;
using shaco.ExcelDataReader.Core.BinaryFormat;

namespace shaco.ExcelDataReader
{
    /// <summary>
    /// ExcelDataReader Class
    /// </summary>
    internal class ExcelBinaryReader : ExcelDataReader<XlsWorkbook, XlsWorksheet>
    {
        public ExcelBinaryReader(Stream stream, string password, Encoding fallbackEncoding)
        {
            Workbook = new XlsWorkbook(stream, password, fallbackEncoding);

            // By default, the data reader is positioned on the first result.
            Reset();
        }

        public override void Close()
        {
            base.Close();
            if (null != Workbook)
            {
                if (null != Workbook.Stream) 
                    Workbook.Stream.Dispose();
            }
            Workbook = null;
        }
    }
}
