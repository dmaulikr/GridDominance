﻿using GridDominance.DSLEditor.Drawing;
using GridDominance.DSLEditor.Helper;
using GridDominance.Graphfileformat.Blueprint;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using GridDominance.Graphfileformat;
using GridDominance.SAMScriptParser;

namespace GridDominance.DSLEditor
{
	public partial class MainWindowViewModel
	{
		private readonly GraphPreviewPainter graphPainter = new GraphPreviewPainter();

		private ImageSource ReparseGraphFile(string input)
		{
			try
			{
				var sw = Stopwatch.StartNew();

				ClearLog();
				AddLog("Start parsing");

				var lp = ParseGraphFile(input);

				var img = ImageHelper.CreateImageSource(graphPainter.Draw(lp, FilePath, AddLog));

				AddLog("File parsed  in " + sw.ElapsedMilliseconds + "ms");

				return img;
			}
			catch (ParsingException pe)
			{
				AddLog(pe.ToOutput());
				Console.Out.WriteLine(pe.ToString());

				return ImageHelper.CreateImageSource(graphPainter.Draw(null, null, AddLog));
			}
			catch (Exception pe)
			{
				AddLog(pe.Message);
				Console.Out.WriteLine(pe.ToString());

				return ImageHelper.CreateImageSource(graphPainter.Draw(null, null, AddLog));
			}
		}

		private void CompileGraph()
		{
			if (!File.Exists(FilePath)) throw new FileNotFoundException(FilePath);

			var lp = ParseGraphFile(Code);

			var dir = Path.GetDirectoryName(FilePath);
			var name = Path.GetFileNameWithoutExtension(FilePath) + ".xnb";

			if (string.IsNullOrWhiteSpace(dir)) throw new Exception("dir == null");
			if (string.IsNullOrWhiteSpace(name)) throw new Exception("name == null");

			var outPath = Path.Combine(dir, name);

			byte[] binData;
			using (var ms = new MemoryStream())
			using (var bw = new BinaryWriter(ms))
			{
				lp.BinarySerialize(bw);
				binData = ms.ToArray();
			}

			using (var fs = new FileStream(outPath, FileMode.Create))
			using (var bw = new ExtendedBinaryWriter(fs))
			{
				// Header

				bw.Write('X');
				bw.Write('N');
				bw.Write('B');
				bw.Write('g');        // Target Platform
				bw.Write((byte)5);    // XNB Version
				bw.Write((byte)0);    // Flags


				bw.Write((UInt32)0x95);

				bw.Write((byte)0x01);
				bw.Write("GridDominance.Graphfileformat.Pipeline.GDLevelReader, GridDominance.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
				bw.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 });

				bw.Write(binData);

				bw.Write(new byte[] { 0x58, 0x4E, 0x42, 0x67, 0x05, 0x00, 0x58, 0x4E, 0x42, 0x67, 0x05, 0x00 });
				bw.Write(new byte[] { 0x9B, 0x00, 0x00, 0x00, 0x01 });

				bw.Write("GridDominance.Graphfileformat.Pipeline.GDLevelReader, GridDominance.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

				bw.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 });
				bw.Write(new byte[] { 0x58, 0x4E, 0x42, 0x67, 0x05, 0x00, 0x58, 0x4E, 0x42, 0x67, 0x05, 0x00 });
				bw.Write(new byte[] { 0x58, 0x4E, 0x42, 0x67, 0x05, 0x00, 0xA1 });
				bw.Write(new byte[] { 0x00, 0x00, 0x00, 0x01 });

				bw.Write("GridDominance.Graphfileformat.Pipeline.GDLevelReader, GridDominance.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

				bw.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 });
			}
		}

		private GraphBlueprint ParseSpecificGraphFile(string f)
		{
			var path = Path.GetDirectoryName(f) ?? "";
			var pattern = "*.gsheader";

			var includes = Directory.EnumerateFiles(path, pattern).ToDictionary(p => Path.GetFileName(p) ?? p, p => File.ReadAllText(p, Encoding.UTF8));

			string IncludesFunc(string x) => includes.FirstOrDefault(p => GraphBlueprint.IsIncludeMatch(p.Key, x)).Value;

			return new GraphParser(File.ReadAllText(f), IncludesFunc).Parse();
		}

		private GraphBlueprint ParseGraphFile(string input)
		{
			Func<string, string> includesFunc = x => null;
			if (File.Exists(FilePath))
			{
				var path = Path.GetDirectoryName(FilePath) ?? "";
				var pattern = "*.gsheader";

				var includes = Directory.EnumerateFiles(path, pattern).ToDictionary(p => Path.GetFileName(p) ?? p, p => File.ReadAllText(p, Encoding.UTF8));

				includesFunc = x => includes.FirstOrDefault(p => GraphBlueprint.IsIncludeMatch(p.Key, x)).Value;
			}

			return new GraphParser(input, includesFunc).Parse();
		}

		private GraphBlueprint ParseGraphFileSafe(string input)
		{
			try
			{
				return ParseGraphFile(input);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private void UpdateSourceFiles()
		{
			if (!File.Exists(FilePath)) { MessageBox.Show("No root folder"); return; }
			var folder = Path.GetDirectoryName(FilePath);
			if (!Directory.Exists(folder)) { MessageBox.Show("No root folder"); return; }
			if (!folder.ToLower().Trim('\\').EndsWith("GridDominance.Shared\\Content\\levels".ToLower())) { MessageBox.Show("Invalid root folder"); return; }

			var files1 = Directory.EnumerateFiles(folder).Where(p => Path.GetExtension(p).ToLower() == ".gslevel").ToList();
			var files2 = Directory.EnumerateFiles(folder).Where(p => Path.GetExtension(p).ToLower() == ".gegraph").ToList();
			var levels = files1.Select(f => ParseSpecificLevelFile(f, false)).ToList();
			var worlds = files2.Select(f => ParseSpecificGraphFile(f)).ToList();
			{
				var f0 = Path.Combine(folder, @"..\..\..\GridDominance.Shared\Content\Content.mgcb");

				if (File.Exists(f0))
				{
					var txt0 = File.ReadAllText(f0);
					foreach (var f in files1)
					{
						if (!txt0.Contains($"#begin levels/{Path.GetFileName(f)}"))
						{
							txt0 += $"#begin levels/{Path.GetFileName(f)}\r\n";
							txt0 += $"/importer:GDLevelImporter\r\n";
							txt0 += $"/processor:GDLevelProcessor\r\n";
							txt0 += $"/build:levels/{Path.GetFileName(f)}\r\n";
							txt0 += $"\r\n";
						}
					}
					if (File.ReadAllText(f0) != txt0) File.WriteAllText(f0, txt0);
				}
				else
				{
					MessageBox.Show("FileNotFound: " + f0);
				}
			}

			{
				var f1 = Path.Combine(folder, @"..\..\Resources\Levels.cs");

				if (File.Exists(f1))
				{
					var txt1 = File.ReadAllText(f1);
					foreach (var loadstr in files1.Select(f => $"LoadLevel(content, \"levels/{Path.GetFileNameWithoutExtension(f)}\");"))
					{
						if (!txt1.Contains(loadstr))
						{
							txt1 = txt1.Replace("/* [MARK_LOAD_LEVEL] */", $"{loadstr}\r\n\t\t\t/* [MARK_LOAD_LEVEL] */");
						}
					}
					if (File.ReadAllText(f1) != txt1) File.WriteAllText(f1, txt1);
				}
				else
				{
					MessageBox.Show("FileNotFound: " + f1);
				}
			}

			{
				var f2 = Path.Combine(folder, @"..\..\..\GridDominance.Server\internals\config_levelids.php");
				StringBuilder txt2 = new StringBuilder();
				txt2.AppendLine("<?php");
				txt2.AppendLine();
				txt2.AppendLine("if(count(get_included_files()) ==1) exit(\"Direct access not permitted.\");");
				txt2.AppendLine();
				txt2.AppendLine("return [");

				bool first = true;
				foreach (var ww in worlds)
				{
					if (!first) txt2.AppendLine();
					first = false;
					foreach (var nn in ww.Nodes)
					{
						var ll = levels.First(l => l.UniqueID == nn.LevelID);
						txt2.AppendLine($"\t[ '{ww.ID:B}', '{ll.UniqueID:B}' ], // {ll.Name,-8} | {ll.FullName}");
					}
				}

				txt2.AppendLine("];");

				if (File.Exists(f2))
				{
					File.WriteAllText(f2, txt2.ToString());
				}
				else
				{
					MessageBox.Show("FileNotFound: " + f2);
				}

			}
		}
	}
}
