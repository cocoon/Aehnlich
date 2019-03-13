﻿namespace AehnlichLib.Dir.Merge
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Implements a merge algorithm to give all common entries (entries with same name)
    /// the same index while all other entries (occurring only in A or only in B) are at
    /// an index where the other item is null.
    /// </summary>
    internal class MergeIndex
    {
        #region ctors
        /// <summary>
        /// Class constructor
        /// </summary>
        public MergeIndex(FileSystemInfo[] infosA,
                          FileSystemInfo[] infosB,
                          bool isSorted)
            : this()
        {
            InfosA = infosA;
            InfosB = infosB;
            IsSorted = IsSorted;
        }

        /// <summary>
        /// Hidden standard constructor
        /// </summary>
        protected MergeIndex()
        {
        }
        #endregion ctors

        #region properties
        public FileSystemInfo[] InfosA { get; }

        public FileSystemInfo[] InfosB { get; }

        public bool IsSorted { get; protected set; }

        public List<MergedEntry> MergedEntries { get; protected set; }
        #endregion properties

        public int Merge()
        {
            if (IsSorted == false)
            {
                if (InfosA is DirectoryInfo[] && InfosB is DirectoryInfo[])
                {
                    // Sort them
                    Array.Sort((DirectoryInfo[])InfosA, FileSystemInfoComparer.DirectoryComparer);
                    Array.Sort((DirectoryInfo[])InfosA, FileSystemInfoComparer.DirectoryComparer);
                }
                else
                {
                    // Sort them
                    Array.Sort((FileInfo[])InfosA, FileSystemInfoComparer.FileComparer);
                    Array.Sort((FileInfo[])InfosB, FileSystemInfoComparer.FileComparer);
                }

                IsSorted = true;
            }

            List<MergedEntry> mergedEntries = new List<MergedEntry>();

            int indexA = 0;
            int indexB = 0;

            int countA = InfosA.Length;
            int countB = InfosB.Length;

            while (indexA < countA && indexB < countB)
            {
                FileSystemInfo infoA = InfosA[indexA];
                FileSystemInfo infoB = InfosB[indexB];

                int compareResult = string.Compare(infoA.Name, infoB.Name, true);
                if (compareResult == 0)
                {
                    // The item is in both directories
                    mergedEntries.Add(new MergedEntry(infoA, infoB));
                    indexA++;
                    indexB++;
                }
                else if (compareResult < 0)
                {
                    // iCompareResult < 0
                    // The item is only in A
                    mergedEntries.Add(new MergedEntry(infoA, null));
                    indexA++;
                }
                else
                {
                    // iCompareResult > 0
                    // The item is only in B
                    mergedEntries.Add(new MergedEntry(null, infoB));
                    indexB++;
                }
            }

            // Add any remaining entries
            if (indexA < countA)
            {
                for (; indexA < countA; indexA++)
                {
                    mergedEntries.Add(new MergedEntry(InfosA[indexA++], null));
                }
            }
            else if (indexB < countB)
            {
                for (; indexB < countB; indexB++)
                {
                    mergedEntries.Add(new MergedEntry(null, InfosB[indexB++]));
                }
            }

            MergedEntries = mergedEntries;

            return mergedEntries.Count;
        }
    }
}
