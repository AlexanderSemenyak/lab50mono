// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Xunit.Performance;

namespace System.Numerics.Tests
{
    public static partial class Perf_Vector4
    {
        [Benchmark(InnerIterationCount = VectorTests.DefaultInnerIterationsCount)]
        public static void GetHashCodeBenchmark()
        {
            const int expectedResult = 536896558;

            foreach (var iteration in Benchmark.Iterations)
            {
                int actualResult;

                using (iteration.StartMeasurement())
                {
                    actualResult = GetHashCodeTest();
                }

                VectorTests.AssertEqual(expectedResult, actualResult);
            }
        }

        public static int GetHashCodeTest()
        {
            var result = 0;

            for (var iteration = 0; iteration < Benchmark.InnerIterationCount; iteration++)
            {
                // The inputs aren't being changed and the output is being reset with each iteration, so a future
                // optimization could potentially throw away everything except for the final call. This would break
                // the perf test. The JitOptimizeCanary code below does modify the inputs and consume each output.
                result = VectorTests.Vector4Value.GetHashCode();
            }

            return result;
        }

        [Benchmark(InnerIterationCount = VectorTests.DefaultInnerIterationsCount)]
        public static void GetHashCodeJitOptimizeCanaryBenchmark()
        {
            const int expectedResult = 504524288;

            foreach (var iteration in Benchmark.Iterations)
            {
                int actualResult;

                using (iteration.StartMeasurement())
                {
                    actualResult = GetHashCodeJitOptimizeCanaryTest();
                }

                VectorTests.AssertEqual(expectedResult, actualResult);
            }
        }

        public static int GetHashCodeJitOptimizeCanaryTest()
        {
            var result = 0;
            var value = VectorTests.Vector4Value;

            for (var iteration = 0; iteration < Benchmark.InnerIterationCount; iteration++)
            {
                value += VectorTests.Vector4Delta;
                result += value.GetHashCode();
            }

            return result;
        }
    }
}
