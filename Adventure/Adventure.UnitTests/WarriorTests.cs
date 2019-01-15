using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Adventure.UnitTests
{
    [TestFixture]
    class WarriorTests
    {
        Warrior warrior;

        [SetUp]
        public void SetUp()
        {
            warrior = new Warrior("airsnipe");
        }

        [Test]
        public void ValidateWeapon_WithValidWeapon_ReturnsTrue()
        {
            var result = warrior.ValidateWeapon(new Weapon { type = "Axe" });

            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateWeapon_WithInvalidWeapon_ReturnsFalse()
        {
            var result = warrior.ValidateWeapon(new Weapon { type = "Staff" });

            Assert.That(result, Is.False);
        }
    }
}
