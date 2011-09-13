﻿namespace PiotrTrzpil.VisualMutator_VSPackage.Views.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;


    [ValueConversion(typeof(Freezable), typeof(Freezable))]
    internal class CloningConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Freezable)value).Clone();
          
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotSupportedException();

        }



    }
}