#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Castle.IO
{
	/// <summary>
	/// 	Path data holder and value object that overrides Equals,
	/// 	implements IEquatable and overrides the == and != operators.
	/// 	
	/// 	Invariant: no fields nor properties are null after c'tor.
	/// </summary>
	[DebuggerDisplay(@"PathInfo: \{ Root: {Root}, Rest: {NonRootPath} \}")]
	public sealed class PathInfo : IEquatable<PathInfo>
	{
		internal const string UNCPrefixRegex = @"(?<UNC_prefix> \\\\\? (?<UNC_literal>\\UNC)?  )";

		internal const string DeviceRegex = 
UNCPrefixRegex + @"?
(?(UNC_prefix)|\\)
(?<device>
 (?<dev_prefix>\\\.\\)
 (
  (?<dev_name>[\w\-]+)
  |(?<dev_guid>\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\})
 )
)\\?";

		private const string DriveRegex = 
UNCPrefixRegex + @"?
(?(UNC_prefix)\\)? # if we have an UNC prefix, there must be an extra backslash
(?<drive>
 (?<drive_letter>[A-Z]{1,3})
 :		# the :-character after the drive letter
 (\\|/) # the trailing slash
)";

		internal const string ServerRegex =
UNCPrefixRegex + @"?
(?(UNC_prefix)|\\) #this is optional IIF we have the UNC_prefix, so only match \\ if we did not have it
(?<server>
 (?<server_prefix>\\)
 (?:
  (?<ipv4>(25[0-5]|2[0-4]\d|[0-1]?\d?\d)(\.(25[0-5]|2[0-4]\d|[0-1]?\d?\d)){3})
  |(?:\[(?<ipv6>[A-F0-9:]{3,39})\])
  |(?<server_name>(?!UNC)[\w\-]+) # allow dashes in server names, but ignore servers named UNC
 )
)\\?";

		private const string StrRegex =
@"
(?<root>
 (" +  DriveRegex + @")
 |(" + ServerRegex + @")
 |(" + DeviceRegex + @")
 |/
 |\\
)?
(?<nonrootpath>
 (?!\\)
 (?<rel_drive>\w{1,3}:)?
 (?<folders_files>.+))?";

		private static readonly Regex _Regex = new Regex(StrRegex,
		                                                 RegexOptions.Compiled |
		                                                 RegexOptions.IgnorePatternWhitespace |
		                                                 RegexOptions.IgnoreCase |
		                                                 RegexOptions.Multiline);

		private readonly string _Root,
		                        _UNCPrefix,
		                        _UNCLiteral,
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

		// too stupid checker
		[ContractVerification(false)]
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

		internal static string GetMatch(MatchCollection matches,
		                               string groupIndex)
		{
			Contract.Ensures(Contract.Result<string>() != null);

			var matchC = matches.Count;

			for (var i = 0; i < matchC; i++)
			{
				if (matches[i].Groups[groupIndex].Success)
					return matches[i].Groups[groupIndex].Value;
			}

			return string.Empty;
		}

		#region c'tor and non null invariants

		private PathInfo(string root, string uncPrefix, string uncLiteral, string drive, string driveLetter,
		                 string server, string iPv4, string iPv6, string serverName, string device, string devicePrefix,
		                 string deviceName, string deviceGuid, string nonRootPath, string relDrive, string folderAndFiles)
		{
			Contract.Requires(root != null);
			Contract.Requires(uncPrefix != null);
			Contract.Requires(uncLiteral != null);
			Contract.Requires(drive != null);
			Contract.Requires(driveLetter != null);
			Contract.Requires(server != null);
			Contract.Requires(iPv4 != null);
			Contract.Requires(iPv6 != null);
			Contract.Requires(serverName != null);
			Contract.Requires(device != null);
			Contract.Requires(devicePrefix != null);
			Contract.Requires(deviceName != null);
			Contract.Requires(deviceGuid != null);
			Contract.Requires(nonRootPath != null);
			Contract.Requires(relDrive != null);
			Contract.Requires(folderAndFiles != null);
			_Root = root;
			_UNCPrefix = uncPrefix;
			_UNCLiteral = uncLiteral;
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

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_Root != null);
			Contract.Invariant(_UNCPrefix != null);
			Contract.Invariant(_UNCLiteral != null);
			Contract.Invariant(_Drive != null);
			Contract.Invariant(_DriveLetter != null);
			Contract.Invariant(_Server != null);
			Contract.Invariant(_IPv4 != null);
			Contract.Invariant(_IPv6 != null);
			Contract.Invariant(_ServerName != null);
			Contract.Invariant(_Device != null);
			Contract.Invariant(_DevicePrefix != null);
			Contract.Invariant(_DeviceName != null);
			Contract.Invariant(_DeviceGuid != null);
			Contract.Invariant(_NonRootPath != null);
			Contract.Invariant(_RelDrive != null);
			Contract.Invariant(_FolderAndFiles != null);
		}

		#endregion

		/// <summary>
		/// 	Examples of return values:
		/// 	<list>
		/// 		<item>\\?\UNC\C:\</item>
		/// 		<item>\\?\UNC\servername\</item>
		/// 		<item>\\192.168.0.2\</item>
		/// 		<item>C:\</item>
		/// 	</list>
		/// 
		/// 	Definition: Returns part of the string that is in itself uniquely from the currently 
		/// 	executing CLR.
		/// </summary>
		[Pure]
		public string Root
		{
			get { return _Root; }
		}

		/// <summary>
		/// 	Examples of return values:
		/// 	<list>
		/// 		<item></item>
		/// 	</list>
		/// </summary>
		[Pure]
		public string UNCPrefix
		{
			get { return _UNCPrefix; }
		}

		/// <summary>
		/// </summary>
		[Pure]
		public string UNCLiteral
		{
			get { return _UNCLiteral; }
		}

		/// <summary>
		/// </summary>
		[Pure]
		public string Drive
		{
			get { return _Drive; }
		}

		/// <summary>
		/// </summary>
		[Pure]
		public string DriveLetter
		{
			get { return _DriveLetter; }
		}

		/// <summary>
		/// </summary>
		[Pure]
		public string Server
		{
			get { return _Server; }
		}

		/// <summary>
		/// 	Gets the IPv4 IP-address if any. <see cref="IPAddress.None"/>
		/// 	if none was found.
		/// </summary>
		[Pure]
		public IPAddress IPv4
		{
			get
			{
				IPAddress addr;
				return !string.IsNullOrEmpty(_IPv4) && IPAddress.TryParse(_IPv4, out addr)
					? addr
					: IPAddress.None;
			}
		}

		/// <summary>
		/// Gets the IPv6 IP-address if any. <see cref="IPAddress.None"/>
		/// if non was found.
		/// 
		/// </summary>
		[Pure]
		public IPAddress IPv6
		{
			get
			{
				IPAddress addr;
				return !string.IsNullOrEmpty(_IPv6) && IPAddress.TryParse(_IPv6, out addr)
					? addr 
					: IPAddress.IPv6None;
			}
		}

		/// <summary>
		/// </summary>
		[Pure]
		public string ServerName
		{
			get { return _ServerName; }
		}

		/// <summary>
		/// </summary>
		[Pure]
		public string Device
		{
			get { return _Device; }
		}

		/// <summary>
		/// </summary>
		[Pure]
		public string DevicePrefix
		{
			get { return _DevicePrefix; }
		}

		/// <summary>
		/// </summary>
		[Pure]
		public string DeviceName
		{
			get { return _DeviceName; }
		}

		/// <summary>
		/// 	Gets the device GUID in the form
		/// 	<code>{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}</code>
		/// 	i.e. 8-4-4-4-12 hex digits with curly brackets.
		/// </summary>
		[Pure]
		public Guid DeviceGuid
		{
			get { return _DeviceGuid == string.Empty ? Guid.Empty : Guid.Parse(_DeviceGuid); }
		}

		/// <summary>
		/// 	Gets a the part of the path that starts when the root ends.
		/// 	The root in turn is any UNC-prefix plus device, drive, server or ip-prefix.
		/// 	This string may not start with neither of '\' or '/'.
		/// </summary>
		[Pure]
		public string NonRootPath
		{
			get { return _NonRootPath; }
		}

		/// <summary>
		/// </summary>
		[Pure]
		public string RelDrive
		{
			get { return _RelDrive; }
		}

		/// <summary>
		/// 	The only time when this differs from <see cref = "NonRootPath" />
		/// 	is when a path like this is used:
		/// 	<code>C:../parent/a.txt</code>, otherwise, for all paths,
		/// 	this property equals <see cref = "NonRootPath" />.
		/// </summary>
		[Pure]
		public string FolderAndFiles
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);
				return _FolderAndFiles;
			}
		}

		[Pure]
		public PathType Type
		{
			get
			{
				if (!string.IsNullOrEmpty(Device))
					return PathType.Device;
				if (!string.IsNullOrEmpty(ServerName))
					return PathType.Server;
				if (IPv4 != IPAddress.None)
					return PathType.IPv4;
				if (IPv6 != IPAddress.IPv6None)
					return PathType.IPv6;
				if (!string.IsNullOrEmpty(Drive))
					return PathType.Drive;

				return PathType.Relative;
			}
		}

		/// <summary>
		/// 	Returns whether <see cref = "Root" /> is not an empty string.
		/// </summary>
		[Pure]
		public bool IsRooted
		{
			get { return !string.IsNullOrEmpty(_Root); }
		}

		/// <summary>
		/// 	Returns whether the current PathInfo is a valid parent of the child path info
		/// 	passed as argument.
		/// </summary>
		/// <param name = "child">The path info to verify</param>
		/// <returns>Whether it is true that the current path info is a parent of child.</returns>
		/// <exception cref = "NotSupportedException">If this instance of path info and child aren't rooted.</exception>
		public bool IsParentOf(PathInfo child)
		{
			if (Root == string.Empty || child.Root == string.Empty)
				throw new NotSupportedException("Non-rooted paths are not supported.");

			// TODO: Normalize Path

			var OK = child.FolderAndFiles.StartsWith(FolderAndFiles);

			switch (Type)
			{
				case PathType.Device:
					OK &= child.DeviceName.Equals(DeviceName, StringComparison.InvariantCultureIgnoreCase);
					break;
				case PathType.Server:
					OK &= child.ServerName.Equals(ServerName, StringComparison.InvariantCultureIgnoreCase);
					break;
				case PathType.IPv4:
					OK &= child.IPv4.Equals(IPv4);
					break;
				case PathType.IPv6:
					OK &= child.IPv6.Equals(IPv6);
					break;
				case PathType.Relative:
					throw new NotSupportedException("Since root isn't empty we should never get relative paths.");

				case PathType.Drive:
					OK &= DriveLetter.ToLowerInvariant() == child.DriveLetter.ToLowerInvariant();
					break;
			}

			return OK;
		}

		/// <summary>
		/// 	Removes the path info passes as a parameter from the current root. Only works for two rooted paths with same root.
		/// 	Does NOT cover all edge cases, please verify its intended results yourself.
		/// 	<example>
		/// 	</example>
		/// </summary>
		/// <param name = "other"></param>
		/// <returns></returns>
		public string RemoveParameterFromRoot(PathInfo other)
		{
			Contract.Requires(Root == other.Root, "roots must match to be able to subtract");
			Contract.Requires(FolderAndFiles.Length >= other.FolderAndFiles.Length,
			                  "The folders and files part of the parameter must be shorter or equal to in length, than that path you wish to subtract from.");

			if (other.FolderAndFiles == FolderAndFiles)
				return string.Empty;

			var startIndex = other.FolderAndFiles.Length;
			Contract.Assume(startIndex <= FolderAndFiles.Length);
			var substring = FolderAndFiles.Substring(startIndex);
			return substring.TrimStart(Path.GetDirectorySeparatorChars());
		}

		public bool Equals(PathInfo other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other._Drive, _Drive)
				&& Equals(other._DriveLetter, _DriveLetter)
				&& Equals(other._Server, _Server)
				&& Equals(other._IPv4, _IPv4)
				&& Equals(other._IPv6, _IPv6)
				&& Equals(other._ServerName, _ServerName) 
				&& Equals(other._Device, _Device) 
				&& Equals(other._DevicePrefix, _DevicePrefix) 
				&& Equals(other._DeviceName, _DeviceName) 
				&& Equals(other._DeviceGuid, _DeviceGuid) 
				&& Equals(other._NonRootPath, _NonRootPath) 
				&& Equals(other._RelDrive, _RelDrive) 
				&& Equals(other._FolderAndFiles, _FolderAndFiles);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (PathInfo)) return false;
			return Equals((PathInfo) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _Drive.GetHashCode();
				result = (result*397) ^ _DriveLetter.GetHashCode();
				result = (result*397) ^ _Server.GetHashCode();
				result = (result*397) ^ _IPv4.GetHashCode();
				result = (result*397) ^ _IPv6.GetHashCode();
				result = (result*397) ^ _ServerName.GetHashCode();
				result = (result*397) ^ _Device.GetHashCode();
				result = (result*397) ^ _DevicePrefix.GetHashCode();
				result = (result*397) ^ _DeviceName.GetHashCode();
				result = (result*397) ^ _DeviceGuid.GetHashCode();
				result = (result*397) ^ _NonRootPath.GetHashCode();
				result = (result*397) ^ _RelDrive.GetHashCode();
				result = (result*397) ^ _FolderAndFiles.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(PathInfo left, PathInfo right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(PathInfo left, PathInfo right)
		{
			return !Equals(left, right);
		}
	}
}