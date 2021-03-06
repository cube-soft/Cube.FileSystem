﻿/* ------------------------------------------------------------------------- */
//
// Copyright (c) 2010 CubeSoft, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/* ------------------------------------------------------------------------- */
using NUnit.Framework;

namespace Cube.FileSystem.Tests
{
    /* --------------------------------------------------------------------- */
    ///
    /// PathFilterTest
    ///
    /// <summary>
    /// Provides a test fixture for the PathFilter class.
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    class PathFilterTest
    {
        #region Tests

        /* ----------------------------------------------------------------- */
        ///
        /// Escape
        ///
        /// <summary>
        /// Tests the escaping process.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(@"C:\windows\dir\file.txt",       '_', ExpectedResult = @"C:\windows\dir\file.txt")]
        [TestCase(@"C:\windows\dir\file*?<>|.txt",  '_', ExpectedResult = @"C:\windows\dir\file_____.txt")]
        [TestCase(@"C:\windows\dir\file:.txt",      '+', ExpectedResult = @"C:\windows\dir\file+.txt")]
        [TestCase(@"C:\windows\dir:\file.txt",      '+', ExpectedResult = @"C:\windows\dir+\file.txt")]
        [TestCase(@"C:\windows\dir\\file.txt.",     '+', ExpectedResult = @"C:\windows\dir\file.txt")]
        [TestCase(@"C:\windows\dir\file.txt.",      '+', ExpectedResult = @"C:\windows\dir\file.txt")]
        [TestCase(@"C:\windows\dir\file.txt. ... ", '+', ExpectedResult = @"C:\windows\dir\file.txt")]
        [TestCase(@"C:\CON\PRN\AUX.txt",            '_', ExpectedResult = @"C:\_CON\_PRN\_AUX.txt")]
        [TestCase(@"C:\COM0\com1\Com2.txt",         '_', ExpectedResult = @"C:\_COM0\_com1\_Com2.txt")]
        [TestCase(@"C:\LPT1\LPT10\LPT5.txt",        '_', ExpectedResult = @"C:\_LPT1\LPT10\_LPT5.txt")]
        [TestCase(@"C:\LPT2\:LPT3:\LPT4.txt",       '_', ExpectedResult = @"C:\_LPT2\_LPT3_\_LPT4.txt")]
        [TestCase(@"/unix/foo/bar.txt",             '_', ExpectedResult = @"unix\foo\bar.txt")]
        [TestCase(@"",                              '_', ExpectedResult = @"")]
        [TestCase(null,                             '_', ExpectedResult = @"")]
        public string Escape(string src, char replaced) =>
            new PathFilter(src)
            {
                AllowDriveLetter      = true,
                AllowParentDirectory  = false,
                AllowCurrentDirectory = false,
                AllowInactivation     = false,
                AllowUnc              = false,
                EscapeChar            = replaced,
            }.Value;

        /* ----------------------------------------------------------------- */
        ///
        /// Escape_DriveLetter
        ///
        /// <summary>
        /// Confirms the results according to the drive letter settings.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(@"C:\windows\dir\allow.txt",    true,  ExpectedResult = @"C:\windows\dir\allow.txt")]
        [TestCase(@"C:\C:\windows\dir\allow.txt", true,  ExpectedResult = @"C:\C_\windows\dir\allow.txt")]
        [TestCase(@"C:\windows\dir\deny.txt",     false, ExpectedResult = @"C_\windows\dir\deny.txt")]
        [TestCase(@"C:\C:\windows\dir\deny.txt",  false, ExpectedResult = @"C_\C_\windows\dir\deny.txt")]
        public string Escape_DriveLetter(string src, bool drive) =>
            new PathFilter(src) { AllowDriveLetter = drive }.Value;

        /* ----------------------------------------------------------------- */
        ///
        /// Escape_CurrentDirectory
        ///
        /// <summary>
        /// Confirms the results according to the "." letter settings.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(@"C:\windows\dir\.\allow.txt", true, ExpectedResult = @"C:\windows\dir\.\allow.txt")]
        [TestCase(@"C:\windows\dir\.\deny.txt", false, ExpectedResult = @"C:\windows\dir\deny.txt")]
        public string Escape_CurrentDirectory(string src, bool allow) =>
            new PathFilter(src)
            {
                AllowInactivation     = false,
                AllowCurrentDirectory = allow,
            }.Value;

        /* ----------------------------------------------------------------- */
        ///
        /// Escape_ParentDirectory
        ///
        /// <summary>
        /// Confirms the results according to the ".." letter settings.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(@"C:\windows\dir\..\allow.txt", true, ExpectedResult = @"C:\windows\dir\..\allow.txt")]
        [TestCase(@"C:\windows\dir\..\deny.txt", false, ExpectedResult = @"C:\windows\dir\deny.txt")]
        public string Escape_ParentDirectory(string src, bool allow) =>
            new PathFilter(src)
            {
                AllowInactivation    = false,
                AllowParentDirectory = allow,
            }.Value;

        /* ----------------------------------------------------------------- */
        ///
        /// Escape_Inactivation
        ///
        /// <summary>
        /// Confirms the results according to the inactivation settings.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(@"\\?\C:\windows\dir\deny.txt",  false, ExpectedResult = @"C:\windows\dir\deny.txt")]
        [TestCase(@"\\?\C:\windows\dir\allow.txt",  true, ExpectedResult = @"\\?\C:\windows\dir\allow.txt")]
        [TestCase(@"\\?\C:\windows\.\..\allow.txt", true, ExpectedResult = @"\\?\C:\windows\allow.txt")]
        public string Escape_Inactivation(string src, bool allow) =>
            new PathFilter(src)
            {
                AllowInactivation     = allow,
                AllowDriveLetter      = true,
                AllowCurrentDirectory = true,
                AllowParentDirectory  = true,
                AllowUnc              = true,
            }.Value;

        /* ----------------------------------------------------------------- */
        ///
        /// Escape_Unc
        ///
        /// <summary>
        /// Confirms the results according to the UNC path settings.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(@"\\domain\dir\allow.txt", true, false, ExpectedResult = @"\\domain\dir\allow.txt")]
        [TestCase(@"\\domain\dir\allow.txt", true,  true, ExpectedResult = @"domain\dir\allow.txt")]
        [TestCase(@"\\domain\dir\deny.txt", false, false, ExpectedResult = @"domain\dir\deny.txt")]
        public string Escape_Unc(string src, bool unc, bool inactivation) =>
            new PathFilter(src)
            {
                AllowInactivation = inactivation,
                AllowUnc          = unc
            }.Value;

        /* ----------------------------------------------------------------- */
        ///
        /// Match
        ///
        /// <summary>
        /// Testss the Match method.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(@"C:\windows\dir\file.txt", "file.txt", ExpectedResult = true)]
        [TestCase(@"C:\windows\dir\FILE.txt", "file.txt", ExpectedResult = true)]
        [TestCase(@"C:\windows\dir\file.txt", "file",     ExpectedResult = false)]
        public bool Match(string src, string cmp) => new PathFilter(src).Match(cmp);

        /* ----------------------------------------------------------------- */
        ///
        /// MatchAny
        ///
        /// <summary>
        /// Testss the MatchAny method.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(@"C:\windows\dir\Thumbs.db",       ExpectedResult = true)]
        [TestCase(@"C:\windows\__MACOSX\file.txt",   ExpectedResult = true)]
        [TestCase(@"C:\windows\__MACOSX__\file.txt", ExpectedResult = false)]
        [TestCase(@"",                               ExpectedResult = false)]
        [TestCase(null,                              ExpectedResult = false)]
        public bool MatchAny(string src) =>
            new PathFilter(src).MatchAny(new[]
            {
                ".DS_Store", "Thumbs.db", "__MACOSX", "desktop.ini"
            });

        /* ----------------------------------------------------------------- */
        ///
        /// Match_IgnoreCase
        ///
        /// <summary>
        /// Confirms the results according to the case sensitive settings.
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [TestCase(@"C:\windows\.ds_store\file.txt", true,  ExpectedResult = true)]
        [TestCase(@"C:\windows\.ds_store\file.txt", false, ExpectedResult = false)]
        public bool MatchAny_IgnoreCase(string src, bool ignore) =>
            new PathFilter(src).MatchAny(new[]
            {
                ".DS_Store", "Thumbs.db", "__MACOSX", "desktop.ini"
            }, ignore);

        #endregion
    }
}
