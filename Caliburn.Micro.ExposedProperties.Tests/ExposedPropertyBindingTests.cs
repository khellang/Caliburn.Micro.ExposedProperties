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
            var button = GetNamedElement<Button>("FirstName");
            var textBlock = GetNamedElement<TextBlock>("LastName");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button, textBlock }, typeof(TestViewModel));

            Assert.That(unhandled.Any(), Is.False);
        }

        [Test]
        public void ElementsWithBlankOrUnderscoreNameAreSkipped()
        {
            var button = GetNamedElement<Button>("_");
            var textBlock = GetNamedElement<TextBlock>("");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button, textBlock }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled, Has.Count.EqualTo(2));
        }

        [Test]
        public void ElementsWithNoMatchingPropertyAreSkipped()
        {
            var button = GetNamedElement<Button>("Foo");
            var textBlock = GetNamedElement<TextBlock>("Bar");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button, textBlock }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled, Has.Count.EqualTo(2));
        }

        [Test]
        public void ElementsWithExistingBindingAreSkipped()
        {
            var binding = new Binding("FirstName");
            var button = GetNamedElement<Button>("FirstName", b => b.SetBinding(ContentControl.ContentProperty, binding));

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled, Has.Count.EqualTo(1));
        }

        [Test]
        public void MultipartExistingPathsAreBound()
        {
            var button = GetNamedElement<Button>("Child_Address");
            var textBlock = GetNamedElement<TextBlock>("Child_DeepChild_DeepString");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button, textBlock }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled.Any(), Is.False);
        }

        [Test]
        public void MultipartNonExistingPathsAreSkipped()
        {
            var button = GetNamedElement<Button>("Child_Foo");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled, Has.Count.EqualTo(1));
        }

        [Test]
        public void SimpleExposedPropertiesAreBound()
        {
            var button = GetNamedElement<Button>("Address");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled.Any(), Is.False);
        }

        [Test]
        public void AliasedExposedPropertiesAreBound()
        {
            var button = GetNamedElement<Button>("Testing");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled.Any(), Is.False);
        }

        [Test]
        public void MultipartExposedPropertiesAreBound()
        {
            var button = GetNamedElement<Button>("Child_Testing");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled.Any(), Is.False);
        }

        [Test]
        public void MultipartExposedPropertiesAreBound2()
        {
            var button = GetNamedElement<Button>("Testing_Length");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled.Any(), Is.False);
        }

        [Test]
        public void MultipartNonExistingExposedPropertiesAreSkipped()
        {
            var button = GetNamedElement<Button>("ZipCode");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled, Has.Count.EqualTo(1));
        }

        [Test]
        public void PropertiesWithExposedNonExisingPropertyNamesAreSkipped()
        {
            var button = GetNamedElement<Button>("Child_Foo");
            var textBlock = GetNamedElement<TextBlock>("Child_Testing_Foo");

            var unhandled = ExposedPropertyBinder.BindProperties(new FrameworkElement[] { button, textBlock }, typeof(TestViewModel)).ToList();

            Assert.That(unhandled, Has.Count.EqualTo(2));
        }

        private static TElement GetNamedElement<TElement>(string name, Action<TElement> initializer = null) 
            where TElement : FrameworkElement, new()
        {
            var element = new TElement { Name = name };

            if (initializer != null)
                initializer.Invoke(element);

            return element;
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