Caliburn.Micro.PropertyExpose
=============================

A small Caliburn.Micro extension to allow exposing Model properties through a ViewModel with a simple attribute.

Usage
----------

Hook up the property binding to Caliburn.Micro by setting the `ViewModelBinder.BindProperties`:

    ViewModelBinder.BindProperties = ExposedPropertyBinder.BindProperties;
	
Decorate your ViewModel properties with the `ExposeAttribute`:

    public class MyViewModel : PropertyChangedBase
    {
        private Person _person;

        [Expose("FirstName")]
        [Expose("LastName")]
        [Expose("ZipCode")]
        public Person Person
        {
            get { return _person; }
            set
            {
                _person = value;
                NotifyOfPropertyChange(() => Person);
            }
        }
    }

    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Expose("ZipCode", "zip_code")]
        public Address Address { get; set; }
    }

    public class Address
    {
        public string zip_code { get; set; }
    }
	
And bind to the new properties:

    <TextBlock x:Name="FirstName" />
    <TextBlock x:Name="LastName" />
    <TextBlock x:Name="ZipCode" />                //
    <TextBlock x:Name="Person_ZipCode" />    // ZipCode and Person_ZipCode are the same ;)