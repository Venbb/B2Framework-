using System.IO;
using shaco.ExcelDataReader.Core;
using shaco.ExcelDataReader.Core.OpenXmlFormat;

namespace shaco.ExcelDataReader
{
    internal class ExcelOpenXmlReader : ExcelDataReader<XlsxWorkbook, XlsxWorksheet>
    {
        public ExcelOpenXmlReader(Stream stream)
        {
            Document = new ZipWorker(stream);
            Workbook = new XlsxWorkbook(Document);

            // By default, the data reader is positioned on the first result.
            Reset();
        }

        private ZipWorker Document { get; set; }

        public override void Close()
        {
            base.Close();

            if (null != Document) Document.Dispose();
            Workbook = null;
            Document = null;
        }
    }
}
