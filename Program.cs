﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BizHawk.Client.Common;
using BizHawk.Common.PathExtensions;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.Nintendo.NES;

namespace BizBenchMark
{
	[Config(typeof(Config))]
	public class Program
	{
		private static NES _nes;

		private class Config : ManualConfig
		{
			public Config()
			{
				Add(Job.MediumRun
					.WithLaunchCount(1)
					.WithId("OutOfProc"));

				Add(Job.MediumRun
					.WithLaunchCount(1)
					.With(InProcessEmitToolchain.Instance)
					.WithId("InProcess"));
			}
		}

		public static void Main(string[] args)
		{
			_nes = CreateCore();
			for (int i = 0; i < 200; i++)
			{
				_nes.FrameAdvance(new SimpleController(), true, true);
			}

			var summary = BenchmarkRunner.Run<Program>();
		}

		private static NES CreateCore()
		{
			string xmlPath = Path.Combine(PathUtils.GetExeDirectoryAbsolute(), "NesCarts.xml");
			var bootGodBytes = File.ReadAllBytes(xmlPath);
			NES.BootGodDB.GetDatabaseBytes = () => bootGodBytes;

			var rom = GetRom();
			var gameInfo = GameInfo.NullInstance;

			var pathEntries = new PathEntryCollection();
			pathEntries.ResolveWithDefaults();

			var cfp = new CoreFileProvider(ShowMessage, new FirmwareManager(), pathEntries, new Dictionary<string, string>());
			var coreComm = new CoreComm(ShowMessage, ShowMessage, cfp);

			var settings = new NES.NESSettings();
			var syncSettings = new NES.NESSyncSettings();
			var nes = new NES(coreComm, gameInfo, rom, settings, syncSettings);
			return nes;
		}

		private static void ShowMessage(string message)
		{
			Console.WriteLine(message);
		}

		private static byte[] GetRom()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream("BizBenchMark.test.nes"))
			{
				if (stream == null)
				{
					throw new InvalidOperationException("Could not find test rom");
				}

				byte[] bytes = new byte[stream.Length];
				stream.Read(bytes, 0, bytes.Length);
				return bytes;
			}
		}

		[Benchmark]
		public decimal FrameAdvance()
		{
			int x = 0;
			x++;
			var y = Math.Round((decimal) x);
			return y;
		}
	}
}
