using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using NUnit.Framework;

namespace Caliburn.Micro.ExposedProperties.Tests
{
    [TestFixture]
    [RequiresSTA]
    public class ExposedPropertyBindingTests
    {
        [Test]
        public void SimplePropertiesAreBound()
        {
            TestElements<TestViewModel>(0, new FrameworkElement[] { Element<Button>("FirstName"), Element<TextBlock>("LastName") });
        }

        [Test]
        public void ElementsWithBlankOrUnderscoreNameAreSkipped()
        {
            TestElements<TestViewModel>(2, new FrameworkElement[] { Element<Button>("_"), Element<TextBlock>(string.Empty) });
        }

        [Test]
        public void ElementsWithNoMatchingPropertyAreSkipped()
        {
            TestElements<TestViewModel>(2, new FrameworkElement[] { Element<Button>("Foo"), Element<TextBlock>("Bar") });
        }

        [Test]
        public void ElementsWithExistingBindingAreSkipped()
        {
            TestElements<TestViewModel>(1, Element<Button>("FirstName", e => e.SetBinding(ContentControl.ContentProperty, new Binding("FirstName"))));
        }

        [Test]
        public void MultipartExistingPathsAreBound()
        {
            TestElements<TestViewModel>(0, new FrameworkElement[] { Element<Button>("Child_Address"), Element<TextBlock>("Child_DeepChild_DeepString") });
        }

        [Test]
        public void MultipartNonExistingPathsAreSkipped()
        {
            TestElements<TestViewModel>(1, Element<Button>("Child_Foo"));
        }

        [Test]
        public void SimpleExposedPropertiesAreBound()
        {
            TestElements<TestViewModel>(0, Element<Button>("Address"));
        }

        [Test]
        public void AliasedExposedPropertiesAreBound()
        {
            TestElements<TestViewModel>(0, Element<Button>("Testing"));
        }

        [TestCase("Child_Testing")]
        [TestCase("Testing_Length")]
        public void MultipartExposedPropertiesAreBound(string elementName)
        {
            TestElements<TestViewModel>(0, Element<Button>(elementName));
        }

        [Test]
        public void MultipartNonExistingExposedPropertiesAreSkipped()
        {
            TestElements<TestViewModel>(1, Element<Button>("ZipCode"));
        }

        [Test]
        public void PropertiesWithExposedNonExisingPropertyNamesAreSkipped()
        {
            TestElements<TestViewModel>(2, new FrameworkElement[] { Element<Button>("Child_Foo"), Element<TextBlock>("Child_Testing_Foo") });
        }

        private static TElement Element<TElement>(string name, Action<TElement> initializer = null)
            where TElement : FrameworkElement, new()
        {
            var element = new TElement { Name = name };
            if (initializer != null) initializer(element);

            return element;
        }

        private static void TestElements<TViewModel>(int expectedCount, params FrameworkElement[] elements)
        {
            var unmatchedElements = ExposedPropertyBinder.BindProperties(elements, typeof(TViewModel)).ToList();
            Assert.That(unmatchedElements, Has.Count.EqualTo(expectedCount));
        }
    }

    public class TestViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Expose("Address")]
        [Expose("Testing")]
        [Expose("ZipCode", "Zip-Code")]
        public ChildTestViewModel Child { get; set; }
    }

    public class ChildTestViewModel
    {
        public string Address { get; set; }

        public string ZipCode { get; set; }

        [Expose("Testing", "DeepString")]
        public DeepChildViewModel DeepChild { get; set; }
    }

    public class DeepChildViewModel
    {
        public string DeepString { get; set; }
    }
}