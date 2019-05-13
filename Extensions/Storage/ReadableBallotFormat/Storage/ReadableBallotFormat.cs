using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text.RegularExpressions;
using MoonsetTechnologies.Voting.Ballots;

// This implements the Readable Ballot Format, designed for hand-modification
// and visual examination.  This format allows exporting ballots for visual
// examination, and for production of test ballot sets through simple means.
// 
// This is not a secure storage format.

// Format looks as follows:
//
//   Readable Ballot Format [Version] [Ballot Type]
//   CANDIDATES
//   [ID] [Candidate GUID] [Candidate Name]
//   ...
//   END
//   LOCATIONS
//   [ID] [Location GUID] [Location Name]
//   ...
//   END
//   BALLOTS [Location ID]
//   [COUNT]: [Candidate ID]>[Candidate ID]>...
//   ...
//   END
//
// Example:
//
//   Readable Ballot Format 1 0
//   CANDIDATES
//   0 00000000-1111-2222-3333-aaaabbbbcccc Alex
//   1 aaaabbbb-1111-2222-dddd-00001111aaaa Chris
//   2 deadbeef-0000-0000-0000-feeddeadbeef Sam
//   END
//   LOCATIONS
//   0 ffff0000-aaaa-bbbb-cccc-000011112222 Precinct 27 Center 34
//   END
//   BALLOTS 0
//   11: 0>1>2
//   29: 1>0>2
//   27: 0>1>2
//   19: 2>1>0
//   3: 2>0>1
//   END
//
// The above uses format v1, RankedBallot (0).

namespace MoonsetTechnologies.Voting.Storage
{
    class ReadableBallotFormat // : IBallotStorage
    {
        private const string headerLine = "Readable Ballot Format";
        protected List<Ballot> ballots;
        public IEnumerable<Ballot> Ballots => ballots;

        public ReadableBallotFormat(IEnumerable<Ballot> ballots, FileStream file)
        {

        }

        /// <summary>
        /// Populates the Ballot Storage object from a file.
        /// </summary>
        /// <param name="file">A newly-opened, readable FileStream at position 0.</param>
        public ReadableBallotFormat(FileStream file)
        {

        }

        private void LoadFile(FileStream file)
        {
            Guid ballotFormat;
            MemoryMappedFile mmf;
            if (file is null)
                throw new ArgumentNullException("file", "FileStream passed is null.");
            if (!file.CanRead)
                throw new ArgumentOutOfRangeException("file","File is not readable.");

            // We do all this crap to get a file that won't change if it's changed on-disk
            // during read.
            mmf = MemoryMappedFile.CreateFromFile(file, null, 0,
                MemoryMappedFileAccess.CopyOnWrite, HandleInheritability.None, true);
            MemoryMappedViewStream mfv = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
            StreamReader sr = new StreamReader(mfv);

            bool loadFormat()
            {
                string line = sr.ReadLine();
                Match m = Regex.Match(line,
                    @"^(" + headerLine + @") (\d+) ([\da-f\-])$");
                if (m.Groups[0].Value != headerLine)
                    return false;
                // maximum format version is 1
                if (int.Parse(m.Groups[1].Value) > 1)
                    return false;

                // Only supports this format of GUID
                try
                {
                    ballotFormat = Guid.ParseExact(m.Groups[2].Value, "D");
                }
                catch (FormatException)
                {
                    return false;
                }

                // TODO:  We now have 
                return true;
            }
        }
    }
}
