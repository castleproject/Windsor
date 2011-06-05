using System;
using System.IO;
using System.Text;
using Castle.IO;
using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.Tests.IO
{
	public abstract class file_system_ctxt<T> : context where T : IFileSystem, new()
	{
		public file_system_ctxt()
		{
			CurrentDirectory = Environment.CurrentDirectory;
			FileSystem = new T();
		}

		protected IFileSystem FileSystem { get; private set; }
		protected string CurrentDirectory { get; set; }

		protected ITemporaryDirectory given_temp_dir()
		{
			return TempDir = FileSystem.CreateTempDirectory();
		}

		protected ITemporaryDirectory TempDir { get; set; }

		protected ITemporaryFile given_temp_file(string content = null)
		{
			var temporaryFile = FileSystem.CreateTempFile();
			if (content != null)
				WriteString(temporaryFile, content);

			return temporaryFile;
		}

		protected string ReadString(IFile file)
		{
			using (var reader = file.OpenRead())
			{
				var stream = new MemoryStream();
				reader.CopyTo(stream);

				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		protected void WriteString(IFile temporaryFile, string content)
		{
			using (var writer = temporaryFile.OpenWrite())
				writer.Write(Encoding.UTF8.GetBytes(content));
		}
		[TestFixtureTearDown]
		public void delete_temp_dir()
		{
			if (TempDir != null)
				TempDir.Dispose();
		}
	}
}