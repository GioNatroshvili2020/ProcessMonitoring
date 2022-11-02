namespace ProcessKillerTests
{
    public class Tests
    {

        NLog.Logger _logger;
        [SetUp]
        public void Setup()
        {
            _logger = NLog.LogManager.GetLogger("testLogger");//In Production application it will be created using dependency injection

        }

        [Test]
        public void Should_Throw_Exception_When_EmptyProcessName()
        {
            var ex= Assert.Throws<ArgumentException>(() => new ProcessKiller("",1,2, _logger));
            Assert.That(ex.Message, Is.EqualTo("ProcessName is required"));
        }

        [Test]
        public void Should_Throw_Exception_When_NullProcessName()
        {
            var ex = Assert.Throws<ArgumentException>(() => new ProcessKiller(null, 1, 2, _logger));
            Assert.That(ex.Message, Is.EqualTo("ProcessName is required"));
        }


        [Test]
        public void Should_Throw_Exception_When_ZeroLifetime()
        {
            var ex = Assert.Throws<ArgumentException>(() => new ProcessKiller("test", 0, 2, _logger));
            Assert.That(ex.Message, Is.EqualTo("lifeTime cannot be zero"));
        }

        [Test]
        public void Should_Throw_Exception_When_ZeroFrequency()
        {
            var ex = Assert.Throws<ArgumentException>(() => new ProcessKiller("test", 1, 0, _logger));
            Assert.That(ex.Message, Is.EqualTo("frequency cannot be zero"));
        }
    }
}