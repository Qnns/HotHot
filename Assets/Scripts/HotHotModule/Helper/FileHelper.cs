using System;
using System.Collections.Generic;
using System.IO;

namespace HotHot
{
	public static class FileHelper
	{
		public static List<string> GetAllFiles(string dir, string searchPattern = "*")
		{
			List<string> list = new List<string>();
			GetAllFiles(list, dir, searchPattern);
			return list;
		}

		public static void GetAllFiles(List<string> files, string dir, string searchPattern = "*")
		{
			string[] fls = Directory.GetFiles(dir, searchPattern);
			foreach (string fl in fls)
			{
				files.Add(fl);
			}

			string[] subDirs = Directory.GetDirectories(dir);
			foreach (string subDir in subDirs)
			{
				GetAllFiles(files, subDir, searchPattern);
			}
		}

		public static void CleanDirectory(string dir)
		{
			if (!Directory.Exists(dir))
			{
				return;
			}
			foreach (string subdir in Directory.GetDirectories(dir))
			{
				Directory.Delete(subdir, true);
			}

			foreach (string subFile in Directory.GetFiles(dir))
			{
				File.Delete(subFile);
			}
		}

		public static void CopyDirectory(string srcDir, string tgtDir)
		{
			DirectoryInfo source = new DirectoryInfo(srcDir);
			DirectoryInfo target = new DirectoryInfo(tgtDir);

			if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
			{
				throw new Exception("父目录不能拷贝到子目录！");
			}

			if (!source.Exists)
			{
				return;
			}

			if (!target.Exists)
			{
				target.Create();
			}

			FileInfo[] files = source.GetFiles();

			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
			}

			DirectoryInfo[] dirs = source.GetDirectories();

			for (int j = 0; j < dirs.Length; j++)
			{
				CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
			}
		}

		public static void CreateFile(string path, byte[] content)
		{
			MakeSureDirectoryExists(Path.GetDirectoryName(path));
			using (var fileStream = File.Create(path))
			{
				fileStream.Write(content, 0, content.Length);
			}
		}

		public static void CreateFile(string path, Stream content)
		{
			MakeSureDirectoryExists(Path.GetDirectoryName(path));
			byte[] bytes = new byte[content.Length];
			content.Read(bytes, 0, bytes.Length);
			// 设置当前流的位置为流的开始 
			content.Seek(0, SeekOrigin.Begin);
			CreateFile(path, bytes);
		}

		public static void Copy(string srcPath, string desPath)
		{
			MakeSureDirectoryExists(Path.GetDirectoryName(desPath));
			File.Copy(srcPath, desPath, true);
		}

		public static void MakeSureDirectoryExists(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
	}

}