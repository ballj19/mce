using System;
using NUnit.Framework;
using Adventure;


namespace Adventure.UnitTests
{
    [TestFixture]
    public class CharacterTests
    {
        private Character character;

        [SetUp]
        public void SetUp()
        {
            character = new Warrior("Jake");
        }

        [Test]
        [TestCase(50,50)]
        [TestCase(20,80)]
        public void Hit_DamageLessThanHealth_HealthLowered(int damage, int health_remaining)
        {
            character.Hit(damage);
            
            Assert.That(character.health, Is.EqualTo(health_remaining));
        }

        [Test]
        [TestCase(20, 100)]
        [TestCase(50, 100)]
        public void Heal_HealedToGreaterThanMax_HealthIsMaxHP(int heal, int health)
        {
            character.health = 90;

            character.Heal(heal);

            Assert.That(character.health, Is.EqualTo(health));
        }

        [Test]
        [TestCase(20, 70)]
        [TestCase(5, 55)]
        public void Heal_HealedToLessThanMax_HealthIsMaxHP(int heal, int health)
        {
            character.health = 50;

            character.Heal(heal);

            Assert.That(character.health, Is.EqualTo(health));
        }

        [Test]
        public void AddTitle_WhenCalled_TitleAddedToEndOfName()
        {
            string original_name = character.name;
            character.AddTitle();

            Assert.That(character.name, Does.StartWith(original_name));
            Assert.That(character.name, Does.Not.EndsWith(original_name));
        }
    }
}
