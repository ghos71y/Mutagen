using Mutagen.Bethesda.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutagen.Bethesda.Binary
{
    /// <summary>
    /// Class containing all the extra meta bits for parsing
    /// </summary>
    public class ParsingBundle
    {
        /// <summary>
        /// Game constants meta object to reference for header length measurements
        /// </summary>
        public GameConstants Constants { get; }

        /// <summary>
        /// Optional MasterReferenceReader to reference while reading
        /// </summary>
        public MasterReferenceReader? MasterReferences { get; set; }

        /// <summary>
        /// Optional RecordInfoCache to reference while reading
        /// </summary>
        public RecordInfoCache? RecordInfoCache { get; set; }

        /// <summary>
        /// Optional strings lookup to reference while reading
        /// </summary>
        public IStringsFolderLookup? StringsLookup { get; set; }

        /// <summary>
        /// Whether to do parallel work when possible
        /// </summary>
        public bool Parallel { get; set; }

        /// <summary>
        /// Tracker of whether within worldspace data section
        /// </summary>
        public bool InWorldspace { get; set; }

        /// <summary>
        /// Tracker of current major record version
        /// </summary>
        public ushort? FormVersion { get; set; }

        /// <summary>
        /// ModKey of the mod being parsed
        /// </summary>
        public ModKey ModKey { get; set; }

        public ParsingBundle(GameConstants constants)
        {
            this.Constants = constants;
        }

        public static implicit operator ParsingBundle(GameRelease release)
        {
            return new ParsingBundle(GameConstants.Get(release));
        }

        public static implicit operator ParsingBundle(GameConstants constants)
        {
            return new ParsingBundle(constants);
        }

        public static implicit operator GameConstants(ParsingBundle bundle)
        {
            return bundle.Constants;
        }

        public void ReportIssue(RecordType? recordType, string note)
        {
            // Nothing for now.  Need to implement
        }
    }
}
