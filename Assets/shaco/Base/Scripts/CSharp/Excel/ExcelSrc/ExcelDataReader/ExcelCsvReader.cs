using System.IO;
using System.Text;
using shaco.ExcelDataReader.Core.CsvFormat;

namespace shaco.ExcelDataReader
{
    internal class ExcelCsvReader : ExcelDataReader<CsvWorkbook, CsvWorksheet>
    {
        public ExcelCsvReader(Stream stream, Encoding fallbackEncoding, char[] autodetectSeparators, int analyzeInitialCsvRows)
        {
            Workbook = new CsvWorkbook(stream, fallbackEncoding, autodetectSeparators, analyzeInitialCsvRows);

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
