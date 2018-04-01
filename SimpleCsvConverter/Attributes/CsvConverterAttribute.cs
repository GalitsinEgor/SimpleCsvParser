using System;

namespace SimpleCsvConverter.Attributes
{
	public class CsvConverterAttribute : Attribute
	{
		private bool ignore;
		private string name;

        public bool Ignore
        {
            get
            {
                return ignore;
            }

            set
            {
                ignore = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
    }
}
