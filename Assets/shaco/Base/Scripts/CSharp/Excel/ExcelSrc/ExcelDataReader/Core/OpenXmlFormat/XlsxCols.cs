using System.Collections.Generic;

namespace shaco.ExcelDataReader.Core.OpenXmlFormat
{
    internal class XlsxCols : XlsxElement
    {
        public XlsxCols()
            : base(XlsxElementType.Cols)
        {
        }

        public List<Col> Value { get; set; }
    }
}
