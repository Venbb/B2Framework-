using System;

namespace shaco.ExcelDataReader.Core.BinaryFormat
{
    /// <summary>
    /// Represents Workbook's global window description
    /// </summary>
    internal class XlsBiffWindow1 : XlsBiffRecord
    {
        internal XlsBiffWindow1(byte[] bytes, uint offset)
            : base(bytes, offset)
        {
        }

        [Flags]
        public enum Window1Flags : ushort
        {
            Hidden = 0x1,
            Minimized = 0x2,
            
            // (Reserved) = 0x4,
            HScrollVisible = 0x8,
            VScrollVisible = 0x10,
            WorkbookTabs = 0x20
            
            // (Other bits are reserved)
        }

        /// <summary>
        /// Gets the X position of a window
        /// </summary>
        public ushort Left {get{return ReadUInt16(0x0); }}

        /// <summary>
        /// Gets the Y position of a window
        /// </summary>
        public ushort Top {get{return ReadUInt16(0x2); }}

        /// <summary>
        /// Gets the width of the window
        /// </summary>
        public ushort Width {get{return ReadUInt16(0x4); }}

        /// <summary>
        /// Gets the height of the window
        /// </summary>
        public ushort Height {get{return ReadUInt16(0x6); }}

        /// <summary>
        /// Gets the window flags
        /// </summary>
        public Window1Flags Flags {get {return (Window1Flags)ReadUInt16(0x8); }}

        /// <summary>
        /// Gets the active workbook tab (zero-based)
        /// </summary>
        public ushort ActiveTab { get{return ReadUInt16(0xA); }}

        /// <summary>
        /// Gets the first visible workbook tab (zero-based)
        /// </summary>
        public ushort FirstVisibleTab {get{return ReadUInt16(0xC); }}

        /// <summary>
        /// Gets the number of selected workbook tabs
        /// </summary>
        public ushort SelectedTabCount {get{return ReadUInt16(0xE); }}

        /// <summary>
        /// Gets the workbook tab width to horizontal scrollbar width
        /// </summary>
        public ushort TabRatio {get {return ReadUInt16(0x10); }}
    }
}