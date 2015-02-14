using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffinityGroups.Test
{
    public interface IOfferConcurrencyStressTest
    {
        void Setup();

        void Teardown();
    }

    public static class IOfferConcurrencyStressTestExtensions
    {
        public static void StressTest(this IOfferConcurrencyStressTest instance, Action testCase, int repetitions = 1000)
        {
            for (int i = 0; i < repetitions; i++)
            {
                instance.Setup();

                testCase();

                instance.Teardown();
            }
        }
    }
}
