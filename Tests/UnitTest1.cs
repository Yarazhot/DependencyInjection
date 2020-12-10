using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DILib;
using Moq;
using Single = DILib.Single;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var config = new DiConfig();
            config.AddFabricGenerator<Dog>();

            var provider = new DiProvider(config);
            var pet = provider.Inject<Pet>();
            Assert.AreEqual(typeof(Dog), pet.GetType());
        }

        [TestMethod]
        public void TestMethod2()
        {
            var config = new DiConfig();
            config.AddFabricGenerator<Cat>();
            config.AddFabricGenerator<Dog>();

            var provider = new DiProvider(config);
            var pets = provider.Inject<IEnumerable<Pet>>();

            Assert.AreEqual(2, pets.Count());
        }

        [TestMethod]
        public void TestMethod3()
        {
            var config = new DiConfig();
            config.AddFabricGenerator<Cat>();
            config.AddGenerator<Dog>(new Single(() => new Dog()));
            config.AddSingleGenerator<Man>();

            var provider = new DiProvider(config);
            var man = provider.Inject<Man>();

            Assert.AreEqual(2, man.Pets.Count());
        }

        [TestMethod]
        public void TestMethod4()
        {
            var config = new DiConfig();
            config.AddGenerator<Color>(new Single(() => Color.WHITE));
            config.AddSingleGenerator<Fence>();

            var provider = new DiProvider(config);
            var fence = provider.Inject<Fence>();
            Assert.AreEqual(Color.WHITE, fence.Color);

            fence.Color = Color.GREEN;

            var secondFenceInstance = provider.Inject<Fence>();
            Assert.AreEqual(Color.GREEN, secondFenceInstance.Color);
        }

        [TestMethod]
        public void TestMethod5()
        {
            var config = new DiConfig();
            config.AddGenerator<Color>(new Single(() => Color.WHITE));
            config.AddFabricGenerator<Fence>();

            var provider = new DiProvider(config);
            var fence = provider.Inject<Fence>();
            Assert.AreEqual(Color.WHITE, fence.Color);

            fence.Color = Color.GREEN;

            var secondFenceInstance = provider.Inject<Fence>();
            Assert.AreEqual(Color.WHITE, secondFenceInstance.Color);
        }

        [TestMethod]
        public void TestMethod6()
        {
            var mock = new Mock<IDiConfig>();
            mock.Setup(m => m.Get(It.IsAny<Type>()))
                .Returns<object>(type => Activator.CreateInstance(type as Type));
            var provider = new DiProvider(mock.Object);
            var dog = provider.Inject<Dog>();

            Assert.IsNotNull(dog);
        }

        [TestMethod]
        public void TestMethod7()
        {
            var mock = new Mock<IGenerator>();
            mock.Setup(m => m.Generate())
                .Returns(new Fence(Color.GREEN));

            var config = new DiConfig();
            config.AddGenerator<Barrier>(mock.Object);

            var provider = new DiProvider(config);
            var barrier = provider.Inject<Barrier>();

            Assert.AreEqual(typeof(Fence), barrier.GetType());
        }

        [TestMethod]
        public void TestMethod8()
        {
            var mock = new Mock<IDiConfig>();
            mock.Setup(m => m.Get(It.IsAny<Type>()))
                .Throws(new TypeNotDefinedException());

            var provider = new DiProvider(mock.Object);

            try
            {
                var barrier = provider.Inject<Barrier>();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(TypeNotDefinedException));
            }
        }

        class Man
        {
            public Man(IEnumerable<Pet> pets)
            {
                Pets = pets;
            }

            public IEnumerable<Pet> Pets { get; }
        }

        class Pet
        {
        }

        class Dog : Pet
        {
        }

        class Cat : Pet
        {
        }

        class Barrier
        {
        }

        class Fence : Barrier
        {
            public Color Color { get; set; }

            public Fence(Color Color)
            {
                this.Color = Color;
            }
        }

        enum Color
        {
            GREEN,
            WHITE
        }
    }
}