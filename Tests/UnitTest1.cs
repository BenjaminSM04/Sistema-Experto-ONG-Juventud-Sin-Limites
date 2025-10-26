namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Tests;

/// <summary>
/// Tests de ejemplo - Estos solo se ejecutan cuando corres los tests explícitamente
/// No se ejecutan al iniciar la aplicación
/// </summary>
public class UnitTest1
{
    [Fact]
    public void Test_Example_Always_Passes()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Test_Math_Addition()
    {
        // Arrange
        int a = 2;
        int b = 3;
        int expected = 5;

        // Act
        int actual = a + b;

        // Assert
        Assert.Equal(expected, actual);
    }
}
