﻿namespace MonoSAMFramework.Portable.FileHelper
{
	//see https://github.com/SupSuper/MonoGame-SaveManager 
	public abstract class FileHelper
	{
		public abstract void WriteData(string fileid, string data);
		public abstract string ReadDataOrNull(string fileid);

		public static FileHelper Inst { get; private set; }

		public static void RegisterSytsemSecificHandler(FileHelper h)
		{
			Inst = h;
		}
	}
}
