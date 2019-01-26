using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RimWorldLauncher
{
    public static class DependencyObjectExtensions
    {
        public static DependencyObject GetParent(this DependencyObject current)
        {
            return VisualTreeHelper.GetParent(current);
        }

        public static T FindAncestor<T>(this DependencyObject current)
            where T : DependencyObject
        {
            if (current is T)
            {
                return current as T;
            }
            var parent = current.GetParent();
            if (parent == null)
            {
                return null;
            }
            return parent.FindAncestor<T>();
        }
    }
}
