namespace HelloWorld.Tests
{
    using AutoFixture;
    using AutoFixture.Xunit3;
    using FluentAssertions;

    public class UnitTest1
    {
        [Fact]
        public void TestWithFixtureCreate()
        {
            // Arrange
            var fixture = new Fixture();
            int a = fixture.Create<int>();
            int b = fixture.Create<int>();

            // Act
            int result = a + b;

            // Assert
            result.Should().Be(a + b);
        }

        [Theory]
        [AutoData]
        public void TestWithAutoFixture(int a, int b)
        {
            // Arrange & Act (O AutoFixture gera 'a' e 'b' com valores aleatórios válidos)
            int result = a + b;

            // Assert
            result.Should().Be(a + b);
        }

        [Theory]
        [AutoData]
        public void TestFailWithAutoFixture(int a, int b)
        {
            // Arrange & Act (O AutoFixture gera 'a' e 'b' com valores aleatórios válidos)
            int result = a + b;

            // Assert
            result.Should().Be(a - b);
        }
    }
}
