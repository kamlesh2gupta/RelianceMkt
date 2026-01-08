//using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//public static class ExcelReader
//{
//    public static List<T> ReadExcel<T>(Stream fileStream) where T : class, new()
//    {
//        using (var package = new ExcelPackage(fileStream))
//        {
//            var worksheet = package.Workbook.Worksheets[0];
//            var rows = worksheet.Dimension.Rows;
//            var columns = worksheet.Dimension.Columns;
//            var properties = typeof(T).GetProperties();

//            var result = new List<T>();

//            for (int i = 2; i <= rows; i++)
//            {
//                var row = worksheet.Cells[i, 1, i, columns].Select(c => c.Value == null ? "" : c.Value.ToString()).ToList();
//                var item = new T();
//                for (int j = 0; j < properties.Length; j++)
//                {
//                    var property = properties[j];
//                    var value = row[j];
//                    if (!string.IsNullOrWhiteSpace(value))
//                    {
//                        var propertyType = property.PropertyType;
//                        var converter = System.ComponentModel.TypeDescriptor.GetConverter(propertyType);
//                        if (converter.IsValid(value))
//                        {
//                            property.SetValue(item, converter.ConvertFromString(value));
//                        }
//                    }
//                }
//                result.Add(item);
//            }
//            return result;
//        }
//    }
//}
