using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

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
		}
	}
}
