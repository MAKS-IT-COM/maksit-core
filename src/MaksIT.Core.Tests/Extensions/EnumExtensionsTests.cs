using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MaksIT.Core.Extensions;
using Xunit;

namespace MaksIT.Core.Tests.Extensions;

public class EnumExtensionsTests
{
    private enum TestEnum
    {
        [Display(Name = "First Value")]
        First,
        Second
    }

    [Fact]
    public void GetDisplayName_ReturnsDisplayName_WhenDisplayAttributeIsPresent()
    {
        // Arrange
        var value = TestEnum.First;

        // Act
        var displayName = value.GetDisplayName();

        // Assert
        Assert.Equal("First Value", displayName);
    }

    [Fact]
    public void GetDisplayName_ReturnsEnumName_WhenDisplayAttributeIsAbsent()
    {
        // Arrange
        var value = TestEnum.Second;

        // Act
        var displayName = value.GetDisplayName();

        // Assert
        Assert.Equal("Second", displayName);
    }
}
