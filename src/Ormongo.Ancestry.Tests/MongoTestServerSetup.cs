using NUnit.Framework;
using Ormongo.TestHelper;

namespace Ormongo.Ancestry.Tests
{
	[SetUpFixture]
	public class MongoTestServerSetup
	{
		private MongoTestServer _testServer;

		[SetUp]
		public void SetUp()
		{
			_testServer = new MongoTestServer(@"..\..\..\..\tools\mongodb\binaries\mongod", "OrmongoAncestryTests");
			_testServer.Start();
		}

		[TearDown]
		public void TearDown()
		{
			_testServer.Stop();
		}
	}
}