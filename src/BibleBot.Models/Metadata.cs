/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using BibleBot.Models;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class MDBookMap : Dictionary<string, Dictionary<string, string>>;
public class MDBookNames : Dictionary<string, List<string>>;
public class MDABBookMap : Dictionary<string, string>;
public class MDABVersionBookData : Dictionary<Version, ABBooksResponse>;
public class MDVersionBookList : Dictionary<BookCategories, Dictionary<string, string>>;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member