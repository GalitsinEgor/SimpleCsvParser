# SimpleCsvParser
A simple parser for data formatted in csv. It's able to parse simple value types and classes via System.ComponentModel.TypeDescriptor.

To parse class values from a string, you need to use System.ComponentModel.TypeDescriptor. It doesn't work with user classes initially. There is an example of how you can make it parse strings into classes:

[TypeConverter(typeof(CustomClassTypeConverter))]
public class CustomClass{...}

public class CustomClass : TypeConverter
{
  // object value <- the string value you need to converto into a class object
  public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
  {
      if (value is string)
			{
        //parsing logic 

				return customClass;
			}
			else
			{
				return null;
			}
  }
}
