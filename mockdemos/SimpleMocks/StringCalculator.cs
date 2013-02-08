using System;
using MyBillingProduct;

namespace Step1Mocks
{
    public class StringCalculator
    {
        private ILogger log;

        public StringCalculator(ILogger logger)
        {
            log = logger;

        }

        private void test()
        {

        }
        public int Add(int x, int y)
        {
            if (x<0)
            {
                throw new ArgumentException();
            }
            return x + y;
        }
        
        public int Add(string numbers)
        {
            //test that the following line uses the machine name
            CallLogger( Environment.MachineName + "-" +  numbers);
            Add(1, 2);
            if (numbers == string.Empty)
            {
                return 0;
            }
            if (numbers.Contains(","))
            {
                string[] splitted = numbers.Split(',');
                return Add(splitted[0]) + Add(splitted[1]);
            }
            return int.Parse(numbers);
        }
    }
}
