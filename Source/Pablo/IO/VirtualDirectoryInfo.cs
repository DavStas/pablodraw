using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using SI = System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Eto.IO
{
	public abstract class VirtualDirectoryInfo : EtoDirectoryInfo
	{
		VirtualDirectoryInfo parent;
		
		public EtoFileInfo FileInfo { get; private set; }
		
		protected string VirtualPath { get; private set; }
		protected string FileName 
		{
			get { return FileInfo.FullName; }
		}
		protected List<VirtualFileEntry> Files { get; private set; }
		
		public static bool FlattenInitialDirectories { get; set; }
		
		static VirtualDirectoryInfo()
		{
			FlattenInitialDirectories = true;
		}
		
		public bool FlattenInitialDirectory
		{
			get; set;
		}

		protected VirtualDirectoryInfo(EtoFileInfo fileInfo)
			: this()
		{
			this.FileInfo = fileInfo;
			this.VirtualPath = string.Empty;
		}

		protected VirtualDirectoryInfo(VirtualDirectoryInfo parent, string path)
			: this()
		{
			this.FileInfo = parent.FileInfo;
			this.VirtualPath = path;
			if (parent != null) {
				this.parent = parent;
				this.Files = parent.Files;
			}
		}

		protected VirtualDirectoryInfo(Stream stream)
			: this()
		{
			this.VirtualPath = string.Empty;
			ReadStream(stream);
		}
		
		private VirtualDirectoryInfo()
		{
			this.FlattenInitialDirectory = FlattenInitialDirectories;
		}
		
		protected abstract VirtualDirectoryInfo CreateDirectory(VirtualDirectoryInfo parent, string path);
		protected virtual VirtualFileInfo CreateFile(VirtualDirectoryInfo parent, string path)
		{
			return new VirtualFileInfo(parent, path);
		}

		protected void ReadEntries()
		{
			if (Files != null)  return;
			using (Stream stream = FileInfo.OpenRead()) {
				ReadStream(stream);
			}
		}
		private void ReadStream(Stream stream)
		{
			Files = new List<VirtualFileEntry>();
			bool hasFiles = false;
			int topDirectories = 0;
			VirtualFileEntry topDirectory = null;
			foreach (var entry in ReadEntries(stream))
			{
				Files.Add(entry);
				if (VirtualPath.Length == 0 && string.IsNullOrEmpty(entry.Path)) {
					if (entry.IsDirectory) 
					{
						topDirectory = entry;
						topDirectories++;	
					}
					else
						hasFiles = true;
				}
			}
			
			if (FlattenInitialDirectory && VirtualPath.Length == 0 && !hasFiles && topDirectories == 1)
			{
				this.VirtualPath = topDirectory.FullPath;
			}
		}

		protected abstract IEnumerable<VirtualFileEntry> ReadEntries(Stream stream);

		public abstract Stream OpenRead(string fileName);

		public override string FullName
		{
			get { 
				if (!string.IsNullOrEmpty(VirtualPath)) return Path.Combine(FileName, VirtualPath); 
				else return FileName;
			}
		}
		
		public override string Name {
			get {
				if (!string.IsNullOrEmpty(VirtualPath)) return Path.GetFileName(VirtualPath.TrimEnd(Path.DirectorySeparatorChar));
				else return Path.GetFileName(FullName);
			}
		}

		public override EtoDirectoryInfo Parent
		{
			get
			{
				if (parent != null) return parent;
				else if (FileInfo != null) return FileInfo.Directory;
				else return null; //return new DiskDirectoryInfo(Path.Combine(Path.GetPathRoot(FileName), Path.GetDirectoryName(FileName)));
			}
		}


		public override EtoDirectoryInfo GetSubDirectory(string subDirectory)
		{
			ReadEntries();
			string dir = Path.Combine(VirtualPath, subDirectory);
			foreach (var file in Files)
			{
				if (file.IsDirectory && string.Compare(file.Path, dir, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return CreateDirectory(this, file.Path);
				}
			}
			return null;
		}

		protected override IEnumerable<EtoDirectoryInfo> GetPathDirectories()
		{
			ReadEntries();
			foreach (var file in Files)
			{
				if (file.IsDirectory && file.Path.Equals(VirtualPath, StringComparison.OrdinalIgnoreCase))
				{
					yield return CreateDirectory(this, file.FullPath);
				}
			}
		}

		public override IEnumerable<EtoFileInfo> GetFiles()
		{
			ReadEntries();
			foreach (var file in Files)
			{
				if (!file.IsDirectory && file.Path.Equals(VirtualPath, StringComparison.OrdinalIgnoreCase))
				{
					yield return CreateFile(this, file.FullPath);
				}
			}
		}

		public override IEnumerable<EtoFileInfo> GetFiles(IEnumerable<string> patterns)
		{
			ReadEntries();
			// convert search pattern to regular expression!
			string filter = string.Join("|", patterns.ToArray());
			filter = filter.Replace(".", "\\.");
			filter = filter.Replace("*", ".+");

			Regex reg = new Regex(filter, RegexOptions.IgnoreCase 
#if !MOBILE
				| RegexOptions.Compiled
#endif
				);

			foreach (var file in Files)
			{
				if (!file.IsDirectory && file.Path.Equals(VirtualPath, StringComparison.OrdinalIgnoreCase) && reg.IsMatch(file.Name))
				{
					yield return CreateFile(this, file.FullPath);
				}
			}
		}

	}
}
