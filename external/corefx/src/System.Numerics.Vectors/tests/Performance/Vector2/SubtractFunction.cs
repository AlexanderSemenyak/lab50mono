// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Xunit.Performance;

namespace System.Numerics.Tests
{
    public static partial class Perf_Vector2
    {
        [Benchmark(InnerIterationCount = VectorTests.DefaultInnerIterationsCount)]
        public static void SubtractFunctionBenchmark()
        {
            var expectedResult = VectorTests.Vector2Value;

            foreach (var iteration in Benchmark.Iterations)
            {
                Vector2 actualResult;

                using (iteration.StartMeasurement())
                {
                    actualResult = SubtractFunctionTest();
                }

                VectorTests.AssertEqual(expectedResult, actualResult);
            }
        }

        public static Vector2 SubtractFunctionTest()
        {
            var result = VectorTests.Vector2Value;

            for (var iteration = 0; iteration < Benchmark.InnerIterationCount; iteration++)
            {
                result = Vector2.Subtract(result, VectorTests.Vector2Delta);
            }

            return result;
        }
    }
}
