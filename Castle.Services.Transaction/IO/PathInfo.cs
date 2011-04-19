#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion

using System.Diagnostics.Contracts;

namespace Castle.Services.Transaction.IO
{
	using System;
	using System.Net;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Path data holder.
	/// Invariant: no fields nor properties are null after c'tor.
	/// </summary>
	public struct PathInfo
	{
		private const string _StrRegex =
			@"(?<root>
 (?<UNC_prefix> \\\\\?\\ (?<UNC_literal>UNC\\)?  )?
 (?<options>
  (?:
   (?<drive>(?<drive_letter>[A-Z]{1,3}):
   )\\
  )
  |(?<server>(?(UNC_prefix)|\\\\) #this is optional IIF we have the UNC_prefix, so only match \\ if we did not have it
    (?:
     (?<ipv4>(25[0-5]|2[0-4]\d|[0-1]?\d?\d)(\.(25[0-5]|2[0-4]\d|[0-1]?\d?\d)){3})
     |(?:\[(?<ipv6>[A-Fa-f0-9:]{3,39})\])
     |(?<server_name>[\w\-]+) #allow dashes in server names
    )\\
  )
  |(?<device>
   (?<dev_prefix>\\\\\.\\)
   ((?<dev_name>[\w\-]+)
    |(?<dev_guid>\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\})
    )\\
  )
  |/
  |\\ # we can also refer to the current drive alone
 )
)?
(?<nonrootpath>
 (?!\\)
 (?<rel_drive>\w{1,3}:)?
 (?<folders_files>.+))?";

		private static Regex _Regex = new Regex(_StrRegex,
			                   RegexOptions.Compiled |
			                   RegexOptions.IgnorePatternWhitespace |
			                   RegexOptions.IgnoreCase |
			                   RegexOptions.Multiline);

		private readonly string _Root,
								_UNCPrefix,
								_UNCLiteral,
								_Options,
								_Drive,
								_DriveLetter,
								_Server,
								_IPv4,
								_IPv6,
								_ServerName,
								_Device,
								_DevicePrefix,
								_DeviceName,
								_DeviceGuid,
								_NonRootPath,
								_RelDrive,
								_FolderAndFiles;

		public static PathInfo Parse(string path)
		{
			Contract.Requires(path != null);

			var matches = _Regex.Matches(path);

			Func<string, string> m = s => GetMatch(matches, s);
			// this might be possible to improve using raw indicies (ints) instead.
			return new PathInfo(
				m("root"),
				m("UNC_prefix"),
				m("UNC_literal"),
				m("options"),
				m("drive"),
				m("drive_letter"),
				m("server"),
				m("ipv4"),
				m("ipv6"),
				m("server_name"),
				m("device"),
				m("dev_prefix"),
				m("dev_name"),
				m("dev_guid"),
				m("nonrootpath"),
				m("rel_drive"),
				m("folders_files")
				);
		}

		private static string GetMatch(MatchCollection matches,
		                               string groupIndex)
		{
			var matchC = matches.Count;

			for (int i = 0; i < matchC; i++)
			{
				if (matches[i].Groups[groupIndex].Success)
					return matches[i].Groups[groupIndex].Value;
			}

			return string.Empty;
		}

		private PathInfo(string root, string uncPrefix, string uncLiteral, string options, string drive, string driveLetter, string server, string iPv4, string iPv6, string serverName, string device, string devicePrefix, string deviceName, string deviceGuid, string nonRootPath, string relDrive, string folderAndFiles)
		{
			_Root = root;
			_UNCPrefix = uncPrefix;
			_UNCLiteral = uncLiteral;
			_Options = options;
			_Drive = drive;
			_DriveLetter = driveLetter;
			_Server = server;
			_IPv4 = iPv4;
			_IPv6 = iPv6;
			_ServerName = serverName;
			_Device = device;
			_DevicePrefix = devicePrefix;
			_DeviceName = deviceName;
			_DeviceGuid = deviceGuid;
			_NonRootPath = nonRootPath;
			_RelDrive = relDrive;
			_FolderAndFiles = folderAndFiles;
		}

		/// <summary>
		/// Examples of return values:
		/// <list>
		/// <item>\\?\UNC\C:\</item>
		/// <item>\\?\UNC\servername\</item>
		/// <item>\\192.168.0.2\</item>
		/// <item>C:\</item>
		/// </list>
		/// 
		/// Definition: Returns part of the string that is in itself uniquely from the currently 
		/// executing CLR.
		/// </summary>
		public string Root
		{
			get { return _Root; }
		}

		/// <summary>
		/// Examples of return values:
		/// <list>
		/// <item></item>
		/// </list>
		/// </summary>
		public string UNCPrefix
		{
			get { return _UNCPrefix; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string UNCLiteral
		{
			get { return _UNCLiteral; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Options
		{
			get { return _Options; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Drive
		{
			get { return _Drive; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string DriveLetter
		{
			get { return _DriveLetter; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Server
		{
			get { return _Server; }
		}

		/// <summary>
		/// Gets the 0.0.0.0-based IP-address if any.
		/// </summary>
		public string IPv4
		{
			get { return _IPv4; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string IPv6
		{
			get { return _IPv6; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string ServerName
		{
			get { return _ServerName; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Device
		{
			get { return _Device; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string DevicePrefix
		{
			get { return _DevicePrefix; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string DeviceName
		{
			get { return _DeviceName; }
		}

		/// <summary>
		/// Gets the device GUID in the form
		/// <code>{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}</code>
		/// i.e. 8-4-4-4-12 hex digits with curly brackets.
		/// </summary>
		public string DeviceGuid
		{
			get { return _DeviceGuid; }
		}

		/// <summary>
		/// Gets a the part of the path that starts when the root ends.
		/// The root in turn is any UNC-prefix plus device, drive, server or ip-prefix.
		/// This string may not start with neither of '\' or '/'.
		/// </summary>
		public string NonRootPath
		{
			get { return _NonRootPath; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string RelDrive
		{
			get { return _RelDrive; }
		}

		/// <summary>
		/// The only time when this differs from <see cref="NonRootPath"/>
		/// is when a path like this is used:
		/// <code>C:../parent/a.txt</code>, otherwise, for all paths,
		/// this property equals <see cref="NonRootPath"/>.
		/// </summary>
		public string FolderAndFiles
		{
			get { return _FolderAndFiles; }
		}

		public PathType Type
		{
			get
			{
				if (!string.IsNullOrEmpty(Device))
					return PathType.Device;
				if (!string.IsNullOrEmpty(ServerName))
					return PathType.Server;
				if (!string.IsNullOrEmpty(IPv4))
					return PathType.IPv4;
				if (!string.IsNullOrEmpty(IPv6))
					return PathType.IPv6;
				if (!string.IsNullOrEmpty(Drive))
					return PathType.Drive;
				return PathType.Relative;
			}
		}

		/// <summary>
		/// Returns whether <see cref="Root"/> is not an empty string.
		/// </summary>
		public bool IsRooted
		{
			get { return !string.IsNullOrEmpty(_Root); }
		}

		/// <summary>
		/// Returns whether the current PathInfo is a valid parent of the child path info
		/// passed as argument.
		/// </summary>
		/// <param name="child">The path info to verify</param>
		/// <returns>Whether it is true that the current path info is a parent of child.</returns>
		/// <exception cref="NotSupportedException">If this instance of path info and child aren't rooted.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength",
			Justification = "Would change semantics")]
		public bool IsParentOf(PathInfo child)
		{
			if (Root == string.Empty || child.Root == string.Empty)
				throw new NotSupportedException("Non-rooted paths are not supported.");

			var OK = child.FolderAndFiles.StartsWith(FolderAndFiles);

			switch (Type)
			{
				case PathType.Device:
					OK &= child.DeviceName.ToLowerInvariant() == DeviceName.ToLowerInvariant();
					break;
				case PathType.Server:
					OK &= child.ServerName.ToLowerInvariant() == ServerName.ToLowerInvariant();
					break;
				case PathType.IPv4:
					OK &= IPAddress.Parse(child.IPv4).Equals(IPAddress.Parse(IPv4));
					break;
				case PathType.IPv6:
					OK &= (IPAddress.Parse(child.IPv6).Equals(IPAddress.Parse(IPv6)));
					break;
				case PathType.Relative:
					throw new InvalidOperationException("Since root isn't empty we should never get relative paths.");
				case PathType.Drive:
					OK &= DriveLetter.ToLowerInvariant() == child.DriveLetter.ToLowerInvariant();
					break;
			}

			return OK;
		}

		/// <summary>
		/// Removes the path info passes as a parameter from the current root. Only works for two rooted paths with same root.
		/// Does NOT cover all edge cases, please verify its intended results yourself.
		/// <example>
		/// 
		/// </example>
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public string RemoveParameterFromRoot(PathInfo other)
		{
			if (Root != other.Root)
				throw new InvalidOperationException("Roots of this and other don't match.");
			
			if (other.FolderAndFiles.Length > FolderAndFiles.Length)
				throw new InvalidOperationException(
					"The folders and files part of the second parameter must be shorter than that path you wish to subtract from.");

			if (other.FolderAndFiles == FolderAndFiles) return string.Empty;

			return FolderAndFiles.Substring(other.FolderAndFiles.Length).TrimStart(Path.GetDirectorySeparatorChars());
		}
	}
}