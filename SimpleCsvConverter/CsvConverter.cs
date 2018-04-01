using SimpleCsvConverter.Attributes;
using SimpleCsvConverter.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace SimpleCsvConverter
{
	public class CsvConverter
	{
		private string[] headingRow;
		private Dictionary<string, string> colNamesDictionary = new Dictionary<string, string>();

		private char rowSeparator;
		private char colSeparator;
		private string nullValue;

		public CsvConverter(char rowSeparator, char colSeparator, string nullValue)
		{
			this.rowSeparator = rowSeparator;
			this.colSeparator = colSeparator;
			this.nullValue = nullValue;
		}

		public IList<T> ConvertStringTo<T>(string toConvert) where T : class
		{
			try
			{
				string[] rows = toConvert.Split(rowSeparator);
				if (rows.Length == 0)
				{
					throw new CsvConverterException($"Error: Splitted by {colSeparator} string toConvert doesn't contain values.");
				}

				FormHeadingRow(rows[0]);
				FormNameDictionary(typeof(T));
				string[,] preparedData = SplitRows(rows);
				IList<T> response = ConvertParsedStringToType<T>(preparedData);
				return response;
			}
			catch (Exception e)
			{
				throw new CsvConverterException("Method: ConvertStringTo.", e);
			}
		}
		private void FormHeadingRow(string firstRow)
		{
			try
			{
				string[] tmpHeadingRow = firstRow.Split(colSeparator);
				if (tmpHeadingRow.Length == 0)
				{
					throw new CsvConverterException($"Error: Splitted by {colSeparator} first row doesn't contain values.");
				}

				headingRow = tmpHeadingRow;
			}
			catch(Exception e)
			{
				throw new CsvConverterException("Method: FormHeadingRow.", e);
			}
		}
		private void FormNameDictionary(Type type)
		{
			try
			{
				colNamesDictionary = new Dictionary<string, string>();

				PropertyInfo[] propertiesInfo = type.GetProperties();
				foreach (PropertyInfo info in propertiesInfo)
				{
					CsvConverterAttribute attribute = info.GetCustomAttribute(typeof(CsvConverterAttribute)) as CsvConverterAttribute;
					if (attribute != null)
					{
						if (attribute.Ignore)
							continue;

						if (!String.IsNullOrEmpty(attribute.Name))
						{
							colNamesDictionary.Add(attribute.Name, info.Name);
							continue;
						}

					}
					colNamesDictionary.Add(info.Name, info.Name);
				}

				if (colNamesDictionary.Count == 0)
				{
					throw new CsvConverterException($"Error: HeadingRow doesn't contain similar values to the properties of the class {type.Name}.");
				}
			}
			catch (Exception e)
			{
				throw new CsvConverterException("Method: FormNameDictionary.", e);
			}
		}
		private string[,] SplitRows(string[] rows)
		{
			try
			{
				string[,] preparedData = new string[rows.Length - 1, headingRow.Length];

				for (int i = 1; i < rows.Length; i++)
				{
					string[] tmpValues = rows[i].Split(colSeparator);
					if (tmpValues.Length < headingRow.Length)
					{
						throw new CsvConverterException($"Error: Splitted by {colSeparator} row {i} contains less values than expected.");
					}

					for (int i1 = 0; i1 < headingRow.Length; i1++)
					{
						if(tmpValues[i1] == nullValue)
						{
							preparedData[i - 1, i1] = null;
						}
						else
						{
							preparedData[i - 1, i1] = tmpValues[i1];
						}
					}
				}

				return preparedData;
			}
			catch (Exception e)
			{
				throw new CsvConverterException("Method: SplitString.", e);
			}
		}
		private List<T> ConvertParsedStringToType<T>(string[,] parsedString) where T : class
		{
			try
			{
				List<T> outputObjects = new List<T>();
				for (int i = 0; i < parsedString.GetLength(0); i++)
				{
					Type type = typeof(T);
					object outputObject = Activator.CreateInstance(type);

					for (int i1 = 0; i1 < headingRow.Length; i1++)
					{
                        string fieldValue = parsedString[i, i1];
                        string colName = headingRow[i1];
                        FillOutputObjectsField(outputObject, type, fieldValue, colName);
                    }

					outputObjects.Add(outputObject as T);
				}

				return outputObjects;
			}
			catch(Exception e)
			{
				throw new CsvConverterException("Method: ConvertParsedStringToType.", e);
			}
		}
        private void FillOutputObjectsField(object outputObject, Type objectType, string fieldValue, string colName)
        {
            try
            {
                if (colNamesDictionary.ContainsKey(colName))
                {
                    string propertyName = colNamesDictionary[colName];
                    PropertyInfo info = objectType.GetProperty(propertyName);

                    if (fieldValue != null)
                    {
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(info.PropertyType);
                        object propValue = typeConverter.ConvertFromString(fieldValue);
                        info.SetValue(outputObject, propValue, null);
                    }
                    else
                    {
                        object defaultValue = GetDefaultValueForType(info.PropertyType);
                        info.SetValue(outputObject, defaultValue, null);
                    }
                }
            }
            catch (Exception e)
            {
                throw new CsvConverterException("Method: FillOutputObjectsField.", e);
            }
        }
		private object GetDefaultValueForType(Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			return null;
		}
	}
}
