namespace Infrastructure.Unit.Tests.Orderers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PriorityAttribute : Attribute
    {
        public uint Order { get;}

        public PriorityAttribute(uint order = uint.MaxValue)
        {
            Order = order;
        }
    }

    public class PriorityMethodOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
                where TTestCase : ITestCase
        {
            var result = testCases.OrderBy(x => GetNumericalOrderAttribute(x));
            return result;
        }

        private uint? GetNumericalOrderAttribute(ITestCase testCase)
        {
            var attribute = testCase.TestMethod.Method.GetCustomAttributes(typeof(PriorityAttribute))
                .OfType<IReflectionAttributeInfo>()
                .Select(x => x.Attribute)
                .OfType<PriorityAttribute>().FirstOrDefault();

            return attribute?.Order;
        }
    }

}
