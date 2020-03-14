using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BizHawk.Emulation.Common;

namespace BizBenchMark
{
	[Config(typeof(Config))]
	public class Program
	{
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
			var rom = GetRom();
			var gameInfo = GameInfo.NullInstance;
			var coreComm = new CoreComm(ShowMessage, ShowMessage);
			var summary = BenchmarkRunner.Run<Program>();
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
