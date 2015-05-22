using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordChange.View.Converters
{
    public class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter()
            : base(Visibility.Visible, Visibility.Collapsed) {}
    }
}
