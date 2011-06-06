using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Castle.IO.FileSystems.Local.Win32.Interop
{
	// http://msdn.microsoft.com/en-us/library/aa363858(v=vs.85).aspx
	
	[Serializable, Flags]
	public enum NativeFileOptions : uint
	{
		/// <summary>The file is read only. Applications can read the file, but cannot write to or delete it.</summary>
		FILE_ATTRIBUTE_READONLY = 1,
		
		/// <summary>The file is hidden. Do not include it in an ordinary directory listing.</summary>
		FILE_ATTRIBUTE_HIDDEN = 2,
		
		/// <summary>The file is part of or used exclusively by an operating system.</summary>
		FILE_ATTRIBUTE_SYSTEM = 4,
		
		/// <summary>The file should be archived. Applications use this attribute to mark files for backup or removal.</summary>
		FILE_ATTRIBUTE_ARCHIVE = 32,

		/// <summary>The file does not have other attributes set. This attribute is valid only if used alone.</summary>
		FILE_ATTRIBUTE_NORMAL = 128,
		
		/// <summary>	
		/// The file is being used for temporary storage.
		/// For more information, see the Caching Behavior section of this topic.
		/// </summary>
		FILE_ATTRIBUTE_TEMPORARY = 256,
		
		/// <summary>The data of a file is not immediately available. This attribute indicates that file data is physically moved to offline storage. This attribute is used by Remote Storage, the hierarchical storage management software. Applications should not arbitrarily change this attribute.</summary>
		FILE_ATTRIBUTE_OFFLINE = 4096,

		/// <summary>
		/// <para>
		/// The file or directory is encrypted. For a file, this means that all data in the file is encrypted. For a directory, this means that encryption is the default for newly created files and subdirectories. For more information, see File Encryption.
		/// </para><para>
		/// This flag has no effect if FILE_ATTRIBUTE_SYSTEM is also specified.
		/// </para>
		/// </summary>
		FILE_ATTRIBUTE_ENCRYPTED = 16384,
	
		// 'special cases':

	}
}
