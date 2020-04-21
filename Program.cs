using System;
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
		private static readonly SimpleController EmptyController = new SimpleController();

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
			var summary = BenchmarkRunner.Run<Program>();
		}

		private static NES CreateCore()
		{
			string xmlPath = Path.Combine(PathUtils.ExeDirectoryPath, "NesCarts.xml");
			var bootGodBytes = File.ReadAllBytes(xmlPath);
			BootGodDb.GetDatabaseBytes = () => bootGodBytes;

			var rom = GetRom();
			var gameInfo = GameInfo.NullInstance;

			var pathEntries = new PathEntryCollection();
			pathEntries.ResolveWithDefaults();

			var cfp = new CoreFileProvider(Console.WriteLine, new FirmwareManager(), pathEntries, new Dictionary<string, string>());
			var coreComm = new CoreComm(Console.WriteLine, Console.WriteLine, cfp);

			var settings = new NES.NESSettings();
			var syncSettings = new NES.NESSyncSettings();
			var nes = new NES(coreComm, gameInfo, rom, settings, syncSettings);

			for (int i = 0; i < 200; i++)
			{
				nes.FrameAdvance(EmptyController, true, true);
			}

			return nes;
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
		public void FrameAdvance()
		{
			_nes.FrameAdvance(EmptyController, true, true);
		}
	}
}
